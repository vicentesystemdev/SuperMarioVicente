using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds Assets/Scenes/8-4.unity: the final castle of original design, in
// 8 sections, plus the Victory scene. It also configures the import settings
// of the new sprites in Assets/Sprites/Castle and creates the prefabs in
// Assets/Prefabs/Castle.
//
// Layout (tile centers on integers, floor rows 0-1 with top at y = 1.5):
//   lower floor  x -2..287, ceiling rows 12-13          camera zone A
//   tower        x 246..287, rows 0..41 (vertical climb) camera zone B
//   upper floor  x 276..402, rows 28-29, ceiling 40-41  camera zone C
public static class CastleLevelBuilder
{
    private const string ScenePath = "Assets/Scenes/8-4.unity";
    private const string SpritesFolder = "Assets/Sprites/Castle";
    private const string PrefabFolder = "Assets/Prefabs/Castle";
    private const string Prefabs = "Assets/Prefabs/";

    private const float LowerStand = 2f;     // Mario / enemies on the lower floor
    private const float UpperStand = 30f;    // Mario / enemies on the upper floor
    private const float LowerFloorTop = 1.5f;
    private const float UpperFloorTop = 29.5f;

    private static readonly Color BackgroundColor = new Color(0.06f, 0.04f, 0.08f);

    private static LevelMusic music;
    private static BossHealthBar healthBar;

    private class Section
    {
        public Transform terrain;
        public Transform hazards;
        public Transform blocks;
        public Transform coins;
        public Transform enemies;
        public Transform decoration;
    }

