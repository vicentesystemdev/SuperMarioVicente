using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds Assets/Scenes/2-2.unity: a long underwater level in 8 sections.
//
// It also configures the import settings of the new sprites in
// Assets/Sprites/Water (16 px per unit, point filter, full rect) and creates
// the new prefabs in Assets/Prefabs/Water (Seafloor, Rock, Coral, Fish,
// Jellyfish). Coordinates follow the 1-1 / 1-2 conventions: tile centers on
// integers and the underwater band uses camera height -9.5, with the
// seabed on rows -16 and -15 and the water surface on row -3.
public static class WaterLevelBuilder
{
    private const string ScenePath = "Assets/Scenes/2-2.unity";
    private const string SpritesFolder = "Assets/Sprites/Water";
    private const string WaterPrefabsFolder = "Assets/Prefabs/Water";
    private const string Prefabs = "Assets/Prefabs/";

    private const int FloorRow = -16;   // seabed rows -16 and -15
    private const int StandRow = -14;   // first free row above the seabed
    private const int TopRow = -4;      // last row below the surface
    private const int SurfaceRow = -3;
    private const int GroundRow = 0;    // surface ground rows 0 and 1
    private const int SectionWidth = 56;
    private const float ExitX = 438.5f;

    private static readonly Color WaterColor = new Color(0.13f, 0.33f, 0.78f);
    private static readonly Color FastFishColor = new Color(1f, 0.55f, 0.45f);

    private class Section
    {
        public int x;
        public Transform terrain;
        public Transform blocks;
        public Transform coins;
        public Transform enemies;
        public Transform decoration;
    }

