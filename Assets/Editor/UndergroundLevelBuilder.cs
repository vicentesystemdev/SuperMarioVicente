using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds Assets/Scenes/1-2.unity: an underground level made from the
// project prefabs, registers it in Build Profiles and links the 1-1 flag
// pole to it.
//
// Coordinates follow the 1-1 conventions: tile centers on integers, the
// underground band uses camera height -9.5, so the floor fills rows -16 and
// -15 (top at -14.5) and the ceiling sits on row -3.
public static class UndergroundLevelBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string ScenePath = ScenesFolder + "/1-2.unity";
    private const string FirstLevelGuid = "2cda990e2423bbf4892e6590ba056729"; // 1-1.unity
    private const string GameOverPath = ScenesFolder + "/GameOver.unity";
    private const string Prefabs = "Assets/Prefabs/";
    private const string Sprites = "Assets/Sprites/";

    private const int FloorRow = -16;   // floor rows -16 and -15
    private const int StandRow = -14;   // first free row above the floor
    private const int CeilingRow = -3;
    private const int GroundRow = 0;    // surface ground rows 0 and 1

    private static Sprite undergroundBrickSprite;
    private static Sprite undergroundCoinSprite;

    [MenuItem("Tools/Super Mario/Crear nivel subterráneo 1-2")]
    public static void CreateLevel()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[UndergroundLevelBuilder] Sal del modo Play antes de crear el nivel.");
            return;
        }

        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Nivel 1-2", $"{ScenePath} ya existe. ¿Reemplazarla?", "Reemplazar", "Cancelar")) {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        if (!AssetDatabase.IsValidFolder(ScenesFolder)) {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string previousScene = SceneManager.GetActiveScene().path;

        undergroundBrickSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + "UndergroundBrick.png");
        undergroundCoinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + "Coin_Underground.png");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Build();
        EditorSceneManager.SaveScene(scene, ScenePath);

        RegisterBuildScenes();
        LinkFirstLevel();

        if (!string.IsNullOrEmpty(previousScene)) {
            EditorSceneManager.OpenScene(previousScene);
        }

        Debug.Log($"[UndergroundLevelBuilder] Nivel creado en {ScenePath}, registrado en Build Profiles y enlazado desde 1-1.");
    }

    [MenuItem("Tools/Super Mario/Enlazar bandera de 1-1 con 1-2")]
    public static void LinkFirstLevel()
    {
        string path = AssetDatabase.GUIDToAssetPath(FirstLevelGuid);

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[UndergroundLevelBuilder] No se encontró 1-1.unity.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        foreach (FlagPole flagPole in Object.FindObjectsByType<FlagPole>(FindObjectsSortMode.None))
        {
            SerializedObject serialized = new SerializedObject(flagPole);
            serialized.FindProperty("nextWorld").intValue = 1;
            serialized.FindProperty("nextStage").intValue = 2;

            if (serialized.ApplyModifiedProperties()) {
                Debug.Log("[UndergroundLevelBuilder] La bandera de 1-1 ahora lleva a 1-2.");
            }
        }

        EditorSceneManager.SaveScene(scene);
    }

    private static void RegisterBuildScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string firstLevelPath = AssetDatabase.GUIDToAssetPath(FirstLevelGuid);

        // keep every existing entry, only add the missing ones
        if (!string.IsNullOrEmpty(firstLevelPath) && !scenes.Exists(s => s.path == firstLevelPath)) {
            scenes.Insert(0, new EditorBuildSettingsScene(firstLevelPath, true));
        }

        foreach (string path in new[] { GameOverPath, ScenePath })
        {
            if (File.Exists(path) && !scenes.Exists(s => s.path == path)) {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();

        foreach (EditorBuildSettingsScene s in scenes) {
            Debug.Log($"[UndergroundLevelBuilder] Build scene: {s.path} (enabled: {s.enabled})");
        }
    }

    private static void Build()
    {
        // core objects
        GameObject gameManager = new GameObject("Game Manager");
        gameManager.AddComponent<GameManager>();

        GameObject music = new GameObject("Music", typeof(AudioSource));
        music.GetComponent<AudioSource>().playOnAwake = false;
        music.AddComponent<LevelMusic>();

        // Mario drops out of the entrance pipe hanging from the ceiling
        Place("Mario", null, 2.5f, -10f);

        GameObject level = new GameObject("Level 1-2");
        Transform underground = Group("Underground", level.transform);
        Transform surface = Group("Surface", level.transform);

        BuildUnderground(underground, out Pipe exitPipe);
        Transform surfacePipe = BuildSurface(surface);

        exitPipe.connection = surfacePipe;
        Record(exitPipe);

        // death barrier below the visible underground area
        GameObject barrier = new GameObject("Death Barrier");
        barrier.transform.SetParent(level.transform, false);
        barrier.transform.position = new Vector3(105f, -18.5f, 0f);
        barrier.AddComponent<DeathBarrier>();
        BoxCollider2D barrierCollider = barrier.GetComponent<BoxCollider2D>();
        barrierCollider.isTrigger = true;
        barrierCollider.size = new Vector2(240f, 1f);

        BuildCamera();
    }

    private static void BuildCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(11.5f, -9.5f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

        SideScrolling scrolling = cameraObject.AddComponent<SideScrolling>();
        scrolling.height = 6.5f;
        scrolling.undergroundHeight = -9.5f;
        scrolling.undergroundThreshold = 0f;
        scrolling.limitRight = true;
        scrolling.rightLimit = 196f;
        scrolling.changeBackground = true;
        scrolling.undergroundBackground = Color.black;
    }

    private static void BuildUnderground(Transform root, out Pipe exitPipe)
    {
        Transform terrain = Group("Terrain", root);
        Transform blocks = Group("Blocks", root);
        Transform coins = Group("Coins", root);
        Transform pipes = Group("Pipes", root);
        Transform enemies = Group("Enemies", root);
        Transform decoration = Group("Decoration", root);
        Transform exit = Group("Exit", root);

        // walls, ceiling and floor (pits at 61-62, 81-83, 101-108, 126-127)
        Tile("UndergroundBlock", terrain, -2, StandRow, 2, 11, "Left Wall");
        Tile("UndergroundBrick", terrain, -2, CeilingRow, 159, 1, "Ceiling");
        Tile("UndergroundBlock", terrain, 159, StandRow, 14, 11, "Right Wall");

        Floor(terrain, -2, 60);
        Floor(terrain, 63, 80);
        Floor(terrain, 84, 100);
        Floor(terrain, 109, 125);
        Floor(terrain, 128, 172);

        // A — entrance and first blocks
        Transform entrance = Place("Pipe", pipes, 2.5f, -8f, "Entrance Pipe").transform;
        entrance.rotation = Quaternion.Euler(0f, 0f, 180f);
        SetPipeLength(entrance.gameObject, 4);
        Record(entrance);

        Coin(coins, 7, -12); Coin(coins, 8, -12); Coin(coins, 9, -12);

        Mystery(blocks, 12, -10, "MagicMushroom");
        Brick(blocks, 13, -10);
        Mystery(blocks, 14, -10, null);
        Brick(blocks, 15, -10);
        Brick(blocks, 16, -10, "BlockCoin", 6);

        Enemy("Goomba", enemies, 19);

        // B — staircase platform and rising pipes
        Tile("UndergroundBlock", terrain, 24, StandRow, 1, 1, "Step");
        Tile("UndergroundBlock", terrain, 25, StandRow, 1, 2, "Step");
        Tile("UndergroundBlock", terrain, 26, StandRow, 1, 3, "Step");
        Tile("UndergroundBlock", terrain, 27, StandRow, 4, 4, "Platform");
        Tile("UndergroundBlock", terrain, 31, StandRow, 1, 3, "Step");
        Tile("UndergroundBlock", terrain, 32, StandRow, 1, 2, "Step");
        Tile("UndergroundBlock", terrain, 33, StandRow, 1, 1, "Step");

        for (int x = 27; x <= 30; x++) {
            Coin(coins, x, -9);
        }

        Enemy("Goomba", enemies, 37);
        Enemy("Goomba", enemies, 39);

        PipeUp(pipes, 43.5f, 2);
        PipeUp(pipes, 49.5f, 3);
        PipeUp(pipes, 55.5f, 4);

        Coin(coins, 46, -8); Coin(coins, 47, -8);
        Coin(coins, 52, -7); Coin(coins, 53, -7);

        Enemy("Goomba", enemies, 52);
        Enemy("Koopa", enemies, 58);

        // C — brick bridge, hidden 1-up and low corridor
        for (int x = 66; x <= 72; x++)
        {
            if (x == 69) {
                Mystery(blocks, x, -10, null);
            } else {
                Brick(blocks, x, -10);
            }
        }

        Coin(coins, 66, -9); Coin(coins, 67, -9); Coin(coins, 71, -9); Coin(coins, 72, -9);
        Mystery(blocks, 69, -6, "1upMushroom");

        Tile("UndergroundBlock", terrain, 88, -9, 9, 6, "Low Ceiling");

        for (int x = 89; x <= 95; x += 2) {
            Coin(coins, x, -12);
        }

        Enemy("Goomba", enemies, 91);
        Enemy("Goomba", enemies, 93);
        Enemy("Koopa", enemies, 99);

        // D — floating platforms over a wide pit and a brick wall
        Tile("UndergroundBlock", terrain, 102, -12, 3, 1, "Floating Platform");
        Tile("UndergroundBlock", terrain, 106, -11, 2, 1, "Floating Platform");

        for (int x = 103; x <= 106; x++) {
            Coin(coins, x, -8);
        }

        for (int y = StandRow; y <= -12; y++) {
            Brick(blocks, 117, y);
        }

        Enemy("Goomba", enemies, 120);
        Enemy("Goomba", enemies, 122);
        Enemy("Koopa", enemies, 124);

        // E — final climb and exit pipe
        for (int x = 130; x <= 134; x++)
        {
            if (x == 132) {
                Mystery(blocks, x, -10, "Starman");
            } else {
                Brick(blocks, x, -10);
            }
        }

        Tile("UndergroundBlock", terrain, 137, StandRow, 1, 1, "Step");
        Tile("UndergroundBlock", terrain, 138, StandRow, 1, 2, "Step");
        Tile("UndergroundBlock", terrain, 139, StandRow, 1, 3, "Step");
        Tile("UndergroundBlock", terrain, 140, StandRow, 1, 4, "Step");
        Tile("UndergroundBlock", terrain, 141, StandRow, 4, 5, "Plateau");

        for (int x = 141; x <= 144; x++) {
            Coin(coins, x, -8);
        }

        Place("Goomba", enemies, 143f, -9f);
        Enemy("Koopa", enemies, 149);
        Enemy("Goomba", enemies, 151);

        exitPipe = Place("PipeConnection", exit, 154.5f, -13.5f, "Exit Pipe").GetComponent<Pipe>();

        // dim background walls, no colliders
        Decoration(decoration, 5, -8, 7, 4);
        Decoration(decoration, 34, -9, 7, 4);
        Decoration(decoration, 74, -8, 6, 4);
        Decoration(decoration, 111, -9, 5, 4);
        Decoration(decoration, 146, -8, 5, 4);
    }

    private static Transform BuildSurface(Transform root)
    {
        Transform terrain = Group("Terrain", root);
        Transform pipes = Group("Pipes", root);
        Transform exit = Group("Exit", root);

        Tile("Ground", terrain, 152, GroundRow, 64, 2, "Ground");

        // exit of the underground warp (connection target)
        Transform surfacePipe = PipeUp(pipes, 166.5f, 2, GroundRow + 1.5f).transform;
        surfacePipe.name = "Surface Pipe";

        for (int i = 0; i < 4; i++) {
            Tile("HardBlock", terrain, 176 + i, GroundRow + 2, 1, i + 1, "Stair");
        }

        Place("HardBlock", exit, 186f, 2f, "Flag Base");
        Transform castle = Place("Castle", exit, 193f, 2f).transform;

        // the next level (2-2) does not exist yet; GameManager.LoadLevel
        // falls back to 1-1 until it is added to Build Profiles
        FlagPole flagPole = Place("Flag Pole", exit, 186f, 2f).GetComponent<FlagPole>();
        flagPole.castle = castle;
        flagPole.nextWorld = 2;
        flagPole.nextStage = 2;
        Record(flagPole);

        return surfacePipe;
    }

    // helpers

    private static Transform Group(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static GameObject Place(string prefab, Transform parent, float x, float y, string name = null)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        instance.transform.position = new Vector3(x, y, 0f);

        if (name != null) {
            instance.name = name;
        }

        Record(instance.transform);
        return instance;
    }

    // places a tiled block prefab whose bottom-left tile center is (x, y)
    private static GameObject Tile(string prefab, Transform parent, int x, int y, int width, int height, string name)
    {
        GameObject tile = Place(prefab, parent, x + (width - 1) / 2f, y + (height - 1) / 2f, name);
        Vector2 size = new Vector2(width, height);

        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        spriteRenderer.size = size;

        BoxCollider2D collider = tile.GetComponent<BoxCollider2D>();
        collider.autoTiling = true;
        collider.size = size;

        Record(spriteRenderer, collider);
        return tile;
    }

    private static void Floor(Transform parent, int fromX, int toX)
    {
        Tile("UndergroundBlock", parent, fromX, FloorRow, toX - fromX + 1, 2, $"Floor {fromX}-{toX}");
    }

    private static void Brick(Transform parent, int x, int y, string item = null, int maxHits = -1)
    {
        GameObject brick = Place("Brick", parent, x, y, "Brick");

        SpriteRenderer spriteRenderer = brick.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = undergroundBrickSprite;

        BlockHit blockHit = brick.GetComponent<BlockHit>();
        blockHit.breakable = true;
        blockHit.maxHits = maxHits;

        if (item != null) {
            blockHit.item = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + item + ".prefab");
        }

        Record(spriteRenderer, blockHit);
    }

    // item null keeps the prefab default (a coin)
    private static void Mystery(Transform parent, int x, int y, string item)
    {
        GameObject block = Place("MysteryBlock", parent, x, y, "Mystery Block");

        if (item != null)
        {
            BlockHit blockHit = block.GetComponent<BlockHit>();
            blockHit.item = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + item + ".prefab");
            Record(blockHit);
        }
    }

    private static void Coin(Transform parent, int x, int y)
    {
        SpriteRenderer spriteRenderer = Place("Coin", parent, x, y).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = undergroundCoinSprite;
        Record(spriteRenderer);
    }

    private static void Enemy(string prefab, Transform parent, int x)
    {
        Place(prefab, parent, x, StandRow);
    }

    // vertical pipe standing on the floor, height counted in tiles
    private static GameObject PipeUp(Transform parent, float x, int height, float floorTop = FloorRow + 1.5f)
    {
        GameObject pipe = Place("Pipe", parent, x, floorTop - 0.5f + height);
        SetPipeLength(pipe, height - 1);
        return pipe;
    }

    // trims the pipe body (12 tiles long in the prefab) to the visible part
    private static void SetPipeLength(GameObject pipe, int length)
    {
        Transform bottom = pipe.transform.Find("Bottom");
        Vector2 size = new Vector2(2f, Mathf.Max(1, length));

        bottom.localPosition = new Vector3(0f, -0.5f - size.y / 2f, 0f);

        SpriteRenderer spriteRenderer = bottom.GetComponent<SpriteRenderer>();
        spriteRenderer.size = size;

        BoxCollider2D collider = bottom.GetComponent<BoxCollider2D>();
        collider.size = size;

        Record(bottom, spriteRenderer, collider);
    }

    private static void Decoration(Transform parent, int x, int y, int width, int height)
    {
        GameObject wall = new GameObject("Back Wall");
        wall.transform.SetParent(parent, false);
        wall.transform.position = new Vector3(x + (width - 1) / 2f, y + (height - 1) / 2f, 0f);

        SpriteRenderer spriteRenderer = wall.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = undergroundBrickSprite;
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.size = new Vector2(width, height);
        spriteRenderer.color = new Color(0.25f, 0.25f, 0.4f);
        spriteRenderer.sortingOrder = -5;
    }

    private static void Record(params Object[] objects)
    {
        foreach (Object o in objects)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(o)) {
                PrefabUtility.RecordPrefabInstancePropertyModifications(o);
            }
        }
    }

}
