using UnityEditor;

// Compatibility entry points now rebuild the imported menu rather than V1.
public static class MainMenuSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Tools/Super Mario/Crear menu principal")]
    public static void CreateScene() { Hito4PixelUIBuilder.CreateMenu(); }

    [MenuItem("Tools/Super Mario/Registrar menu como escena inicial")]
    public static void RegisterBuildScene() { Hito4PixelUIBuilder.RegisterBuildSceneList(); }
}
