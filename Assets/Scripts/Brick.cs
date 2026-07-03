using System;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] lifeStageSprites; // index 0 = full health, last = about to break
    [SerializeField] private Color brickColor = Color.white;

    [Header("Life")]
    [SerializeField] private int life = 1;
    [SerializeField] private int scoreValue = 10;

    [Header("Collision")]
    [SerializeField] private float collisionInset = 0.05f; // shrinks collision box only, not the visual sprite

    [Header("Effects")]
    [SerializeField] private GameObject debrisPrefab;
    [SerializeField] private AudioManager audioManager;

    public static readonly List<Brick> Active = new();
    public static event Action<int> OnBrickBroken;

    private int maxLife;

    public Vector2 Position => transform.position;
    public float HalfWidth => spriteRenderer.size.x * 0.5f;
    public float HalfHeight => spriteRenderer.size.y * 0.5f;

    public float Left => Position.x - HalfWidth + collisionInset;
    public float Right => Position.x + HalfWidth - collisionInset;
    public float Top => Position.y + HalfHeight - collisionInset;
    public float Bottom => Position.y - HalfHeight + collisionInset;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = FindAnyObjectByType<AudioManager>();

        maxLife = life;
        UpdateSprite();
    }

    private void OnEnable() => Active.Add(this);
    private void OnDisable() => Active.Remove(this);

    public void TakeDamage(int amount = 1)
    {
        life -= amount;

        if (life <= 0)
        {
            BrickBreak();
            return;
        }

        UpdateSprite();
        audioManager.PlayMetalBrickHit();
    }

    private void UpdateSprite()
    {
        if (lifeStageSprites == null || lifeStageSprites.Length == 0)
            return;

        int damageTaken = maxLife - life;
        int spriteIndex = Mathf.Clamp(damageTaken, 0, lifeStageSprites.Length - 1);
        spriteRenderer.sprite = lifeStageSprites[spriteIndex];
    }

    private void BrickBreak()
    {
        OnBrickBroken?.Invoke(scoreValue);
        SpawnDebris();
        audioManager.PlayBrickBreak();
        Destroy(gameObject);
    }

    private void SpawnDebris()
    {
        if (debrisPrefab == null)
            return;

        GameObject debris = Instantiate(debrisPrefab, transform.position, Quaternion.identity);

        if (debris.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            main.startColor = brickColor;
            Destroy(debris, main.duration + main.startLifetime.constantMax);
        }
    }
}