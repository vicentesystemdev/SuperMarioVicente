using UnityEngine;

// Camera for levels with horizontal and vertical movement. It follows Mario
// horizontally inside [minX, maxX] and vertically (smoothed) inside the
// height range of the zone Mario is in, so it never shows outside the map.
// SideScrolling is still used by the other levels.
[RequireComponent(typeof(Camera))]
public class CastleCamera : MonoBehaviour
{
    [System.Serializable]
    public struct Zone
    {
        public float fromX;
        public float toX;
        public float minY;
        public float maxY;
    }

    public float minX;
    public float maxX;
    public Zone[] zones;
    public float smoothTime = 0.2f;
    public float lookUp = 1f;

    private new Camera camera;
    private Transform player;
    private Vector3 basePosition;
    private float velocityY;
    private float shakeUntil;
    private float shakeStrength;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        basePosition = transform.position;

        Player mario = FindAnyObjectByType<Player>();

        if (mario != null) {
            player = mario.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null) {
            return;
        }

        Vector2 target = Target(player.position);
        basePosition.x = target.x;
        basePosition.y = Mathf.SmoothDamp(basePosition.y, target.y, ref velocityY, smoothTime);

        Vector3 position = basePosition;

        if (Time.time < shakeUntil) {
            position += (Vector3)(Random.insideUnitCircle * shakeStrength);
        }

        transform.position = position;
    }

    // jumps to the target without smoothing (used when spawning at a checkpoint)
    public void SnapTo(Vector3 playerPosition)
    {
        Vector2 target = Target(playerPosition);
        basePosition = new Vector3(target.x, target.y, transform.position.z);
        velocityY = 0f;
        transform.position = basePosition;
    }

    public void Shake(float duration, float strength)
    {
        shakeUntil = Time.time + duration;
        shakeStrength = strength;
    }

    private Vector2 Target(Vector3 playerPosition)
    {
        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * camera.aspect;

        float x = Clamp(playerPosition.x, minX, maxX, halfWidth);
        Zone zone = FindZone(playerPosition.x);
        float y = Clamp(playerPosition.y + lookUp, zone.minY, zone.maxY, halfHeight);

        return new Vector2(x, y);
    }

    private Zone FindZone(float x)
    {
        foreach (Zone zone in zones)
        {
            if (x >= zone.fromX && x < zone.toX) {
                return zone;
            }
        }

        return zones.Length > 0 ? zones[zones.Length - 1] : new Zone { minY = -100f, maxY = 100f };
    }

    private static float Clamp(float value, float min, float max, float half)
    {
        // area smaller than the view: center it
        if (max - min <= half * 2f) {
            return (min + max) / 2f;
        }

        return Mathf.Clamp(value, min + half, max - half);
    }

}
