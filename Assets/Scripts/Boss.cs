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
    public float walkSpeed = 2.8f;
    public float acceleration = 28f;
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
    private float targetVelocityX;
    private Vector3 baseScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private bool wasGrounded = true;
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
        baseScale = transform.localScale;
        targetScale = baseScale;
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

        bool isOnGround = OnGround();
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
        velocity.y += gravity * Time.fixedDeltaTime;

        if (velocity.y < 0f && isOnGround)
        {
            if (!wasGrounded && velocity.y < -6f) {
                PulseScale(1.18f, 0.84f);
            }
            velocity.y = 0f;
        }

        wasGrounded = isOnGround && velocity.y <= 0f;

        Vector2 position = body.position + velocity * Time.fixedDeltaTime;
        if (position.x <= minX || position.x >= maxX)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
            velocity.x = 0f;
        }

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

        if (!attacking)
        {
            float animRate = Mathf.Abs(velocity.x) > 0.5f ? (phaseTwo ? 6f : 4.2f) : 2.5f;
            spriteRenderer.sprite = Mathf.FloorToInt(Time.time * animRate) % 2 == 0 ? idle1 : idle2;
        }

        // subtle enraged fiery glow in phase 2
        if (!invulnerable) {
            spriteRenderer.color = enraged ? new Color(1f, 0.82f, 0.72f) : Color.white;
        }

        // smooth squash-and-stretch recovery + walking step bounce
        targetScale = Vector3.Lerp(targetScale, baseScale, 10f * Time.deltaTime);
        Vector3 stepBounce = Vector3.zero;
        if (grounded && Mathf.Abs(velocity.x) > 0.4f && !attacking)
        {
            float bob = Mathf.Sin(Time.time * (phaseTwo ? 16f : 11f)) * 0.05f;
            stepBounce = new Vector3(-bob * 0.5f, bob, 0f);
        }
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale + stepBounce, 16f * Time.deltaTime);

        // blink while invulnerable
        spriteRenderer.enabled = !invulnerable || Time.frameCount % 6 < 3;
    }

    private void PulseScale(float scaleX, float scaleY)
    {
        transform.localScale = new Vector3(baseScale.x * scaleX, baseScale.y * scaleY, baseScale.z);
        targetScale = baseScale;
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
            yield return Roar(0.9f);
        }

        int cycle = 0;

        while (!dead)
        {
            yield return TacticalMove();

            float roll = Random.value;

            if (phaseTwo)
            {
                if (roll < 0.38f) {
                    yield return SlamAttack();
                } else if (roll < 0.65f) {
                    yield return ChargeAttack();
                } else if (roll < 0.82f) {
                    yield return ArcJumpAttack();
                } else {
                    yield return Breathe(3);
                }
            }
            else
            {
                if (cycle % 3 == 1 || roll < 0.35f) {
                    yield return ArcJumpAttack();
                } else if (roll < 0.55f) {
                    yield return ChargeAttack();
                } else {
                    yield return Breathe(1);
                }
            }

            cycle++;
            yield return new WaitForSeconds(phaseTwo ? 0.35f : 0.65f);
        }
    }

    private IEnumerator Roar(float duration)
    {
        attacking = true;
        spriteRenderer.sprite = attack;
        targetVelocityX = 0f;
        velocity.x = 0f;
        PulseScale(1.2f, 0.85f);
        Sfx.Play(ChiptuneSynth.BossRoar);
        ShakeCamera(duration * 0.6f, 0.15f);

        yield return new WaitForSeconds(duration);
        attacking = false;
    }

    // Smart tactical movement: stalks Mario around an ideal combat range,
    // weaves with rhythmic Bowser-style hops, and repositions if cornered
    private IEnumerator TacticalMove()
    {
        float speed = phaseTwo ? walkSpeed * 1.65f : walkSpeed;
        float playerX = player != null ? player.position.x : (minX + maxX) * 0.5f;
        float distanceToMario = playerX - body.position.x;
        float dirToMario = Mathf.Sign(distanceToMario);
        if (Mathf.Abs(distanceToMario) < 0.1f) {
            dirToMario = facing;
        }

        float target;
        if (Mathf.Abs(distanceToMario) > 11f)
        {
            // close the gap toward Mario
            target = playerX - dirToMario * Random.Range(4.5f, 7.5f);
        }
        else if (Mathf.Abs(distanceToMario) < 4.2f)
        {
            // Mario is very close: agile back-step or leap over to keep spacing dynamic
            target = body.position.x - dirToMario * Random.Range(4f, 7f);
            if (grounded)
            {
                PulseScale(0.88f, 1.15f);
                velocity.y = 9.5f;
            }
        }
        else
        {
            // weave around Mario at medium range
            float offset = Random.value < 0.65f ? Random.Range(3.5f, 7f) : -Random.Range(3.5f, 6f);
            target = playerX - dirToMario * offset;
        }

        target = Mathf.Clamp(target, minX + 1.5f, maxX - 1.5f);

        float duration = phaseTwo ? Random.Range(1.0f, 1.6f) : Random.Range(1.3f, 2.0f);
        float elapsed = 0f;
        float nextHopTime = Random.Range(0.25f, 0.65f);

        while (Mathf.Abs(target - body.position.x) > 0.35f && elapsed < duration && !dead)
        {
            targetVelocityX = Mathf.Sign(target - body.position.x) * speed;

            // rhythmic hops so the boss feels lively and Mario can duck under high jumps
            if (grounded && elapsed >= nextHopTime)
            {
                PulseScale(0.9f, 1.14f);
                velocity.y = Random.value < 0.45f ? 11.5f : 7.8f;
                nextHopTime = elapsed + Random.Range(0.55f, 1.0f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        targetVelocityX = 0f;
    }

    // Classic Bowser-style high arc jump with a fireball shot near the apex
    private IEnumerator ArcJumpAttack()
    {
        if (!grounded) {
            yield return new WaitUntil(() => grounded || dead);
        }
        if (dead) {
            yield break;
        }

        // crouch wind-up before jumping
        targetVelocityX = 0f;
        PulseScale(1.2f, 0.8f);
        yield return new WaitForSeconds(0.18f);

        float dir = player != null ? Mathf.Sign(player.position.x - body.position.x) : facing;
        float horizSpeed = (phaseTwo ? walkSpeed * 1.45f : walkSpeed * 1.2f) * dir;

        PulseScale(0.85f, 1.18f);
        velocity.y = 13.2f;
        velocity.x = horizSpeed;
        targetVelocityX = horizSpeed;

        // wait until near the apex of the jump, then breathe fire in mid-air
        yield return new WaitForSeconds(0.28f);

        if (!dead)
        {
            attacking = true;
            spriteRenderer.sprite = attack;
            FireProjectiles(phaseTwo ? 2 : 1);
            yield return new WaitForSeconds(0.22f);
            attacking = false;
        }

        yield return new WaitUntil(() => grounded || dead);
        targetVelocityX = 0f;
    }

    // Telegraphed horizontal dash towards Mario, followed by a brief recovery window
    private IEnumerator ChargeAttack()
    {
        targetVelocityX = 0f;
        velocity.x = 0f;
        attacking = true;
        spriteRenderer.sprite = attack;

        // wind-up crouch telegraph
        PulseScale(1.22f, 0.78f);
        Sfx.Play(ChiptuneSynth.BossRoar);
        yield return new WaitForSeconds(phaseTwo ? 0.28f : 0.4f);

        float dir = player != null ? Mathf.Sign(player.position.x - body.position.x) : facing;
        float dashSpeed = walkSpeed * (phaseTwo ? 2.8f : 2.2f);
        float dashTime = phaseTwo ? 0.65f : 0.55f;
        float elapsed = 0f;

        PulseScale(1.15f, 0.9f);

        while (elapsed < dashTime && !dead)
        {
            targetVelocityX = dir * dashSpeed;
            velocity.x = dir * dashSpeed;

            if ((dir < 0f && body.position.x <= minX + 1f) || (dir > 0f && body.position.x >= maxX - 1f)) {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        targetVelocityX = 0f;
        attacking = false;
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator Breathe(int count)
    {
        targetVelocityX = 0f;
        attacking = true;
        spriteRenderer.sprite = attack;
        PulseScale(1.12f, 0.9f);

        // wind up, gives Mario time to react
        yield return new WaitForSeconds(phaseTwo ? 0.3f : 0.45f);

        FireProjectiles(count);

        yield return new WaitForSeconds(0.3f);
        attacking = false;
    }

    private void FireProjectiles(int count)
    {
        Vector3 mouth = transform.position + new Vector3(facing * 1.1f, 0.3f, 0f);
        Vector2 aim = player != null ? (Vector2)(player.position - mouth) : new Vector2(facing, 0f);
        aim.x = facing * Mathf.Max(Mathf.Abs(aim.x), 0.1f);
        aim.y = Mathf.Clamp(aim.y / Mathf.Abs(aim.x), -0.35f, 0.35f) * Mathf.Abs(aim.x);
        aim.Normalize();

        for (int i = 0; i < count; i++)
        {
            float spread = count == 1 ? 0f : (i - (count - 1) / 2f) * 0.28f;
            Vector2 direction = new Vector2(aim.x, aim.y + spread);

            Projectile fireball = Instantiate(fireballPrefab, mouth, Quaternion.identity);
            fireball.Launch(direction, phaseTwo ? 7.5f : 6.2f);
        }

        PulseScale(0.92f, 1.08f);
        Sfx.Play(ChiptuneSynth.BossFire);
    }

    private IEnumerator SlamAttack()
    {
        // crouch wind-up before leaping
        targetVelocityX = 0f;
        PulseScale(1.22f, 0.78f);
        yield return new WaitForSeconds(0.2f);

        // leap towards Mario
        PulseScale(0.84f, 1.2f);
        velocity.y = 14.5f;
        float leapSpeed = player != null ? Mathf.Clamp((player.position.x - transform.position.x) * 1.1f, -walkSpeed * 2.2f, walkSpeed * 2.2f) : 0f;
        velocity.x = leapSpeed;
        targetVelocityX = leapSpeed;

        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => grounded || dead);

        targetVelocityX = 0f;
        velocity.x = 0f;

        if (dead) {
            yield break;
        }

        PulseScale(1.26f, 0.74f);
        Sfx.Play(ChiptuneSynth.Slam);
        ShakeCamera(0.35f, 0.25f);

        // fire waves along the floor in both directions
        Vector3 floor = transform.position + Vector3.down * 0.5f;

        foreach (float side in new[] { -1f, 1f })
        {
            Projectile wave = Instantiate(shockwavePrefab, floor + Vector3.right * side * 1.2f, Quaternion.identity);
            wave.Launch(new Vector2(side, 0f), 6.8f);
        }

        yield return new WaitForSeconds(0.5f);
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
            // squash and recoil away from Mario instead of jumping up into him
            PulseScale(1.25f, 0.72f);
            float pushDir = player != null ? Mathf.Sign(transform.position.x - player.position.x) : -facing;
            velocity.x = pushDir * 5f;
            velocity.y = 3f;
        }
    }

    private void Defeat()
    {
        dead = true;
        StopAllCoroutines();

        if (GameManager.Instance != null) {
            GameManager.Instance.AddScore(GameManager.BossPoints);
        }

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
