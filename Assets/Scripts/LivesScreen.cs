using UnityEngine;
using UnityEngine.UI;

// NES style black screen shown between a death and the restart of the
// level: world, Mario and the remaining lives. It is created at runtime by
// the GameManager (like the HUD and the ScreenFader), so it needs no scene
// and there is only one across the whole game.
public class LivesScreen : MonoBehaviour
{
    private CanvasGroup group;
    private PixelText worldLabel;
    private PixelText livesLabel;
    private Image mario;

    public bool visible => group.alpha > 0f;

    public static LivesScreen Create(Transform parent)
    {
        GameObject root = new GameObject("Lives Screen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        root.transform.SetParent(parent, false);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // covers the level and the HUD (100), stays below the fader
        canvas.sortingOrder = 200;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject background = new GameObject("Black", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(root.transform, false);

        RectTransform rect = (RectTransform)background.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = background.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        LivesScreen screen = root.AddComponent<LivesScreen>();
        screen.group = root.GetComponent<CanvasGroup>();
        screen.group.interactable = false;
        screen.group.blocksRaycasts = false;

        screen.worldLabel = Text("World", root.transform, new Vector2(0f, 110f));
        screen.livesLabel = Text("Lives", root.transform, new Vector2(70f, -40f));

        GameObject marioObject = new GameObject("Mario", typeof(RectTransform), typeof(Image));
        marioObject.transform.SetParent(root.transform, false);
        ((RectTransform)marioObject.transform).anchoredPosition = new Vector2(-110f, -40f);

        screen.mario = marioObject.GetComponent<Image>();
        screen.mario.preserveAspect = true;
        screen.mario.raycastTarget = false;

        screen.Hide();
        return screen;
    }

    private static PixelText Text(string name, Transform parent, Vector2 position)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        textObject.transform.SetParent(parent, false);
        textObject.GetComponent<RawImage>().raycastTarget = false;
        ((RectTransform)textObject.transform).anchoredPosition = position;

        PixelText text = textObject.AddComponent<PixelText>();
        text.pixelSize = 8f;
        return text;
    }

    // marioSprite is optional, the sprite is hidden when it is missing
    public void Show(int world, int stage, int lives, Sprite marioSprite)
    {
        worldLabel.SetText($"MUNDO {world}-{stage}");
        livesLabel.SetText($"× {lives:D2}");

        mario.enabled = marioSprite != null;

        if (marioSprite != null)
        {
            // 8 screen pixels per sprite pixel, like the text
            mario.sprite = marioSprite;
            mario.rectTransform.sizeDelta = marioSprite.rect.size * 8f;
        }

        group.alpha = 1f;
    }

    public void Hide()
    {
        group.alpha = 0f;
    }

}
