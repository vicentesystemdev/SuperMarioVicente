using UnityEngine;

public static class CameraUtil
{
    // Checks if a world position is inside the main camera view, expanded
    // by a margin (used to only play sounds / fire when close to Mario).
    public static bool IsVisible(Vector3 position, float margin)
    {
        Camera camera = Camera.main;

        if (camera == null) {
            return false;
        }

        float halfHeight = camera.orthographicSize + margin;
        float halfWidth = camera.orthographicSize * camera.aspect + margin;
        Vector3 center = camera.transform.position;

        return Mathf.Abs(position.x - center.x) <= halfWidth && Mathf.Abs(position.y - center.y) <= halfHeight;
    }

}
