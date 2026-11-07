using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Walks through every screen on its own and saves a screenshot of each, so the
// layout can be checked from the command line:
//   "AI Arena.app/Contents/MacOS/AI Arena" -screenshots /some/folder [-live]
// Uses the stand-in brain, so no Claude call is made, unless -live is given: then
// two agents play a short round through whatever Claude access the machine has.
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

    static bool Live => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-live") >= 0;

    // -cover <file>: one picture of the market for the platform's menu instead of the walk.
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
        if (!Live) ClaudeAccess.ForceOffline = true;
        yield return new WaitForSeconds(1f);
        yield return Shot("1-title");

        Luma.Silent = true; // the walk is silent
        AudioListener.volume = 0f;
        app.ShowStory(null);
        yield return new WaitForSeconds(2.5f);
        yield return Shot("2-story");
        float waitedBeat = 0;
        while (app.story != null && app.story.Current < 4 && waitedBeat < 60f) { waitedBeat += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(5f);
        yield return Shot("2b-story-market");
        waitedBeat = 0;
        while (app.story != null && app.story.Current < 5 && waitedBeat < 60f) { waitedBeat += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(4.5f);
        yield return Shot("2c-story-tomato");
        app.StartLevel(1);
        yield return new WaitForSeconds(2.5f); // LUMA's line has been typed by then
        yield return Shot("3-lesson");
        app.ShowFile();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("3c-file");
        app.ShowFile();
        Tour.Seen = false;
        app.ShowSetup();
        yield return new WaitForSeconds(1.5f);
        yield return Shot("4-tour");
        app.StartLevel(Ladder.DecideLevel);
        app.ShowSetup();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("4b-decide");
        app.StartLevel(Ladder.Count);

        app.configs = new List<AgentConfig>
        {
            new AgentConfig { name = "Nadia", colour = 0, model = 2, vision = 1, memory = 1, effort = 0,
                buy = new List<string> { "banana", "lemon", "pear", "corn", "broccoli" },
                never = new List<string> { "apple", "orange", "strawberry", "grapes", "cherry", "carrot", "potato", "mushroom", "chili", "tomato" },
                fallback = 2, words = "An orange is orange, so no." },
            new AgentConfig { name = "Omar", colour = 1, model = 0, vision = 2, memory = 0, effort = 1, freeText = true,
                instructions = "Get fruit. No vegetables." },
            new AgentConfig { name = "Lina", colour = 3, model = 1, vision = 3, memory = 2, effort = 2, freeText = true,
                instructions = "Buy food." },
            new AgentConfig { name = "Sami", colour = 4, model = 2, vision = 1, memory = 3, effort = 0, freeText = true,
                instructions = "Only yellow and green things: bananas, lemons, corn, pears and broccoli. Leave everything else at the market." },
        };
        app.turns = 8;
        if (Live)
        {
            app.configs = new List<AgentConfig> { app.configs[0], app.configs[1] };
            app.configs[1].memory = 3; // exercise the notebook
            app.turns = 4;
        }
        app.ShowSetup();
        yield return new WaitForSeconds(1.2f);
        yield return Shot("5-setup");
        Coverage("5-setup: the agent in the garage", app.world.BodyOf(app.world.Highlighted));
        UiCheck("5-setup", app);

        app.StartRound();
        float waited = 0;
        int shotAt = Live ? 2 : 4;
        float shotTimeout = Live ? 180f : 60f;
        while (app.Sim != null && !app.Sim.finished && app.Sim.turn < shotAt && waited < shotTimeout) { waited += Time.deltaTime; yield return null; }
        // The main play screen as the player sees it between steps: the agent standing, LUMA's line said.
        waited = 0;
        while (app.world.Animating && waited < 10f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.9f);
        yield return Shot("6-market");
        Coverage("6-market: the agent on the street (main play screen)", app.world.BodyOf(app.world.Highlighted));
        UiCheck("6-market", app);
        WorldCheck("6-market");
        if (Market.Real && ArenaScreen.Current != null) ArenaScreen.Current.SelectView(1); else app.world.SetView(Market.View.RobotsEye);
        yield return new WaitForSeconds(1.8f);
        yield return Shot("6b-agents-eye");
        Coverage("6b-agents-eye: the agent", app.world.BodyOf(app.world.Highlighted));
        if (Market.Real)
        {
            if (ArenaScreen.Current != null) ArenaScreen.Current.SelectView(2); else app.world.SetView(Market.View.Market);
            yield return new WaitForSeconds(1.8f);
            yield return Shot("6c-overview");
            if (ArenaScreen.Current != null) ArenaScreen.Current.SelectView(0); else app.world.SetView(Market.View.Street);
        }
        else app.world.SetView(Market.View.Market);
        waited = 0;
        while (app.Sim != null && !app.Sim.finished && waited < (Live ? 300f : 150f)) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
        app.ShowResults(app.CurrentArena, app.Sim);
        yield return new WaitForSeconds(2.6f); // the medal has been revealed by then
        yield return Shot("7-debrief");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    // The cover, as Claude Bomb's: the agent close among the lit stalls of the evening
    // square (the title's picture), filling the whole window with nothing drawn over it.
    // Without the kit, the market day from above as before.
    IEnumerator Cover(App app)
    {
        ClaudeAccess.ForceOffline = true;
        Luma.Silent = true;
        AudioListener.volume = 0f;
        yield return null; // the title has built by now; the market takes its place
        if (Market.Real)
        {
            var top = app.Stage.parent;
            var hero = Ui.Node("Cover", top);
            Ui.Stretch(hero);
            foreach (Transform t in top) if (t != hero) t.gameObject.SetActive(false);
            app.Paper(Color.clear);
            app.world.ShowCover(hero, hero: true);
            yield return new WaitForSeconds(2.0f);
            // The memory's glass cards read as a glitch without the game round them.
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (t.name == "turn") t.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(CoverPath);
            yield return new WaitForSeconds(1f);
            Application.Quit();
            yield break;
        }
        var rng = new System.Random(4);
        var configs = new List<AgentConfig>();
        for (int i = 0; i < 4; i++) configs.Add(new AgentConfig { name = Theme.TeamNames[i], colour = i, vision = 1 + i % 2 });
        var arena = new Arena(Brief.Make(4, rng), configs, 30_000, 17);
        var spots = new[] { new Vector2Int(2, 3), new Vector2Int(6, 2), new Vector2Int(3, 7), new Vector2Int(7, 6) };
        for (int i = 0; i < 4; i++)
        {
            var a = arena.agents[i];
            if (arena.ItemAt(spots[i]) != null) arena.RemoveItem(spots[i]);
            a.pos = spots[i];
            a.facing = (i * 3) % 4;
        }
        var canvas = app.Stage.parent.GetComponent<Canvas>();
        var frame = Ui.Node("Cover", app.Stage.parent);
        Ui.Stretch(frame, 40, 40, 40, 40);
        app.Paper(Color.clear);
        app.world.Build(arena, false);
        app.world.ShowMarket(frame);
        app.world.Highlight(arena.agents[1]);
        canvas.enabled = false;
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(CoverPath);
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    // How much of the frame the agent's bounding box covers, as the realism pass measures it:
    // its skinned meshes only (not the antenna or the basket), the eight corners projected.
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
        Debug.Log($"[Coverage] {what}: {w:P0} x {h:P0} of the frame = {w * h:P0} (box {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2} m, {Vector3.Distance(cam.transform.position, b.center):F2} m from the camera)");
    }

    // The realism pass's UI rules, measured on a play screen: no light-theme panel (a fill
    // brighter than 75 % luminance, over half opaque, bigger than 200 by 120 units), the drawer's
    // and so the world's share of the stage, and the smallest text on screen.
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
        foreach (var t in canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>())
            if (t.enabled && t.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(t.text) && t.fontSize < minText) { minText = t.fontSize; smallest = t.text.Length > 30 ? t.text.Substring(0, 30) : t.text; }
        var drawer = canvas.GetComponentsInChildren<RectTransform>().FirstOrDefault(x => x.name == "Drawer" && x.gameObject.activeInHierarchy);
        float drawerW = drawer != null ? drawer.rect.width : 0f;
        Debug.Log($"[UiCheck] {what}: {light.Count} light panel(s){(light.Count > 0 ? " (" + string.Join(", ", light.Take(6)) + ")" : "")}; drawer {drawerW:F0} of {Ui.W:F0} units = {drawerW / Ui.W:P0}, world {1f - drawerW / Ui.W:P0}; smallest text {minText:F0} on the 1080 canvas (" + smallest + ")");
    }

    // The realism pass's world check, at run time: every renderer in the scene, by shader; the
    // old Flat shader or a MeshKit mesh is listed by name (the effects, the sight square, the
    // ring of the loop and the memory's glass cards are the expected exceptions).
    static void WorldCheck(string what)
    {
        string[] kitMeshes = { "box", "sphere", "cylinder", "cone", "capsule", "dome", "pear", "pyramid", "tube", "thintube" };
        var byShader = new Dictionary<string, int>();
        var flat = new List<string>(); var meshKit = new List<string>();
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
            var f = r.GetComponent<MeshFilter>();
            if (f != null && f.sharedMesh != null && System.Array.IndexOf(kitMeshes, f.sharedMesh.name) >= 0) meshKit.Add(r.name);
        }
        Debug.Log($"[WorldCheck] {what}: {total} renderers; shaders: " + string.Join(", ", byShader.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value}")) +
                  $"; Flat: {flat.Count}{(flat.Count > 0 ? " (" + string.Join(", ", flat.Distinct().Take(8)) + ")" : "")}; MeshKit meshes: {meshKit.Count}{(meshKit.Count > 0 ? " (" + string.Join(", ", meshKit.Distinct().Take(8)) + ")" : "")}");
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
