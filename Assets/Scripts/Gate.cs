using System.Collections;
using UnityEngine;

// Solid portcullis that can slam shut or slide up to open.
public class Gate : MonoBehaviour
{
    public float openHeight = 6f;
    public float duration = 1f;

    public void Close()
    {
        gameObject.SetActive(true);
        Sfx.Play(ChiptuneSynth.Slam, 0.7f);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        StartCoroutine(OpenAnimation());
    }

    private IEnumerator OpenAnimation()
    {
        Sfx.Play(ChiptuneSynth.GateMove);

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * openHeight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

}
