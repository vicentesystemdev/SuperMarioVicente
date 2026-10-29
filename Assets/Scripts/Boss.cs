using System.Collections;
using UnityEngine;

// Magmor, the lava king: original final boss that plays the role of Bowser.
//
// Phase 1: paces the arena and breathes single fireballs aimed at Mario.
// Phase 2 (half health): faster, fires 3-way volleys and jumps to slam the
// floor, sending a shockwave of fire in both directions.
//
// Mario damages it by stomping on its head (or with starpower). After each
// hit it is invulnerable for a moment and blinks. Touching it from the side
// hurts Mario.
[RequireComponent(typeof(Rigidbody2D))]
public class Boss : MonoBehaviour
{
    public int maxHealth = 6;
    public float walkSpeed = 2.2f;
    public float gravity = -30f;
    public float invulnerableTime = 1.2f;
    public float minX;
    public float maxX;

    public Sprite idle1;
    public Sprite idle2;
    public Sprite attack;
    public Sprite defeated;

    public Projectile fireballPrefab;
    public Projectile shockwavePrefab;
    public BossHealthBar healthBar;
    public BossArena arena;

    public int health { get; private set; }
    public bool phaseTwo => health <= maxHealth / 2;
    public bool dead { get; private set; }

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;
    private Transform player;
    private Vector2 velocity;
    private float invulnerableUntil;
    private bool fighting;
    private bool attacking;
    private bool enraged;

