using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Adds scenes to the shared Build Profiles scene list without removing or
// reordering the existing entries, and writes the list to disk.
public static class BuildSceneList
{
    public static void Add(params string[] paths)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        foreach (string path in paths)
        {
            if (File.Exists(path) && !scenes.Exists(s => s.path == path)) {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();

        foreach (EditorBuildSettingsScene s in scenes) {
            Debug.Log($"[BuildSceneList] Build scene: {s.path} (enabled: {s.enabled})");
        }
    }

}
