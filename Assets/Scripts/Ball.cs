using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float ballSpeed = 10f;
    [SerializeField] private float bounceFactor = 1.5f;

    [Header("Direction Clamp")]
    [SerializeField] private float minHorizontalDirection = 0.20f;
    [SerializeField] private float minVerticalDirection = 0.20f;

    [Header("Collision")]
    [SerializeField] private float collisionEpsilon = 0.005f;
    [SerializeField] private LayerMask collisionMask;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;

    [SerializeField]
    private Vector2 direction = new Vector2(0.7f, 0.7f);

    private float BallRadius => circleCollider.radius * transform.lossyScale.x;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        direction.Normalize();
    }

    private void FixedUpdate()
    {
        BallMove();
    }

    private void BallMove()
    {
        float moveDistance = ballSpeed * Time.fixedDeltaTime;

        Vector2 currentPosition = rb.position;

        RaycastHit2D hit = Physics2D.CircleCast(
            currentPosition,
            BallRadius,
            direction,
            moveDistance,
            collisionMask);

        if (hit.collider == null)
        {
            currentPosition += direction * moveDistance;
        }
        else
        {
            float travelDistance = Mathf.Max(hit.distance - collisionEpsilon, 0f);

            currentPosition += direction * travelDistance;

            currentPosition = HandleHit(hit, currentPosition);
        }

        rb.MovePosition(currentPosition);
    }

    private Vector2 HandleHit(RaycastHit2D hit, Vector2 currentPosition)
    {
        GameObject hitObject = hit.collider.gameObject;

        if (hitObject.CompareTag("Player"))
            return HandlePlayerHit(hitObject, hit, currentPosition);

        if (hitObject.CompareTag("Brick"))
            return HandleBrickHit(hitObject, hit, currentPosition);

        if (hitObject.CompareTag("Wall"))
            return HandleWallHit(hit, currentPosition);

        return currentPosition;
    }

    private Vector2 HandleWallHit(RaycastHit2D hit, Vector2 currentPosition)
    {
        direction = Vector2.Reflect(direction, hit.normal).normalized;

        ClampDirection();

        return currentPosition + (Vector2)hit.normal * collisionEpsilon;
    }

    private Vector2 HandleBrickHit(GameObject brick, RaycastHit2D hit, Vector2 currentPosition)
    {
        direction = Vector2.Reflect(direction, hit.normal).normalized;

        ClampDirection();

        brick.GetComponent<Brick>().BrickBreak();

        return currentPosition + (Vector2)hit.normal * collisionEpsilon;
    }

    private Vector2 HandlePlayerHit(GameObject player, RaycastHit2D hit, Vector2 currentPosition)
    {
        BoxCollider2D paddleCollider = player.GetComponent<BoxCollider2D>();

        float paddleWidth = paddleCollider.bounds.size.x;

        // -1 = extremo izquierdo, +1 = extremo derecho
        float impact =
            (currentPosition.x - player.transform.position.x) /
            (paddleWidth * 0.5f);

        impact *= bounceFactor;
        impact = Mathf.Clamp(impact, -1f, 1f);

        // Partimos de la normal real de la cápsula.
        Vector2 bounce = hit.normal;

        // El impacto del jugador modifica únicamente la componente X.
        bounce.x += impact;

        direction = bounce.normalized;

        ClampDirection();

        // Separar la pelota fuera del paddle usando la normal.
        return currentPosition + hit.normal * collisionEpsilon;
    }
    private void ClampDirection()
    {
        if (Mathf.Abs(direction.x) < minHorizontalDirection)
        {
            direction.x = Mathf.Sign(direction.x) * minHorizontalDirection;
        }

        if (Mathf.Abs(direction.y) < minVerticalDirection)
        {
            direction.y = Mathf.Sign(direction.y) * minVerticalDirection;
        }

        direction.Normalize();
    }
}