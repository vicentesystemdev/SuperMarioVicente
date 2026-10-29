using UnityEngine;

// Wall cannon that periodically shoots fireballs while it is near the screen.
public class FireCannon : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Vector2 direction = Vector2.left;
    public float speed = 5f;
    public float interval = 2.5f;
    public float offset = 0f;

    private float nextShot;

    private void Start()
    {
        nextShot = Time.time + offset;
    }

    private void Update()
    {
        if (Time.time < nextShot) {
            return;
        }

        nextShot = Time.time + interval;

        if (!CameraUtil.IsVisible(transform.position, 1f)) {
            return;
        }

        Vector3 position = transform.position + (Vector3)(direction.normalized * 0.8f);
        Projectile projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
        projectile.Launch(direction, speed);

        Sfx.Play(ChiptuneSynth.Fire, 0.6f);
    }

}
