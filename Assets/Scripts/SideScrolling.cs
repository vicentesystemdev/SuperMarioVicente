using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SideScrolling : MonoBehaviour
{
    private new Camera camera;
    private Transform player;

    public float height = 6.5f;
    public float undergroundHeight = -9.5f;
    public float undergroundThreshold = 0f;

    // optional right limit so the camera stops at the end of the level
    public bool limitRight = false;
    public float rightLimit = 0f;

    // optional background color swap when entering / leaving underground
    public bool changeBackground = false;
    public Color overworldBackground = new Color(0.36f, 0.58f, 0.99f);
    public Color undergroundBackground = Color.black;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        player = GameObject.FindWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        // track the player moving to the right
        Vector3 cameraPosition = transform.position;
        cameraPosition.x = Mathf.Max(cameraPosition.x, player.position.x);

        if (limitRight) {
            cameraPosition.x = Mathf.Min(cameraPosition.x, rightLimit);
        }

        transform.position = cameraPosition;
    }

    public void SetUnderground(bool underground)
    {
        // set underground height offset
        Vector3 cameraPosition = transform.position;
        cameraPosition.y = underground ? undergroundHeight : height;
        transform.position = cameraPosition;

        if (changeBackground) {
            camera.backgroundColor = underground ? undergroundBackground : overworldBackground;
        }
    }

}
