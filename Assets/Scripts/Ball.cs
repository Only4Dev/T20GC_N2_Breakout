using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector2 direction = new Vector2(0.6f, 0.8f);
    [SerializeField] private float speed = 8f;
    private float baseSpeed;

    [Header("Ball Shape")]
    [SerializeField] private float radius = 0.5f;

    [Header("Angle Limits")]
    [SerializeField, Range(0.05f, 0.5f)] private float minAxisRatio = 0.35f;
    [SerializeField, Range(0f, 20f)] private float bounceJitterDegrees = 8f;
    [SerializeField] private float rapidBounceWindow = 0.08f; // seconds

    private static readonly Vector2[] LaunchDirections =
    {
        new Vector2(-0.5f, 1f),
        new Vector2(0f, 1f),
        new Vector2(0.5f, 1f)
    };

    public static event Action OnBallLost;
    public static event Action OnPaddleHit;
    public static event Action OnCeilingHit;

    public void ReportCeilingHit()
    {
        OnCeilingHit?.Invoke();
    }

    private Vector2 position;
    private float lastBounceTime = -999f;

    public Vector2 Position => position;
    public Vector2 PreviousPosition { get; private set; }
    public Vector2 Direction => direction;
    public float Radius => radius;
    public bool IsLaunched { get; private set; }

    private void Start()
    {
        position = transform.position;
        baseSpeed = speed;
    }

    private void Update()
    {
        Move();
    }

    private void LateUpdate()
    {
        Render();
    }

    private void Move()
    {
        if (!IsLaunched)
            return;

        PreviousPosition = position;
        position += direction * speed * Time.deltaTime;
    }

    private void Render()
    {
        transform.position = position;
    }

    public void Launch(int directionIndex)
    {
        int index = Mathf.Clamp(directionIndex, 0, LaunchDirections.Length - 1);
        direction = LaunchDirections[index].normalized;
        IsLaunched = true;
    }

    public void ResetSpeed()
    {
        speed = baseSpeed;
    }

    public void Stop()
    {
        IsLaunched = false;
    }

    // --- Public API used by BallCollisionSystem ---

    public void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }

    public void ReflectX()
    {
        direction.x *= -1;
        ApplyJitter();
        ClampAngle();
    }

    public void ReflectY()
    {
        direction.y *= -1;
        ApplyJitter();
        ClampAngle();
    }

    public void BounceAngled(float sideways, float minUpward)
    {
        direction = new Vector2(sideways, 1f).normalized;

        if (direction.y < minUpward)
            direction.y = minUpward;

        ClampAngle();
    }

    public void IncreaseSpeed(float amount)
    {
        speed += amount;
    }

    public void ReportLost()
    {
        OnBallLost?.Invoke();
    }

    public void ReportPaddleHit()
    {
        OnPaddleHit?.Invoke();
    }

    private void ClampAngle()
    {
        direction.Normalize();

        if (Mathf.Abs(direction.y) < minAxisRatio)
            direction.y = direction.y >= 0f ? minAxisRatio : -minAxisRatio;

        if (Mathf.Abs(direction.x) < minAxisRatio)
            direction.x = direction.x >= 0f ? minAxisRatio : -minAxisRatio;

        direction.Normalize();
    }

    private void ApplyJitter()
    {
        float timeSinceLastBounce = Time.time - lastBounceTime;
        lastBounceTime = Time.time;

        float rapidFactor = Mathf.Clamp01(timeSinceLastBounce / rapidBounceWindow);
        float effectiveJitter = bounceJitterDegrees * rapidFactor;

        float jitter = UnityEngine.Random.Range(-effectiveJitter, effectiveJitter);
        direction = Quaternion.Euler(0f, 0f, jitter) * direction;
    }
}