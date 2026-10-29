using UnityEngine;

// Fireball that flies in a straight line (cannons, boss attacks, ground
// shockwaves). Damage comes from a Hazard component on the same object.
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float lifetime = 6f;

    private Vector2 velocity;

    private void Awake()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction, float speed)
    {
        velocity = direction.normalized * speed;

        // sprites face left; tilt them for diagonal shots
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = velocity.x > 0f;

        float angle = Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, velocity.x > 0f ? angle : -angle);
    }

    private void Update()
    {
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

}
