using UnityEngine;

public class PufferFish : WaterEnemy
{
    public float speed = 1.2f;
    public float waveAmplitude = 0.7f;
    public float pulseSpeed = 3.5f;

    private float baseY;
    private float time;

    protected override void Awake()
    {
        base.Awake();
        puntos = 400;
    }

    protected override void OnActivate()
    {
        baseY = transform.position.y;
    }

    protected override void Move()
    {
        time += Time.deltaTime;

        Vector3 position = transform.position;
        position.x -= speed * Time.deltaTime;
        position.y = baseY + Mathf.Sin(time * 2f) * waveAmplitude;
        transform.position = position;

        float pulse = 1f + (Mathf.Sin(time * pulseSpeed) + 1f) * 0.18f;
        transform.localScale = new Vector3(pulse, pulse, 1f);
    }
}
