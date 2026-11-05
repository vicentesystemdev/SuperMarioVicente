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

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (movement.swimming && !movement.grounded)
        {
            // El balanceo hace que Mario se vea nadando y no simplemente
            // flotando mientras cambia entre los sprites existentes.
            float swimTilt = movement.stroking
                ? -12f
                : Mathf.Sin(Time.time * 7f) * 6f;

            transform.localRotation = Quaternion.Euler(0f, 0f, swimTilt);
            run.enabled = false;

            if (movement.stroking || run.sprites.Length == 0) {
                spriteRenderer.sprite = jump;
            } else {
                spriteRenderer.sprite = run.sprites[Mathf.FloorToInt(Time.time * 5f) % run.sprites.Length];
            }

            return;
        }

        transform.localRotation = Quaternion.identity;
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
        spriteRenderer.enabled = false;
        run.enabled = false;
    }

}
