using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the Victory scene: title animation with color cycling, falling
// confetti, Mario celebrating, victory music, final stats and the
// NUEVA PARTIDA / SALIR buttons (persistent onClick events).
public class VictoryScreen : MonoBehaviour
{
    public CanvasGroup content;
    public RectTransform title;
    public RawImage titleImage;
    public PixelText statsLabel;
    public CanvasGroup buttons;
    public Button newGameButton;
    public Button quitButton;
    public RectTransform cursor;
    public RectTransform mario;
    public RectTransform confettiArea;
    public AudioSource audioSource;

    // optional, the synthesized victory theme is used when left empty
    public AudioClip music;
    public string firstLevelScene = "1-1";
    public float titleDropDuration = 1.2f;
    public int maxConfetti = 70;

    private readonly List<RectTransform> confetti = new List<RectTransform>();
    private readonly List<Vector2> confettiVelocity = new List<Vector2>();
    private Vector2 marioOrigin;
    private GameObject lastSelected;
    private bool leaving;

    private void Start()
    {
        if (GameManager.Instance != null && statsLabel != null) {
            statsLabel.SetText($"MONEDAS {GameManager.Instance.coins}  VIDAS {GameManager.Instance.lives}");
        }

        marioOrigin = mario.anchoredPosition;
        buttons.alpha = 0f;
        buttons.interactable = false;
        cursor.gameObject.SetActive(false);

        audioSource.clip = music != null ? music : ChiptuneSynth.VictoryTheme;
        audioSource.loop = true;
        audioSource.Play();

        StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        Vector2 target = title.anchoredPosition;
        Vector2 start = target + Vector2.up * 800f;
        float elapsed = 0f;

        while (elapsed < titleDropDuration)
        {
            float t = elapsed / titleDropDuration;
            title.anchoredPosition = Vector2.LerpUnclamped(start, target, 1f - Mathf.Pow(1f - t, 3f) + Mathf.Sin(t * Mathf.PI) * 0.08f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        title.anchoredPosition = target;

        yield return new WaitForSecondsRealtime(0.6f);
        yield return Fade(buttons, 1f, 0.3f);

        buttons.interactable = true;
        cursor.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
    }

    private void Update()
    {
        // rainbow title and Mario jumping for joy
        titleImage.color = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 0.25f, 1f), 0.45f, 1f);
        mario.anchoredPosition = marioOrigin + Vector2.up * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) * 60f;

        UpdateConfetti();
        UpdateSelection();
    }

    private void UpdateConfetti()
    {
        Rect area = confettiArea.rect;

        if (confetti.Count < maxConfetti && Random.value < 0.5f)
        {
            GameObject piece = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
            piece.transform.SetParent(confettiArea, false);

            RectTransform rect = (RectTransform)piece.transform;
            rect.sizeDelta = new Vector2(12f, 12f);
            rect.anchoredPosition = new Vector2(Random.Range(area.xMin, area.xMax), area.yMax + 20f);

            Image image = piece.GetComponent<Image>();
            image.color = Color.HSVToRGB(Random.value, 0.8f, 1f);
            image.raycastTarget = false;

            confetti.Add(rect);
            confettiVelocity.Add(new Vector2(Random.Range(-40f, 40f), Random.Range(-260f, -140f)));
        }

        for (int i = confetti.Count - 1; i >= 0; i--)
        {
            RectTransform rect = confetti[i];
            rect.anchoredPosition += confettiVelocity[i] * Time.unscaledDeltaTime;
            rect.Rotate(0f, 0f, 180f * Time.unscaledDeltaTime);

            if (rect.anchoredPosition.y < area.yMin - 20f)
            {
                Destroy(rect.gameObject);
                confetti.RemoveAt(i);
                confettiVelocity.RemoveAt(i);
            }
        }
    }

    private void UpdateSelection()
    {
        if (!buttons.interactable || EventSystem.current == null) {
            return;
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null) {
            EventSystem.current.SetSelectedGameObject(lastSelected != null ? lastSelected : newGameButton.gameObject);
            return;
        }

        lastSelected = selected;

        RectTransform rect = (RectTransform)selected.transform;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Vector3 left = (corners[0] + corners[1]) / 2f;
        float bob = Mathf.Sin(Time.unscaledTime * 8f) * 6f * cursor.lossyScale.x;
        cursor.position = left + Vector3.left * (cursor.rect.width * cursor.lossyScale.x + bob);
    }

    public void NewGame()
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
        Debug.Log("[Victory] SALIR: en el ejecutable de Windows se llama a Application.Quit() y el juego se cierra. En el Editor se detiene el modo Play.");
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

}
