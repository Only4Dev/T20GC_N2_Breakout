using UnityEngine;

public class BallVisual : MonoBehaviour
{
    [SerializeField] private Ball ball;
    [SerializeField] private float followSpeed = 20f;

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            ball.Position,
            followSpeed * Time.deltaTime);
    }
}