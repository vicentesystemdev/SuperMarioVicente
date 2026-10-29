using UnityEngine;

// Loops the level background music and stops it when Mario dies or
// reaches the flag pole.
[RequireComponent(typeof(AudioSource))]
public class LevelMusic : MonoBehaviour
{
    public enum Theme { Underground, Water, Castle, Boss, Victory }

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

        if (music != null)
        {
            source.clip = music;
            source.Play();
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

    // switches to another synthesized theme (e.g. when the boss fight starts)
    public void Play(Theme theme)
    {
        if (source == null) {
            return;
        }

        this.theme = theme;
        source.clip = Clip(theme);
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
            case Theme.Castle: return ChiptuneSynth.CastleTheme;
            case Theme.Boss: return ChiptuneSynth.BossTheme;
            case Theme.Victory: return ChiptuneSynth.VictoryTheme;
            default: return ChiptuneSynth.UndergroundTheme;
        }
    }

}
