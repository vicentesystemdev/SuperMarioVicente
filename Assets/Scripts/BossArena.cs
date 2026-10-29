using UnityEngine;

// Starts the boss fight when Mario walks in (closing the gate behind him and
// switching to the boss music) and opens the way to the treasure room when
// the boss is defeated.
[RequireComponent(typeof(BoxCollider2D))]
public class BossArena : MonoBehaviour
{
    public Boss boss;
    public Gate entryGate;
    public Gate exitGate;
    public LevelMusic music;

    private bool started;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (started || !other.CompareTag("Player")) {
            return;
        }

        started = true;
        entryGate.Close();

        if (music != null) {
            music.Play(LevelMusic.Theme.Boss);
        }

        boss.StartFight();
    }

    public void OnBossDefeated()
    {
        exitGate.Open();

        if (music != null) {
            music.Play(LevelMusic.Theme.Castle);
        }
    }

}
