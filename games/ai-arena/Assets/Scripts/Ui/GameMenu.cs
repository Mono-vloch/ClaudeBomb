using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The game menu, as Claude Bomb's: Esc or the icon in the top-right corner. The game pauses
// while it is open. From here the player resumes, reads the briefing again, opens the file,
// replays the story, starts the errand again, goes to free play, back to the title or to the
// games menu, and sets the sound, LUMA's voice, the speed, the turns and the class. Every
// button that used to sit on a screen and was not that screen's own action lives here.
public class GameMenu : MonoBehaviour
{
    public static GameMenu Instance { get; private set; }
    public static bool Paused => Instance != null && Instance.IsOpen;
    public bool IsOpen => root != null && root.gameObject.activeSelf;

    App app;
    RectTransform root;
    const float W = 600f, Inner = 520f, Left = 40f;
    static readonly int[] TurnOptions = { 20, 30, 40 };
    static readonly float[] SpeedOptions = { 1f, 2f, 4f };

    void Awake() { Instance = this; app = GetComponent<App>(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { if (FileSheet.IsOpen) FileSheet.Close(); else if (SettingsSheet.IsOpen) SettingsSheet.Close(); else Toggle(); }
    }

    // The icon: the one control in a corner that opens this. A screen puts it where it wants.
    public static Button Icon(Transform parent, float x = Ui.W - 40 - 52, float y = 24)
    {
        var b = Ui.Button(parent, "≡", () => Instance?.Toggle(), Ui.ButtonStyle.GhostOnDark, 26);
        Ui.Place(b.GetComponent<RectTransform>(), x, y, 52, 52);
        b.name = "Menu";
        return b;
    }

    public void Toggle() { if (IsOpen) Close(); else Open(); }

    public void Open()
    {
        if (IsOpen || app.Stage == null) return;
        if (root != null) Destroy(root.gameObject);
        root = Ui.Node("Game menu", app.Stage.parent); // over every screen
        Ui.Stretch(root);
        var dim = Ui.Box(root, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.86f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(Close);
        var card = Ui.Box(root, "Card", Ui.DarkFill);
        var t = card.transform;
        var l = app.lesson;
        float y = 28;
        var kicker = Ui.Eyebrow(t, "PAUSED", Theme.Teal); kicker.font = Theme.Bold;
        Ui.Place(kicker.rectTransform, Left, y, Inner, 20); y += 26;
        var title = Ui.Text(t, l != null ? $"Errand {l.id}" : "Free play", 32, Color.white, true);
        Ui.Place(title.rectTransform, Left, y, Inner, 44); y += 58;

        Button Row(string label, Action act, Ui.ButtonStyle style, float h = 48)
        {
            var b = Ui.Button(t, label, () => { Close(); act(); }, style, 18);
            Ui.Place(b.GetComponent<RectTransform>(), Left, y, Inner, h);
            y += h + 8;
            return b;
        }
        Row("Resume", () => { }, Ui.ButtonStyle.Accent, 56);
        if (l != null)
        {
            Row("The briefing", app.ShowLesson, Ui.ButtonStyle.GhostOnDark);
            Row("The file", () => app.ShowFile(), Ui.ButtonStyle.GhostOnDark);
            Row("This errand again", () => app.StartLevel(l.number), Ui.ButtonStyle.GhostOnDark);
        }
        else Row("The file", () => app.ShowFile(), Ui.ButtonStyle.GhostOnDark);
        // Two to a row: the story and free play.
        {
            var story = Ui.Button(t, "The story", () => { Close(); app.ShowStory(null); }, Ui.ButtonStyle.GhostOnDark, 16);
            Ui.Place(story.GetComponent<RectTransform>(), Left, y, Inner / 2 - 5, 44);
            var free = Ui.Button(t, "Free play", () => { Close(); app.StartFree(); }, Ui.ButtonStyle.GhostOnDark, 16);
            Ui.Place(free.GetComponent<RectTransform>(), Left + Inner / 2 + 5, y, Inner / 2 - 5, 44);
            y += 52;
        }
        y += 8;
        var line = Ui.Box(t, "Line", Ui.Hairline, false); Ui.Place(line.rectTransform, Left, y, Inner, 1); y += 16;
        var st = Ui.Eyebrow(t, "SETTINGS", Theme.Teal); Ui.Place(st.rectTransform, Left, y, Inner, 20); y += 28;

        void Setting(string label, RectTransform control, float w = 140)
        {
            var name = Ui.Text(t, label, 18, Color.white, true);
            Ui.Place(name.rectTransform, Left, y + 8, 240, 26);
            Ui.Place(control, Left + Inner - w, y, w, 40);
            y += 48;
        }
        Button sound = null;
        sound = Ui.Button(t, Sfx.Enabled ? "On" : "Off", () => { Sfx.Enabled = !Sfx.Enabled; Ui.SetLabel(sound, Sfx.Enabled ? "On" : "Off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Setting("Sound", sound.GetComponent<RectTransform>());
        Button voice = null;
        voice = Ui.Button(t, Luma.Voice ? "On" : "Off", () => { Luma.Voice = !Luma.Voice; if (!Luma.Voice) app.luma.Hush(); Ui.SetLabel(voice, Luma.Voice ? "On" : "Off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Setting("LUMA's voice", voice.GetComponent<RectTransform>());
        int si = Array.IndexOf(SpeedOptions, app.speed); if (si < 0) si = 2;
        var speed = new Ui.Segmented(t, new[] { "1x", "2x", "4x" }, si, 240, 40, 15);
        speed.OnSelect = i => { app.speed = SpeedOptions[i]; if (app.Sim != null) app.Sim.speed = app.speed; };
        Setting("Speed", speed.root, 240);
        var turns = new Ui.Segmented(t, new[] { "20", "30", "40" }, Mathf.Max(0, Array.IndexOf(TurnOptions, app.turns)), 240, 40, 15);
        turns.OnSelect = i => app.turns = TurnOptions[i];
        Setting("Turns an errand lasts", turns.root, 240);
        var roster = Ui.Button(t, app.configs.Count > 1 ? $"{app.configs.Count} agents" : "One agent", () => { Close(); SettingsSheet.Toggle(app, app.Stage.parent, null); }, Ui.ButtonStyle.GhostOnDark, 16);
        Setting("The class", roster.GetComponent<RectTransform>(), 160);
        y += 8;

        var home = Ui.Button(t, "Title screen", () => { Close(); app.ShowTitle(); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(home.GetComponent<RectTransform>(), Left, y, 200, 46);
        if (Platform.IsWeb)
        {
            var leave = Ui.Button(t, "Games menu", () => { Close(); Platform.GoToMenu(); }, Ui.ButtonStyle.GhostOnDark, 16);
            Ui.Place(leave.GetComponent<RectTransform>(), Left + Inner - 200, y, 200, 46);
        }
        y += 46 + 32;
        Ui.Place(card.rectTransform, (Ui.W - W) / 2, Mathf.Max(10f, (Ui.H - y) / 2), W, y);
        var group = Ui.Group(card);
        StartCoroutine(Ui.Animate(0.22f, k => { group.alpha = k; }));
        if (app.Sim != null) app.Sim.paused = true;
        Sfx.Tick();
    }

    public void Close()
    {
        if (root != null) Destroy(root.gameObject);
        root = null;
        if (app.Sim != null && !app.Sim.finished && ArenaScreen.Current != null && ArenaScreen.Current.Started) app.Sim.paused = false;
    }
}
