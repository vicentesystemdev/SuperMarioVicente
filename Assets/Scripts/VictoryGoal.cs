using System.Collections;
using UnityEngine;

// The castle treasure: touching it ends the adventure and loads the
// victory screen.
[RequireComponent(typeof(BoxCollider2D))]
public class VictoryGoal : MonoBehaviour
{
    public float delay = 2.5f;

    private bool reached;
    private Vector3 origin;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        origin = transform.position;
    }

    private void Update()
    {
        // floating animation
        if (!reached) {
            transform.position = origin + Vector3.up * Mathf.Sin(Time.time * 3f) * 0.15f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || !other.CompareTag("Player")) {
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player == null || player.dead) {
            return;
        }

        reached = true;
        StartCoroutine(Finish(other.gameObject));
    }

    private IEnumerator Finish(GameObject player)
    {
        player.GetComponent<PlayerMovement>().enabled = false;

        LevelMusic music = FindAnyObjectByType<LevelMusic>();

        if (music != null) {
            music.Stop();
        }

        Sfx.Play(ChiptuneSynth.Fanfare);

        // the treasure rises above Mario
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < 1f)
        {
            transform.position = Vector3.Lerp(start, player.transform.position + Vector3.up * 2.5f, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(delay - 1f);

        if (GameManager.Instance != null) {
            GameManager.Instance.Victory();
        }
    }

}
