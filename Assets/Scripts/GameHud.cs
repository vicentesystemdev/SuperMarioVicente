using UnityEngine;
using UnityEngine.UI;

// NES style heads-up display (score, coins, world and time). It is created
// at runtime by the GameManager as a child of its persistent object, so
// there is exactly one HUD across every scene. It only reads the state of
// the GameManager and redraws a label when its value changes.
//
// The columns are anchored to the left and right edges of the top band,
// keeping the center free for the boss health bar of 8-4.
public class GameHud : MonoBehaviour
{
    private const float LabelY = -22f;
    private const float ValueY = -58f;
    private const float PixelSize = 4f;

    private static readonly Color HurryColor = new Color(1f, 0.35f, 0.25f);

    private CanvasGroup group;
    private PixelText scoreValue;
    private PixelText coinsValue;
    private PixelText worldValue;
    private PixelText timeValue;

    private int shownScore = -1;
    private int shownCoins = -1;
    private int shownTime = -1;
    private int shownWorld = -1;
    private int shownStage = -1;

    public static GameHud Create(Transform parent)
    {
        GameObject root = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        root.transform.SetParent(parent, false);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // above the level UI (boss bar uses 10), below the screen fader
        canvas.sortingOrder = 100;

        // same reference as the other canvases of the project
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameHud hud = root.AddComponent<GameHud>();
        hud.group = root.GetComponent<CanvasGroup>();
        hud.group.interactable = false;
        hud.group.blocksRaycasts = false;

        hud.scoreValue = hud.Column("Score", "MARIO", 0f, 150f);
        hud.coinsValue = hud.Column("Coins", "MONEDAS", 0f, 390f);
        hud.worldValue = hud.Column("World", "MUNDO", 1f, -390f);
        hud.timeValue = hud.Column("Time", "TIEMPO", 1f, -150f);

        hud.SetVisible(false);
        return hud;
    }

    private PixelText Column(string name, string label, float anchorX, float x)
    {
        RectTransform column = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        column.SetParent(transform, false);
        column.anchorMin = column.anchorMax = new Vector2(anchorX, 1f);
        column.pivot = new Vector2(0.5f, 1f);
        column.anchoredPosition = new Vector2(x, 0f);
        column.sizeDelta = Vector2.zero;

        Text(label + " Label", column, label, LabelY);
        return Text(label + " Value", column, "", ValueY);
    }

    private static PixelText Text(string name, Transform parent, string value, float y)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        textObject.transform.SetParent(parent, false);
        textObject.GetComponent<RawImage>().raycastTarget = false;

        RectTransform rect = (RectTransform)textObject.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);

        PixelText text = textObject.AddComponent<PixelText>();
        text.pixelSize = PixelSize;
        text.SetText(value);
        return text;
    }

    public void SetVisible(bool visible)
    {
        group.alpha = visible ? 1f : 0f;

        // force a redraw of every value the next time it is shown
        shownScore = shownCoins = shownTime = shownWorld = shownStage = -1;
    }

    private void LateUpdate()
    {
        GameManager game = GameManager.Instance;

        if (game == null || group.alpha <= 0f) {
            return;
        }

        if (game.score != shownScore)
        {
            shownScore = game.score;
            scoreValue.SetText(shownScore.ToString("D6"));
        }

        if (game.coins != shownCoins)
        {
            shownCoins = game.coins;
            coinsValue.SetText("×" + shownCoins.ToString("D2"));
        }

        if (game.world != shownWorld || game.stage != shownStage)
        {
            shownWorld = game.world;
            shownStage = game.stage;
            worldValue.SetText($"{shownWorld}-{shownStage}");
        }

        if (game.time != shownTime)
        {
            shownTime = game.time;
            timeValue.color = game.hurryUp ? HurryColor : Color.white;
            timeValue.SetText(shownTime.ToString("D3"));
        }
    }

}
