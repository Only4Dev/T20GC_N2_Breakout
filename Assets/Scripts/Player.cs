using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputAction moveAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Stage Bounds")]
    [SerializeField] private float leftWall;
    [SerializeField] private float rightWall;

    [Header("Paddle Shape")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField, Range(0.1f, 1f)] private float shrinkWidthMultiplier = 0.66f;

    private float normalWidth;

    private Vector2 position;
    private float horizontalInput;
    private bool movementEnabled = true;

    // --- Public read-only info other scripts (e.g. Ball, GameManager) need ---
    public Vector2 Position => position;
    public float HalfWidth => spriteRenderer.size.x * 0.5f;
    public float HalfHeight => spriteRenderer.size.y * 0.5f;
    public float LeftEdge => Position.x - HalfWidth;
    public float RightEdge => Position.x + HalfWidth;
    public float Top => position.y + HalfHeight;
    public float Velocity { get; private set; }
    public float RawInput => horizontalInput;

    private void Awake()
    {
        moveAction = inputActions.FindAction("Move");
    }

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    private void Start()
    {
        position = transform.position;
        normalWidth = spriteRenderer.size.x;
    }

    private void Update()
    {
        ReadInput();
        Move();
        Render();
    }

    private void ReadInput()
    {
        horizontalInput = moveAction.ReadValue<float>();
    }

    private void Move()
    {
        float velocity = movementEnabled ? horizontalInput * moveSpeed : 0f;
        float leftMovementLimit = leftWall + HalfWidth;
        float rightMovementLimit = rightWall - HalfWidth;

        position.x += velocity * Time.deltaTime;
        position.x = Mathf.Clamp(position.x, leftMovementLimit, rightMovementLimit);

        Velocity = velocity;
    }

    public void Shrink()
    {
        spriteRenderer.size = new Vector2(normalWidth * shrinkWidthMultiplier, spriteRenderer.size.y);
    }

    public void ResetWidth()
    {
        spriteRenderer.size = new Vector2(normalWidth, spriteRenderer.size.y);
    }

    private void Render()
    {
        transform.position = position;
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
    }
}