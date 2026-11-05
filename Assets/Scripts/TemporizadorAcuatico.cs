using UnityEngine;
using UnityEngine.SceneManagement;

public class TemporizadorAcuatico : MonoBehaviour
{
    public static TemporizadorAcuatico Instance { get; private set; }

    public int tiempoInicial = 300;
    public int tiempoRestante { get; private set; }

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
        tiempoRestante = tiempoInicial;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
