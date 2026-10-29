using UnityEngine;

// Trigger that hurts Mario on contact (spikes, fire, projectiles) or kills
// him instantly (lava). Lives on the Ignore Raycast layer so it never
// interferes with Mario's ground and wall checks.
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public bool instantKill = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Touch(other);
    }

    // continuous contact hurts again once the invulnerability ends
    private void OnTriggerStay2D(Collider2D other)
    {
        Touch(other);
    }

    private void Touch(Collider2D other)
    {
        if (!other.CompareTag("Player")) {
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player == null || player.dead) {
            return;
        }

        if (instantKill) {
            player.Death();
        } else {
            player.Hit();
        }
    }

}
