using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Preferencias de audio persistentes entre todas las escenas del proyecto.</summary>
public sealed class Hito4AudioSettings : MonoBehaviour
{
    private const string MusicKey = "Hito4_Menu_Music";
    private const string SfxKey = "Hito4_Menu_Sfx";
    private const string VolumeKey = "Hito4_Menu_Volume";
    private static Hito4AudioSettings instance;
    private float nextCheck;

    public static bool MusicEnabled
    {
        get { return PlayerPrefs.GetInt(MusicKey, 1) == 1; }
        set { PlayerPrefs.SetInt(MusicKey, value ? 1 : 0); PlayerPrefs.Save(); if (instance != null) instance.ApplyNow(); }
    }
    public static bool SfxEnabled
    {
        get { return PlayerPrefs.GetInt(SfxKey, 1) == 1; }
        set { PlayerPrefs.SetInt(SfxKey, value ? 1 : 0); PlayerPrefs.Save(); if (instance != null) instance.ApplyNow(); }
    }
    public static float Volume
    {
        get { return Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, .8f)); }
        set { PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); if (instance != null) instance.ApplyNow(); }
    }
    public static void Ensure()
    {
        if (instance != null) return;
        Hito4AudioSettings found = FindFirstObjectByType<Hito4AudioSettings>();
        if (found != null) { instance = found; return; }
        new GameObject("Hito4 Audio Settings (persistente)").AddComponent<Hito4AudioSettings>();
    }
    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyNow();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { ApplyNow(); }
    private void Update()
    {
        if (Time.unscaledTime < nextCheck) return;
        nextCheck = Time.unscaledTime + .2f;
        ApplyNow();
    }
    public void ApplyNow()
    {
        // Los AudioSources con loop se tratan como ambientación; Sfx y jingles sin loop.
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null) continue;
            source.mute = (source.loop || source.GetComponent<LevelMusic>() != null) ? !MusicEnabled : !SfxEnabled;
        }
        AudioListener.volume = Volume;
    }
    private void OnDestroy()
    {
        if (instance == this) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
            AudioListener.volume = 1f;
        }
    }
}
