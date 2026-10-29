using System.IO;
using UnityEditor;
using UnityEngine;

// Import settings shared by the generated pixel art: 16 pixels per unit,
// point filter, no compression and full rect meshes (needed for tiling).
public static class SpriteImport
{
    public static bool Configure(string folder, string requiredSprite)
    {
        if (!File.Exists($"{folder}/{requiredSprite}.png"))
        {
            Debug.LogError($"[SpriteImport] Faltan los sprites de {folder}. Ejecuta el generador de Tools/.");
            return false;
        }

        foreach (string file in Directory.GetFiles(folder, "*.png"))
        {
            string path = file.Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError($"[SpriteImport] {path} aún no está importado. Vuelve a enfocar Unity y repite.");
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

}
