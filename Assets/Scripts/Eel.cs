using UnityEngine;

public class Eel : WaterEnemy
{
    public float patrolSpeed = 1.3f;
    public float dashSpeed = 5.5f;
    public float dashEvery = 2.2f;
    public float dashDuration = 0.55f;

    private float timer;
    private float dashTimer;
    private bool dashing;

    protected override void Awake()
    {
        base.Awake();
        scoreValue = 500;
    }

    protected override void OnActivate()
    {
        timer = dashEvery;
    }

    protected override void Move()
    {
        timer -= Time.deltaTime;

        if (!dashing && timer <= 0f)
        {
            dashing = true;
            dashTimer = dashDuration;
            timer = dashEvery;
            Sfx.Play(ChiptuneSynth.Swim, 0.35f);
        }

        float speed = dashing ? dashSpeed : patrolSpeed;
        Vector3 position = transform.position;
        float direction = -1f;

        if (dashing && player != null)
            direction = Mathf.Sign(player.position.x - position.x);

        position.x += direction * speed * Time.deltaTime;

        if (!dashing)
            position.y += Mathf.Sin(Time.time * 4f) * 0.25f * Time.deltaTime;

        transform.position = position;
        spriteRenderer.flipX = direction > 0f;

        if (dashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
                dashing = false;
        }
    }
}
