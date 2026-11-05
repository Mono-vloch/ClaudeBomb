using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The one object in the scene. Builds the canvas, keeps the campaign (which
// levels are done, the stars each earned), the level's session (its runs, races
// and answers), runs the training steps of a level and moves between screens.
public class App : MonoBehaviour
{
    public enum Step { SeeItWork, FixIt, YourTurn }

    public Lesson lesson;
    public Session session;
    public Settings settings = new Settings();   // free play's, or the level's own
    public Step step = Step.YourTurn;
    public Luma luma;
    public Circuit world;
    public SceneCamera view;

    const string PrefLevel = "robot_racing_level";
    const string PrefStars = "robot_racing_stars_";
    Image page;
    Settings own; // the student's own settings, kept while a ready setup is loaded
    public Action onLeave;

    public void Paper(Color c) { if (page != null) page.color = c; }
    public int LevelsDone { get => PlayerPrefs.GetInt(PrefLevel, 0); set { PlayerPrefs.SetInt(PrefLevel, value); PlayerPrefs.Save(); } }
    public int StarsOf(int level) => PlayerPrefs.GetInt(PrefStars + level, 0);
    public void SetStars(int level, int stars) { if (stars <= StarsOf(level)) return; PlayerPrefs.SetInt(PrefStars + level, stars); PlayerPrefs.Save(); }
    public int TotalStars => Enumerable.Range(1, Ladder.Count).Sum(StarsOf);
    public string Rank => TotalStars >= 32 ? "Chief engineer" : TotalStars >= 20 ? "Race engineer" : TotalStars >= 8 ? "Mechanic" : "Trainee";

    RectTransform canvas, stage, screen;
    public RectTransform Stage => stage;

    public Vector2 StagePoint(Vector3 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(stage, screenPoint, null, out var local);
        return new Vector2(local.x + Ui.W / 2, Ui.H / 2 - local.y);
    }

    void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true;
#endif
        Maaloomatiia.Games.Platform.GameId = "robot-racing";
        BuildCanvas();
        Circuit.Setup();
        view = Camera.main.gameObject.AddComponent<SceneCamera>();
        world = new GameObject("World").AddComponent<Circuit>();
        world.cam = view;
        world.gameObject.SetActive(false);
        luma = gameObject.AddComponent<Luma>();
        Sfx.Init(gameObject);
        if (DemoDriver.Wanted) gameObject.AddComponent<DemoDriver>();
    }

    void Start() => ShowTitle();

    void BuildCanvas()
    {
        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(Ui.W, Ui.H);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvas = go.GetComponent<RectTransform>();
        page = Ui.Box(canvas, "Page", Theme.Page, false);
        Ui.Stretch(page.rectTransform);
        page.raycastTarget = false;
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
        luma.Hush();
        onLeave?.Invoke();
        onLeave = null;
        world.Hide();
        if (screen != null) Destroy(screen.gameObject);
        screen = Ui.Node("Screen", stage);
        Ui.Stretch(screen);
        var group = Ui.Group(screen);
        group.alpha = 0f;
        StartCoroutine(Ui.Fade(group, 1f, 0.22f));
        return screen;
    }

    public void ShowTitle() { new TitleScreen(Fresh(), this); }
    public void ShowLesson() { new LessonScreen(Fresh(), this); }
    public void ShowPit() { new PitScreen(Fresh(), this); }
    public void ShowResults() { new ResultsScreen(Fresh(), this); }

    // A level: its session, its settings, its first training run.
    public void StartLevel(int number)
    {
        lesson = Ladder.Get(number);
        session = new Session { lesson = lesson };
        own = null;
        settings = lesson.Start();
        session.settings = settings;
        step = lesson.ready != null ? Step.SeeItWork : lesson.broken != null ? Step.FixIt : Step.YourTurn;
        if (step != Step.YourTurn) LoadStep();
        ShowLesson();
    }

    public void NextLevel()
    {
        int next = lesson == null ? 1 : lesson.number + 1;
        if (next > Ladder.Count) StartFree(); else StartLevel(next);
    }

    public void StartFree()
    {
        lesson = null;
        session = new Session();
        own = null;
        settings = new Settings();
        session.settings = settings;
        step = Step.YourTurn;
        ShowPit();
    }

    public Setup CurrentSetup => step == Step.SeeItWork ? lesson?.ready : step == Step.FixIt ? lesson?.broken : null;
    public string StepName => step == Step.SeeItWork ? "Watch the pro" : step == Step.FixIt ? "Fix the broken bot" : "Your run";
    public bool Training => step != Step.YourTurn && CurrentSetup != null;

    public void SetStep(Step s)
    {
        step = s;
        if (s == Step.YourTurn) { if (own != null) { settings = own; own = null; } }
        else LoadStep();
        session.settings = settings;
    }

    void LoadStep()
    {
        var setup = CurrentSetup;
        if (setup == null) { step = Step.YourTurn; return; }
        if (own == null) own = settings;
        settings = setup.a.Clone();
        session.settings = settings;
    }

    // Training starts: the run screen trains, races, and comes back to the debrief.
    public void StartTraining()
    {
        var setup = CurrentSetup;
        var runs = new List<(Settings s, string label)>();
        if (setup != null)
        {
            runs.Add((setup.a.Clone(), setup.aName));
            if (setup.b != null) runs.Add((setup.b.Clone(), setup.bName));
        }
        else runs.Add((settings.Clone(), ""));
        new RunScreen(Fresh(), this, runs);
    }

    public void FinishLevel()
    {
        if (lesson != null && step == Step.YourTurn) LevelsDone = Mathf.Max(LevelsDone, lesson.number);
        ShowResults();
    }
}
