using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

// Walks through the game on its own and saves a screenshot of each screen, with the game-feel
// pass's counts: the words and the controls on each screen, what must not be there (OPENS AT,
// a ladder, coordinates, seeds, probabilities), and whether every LUMA line is whole and short.
// Every level gets its briefing, its room as handed over and its room set right; 1.1, 1.3 and
// 2.4 are played through: the rehearsal (1.1), the attack, the debrief and its details.
//   "Tech Escape.app/Contents/MacOS/Tech Escape" -screenshots /some/folder [-levels 1,9]
// Writes counts.jsonl beside the pictures. The walk is silent.
public class DemoDriver : MonoBehaviour
{
    public static string Folder => Arg("-screenshots");
    public static string CoverPath => Arg("-cover");

    static string Arg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return null;
    }

    public static bool Wanted => Folder != null || CoverPath != null;

    static readonly int[] Played = { 1, 3, 9 };

    IEnumerator Start()
    {
        var app = GetComponent<App>();
        if (CoverPath != null) { yield return Cover(app); yield break; }
        Directory.CreateDirectory(Folder);
        File.WriteAllText(Path.Combine(Folder, "counts.jsonl"), "");
        Luma.Silent = true;
        AudioListener.volume = 0f;
        app.ResetProgress(); // a fresh student: the rehearsal shows, the map is empty
        app.ShowTitle();
        var only = Arg("-levels")?.Split(',').Select(int.Parse).ToList();
        yield return new WaitForSeconds(1.5f);
        yield return Shot("01-title");
        ClickButton("Levels");
        yield return new WaitForSeconds(0.5f);
        yield return Shot("01b-levels");
        ClickButton("Close");

        for (int n = 1; n <= Rooms.Count; n++)
        {
            if (only != null && !only.Contains(n)) continue;
            yield return Level(app, n, Played.Contains(n));
        }

        app.ShowFile();
        yield return new WaitForSeconds(0.6f);
        yield return Shot("98-file");
        FileSheet.Close();
        app.menu.Open();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("99-menu");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    // One level: the briefing, the room as handed over, the room set right; when it is played,
    // the rehearsal on the untouched room first (1.1), then the attack and the debrief.
    IEnumerator Level(App app, int n, bool play)
    {
        var l = Rooms.Get(n);
        string tag = $"{n:00}-{l.id}";
        app.StartLevel(n);
        yield return new WaitForSeconds(7f); // the transmission typed
        yield return Shot($"{tag}-1-briefing");

        app.ShowRoom();
        yield return new WaitForSeconds(11f); // LUMA's cue said and gone: the room as the player works in it
        yield return Shot($"{tag}-2-room");
        Coverage($"{tag}-2-room: the hero", app.world.HeroOf(Facility.RoomIndex(l.room)));
        DumpMonitor(app, $"{tag}-2-monitor");
        if (play && n == 1)
        {
            app.RunAttack();
            yield return new WaitForSeconds(4.2f);
            yield return Shot($"{tag}-3-rehearsal");
            float w0 = 0f;
            while (AttackScreen.Running && w0 < 60f) { w0 += Time.unscaledDeltaTime; yield return null; }
            yield return new WaitForSeconds(0.6f);
            yield return Shot($"{tag}-3b-rehearsal-end");
            ClickButton("Back to the room");
            yield return new WaitForSeconds(3.2f);
            yield return Shot($"{tag}-3c-room-after-rehearsal");
        }
        if (RoomScreen.Current != null && RoomScreen.Current.Rig != null && RoomScreen.Current.Rig.Tokens.Count > 0)
        {
            var first = RoomScreen.Current.Rig.Tokens.First(t => !t.loose && !t.sign && !t.console);
            RoomScreen.Current.Rig.Select(first);
            yield return new WaitForSeconds(1.6f); // the camera glides to face the card
            yield return Shot($"{tag}-4-card-close");
            var console = RoomScreen.Current.Rig.Tokens.FirstOrDefault(t => t.console);
            if (console != null)
            {
                RoomScreen.Current.Rig.Select(console);
                yield return new WaitForSeconds(1.6f);
                yield return Shot($"{tag}-4b-console-close");
            }
            RoomScreen.Current.Rig.Select(null);
            yield return new WaitForSeconds(1.2f);
        }
        RoomScreen.Current?.AutoFix();
        yield return new WaitForSeconds(1.2f);
        yield return Shot($"{tag}-5-room-set");
        if (play)
        {
            ClickButton("Camera");
            yield return new WaitForSeconds(1.4f);
            yield return Shot($"{tag}-5b-close-up");
            ClickButton("Camera");
            yield return new WaitForSeconds(0.6f);
            app.RunAttack();
            yield return new WaitForSeconds(4.2f);
            yield return Shot($"{tag}-6-attack");
            Coverage($"{tag}-6-attack: the attacker", app.world.AttackerTransform);
            float waited = 0f;
            while (AttackScreen.Running && waited < 60f) { waited += Time.unscaledDeltaTime; yield return null; }
            yield return new WaitForSeconds(0.6f);
            yield return Shot($"{tag}-6b-attack-end");
            ClickButton("Debrief");
            yield return new WaitForSeconds(3.6f); // the medal has risen, LUMA's line is up
            yield return Shot($"{tag}-7-debrief");
            ClickButton("Details");
            yield return new WaitForSeconds(0.6f);
            yield return Shot($"{tag}-7b-debrief-details");
            ClickButton("Close");
        }
    }

    // What the corridor monitor shows, as a picture beside the frame.
    static void DumpMonitor(App app, string name)
    {
        var rt = app.world.MonitorTexture;
        if (rt == null) { Debug.Log("[Monitor] no texture"); return; }
        var cam = app.world.MonitorCamera;
        Debug.Log($"[Monitor] camera {(cam != null ? cam.transform.position.ToString() : "none")} enabled {(cam != null && cam.enabled)} mask {(cam != null ? cam.cullingMask : 0)} rt {rt.width}x{rt.height}");
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        File.WriteAllBytes(Path.Combine(Folder, name + ".png"), tex.EncodeToPNG());
        Destroy(tex);
    }

    static void ClickButton(string label)
    {
        foreach (var b in FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None))
        {
            if (!b.isActiveAndEnabled) continue;
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null && t.text == label) { b.onClick.Invoke(); return; }
        }
        Debug.Log($"[Walk] no button \"{label}\"");
    }

    // The cover, as Claude Bomb's: the post room's mail sorter under its lamp, seen through the
    // security camera's feed, filling the whole window with nothing else drawn over it.
    IEnumerator Cover(App app)
    {
        Luma.Silent = true;
        AudioListener.volume = 0f;
        yield return null;
        var top = app.Stage.parent;
        var frame = Ui.Node("Cover", top);
        Ui.Stretch(frame);
        foreach (Transform t in top) if (t != frame) t.gameObject.SetActive(false);
        app.Paper(Color.clear);
        app.world.Build();
        int index = Facility.RoomIndex("Suspicious Inbox");
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == index ? Building.DoorState.Open : i < index ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowRoom(frame, index);
        app.atmosphere.MenuBlur(false);
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (t.name == "Rack") t.gameObject.SetActive(false);
        if (app.world.HasInterior(index)) CameraFeed.Build(frame, $"CAM {index + 1:00}  /  SUSPICIOUS INBOX", "PERMIT OFFICE  /  LOCKDOWN");
        else { app.world.ShowOverview(frame); app.world.cam.pitch = 38; app.world.cam.yaw = 32; app.world.cam.Snap(); app.world.Demo(true); }
        yield return new WaitForSeconds(2.5f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(CoverPath);
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    // How much of the frame the hero's bounding box covers, as the realism pass measures it.
    static void Coverage(string what, Transform hero)
    {
        var cam = Camera.main;
        if (hero == null || cam == null) { Debug.Log($"[Coverage] {what}: none"); return; }
        var all = hero.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy && !(r is ParticleSystemRenderer)).ToArray();
        var rs = all.OfType<SkinnedMeshRenderer>().Any() ? all.OfType<SkinnedMeshRenderer>().Cast<Renderer>().ToArray() : all;
        if (rs.Length == 0) return;
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

    // The game-feel pass's counts on the screen just shot. A word is a whitespace-separated
    // token with a letter or a digit in it; the player's own typed text is left out. A control
    // is a button or a field the player can press. Labels standing in the world (TextMeshPro)
    // count when the camera sees them.
    static void Count(string what)
    {
        int words = 0, controls = 0;
        var flags = new List<string>();
        var lines = new List<string>();
        var lumaLines = new List<string>();
        bool Visible(CanvasGroup[] groups) => groups.All(g => g.alpha > 0.05f);
        foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
        {
            if (!t.isActiveAndEnabled || string.IsNullOrWhiteSpace(t.text) || t.color.a < 0.05f) continue;
            if (!Visible(t.GetComponentsInParent<CanvasGroup>(true))) continue;
            if (t is TextMeshPro world3d && !(world3d.renderer != null && world3d.renderer.isVisible)) continue;
            if (t is TextMeshPro && t.name != "Text" && t.name != "Step") continue; // the kit's door signs, stencils and exit sign: signage, not the screen's words
            if (t.GetComponentInParent<TMP_InputField>() != null) continue;
            string shown = t.maxVisibleCharacters < t.textInfo.characterCount ? t.text.Substring(0, Mathf.Min(t.text.Length, t.maxVisibleCharacters)) : t.text;
            string plain = Regex.Replace(shown, "<[^>]+>", "");
            int n = plain.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries).Count(w => w.Any(char.IsLetterOrDigit));
            if (n == 0) continue;
            words += n;
            lines.Add((t is TextMeshPro ? "[world] " : "") + plain.Replace("\n", " / "));
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
            controls++;
        }
        int ladder = lines.Count(l => Regex.IsMatch(l, @"^\d\.\d+$"));
        if (ladder >= 6) flags.Add("ladder");
        string Q(string x) => "\"" + x.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ") + "\"";
        string Arr(IEnumerable<string> xs) => "[" + string.Join(",", xs.Select(Q)) + "]";
        string json = $"{{\"screen\":{Q(what)},\"words\":{words},\"controls\":{controls},\"flags\":{Arr(flags.Distinct())},\"luma\":{Arr(lumaLines)},\"lines\":{Arr(lines)}}}";
        File.AppendAllText(Path.Combine(Folder, "counts.jsonl"), json + "\n");
        Debug.Log($"[Count] {what}: {words} words, {controls} controls; flags: {(flags.Count == 0 ? "none" : string.Join(", ", flags.Distinct()))}");
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        Count(name);
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
