using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void JugarNivelBase()
    {
        CargarEscena("1-1");
    }

    public void JugarSubterraneo()
    {
        CargarEscena("1-2");
    }

    public void JugarAcuatico()
    {
        CargarEscena("2-2");
    }

    public void JugarCastillo()
    {
        CargarEscena("8-4");
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CargarEscena(string escena)
    {
        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(escena))
        {
            SceneManager.LoadScene(escena, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError($"[MainMenu] La escena '{escena}' no esta registrada en Build Profiles.");
        }
    }
}
