using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public const int StartingLives = 3;
    public const string GameOverScene = "GameOver";
    public const string VictoryScene = "Victory";

    public static GameManager Instance { get; private set; }

    public int world { get; private set; }
    public int stage { get; private set; }
    public int lives { get; private set; }
    public int coins { get; private set; }
    public bool resetting { get; private set; }

    // optional, a synthesized jingle is used when left empty
    public AudioClip deathSound;
    public float fadeDuration = 0.5f;

    private AudioSource audioSource;
    private ScreenFader fader;
    private Coroutine resetRoutine;

    // checkpoint of the current level, kept while the level is reloaded
    // after a death and cleared when another level or a new game starts
    private bool hasCheckpoint;
    private Vector3 checkpoint;
    private string checkpointLevel;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            audioSource = GetComponent<AudioSource>();

            if (audioSource == null) {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            fader = ScreenFader.Create(transform);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        // start the game in the level that is already open instead of
        // loading it a second time (also allows testing other levels)
        if (TryParseLevel(SceneManager.GetActiveScene().name, out int world, out int stage))
        {
            ResetGameState();
            this.world = world;
            this.stage = stage;
        }
        else
        {
            NewGame();
        }
    }

    public void NewGame()
    {
        ResetGameState();

        LoadLevel(1, 1);
    }

    private void ResetGameState()
    {
        if (resetRoutine != null) {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        resetting = false;
        hasCheckpoint = false;
        lives = StartingLives;
        coins = 0;

        Debug.Log($"[GameManager] Nueva partida: {lives} vidas");
    }

    public void GameOver()
    {
        Debug.Log("[GameManager] Game Over");

        if (Application.CanStreamedLevelBeLoaded(GameOverScene)) {
            SceneManager.LoadScene(GameOverScene);
        } else {
            Debug.LogError($"[GameManager] La escena '{GameOverScene}' no está en Build Profiles. Ejecuta Tools > Super Mario > Crear escena Game Over.");
            NewGame();
        }
    }

    public void LoadLevel(int world, int stage)
    {
        // levels that are not built yet (or not in Build Profiles) return
        // to 1-1 keeping lives and coins instead of failing to load
        if (!Application.CanStreamedLevelBeLoaded($"{world}-{stage}"))
        {
            Debug.LogWarning($"[GameManager] El nivel {world}-{stage} aún no existe o no está en Build Profiles. Se vuelve a 1-1 conservando vidas y monedas.");
            world = 1;
            stage = 1;
        }

        this.world = world;
        this.stage = stage;

        if (checkpointLevel != $"{world}-{stage}") {
            hasCheckpoint = false;
        }

        SceneManager.LoadScene($"{world}-{stage}");
    }

    public void SetCheckpoint(Vector3 position)
    {
        hasCheckpoint = true;
        checkpoint = position;
        checkpointLevel = SceneManager.GetActiveScene().name;
    }

    public bool TryGetCheckpoint(out Vector3 position)
    {
        position = checkpoint;
        return hasCheckpoint && checkpointLevel == SceneManager.GetActiveScene().name;
    }

    public void Victory()
    {
        Debug.Log($"[GameManager] ¡Victoria! Monedas: {coins}, vidas: {lives}");
        hasCheckpoint = false;

        if (Application.CanStreamedLevelBeLoaded(VictoryScene)) {
            SceneManager.LoadScene(VictoryScene);
        } else {
            Debug.LogError($"[GameManager] La escena '{VictoryScene}' no está en Build Profiles. Ejecuta Tools > Super Mario > Crear Castillo de Bowser 8-4.");
            NewGame();
        }
    }

    public void NextLevel()
    {
        LoadLevel(world, stage + 1);
    }

    public void ResetLevel(float delay)
    {
        // a single death can be reported more than once (enemy, pit,
        // shell...), only the first report costs a life
        if (resetting) {
            return;
        }

        resetting = true;

        audioSource.PlayOneShot(deathSound != null ? deathSound : ChiptuneSynth.DeathJingle);
        resetRoutine = StartCoroutine(ResetLevelRoutine(delay));
    }

    private IEnumerator ResetLevelRoutine(float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay - fadeDuration));
        yield return fader.FadeTo(1f, fadeDuration);

        resetRoutine = null;
        ResetLevel();
    }

    public void ResetLevel()
    {
        lives--;

        Debug.Log($"[GameManager] Muerte: quedan {lives} vidas");

        if (lives > 0) {
            LoadLevel(world, stage);
        } else {
            GameOver();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        resetting = false;

        if (fader.alpha > 0f) {
            StartCoroutine(fader.FadeTo(0f, fadeDuration));
        }
    }

    private static bool TryParseLevel(string sceneName, out int world, out int stage)
    {
        world = 0;
        stage = 0;

        string[] parts = sceneName.Split('-');
        return parts.Length == 2 && int.TryParse(parts[0], out world) && int.TryParse(parts[1], out stage);
    }

    public void AddCoin()
    {
        coins++;

        if (coins == 100)
        {
            coins = 0;
            AddLife();
        }
    }

    public void AddLife()
    {
        lives++;
    }

}
