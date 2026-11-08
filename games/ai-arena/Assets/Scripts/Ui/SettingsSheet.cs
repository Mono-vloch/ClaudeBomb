using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The class, behind the menu: one agent is the player's; a class adds one per student here
// and they all set out together. Sound, the voice, the speed and the turns are in the menu
// itself. Click anywhere off the sheet to close it.
public static class SettingsSheet
{
    static RectTransform open;
    public static bool IsOpen => open != null;

    public static void Toggle(App app, Transform screen, System.Action changed)
    {
        if (open != null) { Close(); return; }
        if (screen == null) return;
        open = Ui.Node("Class", screen);
        Ui.Stretch(open);
        var dim = Ui.Box(open, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.55f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(() => { Close(); changed?.Invoke(); });
        var card = Ui.Box(open, "Sheet", Ui.DarkFill);
        Ui.Place(card.rectTransform, 560, 150, 800, 700);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, "THE CLASS", Theme.Teal);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 40, 28, 400, 20);
        var close = Ui.Button(t, "Done", () => { Close(); changed?.Invoke(); }, Ui.ButtonStyle.Accent, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 800 - 40 - 100, 24, 100, 40);
        var rs = Ui.Text(t, "One agent is yours. A class adds one per student; they all set out together.", 15, Theme.Faint);
        Ui.Place(rs.rectTransform, 40, 60, 720, 22);
        var rows = Ui.Node("Rows", t);
        Ui.Place(rows, 40, 100, 720, 560);
        void Build()
        {
            Ui.Clear(rows);
            float y = 0;
            for (int i = 0; i < app.configs.Count; i++)
            {
                int index = i;
                var c = app.configs[i];
                var row = Ui.DarkCard(rows, "Agent");
                Ui.Place(row.rectTransform, 0, y, 720, 48);
                var dot = Ui.Box(row.transform, "Dot", Theme.Team[c.colour]);
                Ui.Place(dot.rectTransform, 14, 15, 18, 18);
                dot.raycastTarget = false;
                var name = Ui.DarkInput(row.transform, "Its name", false, 16);
                Ui.Place(name.GetComponent<RectTransform>(), 44, 6, 360, 36);
                name.text = string.IsNullOrWhiteSpace(c.name) ? "" : c.name;
                name.onEndEdit.AddListener(v => { if (!string.IsNullOrWhiteSpace(v)) c.name = v.Trim(); app.SaveConfigs(); });
                if (app.configs.Count > 1)
                {
                    var remove = Ui.Button(row.transform, "Remove", () => { app.RemoveRobot(index); app.SaveConfigs(); Build(); }, Ui.ButtonStyle.GhostOnDark, 13);
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
                }, Ui.ButtonStyle.GhostOnDark, 14);
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
