using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Creates a serialized menu with persistent button events and existing art.
public static class MainMenuSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private static readonly Color Ink = new Color32(18, 28, 54, 255);
    private static readonly Color Gold = new Color32(255, 213, 93, 255);

    [MenuItem("Tools/Super Mario/Crear menu principal")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) {
            Debug.LogError("[MainMenu] Sal del modo Play antes de crear la escena.");
            return;
        }

        if (File.Exists(ScenePath) && !Application.isBatchMode &&
            !EditorUtility.DisplayDialog("Menu principal", "La escena ya existe. ¿Reemplazarla?", "Reemplazar", "Cancelar")) {
            return;
        }

        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Build();
        EditorSceneManager.SaveScene(scene, ScenePath);
        RegisterBuildScene();
        Debug.Log("[MainMenu] Menu creado y registrado como primera escena del juego.");
    }

    [MenuItem("Tools/Super Mario/Registrar menu como escena inicial")]
    public static void RegisterBuildScene()
    {
        if (!File.Exists(ScenePath)) {
            Debug.LogError("[MainMenu] Crea primero la escena del menu principal.");
            return;
        }

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(scene => scene.path == ScenePath);
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
    }

    private static void Build()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(92, 148, 252, 255);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        new GameObject("GameManager", typeof(GameManager));

        GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        RectTransform scenery = Rect("Scenery", canvasObject.transform, Vector2.zero, Vector2.zero);
        Stretch(scenery);
        SpriteImage("Cloud Left", scenery, "Cloud2", new Vector2(-475f, 230f), new Vector2(240f, 84f));
        SpriteImage("Cloud Right", scenery, "Cloud1", new Vector2(480f, 165f), new Vector2(156f, 84f));
        RectTransform landscape = Rect("Landscape", scenery, new Vector2(0f, 96f), Vector2.zero);
        landscape.anchorMin = new Vector2(0.5f, 0f);
        landscape.anchorMax = new Vector2(0.5f, 0f);
        SpriteImage("Hill Left", landscape, "Hill1", new Vector2(-485f, 72f), new Vector2(300f, 144f));
        SpriteImage("Bush Right", landscape, "Bush2", new Vector2(495f, 36f), new Vector2(228f, 72f));
        SpriteImage("Mario", landscape, "Mario_Small_Idle", new Vector2(-455f, 32f), new Vector2(64f, 64f));
        SpriteImage("Pipe Top", landscape, "PipeTop", new Vector2(470f, 77f), new Vector2(128f, 64f));
        SpriteImage("Pipe Bottom", landscape, "PipeBottom", new Vector2(470f, 13f), new Vector2(128f, 64f));
        CreateGround(scenery);

        RectTransform frame = Rect("Menu Frame", canvasObject.transform, new Vector2(0f, 28f), new Vector2(736f, 528f));
        frame.gameObject.AddComponent<Image>().color = Gold;
        RectTransform surface = Rect("Menu Surface", frame, Vector2.zero, new Vector2(720f, 512f));
        surface.gameObject.AddComponent<Image>().color = Ink;
        CanvasGroup content = surface.gameObject.AddComponent<CanvasGroup>();

        RectTransform main = Rect("Main Panel", surface, Vector2.zero, Vector2.zero);
        Stretch(main);
        Label("Title", main, "SUPER MARIO", 8f, Gold, 168f);
        Label("Subtitle", main, "VICENTE", 5f, Color.white, 104f);
        Label("Adventure", main, "UNA NUEVA AVENTURA", 2.5f, Gold, 56f);

        Button play = CreateButton(main, "JUGAR", 0f);
        Button controls = CreateButton(main, "CONTROLES", -76f);
        Button quit = CreateButton(main, "SALIR", -152f);
        Link(play, controls, quit);
        Label("Navigation", main, "FLECHAS - ELEGIR    ENTER - ACEPTAR", 2.5f, Color.white, -218f);

        RectTransform help = Rect("Controls Panel", surface, Vector2.zero, Vector2.zero);
        Stretch(help);
        Label("Heading", help, "CONTROLES", 6f, Gold, 178f);
        Label("Move", help, "MOVER - A Y D O FLECHAS", 3.5f, Color.white, 108f);
        Label("Jump", help, "SALTAR - ESPACIO", 3.5f, Color.white, 53f);
        Label("Swim", help, "NADAR - PULSAR ESPACIO", 3.5f, Color.white, -2f);
        Label("Pipe", help, "TUBERIAS - S ABAJO O D LATERAL", 3f, Color.white, -57f);
        Label("Lives", help, "EMPIEZAS CON 3 VIDAS", 3f, Gold, -112f);
        Button back = CreateButton(help, "VOLVER", -180f);
        back.navigation = new Navigation { mode = Navigation.Mode.None };

        MainMenuScreen screen = canvasObject.AddComponent<MainMenuScreen>();
        screen.content = content;
        screen.mainPanel = main.gameObject;
        screen.controlsPanel = help.gameObject;
        screen.playButton = play;
        screen.controlsButton = controls;
        screen.backButton = back;
        screen.playLabel = play.GetComponentInChildren<PixelText>();
        UnityEventTools.AddPersistentListener(play.onClick, screen.Play);
        UnityEventTools.AddPersistentListener(controls.onClick, screen.ShowControls);
        UnityEventTools.AddPersistentListener(quit.onClick, screen.Quit);
        UnityEventTools.AddPersistentListener(back.onClick, screen.HideControls);
        help.gameObject.SetActive(false);
        EditorUtility.SetDirty(screen);
    }

    private static void CreateGround(RectTransform parent)
    {
        RectTransform ground = Rect("Ground", parent, Vector2.zero, new Vector2(0f, 96f));
        ground.anchorMin = new Vector2(0f, 0f);
        ground.anchorMax = new Vector2(1f, 0f);
        ground.pivot = new Vector2(0.5f, 0f);
        GridLayoutGroup grid = ground.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(48f, 48f);
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.UpperCenter;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GroundBlock.png");
        for (int i = 0; i < 160; i++) {
            Image block = Rect("Block", ground, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
            block.sprite = sprite;
            block.raycastTarget = false;
        }
    }

    private static Button CreateButton(Transform parent, string label, float y)
    {
        RectTransform rect = Rect(label, parent, new Vector2(0f, y), new Vector2(440f, 60f));
        Image image = rect.gameObject.AddComponent<Image>();
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color32(35, 54, 88, 255);
        colors.highlightedColor = new Color32(143, 62, 23, 255);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color32(103, 43, 18, 255);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = Gold;
        outline.effectDistance = new Vector2(2f, -2f);
        rect.gameObject.AddComponent<SelectOnHover>();
        Label("Label", rect, label, 4f, Color.white, 0f);
        return button;
    }

    private static void Link(params Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].navigation = new Navigation {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[(i + buttons.Length - 1) % buttons.Length],
                selectOnDown = buttons[(i + 1) % buttons.Length]
            };
        }
    }

    private static PixelText Label(string name, Transform parent, string value, float pixelSize, Color color, float y)
    {
        RectTransform rect = Rect(name, parent, new Vector2(0f, y), Vector2.zero);
        rect.gameObject.AddComponent<RawImage>().raycastTarget = false;
        PixelText text = rect.gameObject.AddComponent<PixelText>();
        text.text = value;
        text.pixelSize = pixelSize;
        text.color = color;
        text.Refresh();
        return text;
    }

    private static void SpriteImage(string name, Transform parent, string spriteName, Vector2 position, Vector2 size)
    {
        Image image = Rect(name, parent, position, size).gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + spriteName + ".png");
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        RectTransform rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
