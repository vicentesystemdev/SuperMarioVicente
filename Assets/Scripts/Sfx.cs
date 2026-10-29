using UnityEngine;

// Plays one-shot sound effects through a single persistent AudioSource,
// so sounds keep playing even if the object that triggered them is destroyed.
public static class Sfx
{
    private static AudioSource source;

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null) {
            return;
        }

        if (source == null)
        {
            GameObject gameObject = new GameObject("Sfx");
            Object.DontDestroyOnLoad(gameObject);

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        source.PlayOneShot(clip, volume);
    }

}
