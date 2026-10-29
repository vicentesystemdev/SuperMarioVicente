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
            // swim cycle reusing the existing frames: stroke pose after a
            // stroke, slow leg kicks with the run frames while gliding
            run.enabled = false;

            if (movement.stroking || run.sprites.Length == 0) {
                spriteRenderer.sprite = jump;
            } else {
                spriteRenderer.sprite = run.sprites[Mathf.FloorToInt(Time.time * 5f) % run.sprites.Length];
            }

            return;
        }

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
        spriteRenderer.enabled = false;
        run.enabled = false;
    }

}
