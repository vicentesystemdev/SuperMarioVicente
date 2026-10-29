using System;
using System.Collections.Generic;
using UnityEngine;

// Generates 8-bit style jingles, music loops and sound effects at runtime
// (square, triangle and noise waves). Every melody is an original
// composition for this project, so no external or copyrighted audio files
// are required. Any AudioClip assigned in the Inspector takes priority.
public static class ChiptuneSynth
{
    private const int SampleRate = 44100;
    private const int Rest = -1;

    private enum Wave { Square, Triangle, Noise }

    private static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    private static readonly System.Random random = new System.Random(12);

    // jingles and music

    public static AudioClip DeathJingle => Get("Death Jingle", () => Mix(
        Voice(Wave.Square, 0.22f,
            (76, 0.07f), (84, 0.07f), (Rest, 0.12f),
            (79, 0.11f), (78, 0.11f), (77, 0.11f), (76, 0.11f),
            (Rest, 0.06f), (72, 0.16f), (67, 0.16f), (60, 0.45f))));

    public static AudioClip GameOverJingle => Get("Game Over Jingle", () => Mix(
        Voice(Wave.Square, 0.18f,
            (69, 0.25f), (72, 0.25f), (71, 0.25f), (67, 0.25f),
            (69, 0.35f), (64, 0.35f),
            (65, 0.25f), (64, 0.25f), (62, 0.25f), (59, 0.25f),
            (57, 0.9f)),
        Voice(Wave.Triangle, 0.3f,
            (45, 1f), (41, 0.7f), (43, 1f), (45, 0.9f))));

    // 4 second loop: walking minor bass with a sparse echoing lead
    public static AudioClip UndergroundTheme => Get("Underground Theme", () => Mix(
        Voice(Wave.Triangle, 0.35f,
            (45, 0.25f), (45, 0.25f), (52, 0.25f), (45, 0.25f),
            (43, 0.25f), (43, 0.25f), (50, 0.25f), (43, 0.25f),
            (41, 0.25f), (41, 0.25f), (48, 0.25f), (41, 0.25f),
            (40, 0.25f), (40, 0.25f), (47, 0.25f), (44, 0.25f)),
        Voice(Wave.Square, 0.07f,
            (Rest, 0.5f), (69, 0.25f), (72, 0.25f),
            (Rest, 0.5f), (71, 0.25f), (67, 0.25f),
            (Rest, 0.5f), (65, 0.25f), (69, 0.25f),
            (Rest, 0.5f), (68, 0.5f))));

    // 6 second loop in 3/4: gentle waltz with an oom-pah-pah bass
    public static AudioClip WaterTheme => Get("Water Theme", () => Mix(
        Voice(Wave.Square, 0.07f,
            (77, 0.5f), (76, 0.25f),
            (74, 0.75f),
            (72, 0.5f), (74, 0.25f),
            (76, 0.75f),
            (77, 0.5f), (79, 0.25f),
            (81, 0.5f), (79, 0.25f),
            (77, 0.25f), (76, 0.25f), (74, 0.25f),
            (72, 0.75f)),
        Voice(Wave.Triangle, 0.3f,
            (41, 0.25f), (48, 0.25f), (48, 0.25f),
            (46, 0.25f), (53, 0.25f), (53, 0.25f),
            (36, 0.25f), (43, 0.25f), (43, 0.25f),
            (41, 0.25f), (48, 0.25f), (48, 0.25f),
            (38, 0.25f), (45, 0.25f), (45, 0.25f),
            (41, 0.25f), (48, 0.25f), (48, 0.25f),
            (36, 0.25f), (43, 0.25f), (43, 0.25f),
            (41, 0.25f), (48, 0.25f), (48, 0.25f))));

    // 3.2 second loop: ominous tritone bass walking down by semitones
    public static AudioClip CastleTheme => Get("Castle Theme", () => Mix(
        Voice(Wave.Triangle, 0.35f,
            (38, 0.2f), (38, 0.2f), (44, 0.2f), (38, 0.2f),
            (37, 0.2f), (37, 0.2f), (43, 0.2f), (37, 0.2f),
            (36, 0.2f), (36, 0.2f), (42, 0.2f), (36, 0.2f),
            (37, 0.2f), (37, 0.2f), (43, 0.2f), (37, 0.2f)),
        Voice(Wave.Square, 0.06f,
            (Rest, 0.8f), (74, 0.4f), (73, 0.4f),
            (Rest, 0.8f), (70, 0.4f), (69, 0.4f))));

