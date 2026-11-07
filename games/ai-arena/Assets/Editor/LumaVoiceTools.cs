using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// AI Arena > Generate LUMA Voice: records every line in Luma.Lines with a
// text-to-speech voice, one file per line in Resources/LumaVoice. On a Mac with
// the built-in `say` command, on Windows with a Windows voice
// (Tools/LumaVoice/generate.ps1). Run it again after changing what LUMA says;
// recordings of lines that no longer exist are deleted. The two voices sound
// different, so record every line on the same system. Same tool as Claude Bomb's.
public static class LumaVoiceTools
{
    const string OutDir = "Assets/Resources/LumaVoice";
    const string WindowsVoice = "Microsoft Zira Desktop";
    const string MacVoice = "Samantha";
    const int MacWordsPerMinute = 185;

    [MenuItem("AI Arena/Generate LUMA Voice")]
    public static void Generate() => Run();

    // Command line: Unity -batchmode -executeMethod LumaVoiceTools.GenerateBatch
    public static void GenerateBatch() => EditorApplication.Exit(Run() ? 0 : 1);

    static bool Run()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(root, OutDir);
        Directory.CreateDirectory(outDir);
        var spoken = Luma.Lines.Keys.ToDictionary(id => id, Luma.Spoken);

        bool ok = Application.platform == RuntimePlatform.OSXEditor ? RecordWithSay(spoken, outDir) : RecordWithWindows(spoken, root, outDir);
        if (!ok) return false;

        foreach (string old in Directory.GetFiles(outDir, "*.wav").Where(f => !spoken.ContainsKey(Path.GetFileNameWithoutExtension(f))))
            AssetDatabase.DeleteAsset(OutDir + "/" + Path.GetFileName(old));

        AssetDatabase.Refresh();
        // Short speech: mono, compressed, decoded when loaded, so LUMA starts talking without a delay.
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
        AssetDatabase.SaveAssets();
        return true;
    }

    static bool RecordWithSay(Dictionary<string, string> spoken, string outDir)
    {
        string textFile = Path.Combine(Path.GetTempPath(), "luma-line.txt");
        // `say` hangs now and then (a stuck speech server): each line gets a minute, then a
        // second try. LUMA_VOICE_MISSING_ONLY=1 records only the lines that have no file yet,
        // for a batch that was cut short.
        bool missingOnly = Environment.GetEnvironmentVariable("LUMA_VOICE_MISSING_ONLY") == "1";
        foreach (var line in spoken)
        {
            string wav = Path.Combine(outDir, line.Key + ".wav");
            if (missingOnly && File.Exists(wav)) continue;
            File.WriteAllText(textFile, line.Value);
            string args = $"-v \"{MacVoice}\" -r {MacWordsPerMinute} -f \"{textFile}\" -o \"{wav}\" --file-format=WAVE --data-format=LEI16@22050";
            if (!RunProgram("/usr/bin/say", args, 60000) && !RunProgram("/usr/bin/say", args, 60000))
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
        return RunProgram("powershell.exe",
            $"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -LinesJson \"{json}\" -OutDir \"{outDir}\" -Voice \"{WindowsVoice}\"");
    }

    static bool RunProgram(string program, string arguments, int timeoutMs = 0)
    {
        var psi = new ProcessStartInfo(program, arguments)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
        };
        using (var p = Process.Start(psi))
        {
            var output = p.StandardOutput.ReadToEndAsync();
            var errors = p.StandardError.ReadToEndAsync();
            if (timeoutMs > 0 ? !p.WaitForExit(timeoutMs) : false)
            {
                try { p.Kill(); } catch (Exception) { }
                Debug.LogWarning($"[LumaVoice] {program} took more than {timeoutMs / 1000} s and was stopped: {arguments}");
                return false;
            }
            p.WaitForExit();
            string err = errors.Result, outText = output.Result;
            if (p.ExitCode != 0 || err.Length > 0) { Debug.LogError("[LumaVoice] " + (err.Length > 0 ? err : $"exit {p.ExitCode}")); return false; }
            if (outText.Trim().Length > 0) Debug.Log("[LumaVoice] " + outText.Trim());
        }
        return true;
    }
}
