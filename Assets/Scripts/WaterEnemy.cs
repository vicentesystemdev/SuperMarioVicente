using UnityEngine;

// Base class for enemies that swim freely (they are not affected by gravity
// or walls). They wake up when they come into view, hurt Mario on contact
// (underwater enemies cannot be stomped) and die from starpower.
[RequireComponent(typeof(Rigidbody2D))]
public abstract class WaterEnemy : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    protected Transform player;

    private bool active;
    private bool dead;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
    }

    protected virtual void Start()
    {
        Player mario = FindAnyObjectByType<Player>();

        if (mario != null) {
            player = mario.transform;
        }
    }

    private void Update()
    {
        if (dead) {
            return;
        }

        Camera camera = Camera.main;

        if (camera == null) {
            return;
        }

        float halfWidth = camera.orthographicSize * camera.aspect;
        float left = camera.transform.position.x - halfWidth;
        float right = camera.transform.position.x + halfWidth;

        // wake up just before entering the screen
        if (!active)
        {
            if (transform.position.x - right > 1f) {
                return;
            }

            active = true;
            OnActivate();
        }

        Move();

        // left far behind the camera
        if (transform.position.x < left - 6f) {
            Destroy(gameObject);
        }
    }

    protected virtual void OnActivate() { }

    protected abstract void Move();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dead || !other.CompareTag("Player")) {
            return;
        }

        Player mario = other.GetComponent<Player>();

        if (mario == null || mario.dead) {
            return;
        }

        if (mario.starpower) {
            Hit();
        } else {
            mario.Hit();
        }
    }

    public void Hit()
    {
        dead = true;
        Sfx.Play(ChiptuneSynth.Kick);

        AnimatedSprite animatedSprite = GetComponent<AnimatedSprite>();

        if (animatedSprite != null) {
            animatedSprite.enabled = false;
        }

        GetComponent<DeathAnimation>().enabled = true;
        Destroy(gameObject, 3f);
    }

}
