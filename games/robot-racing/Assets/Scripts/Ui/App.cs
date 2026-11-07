using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The one object in the scene. Builds the canvas, keeps the campaign (which
// levels are done, the stars each earned), the level's session (its runs, races
// and answers) and moves between screens. The game menu (Esc, the icon) lives on it.
public class App : MonoBehaviour
{
    public Lesson lesson;
    public Session session;
    public Settings settings = new Settings();   // free play's, or the level's own
    public Luma luma;
    public GameMenu menu;
    public Circuit world;
    public SceneCamera view;
    public Atmosphere atmosphere;
    public CameraRig rig;

    const string PrefLevel = "robot_racing_level";
    const string PrefStars = "robot_racing_stars_";
    const string PrefPace = "robot_racing_pace";
    Image page;
    public Action onLeave;

    public void Paper(Color c) { if (page != null) page.color = c; }
    public int LevelsDone { get => PlayerPrefs.GetInt(PrefLevel, 0); set { PlayerPrefs.SetInt(PrefLevel, value); PlayerPrefs.Save(); } }
    public int StarsOf(int level) => PlayerPrefs.GetInt(PrefStars + level, 0);
    public void SetStars(int level, int stars) { if (stars <= StarsOf(level)) return; PlayerPrefs.SetInt(PrefStars + level, stars); PlayerPrefs.Save(); }
    public int TotalStars => Enumerable.Range(1, Ladder.Count).Sum(StarsOf);
    public string Rank => TotalStars >= 32 ? "Chief engineer" : TotalStars >= 20 ? "Race engineer" : TotalStars >= 8 ? "Mechanic" : "Trainee";
    // The training pace: 0 every attempt at race speed, 1 the montage (the default), 2 straight to the end.
    public int pace { get => PlayerPrefs.GetInt(PrefPace, 1); set { PlayerPrefs.SetInt(PrefPace, Mathf.Clamp(value, 0, 2)); PlayerPrefs.Save(); } }

    RectTransform canvas, stage, screen;
    public RectTransform Stage => stage;

    public Vector2 StagePoint(Vector3 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(stage, screenPoint, null, out var local);
        return new Vector2(local.x + Ui.W / 2, Ui.H / 2 - local.y);
    }

    void Awake()
    {
        Application.targetFrameRate = Maaloomatiia.Games.Platform.IsWeb ? -1 : 60; // the browser paces its own frames
        Application.runInBackground = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true;
#endif
        Maaloomatiia.Games.Platform.GameId = "robot-racing";
        BuildCanvas();
        Circuit.Setup();
        view = Camera.main.gameObject.AddComponent<SceneCamera>();
        atmosphere = Camera.main.gameObject.AddComponent<Atmosphere>();
        rig = Camera.main.gameObject.AddComponent<CameraRig>();
        world = new GameObject("World").AddComponent<Circuit>();
        world.cam = view;
        world.rig = rig;
        world.gameObject.SetActive(false);
        luma = gameObject.AddComponent<Luma>();
        menu = gameObject.AddComponent<GameMenu>();
        Sfx.Init(gameObject);
        Sfx.luma = luma;
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
        FileSheet.Close();
        menu?.Close();
        luma.Hush();
        onLeave?.Invoke();
        onLeave = null;
        world.Hide();
        atmosphere.MenuBlur(false); // a screen that wants the world soft asks for it
        if (screen != null) Destroy(screen.gameObject);
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
    public void ShowPit() { new PitScreen(Fresh(), this); }
    public void ShowQuiz() { new QuizScreen(Fresh(), this); }
    public void ShowResults() { new ResultsScreen(Fresh(), this); }

    // A level: its session and its settings. The robot runs by itself; the player sets
    // it up and watches. A level with a planned mistake opens with that setup already in
    // the pit lane (fix it, or train it and watch it go wrong); one with only a ready
    // setup opens with that; the capstone opens on the fixed defaults.
    public void StartLevel(int number)
    {
        lesson = Ladder.Get(number);
        session = new Session { lesson = lesson };
        settings = StartingSetup?.a.Clone() ?? lesson.Start();
        session.settings = settings;
        ShowLesson();
    }

    // The setup the level started with, for LUMA to say why it works or what is wrong with it.
    public Setup StartingSetup => lesson == null ? null : lesson.broken ?? lesson.ready;

    public void NextLevel()
    {
        int next = lesson == null ? 1 : lesson.number + 1;
        if (next > Ladder.Count) StartFree(); else StartLevel(next);
    }

    public void StartFree()
    {
        lesson = null;
        session = new Session();
        settings = new Settings();
        session.settings = settings;
        ShowPit();
    }

    // Training starts: the run screen trains, races, and comes back to the debrief.
    // The player's settings run; a level whose mistake needs two robots runs the second
    // one after it, unchanged, so the two curves can be compared.
    public void StartTraining()
    {
        var setup = StartingSetup;
        var runs = new List<(Settings s, string label)>();
        bool pair = setup != null && setup.b != null;
        runs.Add((settings.Clone(), pair ? setup.aName : ""));
        if (pair) runs.Add((setup.b.Clone(), setup.bName));
        new RunScreen(Fresh(), this, runs);
    }

    // The race is over: the level's questions on the world where they fit, then the debrief.
    public void FinishLevel()
    {
        if (lesson != null) LevelsDone = Mathf.Max(LevelsDone, lesson.number);
        if (lesson != null && QuizScreen.Fits(lesson) && !ResultsScreen.Answered(session)) ShowQuiz();
        else ShowResults();
    }
}
