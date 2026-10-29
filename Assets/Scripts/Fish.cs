using UnityEngine;

// Fish that crosses the screen horizontally following a wave.
public class Fish : WaterEnemy
{
    public float speed = 2f;
    public Vector2 direction = Vector2.left;
    public float waveAmplitude = 0.5f;
    public float waveFrequency = 0.6f;

    private float baseY;
    private float time;

    protected override void OnActivate()
    {
        baseY = transform.position.y;
    }

    protected override void Move()
    {
        time += Time.deltaTime;

        Vector3 position = transform.position;
        position.x += direction.x * speed * Time.deltaTime;
        position.y = baseY + Mathf.Sin(time * waveFrequency * 2f * Mathf.PI) * waveAmplitude;
        transform.position = position;

        // the sprite faces left
        spriteRenderer.flipX = direction.x > 0f;
    }

}
