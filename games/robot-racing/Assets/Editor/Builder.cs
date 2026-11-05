using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class Builder
{
    const string Scene = "Assets/Scenes/Main.unity";

    [MenuItem("Robot Racing/Build macOS app")]
    public static void BuildMac()
    {
        ProjectSetup.Run(); ProjectSetup.FixPictures(); FontBaker.Bake(false);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { Scene },
            locationPathName = "Builds/mac/Robot Racing.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
        });
        Debug.Log($"Build: {report.summary.result}, {report.summary.totalErrors} errors, {report.summary.totalSize / 1_000_000} MB");
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    // Builds the browser version into the platform's site (site/games/robot-racing at the
    // root of the repository), where the Maaloomatiia Games menu launches it; see the
    // root README. Gzip with the decompression fallback, so the files load from any host
    // whatever headers it sends; new file names on every build, so a browser never mixes
    // an old cached build with a new one.
    // Three levels up from Assets: <repository>/site/games/robot-racing.
    public static string WebDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../site/games/robot-racing"));

    [MenuItem("Robot Racing/Build web")]
    public static void BuildWeb()
    {
        ProjectSetup.Run(); ProjectSetup.FixPictures(); FontBaker.Bake(false);
        Maaloomatiia.Games.Editor.WebTemplate.Install(); // the platform's page, from the kit
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        // The release settings (Unity's web optimisation guide): strip the engine, size over
        // speed in IL2CPP, no debug symbols, the smallest code the linker can make. Managed
        // stripping stays at Medium: the kit reads JSON through reflection.
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ManagedStrippingLevel.Medium);
        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.WebGL, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { Scene },
            locationPathName = WebDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });
        Debug.Log($"[WebBuild] {report.summary.result}: {report.summary.totalErrors} errors, {report.summary.totalSize / 1_000_000} MB " +
                  $"in {report.summary.totalTime:mm\\:ss} -> {WebDir}");
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    // Command line: smoke test, then the build.
    public static void VerifyAndBuild()
    {
        
        BuildMac();
    }
}

// The entry point every game on the platform has, so one command builds any of them
// (Docker/unity.sh at the repository root: -executeMethod WebBuild.BuildFromCommandLine).
public static class WebBuild
{
    public static void BuildFromCommandLine() => Builder.BuildWeb();
}
