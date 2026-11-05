using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteRenderer : MonoBehaviour
{
    private PlayerMovement movement;
    public SpriteRenderer spriteRenderer { get; private set; }
    public Sprite idle;
    public Sprite jump;
    public Sprite slide;
    public AnimatedSprite run;

    private Vector3 restingLocalPosition;

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        restingLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (movement.swimming)
        {
            run.enabled = false;

            float phase = Time.time * 9f;
            float swimTilt = movement.stroking
                ? -28f
                : Mathf.Sin(phase) * 12f;

            float bob = movement.stroking
                ? 0.14f
                : Mathf.Sin(phase * 0.7f) * 0.08f;

            transform.localRotation = Quaternion.Euler(0f, 0f, swimTilt);
            transform.localPosition = restingLocalPosition + Vector3.up * bob;

            if (movement.stroking || run.sprites.Length == 0)
            {
                spriteRenderer.sprite = jump;
            }
            else
            {
                int frame = Mathf.FloorToInt(Time.time * 8f) % run.sprites.Length;
                spriteRenderer.sprite = run.sprites[frame];
            }

            return;
        }

        transform.localRotation = Quaternion.identity;
        transform.localPosition = restingLocalPosition;
        run.enabled = movement.running;

        if (movement.jumping) {
            spriteRenderer.sprite = jump;
        } else if (movement.sliding) {
            spriteRenderer.sprite = slide;
        } else if (!movement.running) {
            spriteRenderer.sprite = idle;
        }
    }

    private void OnEnable()
    {
        spriteRenderer.enabled = true;
    }

    private void OnDisable()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = restingLocalPosition;
        spriteRenderer.enabled = false;
        run.enabled = false;
    }

}
