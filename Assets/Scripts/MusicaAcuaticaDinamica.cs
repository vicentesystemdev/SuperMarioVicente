using UnityEngine;

public class MusicaAcuaticaDinamica : MonoBehaviour
{
    public float cambioX = 58f;

    private Transform player;
    private LevelMusic music;
    private bool changed;

    private void Start()
    {
        GameObject mario = GameObject.FindWithTag("Player");
        if (mario != null)
            player = mario.transform;

        music = GetComponent<LevelMusic>();
    }

    private void Update()
    {
        if (changed || player == null || music == null)
            return;

        if (player.position.x >= cambioX)
        {
            changed = true;
            music.Play(LevelMusic.Theme.DeepWater);
        }
    }
}
