using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class Builder
{
    const string Scene = "Assets/Scenes/Main.unity";

    [MenuItem("AI Arena/Build macOS app")]
    public static void BuildMac()
    {
        ProjectSetup.FixPictures();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { Scene },
            locationPathName = "Builds/mac/AI Arena.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
        });
        Debug.Log($"Build: {report.summary.result}, {report.summary.totalErrors} errors, {report.summary.totalSize / 1_000_000} MB");
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    // Builds the browser version into the platform's site (site/games/ai-arena at the
    // root of the repository), where the Maaloomatiia Games menu launches it; see the
    // root README. Gzip with the decompression fallback, so the files load from any host
    // whatever headers it sends; new file names on every build, so a browser never mixes
    // an old cached build with a new one.
    // Three levels up from Assets: <repository>/site/games/ai-arena.
    public static string WebDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../site/games/ai-arena"));

    [MenuItem("AI Arena/Build web")]
    public static void BuildWeb()
    {
        ProjectSetup.FixPictures();
        Maaloomatiia.Games.Editor.WebTemplate.Install(); // the platform's page, from the kit
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.nameFilesAsHashes = true;
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
        SmokeTest.Run();
        BuildMac();
    }
}

// The entry point every game on the platform has, so one command builds any of them
// (Docker/unity.sh at the repository root: -executeMethod WebBuild.BuildFromCommandLine).
public static class WebBuild
{
    public static void BuildFromCommandLine() => Builder.BuildWeb();
}
