using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public const int StartingLives = 3;
    public const string MainMenuScene = "MainMenu";
    public const string GameOverScene = "GameOver";
    public const string VictoryScene = "Victory";

    public static GameManager Instance { get; private set; }

    public int world { get; private set; }
    public int stage { get; private set; }
    public int lives { get; private set; }
    public int coins { get; private set; }
    public bool resetting { get; private set; }

    // points (NES values)
    public const int CoinPoints = 200;
    public const int StompPoints = 100;
    public const int EnemyKillPoints = 200;
    public const int PowerUpPoints = 1000;
    public const int BossPoints = 5000;
    public const int TimeBonusPoints = 50;

    // level timer, a LevelSettings component in the scene overrides these
    public const int DefaultTimeLimit = 400;
    public const float DefaultSecondsPerTick = 0.5f;
    public const int HurryUpTime = 100;

    public int score { get; private set; }
    public int time { get; private set; }
    public bool timerRunning { get; private set; }
    public bool hurryUp => timerRunning && time <= HurryUpTime;

    // optional, a synthesized jingle is used when left empty
    public AudioClip deathSound;
    public float fadeDuration = 0.5f;

    // seconds the lives screen is shown after a death (0 disables it)
    public float livesScreenDuration = 2.5f;

    private AudioSource audioSource;
    private ScreenFader fader;
    private GameHud hud;
    private LivesScreen livesScreen;
    private Sprite marioSprite;
    private Coroutine resetRoutine;
    private float secondsPerTick = DefaultSecondsPerTick;
    private float tickTimer;
    private PlayerMovement playerMovement;
    private TemporizadorAcuatico aquaticTimer;

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
            hud = GameHud.Create(transform);
            livesScreen = LivesScreen.Create(transform);
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
            StartLevel(SceneManager.GetActiveScene());
        }
        else if (SceneManager.GetActiveScene().name == MainMenuScene)
        {
            ResetGameState();
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

        livesScreen.Hide();
        Time.timeScale = 1f;
        resetting = false;
        hasCheckpoint = false;
        lives = StartingLives;
        coins = 0;
        score = 0;

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
        // the castle goal has no flag pole, the time bonus is added here
        LevelComplete();

        Debug.Log($"[GameManager] ¡Victoria! Puntos: {score}, monedas: {coins}, vidas: {lives}");
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
        timerRunning = false;

        audioSource.PlayOneShot(deathSound != null ? deathSound : ChiptuneSynth.DeathJingle);
        resetRoutine = StartCoroutine(ResetLevelRoutine(delay));
    }

    private IEnumerator ResetLevelRoutine(float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay - fadeDuration));
        yield return fader.FadeTo(1f, fadeDuration);

        // ResetLevel() takes the life, so the screen shows the lives that
        // will remain; with no lives left GameOver is loaded directly.
        // resetting stays true until the level reloads, so the clock is
        // stopped and further deaths are ignored while it is shown
        int remaining = lives - 1;

        if (remaining > 0 && livesScreenDuration > 0f)
        {
            // the old level is frozen behind the screen (no enemy sounds)
            Time.timeScale = 0f;
            hud.SetVisible(false);
            livesScreen.Show(world, stage, remaining, marioSprite);

            yield return fader.FadeTo(0f, fadeDuration);
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, livesScreenDuration - fadeDuration));
            yield return fader.FadeTo(1f, fadeDuration);

            livesScreen.Hide();
            Time.timeScale = 1f;
        }

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

        StartLevel(scene);
    }

    // restarts the timer and shows the HUD in level scenes, the GameOver
    // and Victory scenes have their own stats so the HUD is hidden there
    private void StartLevel(Scene scene)
    {
        bool isLevel = TryParseLevel(scene.name, out _, out _);
        hud.SetVisible(isLevel);

        timerRunning = false;
        playerMovement = null;
        aquaticTimer = null;

        if (!isLevel) {
            return;
        }

        LevelSettings settings = FindAnyObjectByType<LevelSettings>();
        time = settings != null ? settings.timeLimit : DefaultTimeLimit;
        secondsPerTick = settings != null ? settings.secondsPerTick : DefaultSecondsPerTick;
        tickTimer = 0f;
        timerRunning = true;

        Player player = FindAnyObjectByType<Player>();

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();

            // kept for the lives screen, Mario may be inactive (pit) or
            // showing his death sprite by the time it is needed
            if (player.smallRenderer != null) {
                marioSprite = player.smallRenderer.idle;
            }
        }

        // El 2-2 conserva su reloj acuatico, pero usa el HUD general del equipo
        // para monedas, puntuacion, mundo y tiempo.
        if (scene.name == "2-2")
        {
            aquaticTimer = FindAnyObjectByType<TemporizadorAcuatico>();
            if (aquaticTimer == null) {
                aquaticTimer = new GameObject("Temporizador Acuatico").AddComponent<TemporizadorAcuatico>();
            }
            time = aquaticTimer.tiempoRestante;
        }
    }

    private void Update()
    {
        if (!timerRunning || resetting) {
            return;
        }

        if (aquaticTimer != null)
        {
            aquaticTimer.enabled = playerMovement != null && playerMovement.enabled;
            time = aquaticTimer.tiempoRestante;
            return;
        }

        // the clock stops while Mario has no control (pipes, flag pole,
        // castle goal), so the level ending can never run out of time
        if (playerMovement == null || !playerMovement.enabled) {
            return;
        }

        tickTimer += Time.deltaTime;

        while (tickTimer >= secondsPerTick && time > 0)
        {
            tickTimer -= secondsPerTick;
            time--;
        }

        if (time <= 0) {
            TimeUp();
        }
    }

    private void TimeUp()
    {
        timerRunning = false;
        Debug.Log("[GameManager] Tiempo agotado");

        Player player = playerMovement != null ? playerMovement.GetComponent<Player>() : null;

        // the normal death sequence costs the life and plays the jingle
        if (player != null && player.gameObject.activeInHierarchy) {
            player.Death();
        } else {
            ResetLevel(3f);
        }
    }

    // stops the clock and converts the remaining time into points, only
    // the first call of a level counts
    public void LevelComplete()
    {
        if (!timerRunning) {
            return;
        }

        timerRunning = false;
        if (aquaticTimer != null) {
            time = aquaticTimer.tiempoRestante;
            aquaticTimer.enabled = false;
        }
        AddScore(time * TimeBonusPoints);
        Debug.Log($"[GameManager] Nivel completado: bonus de tiempo {time} x {TimeBonusPoints}");
    }

    public void AddScore(int points)
    {
        score = Mathf.Max(0, score + points);
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
