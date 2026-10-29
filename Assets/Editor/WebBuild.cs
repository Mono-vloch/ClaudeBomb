using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds the web version into Web/public, ready for Netlify (see Web/netlify.toml).
// Refuses to build while an API key file sits anywhere under Assets: in a web build
// every file can be downloaded by anyone who opens the page.
public static class WebBuild
{
    const string OutputDir = "Web/public";

    [MenuItem("Claude Bomb/Build Web Version")]
    public static void Build() => Run();

    // For builds without the Editor window (Unity -batchmode -executeMethod
    // WebBuild.BuildFromCommandLine): the exit code says whether the build worked.
    public static void BuildFromCommandLine() => EditorApplication.Exit(Run() == BuildResult.Succeeded ? 0 : 1);

    static BuildResult Run()
    {
        if (Directory.GetFiles("Assets", "claude_key*", SearchOption.AllDirectories).Length > 0)
        {
            Debug.LogError("[WebBuild] A claude_key file is inside Assets and would be published with the game. Keep it in UserSettings/ instead.");
            return BuildResult.Failed;
        }

        PlayerSettings.WebGL.template = "PROJECT:ClaudeBomb";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        // Lets the game load from any web host, whatever compression headers it sends.
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true; // a second visit loads from the browser cache
        // New file names on every build, so browsers never mix a cached old build
        // with a newly published one.
        PlayerSettings.WebGL.nameFilesAsHashes = true;

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = OutputDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"[WebBuild] {report.summary.result}: {report.summary.totalSize / (1024f * 1024f):F1} MB " +
                  $"in {report.summary.totalTime:mm\\:ss} -> {OutputDir}");
        return report.summary.result;
    }
}
