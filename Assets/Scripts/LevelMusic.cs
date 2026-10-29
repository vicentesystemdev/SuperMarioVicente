using UnityEngine;

// Loops the level background music and stops it when Mario dies or
// reaches the flag pole.
[RequireComponent(typeof(AudioSource))]
public class LevelMusic : MonoBehaviour
{
    public enum Theme { Underground, Water }

    // optional, the synthesized theme is used when left empty
    public AudioClip music;
    public Theme theme = Theme.Underground;
    public float volume = 0.6f;

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();

        if (music != null) {
            source.clip = music;
        } else {
            source.clip = theme == Theme.Water ? ChiptuneSynth.WaterTheme : ChiptuneSynth.UndergroundTheme;
        }

        source.loop = true;
        source.volume = volume;
        source.Play();
    }

    private void Update()
    {
        if (source.isPlaying && GameManager.Instance != null && GameManager.Instance.resetting) {
            source.Stop();
        }
    }

    public void Stop()
    {
        if (source != null) {
            source.Stop();
        }
    }

}
