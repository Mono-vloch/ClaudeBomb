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
// line is whole and short. Every level gets its briefing and its pit lane; 1.1, 1.2, 2.3,
// 2.5 and 3.4 are played through: training, race, questions, debrief.
//   "Robot Racing.app/Contents/MacOS/Robot Racing" -screenshots /some/folder [-levels 1,6]
// Writes counts.jsonl beside the pictures.
public class DemoDriver : MonoBehaviour
{
    public static string Folder => Arg("-screenshots");

    // -cover <file>: one picture of the circuit for the platform's menu instead of the walk.
    public static string CoverPath => Arg("-cover");

    static string Arg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return null;
    }

    public static bool Wanted => Folder != null || CoverPath != null;

    static readonly int[] Played = { 1, 2, 6, 8, 13 };

    IEnumerator Start()
    {
        var app = GetComponent<App>();
        if (CoverPath != null) { yield return Cover(app); yield break; }
        Directory.CreateDirectory(Folder);
        File.WriteAllText(Path.Combine(Folder, "counts.jsonl"), "");
        Luma.Silent = true;
        AudioListener.volume = 0f; // the walk is silent: it runs on a desk, not in a class
        for (int i = 0; i <= Ladder.Count; i++) PlayerPrefs.DeleteKey("robot_racing_seen_" + i);
        PlayerPrefs.DeleteKey("robot_racing_seen_rewards");
        app.pace = 1;
        var only = Arg("-levels")?.Split(',').Select(int.Parse).ToList();
        yield return new WaitForSeconds(1.2f);
        yield return Shot("01-title");
        ClickButton("Levels");
        yield return new WaitForSeconds(0.5f);
        yield return Shot("01b-levels");
        ClickButton("Close");

        for (int n = 1; n <= Ladder.Count; n++)
        {
            if (only != null && !only.Contains(n)) continue;
            yield return Level(app, n, Played.Contains(n));
        }

        app.menu.Open();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("99-menu");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    // One level: the briefing and the pit lane always; the run, the questions and the
    // debrief when it is played. The planned mistake is fixed in the pit lane as a player
    // would, by acting on the thing in the world.
    IEnumerator Level(App app, int n, bool play)
    {
        var l = Ladder.Get(n);
        string tag = $"{n:00}-{l.id}";
        app.StartLevel(n);
        yield return new WaitForSeconds(7f); // the transmission typed
        yield return Shot($"{tag}-1-briefing");

        app.ShowPit();
        yield return new WaitForSeconds(5.5f); // the car, the track, the shelf; the arrival
        yield return Shot($"{tag}-2-pit-lane");
        Coverage($"{tag}-2-pit-lane: the car", app.world.Car);
        bool fixedIt = Fix(app, n);
        if (fixedIt) { yield return new WaitForSeconds(0.9f); yield return Shot($"{tag}-3-pit-lane-set"); }
        if (!play) yield break;

        if (l.prediction) app.session.prediction = "It will learn the bend and finish most laps.";
        app.StartTraining();
        yield return new WaitForSeconds(3.5f);
        yield return Shot($"{tag}-4-training");
        Coverage($"{tag}-4-training: the car", app.world.Car);
        float waited = 0;
        while (RunScreen.Current != null && !RunScreen.Current.BannerUp && waited < 40f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
        yield return Shot($"{tag}-5-training-moment");
        if (n == 6)
        {
            ClickButton("Details");
            yield return new WaitForSeconds(0.8f);
            yield return Shot($"{tag}-5b-training-details");
            ClickButton("Close");
        }
        waited = 0;
        while (RunScreen.Current != null && !RunScreen.Current.RaceRunning && waited < 150f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(1.5f);
        yield return Shot($"{tag}-6-race");
        Coverage($"{tag}-6-race: the car", app.world.Car);
        waited = 0;
        while (RunScreen.Current != null && RunScreen.Current.phase != RunScreen.Phase.Done && waited < 240f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(2.2f);
        yield return Shot($"{tag}-7-race-over");
        app.FinishLevel();
        yield return new WaitForSeconds(1.6f);
        int q = 0;
        while (QuizScreen.Current != null && q < 12)
        {
            yield return Shot($"{tag}-8-question-{q + 1}");
            QuizScreen.Current?.AutoAnswer();
            yield return new WaitForSeconds(1.4f);
            q++;
        }
        waited = 0;
        while (ResultsScreen.Current == null && waited < 10f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(3.4f); // the medal has risen, LUMA's line is up
        yield return Shot($"{tag}-9-debrief");
        if (n == 1 || n == 13)
        {
            ClickButton("Details");
            yield return new WaitForSeconds(0.6f);
            yield return Shot($"{tag}-9b-debrief-details");
            ClickButton("Close");
        }
    }

    // What a player does in the pit lane of each level to set it right.
    static bool Fix(App app, int n)
    {
        var w = app.world;
        switch (n)
        {
            case 1: w.PlaceMarker("finish", 100f); return true;
            case 2: if (app.settings.stopAtFirstFinish) w.TapMarker("stopcap"); return true;
            case 4: for (int i = 0; i < Sensors.Count; i++) if (app.settings.sensors[i] != Lesson.Fixed.sensors[i]) w.TapMarker("sense" + i); return true;
            case 5: for (int i = 0; i < Moves.Count; i++) if (!app.settings.moves[i]) w.TapMarker("move" + i); return true;
            case 6: w.PlaceMarker("crash", -100f); w.PlaceMarker("toward", 2f); w.PlaceMarker("away", -2f); return true;
            case 7: if (app.settings.speedBonus > 0) w.PlaceMarker("bonus", 0f); return true;
            case 8: if (app.settings.exploreOffAfterFinish) w.TapMarker("nevercap"); return true;
            case 10: if (!app.settings.training.Contains("bend")) w.TapMarker("track:bend"); return true;
            case 11: w.PlaceMarker("measure", 2f); return true;
            case 12: w.PlaceMarker("rewrite", 0.1f); return true;
            default: return false;
        }
    }

    // Presses a button on the screen by its label.
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

    // The cover, as Claude Bomb's: the robot car on the grid at dusk, the trackside camera's
    // feed over it, filling the whole window with nothing else drawn over it.
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
        app.world.Build(Track.Get("grand"), true);
        if (Circuit.HasKit)
        {
            app.world.ShowCarOrbit(frame, 1.32f, 8f);
            float yaw = -30f;
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-coveryaw") float.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out yaw);
            app.world.cam.orbitBase += yaw; app.world.cam.orbitArc = 0.01f; app.world.cam.orbitSpeed = 0f; app.world.cam.Snap();
            CameraFeed.Build(frame, "CAM 3  /  THE GRID", "MAALOOMATIIA RACEWAY  /  EVENING PRACTICE");
        }
        else { app.world.ShowOverview(frame); app.world.cam.pitch = 40; app.world.cam.yaw = 28; app.world.cam.Snap(); app.world.Demo(true); }
        yield return new WaitForSeconds(2.5f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(CoverPath);
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    // How much of the frame the car's bounding box covers (the realism pass's measure).
    static void Coverage(string what, Transform car)
    {
        var cam = Camera.main;
        if (car == null || cam == null) { Debug.Log($"[Coverage] {what}: none"); return; }
        var rs = car.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy && !(r is TrailRenderer) && !(r is ParticleSystemRenderer) && r.name != "beam").ToArray();
        if (rs.Length == 0) { Debug.Log($"[Coverage] {what}: no renderers"); return; }
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
            if (t.GetComponentInParent<TMP_InputField>() != null) continue; // the player's own text, or its placeholder
            string shown = t.maxVisibleCharacters < t.textInfo.characterCount ? t.text.Substring(0, Mathf.Min(t.text.Length, t.maxVisibleCharacters)) : t.text;
            string plain = Regex.Replace(shown, "<[^>]+>", "");
            int n = plain.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries).Count(w => w.Any(char.IsLetterOrDigit));
            if (n == 0) continue;
            words += n;
            lines.Add(plain.Replace("\n", " / "));
            if (Regex.IsMatch(plain, @"OPENS AT", RegexOptions.IgnoreCase)) flags.Add("OPENS AT");
            if (Regex.IsMatch(plain, @"\bseed\b\s*[:=]?\s*\d", RegexOptions.IgnoreCase)) flags.Add("seed"); // a seed's number, not the word
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
        var record = new { screen = what, words, controls, flags = flags.Distinct().ToList(), luma = lumaLines, lines };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(record);
        File.AppendAllText(Path.Combine(Folder, "counts.jsonl"), json + "\n");
        Debug.Log($"[Count] {what}: {words} words, {controls} controls; flags: {(flags.Count == 0 ? "none" : string.Join(", ", flags.Distinct()))}");
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        Count(name);
        var world = GetComponent<App>().world;
        foreach (var m in world.Markers) { if (m.root == null) continue; var sp = world.ScreenOf(m.root.position + Vector3.up * 0.3f); Debug.Log($"[Markers] {name}: {m.kind} at screen {sp.x:0},{sp.y:0} z {sp.z:0.0} value {m.Value}{(m.dim ? " dim" : "")}"); }
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
