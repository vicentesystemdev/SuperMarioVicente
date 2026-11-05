using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Main menu input shares the project's EventSystem and pixel UI.
public class MainMenuScreen : MonoBehaviour
{
    public CanvasGroup content;
    public GameObject mainPanel;
    public GameObject controlsPanel;
    public Button playButton;
    public Button controlsButton;
    public Button backButton;
    public PixelText playLabel;

    private GameObject lastSelected;
    private bool leaving;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        controlsPanel.SetActive(false);
        mainPanel.SetActive(true);
        Select(playButton);
    }

    private void Update()
    {
        if (leaving || EventSystem.current == null) {
            return;
        }

        if (controlsPanel.activeSelf && Input.GetButtonDown("Cancel")) {
            HideControls();
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.activeInHierarchy) {
            EventSystem.current.SetSelectedGameObject(lastSelected != null && lastSelected.activeInHierarchy
                ? lastSelected : (controlsPanel.activeSelf ? backButton.gameObject : playButton.gameObject));
        } else {
            lastSelected = selected;
        }
    }

    public void Play()
    {
        if (leaving) {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded("1-1")) {
            Debug.LogError("[MainMenu] Registra el nivel 1-1 en Build Profiles antes de jugar.");
            return;
        }

        leaving = true;
        content.interactable = false;
        playLabel.SetText("CARGANDO");
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        float elapsed = 0f;
        const float duration = 0.25f;
        while (elapsed < duration)
        {
            content.alpha = 1f - elapsed / duration;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.NewGame();
        } else {
            SceneManager.LoadScene("1-1");
        }
    }

    public void ShowControls()
    {
        if (leaving) {
            return;
        }

        mainPanel.SetActive(false);
        controlsPanel.SetActive(true);
        Select(backButton);
    }

    public void HideControls()
    {
        if (leaving) {
            return;
        }

        controlsPanel.SetActive(false);
        mainPanel.SetActive(true);
        Select(controlsButton);
    }

    private void Select(Button button)
    {
        lastSelected = button.gameObject;
        if (EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }

    public void Quit()
    {
        if (leaving) {
            return;
        }

        leaving = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
