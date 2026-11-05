using System.Collections;
using UnityEngine;

public class FlagPole : MonoBehaviour
{
    public Transform flag;
    public Transform poleBottom;
    public Transform castle;
    public float speed = 6f;
    public int nextWorld = 1;
    public int nextStage = 1;
    // pause after the fanfare ends before the next level is loaded
    public float exitDelay = 0.4f;

    // the trigger can fire again while Mario slides down the pole, the
    // sequence (sounds, time bonus, level load) only runs once
    private bool reached;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || !other.CompareTag("Player")) {
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player == null || player.dead) {
            return;
        }

        reached = true;

        // stops the clock and adds the time bonus (only once)
        GameManager.Instance.LevelComplete();

        LevelMusic music = FindAnyObjectByType<LevelMusic>();

        if (music != null) {
            music.Stop();
        }

        StartCoroutine(LevelCompleteSequence(other.transform));
    }

    private IEnumerator LevelCompleteSequence(Transform player)
    {
        player.GetComponent<PlayerMovement>().enabled = false;

        // flag and Mario slide down together: grab blip, then slide sound
        Coroutine flagDown = StartCoroutine(MoveTo(flag, poleBottom.position));
        Coroutine slideDown = StartCoroutine(MoveTo(player, poleBottom.position));

        Sfx.Play(ChiptuneSynth.FlagTouch);
        yield return new WaitForSeconds(0.08f);
        Sfx.Play(ChiptuneSynth.Flag);

        yield return slideDown;
        yield return flagDown;

        // the fanfare plays while Mario jumps off and walks to the castle
        AudioClip fanfare = ChiptuneSynth.LevelClear;
        Sfx.Play(fanfare);
        float fanfareEnd = Time.time + fanfare.length;

        yield return MoveTo(player, player.position + Vector3.right);
        yield return MoveTo(player, player.position + Vector3.right + Vector3.down);
        yield return MoveTo(player, castle.position);

        player.gameObject.SetActive(false);

        // leave once the fanfare is over, never earlier and without a
        // fixed wait on top of it
        yield return new WaitForSeconds(Mathf.Max(0f, fanfareEnd - Time.time) + exitDelay);

        // a death reported during the sequence already reloads the level
        if (!GameManager.Instance.resetting) {
            GameManager.Instance.LoadLevel(nextWorld, nextStage);
        }
    }

    private IEnumerator MoveTo(Transform subject, Vector3 position)
    {
        while (Vector3.Distance(subject.position, position) > 0.125f)
        {
            subject.position = Vector3.MoveTowards(subject.position, position, speed * Time.deltaTime);
            yield return null;
        }

        subject.position = position;
    }

}
