using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu art from the supplied Hito 4 package; gameplay remains owned by the game.
public static class Hito4PixelUIBuilder
{
    private const string Art = "Assets/Hito4PixelUI/Art/";
    private const string MenuPath = "Assets/Scenes/MainMenu.unity";
    private const string PausePath = "Assets/Hito4PixelUI/Resources/Hito4PauseMenu.prefab";
    private static readonly string[] Levels = { "1-1", "1-2", "2-2", "8-4" };
    private static readonly string[] ScreenNames = {
        "01_MenuPrincipal", "02_SeleccionNiveles", "03_ComoJugar", "04_Creditos", "05_Configuracion", "06_SFX_Referencia"
    };
    private static readonly Color Highlight = new Color(1f, 1f, .7f, .23f);

    [MenuItem("Tools/Super Mario/Hito 4/Crear menu del paquete")]
    public static void CreateMenu() { Generate(false); }

    // Supports local validation and headless scene generation.
    public static void CreateMenuForBatch() { Generate(true); }

    private static void Generate(bool unattended)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) {
            Debug.LogError("[Hito4] Sal de Play antes de generar el menu."); return;
        }
        if (!unattended && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (unattended && SceneManager.GetActiveScene().isDirty) {
            throw new System.InvalidOperationException("Save the open scene before generating the menu.");
        }
        PrepareArt();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildMenuScene();
        EditorSceneManager.SaveScene(scene, MenuPath);
        BuildPausePrefab();
        EditorSceneManager.SaveScene(scene, MenuPath);
        RegisterBuildSceneList();
        Debug.Log("[Hito4] Menu y pausa generados; niveles y prefabs del juego conservados.");
    }

    public static void RegisterBuildSceneList()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(scene => scene.path == MenuPath);
        scenes.Insert(0, new EditorBuildSettingsScene(MenuPath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
    }

    private static void PrepareArt()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png")) {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Art not imported: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }

    private static void BuildPausePrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Hito4PixelUI/Resources")) {
            AssetDatabase.CreateFolder("Assets/Hito4PixelUI", "Resources");
        }
        GameObject canvas = NewCanvas("Hito4 Pause Menu", 1000);
        try {
            Hito4PauseController pause = canvas.AddComponent<Hito4PauseController>();
            RectTransform overlay = NewRect("Pause 16-9", canvas.transform);
            overlay.sizeDelta = new Vector2(1920f, 1080f);
            AddImage(overlay.gameObject, SpriteOf("08_Pausa_Overlay")).raycastTarget = true;
            Button resume = Hotspot(overlay, "CONTINUAR", 329, 206, 302, 57);
            Button restart = Hotspot(overlay, "REINICIAR", 329, 281, 302, 57);
            Button home = Hotspot(overlay, "MENU PRINCIPAL", 329, 356, 302, 57);
            Bind(resume, pause.Reanudar);
            Bind(restart, pause.Reiniciar);
            Bind(home, pause.MenuPrincipal);
            Chain(resume, restart, home);
            pause.ventanaPausa = overlay.gameObject;
            pause.continuar = resume;
            overlay.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(canvas, PausePath);
        } finally {
            Object.DestroyImmediate(canvas);
        }
    }

    private static void BuildMenuScene()
    {
        GameObject camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 0f, -10f);
        Camera cam = camera.GetComponent<Camera>();
        cam.orthographic = true;
        cam.backgroundColor = new Color(.1f,.4f,.85f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        new GameObject("GameManager", typeof(GameManager));
        NewEventSystem();
        GameObject canvas = NewCanvas("Hito4 - Canvas Menu", 100);
        RectTransform backdrop = NewRect("Letterbox", canvas.transform);
        Stretch(backdrop);
        backdrop.gameObject.AddComponent<Image>().color = Color.black;
        RectTransform frame = NewRect("Menu 16-9", canvas.transform);
        frame.sizeDelta = new Vector2(1920f, 1080f);
        Hito4MenuController manager = canvas.AddComponent<Hito4MenuController>();
        manager.pantallas = new GameObject[ScreenNames.Length];
        manager.botonesRegresar = new Button[ScreenNames.Length];

        for (int i = 0; i < ScreenNames.Length; i++)
        {
            RectTransform screen = NewRect("Ventana_" + ScreenNames[i], frame);
            Stretch(screen);
            AddImage(screen.gameObject, SpriteOf(ScreenNames[i])).raycastTarget = false;
            manager.pantallas[i] = screen.gameObject;
        }
        Transform menu = manager.pantallas[0].transform;
        Button play = Hotspot(menu, "JUGAR", 338, 220, 284, 43);
        Button levels = Hotspot(menu, "SELECCIONAR NIVEL", 338, 270, 284, 43);
        Button controls = Hotspot(menu, "CONTROLES", 338, 320, 284, 43);
        Button credits = Hotspot(menu, "CREDITOS", 338, 370, 284, 43);
        Button settings = Hotspot(menu, "CONFIGURACION", 338, 420, 284, 43);
        Button quit = Hotspot(menu, "SALIR", 338, 470, 284, 43);
        Button quickMusic = Hotspot(menu, "MUSICA RAPIDA", 853, 14, 44, 47);
        Button quickSfx = Hotspot(menu, "SFX RAPIDO", 905, 14, 44, 47);
        // Sprite independiente: el personaje realmente salta en la escena de Unity.
        manager.mascota = OverlaySprite(menu,"Mascota animada",80,273,210,242,"09_Mascota");
        manager.monedas = new RectTransform[2];
        manager.monedas[0] = OverlaySprite(menu,"Moneda flotante 1",268,228,46,46,"10_Moneda");
        manager.monedas[1] = OverlaySprite(menu,"Moneda flotante 2",650,163,44,44,"10_Moneda");
        manager.jugar = play;
        Bind(play, manager.Iniciar);
        Bind(levels, manager.MostrarNiveles);
        Bind(controls, manager.MostrarControles);
        Bind(credits, manager.MostrarCreditos);
        Bind(settings, manager.MostrarAjustes);
        Bind(quit, manager.Salir);
        Bind(quickMusic, manager.AjustarMusicaRapida);
        Bind(quickSfx, manager.AjustarSfxRapido);
        Chain(play,levels,controls,credits,settings,quit,quickMusic,quickSfx);

        Transform levelPanel = manager.pantallas[1].transform;
        manager.tarjetasNiveles = new Button[4];
        int[] xx = { 45, 265, 485, 705 };
        for (int i = 0; i < 4; i++) manager.tarjetasNiveles[i] = Hotspot(levelPanel, "CARGAR NIVEL " + Levels[i], xx[i], 148, 205, 267);
        Bind(manager.tarjetasNiveles[0], manager.Nivel11);
        Bind(manager.tarjetasNiveles[1], manager.Nivel12);
        Bind(manager.tarjetasNiveles[2], manager.Nivel22);
        Bind(manager.tarjetasNiveles[3], manager.Nivel84);
        Chain(manager.tarjetasNiveles);
        // Las tarjetas poseen un marco y una descripción que cambian al usar flechas o ratón.
        manager.contornoNivel = OverlaySprite(levelPanel,"Marco nivel seleccionado",40,143,215,277,"11_MarcoSeleccion");
        Image footer = OverlaySprite(levelPanel,"Descripcion del nivel",143,441,674,50,"12_Footer_11").GetComponent<Image>();
        manager.descripcionNivel = footer;
        manager.descripciones = new Sprite[] { SpriteOf("12_Footer_11"), SpriteOf("13_Footer_12"), SpriteOf("14_Footer_22"), SpriteOf("15_Footer_84") };
        manager.botonesRegresar[1] = Hotspot(levelPanel, "VOLVER AL MENU", 15, 20, 72, 52);
        Bind(manager.botonesRegresar[1],manager.Volver);
        Button backLevels = manager.botonesRegresar[1];
        for (int i = 0; i < 4; i++) {
            manager.tarjetasNiveles[i].navigation = new Navigation {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = manager.tarjetasNiveles[(i + 3) % 4],
                selectOnRight = manager.tarjetasNiveles[(i + 1) % 4],
                selectOnUp = backLevels, selectOnDown = backLevels
            };
        }
        backLevels.navigation = new Navigation { mode = Navigation.Mode.Explicit,
            selectOnDown = manager.tarjetasNiveles[0], selectOnUp = manager.tarjetasNiveles[0] };

        for (int i = 2; i < 6; i++)
        {
            Transform screen = manager.pantallas[i].transform;
            manager.botonesRegresar[i] = Hotspot(screen, "VOLVER", 15, 20, 72, 52);
            if (i == 5) Bind(manager.botonesRegresar[i], manager.VolverAjustes);
            else Bind(manager.botonesRegresar[i], manager.Volver);
        }
        Transform cfg = manager.pantallas[4].transform;
        Button music = Hotspot(cfg, "MUSICA ACTIVADA DESACTIVADA", 632, 181, 111, 47);
        Button sfx = Hotspot(cfg, "SFX ACTIVADOS DESACTIVADOS", 632, 258, 111, 47);
        Button minus = Hotspot(cfg, "BAJAR VOLUMEN", 432, 356, 44, 45);
        Button plus = Hotspot(cfg, "SUBIR VOLUMEN", 709, 356, 42, 45);
        Button info = Hotspot(cfg, "VER EFECTOS Y EXPLICACIONES", 560, 408, 230, 44);
        Bind(music, manager.AlternarMusica);
        Bind(sfx, manager.AlternarSfx);
        Bind(minus, manager.BajarVolumen);
        Bind(plus, manager.SubirVolumen);
        Bind(info, manager.MostrarSfx);
        Chain(manager.botonesRegresar[4], music, sfx, minus, plus, info);
        manager.ledMusica = PlainImage(cfg,"Indicador musica",655,191,65,27,new Color(.2f,.9f,.4f));
        manager.ledSfx = PlainImage(cfg,"Indicador SFX",655,269,65,27,new Color(.2f,.9f,.4f));
        manager.barraVolumen = PlainImage(cfg,"Barra volumen",492,374,198,15,new Color(.17f,.68f,.97f));
        manager.barraVolumen.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        manager.barraVolumen.type = Image.Type.Filled;
        manager.barraVolumen.fillMethod = Image.FillMethod.Horizontal;
        manager.barraVolumen.fillOrigin = (int)Image.OriginHorizontal.Left;
        manager.barraVolumen.fillAmount = .8f;
        manager.numeroVolumen = AddText(cfg,"Valor volumen",774,382,"80",4f);

        for (int i = 0; i < manager.pantallas.Length; i++) manager.pantallas[i].SetActive(i == 0);
        EditorUtility.SetDirty(manager);
    }

    private static void NewEventSystem()
    {
        new GameObject("EventSystem Hito4",typeof(EventSystem),typeof(StandaloneInputModule));
    }
    private static GameObject NewCanvas(string name,int order)
    {
        GameObject obj = new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        Canvas c = obj.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = order;
        CanvasScaler s = obj.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920f,1080f);
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        s.matchWidthOrHeight = .5f;
        return obj;
    }
    private static Sprite SpriteOf(string name)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        if (sprite == null) Debug.LogError("[Hito4] No se pudo importar sprite: " + name);
        return sprite;
    }
    private static RectTransform NewRect(string name,Transform parent)
    {
        GameObject go = new GameObject(name,typeof(RectTransform));
        go.transform.SetParent(parent,false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(.5f,.5f);
        rt.pivot = new Vector2(.5f,.5f);
        rt.localScale = Vector3.one;
        return rt;
    }
    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
        rt.offsetMin=rt.offsetMax=Vector2.zero;
    }
    private static Image AddImage(GameObject go,Sprite sprite)
    {
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = false;
        return image;
    }
    private static Button Hotspot(Transform parent,string name,float x,float y,float w,float h)
    {
        RectTransform rect = NewRect("Boton - " + name,parent);
        rect.anchoredPosition = new Vector2((x+w*.5f-480f)*2f,(270f-y-h*.5f)*2f);
        rect.sizeDelta = new Vector2(w*2f,h*2f);
        Image img=rect.gameObject.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget=true;
        Button button=rect.gameObject.AddComponent<Button>();
        button.targetGraphic=img;
        button.transition=Selectable.Transition.ColorTint;
        ColorBlock block=button.colors;
        block.normalColor = Color.clear;
        block.highlightedColor = Highlight;
        block.selectedColor = new Color(1f,.88f,.4f,.27f);
        block.pressedColor = new Color(.3f,1f,.62f,.35f);
        block.disabledColor = Color.clear;
        block.fadeDuration = .08f;
        button.colors=block;
        rect.gameObject.AddComponent<SelectOnHover>();
        return button;
    }
    private static void Bind(Button button,UnityEngine.Events.UnityAction cb)
    {
        UnityEventTools.AddPersistentListener(button.onClick,cb);
        EditorUtility.SetDirty(button);
    }
    private static void Chain(params Button[] buttons)
    {
        for (int i=0;i<buttons.Length;i++)
        {
            Button b=buttons[i];
            Navigation n=new Navigation {mode=Navigation.Mode.Explicit};
            n.selectOnUp=buttons[(i+buttons.Length-1)%buttons.Length];
            n.selectOnDown=buttons[(i+1)%buttons.Length];
            n.selectOnLeft=n.selectOnUp;
            n.selectOnRight=n.selectOnDown;
            b.navigation=n;
        }
    }
    private static Image PlainImage(Transform parent,string name,float x,float y,float w,float h,Color color)
    {
        RectTransform rt=NewRect(name,parent);
        rt.anchoredPosition=new Vector2((x+w*.5f-480f)*2f,(270f-y-h*.5f)*2f);
        rt.sizeDelta=new Vector2(w*2f,h*2f);
        Image im=rt.gameObject.AddComponent<Image>();
        im.raycastTarget=false;
        im.color=color;
        return im;
    }
    private static RectTransform OverlaySprite(Transform parent,string name,float x,float y,float w,float h,string spriteName)
    {
        RectTransform rt = NewRect(name,parent);
        rt.anchoredPosition = new Vector2((x+w*.5f-480f)*2f,(270f-y-h*.5f)*2f);
        rt.sizeDelta = new Vector2(w*2f,h*2f);
        AddImage(rt.gameObject,SpriteOf(spriteName)).raycastTarget = false;
        return rt;
    }
    private static PixelText AddText(Transform parent,string name,float x,float y,string content,float pixelSize)
    {
        RectTransform rt=NewRect(name,parent);
        rt.anchoredPosition=new Vector2((x-480f)*2f,(270f-y)*2f);
        rt.gameObject.AddComponent<RawImage>().raycastTarget=false;
        PixelText t=rt.gameObject.AddComponent<PixelText>();
        t.color=new Color(.12f,.14f,.19f);
        t.pixelSize=pixelSize;
        t.shadow=false;
        t.SetText(content);
        return t;
    }
    private static PixelText AddTextAbsolute(Transform parent,string name,float x,float y,string content,float pixelSize)
    {
        RectTransform rt=NewRect(name,parent);
        rt.anchorMin=rt.anchorMax=new Vector2(0f,1f);
        rt.pivot=new Vector2(.5f,.5f);
        rt.anchoredPosition = new Vector2(x,y);
        rt.gameObject.AddComponent<RawImage>().raycastTarget=false;
        PixelText t=rt.gameObject.AddComponent<PixelText>();
        t.color=Color.white;
        t.pixelSize=pixelSize;
        t.shadow=true;
        t.SetText(content);
        return t;
    }
}
