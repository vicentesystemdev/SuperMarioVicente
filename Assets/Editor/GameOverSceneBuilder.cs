using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds Assets/Scenes/GameOver.unity with real components and persistent
// button events, and registers the scenes in the shared Build Profiles list.
public static class GameOverSceneBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string ScenePath = ScenesFolder + "/GameOver.unity";
    private const string FirstLevelGuid = "2cda990e2423bbf4892e6590ba056729"; // 1-1.unity
    private const string GroundSpritePath = "Assets/Sprites/GroundBlock.png";
    private const string CursorSpritePath = "Assets/Sprites/Coin.png";

    [MenuItem("Tools/Super Mario/Crear escena Game Over")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[GameOverSceneBuilder] Sal del modo Play antes de crear la escena.");
            return;
        }

        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Game Over", $"{ScenePath} ya existe. ¿Reemplazarla?", "Reemplazar", "Cancelar")) {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        if (!AssetDatabase.IsValidFolder(ScenesFolder)) {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string previousScene = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Build();

        EditorSceneManager.SaveScene(scene, ScenePath);
        RegisterBuildScenes();

        if (!string.IsNullOrEmpty(previousScene)) {
            EditorSceneManager.OpenScene(previousScene);
        }

        Debug.Log($"[GameOverSceneBuilder] Escena creada en {ScenePath} y registrada en Build Profiles.");
    }

    [MenuItem("Tools/Super Mario/Registrar escenas en Build Profiles")]
    public static void RegisterBuildScenes()
    {
        string firstLevelPath = AssetDatabase.GUIDToAssetPath(FirstLevelGuid);
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // drop entries whose file no longer exists (e.g. a moved scene)
        scenes.RemoveAll(s => !File.Exists(s.path));

        if (!string.IsNullOrEmpty(firstLevelPath) && !scenes.Exists(s => s.path == firstLevelPath)) {
            scenes.Insert(0, new EditorBuildSettingsScene(firstLevelPath, true));
        }

        if (File.Exists(ScenePath) && !scenes.Exists(s => s.path == ScenePath)) {
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();

        foreach (EditorBuildSettingsScene s in scenes) {
            Debug.Log($"[GameOverSceneBuilder] Build scene: {s.path} (enabled: {s.enabled})");
        }
    }

    private static void Build()
    {
        // camera
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

        // input for the buttons (project uses the legacy Input Manager)
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // audio
        GameObject audioObject = new GameObject("Audio", typeof(AudioSource));
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // canvas scaled for any resolution
        GameObject canvasObject = new GameObject("Game Over Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform background = CreateRect("Background", canvasObject.transform);
        Stretch(background);
        background.gameObject.AddComponent<Image>().color = Color.black;

        RectTransform contentRect = CreateRect("Content", canvasObject.transform);
        Stretch(contentRect);
        CanvasGroup content = contentRect.gameObject.AddComponent<CanvasGroup>();

        CreateGround(contentRect);

        PixelText title = CreatePixelText("Title", contentRect, "GAME OVER", 14f, Color.white);
        title.rectTransform().anchoredPosition = new Vector2(0f, 190f);

        PixelText worldLabel = CreatePixelText("World Label", contentRect, "MUNDO 1-1", 6f, new Color(0.75f, 0.75f, 0.75f));
        worldLabel.rectTransform().anchoredPosition = new Vector2(0f, 70f);

        // buttons
        RectTransform buttonsRect = CreateRect("Buttons", contentRect);
        buttonsRect.sizeDelta = new Vector2(600f, 220f);
        buttonsRect.anchoredPosition = new Vector2(0f, -110f);
        CanvasGroup buttons = buttonsRect.gameObject.AddComponent<CanvasGroup>();

        VerticalLayoutGroup layout = buttonsRect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Button retryButton = CreateButton("Retry Button", buttonsRect, "REINTENTAR");
        Button quitButton = CreateButton("Quit Button", buttonsRect, "SALIR");
        Link(retryButton, quitButton);

        // coin cursor next to the selected button
        RectTransform cursor = CreateRect("Cursor", contentRect);
        cursor.sizeDelta = new Vector2(48f, 48f);
        Image cursorImage = cursor.gameObject.AddComponent<Image>();
        cursorImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CursorSpritePath);
        cursorImage.preserveAspect = true;
        cursorImage.raycastTarget = false;

        // controller
        GameOverScreen screen = canvasObject.AddComponent<GameOverScreen>();
        screen.content = content;
        screen.title = title.rectTransform();
        screen.worldLabel = worldLabel;
        screen.buttons = buttons;
        screen.retryButton = retryButton;
        screen.quitButton = quitButton;
        screen.cursor = cursor;
        screen.audioSource = audioSource;

        UnityEventTools.AddPersistentListener(retryButton.onClick, screen.Retry);
        UnityEventTools.AddPersistentListener(quitButton.onClick, screen.Quit);

        EditorUtility.SetDirty(screen);
    }

    private static void CreateGround(RectTransform parent)
    {
        RectTransform ground = CreateRect("Ground", parent);
        ground.anchorMin = new Vector2(0f, 0f);
        ground.anchorMax = new Vector2(1f, 0f);
        ground.pivot = new Vector2(0.5f, 0f);
        ground.sizeDelta = new Vector2(0f, 128f);
        ground.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = ground.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(64f, 64f);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 2;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);

        // 2 rows wide enough for ultra wide screens, dimmed for a dark mood
        for (int i = 0; i < 96; i++)
        {
            Image block = CreateRect("Block", ground).gameObject.AddComponent<Image>();
            block.sprite = sprite;
            block.color = new Color(0.45f, 0.45f, 0.45f);
            block.raycastTarget = false;
        }
    }

    private static Button CreateButton(string name, RectTransform parent, string label)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.sizeDelta = new Vector2(520f, 84f);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = Color.white;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
        colors.selectedColor = new Color(1f, 1f, 1f, 0.12f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.3f);
        colors.disabledColor = new Color(1f, 1f, 1f, 0f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        rect.gameObject.AddComponent<SelectOnHover>();

        PixelText text = CreatePixelText("Label", rect, label, 7f, Color.white);
        text.GetComponent<RawImage>().raycastTarget = false;

        return button;
    }

    private static void Link(Button up, Button down)
    {
        Navigation upNavigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = down, selectOnUp = down };
        Navigation downNavigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = up, selectOnDown = up };
        up.navigation = upNavigation;
        down.navigation = downNavigation;
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

    private static RectTransform rectTransform(this Component component)
    {
        return (RectTransform)component.transform;
    }

}
