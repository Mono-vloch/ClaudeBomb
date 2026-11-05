using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The one object in the scene. Builds the canvas, keeps the robots' settings
// between rounds, keeps the campaign (which errands are done and the stars each
// earned), runs the training steps of an errand and moves between the screens.
public class App : MonoBehaviour
{
    // The three runs of an errand, in the curriculum's order: a ready setup that
    // works (See it work), one with the planned mistake in it (Fix it), then the
    // students' own lists (Your turn).
    public enum Step { SeeItWork, FixIt, YourTurn }

    public List<AgentConfig> configs = new List<AgentConfig>();
    public Brief brief;
    public int briefIndex;
    public int turns = 20;
    public long budget = 30_000;
    public Lesson lesson;        // the level being played, or null in free play
    public Step step = Step.YourTurn;
    public DecideRun decide;     // errand 1.6, while its tasks run
    public Luma luma;
    public Market world;         // the market in three dimensions, behind the canvas
    public MarketCamera view;
    public Atmosphere atmosphere;
    public CameraRig rig;

    const string PrefConfigs = "ai_arena_agents";
    const string PrefLevel = "ai_arena_level";  // the highest level finished on this machine
    const string PrefStory = "story_seen";
    const string PrefStars = "ai_arena_stars_"; // + the level number
    Image page;
    List<AgentConfig> own;       // the students' own lists, kept while a training setup is loaded
    public Action onLeave;       // a screen with coroutines of its own stops them here

