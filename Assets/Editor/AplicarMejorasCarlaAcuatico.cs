using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AplicarMejorasCarlaAcuatico
{
    private const string ScenePath = "Assets/Scenes/2-2.unity";
    private const string MarkerName = "MEJORAS_CARLA_ACUATICO_V3";

    static AplicarMejorasCarlaAcuatico()
    {
        EditorApplication.delayCall += AplicarSiHaceFalta;
    }

    [MenuItem("Tools/Super Mario/Aplicar mejoras Carla al nivel 2-2")]
    [MenuItem("Tools/Carla/Aplicar mejoras nivel acuatico 2-2")]
    public static void AplicarManual()
    {
        Aplicar(true);
    }

    private static void AplicarSiHaceFalta()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Aplicar(false);
    }

    private static void Aplicar(bool force)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene original = SceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool alreadyOpen = scene.IsValid() && scene.isLoaded;

        if (!alreadyOpen)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        SceneManager.SetActiveScene(scene);

        if (!force && FindRoot(scene, MarkerName) != null)
        {
            Restaurar(scene, original, alreadyOpen);
            return;
        }

        GameObject oldMarker = FindRoot(scene, MarkerName);
        if (oldMarker != null)
            Object.DestroyImmediate(oldMarker);

        GameObject oldMarkerV1 = FindRoot(scene, "MEJORAS_CARLA_ACUATICO_V1");
        if (oldMarkerV1 != null)
            Object.DestroyImmediate(oldMarkerV1);

        GameObject oldMarkerV2 = FindRoot(scene, "MEJORAS_CARLA_ACUATICO_V2");
        if (oldMarkerV2 != null)
            Object.DestroyImmediate(oldMarkerV2);

        GameObject marker = new GameObject(MarkerName);
        SceneManager.MoveGameObjectToScene(marker, scene);

        QuitarSistemasDuplicados(scene);
        PrepararConfiguracionNivel(scene);
        PrepararSalida(scene);
        PrepararCamara(scene);
        PrepararAgua(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[Carla] Mejoras acuaticas aplicadas y guardadas en Assets/Scenes/2-2.unity.");

        Restaurar(scene, original, alreadyOpen);
    }

    private static void QuitarSistemasDuplicados(Scene scene)
    {
        GameObject hudAnterior = FindRoot(scene, "HUD Acuatico");
        if (hudAnterior != null)
            Object.DestroyImmediate(hudAnterior);

        GameObject relojAnterior = FindRoot(scene, "Temporizador Acuatico");
        if (relojAnterior != null)
            Object.DestroyImmediate(relojAnterior);
    }

    private static void PrepararConfiguracionNivel(Scene scene)
    {
        GameObject settingsObject = FindRoot(scene, "Level Settings");

        if (settingsObject == null)
        {
            settingsObject = new GameObject("Level Settings");
            SceneManager.MoveGameObjectToScene(settingsObject, scene);
        }

        LevelSettings settings = settingsObject.GetComponent<LevelSettings>();
        if (settings == null)
            settings = settingsObject.AddComponent<LevelSettings>();

        settings.timeLimit = 120;
        settings.secondsPerTick = 0.5f;
    }

    private static void PrepararSalida(Scene scene)
    {
        GameObject salida = FindRoot(scene, "Salida Acuatica Corta");

        if (salida == null)
        {
            salida = new GameObject("Salida Acuatica Corta");
            SceneManager.MoveGameObjectToScene(salida, scene);
            salida.AddComponent<BoxCollider2D>();
            salida.AddComponent<SalidaAcuaticaCorta>();
        }

        salida.transform.position = new Vector3(166f, -9f, 0f);

        BoxCollider2D collider = salida.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.size = new Vector2(3f, 14f);
        }
    }

    private static void PrepararCamara(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Camera camera = root.GetComponentInChildren<Camera>(true);

            if (camera == null)
                continue;

            SideScrolling scrolling = camera.GetComponent<SideScrolling>();

            if (scrolling != null)
            {
                scrolling.smoothTime = 0.12f;
                scrolling.lookAhead = 2f;
            }

            break;
        }
    }

    private static void PrepararAgua(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            WaterZone[] zones = root.GetComponentsInChildren<WaterZone>(true);

            foreach (WaterZone zone in zones)
            {
                zone.bubbleInterval = 0.9f;
                zone.ambientBubbleInterval = 1.8f;
                zone.logEvents = false;
            }
        }
    }

    private static void CrearTexto(string name, Transform parent, string value, Vector2 position)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(RawImage), typeof(PixelText));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        RawImage image = textObject.GetComponent<RawImage>();
        image.raycastTarget = false;

        PixelText text = textObject.GetComponent<PixelText>();
        text.text = value;
        text.pixelSize = 5f;
        text.color = Color.white;
        text.Refresh();
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }

    private static void Restaurar(Scene scene, Scene original, bool alreadyOpen)
    {
        if (!alreadyOpen && scene.IsValid() && scene.isLoaded)
            EditorSceneManager.CloseScene(scene, true);

        if (original.IsValid() && original.isLoaded)
            SceneManager.SetActiveScene(original);
    }
}
