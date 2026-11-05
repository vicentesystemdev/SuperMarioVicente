using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type
    {
        Coin,
        ExtraLife,
        MagicMushroom,
        Starpower,
    }

    public Type type;

    // Destroy happens at the end of the frame, a second trigger in the
    // same frame must not give the item (and its points) twice
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected && other.CompareTag("Player")) {
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        collected = true;

        if (type == Type.MagicMushroom || type == Type.Starpower) {
            GameManager.Instance.AddScore(GameManager.PowerUpPoints);
        }

        switch (type)
        {
            case Type.Coin:
                GameManager.Instance.AddCoin();
                Sfx.Play(ChiptuneSynth.Coin);
                break;

            case Type.ExtraLife:
                GameManager.Instance.AddLife();
                Sfx.Play(ChiptuneSynth.OneUp);
                break;

            case Type.MagicMushroom:
                player.GetComponent<Player>().Grow();
                Sfx.Play(ChiptuneSynth.PowerUpCollect);
                break;

            case Type.Starpower:
                player.GetComponent<Player>().Starpower();
                Sfx.Play(ChiptuneSynth.PowerUpCollect);
                break;
        }

        Destroy(gameObject);
    }

}
