using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The mission briefing, before the lists are written: the errand's market on the
// right, sharp, the agents at their houses; on the left, in dark glass, the errand's
// question, its headline, LUMA's opening line spoken and typed, the family's message,
// and the agent's six parts with this errand's lit. Everything the course says beyond
// that is in the file.
public class LessonScreen
{
    public readonly RectTransform root;
    static readonly string[] Parts = { "The list", "Sight", "Memory", "Brain", "Effort", "Pocket money" };

    public LessonScreen(Transform parent, App app)
    {
        var l = app.lesson;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The world: this errand's market by day, on the right, sharp.
        var window = Ui.Node("Window", root);
        Ui.Place(window, 900, 150, 980, 880);
        app.world.Build(new Arena(app.brief, app.configs.ConvertAll(c => c.Clone()), app.budget, l.number * 17 + 3), false);
        if (Market.Real) app.world.ShowOrbitOf(window); else app.world.ShowMarket(window); // the real square turning slowly, at the height of its roofs
        app.atmosphere.MenuBlur(false);

        // The briefing, left, in dark glass over the park.
        var panel = Ui.Glass(root, "Brief", 40, 40, 840, 860, false);
        var b = panel.transform;
        var eyebrow = Ui.Eyebrow(b, $"ERRAND {l.id} OF {Ladder.Count}  ·  THE BRIEFING", Theme.Teal);
        eyebrow.font = Theme.Bold;
        Ui.Place(eyebrow.rectTransform, 40, 36, 760, 22);
        var title = Ui.Text(b, l.title, 44, Color.white, true);
        Ui.Place(title.rectTransform, 36, 66, 780, 120);
        var kicker = Ui.Text(b, l.kicker, 24, Theme.Teal, true);
        Ui.Place(kicker.rectTransform, 40, 192, 760, 70);

        app.luma.Line(b, 40, 290, 760, app.luma.Say("intro-" + l.number), true, true, 18);

        // The mission: the family's message, as the garage will show it.
        var msg = Ui.Box(b, "Message", new Color(1f, 1f, 1f, 0.08f));
        Ui.Place(msg.rectTransform, 40, 420, 760, 120);
        msg.raycastTarget = false;
        var from = Ui.Eyebrow(msg.transform, $"A MESSAGE FROM {app.brief.from.ToUpperInvariant()}", Theme.Teal);
        Ui.Place(from.rectTransform, 24, 16, 700, 22);
        var rule = Ui.Text(msg.transform, app.brief.rule, 22, Color.white);
        Ui.Place(rule.rectTransform, 24, 42, 712, 70);

        // The agent's six parts: this errand's lit, the ones already open in white, the rest dim.
        var partsTag = Ui.Eyebrow(b, "YOUR AGENT, PART BY PART  ·  THIS ERRAND OPENS", Theme.Teal);
        Ui.Place(partsTag.rectTransform, 40, 572, 760, 20);
        bool[] open = { true, l.vision, l.memory, l.model, l.effort, true };
        bool[] lit =
        {
            l.number <= 2 || l.decide, l.number == Ladder.VisionLevel, l.number == Ladder.MemoryLevel,
            l.number == Ladder.ModelLevel, l.number == Ladder.EffortLevel, l.number == Ladder.Count,
        };
        for (int i = 0; i < Parts.Length; i++)
        {
            float x = 40 + (i % 3) * 256, y = 600 + (i / 3) * 56;
            var chip = Ui.Box(b, Parts[i], lit[i] ? Theme.Teal : open[i] ? new Color(1f, 1f, 1f, 0.14f) : new Color(1f, 1f, 1f, 0.05f));
            Ui.Place(chip.rectTransform, x, y, 240, 46);
            chip.raycastTarget = false;
            var name = Ui.Text(chip.transform, l.decide && i == 0 ? "Agent, or a fixed rule" : Parts[i], 16, lit[i] ? Theme.Ink : open[i] ? Color.white : Theme.Faint, true, TextAlignmentOptions.Center);
            Ui.Stretch(name.rectTransform);
        }

        var go = Ui.Button(b, l.decide ? "Decide the tasks" : "Go to the garage", app.ShowSetup, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 40, 756, 320, 64);
        var file = Ui.Button(b, "The file", () => app.ShowFile(), Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(file.GetComponent<RectTransform>(), 380, 756, 130, 64);
        var home = Ui.Button(b, "Title screen", app.ShowTitle, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 530, 756, 170, 64);

        // The ladder, over the market: the errands. Any other step jumps there.
        var strip = Ui.Glass(root, "Ladder", 920, 40, 960, 82, false);
        var ladder = Ui.Eyebrow(strip.transform, "THE LADDER", Theme.Teal);
        Ui.Place(ladder.rectTransform, 20, 10, 600, 20);
        for (int i = 1; i <= Ladder.Count; i++)
        {
            bool current = i == l.number, done = i <= app.LevelsDone;
            var step = Ui.Card(strip.transform, "Step", current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.08f), current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.3f));
            Ui.Place(step.rectTransform, 20 + (i - 1) * 70, 34, 62, 38);
            var n = Ui.Text(step.transform, "1." + i, 14, current ? Theme.Ink : Color.white, true, TextAlignmentOptions.Center);
            Ui.Stretch(n.rectTransform);
            if (!current)
            {
                step.raycastTarget = true;
                var button = step.gameObject.AddComponent<Button>();
                button.targetGraphic = step;
                int number = i;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
        }
        Ui.Emblem(root);

        // The warm-up question (Recall) is off: the briefing opens straight onto the level.
        // To bring it back: if (l.recall != null) Recall(app, l.recall);
    }

