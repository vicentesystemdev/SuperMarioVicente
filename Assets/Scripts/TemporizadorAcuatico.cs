using UnityEngine;
using UnityEngine.SceneManagement;

public class TemporizadorAcuatico : MonoBehaviour
{
    public static TemporizadorAcuatico Instance { get; private set; }

    public int tiempoInicial = 300;
    public int tiempoRestante { get; private set; }

    private float tiempoActual;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
