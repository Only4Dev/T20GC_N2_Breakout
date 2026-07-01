using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] float ballSpeed = 10f;
    [SerializeField] float bounceFactor = 1.5f;
    [SerializeField] private float minHorizontalDirection = 0.15f;
    [SerializeField] private float minVerticalDirection = 0.15f;

    [SerializeField] Vector2 direction;
    private RaycastHit2D[] hits = new RaycastHit2D[4];

    [SerializeField] Rigidbody2D rigidbody2D;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        direction = direction.normalized;
    }

    private void FixedUpdate()
    {
        BallMove();
    }


    private void BallMove()
    {
        float moveDistance = ballSpeed * Time.fixedDeltaTime;

        int hitCount = rigidbody2D.Cast(direction, hits, moveDistance);

        if (hitCount == 0)
        {
            Vector2 targetPosition = rigidbody2D.position;

            targetPosition += direction * moveDistance;

            rigidbody2D.MovePosition(targetPosition);
        }
        else
        {
            RaycastHit2D hit = hits[0];

            GameObject hitObject = hit.collider.gameObject;

            float travelDistance = hit.distance;

            Vector2 targetPosition = rigidbody2D.position;

            targetPosition += direction * travelDistance;

            rigidbody2D.MovePosition(targetPosition);

            if (hitObject.CompareTag("Wall"))
            {
                direction = Vector2.Reflect(direction, hit.normal).normalized;

                ClampDirection();
            }

            if (hitObject.CompareTag("Brick"))
            {
                direction = Vector2.Reflect(direction, hit.normal).normalized;

                ClampDirection();

                hitObject.GetComponent<Brick>().BrickBreak();
            }

            if (hitObject.CompareTag("Player"))
            {
                BoxCollider2D paddleCollider = hitObject.GetComponent<BoxCollider2D>();

                float paddleWidth = paddleCollider.bounds.size.x;

                float impact = (rigidbody2D.position.x - hitObject.transform.position.x) / (paddleWidth / 2);

                impact *= bounceFactor;

                impact = Mathf.Clamp(impact, -1f, 1f);

                direction = new Vector2(impact, 1f).normalized;

                ClampDirection();
            }
        }
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
