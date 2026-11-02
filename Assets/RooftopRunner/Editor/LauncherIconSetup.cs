using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

public static class LauncherIconSetup
{
    const string Folder = "Assets/RooftopRunner/Art/Launcher/";
    [MenuItem("Rooftop Runner/Apply Launcher Icon")]
    public static void Apply()
    {
        var icon = Import(Folder + "RooftopRunnerIcon.png");
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon });
#if UNITY_ANDROID
        // A static adaptive icon: artwork on the background layer, transparent foreground.
        // Android applies its own circle/squircle mask; no corner shape is baked into the art.
        string clearPath = Folder + "AdaptiveForeground.png";
        if (!File.Exists(clearPath))
        {
            var clear = new Texture2D(108, 108, TextureFormat.RGBA32, false);
            clear.SetPixels(new Color[108 * 108]); clear.Apply();
            File.WriteAllBytes(clearPath, clear.EncodeToPNG()); Object.DestroyImmediate(clear);
        }
        var foreground = Import(clearPath);
        var target = NamedBuildTarget.Android;
        foreach (var kind in new[] { AndroidPlatformIconKind.Adaptive })
        {
            var icons = PlayerSettings.GetPlatformIcons(target, kind);
            foreach (var slot in icons)
            {
                if (kind == AndroidPlatformIconKind.Adaptive) slot.SetTextures(new[] { icon, foreground });
                else slot.SetTexture(icon);
            }
            PlayerSettings.SetPlatformIcons(target, kind, icons);
            Debug.Log("LAUNCHER_ICON: " + kind + " slots=" + icons.Length);
        }
#else
        Debug.Log("Default icon assigned. Switch to Android and run Apply Launcher Icon for Android-specific slots.");
#endif
        AssetDatabase.SaveAssets();
    }
    static Texture2D Import(string path)
    {
        if (!File.Exists(path)) throw new System.InvalidOperationException("Launcher artwork is missing: " + path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.maxTextureSize = 1024; importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    public static void ApplyAndBuild() { Apply(); RooftopRunnerSetup.BuildAndroid(); }
}
