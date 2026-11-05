using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CorrienteAcuatica : MonoBehaviour
{
    public Vector2 fuerza = new Vector2(2.5f, 0f);

    private void Awake()
    {
        BoxCollider2D area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerMovement movement = other.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.ApplyWaterCurrent(fuerza);
    }
}
