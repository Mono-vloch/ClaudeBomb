using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The settings, behind one button: the things a player never needs on the screen.
// The speed the agents run at (the fastest unless said otherwise), the turns an
// errand lasts, the voice and the sound, and the roster: one agent is the player's;
// a class adds one per student here. Click anywhere off the sheet to close it.
public static class SettingsSheet
{
    static RectTransform open;
    static readonly int[] TurnOptions = { 20, 30, 40 };
    static readonly float[] SpeedOptions = { 1f, 2f, 4f };

    public static void Toggle(App app, Transform screen, System.Action changed)
    {
        if (open != null) { Close(); return; }
        if (screen == null) return;
        open = Ui.Node("Settings", screen);
        Ui.Stretch(open);
        var dim = Ui.Box(open, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.55f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(() => { Close(); changed?.Invoke(); });
        var card = Ui.Float(open, "Sheet", 560, 150, 800, 780);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, "SETTINGS", Theme.TealDeep);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 40, 28, 400, 20);
        var close = Ui.Button(t, "Done", () => { Close(); changed?.Invoke(); }, Ui.ButtonStyle.Primary, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 800 - 40 - 100, 24, 100, 40);

        // The speed
        var sl = Ui.Text(t, "Speed", 18, Theme.Ink, true);
        Ui.Place(sl.rectTransform, 40, 80, 300, 26);
        var speed = new Ui.Segmented(t, new[] { "1x", "2x", "4x" }, System.Array.IndexOf(SpeedOptions, app.speed) < 0 ? 2 : System.Array.IndexOf(SpeedOptions, app.speed), 300, 40, 14);
        Ui.Place(speed.root, 420, 74, 300, 40);
        speed.OnSelect = i => { app.speed = SpeedOptions[i]; if (app.Sim != null) app.Sim.speed = app.speed; };

        // The turns
        var tl = Ui.Text(t, "Turns an errand lasts", 18, Theme.Ink, true);
        Ui.Place(tl.rectTransform, 40, 136, 360, 26);
        var turns = new Ui.Segmented(t, TurnOptions.Select(x => x.ToString()).ToArray(), Mathf.Max(0, System.Array.IndexOf(TurnOptions, app.turns)), 300, 40, 14);
        Ui.Place(turns.root, 420, 130, 300, 40);
        turns.OnSelect = i => app.turns = TurnOptions[i];

        // The voice and the sound
        var vl = Ui.Text(t, "LUMA's voice", 18, Theme.Ink, true);
        Ui.Place(vl.rectTransform, 40, 192, 300, 26);
        Button voice = null;
        voice = Ui.Button(t, Luma.Voice ? "On" : "Off", () => { Luma.Voice = !Luma.Voice; if (!Luma.Voice) app.luma.Hush(); Ui.SetLabel(voice, Luma.Voice ? "On" : "Off"); }, Ui.ButtonStyle.Secondary, 14);
        Ui.Place(voice.GetComponent<RectTransform>(), 420, 186, 140, 40);
        var sdl = Ui.Text(t, "Sound", 18, Theme.Ink, true);
        Ui.Place(sdl.rectTransform, 40, 248, 300, 26);
        Button sound = null;
        sound = Ui.Button(t, Sfx.Enabled ? "On" : "Off", () => { Sfx.Enabled = !Sfx.Enabled; Ui.SetLabel(sound, Sfx.Enabled ? "On" : "Off"); }, Ui.ButtonStyle.Secondary, 14);
        Ui.Place(sound.GetComponent<RectTransform>(), 420, 242, 140, 40);

        // The roster
        var rl = Ui.Text(t, "Agents", 18, Theme.Ink, true);
        Ui.Place(rl.rectTransform, 40, 316, 300, 26);
        var rs = Ui.Text(t, "One is yours. A class adds one per student; they all set out together.", 13, Theme.Text2);
        Ui.Place(rs.rectTransform, 40, 342, 720, 22);
        var rows = Ui.Node("Rows", t);
        Ui.Place(rows, 40, 374, 720, 380);
        void Build()
        {
            Ui.Clear(rows);
            float y = 0;
            for (int i = 0; i < app.configs.Count; i++)
            {
                int index = i;
                var c = app.configs[i];
                var row = Ui.Card(rows, "Agent", Theme.Panel, Theme.Border1);
                Ui.Place(row.rectTransform, 0, y, 720, 48);
                var dot = Ui.Box(row.transform, "Dot", Theme.Team[c.colour]);
                Ui.Place(dot.rectTransform, 14, 15, 18, 18);
                dot.raycastTarget = false;
                var name = Ui.Text(row.transform, string.IsNullOrWhiteSpace(c.name) ? "(unnamed)" : c.name, 16, Theme.Ink, true, TextAlignmentOptions.MidlineLeft);
                Ui.Place(name.rectTransform, 44, 0, 500, 48);
                if (app.configs.Count > 1)
                {
                    var remove = Ui.Button(row.transform, "Remove", () => { app.RemoveRobot(index); app.SaveConfigs(); Build(); }, Ui.ButtonStyle.Danger, 13);
                    Ui.Place(remove.GetComponent<RectTransform>(), 720 - 110, 7, 96, 34);
                }
                y += 56;
            }
            if (app.configs.Count < 10)
            {
                var add = Ui.Button(rows, "+ Add an agent", () =>
                {
                    var used = app.configs.Select(c => c.colour).ToHashSet();
                    int colour = Enumerable.Range(0, Theme.Team.Length).FirstOrDefault(i => !used.Contains(i));
                    var cfg = new AgentConfig { name = "Agent " + (app.configs.Count + 1), colour = colour };
                    app.lesson?.Apply(cfg);
                    var start = app.StartingSetup;
                    if (start != null) { cfg.Write(start.Fill(app.brief)); cfg.model = start.model; cfg.vision = start.vision; cfg.memory = start.memory; cfg.effort = start.effort; app.lesson?.Apply(cfg); }
                    app.AddRobot(cfg);
                    app.SaveConfigs();
                    Build();
                }, Ui.ButtonStyle.Secondary, 14);
                Ui.Place(add.GetComponent<RectTransform>(), 0, y + 4, 200, 40);
            }
        }
        Build();
        Sfx.Tick();
    }

    public static void Close()
    {
        if (open != null) Object.Destroy(open.gameObject);
        open = null;
    }
}
