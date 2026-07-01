using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private Rigidbody2D rigidbody2D;

    [SerializeField] private float leftLimit;
    [SerializeField] private float rightLimit;

    [SerializeField] float direction;
    [SerializeField] float playerSpeed = 10f;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        moveAction = inputActions.FindAction("Move");
    }

    void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

    void Move()
    {

        Vector2 targetPosition = rigidbody2D.position;

        targetPosition.x += direction * playerSpeed * Time.fixedDeltaTime;

        targetPosition.x = Mathf.Clamp(targetPosition.x, leftLimit, rightLimit);

        rigidbody2D.MovePosition(targetPosition);
    }

    private void ReadInput()
    {
        direction = moveAction.ReadValue<float>();
    }
}
