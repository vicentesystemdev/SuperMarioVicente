using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the Game Over scene: intro animation, jingle and the
// REINTENTAR / SALIR buttons (wired as persistent onClick events).
public class GameOverScreen : MonoBehaviour
{
    public CanvasGroup content;
    public RectTransform title;
    public PixelText worldLabel;
    public CanvasGroup buttons;
    public Button retryButton;
    public Button quitButton;
    public RectTransform cursor;
    public AudioSource audioSource;

    // optional, a synthesized jingle is used when left empty
    public AudioClip gameOverSound;
    public string firstLevelScene = "1-1";
    public float titleDropDuration = 1.2f;
    public float buttonsDelay = 0.5f;

    private GameObject lastSelected;
    private bool leaving;

    private void Start()
    {
        if (GameManager.Instance != null && worldLabel != null) {
            worldLabel.SetText($"MUNDO {GameManager.Instance.world}-{GameManager.Instance.stage}");
        }

        buttons.alpha = 0f;
        buttons.interactable = false;
        cursor.gameObject.SetActive(false);

        StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        audioSource.PlayOneShot(gameOverSound != null ? gameOverSound : ChiptuneSynth.GameOverJingle);

        // drop the title from above the screen with a bounce
        Vector2 target = title.anchoredPosition;
        Vector2 start = target + Vector2.up * 800f;
        float elapsed = 0f;

        while (elapsed < titleDropDuration)
        {
            title.anchoredPosition = Vector2.LerpUnclamped(start, target, BounceOut(elapsed / titleDropDuration));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        title.anchoredPosition = target;

        yield return new WaitForSecondsRealtime(buttonsDelay);
        yield return Fade(buttons, 1f, 0.3f);

        buttons.interactable = true;
        cursor.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }

    private void Update()
    {
        if (!buttons.interactable || EventSystem.current == null) {
            return;
        }

        // keep a button selected so the keyboard always works
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null) {
            EventSystem.current.SetSelectedGameObject(lastSelected != null ? lastSelected : retryButton.gameObject);
            return;
        }

        lastSelected = selected;

        // place the cursor to the left of the selected button
        RectTransform rect = (RectTransform)selected.transform;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Vector3 left = (corners[0] + corners[1]) / 2f;
        float bob = Mathf.Sin(Time.unscaledTime * 8f) * 6f * cursor.lossyScale.x;
        cursor.position = left + Vector3.left * (cursor.rect.width * cursor.lossyScale.x + bob);
    }

    public void Retry()
    {
        if (leaving) {
            return;
        }

        StartCoroutine(Leave());
    }

    private IEnumerator Leave()
    {
        leaving = true;
        buttons.interactable = false;

        yield return Fade(content, 0f, 0.4f);

        if (GameManager.Instance != null) {
            GameManager.Instance.NewGame();
        } else {
            SceneManager.LoadScene(firstLevelScene);
        }
    }

    public void Quit()
    {
        if (leaving) {
            return;
        }

        leaving = true;

#if UNITY_EDITOR
        Debug.Log("[GameOver] SALIR: en el ejecutable de Windows se llama a Application.Quit() y el juego se cierra. En el Editor se detiene el modo Play.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static IEnumerator Fade(CanvasGroup group, float target, float duration)
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

    private static float BounceOut(float t)
    {
        if (t < 1f / 2.75f) {
            return 7.5625f * t * t;
        } else if (t < 2f / 2.75f) {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        } else if (t < 2.5f / 2.75f) {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        } else {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }

}
