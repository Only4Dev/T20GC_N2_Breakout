using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    Waiting,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private SpriteNumberDisplay livesDisplay;

    [Header("Score")]
    [SerializeField] private SpriteNumberDisplay scoreDisplay;
    [SerializeField] private SpriteNumberDisplay highScoreDisplay;
    private const string HighScoreKey = "HighScore";

    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Player paddle;
    [SerializeField] private AudioManager audioManager;

    [Header("Difficulty")]
    [SerializeField] private float speedIncreasePerBrick = 0.05f;

    [Header("Aiming")]
    [SerializeField] private float aimInputThreshold = 0.1f;
    [SerializeField] private float autoLaunchTime = 4f;

    private float waitingTimer;

    [Header("Combo")]
    [SerializeField] private int comboBrickInterval = 3;
    [SerializeField] private float comboBonusStep = 0.2f;

    [Header("Levels")]
    [SerializeField] private GameObject[] levelPrefabs; // drag prefab assets here, not scene objects
    [SerializeField] private Transform levelParent; // empty GameObject in the scene to hold the current level
    [SerializeField] private float levelTransitionDelay = 2f;

    private GameObject currentLevelInstance;

    private int currentLevelIndex;
    private bool isTransitioning;
    private bool hasShrunkPaddle;

    private const int AimLeft = 0;
    private const int AimCenter = 1;
    private const int AimRight = 2;

    private GameState currentState = GameState.Waiting;
    private int score;
    private int highScore;
    private int lives;
    private int comboCounter;
    private float comboBonus = 1f;

    public GameState CurrentState => currentState;
    public int Score => score;
    public int Lives => lives;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = FindAnyObjectByType<AudioManager>();
    }

    private void OnEnable()
    {
        Brick.OnBrickBroken += HandleBrickBroken;
        Ball.OnBallLost += HandleBallLost;
        Ball.OnPaddleHit += HandlePaddleHit;
        Ball.OnCeilingHit += HandleCeilingHit;
    }

    private void OnDisable()
    {
        Brick.OnBrickBroken -= HandleBrickBroken;
        Ball.OnBallLost -= HandleBallLost;
        Ball.OnPaddleHit -= HandlePaddleHit;
        Ball.OnCeilingHit -= HandleCeilingHit;
    }

    private void HandleCeilingHit()
    {
        if (hasShrunkPaddle)
            return;

        paddle.Shrink();
        hasShrunkPaddle = true;
    }

    private void Start()
    {
        score = 0;
        lives = startingLives;
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        UpdateScoreUI();
        UpdateLivesUI();
        UpdateHighScoreUI();

        SetupLevels();

        Time.timeScale = 1f;
        EnterWaitingState();
    }

    private void SetupLevels()
    {
        currentLevelIndex = 0;
        currentLevelInstance = Instantiate(levelPrefabs[currentLevelIndex], levelParent);
    }

    private void Update()
    {
        if (currentState != GameState.Waiting || isTransitioning)
            return;

        ParkBallOnPaddle();

        if(!isTransitioning)
        waitingTimer += Time.deltaTime;

        if (ActionPressed() || waitingTimer >= autoLaunchTime)
            LaunchBall();
    }

    private void EnterWaitingState()
    {
        currentState = GameState.Waiting;
        waitingTimer = 0f;
        ball.Stop();
        paddle.SetMovementEnabled(true);
        ParkBallOnPaddle();
    }

    private void ParkBallOnPaddle()
    {
        Vector2 parkPosition = new Vector2(paddle.Position.x, paddle.Top + ball.Radius);
        ball.SetPosition(parkPosition);
    }

    private bool ActionPressed()
    {
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
    }

    private void LaunchBall()
    {
        int aim = GetCurrentAim();

        ball.Launch(aim);
        audioManager.PlayBallHit();
        currentState = GameState.Playing;
    }

    private int GetCurrentAim()
    {
        if (paddle.RawInput < -aimInputThreshold)
            return AimLeft;

        if (paddle.RawInput > aimInputThreshold)
            return AimRight;

        return AimCenter;
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    private void HandleBrickBroken(int points)
    {
        if (currentState != GameState.Playing)
            return;

        comboCounter++;

        if (comboCounter % comboBrickInterval == 0)
            comboBonus += comboBonusStep;

        int bonusPoints = Mathf.RoundToInt(points * comboBonus);
        score += bonusPoints;

        ball.IncreaseSpeed(speedIncreasePerBrick);
        UpdateScoreUI();

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }

        if (Brick.Active.Count == 0)
            StartCoroutine(AdvanceLevelRoutine());
    }

    private void HandlePaddleHit()
    {
        ResetCombo();
    }

    private void HandleBallLost()
    {
        if (currentState != GameState.Playing)
            return;

        ResetCombo();

        lives--;
        UpdateLivesUI();

        if (lives <= 0)
            GameOver();
        else
            EnterWaitingState();
    }

    private void ResetCombo()
    {
        comboCounter = 0;
        comboBonus = 1f;
    }

    private System.Collections.IEnumerator AdvanceLevelRoutine()
    {
        isTransitioning = true;
        EnterWaitingState();

        yield return new WaitForSeconds(levelTransitionDelay);

        Destroy(currentLevelInstance);

        currentLevelIndex = (currentLevelIndex + 1) % levelPrefabs.Length; // wrap back to level 0 after the last level
        currentLevelInstance = Instantiate(levelPrefabs[currentLevelIndex], levelParent);

        paddle.ResetWidth(); // completing a level restores normal paddle size
        hasShrunkPaddle = false;

        paddle.SetMovementEnabled(true);
        isTransitioning = false;
    }

    private void GameOver()
    {
        currentState = GameState.GameOver;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateScoreUI() => scoreDisplay.SetValue(score);
    private void UpdateLivesUI() => livesDisplay.SetValue(lives);
    private void UpdateHighScoreUI() => highScoreDisplay.SetValue(highScore);
}