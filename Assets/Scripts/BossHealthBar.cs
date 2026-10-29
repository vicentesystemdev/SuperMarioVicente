using UnityEngine;

// On-screen health bar of the boss. The fill is scaled with its anchors, so
// no sliced or filled sprite is required.
public class BossHealthBar : MonoBehaviour
{
    public CanvasGroup group;
    public RectTransform fill;

    private void Awake()
    {
        group.alpha = 0f;
    }

    public void Show()
    {
        group.alpha = 1f;
        Set(1f);
    }

    public void Hide()
    {
        group.alpha = 0f;
    }

    public void Set(float fraction)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(fraction), fill.anchorMax.y);
    }

}
