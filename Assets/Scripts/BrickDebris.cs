using UnityEngine;

// A small piece of a broken brick that flies off and falls with gravity.
public class BrickDebris : MonoBehaviour
{
    public Vector2 velocity;
    public float gravity = -40f;

    public static void Spawn(Sprite sprite, Vector3 position, Vector2 velocity)
    {
        GameObject piece = new GameObject("Brick Debris");
        piece.transform.position = position;
        piece.transform.localScale = Vector3.one * 0.5f;

        SpriteRenderer spriteRenderer = piece.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 5;

        piece.AddComponent<BrickDebris>().velocity = velocity;

        Destroy(piece, 2f);
    }

    private void Update()
    {
        velocity.y += gravity * Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
        transform.Rotate(0f, 0f, 720f * Mathf.Sign(velocity.x) * Time.deltaTime);
    }

}
