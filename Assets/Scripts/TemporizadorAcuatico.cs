using UnityEngine;
using UnityEngine.SceneManagement;

public class TemporizadorAcuatico : MonoBehaviour
{
    public static TemporizadorAcuatico Instance { get; private set; }

    public int tiempoInicial = 300;
    public int tiempoRestante { get; private set; }

    private float tiempoActual;
    private bool tiempoAgotado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void PrepararTemporizador()
    {
        if (SceneManager.GetActiveScene().name != "2-2")
            return;

        if (FindFirstObjectByType<TemporizadorAcuatico>() != null)
            return;

        GameObject reloj = new GameObject("Temporizador Acuatico");
        reloj.AddComponent<TemporizadorAcuatico>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        tiempoActual = tiempoInicial;
        tiempoRestante = tiempoInicial;
    }

    private void Update()
    {
        if (tiempoRestante <= 0)
            return;

        tiempoActual = Mathf.Max(0f, tiempoActual - Time.deltaTime);
        tiempoRestante = Mathf.CeilToInt(tiempoActual);

        if (tiempoRestante <= 0)
            AgotarTiempo();
    }

    private void AgotarTiempo()
    {
        if (tiempoAgotado)
            return;

        tiempoAgotado = true;

        Player mario = FindFirstObjectByType<Player>();

        if (mario != null && !mario.dead)
            mario.Death();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
