using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// LUMA speaks each of her fixed lines from a recording in Resources/LumaVoice. A build
// without them is silent and nothing says so: on 7 November 2026 the folder was moved out of
// Resources by hand and the builds went out without her voice. So every build stops here
// when the folder is gone or one of her fixed lines (Luma.Lines) has no recording.
// Record them with AI Arena > Generate LUMA Voice.
public class LumaVoiceGuard : IPreprocessBuildWithReport
{
    public const string Folder = "Assets/Resources/LumaVoice";
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (!Directory.Exists(Folder))
            throw new BuildFailedException("LUMA's voice is missing: " + Folder + " does not exist. Put her recordings back there (or run AI Arena > Generate LUMA Voice).");
        var missing = Luma.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Value) && !File.Exists(Path.Combine(Folder, l.Key + ".wav")))
                                .Select(l => l.Key).ToList();
        if (missing.Count > 0)
            throw new BuildFailedException("LUMA's voice is missing " + missing.Count + " recording(s) in " + Folder + ": " + string.Join(", ", missing) + ". Run AI Arena > Generate LUMA Voice.");
    }
}
