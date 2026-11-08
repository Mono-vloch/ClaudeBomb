using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The one object in the scene. Builds the canvas, keeps the agents' settings between rounds,
// keeps the campaign (which errands are done and the stars each earned) and moves between
// the screens. The game menu (Esc, the icon) lives on it.
public class App : MonoBehaviour
{
    public List<AgentConfig> configs = new List<AgentConfig>();
    public Brief brief;
    public int briefIndex;
    public int turns = 30;       // the market's hour: one notch of the clock a turn
    public long budget = 30_000;
    public float speed = 4f;     // the agents run at the fastest speed unless the menu says otherwise
    public Lesson lesson;        // the level being played, or null in free play
    public DecideRun decide;     // errand 1.6, while its tasks run
    public Luma luma;
    public GameMenu menu;
    public Market world;         // the market in three dimensions, behind the canvas
    public MarketCamera view;
    public Atmosphere atmosphere;
    public CameraRig rig;

    const string PrefConfigs = "ai_arena_agents";
    const string PrefLevel = "ai_arena_level";  // the highest level finished on this machine
    const string PrefStory = "story_seen";
    const string PrefStars = "ai_arena_stars_"; // + the level number
    Image page;
    public Action onLeave;       // a screen with coroutines of its own stops them here
    public int lastMedal = -1, lastMedalLevel; // the last debrief's medal and its errand: the garage knows a failed run

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
    public RectTransform Screen => screen;

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
        WebGLInput.captureAllKeyboardInput = true; // typing reaches the note even when the page, not the canvas, has focus
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
        view.Blocked = world.InsideStall; // the street camera keeps out of the stalls
        world.gameObject.SetActive(false);
        LoadConfigs();
        luma = gameObject.AddComponent<Luma>();
        menu = gameObject.AddComponent<GameMenu>();
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
        Market.MemoryCards = false; // a screen that shows the agent's memory turns it on
        FileSheet.Close();
        SettingsSheet.Close();
        menu?.Close();
        luma.Clear();
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

    // The garage, where the list is packed; errand 1.6 decides tasks instead.
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

    // The ladder: one level at a time, its brief, its locked dials, its briefing first. The
    // agents run by themselves; the player packs the agent and watches. A level with a planned
    // mistake opens with that list already written (fix it, or send it and watch it go wrong);
    // one with only a ready list opens with that.
    public void StartLevel(int number)
    {
        lesson = Ladder.Get(number);
        decide = null;
        brief = Brief.Make(briefIndex = lesson.briefPreset, rng);
        var start = lesson.broken ?? lesson.ready;
        foreach (var c in configs)
        {
            if (start != null) { c.Write(start.Fill(brief)); c.model = start.model; c.vision = start.vision; c.memory = start.memory; c.effort = start.effort; }
            else c.Write(""); // no help on this one: the list is the player's from the first word
            lesson.Apply(c);
        }
        ShowLesson();
    }

    // The list the level started with, for LUMA to say why it works or what is wrong with it.
    public Setup StartingSetup => lesson == null ? null : lesson.broken ?? lesson.ready;

    public void NextLevel()
    {
        int next = lesson == null ? 1 : lesson.number + 1;
        if (next > Ladder.Count) StartFree(); else StartLevel(next);
    }

    // Free play: every part open, any message, no checks.
    public void StartFree()
    {
        lesson = null;
        decide = null;
        brief = Brief.Make(briefIndex = rng.Next(Brief.Random), rng);
        ShowSetup();
    }

    public void AddRobot(AgentConfig cfg) => configs.Add(cfg);
    public void RemoveRobot(int index) => configs.RemoveAt(index);

    // ---- the rounds ---------------------------------------------------------------

    public void StartRound(List<AgentConfig> who = null, int? rounds = null)
    {
        var arena = new Arena(brief, (who ?? configs).ConvertAll(c => c.Clone()), budget, rng.Next());
        CurrentArena = arena;
        var parent = Fresh();
        sim = gameObject.AddComponent<Simulation>();
        sim.speed = speed;
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
        if (lesson != null) LevelsDone = Mathf.Max(LevelsDone, lesson.number);
        luma.Clear();
        onLeave?.Invoke();
        onLeave = null;
        menu?.Close();
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

    // Every shopping task runs with both kinds on the same market: a fixed rule (no AI)
    // against an agent with a list.
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
            new AgentConfig { name = configs.Count > 0 ? configs[0].name : "Agent", colour = configs.Count > 0 ? configs[0].colour : 5, kind = AgentConfig.Agent, instructions = t.list, freeText = true,
                              model = Lesson.FixedModel, vision = Lesson.FixedVision, memory = Lesson.FixedMemory, effort = Lesson.FixedEffort },
        };
        StartRound(pair, 10);
    }

    // ---- the agents' settings -------------------------------------------------------

    public void NewBrief()
    {
        briefIndex = (briefIndex + 1 + rng.Next(Brief.Random - 1)) % Brief.Random;
        brief = Brief.Make(briefIndex, rng);
    }

    public void SaveConfigs()
    {
        PlayerPrefs.SetString(PrefConfigs, JsonUtility.ToJson(new AgentConfigList { agents = configs }));
        PlayerPrefs.Save();
    }

    void LoadConfigs()
    {
        try
        {
            var saved = JsonUtility.FromJson<AgentConfigList>(PlayerPrefs.GetString(PrefConfigs, ""));
            if (saved != null && saved.agents != null && saved.agents.Count >= 1) configs = saved.agents;
        }
        catch (Exception) { }
        // One agent: the player's. A class adds one per student in the menu.
        if (configs.Count < 1) configs = new List<AgentConfig> { new AgentConfig { name = "Agent", colour = 0 } };
        // Lists saved before the garage built them from parts stay as written.
        foreach (var c in configs)
            if (!c.freeText && c.buy.Count == 0 && c.never.Count == 0 && string.IsNullOrWhiteSpace(c.words) && !string.IsNullOrWhiteSpace(c.instructions)) c.freeText = true;
    }
}
