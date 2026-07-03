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

        transform.position = PixelSnap.Snap(target.position, pixelsPerUnit);
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = target.position - previousTargetPosition;
        accumulatedMovement += (Vector2)deltaMovement;

        float pixelSize = 1f / pixelsPerUnit;

        Vector2 snappedOffset = new Vector2(
            Mathf.Floor(Mathf.Abs(accumulatedMovement.x) / pixelSize) * pixelSize * Mathf.Sign(accumulatedMovement.x),
            Mathf.Floor(Mathf.Abs(accumulatedMovement.y) / pixelSize) * pixelSize * Mathf.Sign(accumulatedMovement.y));

        transform.position += (Vector3)snappedOffset;
        accumulatedMovement -= snappedOffset;

        previousTargetPosition = target.position;
    }
}