    // 4 second loop: driving bass with short stabs for the boss fight
    public static AudioClip BossTheme => Get("Boss Theme", () => Mix(
        Voice(Wave.Triangle, 0.38f,
            (40, 0.125f), (40, 0.125f), (52, 0.125f), (40, 0.125f), (46, 0.125f), (40, 0.125f), (52, 0.125f), (40, 0.125f),
            (40, 0.125f), (40, 0.125f), (52, 0.125f), (40, 0.125f), (46, 0.125f), (40, 0.125f), (52, 0.125f), (40, 0.125f),
            (41, 0.125f), (41, 0.125f), (53, 0.125f), (41, 0.125f), (47, 0.125f), (41, 0.125f), (53, 0.125f), (41, 0.125f),
            (41, 0.125f), (41, 0.125f), (53, 0.125f), (41, 0.125f), (47, 0.125f), (41, 0.125f), (53, 0.125f), (41, 0.125f)),
        Voice(Wave.Square, 0.08f,
            (76, 0.25f), (Rest, 0.25f), (75, 0.25f), (Rest, 0.25f), (70, 0.5f), (Rest, 0.5f),
            (77, 0.25f), (Rest, 0.25f), (76, 0.25f), (Rest, 0.25f), (71, 0.5f), (Rest, 0.5f))));

    // 4 second bright loop for the victory screen
    public static AudioClip VictoryTheme => Get("Victory Theme", () => Mix(
        Voice(Wave.Square, 0.1f,
            (72, 0.2f), (76, 0.2f), (79, 0.2f), (84, 0.6f),
            (83, 0.2f), (79, 0.2f), (81, 0.6f), (79, 0.4f),
            (77, 0.2f), (76, 0.2f), (74, 0.2f), (72, 0.8f)),
        Voice(Wave.Triangle, 0.32f,
            (48, 1.2f), (53, 1.4f), (43, 0.6f), (48, 0.8f))));

    // short fanfare when the treasure is reached
    public static AudioClip Fanfare => Get("Fanfare", () => Mix(
        Voice(Wave.Square, 0.12f, (72, 0.12f), (76, 0.12f), (79, 0.12f), (84, 0.5f)),
        Voice(Wave.Triangle, 0.3f, (48, 0.36f), (55, 0.5f))));

    // sound effects

    public static AudioClip Fire => Get("Fire", () => Mix(Sweep(Wave.Noise, 0.18f, 90f, 60f, 0.25f)));

    public static AudioClip LavaBurst => Get("Lava Burst", () => Mix(
        Sweep(Wave.Noise, 0.14f, 70f, 100f, 0.2f),
        Sweep(Wave.Triangle, 0.25f, 40f, 52f, 0.2f)));

    public static AudioClip Crumble => Get("Crumble", () => Mix(Sweep(Wave.Noise, 0.2f, 80f, 50f, 0.35f)));

    public static AudioClip CheckpointReached => Get("Checkpoint", () => Mix(Voice(Wave.Square, 0.12f,
        (72, 0.08f), (79, 0.08f), (84, 0.2f))));

    public static AudioClip GateMove => Get("Gate", () => Mix(
        Sweep(Wave.Triangle, 0.4f, 40f, 52f, 0.8f),
        Sweep(Wave.Noise, 0.06f, 60f, 60f, 0.8f)));

    public static AudioClip BossRoar => Get("Boss Roar", () => Mix(
        Sweep(Wave.Square, 0.16f, 50f, 38f, 0.7f),
        Sweep(Wave.Noise, 0.12f, 70f, 50f, 0.7f)));

    public static AudioClip BossHurt => Get("Boss Hurt", () => Mix(Sequence(
        Sweep(Wave.Square, 0.16f, 76f, 64f, 0.08f),
        Sweep(Wave.Square, 0.16f, 76f, 64f, 0.08f))));

    public static AudioClip BossFire => Get("Boss Fire", () => Mix(
        Sweep(Wave.Noise, 0.16f, 95f, 70f, 0.3f),
        Sweep(Wave.Square, 0.06f, 60f, 48f, 0.3f)));

    public static AudioClip Slam => Get("Slam", () => Mix(
        Sweep(Wave.Noise, 0.24f, 60f, 40f, 0.4f),
        Sweep(Wave.Triangle, 0.45f, 40f, 28f, 0.4f)));

    public static AudioClip BossDefeat => Get("Boss Defeat", () => Mix(Sequence(
        Sweep(Wave.Square, 0.14f, 72f, 60f, 0.25f),
        Sweep(Wave.Square, 0.14f, 67f, 55f, 0.25f),
        Sweep(Wave.Square, 0.14f, 62f, 40f, 0.8f))));

    public static AudioClip Swim => Get("Swim", () => Mix(Sweep(Wave.Triangle, 0.35f, 58f, 66f, 0.09f)));