    private bool invulnerable => Time.time < invulnerableUntil;
    private float facing => spriteRenderer.flipX ? 1f : -1f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        health = maxHealth;
    }

    private void Start()
    {
        Player mario = FindAnyObjectByType<Player>();

        if (mario != null) {
            player = mario.transform;
        }
    }

    public void StartFight()
    {
        if (fighting || dead) {
            return;
        }

        fighting = true;

        if (healthBar != null) {
            healthBar.Show();
        }

        StartCoroutine(Brain(true));
    }

    // movement

    private void FixedUpdate()
    {
        if (dead) {
            return;
        }

        velocity.y += gravity * Time.fixedDeltaTime;

        if (velocity.y < 0f && OnGround()) {
            velocity.y = 0f;
        }

        Vector2 position = body.position + velocity * Time.fixedDeltaTime;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        body.MovePosition(position);
    }

    private void Update()
    {
        if (dead) {
            return;
        }

        // face mario
        if (player != null) {
            spriteRenderer.flipX = player.position.x > transform.position.x;
        }

        if (!attacking) {
            spriteRenderer.sprite = Mathf.FloorToInt(Time.time * 2.5f) % 2 == 0 ? idle1 : idle2;
        }

        // blink while invulnerable
        spriteRenderer.enabled = !invulnerable || Time.frameCount % 6 < 3;
    }

    private bool grounded => velocity.y <= 0f && OnGround();

    // Extensions.Raycast is sized for 1 tile entities, the boss is 2 tiles
    // tall, so it checks below its own collider instead
    private bool OnGround()
    {
        Bounds bounds = bossCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.1f);
        Vector2 size = new Vector2(bounds.size.x * 0.8f, 0.1f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.15f, LayerMask.GetMask("Default"));
        return hit.collider != null && hit.collider != bossCollider;
    }

    // behaviour

    private IEnumerator Brain(bool intro)
    {
        if (intro) {
            yield return Roar(1f);
        }

        while (!dead)
        {
            yield return Walk();

            if (phaseTwo && Random.value < 0.45f) {
                yield return SlamAttack();
            } else {
                yield return Breathe(phaseTwo ? 3 : 1);
            }

            yield return new WaitForSeconds(phaseTwo ? 0.5f : 0.9f);
        }
    }

    private IEnumerator Roar(float duration)
    {
        attacking = true;
        spriteRenderer.sprite = attack;
        velocity.x = 0f;
        Sfx.Play(ChiptuneSynth.BossRoar);
        ShakeCamera(duration * 0.6f, 0.15f);

        yield return new WaitForSeconds(duration);
        attacking = false;
    }

    private IEnumerator Walk()
    {
        float speed = phaseTwo ? walkSpeed * 1.6f : walkSpeed;
        float target = Random.Range(minX, maxX);
        float elapsed = 0f;

        while (Mathf.Abs(target - body.position.x) > 0.2f && elapsed < 2.5f)
        {
            velocity.x = Mathf.Sign(target - body.position.x) * speed;

            // small hops in phase 1 keep the movement lively
            if (!phaseTwo && grounded && Random.value < 0.01f) {
                velocity.y = 8f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        velocity.x = 0f;
    }

    private IEnumerator Breathe(int count)
    {
        attacking = true;
        spriteRenderer.sprite = attack;

        // wind up, gives Mario time to react
        yield return new WaitForSeconds(phaseTwo ? 0.35f : 0.5f);

        Vector3 mouth = transform.position + new Vector3(facing * 1.1f, 0.3f, 0f);
        Vector2 aim = player != null ? (Vector2)(player.position - mouth) : new Vector2(facing, 0f);
        aim.x = facing * Mathf.Max(Mathf.Abs(aim.x), 0.1f);
        aim.y = Mathf.Clamp(aim.y / Mathf.Abs(aim.x), -0.35f, 0.35f) * Mathf.Abs(aim.x);
        aim.Normalize();

        for (int i = 0; i < count; i++)
        {
            float spread = count == 1 ? 0f : (i - (count - 1) / 2f) * 0.3f;
            Vector2 direction = new Vector2(aim.x, aim.y + spread);

            Projectile fireball = Instantiate(fireballPrefab, mouth, Quaternion.identity);
            fireball.Launch(direction, phaseTwo ? 7f : 6f);
        }

        Sfx.Play(ChiptuneSynth.BossFire);

        yield return new WaitForSeconds(0.35f);
        attacking = false;
    }

    private IEnumerator SlamAttack()
    {
        // leap towards Mario
        velocity.y = 14f;
        velocity.x = player != null ? Mathf.Sign(player.position.x - transform.position.x) * walkSpeed * 1.5f : 0f;

        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => grounded || dead);

        velocity.x = 0f;

        if (dead) {
            yield break;
        }

        Sfx.Play(ChiptuneSynth.Slam);
        ShakeCamera(0.35f, 0.25f);

        // fire waves along the floor in both directions
        Vector3 floor = transform.position + Vector3.down * 0.5f;

        foreach (float side in new[] { -1f, 1f })
        {
            Projectile wave = Instantiate(shockwavePrefab, floor + Vector3.right * side * 1.2f, Quaternion.identity);
            wave.Launch(new Vector2(side, 0f), 6.5f);
        }

        yield return new WaitForSeconds(0.6f);
    }

    // damage

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead || !collision.gameObject.CompareTag("Player")) {
            return;
        }

        Player mario = collision.gameObject.GetComponent<Player>();

        if (mario.starpower || collision.transform.DotTest(transform, Vector2.down)) {
            TakeDamage();
        } else {
            mario.Hit();
        }
    }

    // standing against its side keeps hurting once Mario's invulnerability ends
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (dead || !collision.gameObject.CompareTag("Player")) {
            return;
        }

        if (!collision.transform.DotTest(transform, Vector2.down)) {
            collision.gameObject.GetComponent<Player>().Hit();
        }
    }

    public void TakeDamage()
    {
        if (dead || invulnerable || !fighting) {
            return;
        }

        health--;
        invulnerableUntil = Time.time + invulnerableTime;
        Sfx.Play(ChiptuneSynth.BossHurt);

        if (healthBar != null) {
            healthBar.Set(health / (float)maxHealth);
        }

        if (health <= 0)
        {
            Defeat();
        }
        else if (phaseTwo && !enraged)
        {
            // phase change: roar and restart with the new behaviour
            enraged = true;
            StopAllCoroutines();
            attacking = false;
            StartCoroutine(Brain(true));
        }
        else
        {
            // knock back away from Mario
            velocity.y = 6f;
        }
    }

    private void Defeat()
    {
        dead = true;
        StopAllCoroutines();

        if (healthBar != null) {
            healthBar.Hide();
        }

        StartCoroutine(DefeatAnimation());
    }

    private IEnumerator DefeatAnimation()
    {
        bossCollider.enabled = false;
        body.bodyType = RigidbodyType2D.Kinematic;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = defeated;

        Sfx.Play(ChiptuneSynth.BossDefeat);
        ShakeCamera(1.2f, 0.2f);

        // flash
        float elapsed = 0f;

        while (elapsed < 1.2f)
        {
            spriteRenderer.color = Time.frameCount % 8 < 4 ? new Color(1f, 0.5f, 0.4f) : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = Color.white;
        spriteRenderer.flipY = true;

        // jump and fall off the arena
        Vector3 fall = Vector3.up * 10f;
        elapsed = 0f;

        while (elapsed < 2f)
        {
            transform.position += fall * Time.deltaTime;
            fall.y += gravity * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (arena != null) {
            arena.OnBossDefeated();
        }

        Destroy(gameObject);
    }

    private static void ShakeCamera(float duration, float strength)
    {
        if (Camera.main == null) {
            return;
        }

        CastleCamera castleCamera = Camera.main.GetComponent<CastleCamera>();

        if (castleCamera != null) {
            castleCamera.Shake(duration, strength);
        }
    }

}
