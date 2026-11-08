using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

// Walks through the game on its own and saves a screenshot of each screen, with the
// game-feel pass's counts: the words and the controls on each screen, what must not be
// there (OPENS AT, a ladder, coordinates, seeds, probabilities), and whether every LUMA
// line is whole and short. The title and the four story shots; then every errand's briefing
// and garage; errands 1.1, 1.2, 1.5 and 1.6 are played through on the stand-in brain.
//   "AI Arena.app/Contents/MacOS/AI Arena" -screenshots /some/folder [-levels 1,5] [-live]
// Writes counts.jsonl beside the pictures. -cover <file> takes the platform's cover instead.
public class DemoDriver : MonoBehaviour
{
    public static string Folder => Arg("-screenshots");
    public static string CoverPath => Arg("-cover");
    static bool Live => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-live") >= 0;

    static string Arg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return null;
    }

    public static bool Wanted => Folder != null || CoverPath != null;

    static readonly int[] Played = { 1, 2, 5, 6 };

    IEnumerator Start()
    {
        var app = GetComponent<App>();
        if (CoverPath != null) { yield return Cover(app); yield break; }
        Directory.CreateDirectory(Folder);
        File.WriteAllText(Path.Combine(Folder, "counts.jsonl"), "");
        if (!Live) ClaudeAccess.ForceOffline = true;
        Luma.Silent = true;
        AudioListener.volume = 0f; // the walk is silent: it runs on a desk, not in a class
        for (int i = 0; i <= Ladder.Count; i++) PlayerPrefs.DeleteKey("ai_arena_garage_" + i);
        // The pictures start from a fresh machine; the machine's own progress comes back at the end.
        savedLevel = PlayerPrefs.GetInt("ai_arena_level", 0);
        for (int i = 1; i <= Ladder.Count; i++) { savedStars[i] = PlayerPrefs.GetInt("ai_arena_stars_" + i, 0); PlayerPrefs.DeleteKey("ai_arena_stars_" + i); }
        PlayerPrefs.DeleteKey("ai_arena_level");
        PlayerPrefs.DeleteKey("story_seen");
        app.turns = 12;
        app.configs = new List<AgentConfig> { new AgentConfig { name = "Nadia", colour = 0 } };
        var only = Arg("-levels")?.Split(',').Select(int.Parse).ToList();
        app.ShowTitle(); // drawn again from the fresh machine
        yield return new WaitForSeconds(1.4f);
        yield return Shot("01-title");

        // The story, once: the four shots move on by themselves.
        app.ShowStory(null);
        for (int n = 1; n <= 4; n++)
        {
            float waited = 0f;
            while (app.story != null && app.story.Current < n && waited < 20f) { waited += Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(n == 3 ? 2.6f : n == 4 ? 8.4f : 1.5f);
            if (app.story != null) yield return Shot($"02-story-{n}");
        }
        float w0 = 0f;
        while (app.story != null && w0 < 15f) { w0 += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(1f);

        for (int n = 1; n <= Ladder.Count; n++)
        {
            if (only != null && !only.Contains(n)) continue;
            yield return Level(app, n, Played.Contains(n));
        }

        app.ShowTitle();
        yield return new WaitForSeconds(1f);
        app.menu.Open();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("99-menu");
        yield return new WaitForSeconds(0.5f);
        Restore();
        Application.Quit();
    }

    int savedLevel; readonly int[] savedStars = new int[Ladder.Count + 1];
    void Restore()
    {
        PlayerPrefs.SetInt("ai_arena_level", savedLevel);
        PlayerPrefs.SetInt("story_seen", 1);
        for (int i = 1; i <= Ladder.Count; i++) { if (savedStars[i] > 0) PlayerPrefs.SetInt("ai_arena_stars_" + i, savedStars[i]); else PlayerPrefs.DeleteKey("ai_arena_stars_" + i); }
        PlayerPrefs.Save();
    }
    void OnApplicationQuit() { if (Folder != null) Restore(); }

    // One errand: the briefing and the garage always; the round and the debrief when it is
    // played. The planned mistake is fixed in the garage as a player would, by acting on the
    // thing in the world.
    IEnumerator Level(App app, int n, bool play)
    {
        var l = Ladder.Get(n);
        string tag = $"{n + 2:00}-{l.id}";
        app.StartLevel(n);
        yield return new WaitForSeconds(6.5f); // the transmission typed
        yield return Shot($"{tag}-1-briefing");

        app.ShowSetup();
        yield return new WaitForSeconds(n == 3 || n == 4 || n == 5 || n == 7 ? 6.2f : 3.2f); // the arrival, the cue
        yield return Shot($"{tag}-2-garage");
        if (n == 6)
        {
            var d = DecideScreen.Current;
            Coverage($"{tag}-2-garage: the agent", app.world.BodyOf(app.world.Highlighted));
            if (d != null)
            {
                for (int i = 0; i < Decide.Tasks.Length; i++) { d.Place(i, Decide.Tasks[i].right); yield return new WaitForSeconds(0.5f); }
                yield return new WaitForSeconds(0.8f);
                yield return Shot($"{tag}-3-decided");
                if (!play) yield break;
                ClickButton("Run them");
                yield return new WaitForSeconds(9f);
                yield return Shot($"{tag}-4-task");
                float waited = 0f;
                while (DecideDebrief.Current == null && waited < 420f)
                {
                    waited += Time.deltaTime;
                    if (ArenaScreen.Current != null && app.Sim != null && app.Sim.finished) { yield return new WaitForSeconds(2.4f); ClickButton("Next task"); ClickButton("Debrief"); yield return new WaitForSeconds(0.5f); }
                    yield return null;
                }
                yield return new WaitForSeconds(3.6f);
                yield return Shot($"{tag}-8-debrief");
                ClickButton("Details");
                yield return new WaitForSeconds(0.6f);
                yield return Shot($"{tag}-8b-debrief-details");
                ClickButton("Close");
            }
            yield break;
        }
        Coverage($"{tag}-2-garage: the agent", app.world.BodyOf(app.world.Highlighted));
        UiCheck($"{tag}-2-garage", app);
        WorldCheck($"{tag}-2-garage");
        var s = SetupScreen.Current;
        if (s == null) yield break;
        if (n == 1)
        {
            s.ReadPhone();
            yield return new WaitForSeconds(0.6f);
            yield return Shot($"{tag}-2b-phone");
            s.ReadPhone();
            yield return new WaitForSeconds(0.3f);
        }
        bool fixedIt = Fix(app, s, n);
        if (fixedIt) { yield return new WaitForSeconds(1.2f); yield return Shot($"{tag}-3-garage-set"); }
        if (!play) yield break;

        s.SendNow();
        float t = 0f;
        while ((ArenaScreen.Current == null || !ArenaScreen.Current.CurrentBanner.EndsWith("OUT")) && t < 14f) { t += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.25f);
        yield return Shot($"{tag}-4-agents-out");
        t = 0f;
        while (app.Sim != null && !app.Sim.finished && (app.Sim.turn < 3 || app.world.Animating) && t < 60f) { t += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.9f);
        yield return Shot($"{tag}-5-street");
        Coverage($"{tag}-5-street: the agent", app.world.BodyOf(app.world.Highlighted));
        UiCheck($"{tag}-5-street", app);
        WorldCheck($"{tag}-5-street");
        if (ArenaScreen.Current != null)
        {
            ArenaScreen.Current.SelectView(1);
            yield return new WaitForSeconds(1.8f);
            yield return Shot($"{tag}-5b-agents-eye");
            ArenaScreen.Current.SelectView(2);
            yield return new WaitForSeconds(1.8f);
            yield return Shot($"{tag}-5c-market");
            ArenaScreen.Current.SelectView(0);
        }
        // A moment, if one comes: sent home, out of tokens.
        t = 0f;
        while (ArenaScreen.Current != null && app.Sim != null && !app.Sim.finished && !(ArenaScreen.Current.CurrentBanner == "SENT HOME" || ArenaScreen.Current.CurrentBanner == "OUT OF TOKENS") && t < 60f) { t += Time.deltaTime; yield return null; }
        if (ArenaScreen.Current != null && (ArenaScreen.Current.CurrentBanner == "SENT HOME" || ArenaScreen.Current.CurrentBanner == "OUT OF TOKENS")) { yield return new WaitForSeconds(0.3f); yield return Shot($"{tag}-6-moment"); }
        t = 0f;
        while (app.Sim != null && !app.Sim.finished && t < 150f) { t += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(2.6f);
        yield return Shot($"{tag}-7-round-over");
        ClickButton("Debrief");
        t = 0f;
        while (ResultsScreen.Current == null && t < 10f) { t += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(3.6f); // the medal has risen, LUMA's line is up
        yield return Shot($"{tag}-8-debrief");
        if (n == 1 || n == 5)
        {
            ClickButton("Details");
            yield return new WaitForSeconds(0.6f);
            yield return Shot($"{tag}-8b-debrief-details");
            ClickButton("Close");
        }
    }

    // What a player does in the garage of each errand to set it right.
    static bool Fix(App app, SetupScreen s, int n)
    {
        var c = app.configs[0];
        switch (n)
        {
            case 2:
                foreach (var k in Items.Fruit) s.Drop(k.id, NoteText.Zone.Basket);
                s.Drop("tomato", NoteText.Zone.Crate);
                s.Drop("mushroom", NoteText.Zone.Crate);
                s.Write(c.instructions + " If nothing on this list is in view, keep walking.");
                return true;
            case 3:
                s.Fit(Market.Part.Vision, 2);
                s.Write(c.instructions + " If nothing red is in view, walk toward the middle of the market.");
                return true;
            case 4: s.Fit(Market.Part.Memory, MemoryModes.Notebook); return true;
            case 5: s.Fit(Market.Part.Model, 0); s.Fit(Market.Part.Effort, 0); return true;
            case 7: s.AddCoin(1); return true;
            default: return false;
        }
    }

    // Presses a button on the screen by its label.
    static void ClickButton(string label)
    {
        foreach (var b in FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None))
        {
            if (!b.isActiveAndEnabled || !b.interactable) continue;
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null && t.text == label) { b.onClick.Invoke(); return; }
        }
        Debug.Log($"[Walk] no button \"{label}\"");
    }

    // The cover, as Claude Bomb's: the agent close among the lit stalls of the evening
    // square (the title's picture), filling the whole window with nothing drawn over it.
    IEnumerator Cover(App app)
    {
        ClaudeAccess.ForceOffline = true;
        Luma.Silent = true;
        AudioListener.volume = 0f;
        yield return null;
        var top = app.Stage.parent;
        var hero = Ui.Node("Cover", top);
        Ui.Stretch(hero);
        foreach (Transform t in top) if (t != hero) t.gameObject.SetActive(false);
        app.Paper(Color.clear);
        if (Market.Real) app.world.ShowCover(hero, hero: true, heroConfig: app.configs[0]);
        else { app.world.ShowCover(hero); }
        yield return new WaitForSeconds(2.0f);
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (t.name == "turn") t.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(CoverPath);
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    // How much of the frame the agent's bounding box covers, as the realism pass measures it.
    static void Coverage(string what, Transform hero)
    {
        var cam = Camera.main;
        if (hero == null || cam == null) { Debug.Log($"[Coverage] {what}: none"); return; }
        var all = hero.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy && !(r is ParticleSystemRenderer)).ToArray();
        var rs = all.OfType<SkinnedMeshRenderer>().Any() ? all.OfType<SkinnedMeshRenderer>().Cast<Renderer>().ToArray() : all;
        if (rs.Length == 0) { Debug.Log($"[Coverage] {what}: no renderer"); return; }
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        float x0 = 1, x1 = 0, y0 = 1, y1 = 0;
        for (int i = 0; i < 8; i++)
        {
            var c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            var v = cam.WorldToViewportPoint(c);
            x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
        }
        float w = Mathf.Clamp01(x1) - Mathf.Clamp01(x0), h = Mathf.Clamp01(y1) - Mathf.Clamp01(y0);
        Debug.Log($"[Coverage] {what}: {w:P0} x {h:P0} of the frame = {w * h:P0}");
    }

    // The realism pass's UI rules, measured on a play screen: no light panel, the smallest text.
    static void UiCheck(string what, App app)
    {
        var canvas = app.Stage.parent;
        var light = new List<string>();
        foreach (var img in canvas.GetComponentsInChildren<UnityEngine.UI.Image>())
        {
            if (!img.enabled || !img.gameObject.activeInHierarchy) continue;
            var c = img.color; var r = img.rectTransform.rect;
            float lum = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            if (lum > 0.75f && c.a > 0.5f && r.width > 200 && r.height > 120) light.Add(img.name);
        }
        float minText = 999f; string smallest = "";
        foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>())
            if (t.enabled && t.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(t.text) && t.fontSize < minText) { minText = t.fontSize; smallest = t.text.Length > 30 ? t.text.Substring(0, 30) : t.text; }
        Debug.Log($"[UiCheck] {what}: {light.Count} light panel(s){(light.Count > 0 ? " (" + string.Join(", ", light.Take(6)) + ")" : "")}; smallest text {minText:F0} on the 1080 canvas (" + smallest + ")");
    }

    static void WorldCheck(string what)
    {
        var byShader = new Dictionary<string, int>();
        var flat = new List<string>();
        int total = 0;
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy || r is ParticleSystemRenderer) continue;
            total++;
            foreach (var m in r.sharedMaterials)
            {
                string sh = m != null && m.shader != null ? m.shader.name : "(none)";
                byShader[sh] = byShader.TryGetValue(sh, out var n) ? n + 1 : 1;
                if (sh.Contains("Flat")) flat.Add(r.name);
            }
        }
        Debug.Log($"[WorldCheck] {what}: {total} renderers; shaders: " + string.Join(", ", byShader.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value}")) +
                  $"; Flat: {flat.Count}{(flat.Count > 0 ? " (" + string.Join(", ", flat.Distinct().Take(8)) + ")" : "")}");
    }

    // The game-feel pass's counts on the screen just shot. A word is a whitespace-separated
    // token with a letter or a digit in it; the player's own text (a field, the note on the
    // agent) and Claude's words (the bubble, the thought) are left out and listed apart. A
    // control is a button or a field the player can press. Labels standing in the world
    // (TextMeshPro) count when the camera sees them.
    static void Count(string what)
    {
        int words = 0, controls = 0, playerWords = 0, claudeWords = 0;
        var flags = new List<string>();
        var lines = new List<string>();
        var lumaLines = new List<string>();
        bool Visible(CanvasGroup[] groups) => groups.All(g => g.alpha > 0.05f);
        foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
        {
            if (!t.isActiveAndEnabled || string.IsNullOrWhiteSpace(t.text) || t.color.a < 0.05f) continue;
            if (!Visible(t.GetComponentsInParent<CanvasGroup>(true))) continue;
            if (t is TextMeshPro world3d && !(world3d.renderer != null && world3d.renderer.isVisible)) continue;
            string shown = t.maxVisibleCharacters < t.textInfo.characterCount ? t.text.Substring(0, Mathf.Min(t.text.Length, t.maxVisibleCharacters)) : t.text;
            string plain = Regex.Replace(shown, "<[^>]+>", "");
            int n = plain.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries).Count(w => w.Any(char.IsLetterOrDigit));
            if (n == 0) continue;
            if (t.GetComponentInParent<TMP_InputField>() != null || t.name == "Note text") { playerWords += n; continue; }
            bool claude = t.transform.parent != null && (t.transform.parent.name == "Card" && t.transform.parent.parent != null && t.transform.parent.parent.name == "Bubble" || t.transform.parent.name == "Thought");
            if (claude) { claudeWords += n; if (Regex.IsMatch(plain, @"\(\s*\d+\s*,\s*\d+\s*\)")) flags.Add("coordinates in the bubble"); continue; }
            words += n;
            lines.Add(plain.Replace("\n", " / "));
            if (Regex.IsMatch(plain, @"OPENS AT", RegexOptions.IgnoreCase)) flags.Add("OPENS AT");
            if (Regex.IsMatch(plain, @"\bseed\b\s*[:=]?\s*\d", RegexOptions.IgnoreCase)) flags.Add("seed");
            if (Regex.IsMatch(plain, @"\(\s*\d+\s*,\s*\d+\s*\)")) flags.Add("coordinates");
            if (Regex.IsMatch(plain, @"\b0\.\d\d\b")) flags.Add("probability");
            if (t.transform.parent != null && t.transform.parent.name == "Luma line")
            {
                bool whole = t.maxVisibleCharacters >= t.textInfo.characterCount;
                lumaLines.Add($"{(whole ? "whole" : "CUT")} {n}w: {plain}");
                if (!whole) flags.Add("LUMA cut");
                if (n > Luma.CueWords) flags.Add("LUMA long");
            }
        }
        foreach (var s in FindObjectsByType<UnityEngine.UI.Selectable>(FindObjectsSortMode.None))
        {
            if (!s.isActiveAndEnabled || !s.interactable) continue;
            if (!Visible(s.GetComponentsInParent<CanvasGroup>(true))) continue;
            if (s.name == "Tap" || s.name == "Paint") continue; // a tap anywhere is not a control on the screen
            controls++;
        }
        int ladder = lines.Count(l => Regex.IsMatch(l, @"^\d\.\d+$"));
        if (ladder >= 6) flags.Add("ladder");
        var record = new Newtonsoft.Json.Linq.JObject
        {
            ["screen"] = what, ["words"] = words, ["controls"] = controls, ["playerWords"] = playerWords, ["claudeWords"] = claudeWords,
            ["flags"] = new Newtonsoft.Json.Linq.JArray(flags.Distinct()), ["luma"] = new Newtonsoft.Json.Linq.JArray(lumaLines), ["lines"] = new Newtonsoft.Json.Linq.JArray(lines),
        };
        File.AppendAllText(Path.Combine(Folder, "counts.jsonl"), record.ToString(Newtonsoft.Json.Formatting.None) + "\n");
        Debug.Log($"[Count] {what}: {words} words, {controls} controls, player {playerWords}, Claude {claudeWords}; flags: {(flags.Count == 0 ? "none" : string.Join(", ", flags.Distinct()))}");
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        Count(name);
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