    [MenuItem("Tools/Super Mario/Crear Castillo de Bowser 8-4")]
    public static void CreateLevel()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[CastleLevelBuilder] Sal del modo Play antes de crear el nivel.");
            return;
        }

        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Castillo 8-4", $"{ScenePath} ya existe. ¿Reemplazarla? Se perderán los cambios hechos a mano en esa escena.", "Reemplazar", "Cancelar")) {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        AssetDatabase.Refresh();

        if (!SpriteImport.Configure(SpritesFolder, "Castle_Stone")) {
            return;
        }

        string previousScene = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreatePrefabs();
        Build();
        EditorSceneManager.SaveScene(scene, ScenePath);

        // the victory screen is only created if missing or confirmed
        VictorySceneBuilder.CreateScene();

        BuildSceneList.Add(
            "Assets/Scenes/1-1.unity", "Assets/Scenes/GameOver.unity", "Assets/Scenes/1-2.unity",
            "Assets/Scenes/2-2.unity", ScenePath, VictorySceneBuilder.ScenePath);

        if (!string.IsNullOrEmpty(previousScene)) {
            EditorSceneManager.OpenScene(previousScene);
        }

        Debug.Log($"[CastleLevelBuilder] Castillo creado en {ScenePath} y registrado en Build Profiles. La bandera de 2-2 ya apunta a 8-4.");
    }

    // ------------------------------------------------------------ prefabs

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesFolder}/{name}.png");
    }

    private static void CreatePrefabs()
    {
        if (!AssetDatabase.IsValidFolder(PrefabFolder)) {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Castle");
        }

        SavePrefab(TileObject("Stone", LoadSprite("Castle_Stone"), true, 0));
        SavePrefab(TileObject("Brick", LoadSprite("Castle_Brick"), true, 0));
        SavePrefab(TileObject("BrickBack", LoadSprite("Castle_BrickBack"), false, -10));

        // lava: animated surface, kills on contact
        GameObject lava = TileObject("Lava", LoadSprite("Lava1"), true, 1);
        Animate(lava, 0.35f, LoadSprite("Lava1"), LoadSprite("Lava2"));
        MakeHazard(lava, true);
        SavePrefab(lava);

        foreach (string name in new[] { "Spikes", "Spikes_Down" })
        {
            GameObject spikes = TileObject(name, LoadSprite(name), true, 1);
            MakeHazard(spikes, false);
            SavePrefab(spikes);
        }

        GameObject crumble = TileObject("Crumble", LoadSprite("Castle_Crumble"), true, 0);
        crumble.AddComponent<CrumblingPlatform>();
        SavePrefab(crumble);

        GameObject platform = TileObject("MovingPlatform", LoadSprite("Castle_Platform"), true, 0);
        platform.GetComponent<BoxCollider2D>().autoTiling = false;
        platform.GetComponent<BoxCollider2D>().size = new Vector2(1f, 0.5f);
        platform.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        platform.AddComponent<MovingPlatform>();
        SavePrefab(platform);

        GameObject gate = TileObject("Gate", LoadSprite("Gate"), true, 2);
        gate.AddComponent<Gate>();
        SavePrefab(gate);

        GameObject torch = SpriteObject("Torch", LoadSprite("Torch1"), -5);
        Animate(torch, 0.15f, LoadSprite("Torch1"), LoadSprite("Torch2"));
        SavePrefab(torch);

        SavePrefab(SpriteObject("Door", LoadSprite("Door"), -5));

        // projectiles
        SavePrefab(ProjectileObject("Fireball", 1.5f, LoadSprite("Fireball1"), LoadSprite("Fireball2")));
        SavePrefab(ProjectileObject("BossFireball", 1f, LoadSprite("Boss_Fireball1"), LoadSprite("Boss_Fireball2")));

        GameObject shockwave = ProjectileObject("Shockwave", 1f, LoadSprite("Shockwave1"), LoadSprite("Shockwave2"));
        Object.DestroyImmediate(shockwave.GetComponent<CircleCollider2D>());
        BoxCollider2D waveCollider = shockwave.AddComponent<BoxCollider2D>();
        waveCollider.isTrigger = true;
        waveCollider.size = new Vector2(0.8f, 0.7f);
        waveCollider.offset = new Vector2(0f, -0.15f);
        shockwave.GetComponent<Projectile>().lifetime = 4f;
        SavePrefab(shockwave);

        // lava spark
        GameObject spark = SpriteObject("LavaSpark", LoadSprite("Fireball1"), 0);
        spark.transform.localScale = Vector3.one * 1.8f;
        Animate(spark, 0.1f, LoadSprite("Fireball1"), LoadSprite("Fireball2"));
        spark.AddComponent<CircleCollider2D>().radius = 0.2f;
        spark.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        MakeHazard(spark, false);
        spark.AddComponent<LavaSpark>();
        SavePrefab(spark);

        // fire jet: solid nozzle and a flame child that toggles
        GameObject jet = SpriteObject("FireJet", LoadSprite("Castle_Stone"), 0);
        jet.AddComponent<BoxCollider2D>();
        GameObject flame = SpriteObject("Flame", LoadSprite("Flame1"), 3);
        flame.transform.SetParent(jet.transform, false);
        flame.transform.localPosition = new Vector3(0f, 2f, 0f);
        Animate(flame, 0.08f, LoadSprite("Flame1"), LoadSprite("Flame2"));
        flame.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 2.8f);
        MakeHazard(flame, false);
        jet.AddComponent<FireJet>().flame = flame.transform;
        SavePrefab(jet);

        GameObject cannon = SpriteObject("Cannon", LoadSprite("Cannon"), 0);
        cannon.AddComponent<BoxCollider2D>();
        cannon.AddComponent<FireCannon>().projectilePrefab = LoadPrefab<Projectile>("Castle/Fireball");
        SavePrefab(cannon);

        GameObject checkpoint = SpriteObject("Checkpoint", LoadSprite("Checkpoint_Off"), -1);
        checkpoint.AddComponent<BoxCollider2D>().size = new Vector2(1f, 2f);
        Checkpoint checkpointComponent = checkpoint.AddComponent<Checkpoint>();
        checkpointComponent.inactiveSprite = LoadSprite("Checkpoint_Off");
        checkpointComponent.activeSprite = LoadSprite("Checkpoint_On");
        SetLayer(checkpoint, "Ignore Raycast");
        SavePrefab(checkpoint);

        GameObject trophy = SpriteObject("Trophy", LoadSprite("Trophy"), 4);
        trophy.AddComponent<BoxCollider2D>();
        trophy.AddComponent<VictoryGoal>();
        SetLayer(trophy, "Ignore Raycast");
        SavePrefab(trophy);

        // boss
        GameObject boss = SpriteObject("Boss", LoadSprite("Boss_Idle1"), 2);
        SetLayer(boss, "Enemy");
        boss.AddComponent<BoxCollider2D>().size = new Vector2(1.8f, 1.8f);
        Rigidbody2D bossBody = boss.AddComponent<Rigidbody2D>();
        bossBody.bodyType = RigidbodyType2D.Dynamic;
        bossBody.gravityScale = 0f;
        bossBody.mass = 50f;
        bossBody.freezeRotation = true;
        bossBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        Boss bossComponent = boss.AddComponent<Boss>();
        bossComponent.idle1 = LoadSprite("Boss_Idle1");
        bossComponent.idle2 = LoadSprite("Boss_Idle2");
        bossComponent.attack = LoadSprite("Boss_Attack");
        bossComponent.defeated = LoadSprite("Boss_Defeated");
        bossComponent.fireballPrefab = LoadPrefab<Projectile>("Castle/BossFireball");
        bossComponent.shockwavePrefab = LoadPrefab<Projectile>("Castle/Shockwave");
        SavePrefab(boss);
    }

    private static GameObject SpriteObject(string name, Sprite sprite, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name);
        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = sortingOrder;
        return gameObject;
    }

    private static GameObject TileObject(string name, Sprite sprite, bool solid, int sortingOrder)
    {
        GameObject tile = SpriteObject(name, sprite, sortingOrder);
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.size = Vector2.one;

        if (solid)
        {
            BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
            collider.autoTiling = true;
            collider.size = Vector2.one;
        }

        return tile;
    }

    private static GameObject ProjectileObject(string name, float scale, params Sprite[] frames)
    {
        GameObject projectile = SpriteObject(name, frames[0], 5);
        projectile.transform.localScale = Vector3.one * scale;
        Animate(projectile, 0.08f, frames);
        projectile.AddComponent<CircleCollider2D>().radius = 0.2f;
        projectile.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        MakeHazard(projectile, false);
        projectile.AddComponent<Projectile>();
        return projectile;
    }

    private static void Animate(GameObject gameObject, float framerate, params Sprite[] frames)
    {
        AnimatedSprite animated = gameObject.AddComponent<AnimatedSprite>();
        animated.sprites = frames;
        animated.framerate = framerate;
    }

    private static void MakeHazard(GameObject gameObject, bool instantKill)
    {
        gameObject.GetComponent<Collider2D>().isTrigger = true;
        gameObject.AddComponent<Hazard>().instantKill = instantKill;
        SetLayer(gameObject, "Ignore Raycast");
    }

    private static void SetLayer(GameObject gameObject, string layer)
    {
        gameObject.layer = LayerMask.NameToLayer(layer);
    }

    private static void SavePrefab(GameObject gameObject)
    {
        PrefabUtility.SaveAsPrefabAsset(gameObject, $"{PrefabFolder}/{gameObject.name}.prefab");
        Object.DestroyImmediate(gameObject);
    }

    private static T LoadPrefab<T>(string path) where T : Component
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + path + ".prefab").GetComponent<T>();
    }

    // ------------------------------------------------------------ scene

    private static void Build()
    {
        GameObject gameManager = new GameObject("Game Manager");
        gameManager.AddComponent<GameManager>();

        GameObject musicObject = new GameObject("Music", typeof(AudioSource));
        musicObject.GetComponent<AudioSource>().playOnAwake = false;
        music = musicObject.AddComponent<LevelMusic>();
        music.theme = LevelMusic.Theme.Castle;

        Place("Mario", null, 3f, LowerStand);

        BuildCamera();
        healthBar = BuildBossUI();

        GameObject level = new GameObject("Level 8-4");
        BuildStructure(Group("Structure", level.transform));

        Section1(NewSection(level.transform, "1 - Entrada al castillo"));
        Section2(NewSection(level.transform, "2 - Pasillo de trampas"));
        Section3(NewSection(level.transform, "3 - Zona de lava"));
        Section4(NewSection(level.transform, "4 - Camaras de fuego"));
        Section5(NewSection(level.transform, "5 - Torre interior"));
        Section6(NewSection(level.transform, "6 - Antesala del jefe"));
        Section7(NewSection(level.transform, "7 - Sala del jefe"));
        Section8(NewSection(level.transform, "8 - Sala del tesoro"));

        GameObject barrier = new GameObject("Death Barrier");
        barrier.transform.SetParent(level.transform, false);
        barrier.transform.position = new Vector3(200f, -4f, 0f);
        barrier.AddComponent<DeathBarrier>();
        BoxCollider2D barrierCollider = barrier.GetComponent<BoxCollider2D>();
        barrierCollider.isTrigger = true;
        barrierCollider.size = new Vector2(520f, 1f);
    }

    private static void BuildCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(11.5f, 6.5f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = BackgroundColor;

        CastleCamera castleCamera = cameraObject.AddComponent<CastleCamera>();
        castleCamera.minX = -2.5f;
        castleCamera.maxX = 402.5f;
        castleCamera.zones = new[] {
            new CastleCamera.Zone { fromX = -1000f, toX = 246f, minY = -0.5f, maxY = 13.5f },
            new CastleCamera.Zone { fromX = 246f, toX = 288f, minY = -0.5f, maxY = 41.5f },
            new CastleCamera.Zone { fromX = 288f, toX = 1000f, minY = 27.5f, maxY = 41.5f },
        };
    }

    private static BossHealthBar BuildBossUI()
    {
        GameObject canvasObject = new GameObject("Boss UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform panel = UIRect("Health Panel", canvasObject.transform);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = new Vector2(0f, -30f);
        panel.sizeDelta = new Vector2(700f, 120f);
        CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        RectTransform label = UIRect("Label", panel);
        label.gameObject.AddComponent<RawImage>().raycastTarget = false;
        PixelText text = label.gameObject.AddComponent<PixelText>();
        text.text = "MAGMOR";
        text.pixelSize = 6f;
        text.color = new Color(1f, 0.85f, 0.4f);
        text.Refresh();
        label.anchoredPosition = new Vector2(0f, 25f);

        RectTransform bar = UIRect("Bar", panel);
        bar.sizeDelta = new Vector2(640f, 32f);
        bar.anchoredPosition = new Vector2(0f, -28f);
        Image barImage = bar.gameObject.AddComponent<Image>();
        barImage.color = new Color(0.12f, 0.08f, 0.1f, 0.9f);
        barImage.raycastTarget = false;

        RectTransform fill = UIRect("Fill", bar);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = new Vector2(4f, 4f);
        fill.offsetMax = new Vector2(-4f, -4f);
        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = new Color(1f, 0.35f, 0.1f);
        fillImage.raycastTarget = false;

        BossHealthBar bossBar = panel.gameObject.AddComponent<BossHealthBar>();
        bossBar.group = group;
        bossBar.fill = fill;
        group.alpha = 0f;

        return bossBar;
    }

    private static void BuildStructure(Transform parent)
    {
        // lower floor (lava pits at 30-31 and 122-179) and ceiling
        Tile("Castle/Stone", parent, -2, 0, 32, 2, "Floor");
        Tile("Castle/Stone", parent, 32, 0, 84, 2, "Floor");
        Tile("Castle/Stone", parent, 116, 0, 6, 2, "Floor");
        Tile("Castle/Stone", parent, 180, 0, 108, 2, "Floor");
        Tile("Castle/Brick", parent, -2, 12, 248, 2, "Ceiling");
        Tile("Castle/Brick", parent, -2, 2, 2, 10, "Left Wall");

        Lava(parent, 30, 31);
        Lava(parent, 122, 179);

        // tower walls and the masonry around it (keeps the camera inside the map)
        Tile("Castle/Brick", parent, 246, 6, 2, 34, "Tower Left Wall");
        Tile("Castle/Brick", parent, 230, 14, 16, 28, "Masonry");
        Tile("Castle/Brick", parent, 286, 2, 2, 26, "Tower Right Wall");
        Tile("Castle/Brick", parent, 288, 2, 18, 26, "Masonry");

        // upper floor, ceiling and the final wall
        Tile("Castle/Stone", parent, 276, 28, 127, 2, "Upper Floor");
        Tile("Castle/Brick", parent, 230, 40, 173, 2, "Upper Ceiling");
        Tile("Castle/Brick", parent, 400, 30, 3, 10, "Right Wall");

        // dark brick backdrops (decoration)
        Tile("Castle/BrickBack", parent, -2, 2, 248, 10, "Backdrop");
        Tile("Castle/BrickBack", parent, 248, 2, 38, 38, "Backdrop");
        Tile("Castle/BrickBack", parent, 288, 30, 112, 10, "Backdrop");
    }

    // ------------------------------------------------------------ sections

    private static void Section1(Section s)
    {
        Place("Castle/Door", s.decoration, 3.5f, LowerFloorTop + 1.5f, "Entrance Door");
        Torch(s, 8, 7); Torch(s, 20, 7); Torch(s, 34, 7); Torch(s, 48, 7);

        Mystery(s, 12, 5, "MagicMushroom");
        Mystery(s, 13, 5, null);

        Tile("Castle/Stone", s.terrain, 22, 5, 4, 1, "Ledge");
        for (int x = 22; x <= 25; x++) {
            Coin(s, x, 7);
        }

        // first lava pit with a spark
        Spark(s, 30.5f, 5f, 3f, 0f);
        Coin(s, 30, 6); Coin(s, 31, 6);

        Tile("Castle/Brick", s.terrain, 38, 8, 7, 4, "Low Ceiling");

        Enemy(s, "Goomba", 18, LowerStand);
        Enemy(s, "Goomba", 26, LowerStand);
        Enemy(s, "Goomba", 40, LowerStand);
        Enemy(s, "Koopa", 47, LowerStand);
    }

    private static void Section2(Section s)
    {
        Torch(s, 60, 7); Torch(s, 84, 7); Torch(s, 100, 7);

        Spikes(s, 62, 2, 3, false);

        Mystery(s, 66, 5, null);
        Mystery(s, 67, 5, null);
        Enemy(s, "Goomba", 69, LowerStand);

        // spike field crossed on crumbling blocks
        Spikes(s, 74, 2, 8, false);
        foreach (int x in new[] { 75, 76, 78, 79 })
        {
            Crumble(s, x, 5);
            Coin(s, x, 7);
        }

        // low corridor with spikes in the ceiling: no high jumps here
        Tile("Castle/Brick", s.terrain, 86, 7, 11, 5, "Spiked Ceiling");
        Spikes(s, 86, 6, 11, true);
        Enemy(s, "Goomba", 92, LowerStand);

        // fire jets with staggered timing
        AddFireJet(s, 101, 0f);
        AddFireJet(s, 106, 0.9f);
        AddFireJet(s, 111, 1.8f);
        Coin(s, 103, 3); Coin(s, 104, 3); Coin(s, 108, 3); Coin(s, 109, 3);
    }

    private static void Section3(Section s)
    {
        AddCheckpoint(s, 117, LowerFloorTop);
        Torch(s, 119, 7); Torch(s, 140, 9); Torch(s, 170, 9); Torch(s, 183, 7);

        // pillars rising from the lava, moving platforms and a crumbling bridge
        Tile("Castle/Stone", s.terrain, 125, 0, 2, 4, "Pillar");
        Tile("Castle/Stone", s.terrain, 131, 0, 2, 5, "Pillar");
        AddMovingPlatform(s, 136, 4, 3, new Vector2(9f, 0f), 4.5f, 0f);
        Tile("Castle/Stone", s.terrain, 150, 0, 2, 6, "Pillar");
        Tile("Castle/Stone", s.terrain, 155, 0, 1, 6, "Pillar");

        for (int x = 158; x <= 163; x++) {
            Crumble(s, x, 5);
        }

        Tile("Castle/Stone", s.terrain, 167, 0, 2, 5, "Pillar");
        AddMovingPlatform(s, 171, 4, 3, new Vector2(5f, 0f), 3.2f, 0.25f);

        Spark(s, 128.5f, 5f, 3f, 0f);
        Spark(s, 141f, 6f, 3.5f, 1f);
        Spark(s, 153f, 5f, 2.8f, 0.5f);
        Spark(s, 165f, 5f, 3f, 1.5f);
        Spark(s, 176f, 4f, 2.5f, 0.7f);

        Coin(s, 123, 6); Coin(s, 124, 6);
        Coin(s, 128, 8); Coin(s, 129, 8);
        Coin(s, 140, 7); Coin(s, 142, 7); Coin(s, 144, 7);
        Coin(s, 153, 9);
        Coin(s, 159, 8); Coin(s, 161, 8); Coin(s, 163, 8);
        Coin(s, 171, 7); Coin(s, 173, 7); Coin(s, 175, 7);
    }

    private static void Section4(Section s)
    {
        Torch(s, 188, 7); Torch(s, 206, 7); Torch(s, 228, 7);

        AddFirebar(s, 192, 5, 5, -100f);
        Mystery(s, 198, 8, "MagicMushroom");
        AddFirebar(s, 202, 5, 6, 80f);

        // narrow corridor with fire jets
        Tile("Castle/Brick", s.terrain, 210, 8, 13, 4, "Low Ceiling");
        AddFireJet(s, 214, 0f);
        AddFireJet(s, 219, 1.2f);
        Coin(s, 211, 3); Coin(s, 212, 3); Coin(s, 216, 3); Coin(s, 217, 3); Coin(s, 221, 3);

        // cannons: one shoots along the floor, one at head height
        Enemy(s, "Goomba", 226, LowerStand);
        Cannon(s, 230, 2, Vector2.left, 2.6f, 0f);
        Enemy(s, "Goomba", 234, LowerStand);
        Tile("Castle/Brick", s.terrain, 239, 7, 3, 5, "Cannon Mount");
        Cannon(s, 240, 6, Vector2.left, 2.2f, 1f);
    }

    private static void Section5(Section s)
    {
        AddCheckpoint(s, 250, LowerFloorTop);
        Torch(s, 249, 10); Torch(s, 284, 14); Torch(s, 249, 28); Torch(s, 284, 24);

        // zig-zag climb (3 tiles between floors, 2 tile gaps)
        int[,] platforms = { { 250, 4 }, { 257, 7 }, { 264, 10 }, { 257, 13 }, { 250, 16 }, { 257, 19 }, { 269, 25 } };

        for (int i = 0; i < platforms.GetLength(0); i++)
        {
            int x = platforms[i, 0];
            int y = platforms[i, 1];
            Tile("Castle/Stone", s.terrain, x, y, 5, 1, "Tower Platform");
            Coin(s, x + 2, y + 2);
        }

        // elevator from the 6th platform up to the last one
        AddMovingPlatform(s, 264, 19, 3, new Vector2(0f, 6f), 5f, 0f);

        AddFirebar(s, 262, 16, 5, 70f);
        Cannon(s, 248, 22, Vector2.right, 3f, 0.5f);
    }

    private static void Section6(Section s)
    {
        AddCheckpoint(s, 292, UpperFloorTop);
        Torch(s, 296, 35); Torch(s, 312, 35); Torch(s, 322, 35);

        // supplies before the boss
        Mystery(s, 298, 33, "MagicMushroom");
        Mystery(s, 300, 33, null);
        Mystery(s, 302, 33, "1upMushroom");

        for (int x = 306; x <= 310; x++) {
            Coin(s, x, 31);
        }

        Enemy(s, "Goomba", 316, UpperStand);
    }

    private static void Section7(Section s)
    {
        Torch(s, 330, 36); Torch(s, 352, 38); Torch(s, 372, 36);

        Gate entryGate = Tile("Castle/Gate", s.terrain, 327, 30, 1, 10, "Entry Gate").GetComponent<Gate>();
        entryGate.gameObject.SetActive(false);
        Gate exitGate = Tile("Castle/Gate", s.terrain, 376, 30, 1, 10, "Exit Gate").GetComponent<Gate>();

        // platforms to dodge the fire
        Tile("Castle/Stone", s.terrain, 336, 33, 3, 1, "Arena Platform");
        Tile("Castle/Stone", s.terrain, 362, 33, 3, 1, "Arena Platform");
        Tile("Castle/Stone", s.terrain, 349, 36, 3, 1, "Arena Platform");

        Boss boss = Place("Castle/Boss", s.enemies, 366f, 31f, "Magmor").GetComponent<Boss>();
        boss.minX = 329.5f;
        boss.maxX = 373.5f;
        boss.healthBar = healthBar;

        GameObject arenaObject = new GameObject("Boss Arena Trigger");
        arenaObject.transform.SetParent(s.terrain.parent, false);
        arenaObject.transform.position = new Vector3(331f, 35f, 0f);
        arenaObject.AddComponent<BoxCollider2D>().size = new Vector2(1f, 10f);
        BossArena arena = arenaObject.AddComponent<BossArena>();
        arena.boss = boss;
        arena.entryGate = entryGate;
        arena.exitGate = exitGate;
        arena.music = music;

        boss.arena = arena;
        Record(boss);
    }

    private static void Section8(Section s)
    {
        Torch(s, 384, 35); Torch(s, 398, 35);

        for (int x = 380; x <= 388; x++) {
            Coin(s, x, 31);
        }

        Tile("Castle/Stone", s.terrain, 392, 30, 1, 1, "Pedestal");
        Place("Castle/Trophy", s.blocks, 392f, 31.5f, "Treasure");
    }

    // ------------------------------------------------------------ helpers

    private static Section NewSection(Transform parent, string name)
    {
        Transform root = Group(name, parent);

        return new Section {
            terrain = Group("Terrain", root),
            hazards = Group("Hazards", root),
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

    private static RectTransform UIRect(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return (RectTransform)gameObject.transform;
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

    // places a tiled prefab whose bottom-left tile center is (x, y)
    private static GameObject Tile(string prefab, Transform parent, int x, int y, int width, int height, string name)
    {
        GameObject tile = Place(prefab, parent, x + (width - 1) / 2f, y + (height - 1) / 2f, name);
        Vector2 size = new Vector2(width, height);

        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        spriteRenderer.size = size;
        Record(spriteRenderer);

        BoxCollider2D collider = tile.GetComponent<BoxCollider2D>();

        if (collider != null)
        {
            collider.autoTiling = true;
            collider.size = size;
            Record(collider);
        }

        return tile;
    }

    private static void Lava(Transform parent, int fromX, int toX)
    {
        int width = toX - fromX + 1;
        GameObject lava = Tile("Castle/Lava", parent, fromX, 0, width, 2, $"Lava {fromX}-{toX}");

        // the surface is a little below the tile top
        BoxCollider2D collider = lava.GetComponent<BoxCollider2D>();
        collider.autoTiling = false;
        collider.size = new Vector2(width, 1.6f);
        collider.offset = new Vector2(0f, -0.2f);
        Record(collider);
    }

    private static void Spikes(Section s, int x, int y, int width, bool down)
    {
        GameObject spikes = Tile(down ? "Castle/Spikes_Down" : "Castle/Spikes", s.hazards, x, y, width, 1, "Spikes");

        // only the pointed half hurts
        BoxCollider2D collider = spikes.GetComponent<BoxCollider2D>();
        collider.autoTiling = false;
        collider.size = new Vector2(width - 0.2f, 0.5f);
        collider.offset = new Vector2(0f, down ? 0.1f : -0.1f);
        Record(collider);
    }

    private static void Crumble(Section s, int x, int y)
    {
        Place("Castle/Crumble", s.terrain, x, y, "Crumbling Block");
    }

    private static void AddMovingPlatform(Section s, int leftX, int row, int width, Vector2 offset, float period, float phase)
    {
        GameObject platform = Place("Castle/MovingPlatform", s.terrain, leftX + (width - 1) / 2f, row + 0.25f, "Moving Platform");

        SpriteRenderer spriteRenderer = platform.GetComponent<SpriteRenderer>();
        spriteRenderer.size = new Vector2(width, 1f);

        BoxCollider2D collider = platform.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(width, 0.5f);

        MovingPlatform moving = platform.GetComponent<MovingPlatform>();
        moving.offset = offset;
        moving.period = period;
        moving.phase = phase;

        Record(spriteRenderer, collider, moving);
    }

    private static void Spark(Section s, float x, float height, float period, float offset)
    {
        LavaSpark spark = Place("Castle/LavaSpark", s.hazards, x, 0.5f, "Lava Spark").GetComponent<LavaSpark>();
        spark.height = height;
        spark.period = period;
        spark.offset = offset;
        Record(spark);
    }

    private static void AddFireJet(Section s, int x, float offset)
    {
        FireJet jet = Place("Castle/FireJet", s.hazards, x, 2f, "Fire Jet").GetComponent<FireJet>();
        jet.offset = offset;
        Record(jet);
    }

    private static void Cannon(Section s, int x, int y, Vector2 direction, float interval, float offset)
    {
        GameObject cannon = Place("Castle/Cannon", s.hazards, x, y, "Fire Cannon");

        // the sprite faces left
        cannon.GetComponent<SpriteRenderer>().flipX = direction.x > 0f;

        FireCannon fireCannon = cannon.GetComponent<FireCannon>();
        fireCannon.direction = direction;
        fireCannon.interval = interval;
        fireCannon.offset = offset;
        Record(cannon.GetComponent<SpriteRenderer>(), fireCannon);
    }

    // rotating chain of fireballs around a solid block
    private static void AddFirebar(Section s, int x, int y, int length, float speed)
    {
        Tile("Castle/Stone", s.terrain, x, y, 1, 1, "Firebar Block");

        GameObject root = new GameObject("Firebar");
        root.transform.SetParent(s.hazards, false);
        root.transform.position = new Vector3(x, y, 0f);
        root.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        root.AddComponent<Firebar>().speed = speed;

        for (int i = 0; i < length; i++)
        {
            GameObject ball = SpriteObject("Fireball", LoadSprite("Fireball1"), 5);
            ball.transform.SetParent(root.transform, false);
            ball.transform.localPosition = new Vector3(i * 0.5f, 0f, 0f);
            Animate(ball, 0.08f, LoadSprite("Fireball1"), LoadSprite("Fireball2"));
            ball.AddComponent<CircleCollider2D>().radius = 0.2f;
            MakeHazard(ball, false);
        }
    }

    private static void AddCheckpoint(Section s, int x, float floorTop)
    {
        Place("Castle/Checkpoint", s.decoration, x, floorTop + 1f, "Checkpoint");
    }

    private static void Torch(Section s, int x, int y)
    {
        Place("Castle/Torch", s.decoration, x, y, "Torch");
    }

    private static void Coin(Section s, int x, int y)
    {
        Place("Coin", s.coins, x, y);
    }

    // item null keeps the prefab default (a coin)
    private static void Mystery(Section s, int x, int y, string item)
    {
        GameObject block = Place("MysteryBlock", s.blocks, x, y, "Mystery Block");

        if (item != null)
        {
            BlockHit blockHit = block.GetComponent<BlockHit>();
            blockHit.item = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + item + ".prefab");
            Record(blockHit);
        }
    }

    private static void Enemy(Section s, string prefab, int x, float y)
    {
        Place(prefab, s.enemies, x, y);
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
