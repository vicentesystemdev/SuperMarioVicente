using UnityEngine;
using UnityEngine.SceneManagement;

public class SalidaAcuaticaCorta : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void PrepararSalida()
    {
        if (SceneManager.GetActiveScene().name != "2-2")
            return;

        if (GameObject.Find("Salida Acuatica Corta") != null)
            return;

        GameObject salida = new GameObject("Salida Acuatica Corta");
        salida.transform.position = new Vector3(278f, -9f, 0f);

        BoxCollider2D collider = salida.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(3f, 14f);

        salida.AddComponent<SalidaAcuaticaCorta>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerMovement movement = other.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.SetSwimming(false);

        GameObject surfacePipe = GameObject.Find("Surface Pipe");
        Vector3 destino = surfacePipe != null
            ? surfacePipe.transform.position + new Vector3(3.5f, -0.5f, 0f)
            : new Vector3(454f, 2.5f, 0f);

        other.transform.position = destino;

        SideScrolling camera = Camera.main != null
            ? Camera.main.GetComponent<SideScrolling>()
            : null;

        if (camera != null)
            camera.SetUnderground(false);
    }
}