    public bool StorySeen
    {
        get => PlayerPrefs.GetInt(PrefStory, 0) == 1;
        set { PlayerPrefs.SetInt(PrefStory, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // The colour behind the stage, out to the window's edges.
    public void Paper(Color c) { if (page != null) page.color = c; }

    public int LevelsDone
    {
        get => PlayerPrefs.GetInt(PrefLevel, 0);
        set { PlayerPrefs.SetInt(PrefLevel, value); PlayerPrefs.Save(); }
    }

    // Stars are for motivation only; the checks decide the result. Up to three per errand.
    public int StarsOf(int level) => PlayerPrefs.GetInt(PrefStars + level, 0);
    public void SetStars(int level, int stars)
    {
        if (stars <= StarsOf(level)) return;
        PlayerPrefs.SetInt(PrefStars + level, stars);
        PlayerPrefs.Save();
    }
    public int TotalStars => Enumerable.Range(1, Ladder.Count).Sum(StarsOf);
    public string Rank => TotalStars >= 18 ? "Head of agents" : TotalStars >= 10 ? "Agent manager" : TotalStars >= 4 ? "Junior manager" : "Trainee";

    readonly System.Random rng = new System.Random();
    RectTransform canvas;
    RectTransform stage;
    RectTransform screen;
    Simulation sim;
    public Simulation Sim => sim;
    public Arena CurrentArena { get; private set; }
    public RectTransform Stage => stage;

    // A point on the screen, in pixels, as stage coordinates from the top left.
    public Vector2 StagePoint(Vector3 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(stage, screenPoint, null, out var local);
        return new Vector2(local.x + Ui.W / 2, Ui.H / 2 - local.y);
    }

    void Awake()
    {
        Application.targetFrameRate = Maaloomatiia.Games.Platform.IsWeb ? -1 : 60; // the browser paces its own frames
        Application.runInBackground = true; // a round keeps going while the teacher switches windows
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true; // typing reaches the lists even when the page, not the canvas, has focus
#endif
        PlayerPrefs.DeleteKey("anthropic_key"); // earlier versions kept a typed API key here; Claude Code needs none
        Maaloomatiia.Games.Platform.GameId = "ai-arena"; // the platform's name for this game
        BuildCanvas();
        Market.Setup();
        view = Camera.main.gameObject.AddComponent<MarketCamera>();
        atmosphere = Camera.main.gameObject.AddComponent<Atmosphere>();
        rig = Camera.main.gameObject.AddComponent<CameraRig>();
        world = new GameObject("World").AddComponent<Market>();
        world.cam = view;
        world.rig = rig;
        world.gameObject.SetActive(false);
        LoadConfigs();
        luma = gameObject.AddComponent<Luma>();
        Sfx.Init(gameObject);
        Sfx.luma = luma;
        brief = Brief.Make(briefIndex = rng.Next(Brief.Random), rng);
        if (DemoDriver.Wanted) gameObject.AddComponent<DemoDriver>();
    }

    void Start() => ShowTitle();

    void BuildCanvas()
    {
        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(Ui.W, Ui.H);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // never crop: 16:10 and 4:3 projectors get margins instead
        canvas = go.GetComponent<RectTransform>();

        page = Ui.Box(canvas, "Page", Theme.Page, false);
        Ui.Stretch(page.rectTransform);
        page.raycastTarget = false; // clicks that land on nothing may reach the market behind

        // Every screen is designed on this fixed stage, centred in the window.
        stage = Ui.Node("Stage", canvas);
        stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(0.5f, 0.5f);
        stage.sizeDelta = new Vector2(Ui.W, Ui.H);

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
    }

    RectTransform Fresh()
    {
        FileSheet.Close();
        luma.Hush();
        onLeave?.Invoke();
        onLeave = null;
        world.Hide(); // a screen that wants the market shows it again
        atmosphere.MenuBlur(false);
        if (screen != null) Destroy(screen.gameObject);
        if (sim != null) { Destroy(sim); sim = null; }
        screen = Ui.Node("Screen", stage);
        Ui.Stretch(screen);
        var group = Ui.Group(screen);
        group.alpha = 0f;
        StartCoroutine(Ui.Fade(group, 1f, 0.22f));
        return screen;
    }

    public void ShowTitle() { new TitleScreen(Fresh(), this); }
    public void ShowFile(params (string head, string body)[] extra) => FileSheet.Toggle(this, screen, extra);
    public void ShowLesson() { new LessonScreen(Fresh(), this); }
    public StoryScreen story;
    public void ShowStory(Action then) { story = new StoryScreen(Fresh(), this, () => { StorySeen = true; story = null; if (then != null) then(); else ShowTitle(); }); }

    // The garage, where the lists are written; errand 1.6 decides tasks instead.
    public void ShowSetup()
    {
        if (lesson != null && lesson.decide) { new DecideScreen(Fresh(), this); return; }
        new SetupScreen(Fresh(), this);
    }

    // Start: the story the first time, then the errand.
    public void Begin(int level)
    {
        if (!StorySeen) ShowStory(() => StartLevel(level));
        else StartLevel(level);
    }

    // The ladder: one level at a time, its brief, its locked dials, its briefing
    // first. A level with a ready setup opens on the training run.
    public void StartLevel(int number)
    {
        lesson = Ladder.Get(number);
        decide = null;
        RestoreOwn();
        brief = Brief.Make(briefIndex = lesson.briefPreset, rng);
        foreach (var c in configs) lesson.Apply(c);
        step = lesson.ready != null ? Step.SeeItWork : Step.YourTurn;
        if (step != Step.YourTurn) LoadStep();
        ShowLesson();
    }

    public void NextLevel()
    {
        int next = lesson == null ? 1 : lesson.number + 1;
        if (next > Ladder.Count) StartFree(); else StartLevel(next);
    }

    // Free play: every dial open, any brief, no checks.
    public void StartFree()
    {
        lesson = null;
        decide = null;
        RestoreOwn();
        step = Step.YourTurn;
        brief = Brief.Make(briefIndex = rng.Next(Brief.Random), rng);
        ShowSetup();
    }

    // ---- the training runs ----------------------------------------------------

    public void SetStep(Step s)
    {
        step = s;
        if (s == Step.YourTurn) RestoreOwn(); else LoadStep();
    }

    public Setup CurrentSetup => step == Step.SeeItWork ? lesson?.ready : step == Step.FixIt ? lesson?.broken : null;
    public string StepName => step == Step.SeeItWork ? "Watch the pro" : step == Step.FixIt ? "Fix the broken bot" : "Your run";
    public bool Training => step != Step.YourTurn && CurrentSetup != null;

    // The ready setup, or the broken one, goes into every robot. The students'
    // own lists wait, untouched, until Your run.
    void LoadStep()
    {
        var setup = CurrentSetup;
        if (setup == null) { step = Step.YourTurn; RestoreOwn(); return; }
        if (own == null) own = configs.ConvertAll(c => c.Clone());
        foreach (var c in configs)
        {
            c.Write(setup.Fill(brief));
            c.model = setup.model; c.vision = setup.vision; c.memory = setup.memory; c.effort = setup.effort;
            lesson.Apply(c);
        }
    }

    void RestoreOwn()
    {
        if (own == null) return;
        for (int i = 0; i < configs.Count && i < own.Count; i++)
        {
            var c = configs[i]; var o = own[i];
            c.instructions = o.instructions; c.buy = new List<string>(o.buy); c.never = new List<string>(o.never);
            c.fallback = o.fallback; c.words = o.words; c.freeText = o.freeText;
            c.model = o.model; c.vision = o.vision; c.memory = o.memory; c.effort = o.effort;
        }
        own = null;
    }

    public void AddRobot(AgentConfig cfg)
    {
        configs.Add(cfg);
        own?.Add(cfg.Clone());
    }

    public void RemoveRobot(int index)
    {
        configs.RemoveAt(index);
        if (own != null && index < own.Count) own.RemoveAt(index);
    }

    // ---- the rounds ---------------------------------------------------------------

    public void StartRound(List<AgentConfig> who = null, int? rounds = null)
    {
        var arena = new Arena(brief, (who ?? configs).ConvertAll(c => c.Clone()), budget, rng.Next());
        CurrentArena = arena;
        var parent = Fresh();
        sim = gameObject.AddComponent<Simulation>();
        new ArenaScreen(parent, this, arena, sim);
        sim.Begin(arena, rounds ?? turns);
    }

    public void ShowResults(Arena arena, Simulation finished)
    {
        if (decide != null)
        {
            decide.results.Add((decide.Current, arena.agents[0], arena.agents[1]));
            DecideNext();
            return;
        }
        if (lesson != null && step == Step.YourTurn) LevelsDone = Mathf.Max(LevelsDone, lesson.number);
        luma.Hush();
        onLeave?.Invoke();
        onLeave = null;
        world.Hide();
        atmosphere.MenuBlur(false);
        if (screen != null) Destroy(screen.gameObject);
        screen = Ui.Node("Screen", stage);
        Ui.Stretch(screen);
        var group = Ui.Group(screen);
        group.alpha = 0f;
        StartCoroutine(Ui.Fade(group, 1f, 0.22f));
        new ResultsScreen(screen, this, arena, finished);
        if (sim != null) { Destroy(sim); sim = null; }
    }

    // ---- errand 1.6: does this task need an agent? --------------------------------

    public class DecideRun
    {
        public int[] choices;   // per task: Decide.Agent, Automation or Neither
        public int task = -1;   // the task running
        public readonly List<(DecideTask task, AgentState rule, AgentState agent)> results = new List<(DecideTask, AgentState, AgentState)>();
        public DecideTask Current => Decide.Tasks[task];
        public int RunNumber => Decide.Tasks.Take(task + 1).Count(t => t.Runs);
    }

    // Every shopping task runs with both kinds on the same market: a fixed rule
    // (no AI) against an agent with a list.
    public void StartDecide(int[] choices)
    {
        decide = new DecideRun { choices = choices };
        DecideNext();
    }

    void DecideNext()
    {
        do decide.task++; while (decide.task < Decide.Tasks.Length && !Decide.Tasks[decide.task].Runs);
        if (decide.task >= Decide.Tasks.Length)
        {
            LevelsDone = Mathf.Max(LevelsDone, lesson.number);
            var run = decide;
            decide = null;
            new DecideDebrief(Fresh(), this, run);
            return;
        }
        var t = decide.Current;
        brief = t.Brief();
        var pair = new List<AgentConfig>
        {
            new AgentConfig { name = "Rule", colour = 9, kind = AgentConfig.Automation, instructions = t.RuleText, freeText = true,
                              model = 0, vision = Lesson.FixedVision, memory = MemoryModes.None, effort = 0 },
            new AgentConfig { name = "Agent", colour = 5, kind = AgentConfig.Agent, instructions = t.list, freeText = true,
                              model = Lesson.FixedModel, vision = Lesson.FixedVision, memory = Lesson.FixedMemory, effort = Lesson.FixedEffort },
        };
        StartRound(pair, 10);
    }

    // ---- the robots' settings -------------------------------------------------------

    public void NewBrief()
    {
        briefIndex = (briefIndex + 1 + rng.Next(Brief.Random - 1)) % Brief.Random;
        brief = Brief.Make(briefIndex, rng);
    }

    public void SaveConfigs()
    {
        PlayerPrefs.SetString(PrefConfigs, JsonUtility.ToJson(new AgentConfigList { agents = own ?? configs }));
        PlayerPrefs.Save();
    }

    void LoadConfigs()
    {
        try
        {
            var saved = JsonUtility.FromJson<AgentConfigList>(PlayerPrefs.GetString(PrefConfigs, ""));
            if (saved != null && saved.agents != null && saved.agents.Count >= 2) configs = saved.agents;
        }
        catch (Exception) { }
        if (configs.Count < 2)
            configs = new List<AgentConfig>
            {
                new AgentConfig { name = "Agent 1", colour = 0 },
                new AgentConfig { name = "Agent 2", colour = 1 },
            };
        // Lists saved before the garage built them from parts stay as written.
        foreach (var c in configs)
            if (!c.freeText && c.buy.Count == 0 && c.never.Count == 0 && string.IsNullOrWhiteSpace(c.words) && !string.IsNullOrWhiteSpace(c.instructions)) c.freeText = true;
    }
}