    // The warm-up: one question from an earlier errand, over the briefing. No
    // score. A wrong answer shows the right one.
    void Recall(App app, Recall r)
    {
        var overlay = Ui.Node("Recall", root);
        Ui.Stretch(overlay);
        var dim = Ui.Box(overlay, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.72f), false);
        Ui.Stretch(dim.rectTransform);
        dim.raycastTarget = true;
        var card = Ui.Float(overlay, "Card", 560, 300, 800, 440);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"WARM-UP  ·  FROM ERRAND {r.from}  ·  NO SCORE", Theme.TealDeep);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 32, 28, 600, 22);
        var q = Ui.Text(t, r.question, 26, Theme.Ink, true);
        Ui.Place(q.rectTransform, 32, 60, 736, 80);
        var verdict = Ui.Text(t, "", 20, Theme.Ink, true);
        Ui.Place(verdict.rectTransform, 32, 330, 560, 56);
        var go = Ui.Button(t, "Continue", () => Object.Destroy(overlay.gameObject), Ui.ButtonStyle.Accent, 17);
        Ui.Place(go.GetComponent<RectTransform>(), 800 - 32 - 160, 380, 160, 46);
        go.gameObject.SetActive(false);
        var buttons = new Button[r.options.Length];
        for (int i = 0; i < r.options.Length; i++)
        {
            int index = i;
            buttons[i] = Ui.Button(t, r.options[i], () =>
            {
                bool right = index == r.answer;
                verdict.text = right ? "Right." : "Not quite.";
                verdict.color = right ? Theme.TealDeep : Theme.Danger;
                if (right) Sfx.Chime(); else Sfx.Ruin();
                for (int j = 0; j < buttons.Length; j++)
                {
                    buttons[j].interactable = false;
                    buttons[j].transform.Find("Fill").GetComponent<Image>().color = j == r.answer ? Theme.Surface2 : j == index ? Theme.DangerSoft : Theme.Panel;
                    buttons[j].GetComponent<Image>().color = j == r.answer ? Theme.Aqua : j == index ? Theme.Danger : Theme.Border;
                }
                go.gameObject.SetActive(true);
            }, Ui.ButtonStyle.Secondary, 17);
            var rt = buttons[i].GetComponent<RectTransform>();
            Ui.Place(rt, 32, 150 + i * 56, 736, 48);
            var label = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.MidlineLeft;
            Ui.Stretch(label.rectTransform, 20, 0, 20, 0);
        }
    }
}
