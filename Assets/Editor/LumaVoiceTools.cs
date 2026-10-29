using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Claude Bomb > Generate LUMA Voice: records every line in LoomaCoach.Lines with a
// text-to-speech voice, one file per line in Resources/LumaVoice - on a Mac with the
// built-in `say` command, on Windows with a Windows voice (Tools/LumaVoice/generate.ps1).
// Run it again after changing what LUMA says; recordings of lines that no longer
// exist are deleted. Both voices sound different, so record all lines on one system.
public static class LumaVoiceTools
{
    const string OutDir = "Assets/Resources/LumaVoice";
    const string WindowsVoice = "Microsoft Zira Desktop";
    const string MacVoice = "Samantha";
    const int MacWordsPerMinute = 185;

    [MenuItem("Claude Bomb/Generate LUMA Voice")]
    public static void Generate()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(root, OutDir);
        Directory.CreateDirectory(outDir);
        var spoken = LoomaCoach.Lines.Keys.ToDictionary(id => id, LoomaCoach.Spoken);

        bool ok = Application.platform == RuntimePlatform.OSXEditor ? RecordWithSay(spoken, outDir) : RecordWithWindows(spoken, root, outDir);
        if (!ok) return;

        foreach (string old in Directory.GetFiles(outDir, "*.wav").Where(f => !spoken.ContainsKey(Path.GetFileNameWithoutExtension(f))))
            AssetDatabase.DeleteAsset(OutDir + "/" + Path.GetFileName(old));

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

    static bool RecordWithSay(Dictionary<string, string> spoken, string outDir)
    {
        string textFile = Path.Combine(Path.GetTempPath(), "luma-line.txt");
        foreach (var line in spoken)
        {
            File.WriteAllText(textFile, line.Value);
            string wav = Path.Combine(outDir, line.Key + ".wav");
            if (!Run("/usr/bin/say", $"-v \"{MacVoice}\" -r {MacWordsPerMinute} -f \"{textFile}\" -o \"{wav}\" --file-format=WAVE --data-format=LEI16@22050"))
                return false;
        }
        File.Delete(textFile);
        Debug.Log($"[LumaVoice] Generated {spoken.Count} voice lines with {MacVoice}");
        return true;
    }

    static bool RecordWithWindows(Dictionary<string, string> spoken, string root, string outDir)
    {
        string script = Path.Combine(root, "Tools/LumaVoice/generate.ps1");
        string json = Path.Combine(root, "Tools/LumaVoice/lines.json");
        File.WriteAllText(json, JsonConvert.SerializeObject(spoken, Formatting.Indented));
        return Run("powershell.exe",
            $"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -LinesJson \"{json}\" -OutDir \"{outDir}\" -Voice \"{WindowsVoice}\"");
    }

    static bool Run(string program, string arguments)
    {
        var psi = new ProcessStartInfo(program, arguments)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
        };
        using (var p = Process.Start(psi))
        {
            string output = p.StandardOutput.ReadToEnd(), errors = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 || errors.Length > 0) { Debug.LogError("[LumaVoice] " + errors); return false; }
            if (output.Trim().Length > 0) Debug.Log("[LumaVoice] " + output.Trim());
        }
        return true;
    }
}
