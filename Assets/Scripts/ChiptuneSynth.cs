using UnityEngine;

// Generates short 8-bit style jingles at runtime (square + triangle waves).
// The melodies are original compositions for this project, so no external
// or copyrighted audio files are required. Any AudioClip assigned in the
// Inspector takes priority over these fallbacks.
public static class ChiptuneSynth
{
    private const int SampleRate = 44100;
    private const int Rest = -1;

    private enum Wave { Square, Triangle }

    private static AudioClip deathJingle;
    private static AudioClip gameOverJingle;

    public static AudioClip DeathJingle
    {
        get
        {
            if (deathJingle == null)
            {
                deathJingle = Render("Death Jingle (synth)",
                    Voice(Wave.Square, 0.22f,
                        (76, 0.07f), (84, 0.07f), (Rest, 0.12f),
                        (79, 0.11f), (78, 0.11f), (77, 0.11f), (76, 0.11f),
                        (Rest, 0.06f), (72, 0.16f), (67, 0.16f), (60, 0.45f)));
            }

            return deathJingle;
        }
    }

    public static AudioClip GameOverJingle
    {
        get
        {
            if (gameOverJingle == null)
            {
                gameOverJingle = Render("Game Over Jingle (synth)",
                    Voice(Wave.Square, 0.18f,
                        (69, 0.25f), (72, 0.25f), (71, 0.25f), (67, 0.25f),
                        (69, 0.35f), (64, 0.35f),
                        (65, 0.25f), (64, 0.25f), (62, 0.25f), (59, 0.25f),
                        (57, 0.9f)),
                    Voice(Wave.Triangle, 0.3f,
                        (45, 1f), (41, 0.7f), (43, 1f), (45, 0.9f)));
            }

            return gameOverJingle;
        }
    }

    private static float[] Voice(Wave wave, float volume, params (int note, float duration)[] notes)
    {
        float length = 0f;

        foreach (var n in notes) {
            length += n.duration;
        }

        float[] samples = new float[Mathf.CeilToInt(length * SampleRate)];
        int offset = 0;

        foreach (var n in notes)
        {
            int count = Mathf.RoundToInt(n.duration * SampleRate);

            if (n.note != Rest)
            {
                float frequency = 440f * Mathf.Pow(2f, (n.note - 69) / 12f);

                for (int i = 0; i < count && offset + i < samples.Length; i++)
                {
                    float phase = (i * frequency / SampleRate) % 1f;
                    float value = wave == Wave.Square
                        ? (phase < 0.25f ? 1f : -1f)
                        : 1f - 4f * Mathf.Abs(phase - 0.5f);

                    // short attack and release to avoid clicks
                    float t = i / (float)SampleRate;
                    float remaining = (count - i) / (float)SampleRate;
                    float envelope = Mathf.Min(1f, t / 0.005f, remaining / 0.02f) * Mathf.Lerp(1f, 0.6f, i / (float)count);

                    samples[offset + i] = value * volume * envelope;
                }
            }

            offset += count;
        }

        return samples;
    }

    private static AudioClip Render(string name, params float[][] voices)
    {
        int length = 0;

        foreach (float[] voice in voices) {
            length = Mathf.Max(length, voice.Length);
        }

        float[] mix = new float[length];

        foreach (float[] voice in voices)
        {
            for (int i = 0; i < voice.Length; i++) {
                mix[i] = Mathf.Clamp(mix[i] + voice[i], -1f, 1f);
            }
        }

        AudioClip clip = AudioClip.Create(name, length, 1, SampleRate, false);
        clip.SetData(mix, 0);
        return clip;
    }

}
