using UnityEngine;

public class SubPixelRenderer : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float pixelsPerUnit = 16f;

    private Vector2 accumulatedMovement;

    private Vector3 previousTargetPosition;

    private void Start()
    {
        previousTargetPosition = target.position;

        transform.position = target.position;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = target.position - previousTargetPosition;

        accumulatedMovement += (Vector2)deltaMovement;

        float pixelSize = 1f / pixelsPerUnit;

        while (Mathf.Abs(accumulatedMovement.x) >= pixelSize)
        {
            float step = Mathf.Sign(accumulatedMovement.x) * pixelSize;

            transform.position += new Vector3(step, 0f, 0f);

            accumulatedMovement.x -= step;
        }

        while (Mathf.Abs(accumulatedMovement.y) >= pixelSize)
        {
            float step = Mathf.Sign(accumulatedMovement.y) * pixelSize;

            transform.position += new Vector3(0f, step, 0f);

            accumulatedMovement.y -= step;
        }

        previousTargetPosition = target.position;
    }
}