using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Installs only the supplied pause menu; existing HUDs and timers stay in charge.
public sealed class Hito4PauseController : MonoBehaviour
{
    public GameObject ventanaPausa;
    public Button continuar;
    public bool Pausado { get; private set; }
    private GameObject previousSelection;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "1-1" && scene.name != "1-2" && scene.name != "2-2" && scene.name != "8-4") return;
        if (FindAnyObjectByType<Hito4PauseController>() != null) return;
        GameObject prefab = Resources.Load<GameObject>("Hito4PauseMenu");
        if (prefab == null) {
            Debug.LogError("[Hito4] Genera el menu y el prefab de pausa desde Tools > Super Mario > Hito 4.");
            return;
        }
        GameObject pause = Instantiate(prefab);
        SceneManager.MoveGameObjectToScene(pause, scene);
        if (FindAnyObjectByType<EventSystem>() == null) {
            GameObject events = new GameObject("Pause EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(events, scene);
        }
        Hito4AudioSettings.Ensure();
    }

    private void Start() { ventanaPausa.SetActive(false); }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) {
            if (Pausado) Reanudar(); else Pausar();
        }
        if (Pausado && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null) {
            EventSystem.current.SetSelectedGameObject(continuar.gameObject);
        }
    }

    public void Pausar()
    {
        if (Pausado || GameManager.Instance == null || GameManager.Instance.resetting) return;
        Player player = FindAnyObjectByType<Player>();
        if (player == null || player.dead || !player.GetComponent<PlayerMovement>().enabled) return;
        previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        Pausado = true;
        ventanaPausa.SetActive(true);
        Time.timeScale = 0f;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(continuar.gameObject);
    }

    public void Reanudar()
    {
        if (!Pausado) return;
        Pausado = false;
        Time.timeScale = 1f;
        ventanaPausa.SetActive(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(previousSelection);
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        Pausado = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MenuPrincipal()
    {
        Time.timeScale = 1f;
        Pausado = false;
        SceneManager.LoadScene(GameManager.MainMenuScene);
    }

    private void OnDestroy()
    {
        if (Pausado) Time.timeScale = 1f;
    }
}
