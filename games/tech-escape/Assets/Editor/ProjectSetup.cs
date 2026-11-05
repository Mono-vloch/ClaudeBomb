using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Makes a fresh clone of the project playable on its first open: TextMesh Pro's
// resources, the baked sprites, the one scene and the build list. Everything it
// makes is checked in afterwards, so it mostly does nothing.
[InitializeOnLoad]
public static class ProjectSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    static ProjectSetup() { EditorApplication.delayCall += Run; }

    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            Debug.Log("Tech Escape: importing TextMesh Pro essentials.");
            TMP_PackageResourceImporter.ImportResources(true, false, false);
        }
        if (SpriteBaker.Missing) SpriteBaker.Bake();
        FixPictures();
        if (!File.Exists(ScenePath)) CreateScene();
        if (!EditorBuildSettings.scenes.Any(s => s.path == ScenePath))
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        if (EditorSettings.defaultBehaviorMode != EditorBehaviorMode.Mode3D)
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode3D;
        if (PlayerSettings.fullScreenMode != FullScreenMode.Windowed)
        {
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
        }
    }

    // For the command line: runs the setup and exits once any package import has
    // landed, because -quit would cut the TextMesh Pro import short.
    public static void Batch()
    {
        bool importing = Resources.Load<TMP_Settings>("TMP Settings") == null;
        if (importing)
        {
            AssetDatabase.importPackageCompleted += _ => Finish(0);
            AssetDatabase.importPackageFailed += (_, msg) => { Debug.LogError("TMP import failed: " + msg); Finish(1); };
            AssetDatabase.importPackageCancelled += _ => Finish(1);
        }
        Run();
        if (!importing) Finish(0);
    }

    static void Finish(int code)
    {
        Run(); // the scene and sprites may have waited on the import
        AssetDatabase.SaveAssets();
        Debug.Log("Tech Escape: batch setup finished with code " + code);
        EditorApplication.Exit(code);
    }

    // The brand marks and LUMA's portrait are plain pictures: one sprite each,
    // no mipmaps, uncompressed so the gradients stay clean. A fresh import would
    // slice them into a sprite sheet and show only the first piece.
    public static void FixPictures()
    {
        foreach (string folder in new[] { "Assets/Resources/Brand", "Assets/Resources/Luma" })
        {
            if (!Directory.Exists(folder)) continue;
            foreach (string path in Directory.GetFiles(folder, "*.png"))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single && !importer.mipmapEnabled
                    && importer.textureCompression == TextureImporterCompression.Uncompressed) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
                Debug.Log("Tech Escape: set " + path + " to a single sprite.");
            }
        }
    }

    [MenuItem("Tech Escape/Rebuild Main Scene")]
    static void RebuildScene()
    {
        if (!EditorUtility.DisplayDialog("Rebuild Main scene", "Replace Assets/Scenes/Main.unity with a fresh one (camera + App)?", "Rebuild", "Cancel")) return;
        CreateScene();
    }

    static void CreateScene()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = false; // MarketCamera takes it over at runtime
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Theme.Page;
        camGo.AddComponent<AudioListener>();
        new GameObject("App").AddComponent<App>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Tech Escape: created " + ScenePath);
    }
}
