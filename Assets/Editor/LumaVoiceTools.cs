using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Claude Bomb > Generate LUMA Voice: records every line in LoomaCoach.Lines with a
// Windows text-to-speech voice (Tools/LumaVoice/generate.ps1), one file per line
// in Resources/LumaVoice. Run it again after changing what LUMA says.
public static class LumaVoiceTools
{
    const string OutDir = "Assets/Resources/LumaVoice";
    const string Voice = "Microsoft Zira Desktop";

    [MenuItem("Claude Bomb/Generate LUMA Voice")]
    public static void Generate()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string script = Path.Combine(root, "Tools/LumaVoice/generate.ps1");
        string json = Path.Combine(root, "Tools/LumaVoice/lines.json");

        var spoken = LoomaCoach.Lines.Keys.ToDictionary(id => id, LoomaCoach.Spoken);
        File.WriteAllText(json, JsonConvert.SerializeObject(spoken, Formatting.Indented));

        var psi = new ProcessStartInfo("powershell.exe",
            $"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -LinesJson \"{json}\" -OutDir \"{Path.Combine(root, OutDir)}\" -Voice \"{Voice}\"")
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
        };
        using (var p = Process.Start(psi))
        {
            string output = p.StandardOutput.ReadToEnd(), errors = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 || errors.Length > 0) { Debug.LogError("[LumaVoice] " + errors); return; }
            Debug.Log("[LumaVoice] " + output.Trim());
        }

        AssetDatabase.Refresh();
        // Short speech: mono, compressed, decoded when loaded (small, and no delay when LUMA starts talking).
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { OutDir }))
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            importer.forceToMono = true;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
    }
}
