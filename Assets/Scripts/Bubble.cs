using UnityEngine;

// Air bubble that wobbles upwards and pops at the water surface.
public class Bubble : MonoBehaviour
{
    public float speed = 2f;
    public float surface;

    private float seed;

    public static void Spawn(Sprite sprite, Vector3 position, float surface)
    {
        if (sprite == null) {
            return;
        }

        GameObject bubble = new GameObject("Bubble");
        bubble.transform.position = position;

        SpriteRenderer spriteRenderer = bubble.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 4;

        Bubble component = bubble.AddComponent<Bubble>();
        component.surface = surface;
        component.seed = Random.value * 10f;

        Destroy(bubble, 5f);
    }

    private void Update()
    {
        Vector3 position = transform.position;
        position.y += speed * Time.deltaTime;
        position.x += Mathf.Sin((Time.time + seed) * 6f) * 0.5f * Time.deltaTime;
        transform.position = position;

        if (position.y >= surface) {
            Destroy(gameObject);
        }
    }

}
