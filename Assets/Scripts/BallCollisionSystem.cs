using UnityEngine;

public class BallCollisionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Player paddle;
    [SerializeField] AudioManager audioManager;

    [Header("Stage Bounds")]
    [SerializeField] private float leftLimit;
    [SerializeField] private float rightLimit;
    [SerializeField] private float topLimit;
    [SerializeField] private float bottomLimit = -6f; // below the paddle's row

    [Header("Paddle Bounce")]
    [SerializeField, Range(0.05f, 0.5f)] private float minUpwardRatio = 0.2f;
    [SerializeField] private float paddleVelocityInfluence = 0.1f;


    private void Awake()
    {
        if (audioManager == null)
            audioManager = FindAnyObjectByType<AudioManager>();
    }

    private void Update()
    {
        if (!ball.IsLaunched)
            return;

        CheckWallCollision();
        CheckPaddleCollision();
        CheckBrickCollision();
        CheckMiss();
    }


    private void CheckWallCollision()
    {
        Vector2 position = ball.Position;

        if (position.x > rightLimit)
        {
            position.x = rightLimit;
            ball.SetPosition(position);
            ball.ReflectX();
            audioManager.PlayBallHit();
        }
        else if (position.x < leftLimit)
        {
            position.x = leftLimit;
            ball.SetPosition(position);
            ball.ReflectX();
            audioManager.PlayBallHit();
        }

        position = ball.Position;
        if (position.y > topLimit)
        {
            position.y = topLimit;
            ball.SetPosition(position);
            ball.ReflectY();
            audioManager.PlayBallHit();
            ball.ReportCeilingHit();
        }
    }

    private void CheckPaddleCollision()
    {
        float paddleLeft = paddle.LeftEdge;
        float paddleRight = paddle.RightEdge;
        float paddleTop = paddle.Top;
        float paddleBottom = paddle.Position.y - paddle.HalfHeight;

        Vector2 position = ball.Position;
        float radius = ball.Radius;

        float ballLeft = position.x - radius;
        float ballRight = position.x + radius;
        float ballBottom = position.y - radius;
        float ballTop = position.y + radius;

        bool overlapping = ballRight > paddleLeft && ballLeft < paddleRight
                         && ballTop > paddleBottom && ballBottom < paddleTop;

        if (!overlapping)
            return;

        float overlapX = Mathf.Min(ballRight, paddleRight) - Mathf.Max(ballLeft, paddleLeft);
        float overlapY = Mathf.Min(ballTop, paddleTop) - Mathf.Max(ballBottom, paddleBottom);
        bool hitTop = overlapY <= overlapX;

        if (hitTop)
            position.y = paddleTop + radius;
        else
            position.x = position.x < paddle.Position.x ? paddleLeft - radius : paddleRight + radius;

        ball.SetPosition(position);

        float paddleHalfWidth = (paddleRight - paddleLeft) * 0.5f;
        float offset = position.x - paddle.Position.x;
        float hitRatio = Mathf.Clamp(offset / paddleHalfWidth, -1f, 1f);

        float velocityContribution = Mathf.Clamp(paddle.Velocity * paddleVelocityInfluence, -0.3f, 0.3f);
        float sideways = Mathf.Clamp(hitRatio + velocityContribution, -1f, 1f);

        ball.BounceAngled(sideways, minUpwardRatio);
        audioManager.PlayBallHit();
        ball.ReportPaddleHit();
    }

    private void CheckBrickCollision()
    {
        Vector2 position = ball.Position;
        Vector2 previous = ball.PreviousPosition;
        float radius = ball.Radius;

        float ballLeft = position.x - radius;
        float ballRight = position.x + radius;
        float ballBottom = position.y - radius;
        float ballTop = position.y + radius;

        float prevLeft = previous.x - radius;
        float prevRight = previous.x + radius;
        float prevBottom = previous.y - radius;
        float prevTop = previous.y + radius;

        foreach (Brick brick in Brick.Active)
        {
            bool overlapping = ballRight > brick.Left && ballLeft < brick.Right
                             && ballTop > brick.Bottom && ballBottom < brick.Top;

            if (!overlapping)
                continue;

            bool wasOutsideY = prevTop <= brick.Bottom || prevBottom >= brick.Top;
            bool wasOutsideX = prevRight <= brick.Left || prevLeft >= brick.Right;

            bool hitVertical;

            if (wasOutsideY && !wasOutsideX)
            {
                hitVertical = true; // came from above or below
            }
            else if (wasOutsideX && !wasOutsideY)
            {
                hitVertical = false; // came from the side
            }
            else
            {
                // genuine corner case - fall back to the old overlap-size guess
                float overlapXFallback = Mathf.Min(ballRight, brick.Right) - Mathf.Max(ballLeft, brick.Left);
                float overlapYFallback = Mathf.Min(ballTop, brick.Top) - Mathf.Max(ballBottom, brick.Bottom);
                hitVertical = overlapYFallback <= overlapXFallback;
            }

            if (hitVertical)
            {
                ball.ReflectY();
                float overlapY = Mathf.Min(ballTop, brick.Top) - Mathf.Max(ballBottom, brick.Bottom);
                position.y += ball.Direction.y > 0f ? overlapY : -overlapY;
            }
            else
            {
                ball.ReflectX();
                float overlapX = Mathf.Min(ballRight, brick.Right) - Mathf.Max(ballLeft, brick.Left);
                position.x += ball.Direction.x > 0f ? overlapX : -overlapX;
            }

            ball.SetPosition(position);
            brick.TakeDamage();
            break;
        }
    }

    private void CheckMiss()
    {
        if (ball.Position.y < bottomLimit)
            ball.ReportLost();
    }
}