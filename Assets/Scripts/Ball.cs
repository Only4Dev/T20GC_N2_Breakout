using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] float ballSpeed = 10f;
    [SerializeField] float bounceFactor = 1.5f;
    [SerializeField] Vector2 direction;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            direction = Vector2.Reflect(direction, collision.GetContact(0).normal).normalized;
        }

        if (collision.gameObject.CompareTag("Brick"))
        {
            direction = Vector2.Reflect(direction, collision.GetContact(0).normal).normalized;
            collision.gameObject.GetComponent<Brick>().BrickBreak();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            BoxCollider2D paddleCollider = collision.gameObject.GetComponent<BoxCollider2D>();
            float paddleWidth = paddleCollider.bounds.size.x;
            Transform paddle = collision.transform;
            ContactPoint2D contact = collision.GetContact(0);
            float impact;

            impact = (contact.point.x - paddle.position.x) / (paddleWidth / 2);

            impact *= bounceFactor;

            impact = Mathf.Clamp(impact, -1f, 1f);

            direction = new Vector2(impact, 1f).normalized;
        }
    }

    private void BallMove()
    {
        Vector2 targetPosition = rigidbody2D.position;

        targetPosition += direction * ballSpeed * Time.fixedDeltaTime;

        targetPosition = PixelSnap.Snap(targetPosition, 16);

        rigidbody2D.MovePosition(targetPosition);
    }
}