    [MenuItem("Tools/Super Mario/Crear nivel acuático 2-2")]
    public static void CreateLevel()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[WaterLevelBuilder] Sal del modo Play antes de crear el nivel.");
            return;
        }

        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Nivel 2-2", $"{ScenePath} ya existe. ¿Reemplazarla?", "Reemplazar", "Cancelar")) {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        AssetDatabase.Refresh();

        if (!ConfigureSprites()) {
            return;
        }

        string previousScene = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreatePrefabs();
        Build();

        EditorSceneManager.SaveScene(scene, ScenePath);
        BuildSceneList.Add("Assets/Scenes/1-1.unity", "Assets/Scenes/GameOver.unity", "Assets/Scenes/1-2.unity", ScenePath);

        if (!string.IsNullOrEmpty(previousScene)) {
            EditorSceneManager.OpenScene(previousScene);
        }

        Debug.Log($"[WaterLevelBuilder] Nivel creado en {ScenePath} y registrado en Build Profiles. La bandera de 1-2 ya apunta a 2-2.");
    }

    // sprites and prefabs

    private static bool ConfigureSprites()
    {
        string[] required = {
            "Water_Seafloor", "Water_Rock", "Water_Coral", "Water_Surface", "Water_Seaweed1", "Water_Seaweed2",
            "Fish_Swim1", "Fish_Swim2", "Jellyfish_Contracted", "Jellyfish_Extended", "Water_Bubble",
        };

        foreach (string name in required)
        {
            string path = $"{SpritesFolder}/{name}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError($"[WaterLevelBuilder] Falta {path}. Ejecuta Tools/generate_water_sprites.py.");
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        return true;
    }

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesFolder}/{name}.png");
    }

    private static void CreatePrefabs()
    {
        if (!AssetDatabase.IsValidFolder(WaterPrefabsFolder)) {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Water");
        }

        CreateTilePrefab("Seafloor", LoadSprite("Water_Seafloor"));
        CreateTilePrefab("Rock", LoadSprite("Water_Rock"));
        CreateTilePrefab("Coral", LoadSprite("Water_Coral"));

        // fish
        GameObject fish = CreateEnemyBase("Fish", LoadSprite("Fish_Swim1"));
        AnimatedSprite swim = fish.AddComponent<AnimatedSprite>();
        swim.sprites = new[] { LoadSprite("Fish_Swim1"), LoadSprite("Fish_Swim2") };
        swim.framerate = 0.2f;
        CircleCollider2D fishCollider = fish.AddComponent<CircleCollider2D>();
        fishCollider.isTrigger = true;
        fishCollider.radius = 0.4f;
        fish.AddComponent<Fish>();
        SavePrefab(fish);

        // jellyfish
        GameObject jellyfish = CreateEnemyBase("Jellyfish", LoadSprite("Jellyfish_Contracted"));
        BoxCollider2D jellyCollider = jellyfish.AddComponent<BoxCollider2D>();
        jellyCollider.isTrigger = true;
        jellyCollider.size = new Vector2(0.8f, 1.2f);
        Jellyfish jelly = jellyfish.AddComponent<Jellyfish>();
        jelly.contracted = LoadSprite("Jellyfish_Contracted");
        jelly.extended = LoadSprite("Jellyfish_Extended");
        jelly.minY = StandRow + 0.5f;
        jelly.maxY = TopRow - 0.5f;
        SavePrefab(jellyfish);
    }

    private static void CreateTilePrefab(string name, Sprite sprite)
    {
        GameObject tile = new GameObject(name);

        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.size = Vector2.one;

        BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
        collider.autoTiling = true;
        collider.size = Vector2.one;

        SavePrefab(tile);
    }

    private static GameObject CreateEnemyBase(string name, Sprite sprite)
    {
        GameObject enemy = new GameObject(name);
        enemy.layer = LayerMask.NameToLayer("Enemy");

        SpriteRenderer spriteRenderer = enemy.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 3;

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        DeathAnimation death = enemy.AddComponent<DeathAnimation>();
        death.spriteRenderer = spriteRenderer;
        death.enabled = false;

        return enemy;
    }

    private static void SavePrefab(GameObject gameObject)
    {
        PrefabUtility.SaveAsPrefabAsset(gameObject, $"{WaterPrefabsFolder}/{gameObject.name}.prefab");
        Object.DestroyImmediate(gameObject);
    }

    // scene

    private static void Build()
    {
        GameObject gameManager = new GameObject("Game Manager");
        gameManager.AddComponent<GameManager>();

        GameObject music = new GameObject("Music", typeof(AudioSource));
        music.GetComponent<AudioSource>().playOnAwake = false;
        music.AddComponent<LevelMusic>().theme = LevelMusic.Theme.Water;

        Place("Mario", null, 3f, -7f);

        GameObject level = new GameObject("Level 2-2");
        Transform underwater = Group("Underwater", level.transform);
        Transform surface = Group("Surface", level.transform);

        BuildBounds(Group("Seabed and Walls", underwater));
        BuildWater(underwater);

        Section1(NewSection(underwater, 0, "1 - Arrecife de entrada"));
        Section2(NewSection(underwater, 1, "2 - Bosque de coral"));
        Section3(NewSection(underwater, 2, "3 - La fosa"));
        Section4(NewSection(underwater, 3, "4 - Corredor estrecho"));
        Section5(NewSection(underwater, 4, "5 - Laberinto de cavernas"));
        Section6(NewSection(underwater, 5, "6 - Corriente de peces"));
        Section7(NewSection(underwater, 6, "7 - Ruinas sumergidas"));
        Section8(NewSection(underwater, 7, "8 - Salida"));

        Pipe exitPipe = Place("PipeConnection", Group("Exit", underwater), ExitX, -13.5f, "Exit Pipe").GetComponent<Pipe>();
        exitPipe.connection = BuildSurface(surface);
        Record(exitPipe);

        // death barrier below the trenches
        GameObject barrier = new GameObject("Death Barrier");
        barrier.transform.SetParent(level.transform, false);
        barrier.transform.position = new Vector3(227f, -18.5f, 0f);
        barrier.AddComponent<DeathBarrier>();
        BoxCollider2D barrierCollider = barrier.GetComponent<BoxCollider2D>();
        barrierCollider.isTrigger = true;
        barrierCollider.size = new Vector2(470f, 1f);

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
        camera.backgroundColor = WaterColor;

        SideScrolling scrolling = cameraObject.AddComponent<SideScrolling>();
        scrolling.height = 6.5f;
        scrolling.undergroundHeight = -9.5f;
        scrolling.undergroundThreshold = 0f;
        scrolling.limitRight = true;
        scrolling.rightLimit = 480f;
        scrolling.changeBackground = true;
        scrolling.undergroundBackground = WaterColor;
    }

    private static void BuildBounds(Transform parent)
    {
        // seabed with two trenches (120-125 and 136-143)
        Floor(parent, -2, 119);
        Floor(parent, 126, 135);
        Floor(parent, 144, 456);

        Tile("Water/Rock", parent, -2, StandRow, 2, TopRow - StandRow + 1, "Left Wall");
        Tile("Water/Rock", parent, 443, StandRow, 14, TopRow - StandRow + 1, "Right Wall");
    }

    private static void BuildWater(Transform parent)
    {
        // the area where Mario swims: from the bottom of the screen to the surface
        // on Ignore Raycast so Mario's ground / wall checks do not hit it
        GameObject water = new GameObject("Water Zone");
        water.layer = LayerMask.NameToLayer("Ignore Raycast");
        water.transform.SetParent(parent, false);
        water.transform.position = new Vector3(220f, -10f, 0f);

        WaterZone zone = water.AddComponent<WaterZone>();
        zone.bubbleSprite = LoadSprite("Water_Bubble");

        BoxCollider2D area = water.GetComponent<BoxCollider2D>();
        area.isTrigger = true;
        area.size = new Vector2(445f, 13f);

        // animated look of the surface (decoration only)
        GameObject waves = new GameObject("Water Surface");
        waves.transform.SetParent(parent, false);
        waves.transform.position = new Vector3(218.5f, SurfaceRow, 0f);

        SpriteRenderer spriteRenderer = waves.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = LoadSprite("Water_Surface");
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.size = new Vector2(442f, 1f);
        spriteRenderer.sortingOrder = 2;
    }

    private static Transform BuildSurface(Transform root)
    {
        Transform terrain = Group("Terrain", root);
        Transform exit = Group("Exit", root);

        Tile("Ground", terrain, 432, GroundRow, 68, 2, "Ground");

        // exit of the underwater warp (connection target)
        GameObject surfacePipe = Place("Pipe", Group("Pipes", root), 450.5f, GroundRow + 3f, "Surface Pipe");
        SetPipeLength(surfacePipe, 1);

        for (int i = 0; i < 4; i++) {
            Tile("HardBlock", terrain, 460 + i, GroundRow + 2, 1, i + 1, "Stair");
        }

        Place("HardBlock", exit, 470f, 2f, "Flag Base");
        Transform castle = Place("Castle", exit, 477f, 2f).transform;

        // the Bowser castle (8-4) does not exist yet; GameManager.LoadLevel
        // falls back to 1-1 until it is added to Build Profiles
        FlagPole flagPole = Place("Flag Pole", exit, 470f, 2f).GetComponent<FlagPole>();
        flagPole.castle = castle;
        flagPole.nextWorld = 8;
        flagPole.nextStage = 4;
        Record(flagPole);

        return surfacePipe.transform;
    }

    // sections (x positions are relative to the section start)

    private static void Section1(Section s)
    {
        Mystery(s, 8, -10, null);

        Coral(s, 14, 2);
        Coral(s, 24, 3);
        Coral(s, 38, 2);
        Coral(s, 48, 3);

        Coin(s, 16, -10); Coin(s, 17, -9); Coin(s, 18, -9); Coin(s, 19, -9); Coin(s, 20, -10);
        Coin(s, 24, -10); Coin(s, 38, -11); Coin(s, 48, -10);

        AddFish(s, 34, -8f, 1.5f, 0.3f);
        AddFish(s, 50, -11f, 1.5f, 0.5f);

        Seaweed(s, 6); Seaweed(s, 10); Seaweed(s, 30); Seaweed(s, 44);
    }

    private static void Section2(Section s)
    {
        Coral(s, 4, 5, 2);
        Coral(s, 16, 6);
        Coral(s, 28, 5, 2);
        Coral(s, 40, 6);

        Hang(s, 10, 2, 4);
        Hang(s, 22, 2, 5);
        Hang(s, 34, 2, 4);
        Hang(s, 46, 2, 5);

        Coin(s, 7, -11); Coin(s, 7, -10);
        Coin(s, 19, -12); Coin(s, 19, -11);
        Coin(s, 25, -6);
        Coin(s, 31, -8); Coin(s, 31, -7);
        Coin(s, 43, -12); Coin(s, 43, -11);
        Coin(s, 49, -13);

        AddFish(s, 12, -6f, 2f, 0.5f);
        AddFish(s, 26, -12f, 2f, 0.4f);
        AddFish(s, 38, -9f, 2.2f, 0.6f);
        AddFish(s, 52, -6f, 2f, 0.5f);
        AddJellyfish(s, 30, -8f);

        Seaweed(s, 2); Seaweed(s, 14); Seaweed(s, 26); Seaweed(s, 38); Seaweed(s, 50);
    }

    private static void Section3(Section s)
    {
        // trenches at 8-13 and 24-31 (see BuildBounds)
        Tile("Water/Rock", s.terrain, s.x + 16, -10, 4, 1, "Floating Rock");
        Mystery(s, 17, -6, "MagicMushroom");
        Hang(s, 36, 3, 3);

        Coin(s, 10, -11); Coin(s, 11, -11); Coin(s, 11, -15);
        Coin(s, 27, -11); Coin(s, 28, -11);
        Coin(s, 18, -9);

        AddFish(s, 10, -10f, 2.2f, 0.8f);
        AddFish(s, 28, -10f, 2.2f, 0.8f);
        AddFish(s, 44, -6f, 2.5f, 0.3f);
        AddFish(s, 50, -12f, 2.3f, 0.5f);
        AddJellyfish(s, 40, -9f);

        Seaweed(s, 2); Seaweed(s, 20); Seaweed(s, 34); Seaweed(s, 48);
    }

    private static void Section4(Section s)
    {
        // rock band splitting the water in an upper and a lower corridor
        Tile("Water/Rock", s.terrain, s.x + 4, -10, 40, 3, "Rock Band");

        Coral(s, 14, 1);
        Hang(s, 26, 2, 1);

        Mystery(s, 10, -11, null);
        Mystery(s, 30, -11, null);

        for (int x = 8; x <= 36; x += 4) {
            Coin(s, x, -6);
        }

        Coin(s, 20, -13); Coin(s, 22, -13); Coin(s, 24, -13);

        AddFish(s, 40, -5.5f, 2.5f, 0.2f);
        AddFish(s, 44, -12.5f, 2.5f, 0.2f);
        AddFish(s, 52, -8f, 2f, 0.5f);

        Seaweed(s, 6); Seaweed(s, 36); Seaweed(s, 48);
    }

    private static void Section5(Section s)
    {
        // zig-zag maze: gaps alternate between the top and the bottom
        Tile("Water/Rock", s.terrain, s.x + 6, StandRow, 2, 8, "Maze Wall");
        Hang(s, 18, 2, 8);
        Tile("Water/Rock", s.terrain, s.x + 30, StandRow, 2, 7, "Maze Wall");
        Hang(s, 42, 2, 7);

        Mystery(s, 24, -10, "Starman");

        Coin(s, 7, -5);
        Coin(s, 12, -12); Coin(s, 12, -11); Coin(s, 12, -10);
        Coin(s, 19, -13);
        Coin(s, 24, -6); Coin(s, 25, -6);
        Coin(s, 31, -6);
        Coin(s, 36, -12); Coin(s, 36, -11);
        Coin(s, 43, -12);
        Coin(s, 48, -7); Coin(s, 48, -6);

        AddJellyfish(s, 12, -9f);
        AddJellyfish(s, 36, -8f);
        AddFish(s, 26, -12f, 2f, 0.3f);
        AddFish(s, 50, -6f, 2f, 0.4f);

        Seaweed(s, 3); Seaweed(s, 24); Seaweed(s, 36); Seaweed(s, 52);
    }

    private static void Section6(Section s)
    {
        Coral(s, 10, 2);
        Coral(s, 30, 3);
        Coral(s, 46, 2);

        // school of fish with different heights, speeds and waves
        AddFish(s, 8, -6f, 3f, 0.4f);
        AddFish(s, 14, -11f, 3.5f, 0.2f);
        AddFish(s, 20, -8f, 2.8f, 0.6f);
        AddFish(s, 26, -13f, 3.2f, 0.3f);
        AddFish(s, 32, -5f, 3.6f, 0.2f);
        AddFish(s, 38, -10f, 3f, 0.5f);
        AddFish(s, 44, -7f, 3.4f, 0.4f);
        AddFish(s, 50, -12f, 3f, 0.3f);
        AddJellyfish(s, 36, -9f);

        Coin(s, 12, -9); Coin(s, 13, -9);
        Coin(s, 24, -6); Coin(s, 25, -6);
        Coin(s, 36, -12); Coin(s, 37, -12);
        Coin(s, 48, -8); Coin(s, 49, -8);

        Seaweed(s, 4); Seaweed(s, 18); Seaweed(s, 26); Seaweed(s, 40); Seaweed(s, 52);
    }

    private static void Section7(Section s)
    {
        // arch of a sunken ruin with blocks on top
        Tile("Water/Rock", s.terrain, s.x + 8, StandRow, 3, 6, "Ruin Pillar");
        Tile("Water/Rock", s.terrain, s.x + 22, StandRow, 3, 6, "Ruin Pillar");
        Tile("Water/Rock", s.terrain, s.x + 8, -8, 17, 1, "Ruin Lintel");

        Mystery(s, 14, -5, null);
        Mystery(s, 16, -5, "1upMushroom");
        Mystery(s, 18, -5, null);

        for (int x = 13; x <= 19; x++) {
            Coin(s, x, -12);
        }

        // broken staircase
        for (int i = 0; i < 4; i++)
        {
            Tile("Water/Rock", s.terrain, s.x + 34 + i * 2, StandRow, 2, i + 1, "Ruin Step");
            Coin(s, 35 + i * 2, -12 + i);
        }

        AddJellyfish(s, 30, -9f);
        AddJellyfish(s, 50, -8f);
        AddFish(s, 46, -5f, 2.4f, 0.3f);
        AddFish(s, 54, -12f, 2.4f, 0.4f);

        Seaweed(s, 2); Seaweed(s, 28); Seaweed(s, 46);
    }

    private static void Section8(Section s)
    {
        Coral(s, 18, 3);
        Hang(s, 28, 2, 4);

        Coin(s, 10, -12); Coin(s, 11, -12);
        for (int x = 40; x <= 44; x++) {
            Coin(s, x, -13);
        }

        AddJellyfish(s, 8, -8f);
        AddJellyfish(s, 22, -9f);
        AddFish(s, 14, -6f, 3f, 0.4f);
        AddFish(s, 26, -12f, 2.6f, 0.3f);
        AddFish(s, 34, -9f, 3.4f, 0.5f);

        Seaweed(s, 4); Seaweed(s, 12); Seaweed(s, 36);
    }

    // helpers

    private static Section NewSection(Transform parent, int index, string name)
    {
        Transform root = Group(name, parent);

        return new Section {
            x = index * SectionWidth,
            terrain = Group("Terrain", root),
            blocks = Group("Blocks", root),
            coins = Group("Coins", root),
            enemies = Group("Enemies", root),
            decoration = Group("Decoration", root),
        };
    }

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
        Tile("Water/Seafloor", parent, fromX, FloorRow, toX - fromX + 1, 2, $"Seabed {fromX}-{toX}");
    }

    private static void Coral(Section s, int x, int height, int width = 1)
    {
        Tile("Water/Coral", s.terrain, s.x + x, StandRow, width, height, "Coral");
    }

    // rock formation hanging from the surface
    private static void Hang(Section s, int x, int width, int height)
    {
        Tile("Water/Rock", s.terrain, s.x + x, TopRow - height + 1, width, height, "Hanging Rock");
    }

    private static void Coin(Section s, int x, int y)
    {
        Place("Coin", s.coins, s.x + x, y);
    }

    // item null keeps the prefab default (a coin)
    private static void Mystery(Section s, int x, int y, string item)
    {
        GameObject block = Place("MysteryBlock", s.blocks, s.x + x, y, "Mystery Block");

        if (item != null)
        {
            BlockHit blockHit = block.GetComponent<BlockHit>();
            blockHit.item = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + item + ".prefab");
            Record(blockHit);
        }
    }

    private static void AddFish(Section s, int x, float y, float speed, float amplitude)
    {
        GameObject fish = Place("Water/Fish", s.enemies, s.x + x, y);

        Fish component = fish.GetComponent<Fish>();
        component.speed = speed;
        component.waveAmplitude = amplitude;
        Record(component);

        // faster fish are tinted red so the player can read the danger
        if (speed >= 3.3f)
        {
            SpriteRenderer spriteRenderer = fish.GetComponent<SpriteRenderer>();
            spriteRenderer.color = FastFishColor;
            Record(spriteRenderer);
        }
    }

    private static void AddJellyfish(Section s, int x, float y)
    {
        Place("Water/Jellyfish", s.enemies, s.x + x, y);
    }

    private static void Seaweed(Section s, int x)
    {
        GameObject seaweed = new GameObject("Seaweed");
        seaweed.transform.SetParent(s.decoration, false);
        seaweed.transform.position = new Vector3(s.x + x, StandRow + 0.5f, 0f);

        SpriteRenderer spriteRenderer = seaweed.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = LoadSprite("Water_Seaweed1");
        spriteRenderer.sortingOrder = -2;

        AnimatedSprite sway = seaweed.AddComponent<AnimatedSprite>();
        sway.sprites = new[] { LoadSprite("Water_Seaweed1"), LoadSprite("Water_Seaweed2") };
        sway.framerate = 0.5f;
    }

    // vertical pipe: trims the body (12 tiles long in the prefab)
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
