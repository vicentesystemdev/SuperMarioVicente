using UnityEngine;

// Floor nozzle that shoots a column of fire in a cycle:
// idle -> warning flicker (harmless) -> full flame (Hazard enabled).
public class FireJet : MonoBehaviour
{
    public Transform flame;
    public float flameHeight = 3f;
    public float idleTime = 1.8f;
    public float warningTime = 0.6f;
    public float activeTime = 1.2f;
    public float offset = 0f;

    private SpriteRenderer flameRenderer;
    private Collider2D flameCollider;
    private bool wasActive;

    private void Awake()
    {
        flameRenderer = flame.GetComponent<SpriteRenderer>();
        flameCollider = flame.GetComponent<Collider2D>();
    }

    private void Update()
    {
        float cycle = idleTime + warningTime + activeTime;
        float t = Mathf.Repeat(Time.time + offset, cycle);

        bool warning = t >= idleTime && t < idleTime + warningTime;
        bool active = t >= idleTime + warningTime;

        flameRenderer.enabled = warning || active;
        flameCollider.enabled = active;

        // the warning is a short flickering flame
        float height = active ? flameHeight : flameHeight * 0.25f;

        if (warning) {
            flameRenderer.enabled = Time.frameCount % 6 < 4;
        }

        flame.localScale = new Vector3(1f, height / flameHeight, 1f);
        flame.localPosition = new Vector3(0f, 0.5f + height / 2f, 0f);

        if (active && !wasActive && CameraUtil.IsVisible(transform.position, 2f)) {
            Sfx.Play(ChiptuneSynth.Fire, 0.7f);
        }

        wasActive = active;
    }

}
