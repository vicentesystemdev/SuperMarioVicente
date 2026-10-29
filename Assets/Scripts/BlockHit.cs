using System.Collections;
using UnityEngine;

public class BlockHit : MonoBehaviour
{
    public GameObject item;
    public Sprite emptyBlock;
    public int maxHits = -1;
    // empty bricks break when hit by big Mario
    public bool breakable;
    private bool animating;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!animating && maxHits != 0 && collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.DotTest(transform, Vector2.up))
            {
                Player player = collision.gameObject.GetComponent<Player>();

                if (breakable && item == null && player != null && player.big) {
                    Break();
                } else {
                    Hit();
                }
            }
        }
    }

    private void Break()
    {
        Sfx.Play(ChiptuneSynth.BrickBreak);

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;

        for (int i = 0; i < 4; i++)
        {
            float x = i % 2 == 0 ? -1f : 1f;
            float y = i < 2 ? 1f : -1f;

            Vector3 position = transform.position + new Vector3(x, y) * 0.25f;
            BrickDebris.Spawn(sprite, position, new Vector2(x * 3f, y > 0f ? 12f : 8f));
        }

        Destroy(gameObject);
    }

    private void Hit()
    {
        Sfx.Play(ChiptuneSynth.Bump);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true; // show if hidden

        maxHits--;

        if (maxHits == 0) {
            spriteRenderer.sprite = emptyBlock;
        }

        if (item != null) {
            Instantiate(item, transform.position, Quaternion.identity);
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        animating = true;

        Vector3 restingPosition = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * 0.5f;

        yield return Move(restingPosition, animatedPosition);
        yield return Move(animatedPosition, restingPosition);

        animating = false;
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        float duration = 0.125f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            transform.localPosition = Vector3.Lerp(from, to, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = to;
    }

}
