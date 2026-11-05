using UnityEngine;

public class HudAcuatico : MonoBehaviour
{
    private PixelText coinsValue;
    private int lastCoins = -1;

    private void Awake()
    {
        Transform value = transform.Find("Barra Superior/Coins Value");

        if (value != null)
            coinsValue = value.GetComponent<PixelText>();
    }

    private void Update()
    {
        if (coinsValue == null || GameManager.Instance == null)
            return;

        int coins = GameManager.Instance.coins;

        if (coins == lastCoins)
            return;

        lastCoins = coins;
        coinsValue.SetText($"X{coins:00}");
    }
}
