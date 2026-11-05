using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Menú completo de la actividad de Hito 4; se conecta desde el Editor Builder.</summary>
public sealed class Hito4MenuController : MonoBehaviour
{
    public GameObject[] pantallas; // 0 principal, 1 mundos, 2 controles, 3 creditos, 4 ajustes, 5 SFX
    public Button jugar;
    public Button[] tarjetasNiveles;
    public Button[] botonesRegresar;
    public Image ledMusica;
    public Image ledSfx;
    public Image barraVolumen;
    public PixelText numeroVolumen;
    public AudioSource musicaMenu;
    public RectTransform mascota;
    public RectTransform[] monedas;
    public RectTransform contornoNivel;
    public Image descripcionNivel;
    public Sprite[] descripciones;
    private Vector2 mascaraBase;
    private Vector2[] monedasBase;
    private int ultimoNivel = -1;

    private GameObject[] ultimoSeleccionado;
    private int pagina;
    private bool cargando;

    private void Awake()
    {
        Time.timeScale = 1f;
        Hito4AudioSettings.Ensure();
        ultimoSeleccionado = new GameObject[pantallas.Length];
    }
    private void Start()
    {
        // Reuse the game's existing Mario Victor music without a second loop.
        LevelMusic background = FindAnyObjectByType<LevelMusic>();
        if (background != null) musicaMenu = background.GetComponent<AudioSource>();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (mascota != null) mascaraBase = mascota.anchoredPosition;
        monedasBase = new Vector2[monedas != null ? monedas.Length : 0];
        for (int i = 0; i < monedasBase.Length; i++) if (monedas[i] != null) monedasBase[i] = monedas[i].anchoredPosition;
        CambiarPagina(0);
        ActualizarAjustes();
    }
    private void Update()
    {
        float t = Time.unscaledTime;
        if (mascota != null) mascota.anchoredPosition = mascaraBase + Vector2.up * Mathf.Abs(Mathf.Sin(t * 2.1f)) * 17f;
        for (int i = 0; i < monedasBase.Length; i++)
            if (monedas[i] != null) monedas[i].anchoredPosition = monedasBase[i] + Vector2.up * Mathf.Sin(t * 3.5f + i * 1.3f) * 12f;
        if (pagina == 1 && EventSystem.current != null && tarjetasNiveles != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            for (int i = 0; i < tarjetasNiveles.Length; i++)
            {
                if (tarjetasNiveles[i] == null || selected != tarjetasNiveles[i].gameObject || ultimoNivel == i) continue;
                ultimoNivel = i;
                if (contornoNivel != null) contornoNivel.anchoredPosition = ((RectTransform)tarjetasNiveles[i].transform).anchoredPosition;
                if (descripcionNivel != null && descripciones != null && i < descripciones.Length) descripcionNivel.sprite = descripciones[i];
                break;
            }
        }
        if (!cargando && Input.GetKeyDown(KeyCode.Escape) && pagina != 0) {
            if (pagina == 5) VolverAjustes(); else Volver();
        }
        if (!cargando && EventSystem.current != null &&
            (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)) {
            SeleccionarPrimero();
        }
    }
    private void CambiarPagina(int index)
    {
        if (cargando || pantallas == null || index < 0 || index >= pantallas.Length) return;
        if (EventSystem.current != null) ultimoSeleccionado[pagina] = EventSystem.current.currentSelectedGameObject;
        pagina = index;
        for (int i = 0; i < pantallas.Length; i++)
            if (pantallas[i] != null) pantallas[i].SetActive(i == index);
        SeleccionarPrimero();
        if (index == 4) ActualizarAjustes();
        if (index == 1) ultimoNivel = -1;
        Feedback();
    }
    private void SeleccionarPrimero()
    {
        if (EventSystem.current == null) return;
        GameObject first = ultimoSeleccionado[pagina];
        if (first == null || !first.activeInHierarchy) {
            first = pagina == 0 ? jugar.gameObject : pagina == 1 ? tarjetasNiveles[0].gameObject : botonesRegresar[pagina].gameObject;
        }
        EventSystem.current.SetSelectedGameObject(first);
    }
    public void MostrarNiveles() { CambiarPagina(1); }
    public void MostrarControles() { CambiarPagina(2); }
    public void MostrarCreditos() { CambiarPagina(3); }
    public void MostrarAjustes() { CambiarPagina(4); }
    public void MostrarSfx() { CambiarPagina(5); }
    public void Volver() { CambiarPagina(0); }
    public void VolverAjustes() { CambiarPagina(4); }
    public void Iniciar() { IniciarEscena("1-1"); }
    public void Nivel11() { IniciarEscena("1-1"); }
    public void Nivel12() { IniciarEscena("1-2"); }
    public void Nivel22() { IniciarEscena("2-2"); }
    public void Nivel84() { IniciarEscena("8-4"); }
    private void IniciarEscena(string nombre)
    {
        if (cargando) return;
        if (!Application.CanStreamedLevelBeLoaded(nombre))
        {
            Debug.LogError("[Hito4] Nivel " + nombre + " no está habilitado en File > Build Profiles > Scene List.");
            return;
        }
        StartCoroutine(Cargar(nombre));
    }
    private IEnumerator Cargar(string nombre)
    {
        cargando = true;
        Time.timeScale = 1f;
        if (musicaMenu != null) musicaMenu.Stop();
        Feedback();
        yield return new WaitForSecondsRealtime(.18f);
        string[] partes = nombre.Split('-');
        if (GameManager.Instance != null) {
            GameManager.Instance.NewGame(int.Parse(partes[0]), int.Parse(partes[1]));
        } else {
            SceneManager.LoadScene(nombre);
        }
    }

    public void AlternarMusica()
    {
        Hito4AudioSettings.MusicEnabled = !Hito4AudioSettings.MusicEnabled;
        ActualizarAjustes();
        Feedback();
    }
    public void AlternarSfx()
    {
        Hito4AudioSettings.SfxEnabled = !Hito4AudioSettings.SfxEnabled;
        ActualizarAjustes();
        Feedback();
    }
    public void SubirVolumen() { Hito4AudioSettings.Volume += .1f; ActualizarAjustes(); Feedback(); }
    public void BajarVolumen() { Hito4AudioSettings.Volume -= .1f; ActualizarAjustes(); Feedback(); }
    public void AjustarMusicaRapida() { AlternarMusica(); }
    public void AjustarSfxRapido() { AlternarSfx(); }
    private void ActualizarAjustes()
    {
        if (ledMusica != null) ledMusica.color = Hito4AudioSettings.MusicEnabled ? new Color(.17f,.91f,.4f) : new Color(.75f,.25f,.27f);
        if (ledSfx != null) ledSfx.color = Hito4AudioSettings.SfxEnabled ? new Color(.17f,.91f,.4f) : new Color(.75f,.25f,.27f);
        if (barraVolumen != null) barraVolumen.fillAmount = Hito4AudioSettings.Volume;
        if (numeroVolumen != null) numeroVolumen.SetText(Mathf.RoundToInt(Hito4AudioSettings.Volume * 100f).ToString());
        if (musicaMenu != null) musicaMenu.mute = !Hito4AudioSettings.MusicEnabled;
    }
    private void Feedback()
    {
        if (Hito4AudioSettings.SfxEnabled) Sfx.Play(ChiptuneSynth.Coin, .28f);
    }
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
