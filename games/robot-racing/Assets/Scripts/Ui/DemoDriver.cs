using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

// Walks through every screen on its own and saves a screenshot of each, so the
// layout can be checked from the command line:
//   "Robot Racing.app/Contents/MacOS/Robot Racing" -screenshots /some/folder
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
        Luma.Silent = true;
        AudioListener.volume = 0f; // the walk is silent: it runs on a desk, not in a class
        yield return new WaitForSeconds(1.2f);
        yield return Shot("1-title");
        Checks("1-title");

        app.StartLevel(1);
        yield return new WaitForSeconds(2.5f); // LUMA's line has been typed by then
        yield return Shot("2-briefing");
        Checks("2-briefing");
        app.ShowFile();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("2c-file");
        app.ShowFile();

        app.StartLevel(Ladder.PointsLevel);
        app.ShowPit();
        yield return new WaitForSeconds(4.5f); // LUMA's line typed out
        yield return Shot("3-pit-lane");
        Coverage("3-pit-lane: the car", app.world.Car);
        Checks("3-pit-lane");
        app.StartLevel(1);
        app.ShowPit();
        yield return new WaitForSeconds(9f); // LUMA's line said and gone: the pit lane as the player works in it
        yield return Shot("3b-pit-lane-1.1");
        Coverage("3b-pit-lane-1.1: the car", app.world.Car);

        app.StartLevel(1);
        app.StartTraining();
        yield return new WaitForSeconds(1.5f);
        RunScreen.Current?.SetSpeed(0);
        yield return new WaitForSeconds(2.2f);
        yield return Shot("4-training-slow");
        Coverage("4-training-slow: the car", app.world.Car);
        Checks("4-training-slow");
        RunScreen.Current?.SetSpeed(3);
        float waited = 0;
        while (RunScreen.Current != null && RunScreen.Current.phase != RunScreen.Phase.Done && waited < 60f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
        app.FinishLevel();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("6-debrief-questions");
        ResultsScreen.Current?.SkipQuiz();
        yield return new WaitForSeconds(2.4f); // the medal has been revealed by then
        yield return Shot("7-debrief");
        Checks("7-debrief");

        // The race on the grand circuit (2.6's robot learns it; its laps are long): at 1x, between the moments.
        app.StartLevel(9);
        app.StartTraining();
        yield return new WaitForSeconds(0.5f);
        RunScreen.Current?.SetSpeed(3);
        waited = 0;
        while (RunScreen.Current != null && !RunScreen.Current.RaceRunning && waited < 60f) { waited += Time.deltaTime; yield return null; }
        RunScreen.Current?.SetSpeed(0);
        yield return new WaitForSeconds(1.0f);
        float clear = 0f; waited = 0f;
        var rig = app.world.Car != null ? app.world.Car.GetComponentInChildren<CarRig>() : null;
        while (RunScreen.Current != null && RunScreen.Current.phase == RunScreen.Phase.Racing && clear < 0.4f && waited < 40f)
        {
            // Between the moments, on a straight (the chase settled behind the car).
            waited += Time.unscaledDeltaTime;
            clear = RunScreen.Current.BannerUp || (rig != null && Mathf.Abs(rig.Steer) > 3f) ? 0f : clear + Time.unscaledDeltaTime;
            if (Mathf.Repeat(waited, 1f) < Time.unscaledDeltaTime) Debug.Log($"[Walk] race wait {waited:0.0}: banner {RunScreen.Current.BannerUp}, steer {(rig != null ? rig.Steer : -1):0.0}, clear {clear:0.00}");
            yield return null;
        }
        yield return Shot("5-race");
        Coverage("5-race: the car", app.world.Car);
        Checks("5-race");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
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
            // Held still at -coveryaw degrees from straight ahead of the car (default -30), so
            // the picture is the same every time.
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

    // How much of the frame the car's bounding box covers, as the realism pass measures it:
    // the body and its wheels, not the sensor beams or the trails.
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

    // The realism pass's checks on the screen just shot: no Flat shader and no MeshKit mesh on
    // the world (the effects excepted: the sensor beams, trails, ghost lines, particles), no
    // light panel over the world, and no text the player reads under 16 px at 1080p.
    static void Checks(string what)
    {
        int flat = 0, kit = 0, world = 0;
        var meshKit = new System.Collections.Generic.HashSet<Mesh> { MeshKit.Box, MeshKit.Cylinder, MeshKit.Sphere, MeshKit.Cone, MeshKit.Capsule, MeshKit.Dome, MeshKit.Pear, MeshKit.Pyramid, MeshKit.Tube };
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer || r.name == "beam") continue; // effects
            world++;
            if (r.sharedMaterials.Any(m => m != null && m.shader != null && m.shader.name.Contains("Flat"))) { flat++; if (flat <= 3) Debug.Log($"[Check] {what}: Flat on {r.name}"); }
            var f = r.GetComponent<MeshFilter>();
            if (f != null && meshKit.Contains(f.sharedMesh)) { kit++; if (kit <= 3) Debug.Log($"[Check] {what}: MeshKit mesh on {r.name}"); }
        }
        int light = 0, small = 0; float smallest = 99f;
        foreach (var img in Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None))
        {
            if (!img.isActiveAndEnabled || img.canvas == null || img.GetComponentInParent<CanvasGroup>()?.alpha < 0.05f) continue;
            var c = img.color; var rt = img.rectTransform.rect;
            float lum = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            if (c.a > 0.5f && lum > 0.75f && rt.width * rt.height > 60000f) { light++; Debug.Log($"[Check] {what}: light panel {img.name} {rt.width:0}x{rt.height:0}"); }
        }
        foreach (var t in Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (!t.isActiveAndEnabled || string.IsNullOrWhiteSpace(t.text) || t.color.a < 0.05f) continue;
            float size = t.enableAutoSizing ? t.fontSizeMin : t.fontSize;
            smallest = Mathf.Min(smallest, size);
            if (size < 16f) { small++; if (small <= 5) Debug.Log($"[Check] {what}: {size} px text \"{t.text.Substring(0, Mathf.Min(30, t.text.Length))}\""); }
        }
        Debug.Log($"[Check] {what}: {world} world renderers, {flat} Flat, {kit} MeshKit; {light} light panels; {small} texts under 16 px (smallest {smallest})");
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
