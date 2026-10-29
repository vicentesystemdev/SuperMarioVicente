using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Full screen black overlay used to fade out / in between scenes. It is
// created at runtime by the GameManager so it persists across scenes.
public class ScreenFader : MonoBehaviour
{
    private CanvasGroup group;

    public float alpha => group.alpha;

    public static ScreenFader Create(Transform parent)
    {
        GameObject root = new GameObject("Screen Fader", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
        root.transform.SetParent(parent, false);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        GameObject black = new GameObject("Black", typeof(RectTransform), typeof(Image));
        black.transform.SetParent(root.transform, false);

        RectTransform rect = black.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = black.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        ScreenFader fader = root.AddComponent<ScreenFader>();
        fader.group = root.GetComponent<CanvasGroup>();
        fader.group.alpha = 0f;
        fader.group.interactable = false;
        fader.group.blocksRaycasts = false;

        return fader;
    }

    public IEnumerator FadeTo(float target, float duration)
    {
        float start = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(start, target, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        group.alpha = target;
    }

}
