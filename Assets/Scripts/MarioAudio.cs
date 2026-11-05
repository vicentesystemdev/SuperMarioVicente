using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Audio imported from Mario Victor, connected to the existing sound cues.
public static class MarioAudio
{
    private const string ResourceFolder = "MarioAudio/";
    private static readonly Dictionary<string, string> Cues = new Dictionary<string, string>
    {
        ["Death Jingle"] = "mario-bros-game-over-1",
        ["Game Over Jingle"] = "mario-bros-game-over-1",
        ["Underground Theme"] = "mario-bros-1",
        ["Water Theme"] = "super-mario-bros",
        ["Castle Theme"] = "super-mario-bros-castillo",
        ["Boss Theme"] = "super-mario-bros-castillo",
        ["Victory Theme"] = "super-mario",
        ["Fanfare"] = "mario-bros-vida",
        ["Swim"] = "mario-bros-salto",
        ["Jump"] = "mario-bros-salto",
        ["Coin"] = "mario-moneda",
        ["Stomp"] = "mario-moneda",
        ["Pipe Warp"] = "mario-bros-tuberia",
        ["One Up"] = "mario-bros-vida",
        ["Flag"] = "mario-bros-vida-1",
        ["Boss Hurt"] = "mario-bros-vida-1",
        ["Boss Defeat"] = "mario-bros-vida-1",
    };

    public static AudioClip Load(string clipName)
    {
        return Resources.Load<AudioClip>(ResourceFolder + clipName);
    }

    public static AudioClip LoadCue(string cue)
    {
        return Cues.TryGetValue(cue, out string clipName) ? Load(clipName) : null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // These scenes have no music component. Other levels already own one.
        if ((scene.name != "MainMenu" && scene.name != "1-1") ||
            Object.FindAnyObjectByType<LevelMusic>() != null) {
            return;
        }

        GameObject musicObject = new GameObject("Mario Music", typeof(AudioSource));
        SceneManager.MoveGameObjectToScene(musicObject, scene);
        musicObject.GetComponent<AudioSource>().playOnAwake = false;
        LevelMusic music = musicObject.AddComponent<LevelMusic>();
        music.theme = LevelMusic.Theme.Overworld;
        music.volume = 0.42f;
    }
}
