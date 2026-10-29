using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds Assets/Scenes/Victory.unity with real components and persistent
// button events. Called by the castle builder, or on its own from the menu.
public static class VictorySceneBuilder
{
    public const string ScenePath = "Assets/Scenes/Victory.unity";

    private const string GroundSpritePath = "Assets/Sprites/Castle/Castle_Stone.png";
    private const string TrophySpritePath = "Assets/Sprites/Castle/Trophy.png";
    private const string MarioSpritePath = "Assets/Sprites/Mario_Big_Jump.png";
    private const string CursorSpritePath = "Assets/Sprites/Coin.png";

    [MenuItem("Tools/Super Mario/Crear escena de Victoria")]
    public static void CreateFromMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        // the ground and the treasure use the castle sprites
        if (!SpriteImport.Configure("Assets/Sprites/Castle", "Castle_Stone")) {
            return;
        }

        string previousScene = SceneManager.GetActiveScene().path;

        if (CreateScene()) {
            BuildSceneList.Add(ScenePath);
        }

        if (!string.IsNullOrEmpty(previousScene)) {
            EditorSceneManager.OpenScene(previousScene);
        }
    }

    // returns true if the scene exists after the call
    public static bool CreateScene()
    {
        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Victoria", $"{ScenePath} ya existe. ¿Reemplazarla?", "Reemplazar", "Conservar la actual")) {
            return true;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Build();
        EditorSceneManager.SaveScene(scene, ScenePath);

        Debug.Log($"[VictorySceneBuilder] Escena creada en {ScenePath}.");
        return true;
    }

    private static void Build()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.05f, 0.03f, 0.1f);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject audioObject = new GameObject("Music", typeof(AudioSource));
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        GameObject canvasObject = new GameObject("Victory Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform background = UIRect("Background", canvasObject.transform);
        Stretch(background);
        background.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.1f);

        RectTransform confetti = UIRect("Confetti", canvasObject.transform);
        Stretch(confetti);

        RectTransform contentRect = UIRect("Content", canvasObject.transform);
        Stretch(contentRect);
        CanvasGroup content = contentRect.gameObject.AddComponent<CanvasGroup>();

        CreateGround(contentRect);

        PixelText title = CreateText("Title", contentRect, "FELICIDADES", 14f, Color.white, new Vector2(0f, 300f));
        CreateText("Subtitle", contentRect, "EL REY MAGMOR HA CAIDO", 5f, new Color(1f, 0.85f, 0.5f), new Vector2(0f, 190f));
        PixelText stats = CreateText("Stats", contentRect, "MONEDAS 0  VIDAS 3", 5f, new Color(0.8f, 0.8f, 0.9f), new Vector2(0f, 130f));

        // Mario celebrating next to the treasure
        RectTransform mario = SpriteImage("Mario", contentRect, MarioSpritePath, new Vector2(80f, 160f), new Vector2(-430f, -330f));
        SpriteImage("Treasure", contentRect, TrophySpritePath, new Vector2(110f, 110f), new Vector2(430f, -355f));

        // buttons
        RectTransform buttonsRect = UIRect("Buttons", contentRect);
        buttonsRect.sizeDelta = new Vector2(700f, 220f);
        buttonsRect.anchoredPosition = new Vector2(0f, -90f);
        CanvasGroup buttons = buttonsRect.gameObject.AddComponent<CanvasGroup>();

        VerticalLayoutGroup layout = buttonsRect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Button newGameButton = CreateButton("New Game Button", buttonsRect, "NUEVA PARTIDA");
        Button quitButton = CreateButton("Quit Button", buttonsRect, "SALIR");

        newGameButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = quitButton, selectOnUp = quitButton };
        quitButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = newGameButton, selectOnDown = newGameButton };

        RectTransform cursor = SpriteImage("Cursor", contentRect, CursorSpritePath, new Vector2(48f, 48f), Vector2.zero);

        VictoryScreen screen = canvasObject.AddComponent<VictoryScreen>();
        screen.content = content;
        screen.title = (RectTransform)title.transform;
        screen.titleImage = title.GetComponent<RawImage>();
        screen.statsLabel = stats;
        screen.buttons = buttons;
        screen.newGameButton = newGameButton;
        screen.quitButton = quitButton;
        screen.cursor = cursor;
        screen.mario = mario;
        screen.confettiArea = confetti;
        screen.audioSource = audioSource;

        UnityEventTools.AddPersistentListener(newGameButton.onClick, screen.NewGame);
        UnityEventTools.AddPersistentListener(quitButton.onClick, screen.Quit);

        EditorUtility.SetDirty(screen);
    }

    private static void CreateGround(RectTransform parent)
    {
        RectTransform ground = UIRect("Ground", parent);
        ground.anchorMin = new Vector2(0f, 0f);
        ground.anchorMax = new Vector2(1f, 0f);
        ground.pivot = new Vector2(0.5f, 0f);
        ground.sizeDelta = new Vector2(0f, 128f);

        GridLayoutGroup grid = ground.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(64f, 64f);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 2;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);

        for (int i = 0; i < 96; i++)
        {
            Image block = UIRect("Block", ground).gameObject.AddComponent<Image>();
            block.sprite = sprite;
            block.raycastTarget = false;
        }
    }

    private static Button CreateButton(string name, RectTransform parent, string label)
    {
        RectTransform rect = UIRect(name, parent);
        rect.sizeDelta = new Vector2(660f, 84f);

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

        PixelText text = CreateText("Label", rect, label, 7f, Color.white, Vector2.zero);
        text.GetComponent<RawImage>().raycastTarget = false;

        return button;
    }

    private static PixelText CreateText(string name, Transform parent, string value, float pixelSize, Color color, Vector2 position)
    {
        RectTransform rect = UIRect(name, parent);
        rect.gameObject.AddComponent<RawImage>().raycastTarget = false;

        PixelText text = rect.gameObject.AddComponent<PixelText>();
        text.text = value;
        text.pixelSize = pixelSize;
        text.color = color;
        text.Refresh();

        rect.anchoredPosition = position;
        return text;
    }

    private static RectTransform SpriteImage(string name, Transform parent, string spritePath, Vector2 size, Vector2 position)
    {
        RectTransform rect = UIRect(name, parent);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        image.preserveAspect = true;
        image.raycastTarget = false;

        return rect;
    }

    private static RectTransform UIRect(string name, Transform parent)
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

}
