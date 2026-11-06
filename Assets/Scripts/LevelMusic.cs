using UnityEngine;

// Loops the level background music and stops it when Mario dies or
// reaches the flag pole.
[RequireComponent(typeof(AudioSource))]
public class LevelMusic : MonoBehaviour
{
    public enum Theme { Underground, Water, DeepWater, Castle, Boss, Victory, Overworld }

    // optional, the synthesized theme is used when left empty
    public AudioClip music;
    public Theme theme = Theme.Underground;
    public float volume = 0.6f;

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.volume = volume;

        if (music == null && source.clip != null) {
            music = source.clip;
        }

        if (music != null)
        {
            PlayCustomClip();
        }
        else
        {
            Play(theme);
        }
    }

    private void Update()
    {
        if (source.isPlaying && GameManager.Instance != null && GameManager.Instance.resetting) {
            source.Stop();
        }
    }

    // switches to another synthesized theme (e.g. when the boss fight starts),
    // unless a custom AudioClip is assigned for the whole scene
    public void Play(Theme theme)
    {
        if (source == null) {
            return;
        }

        this.theme = theme;

        if (music != null)
        {
            if (source.clip != music || !source.isPlaying) {
                PlayCustomClip();
            }
            return;
        }

        source.clip = Clip(theme);
        source.Play();
    }

    private void PlayCustomClip()
    {
        if (music.loadState == AudioDataLoadState.Unloaded) {
            music.LoadAudioData();
        }

        source.clip = music;
        source.Play();
    }

    public void Stop()
    {
        if (source != null) {
            source.Stop();
        }
    }

    private static AudioClip Clip(Theme theme)
    {
        switch (theme)
        {
            case Theme.Water: return ChiptuneSynth.WaterTheme;
            case Theme.DeepWater: return ChiptuneSynth.DeepWaterTheme;
            case Theme.Castle: return ChiptuneSynth.CastleTheme;
            case Theme.Boss: return ChiptuneSynth.BossTheme;
            case Theme.Victory: return ChiptuneSynth.VictoryTheme;
            case Theme.Overworld: return MarioAudio.Load("super-mario-bros");
            default: return ChiptuneSynth.UndergroundTheme;
        }
    }

}
