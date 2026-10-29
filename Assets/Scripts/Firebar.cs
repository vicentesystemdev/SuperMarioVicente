using UnityEngine;

// Chain of fireballs rotating around a block. The fireballs are children
// with a Hazard trigger each.
[RequireComponent(typeof(Rigidbody2D))]
public class Firebar : MonoBehaviour
{
    // degrees per second, negative turns clockwise
    public float speed = 90f;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
    }

    private void FixedUpdate()
    {
        body.MoveRotation(body.rotation + speed * Time.fixedDeltaTime);
    }

}
