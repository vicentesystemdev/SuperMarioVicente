using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HudAcuatico : MonoBehaviour
{
    private PixelText coinsValue;
    private int lastCoins = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void PrepararHud()
    {
        if (SceneManager.GetActiveScene().name != "2-2")
            return;

        if (FindFirstObjectByType<HudAcuatico>() != null)
            return;

        GameObject canvasObject = new GameObject(
            "HUD Acuatico",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject bar = new GameObject("Barra Superior", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 150f);

        Image image = bar.GetComponent<Image>();
        image.color = new Color(0.015f, 0.04f, 0.12f, 0.72f);
        image.raycastTarget = false;

        CreateText("Mario Label", bar.transform, "MARIO", new Vector2(190f, -42f));
        CreateText("Score Label", bar.transform, "000000", new Vector2(190f, -92f));
        CreateText("World Label", bar.transform, "MUNDO 2-2", new Vector2(960f, -42f));
        CreateText("Coins Label", bar.transform, "MONEDAS", new Vector2(570f, -42f));
        CreateText("Coins Value", bar.transform, "X00", new Vector2(570f, -92f));

        canvasObject.AddComponent<HudAcuatico>();
    }

    private static void CreateText(string name, Transform parent, string value, Vector2 position)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(RawImage),
            typeof(PixelText)
        );

        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        RawImage image = textObject.GetComponent<RawImage>();
        image.raycastTarget = false;

        PixelText text = textObject.GetComponent<PixelText>();
        text.text = value;
        text.pixelSize = 5f;
        text.color = Color.white;
        text.Refresh();
    }

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
