using UnityEngine;

public class PauseButton : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite playSprite;

    private void OnMouseDown()
    {
        gameManager.TogglePause();
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        bool isPaused = gameManager.CurrentState == GameState.Paused;
        spriteRenderer.sprite = isPaused ? playSprite : pauseSprite;
    }
}