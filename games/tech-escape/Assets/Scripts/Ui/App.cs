using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The one object in the scene. Builds the canvas, keeps the campaign (which rooms
// are done, the stars each earned, every room's last state and the record) and
// moves between screens.
public class App : MonoBehaviour
{
    public Room room;
    public RoomState state;
    public Session session = new Session();
    public Attack lastAttack;
    public Luma luma;
    public Building world;
    public SceneCamera view;
    public Atmosphere atmosphere;
    public CameraRig rig;
    public Effects effects;
    public GameMenu menu;
    public bool rehearsed;          // the first attack on the room as handed over was a rehearsal: back in the room, fix it, run again
    RoomState handed;               // the room as it was handed over this visit, to tell an untouched room

    const string PrefLevel = "tech_escape_level", PrefStars = "tech_escape_stars_", PrefRoom = "tech_escape_room_", PrefRecord = "tech_escape_record";
    Image page;
    public Action onLeave;

    public void Paper(Color c) { if (page != null) page.color = c; }
    public int LevelsDone { get => PlayerPrefs.GetInt(PrefLevel, 0); set { PlayerPrefs.SetInt(PrefLevel, value); PlayerPrefs.Save(); } }
    public int StarsOf(int level) => PlayerPrefs.GetInt(PrefStars + level, 0);
    public void SetStars(int level, int stars) { if (stars <= StarsOf(level)) return; PlayerPrefs.SetInt(PrefStars + level, stars); PlayerPrefs.Save(); }
    public int TotalStars => Enumerable.Range(1, Rooms.Count).Sum(StarsOf);
    public string Rank => TotalStars >= 48 ? "Incident commander" : TotalStars >= 30 ? "Security lead" : TotalStars >= 12 ? "Analyst" : "Trainee";

    RectTransform canvas, stage, screen;
    public RectTransform Stage => stage;

    void Awake()
    {
        Application.targetFrameRate = Maaloomatiia.Games.Platform.IsWeb ? -1 : 60; // the browser paces its own frames
        Application.runInBackground = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true;
#endif
        Maaloomatiia.Games.Platform.GameId = "tech-escape";
        BuildCanvas();
        view = Camera.main.gameObject.AddComponent<SceneCamera>();
        atmosphere = Camera.main.gameObject.AddComponent<Atmosphere>();
        rig = Camera.main.gameObject.AddComponent<CameraRig>();
        world = new GameObject("World").AddComponent<Building>();
        world.cam = view;
        world.rig = rig;
        effects = new GameObject("Effects").AddComponent<Effects>();
        world.effects = effects;
        world.gameObject.SetActive(false);
        luma = gameObject.AddComponent<Luma>();
        menu = gameObject.AddComponent<GameMenu>();
        Sfx.Init(gameObject);
        Sfx.luma = luma;
        LoadSession();
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
        rig.Release();
        Time.timeScale = 1f;
        atmosphere.MenuBlur(false);
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
    public void ShowFile(params (string head, string body)[] extra) => FileSheet.Toggle(this, screen, extra);
    public void ShowLesson() { new LessonScreen(Fresh(), this); }
    public void ShowRoom() { new RoomScreen(Fresh(), this); }
    public void RunAttack() { new AttackScreen(Fresh(), this); }
    public void ShowResults() { new ResultsScreen(Fresh(), this); }

    // The facility runs the attack by itself; the player fixes the room and watches. A
    // level with a planned mistake opens with the room as it was handed over, the mistake
    // in it (fix it, or run the attack and watch it get through); one with only a ready
    // room opens with that. A room the player already ran comes back as they left it.
    public void StartLevel(int number)
    {
        room = Rooms.Get(number);
        lastAttack = null;
        rehearsed = false;
        var saved = session.Of(room.id);
        state = saved?.Clone()
             ?? (room.broken.Count > 0 ? room.Preset(room.broken, room.brokenOrder) : room.ready.Count > 0 ? room.Preset(room.ready, room.readyOrder) : room.Blank());
        // A rehearsal is only on a room with a planned mistake, as handed over, never run before.
        handed = saved == null && room.broken.Count > 0 ? state.Clone() : null;
        ShowLesson();
    }

    // True while the room stands exactly as it was handed over (nothing touched): the first
    // attack on it is a free rehearsal, not counted.
    public bool Untouched
    {
        get
        {
            if (handed == null || rehearsed || state == null) return false;
            if (state.added.Count != handed.added.Count || !state.added.SetEquals(handed.added) || state.signed != handed.signed) return false;
            if (!state.order.SequenceEqual(handed.order)) return false;
            if (state.picks.Count != handed.picks.Count) return false;
            foreach (var kv in handed.picks) if (!state.picks.TryGetValue(kv.Key, out int v) || v != kv.Value) return false;
            return true;
        }
    }

    public void NextLevel()
    {
        int next = room == null ? 1 : room.number + 1;
        if (next > Rooms.Count) ShowTitle(); else StartLevel(next);
    }

    // The attack ran: remember the room as the student left it, and what got through.
    public void Record(Attack a)
    {
        lastAttack = a;
        session.rooms[room.id] = state.Clone();
        if (room.tier == 2 && !room.Capstone)
        {
            session.record.RemoveAll(l => l.room == room.room);
            foreach (var l in a.lines.Where(l => !l.held)) session.record.Add((l.room, l.gap, l.reached));
            if (!a.AnyThrough) session.record.Add((room.room, "held", "nothing"));
        }
        LevelsDone = Mathf.Max(LevelsDone, room.number);
        SaveSession();
    }

    // A fresh student, for the screenshot walk: no levels done, no stars, no rooms, no record.
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(PrefLevel); PlayerPrefs.DeleteKey(PrefRecord);
        foreach (var rm in Rooms.All) { PlayerPrefs.DeleteKey(PrefRoom + rm.id); PlayerPrefs.DeleteKey(PrefStars + rm.number); }
        PlayerPrefs.Save();
        session = new Session();
    }

    // ---- the campaign's memory, on this machine -----------------------------------------

    [Serializable] class SavedRoom { public string id; public List<string> picks = new List<string>(); public List<string> order = new List<string>(); public List<string> added = new List<string>(); public bool signed; public string signedBy, signedOn, text; }
    [Serializable] class SavedRecord { public List<string> lines = new List<string>(); }

    void SaveSession()
    {
        foreach (var kv in session.rooms)
        {
            var s = new SavedRoom { id = kv.Key, signed = kv.Value.signed, signedBy = kv.Value.signedBy, signedOn = kv.Value.signedOn, text = kv.Value.text };
            foreach (var p in kv.Value.picks) s.picks.Add(p.Key + "=" + p.Value);
            s.order.AddRange(kv.Value.order); s.added.AddRange(kv.Value.added);
            PlayerPrefs.SetString(PrefRoom + kv.Key, JsonUtility.ToJson(s));
        }
        var r = new SavedRecord();
        foreach (var l in session.record) r.lines.Add(l.room + "|" + l.gap + "|" + l.reached);
        PlayerPrefs.SetString(PrefRecord, JsonUtility.ToJson(r));
        PlayerPrefs.Save();
    }

    void LoadSession()
    {
        foreach (var rm in Rooms.All)
        {
            string json = PlayerPrefs.GetString(PrefRoom + rm.id, "");
            if (string.IsNullOrEmpty(json)) continue;
            try
            {
                var s = JsonUtility.FromJson<SavedRoom>(json);
                var st = new RoomState { signed = s.signed, signedBy = s.signedBy, signedOn = s.signedOn, text = s.text ?? "" };
                foreach (var p in s.picks) { var kv = p.Split('='); if (kv.Length == 2 && int.TryParse(kv[1], out int v)) st.picks[kv[0]] = v; }
                st.order.AddRange(s.order); foreach (var a in s.added) st.added.Add(a);
                session.rooms[rm.id] = st;
            }
            catch (Exception) { }
        }
        try
        {
            var r = JsonUtility.FromJson<SavedRecord>(PlayerPrefs.GetString(PrefRecord, ""));
            if (r != null) foreach (var l in r.lines) { var p = l.Split('|'); if (p.Length == 3) session.record.Add((p[0], p[1], p[2])); }
        }
        catch (Exception) { }
    }
}
