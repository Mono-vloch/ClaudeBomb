using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

// Walks through every screen of the pilot's levels (1.1 and 2.3) on its own and saves a
// screenshot of each, with the game-feel pass's counts: the words and the controls on
// each screen, what must not be there (OPENS AT, a ladder, coordinates, seeds,
// probabilities), and whether every LUMA line is whole and short.
//   "Robot Racing.app/Contents/MacOS/Robot Racing" -screenshots /some/folder
// Writes counts.jsonl beside the pictures.
public class DemoDriver : MonoBehaviour
{
    public static string Folder
    {
        get
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-screenshots") return args[i + 1];
            return null;
        }
    }

    // -cover <file>: one picture of the circuit for the platform's menu instead of the walk.
    public static string CoverPath
    {
        get
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-cover") return args[i + 1];
            return null;
        }
    }

    public static bool Wanted => Folder != null || CoverPath != null;

    IEnumerator Start()
    {
        var app = GetComponent<App>();
        if (CoverPath != null) { yield return Cover(app); yield break; }
        Directory.CreateDirectory(Folder);
        File.WriteAllText(Path.Combine(Folder, "counts.jsonl"), "");
        Luma.Silent = true;
        AudioListener.volume = 0f; // the walk is silent: it runs on a desk, not in a class
        PlayerPrefs.DeleteKey("robot_racing_seen_rewards");
        app.pace = 1;
        yield return new WaitForSeconds(1.2f);
        yield return Shot("01-title");

        // ---- level 1.1 ----
        app.StartLevel(1);
        yield return new WaitForSeconds(7f); // the transmission typed
        yield return Shot("02-briefing-1.1");

        app.ShowPit();
        yield return new WaitForSeconds(3.6f); // the car, then the track and the shelf
        yield return Shot("03-pit-lane-1.1");
        Coverage("03-pit-lane-1.1: the car", app.world.Car);
        app.world.PlaceMarker("finish", 100f);
        yield return new WaitForSeconds(0.8f);
        yield return Shot("03b-pit-lane-1.1-flag");

        app.StartTraining();
        yield return new WaitForSeconds(3.5f);
        yield return Shot("04-training-1.1");
        Coverage("04-training-1.1: the car", app.world.Car);
        yield return new WaitForSeconds(9f);
        yield return Shot("04b-training-1.1-later");
        float waited = 0;
        while (RunScreen.Current != null && !RunScreen.Current.RaceRunning && waited < 90f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(1.2f);
        yield return Shot("05-race-1.1");
        Coverage("05-race-1.1: the car", app.world.Car);
        waited = 0;
        while (RunScreen.Current != null && RunScreen.Current.phase != RunScreen.Phase.Done && waited < 60f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(2.2f);
        yield return Shot("05b-race-over-1.1");
        app.FinishLevel();
        yield return new WaitForSeconds(1.6f);
        yield return Shot("06-questions-1.1");
        for (int i = 0; i < 7 && QuizScreen.Current != null; i++)
        {
            if (i == 5) { yield return new WaitForSeconds(0.4f); yield return Shot("06b-questions-1.1-signs"); }
            QuizScreen.Current?.AutoAnswer();
            yield return new WaitForSeconds(1.2f);
        }
        waited = 0;
        while (ResultsScreen.Current == null && waited < 10f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(3.2f); // the medal has been revealed, LUMA's line is up
        yield return Shot("07-debrief-1.1");
        ClickButton("Details");
        yield return new WaitForSeconds(0.6f);
        yield return Shot("07b-debrief-1.1-details");

        // ---- level 2.3 ----
        app.StartLevel(Ladder.PointsLevel);
        yield return new WaitForSeconds(6f);
        yield return Shot("08-briefing-2.3");
        app.ShowPit();
        yield return new WaitForSeconds(5.5f); // the shelf arrives
        yield return Shot("09-pit-lane-2.3");
        Coverage("09-pit-lane-2.3: the car", app.world.Car);
        app.world.PlaceMarker("crash", -100f);
        yield return new WaitForSeconds(0.4f);
        app.world.PlaceMarker("toward", 2f);
        yield return new WaitForSeconds(0.4f);
        app.world.PlaceMarker("away", -2f);
        yield return new WaitForSeconds(0.8f);
        yield return Shot("09b-pit-lane-2.3-placed");
        app.StartTraining();
        yield return new WaitForSeconds(3.5f);
        yield return Shot("10-training-2.3");
        waited = 0;
        while (RunScreen.Current != null && !RunScreen.Current.BannerUp && waited < 40f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
        yield return Shot("10b-training-2.3-moment");
        ClickButton("Details");
        yield return new WaitForSeconds(0.8f);
        yield return Shot("10c-training-2.3-details");
        ClickButton("Close");
        waited = 0;
        while (RunScreen.Current != null && !RunScreen.Current.RaceRunning && waited < 90f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(1.5f);
        yield return Shot("11-race-2.3");
        Coverage("11-race-2.3: the car", app.world.Car);
        waited = 0;
        while (RunScreen.Current != null && RunScreen.Current.phase != RunScreen.Phase.Done && waited < 60f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(2.2f);
        app.FinishLevel();
        yield return new WaitForSeconds(3.2f);
        yield return Shot("12-debrief-2.3");

        app.menu.Open();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("13-menu");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
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
            if (Regex.IsMatch(plain, @"\bseed\b", RegexOptions.IgnoreCase)) flags.Add("seed");
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
            if (s.GetComponent<UnityEngine.UI.Image>() != null && s.GetComponent<UnityEngine.UI.Image>().color.a < 0.02f && s.name.StartsWith("Level ")) { controls++; continue; }
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
        foreach (var m in world.Markers) { var sp = world.ScreenOf(m.root.position + Vector3.up * 0.3f); Debug.Log($"[Markers] {name}: {m.kind} at screen {sp.x:0},{sp.y:0} z {sp.z:0.0} value {m.Value}"); }
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
