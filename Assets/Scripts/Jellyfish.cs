using UnityEngine;

// Jellyfish that hunts Mario: it sinks slowly while contracted, then pulses
// diagonally upwards towards him with its tentacles extended.
public class Jellyfish : WaterEnemy
{
    public Sprite contracted;
    public Sprite extended;
    public float sinkSpeed = 1.2f;
    public float pulseSpeed = 5f;
    public float pulseDuration = 0.5f;
    public float restDuration = 0.9f;
    public float minY = -13.5f;
    public float maxY = -4.5f;

    private Vector2 pulseDirection;
    private float timer;
    private bool pulsing;

    protected override void Move()
    {
        timer -= Time.deltaTime;
        Vector3 position = transform.position;

        if (pulsing)
        {
            // decelerating pulse
            float strength = Mathf.Clamp01(timer / pulseDuration);
            position += (Vector3)(pulseDirection * pulseSpeed * strength * Time.deltaTime);

            if (timer <= 0f) {
                StartRest();
            }
        }
        else
        {
            position.y -= sinkSpeed * Time.deltaTime;

            // pulse when the rest ends or when it sinks below Mario
            bool belowPlayer = player != null && position.y < player.position.y - 0.5f;

            if (timer <= 0f && (belowPlayer || position.y <= minY + 0.5f)) {
                StartPulse(position);
            }
        }

        position.y = Mathf.Clamp(position.y, minY, maxY);
        transform.position = position;
    }

    protected override void OnActivate()
    {
        StartRest();
    }

    private void StartRest()
    {
        pulsing = false;
        timer = restDuration;
        spriteRenderer.sprite = contracted;
    }

    private void StartPulse(Vector3 position)
    {
        pulsing = true;
        timer = pulseDuration;
        spriteRenderer.sprite = extended;

        float side = player != null ? Mathf.Sign(player.position.x - position.x) : -1f;
        pulseDirection = new Vector2(side * 0.8f, 1f).normalized;
        Sfx.Play(ChiptuneSynth.Swim, 0.4f);
    }

}
