using UnityEngine;

public static class PixelSnap
{
    /// <summary>
    /// Snaps a float value to the nearest pixel based on the given Pixels Per Unit.
    /// </summary>
    public static float Snap(float value, float pixelsPerUnit)
    {
        return Mathf.Round(value * pixelsPerUnit) / pixelsPerUnit;
    }

    /// <summary>
    /// Snaps a Vector2 to the nearest pixel based on the given Pixels Per Unit.
    /// </summary>
    public static Vector2 Snap(Vector2 position, float pixelsPerUnit)
    {
        return new Vector2(
            Snap(position.x, pixelsPerUnit),
            Snap(position.y, pixelsPerUnit));
    }

    /// <summary>
    /// Snaps a Vector3 to the nearest pixel based on the given Pixels Per Unit.
    /// </summary>
    public static Vector3 Snap(Vector3 position, float pixelsPerUnit)
    {
        return new Vector3(
            Snap(position.x, pixelsPerUnit),
            Snap(position.y, pixelsPerUnit),
            position.z);
    }

    public static bool IsSnapped(float value, float pixelsPerUnit)
    {
        float snapped = Snap(value, pixelsPerUnit);
        return Mathf.Approximately(value, snapped);
    }

    public static bool IsSnapped(Vector2 position, float pixelsPerUnit)
    {
        return IsSnapped(position.x, pixelsPerUnit) &&
               IsSnapped(position.y, pixelsPerUnit);
    }
}