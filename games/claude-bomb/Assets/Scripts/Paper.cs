using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

// One manual page on the table. The player sees the Japanese text and can black
// out any segment; Claude only ever receives the English text of what is left.
// Page content lives in Resources/Papers/papers.json - the rules there must match
// Rules.cs (current pages) or Rules.WireAnswerRevC (the superseded wire page).
public class Paper
{
    public class Segment
    {
        public string jp;
        public string en;
        public bool secret;              // private information: must be blacked out before sharing
        [JsonIgnore] public bool redacted;
    }

    public string id;
    public int module;                   // number on the module's plate (1-5)
    public ModuleId moduleId;
    public string revision;              // e.g. "D"
    public string date;                  // e.g. "2026-09-14"
    public bool current;                 // false = a superseded copy still lying on the table
    public string titleJp;
    public string titleEn;
    public List<List<Segment>> lines;

    public string Label => $"MODULE {module} · Rev {revision}";
    public IEnumerable<Segment> Segments => lines.SelectMany(line => line);
    public bool HasVisibleSecrets => Segments.Any(s => s.secret && !s.redacted);
    public bool HasSecrets => Segments.Any(s => s.secret);
    // Blacked out something that wasn't private - often a rule Claude now can't read.
    public bool HidesNonSecrets => Segments.Any(s => !s.secret && s.redacted);

    // What Claude receives when the page is shared: English, blacked-out parts removed.
    public string ToEnglish()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== PAGE: MODULE {module} - {titleEn} - Revision {revision} - dated {date} ===");
        foreach (var line in lines)
        {
            foreach (var s in line) sb.Append(s.redacted ? "██████" : s.en);
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    public static List<Paper> LoadAll()
    {
        var asset = Resources.Load<TextAsset>("Papers/papers");
        return JsonConvert.DeserializeObject<PaperFile>(asset.text).pages;
    }

    class PaperFile { public List<Paper> pages; }
}
