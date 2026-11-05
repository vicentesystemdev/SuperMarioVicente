using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class AplicarMejorasCarlaAcuatico
{
    private const string ScenePath = "Assets/Scenes/2-2.unity";
    private const string MarkerName = "MEJORAS_CARLA_ACUATICO_V1";

    static AplicarMejorasCarlaAcuatico()
    {
        EditorApplication.delayCall += AplicarSiHaceFalta;
    }

    [MenuItem("Tools/Super Mario/Aplicar mejoras Carla al nivel 2-2")]
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

        new GameObject(MarkerName);

        PrepararHud(scene);
        PrepararTemporizador(scene);
        PrepararSalida(scene);
        PrepararCamara(scene);
        PrepararAgua(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[Carla] Mejoras acuaticas aplicadas y guardadas en Assets/Scenes/2-2.unity.");

        Restaurar(scene, original, alreadyOpen);
    }

    private static void PrepararHud(Scene scene)
    {
        GameObject hud = FindRoot(scene, "HUD Acuatico");

        if (hud == null)
        {
            hud = new GameObject(
                "HUD Acuatico",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(HudAcuatico)
            );

            SceneManager.MoveGameObjectToScene(hud, scene);

            Canvas canvas = hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject bar = new GameObject("Barra Superior", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(hud.transform, false);

            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 150f);

            Image image = bar.GetComponent<Image>();
            image.color = new Color(0.015f, 0.04f, 0.12f, 0.72f);
            image.raycastTarget = false;

            CrearTexto("Mario Label", bar.transform, "MARIO", new Vector2(190f, -42f));
            CrearTexto("Score Label", bar.transform, "000000", new Vector2(190f, -92f));
            CrearTexto("Coins Label", bar.transform, "MONEDAS", new Vector2(570f, -42f));
            CrearTexto("Coins Value", bar.transform, "X00", new Vector2(570f, -92f));
            CrearTexto("World Label", bar.transform, "MUNDO 2-2", new Vector2(960f, -42f));
            CrearTexto("Time Label", bar.transform, "TIEMPO", new Vector2(1460f, -42f));
            CrearTexto("Time Value", bar.transform, "180", new Vector2(1460f, -92f));
        }
    }

    private static void PrepararTemporizador(Scene scene)
    {
        GameObject timer = FindRoot(scene, "Temporizador Acuatico");

        if (timer == null)
        {
            timer = new GameObject("Temporizador Acuatico");
            SceneManager.MoveGameObjectToScene(timer, scene);
            timer.AddComponent<TemporizadorAcuatico>();
        }
    }

    private static void PrepararSalida(Scene scene)
    {
        GameObject salida = FindRoot(scene, "Salida Acuatica Corta");

        if (salida == null)
        {
            salida = new GameObject("Salida Acuatica Corta");
            SceneManager.MoveGameObjectToScene(salida, scene);
            salida.transform.position = new Vector3(278f, -9f, 0f);

            BoxCollider2D collider = salida.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(3f, 14f);

            salida.AddComponent<SalidaAcuaticaCorta>();
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
