using UnityEngine;

// Fireball that leaps out of the lava and falls back in a fixed rhythm.
public class LavaSpark : MonoBehaviour
{
    public float height = 5f;
    public float period = 3f;
    public float offset = 0f;
    public float gravity = 30f;

    private SpriteRenderer spriteRenderer;
    private Collider2D sparkCollider;
    private float baseY;
    private bool wasFlying;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sparkCollider = GetComponent<Collider2D>();
        baseY = transform.position.y;
    }

    private void Update()
    {
        float launchSpeed = Mathf.Sqrt(2f * gravity * height);
        float airTime = 2f * launchSpeed / gravity;
        float t = Mathf.Repeat(Time.time + offset, Mathf.Max(period, airTime + 0.2f));

        bool flying = t < airTime;
        spriteRenderer.enabled = flying;
        sparkCollider.enabled = flying;

        Vector3 position = transform.position;
        position.y = flying ? baseY + launchSpeed * t - 0.5f * gravity * t * t : baseY;
        transform.position = position;

        // head first: upside down while falling
        spriteRenderer.flipY = flying && launchSpeed - gravity * t < 0f;

        if (flying && !wasFlying && CameraUtil.IsVisible(position, 1f)) {
            Sfx.Play(ChiptuneSynth.LavaBurst, 0.6f);
        }

        wasFlying = flying;
    }

}