    public static AudioClip Jump => Get("Jump", () => Mix(Sweep(Wave.Square, 0.12f, 62f, 79f, 0.2f)));

    public static AudioClip Coin => Get("Coin", () => Mix(Voice(Wave.Square, 0.12f, (84, 0.06f), (91, 0.22f))));

    public static AudioClip Bump => Get("Bump", () => Mix(Sweep(Wave.Triangle, 0.45f, 50f, 38f, 0.1f)));

    public static AudioClip BrickBreak => Get("Brick Break", () => Mix(
        Sweep(Wave.Noise, 0.22f, 100f, 70f, 0.3f),
        Sweep(Wave.Square, 0.08f, 55f, 36f, 0.15f)));

    public static AudioClip Stomp => Get("Stomp", () => Mix(Sweep(Wave.Square, 0.14f, 74f, 58f, 0.12f)));

    public static AudioClip Kick => Get("Kick", () => Mix(Voice(Wave.Square, 0.12f, (76, 0.04f), (83, 0.07f))));

    public static AudioClip PipeWarp => Get("Pipe Warp", () => Mix(Sequence(
        Sweep(Wave.Square, 0.12f, 60f, 48f, 0.1f),
        Sweep(Wave.Square, 0.12f, 60f, 48f, 0.1f),
        Sweep(Wave.Square, 0.12f, 60f, 48f, 0.1f))));

    public static AudioClip PowerUpCollect => Get("Power Up", () => Mix(Voice(Wave.Square, 0.12f,
        (60, 0.05f), (64, 0.05f), (67, 0.05f), (72, 0.05f), (76, 0.05f), (79, 0.05f), (84, 0.12f))));

    public static AudioClip OneUp => Get("One Up", () => Mix(Voice(Wave.Square, 0.12f,
        (72, 0.07f), (76, 0.07f), (79, 0.07f), (84, 0.07f), (79, 0.07f), (84, 0.14f))));

    public static AudioClip Flag => Get("Flag", () => Mix(Sweep(Wave.Square, 0.1f, 84f, 60f, 0.9f)));

    private static AudioClip Get(string name, Func<float[]> render)
    {
        if (!cache.TryGetValue(name, out AudioClip clip) || clip == null)
        {
            float[] samples = render();
            clip = AudioClip.Create($"{name} (synth)", samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            cache[name] = clip;
        }

        return clip;
    }

    private static float Frequency(float note)
    {
        return 440f * Mathf.Pow(2f, (note - 69f) / 12f);
    }

    private static float Sample(Wave wave, float phase, ref float noise, bool wrapped)
    {
        switch (wave)
        {
            case Wave.Square:
                return phase < 0.25f ? 1f : -1f;

            case Wave.Triangle:
                return 1f - 4f * Mathf.Abs(phase - 0.5f);

            default:
                // sample and hold noise, the note sets how often it changes
                if (wrapped) {
                    noise = (float)random.NextDouble() * 2f - 1f;
                }
                return noise;
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
        float noise = 0f;

        foreach (var n in notes)
        {
            int count = Mathf.RoundToInt(n.duration * SampleRate);

            if (n.note != Rest)
            {
                float step = Frequency(n.note) / SampleRate;
                float phase = 0f;

                for (int i = 0; i < count && offset + i < samples.Length; i++)
                {
                    phase += step;
                    bool wrapped = phase >= 1f;
                    phase %= 1f;

                    // short attack and release to avoid clicks
                    float t = i / (float)SampleRate;
                    float remaining = (count - i) / (float)SampleRate;
                    float envelope = Mathf.Min(1f, t / 0.005f, remaining / 0.02f) * Mathf.Lerp(1f, 0.6f, i / (float)count);

                    samples[offset + i] = Sample(wave, phase, ref noise, wrapped) * volume * envelope;
                }
            }

            offset += count;
        }

        return samples;
    }

    private static float[] Sweep(Wave wave, float volume, float startNote, float endNote, float duration)
    {
        int count = Mathf.RoundToInt(duration * SampleRate);
        float[] samples = new float[count];
        float phase = 0f;
        float noise = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)count;

            phase += Frequency(Mathf.Lerp(startNote, endNote, t)) / SampleRate;
            bool wrapped = phase >= 1f;
            phase %= 1f;

            float envelope = Mathf.Min(1f, i / (0.002f * SampleRate)) * (1f - t);
            samples[i] = Sample(wave, phase, ref noise, wrapped) * volume * envelope;
        }

        return samples;
    }

    private static float[] Sequence(params float[][] parts)
    {
        List<float> samples = new List<float>();

        foreach (float[] part in parts) {
            samples.AddRange(part);
        }

        return samples.ToArray();
    }

    private static float[] Mix(params float[][] voices)
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

        return mix;
    }

}
