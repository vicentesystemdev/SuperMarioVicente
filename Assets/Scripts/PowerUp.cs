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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
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
