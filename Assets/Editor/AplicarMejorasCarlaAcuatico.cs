using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AplicarMejorasCarlaAcuatico
{
    private const string ScenePath = "Assets/Scenes/2-2.unity";
    private const string MarkerName = "MEJORAS_CARLA_ACUATICO_V4";

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

        GameObject oldMarkerV3 = FindRoot(scene, "MEJORAS_CARLA_ACUATICO_V3");
        if (oldMarkerV3 != null)
            Object.DestroyImmediate(oldMarkerV3);

        GameObject marker = new GameObject(MarkerName);
        SceneManager.MoveGameObjectToScene(marker, scene);

        QuitarSistemasDuplicados(scene);
        PrepararConfiguracionNivel(scene);
        RecortarNivel(scene);
        PrepararSalida(scene);
        PrepararCamara(scene);
        PrepararAgua(scene);
        PrepararBloquesPoder(scene);
        PrepararFaunaNueva(scene);
        PrepararCorrientes(scene);
        PrepararMusicaDinamica(scene);

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

    private static void RecortarNivel(Scene scene)
    {
        GameObject level = FindRoot(scene, "Level 2-2");

        if (level == null)
            return;

        Transform underwater = level.transform.Find("Underwater");

        if (underwater == null)
            return;

        for (int i = underwater.childCount - 1; i >= 0; i--)
        {
            Transform child = underwater.GetChild(i);
            string name = child.name;

            if (name.StartsWith("3 -") || name.StartsWith("4 -") || name.StartsWith("5 -") ||
                name.StartsWith("6 -") || name.StartsWith("7 -") || name.StartsWith("8 -"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
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

        settings.timeLimit = 110;
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

        salida.transform.position = new Vector3(110f, -9f, 0f);

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
                scrolling.limitRight = true;
                scrolling.rightLimit = 142f;
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

    private static void PrepararMusicaDinamica(Scene scene)
    {
        GameObject musicObject = FindRoot(scene, "Music");

        if (musicObject == null)
            return;

        LevelMusic levelMusic = musicObject.GetComponent<LevelMusic>();
        if (levelMusic != null)
            levelMusic.theme = LevelMusic.Theme.Water;

        MusicaAcuaticaDinamica dynamicMusic = musicObject.GetComponent<MusicaAcuaticaDinamica>();
        if (dynamicMusic == null)
            dynamicMusic = musicObject.AddComponent<MusicaAcuaticaDinamica>();

        dynamicMusic.cambioX = 58f;
    }

    private static void PrepararCorrientes(Scene scene)
    {
        GameObject oldGroup = FindRoot(scene, "Corrientes Acuaticas");
        if (oldGroup != null)
            Object.DestroyImmediate(oldGroup);

        GameObject group = new GameObject("Corrientes Acuaticas");
        SceneManager.MoveGameObjectToScene(group, scene);

        Sprite bubble = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Water/Water_Bubble.png");

        CrearCorriente(group.transform, new Vector3(52f, -9f, 0f), new Vector2(10f, 8f), new Vector2(2.6f, 0.7f), bubble, "Corriente a favor");
        CrearCorriente(group.transform, new Vector3(86f, -9f, 0f), new Vector2(9f, 8f), new Vector2(-2.2f, 0.3f), bubble, "Corriente en contra");
    }

    private static void CrearCorriente(Transform parent, Vector3 position, Vector2 size, Vector2 force, Sprite bubble, string name)
    {
        GameObject zone = new GameObject(name);
        zone.transform.SetParent(parent, false);
        zone.transform.position = position;

        BoxCollider2D collider = zone.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = size;

        CorrienteAcuatica current = zone.AddComponent<CorrienteAcuatica>();
        current.fuerza = force;

        for (int i = 0; i < 5; i++)
        {
            GameObject indicator = new GameObject($"Burbuja corriente {i + 1}");
            indicator.transform.SetParent(zone.transform, false);
            indicator.transform.localPosition = new Vector3(-size.x * 0.35f + i * size.x * 0.17f, (i % 2 == 0 ? -1f : 1f) * 1.2f, 0f);

            SpriteRenderer renderer = indicator.AddComponent<SpriteRenderer>();
            renderer.sprite = bubble;
            renderer.color = new Color(0.7f, 0.95f, 1f, 0.65f);
            renderer.sortingOrder = 2;
        }
    }

    private static void PrepararFaunaNueva(Scene scene)
    {
        GameObject oldGroup = FindRoot(scene, "Fauna Nueva Carla");
        if (oldGroup != null)
            Object.DestroyImmediate(oldGroup);

        GameObject group = new GameObject("Fauna Nueva Carla");
        SceneManager.MoveGameObjectToScene(group, scene);

        Sprite fish1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Water/Fish_Swim1.png");
        Sprite fish2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Water/Fish_Swim2.png");

        CrearPezGlobo(group.transform, new Vector3(42f, -8f, 0f), fish1, fish2);
        CrearPezGlobo(group.transform, new Vector3(88f, -6f, 0f), fish1, fish2);

        CrearAnguila(group.transform, new Vector3(70f, -11f, 0f), fish1);
        CrearAnguila(group.transform, new Vector3(102f, -8f, 0f), fish1);
    }

    private static void CrearPezGlobo(Transform parent, Vector3 position, Sprite frame1, Sprite frame2)
    {
        GameObject animal = CrearEnemigoBase(parent, "Pez Globo", position, frame1, new Color(1f, 0.82f, 0.25f));
        animal.transform.localScale = Vector3.one * 1.25f;

        AnimatedSprite animation = animal.AddComponent<AnimatedSprite>();
        animation.sprites = new[] { frame1, frame2 };
        animation.framerate = 0.24f;

        PufferFish puffer = animal.AddComponent<PufferFish>();
        puffer.speed = 1.1f;
        puffer.waveAmplitude = 0.8f;
    }

    private static void CrearAnguila(Transform parent, Vector3 position, Sprite sprite)
    {
        GameObject animal = CrearEnemigoBase(parent, "Anguila", position, sprite, new Color(0.35f, 1f, 0.55f));
        animal.transform.localScale = new Vector3(1.8f, 0.75f, 1f);

        Eel eel = animal.AddComponent<Eel>();
        eel.patrolSpeed = 1.2f;
        eel.dashSpeed = 5.8f;
    }

    private static GameObject CrearEnemigoBase(Transform parent, string name, Vector3 position, Sprite sprite, Color tint)
    {
        GameObject animal = new GameObject(name);
        animal.transform.SetParent(parent, false);
        animal.transform.position = position;
        animal.layer = LayerMask.NameToLayer("Enemy");

        SpriteRenderer renderer = animal.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tint;
        renderer.sortingOrder = 4;

        Rigidbody2D body = animal.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        CircleCollider2D collider = animal.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.48f;

        DeathAnimation death = animal.AddComponent<DeathAnimation>();
        death.spriteRenderer = renderer;
        death.enabled = false;

        return animal;
    }

    private static void PrepararBloquesPoder(Scene scene)
    {
        GameObject oldGroup = FindRoot(scene, "Poderes Acuaticos");
        if (oldGroup != null)
            Object.DestroyImmediate(oldGroup);

        GameObject group = new GameObject("Poderes Acuaticos");
        SceneManager.MoveGameObjectToScene(group, scene);

        CrearBloquePoder(group.transform, new Vector3(28f, -9f, 0f), "MagicMushroom", "Bloque Super Hongo");
        CrearBloquePoder(group.transform, new Vector3(62f, -7f, 0f), "Starman", "Bloque Estrella");
        CrearBloquePoder(group.transform, new Vector3(94f, -11f, 0f), "1upMushroom", "Bloque Vida Extra");
    }

    private static void CrearBloquePoder(Transform parent, Vector3 position, string itemName, string objectName)
    {
        GameObject blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MysteryBlock.prefab");
        GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{itemName}.prefab");

        if (blockPrefab == null || itemPrefab == null)
            return;

        GameObject block = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, parent);
        block.name = objectName;
        block.transform.position = position;

        BlockHit hit = block.GetComponent<BlockHit>();
        if (hit != null)
            hit.item = itemPrefab;
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
