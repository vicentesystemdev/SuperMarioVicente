using UnityEngine;

// Banner that saves the respawn point. After a death the level reloads and
// Mario appears at the last activated checkpoint of this level.
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    private SpriteRenderer spriteRenderer;
    private bool active;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<BoxCollider2D>().isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Start()
    {
        GameManager manager = GameManager.Instance;

        if (manager != null && manager.TryGetCheckpoint(out Vector3 position) && (position - transform.position).sqrMagnitude < 0.01f)
        {
            Activate(false);
            Spawn(position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (active || !other.CompareTag("Player") || GameManager.Instance == null) {
            return;
        }

        Activate(true);
        GameManager.Instance.SetCheckpoint(transform.position);
    }

    private void Activate(bool withSound)
    {
        active = true;
        spriteRenderer.sprite = activeSprite;

        if (withSound) {
            Sfx.Play(ChiptuneSynth.CheckpointReached);
        }
    }

    private static void Spawn(Vector3 position)
    {
        Player player = FindAnyObjectByType<Player>();

        if (player == null) {
            return;
        }

        // the banner pivot is at its center, 1 tile above the floor
        Vector3 spawn = position + Vector3.down * 0.5f;
        player.transform.position = spawn;
        player.GetComponent<Rigidbody2D>().position = spawn;

        if (Camera.main != null)
        {
            CastleCamera castleCamera = Camera.main.GetComponent<CastleCamera>();

            if (castleCamera != null) {
                castleCamera.SnapTo(spawn);
            }
        }
    }

}
