using UnityEngine;

// Optional per level configuration read by the GameManager when the scene
// loads. Levels without this component use the defaults of
// GameManager.DefaultTimeLimit / DefaultTimeLimits.
public class LevelSettings : MonoBehaviour
{
    // time shown in the HUD when the level starts (or restarts after a death)
    [Min(1)] public int timeLimit = 400;

    // real seconds per HUD time unit (the NES counts one unit every 0.4 s)
    [Min(0.05f)] public float secondsPerTick = 0.5f;
}
