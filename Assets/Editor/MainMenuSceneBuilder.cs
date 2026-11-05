using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Menu.unity";
    private const string MarkerName = "MENU_MARIO_CARLA";
    private const string GroundSpritePath = "Assets/Sprites/GroundBlock.png";
    private const string Cloud1Path = "Assets/Sprites/Cloud1.png";
    private const string Cloud2Path = "Assets/Sprites/Cloud2.png";
    private const string MarioPath = "Assets/Sprites/Mario_Small_Idle.png";
    private const string MysteryPath = "Assets/Sprites/MysteryBlock.png";

    static MainMenuSceneBuilder()
    {
        EditorApplication.delayCall += EnsureMenuScene;
    }

    [MenuItem("Tools/Super Mario/Crear menu principal Carla")]
    public static void CreateMenuScene()
    {
        BuildScene(true);
    }

    private static void EnsureMenuScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        BuildScene(false);
    }

    private static void BuildScene(bool force)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene original = SceneManager.GetActiveScene();
        Scene menuScene;
        bool wasOpen = false;

        if (File.Exists(ScenePath))
        {
            menuScene = SceneManager.GetSceneByPath(ScenePath);

            if (menuScene.IsValid() && menuScene.isLoaded)
            {
                wasOpen = true;
            }
            else
            {
                menuScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            }
        }
        else
        {
            menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }

        if (!force && HasMarker(menuScene))
        {
            CloseAndRestore(menuScene, original, wasOpen);
            return;
        }

        SceneManager.SetActiveScene(menuScene);

        foreach (GameObject root in menuScene.GetRootGameObjects())
            Object.DestroyImmediate(root);

        Build();

        EditorSceneManager.MarkSceneDirty(menuScene);
        EditorSceneManager.SaveScene(menuScene, ScenePath);

        BuildSceneList.Add(
            ScenePath,
            "Assets/Scenes/1-1.unity",
            "Assets/Scenes/1-2.unity",
            "Assets/Scenes/2-2.unity",
            "Assets/Scenes/8-4.unity",
            "Assets/Scenes/GameOver.unity",
            "Assets/Scenes/Victory.unity"
        );

        Debug.Log("[MainMenuSceneBuilder] Menu principal creado y registrado en Build Profiles.");

        CloseAndRestore(menuScene, original, wasOpen);
    }

    private static bool HasMarker(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == MarkerName)
                return true;
        }

        return false;
    }

    private static void Build()
    {
        new GameObject(MarkerName);

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.20f, 0.55f, 0.95f);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject controllerObject = new GameObject("Menu Controller");
        MainMenu controller = controllerObject.AddComponent<MainMenu>();

        GameObject canvasObject = new GameObject(
            "Menu Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform background = CreateRect("Sky Background", canvasObject.transform);
        Stretch(background);
        Image backgroundImage = background.gameObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.20f, 0.55f, 0.95f);
        backgroundImage.raycastTarget = false;

        CreateDecorations(background);

        RectTransform content = CreateRect("Menu Content", canvasObject.transform);
        Stretch(content);

        PixelText title = CreatePixelText("Title", content, "SUPER MARIO", 16f, new Color(1f, 0.82f, 0.08f));
        title.rectTransform().anchoredPosition = new Vector2(0f, 330f);

        PixelText subtitle = CreatePixelText("Subtitle", content, "CARLA - SELECCION DE NIVEL", 5f, Color.white);
        subtitle.rectTransform().anchoredPosition = new Vector2(0f, 245f);

        RectTransform panel = CreateRect("Level Panel", content);
        panel.sizeDelta = new Vector2(820f, 500f);
        panel.anchoredPosition = new Vector2(0f, -35f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.03f, 0.05f, 0.12f, 0.78f);

        VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(45, 45, 40, 40);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Button level1 = CreateButton("Nivel 1 Button", panel, "MUNDO 1-1", new Color(0.82f, 0.12f, 0.08f));
        Button underground = CreateButton("Subterraneo Button", panel, "SUBTERRANEO 1-2", new Color(0.55f, 0.28f, 0.72f));
        Button water = CreateButton("Acuatico Button", panel, "ACUATICO 2-2", new Color(0.04f, 0.40f, 0.86f));
        Button castle = CreateButton("Castillo Button", panel, "CASTILLO 8-4", new Color(0.55f, 0.10f, 0.10f));
        Button quit = CreateButton("Salir Button", panel, "SALIR", new Color(0.20f, 0.22f, 0.28f));

        Link(level1, underground, water, castle, quit);

        UnityEventTools.AddPersistentListener(level1.onClick, controller.JugarNivelBase);
        UnityEventTools.AddPersistentListener(underground.onClick, controller.JugarSubterraneo);
        UnityEventTools.AddPersistentListener(water.onClick, controller.JugarAcuatico);
        UnityEventTools.AddPersistentListener(castle.onClick, controller.JugarCastillo);
        UnityEventTools.AddPersistentListener(quit.onClick, controller.Salir);

        PixelText hint = CreatePixelText("Hint", content, "FLECHAS PARA ELEGIR  -  ENTER PARA JUGAR", 4f, Color.white);
        hint.rectTransform().anchoredPosition = new Vector2(0f, -340f);

        EditorUtility.SetDirty(controller);
    }

    private static void CreateDecorations(RectTransform parent)
    {
        CreateSprite("Cloud Left", parent, Cloud1Path, new Vector2(230f, 850f), new Vector2(260f, 120f));
        CreateSprite("Cloud Right", parent, Cloud2Path, new Vector2(1600f, 760f), new Vector2(300f, 135f));
        CreateSprite("Mario", parent, MarioPath, new Vector2(225f, 175f), new Vector2(120f, 150f));
        CreateSprite("Mystery Left", parent, MysteryPath, new Vector2(430f, 160f), new Vector2(92f, 92f));
        CreateSprite("Mystery Right", parent, MysteryPath, new Vector2(1490f, 160f), new Vector2(92f, 92f));

        RectTransform ground = CreateRect("Ground", parent);
        ground.anchorMin = new Vector2(0f, 0f);
        ground.anchorMax = new Vector2(1f, 0f);
        ground.pivot = new Vector2(0.5f, 0f);
        ground.sizeDelta = new Vector2(0f, 128f);
        ground.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = ground.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(64f, 64f);
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.UpperCenter;

        Sprite groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);

        for (int i = 0; i < 80; i++)
        {
            Image block = CreateRect("Ground Block", ground).gameObject.AddComponent<Image>();
            block.sprite = groundSprite;
            block.preserveAspect = true;
            block.raycastTarget = false;
        }
    }

    private static void CreateSprite(string name, Transform parent, string path, Vector2 position, Vector2 size)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static Button CreateButton(string name, RectTransform parent, string label, Color color)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.sizeDelta = new Vector2(700f, 72f);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = Color.Lerp(color, Color.black, 0.20f);
        colors.disabledColor = new Color(color.r, color.g, color.b, 0.4f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        rect.gameObject.AddComponent<SelectOnHover>();

        PixelText text = CreatePixelText("Label", rect, label, 5f, Color.white);
        text.GetComponent<RawImage>().raycastTarget = false;

        return button;
    }

    private static void Link(params Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int previous = (i - 1 + buttons.Length) % buttons.Length;
            int next = (i + 1) % buttons.Length;

            buttons[i].navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[previous],
                selectOnDown = buttons[next]
            };
        }
    }

    private static PixelText CreatePixelText(string name, Transform parent, string value, float pixelSize, Color color)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.gameObject.AddComponent<RawImage>();

        PixelText text = rect.gameObject.AddComponent<PixelText>();
        text.text = value;
        text.pixelSize = pixelSize;
        text.color = color;
        text.Refresh();

        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return (RectTransform)gameObject.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CloseAndRestore(Scene menuScene, Scene original, bool wasOpen)
    {
        if (!wasOpen && menuScene.IsValid() && menuScene.isLoaded)
            EditorSceneManager.CloseScene(menuScene, true);

        if (original.IsValid() && original.isLoaded)
            SceneManager.SetActiveScene(original);
    }

    private static RectTransform rectTransform(this Component component)
    {
        return (RectTransform)component.transform;
    }
}
