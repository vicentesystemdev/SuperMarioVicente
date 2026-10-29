using System.Collections;
using UnityEngine;

// Block that shakes and falls shortly after Mario lands on it, then
// reappears after a few seconds.
public class CrumblingPlatform : MonoBehaviour
{
    public float delay = 0.5f;
    public float respawnTime = 3f;

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Vector3 origin;
    private bool crumbling;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        origin = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!crumbling && collision.gameObject.CompareTag("Player") && collision.transform.DotTest(transform, Vector2.down)) {
            StartCoroutine(Crumble());
        }
    }

    private IEnumerator Crumble()
    {
        crumbling = true;

        // shake
        float elapsed = 0f;

        while (elapsed < delay)
        {
            transform.position = origin + (Vector3)(Random.insideUnitCircle * 0.06f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Sfx.Play(ChiptuneSynth.Crumble, 0.8f);
        platformCollider.enabled = false;

        // fall and fade out
        elapsed = 0f;

        while (elapsed < 0.8f)
        {
            transform.position = origin + Vector3.down * (elapsed * elapsed * 10f);
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f - elapsed / 0.8f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(respawnTime);

        transform.position = origin;
        spriteRenderer.color = Color.white;
        spriteRenderer.enabled = true;
        platformCollider.enabled = true;
        crumbling = false;
    }

}
