using UnityEngine;

// Solid platform that moves back and forth with an eased motion and carries
// Mario while he stands on it. Runs before PlayerMovement so the carry is
// applied in the same physics step.
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    public Vector2 offset = new Vector2(8f, 0f);
    public float period = 4f;
    public float phase = 0f;

    private Rigidbody2D body;
    private BoxCollider2D box;
    private PlayerMovement player;
    private Vector2 start;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        box = GetComponent<BoxCollider2D>();
        start = body.position;
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
    }

    private void FixedUpdate()
    {
        float k = (1f - Mathf.Cos(2f * Mathf.PI * (Time.time / period + phase))) / 2f;
        Vector2 target = start + offset * k;
        Vector2 delta = target - body.position;

        body.MovePosition(target);

        if (player != null && IsCarrying()) {
            player.Carry(delta / Time.fixedDeltaTime);
        }
    }

    private bool IsCarrying()
    {
        if (!player.enabled || !player.grounded) {
            return false;
        }

        Bounds bounds = box.bounds;
        Vector3 position = player.transform.position;
        float feet = position.y - 0.5f;

        return Mathf.Abs(feet - bounds.max.y) < 0.25f
            && position.x >= bounds.min.x - 0.45f
            && position.x <= bounds.max.x + 0.45f;
    }

}
