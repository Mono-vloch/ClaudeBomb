using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        Luma.Voice = false; // the walk is silent
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
        yield return new WaitForSeconds(0.5f);
        yield return Shot("3-lesson");
        Tour.Seen = false;
        app.ShowSetup();
        yield return new WaitForSeconds(1.5f);
        yield return Shot("4-tour");
        app.StartLevel(2);
        yield return new WaitForSeconds(0.5f);
        yield return Shot("3b-warm-up");
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
        yield return new WaitForSeconds(0.5f);
        yield return Shot("5-setup");

        app.StartRound();
        float waited = 0;
        int shotAt = Live ? 2 : 5;
        float shotTimeout = Live ? 180f : 7f;
        while (app.Sim != null && !app.Sim.finished && app.Sim.turn < shotAt && waited < shotTimeout) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(1.5f);
        yield return Shot("6-market");
        app.world.SetView(Market.View.RobotsEye);
        yield return new WaitForSeconds(1.6f);
        yield return Shot("6b-agents-eye");
        app.world.SetView(Market.View.Market);
        waited = 0;
        while (app.Sim != null && !app.Sim.finished && waited < (Live ? 300f : 30f)) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
        app.ShowResults(app.CurrentArena, app.Sim);
        yield return new WaitForSeconds(2.6f); // the medal has been revealed by then
        yield return Shot("7-debrief");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    // The cover: four robots out shopping on a market day, placed by hand so the
    // picture is the same every time, the market filling the whole window and
    // nothing drawn over it.
    IEnumerator Cover(App app)
    {
        ClaudeAccess.ForceOffline = true;
        Luma.Voice = false;
        AudioListener.volume = 0f;
        yield return null; // the title has built by now; the market takes its place
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

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
