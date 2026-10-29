using UnityEngine;

// Trigger area filled with water: Mario swims while inside it. Also emits
// bubbles from Mario when he strokes and from time to time while swimming.
[RequireComponent(typeof(BoxCollider2D))]
public class WaterZone : MonoBehaviour
{
    public Sprite bubbleSprite;
    public float bubbleInterval = 1.5f;

    // TEMPORARY diagnostics: logs when Mario starts / stops swimming
    public bool logEvents = true;

    private BoxCollider2D area;
    private PlayerMovement swimmer;
    private float nextBubble;

    // highest point Mario's center can reach while swimming
    public float surface => area.bounds.max.y - 0.5f;

    private void Awake()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;

        // Extensions.Raycast (ground / wall checks) casts against the Default
        // layer and queries hit triggers in this project, so a trigger on
        // Default that surrounds Mario makes him always "grounded" and always
        // "against a wall". Like the Pipe and Flag Pole triggers, the water
        // must live on the Ignore Raycast layer.
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Start()
    {
        // Mario may spawn already inside the water
        PlayerMovement movement = FindAnyObjectByType<PlayerMovement>();

        if (movement != null && area.OverlapPoint(movement.transform.position)) {
            Enter(movement);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement movement = other.GetComponent<PlayerMovement>();

        if (movement != null) {
            Enter(movement);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement movement = other.GetComponent<PlayerMovement>();

        if (movement != null && movement == swimmer)
        {
            swimmer.SetSwimming(false);
            swimmer = null;

            if (logEvents) {
                Debug.Log("[WaterZone] Mario sale del agua: SetSwimming(false)");
            }
        }
    }

    private void Enter(PlayerMovement movement)
    {
        if (movement == swimmer && swimmer.swimming) {
            return;
        }

        swimmer = movement;
        swimmer.SetSwimming(true, surface);

        if (logEvents) {
            Debug.Log($"[WaterZone] Mario entra al agua: SetSwimming(true), superficie y = {surface}");
        }
    }

    private void Update()
    {
        if (swimmer == null || !swimmer.enabled || !swimmer.swimming) {
            return;
        }

        if (Input.GetButtonDown("Jump") || Time.time >= nextBubble)
        {
            nextBubble = Time.time + bubbleInterval;
            Bubble.Spawn(bubbleSprite, swimmer.transform.position + new Vector3(0.3f, 0.5f), area.bounds.max.y - 0.5f);
        }
    }

}
