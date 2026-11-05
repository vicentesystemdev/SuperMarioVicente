using UnityEngine;

public class Goomba : MonoBehaviour
{
    public Sprite flatSprite;

    // a goomba can be stomped and hit by a shell in the same frames, it
    // only dies (and gives points) once
    private bool defeated;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!defeated && collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            if (player.starpower) {
                Hit();
            } else if (collision.transform.DotTest(transform, Vector2.down)) {
                Flatten();
            } else {
                player.Hit();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!defeated && other.gameObject.layer == LayerMask.NameToLayer("Shell")) {
            Hit();
        }
    }

    private void Flatten()
    {
        defeated = true;
        GameManager.Instance.AddScore(GameManager.StompPoints);
        Sfx.Play(ChiptuneSynth.Stomp);

        GetComponent<Collider2D>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = flatSprite;
        Destroy(gameObject, 0.5f);
    }

    private void Hit()
    {
        defeated = true;
        GameManager.Instance.AddScore(GameManager.EnemyKillPoints);
        Sfx.Play(ChiptuneSynth.Kick);

        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimation>().enabled = true;
        Destroy(gameObject, 3f);
    }

}
