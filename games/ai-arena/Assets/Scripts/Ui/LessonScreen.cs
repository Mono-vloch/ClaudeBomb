using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The mission briefing, before the lists are written: a warm-up question from an
// earlier errand (no score; a wrong answer shows the right one), LUMA's opening
// line, the idea in plain words, the family's message, and the robot's parts
// with the dial this errand opens lit up, so the ladder itself is visible.
public class LessonScreen
{
    public readonly RectTransform root;

    public LessonScreen(Transform parent, App app)
    {
        var l = app.lesson;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The world: this errand's market by day, framed in a window top right; soft behind the cards.
        var window = Ui.Node("Window", root);
        Ui.Place(window, 1000, 150, 860, 340);
        app.world.Build(new Arena(app.brief, app.configs.ConvertAll(c => c.Clone()), app.budget, l.number * 17 + 3), false);
        app.world.ShowMarket(window);
        app.atmosphere.MenuBlur(true);
        var scrim = Ui.Box(root, "Scrim", new Color(1f, 1f, 1f, 0.3f), false);
        Ui.Stretch(scrim.rectTransform);
        scrim.raycastTarget = false;

        // The brief, left.
        var brief = Ui.Float(root, "Brief", 60, 60, 900, 960);
        var b = brief.transform;
        var eyebrow = Ui.Eyebrow(b, "MISSION BRIEFING  ·  TIER 1: WHAT AN AGENT IS", Theme.Text2);
        Ui.Place(eyebrow.rectTransform, 40, 32, 820, 22);
        var level = Ui.Eyebrow(b, $"ERRAND {l.id} OF {Ladder.Count}", Theme.TealDeep);
        level.font = Theme.Bold;
        Ui.Place(level.rectTransform, 40, 78, 600, 22);
        var title = Ui.Text(b, l.title, 40, Theme.Ink, true);
        Ui.Place(title.rectTransform, 36, 102, 830, 100); // two lines, the errands' questions are long
        var kicker = Ui.Text(b, l.kicker, 23, Theme.Aqua, true);
        Ui.Place(kicker.rectTransform, 40, 208, 820, 64);

        Luma.Card(b, 40, 276, 820, 110, app.luma.Say("intro-" + l.number), 18);

        var learn = Ui.Text(b, l.learn, 17, Theme.Text2);
        Ui.Place(learn.rectTransform, 40, 400, 820, 150);

        // The mission: the family's message, as the garage will show it.
        var msg = Ui.Soft(b, "Message");
        Ui.Place(msg.rectTransform, 40, 560, 820, 286);
        var from = Ui.Eyebrow(msg.transform, $"MISSION  ·  A MESSAGE FROM {app.brief.from.ToUpperInvariant()}  ·  {app.brief.title.ToUpperInvariant()}", Theme.TealDeep);
        Ui.Place(from.rectTransform, 24, 16, 780, 22);
        var rule = Ui.Text(msg.transform, app.brief.rule, 22, Theme.Ink);
        Ui.Place(rule.rectTransform, 24, 42, 772, 64);
        var note = Ui.Text(msg.transform, "The catch: " + app.brief.note, 14, Theme.Text2);
        Ui.Place(note.rectTransform, 24, 108, 772, 40);
        var sells = Ui.Eyebrow(msg.transform, "AT THE MARKET", Theme.TealDeep);
        Ui.Place(sells.rectTransform, 24, 150, 300, 22);
        Ui.Catalogue(msg.transform, 24, 172, 772, 26, Theme.Ink);

        var go = Ui.Button(b, l.decide ? "Decide the tasks" : "Go to the garage", app.ShowSetup, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 40, 866, 320, 64);
        var home = Ui.Button(b, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 380, 866, 200, 64);

        // The ladder, over the window: the errands. Any other step jumps there.
        var strip = Ui.Glass(root, "Ladder", 1000, 60, 860, 82, false);
        var ladder = Ui.Eyebrow(strip.transform, "THE LADDER  ·  CLICK A STEP TO JUMP THERE", Theme.Teal);
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
        var caption = Ui.Glass(root, "Caption", 1000, 440, 860, 50);
        var ct = Ui.Text(caption.transform, $"the market  ·  {app.brief.title}  ·  the agents at their houses", 14, Look.Chalk, false, TextAlignmentOptions.MidlineLeft);
        Ui.Place(ct.rectTransform, 20, 0, 820, 50);
        ct.enableWordWrapping = false; ct.overflowMode = TextOverflowModes.Ellipsis;

        // The agent's parts, below the window: what each dial is and whether it is open yet.
        var parts = Ui.Float(root, "Parts", 1000, 510, 860, 510);
        var pt = Ui.Text(parts.transform, "Your agent, part by part", 22, Theme.Ink, true);
        Ui.Place(pt.rectTransform, 24, 18, 600, 30);
        var ps = Ui.Text(parts.transform, "Each errand opens one more dial. The ones with a dot are this errand's.", 14, Theme.Text2);
        Ui.Place(ps.rectTransform, 24, 48, 800, 22);
        float y = 80; int col = 0;
        Part(parts.transform, ref y, ref col, "The list", "Its job description: everything it knows about tonight.", "You write it", true, l.number <= 2);
        Part(parts.transform, ref y, ref col, "How far it sees", "Its context: the stalls in view, turned into words each turn.",
             l.vision ? "You choose: 1 to 3 cells" : $"Fixed at {Lesson.FixedVision} cells · opens at errand 1.{Ladder.VisionLevel}", l.vision, l.number == Ladder.VisionLevel);
        Part(parts.transform, ref y, ref col, "Memory", "What rides along from earlier turns, if anything.",
             l.memory ? "You choose" : $"Fixed: {MemoryModes.Labels[Lesson.FixedMemory].ToLowerInvariant()} · opens at errand 1.{Ladder.MemoryLevel}", l.memory, l.number == Ladder.MemoryLevel);
        Part(parts.transform, ref y, ref col, "Brain", "Which Claude runs it, and what each of its tokens costs.",
             l.model ? "You choose" : $"Fixed: {Models.All[Lesson.FixedModel].label} · opens at errand 1.{Ladder.ModelLevel}", l.model, l.number == Ladder.ModelLevel);
        Part(parts.transform, ref y, ref col, "Effort", "How hard it thinks at each stall.",
             l.effort ? "You choose" : $"Fixed: {Efforts.Labels[Lesson.FixedEffort].ToLowerInvariant()} · opens at errand 1.{Ladder.EffortLevel}", l.effort, l.number == Ladder.EffortLevel);
        Part(parts.transform, ref y, ref col, l.decide ? "Agent, or a fixed rule" : "Pocket money",
             l.decide ? "Whether the task needs an agent at all, or a simple automation, or neither." : "The tokens it may spend on the errand. Every dial above spends some, every turn.",
             l.decide ? "You decide, task by task" : "Set in the garage", true, l.decide || l.number == Ladder.Count);
        Ui.Emblem(root);

        if (l.recall != null) Recall(app, l.recall);
    }

    // Two columns of parts, 404 wide, 130 tall.
    static void Part(Transform t, ref float y, ref int col, string name, string what, string state, bool open, bool lit)
    {
        float x = 20 + col * 416;
        var row = Ui.Card(t, name, lit ? Theme.Surface2 : Theme.Panel, lit ? Theme.BorderTeal : Theme.Border1);
        Ui.Place(row.rectTransform, x, y, 404, 130);
        if (lit) { var dot = Ui.Box(row.transform, "Dot", Theme.Aqua); Ui.Place(dot.rectTransform, 14, 18, 10, 10); dot.raycastTarget = false; }
        var n = Ui.Text(row.transform, name, 17, Theme.Ink, true);
        Ui.Place(n.rectTransform, lit ? 30 : 14, 10, 220, 24);
        var s = Ui.Text(row.transform, state, 11, open ? Theme.TealDeep : Theme.Muted, true, TextAlignmentOptions.TopRight);
        Ui.Place(s.rectTransform, 200, 12, 192, 36);
        var w = Ui.Text(row.transform, what, 13, Theme.Text2);
        Ui.Place(w.rectTransform, 14, 52, 378, 72);
        col++;
        if (col == 2) { col = 0; y += 138; }
    }

    // The warm-up: one question from an earlier errand, over the briefing. No
    // score. A wrong answer shows the right one and the errand it came from.
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
        var verdict = Ui.Text(t, "", 17, Theme.Ink);
        Ui.Place(verdict.rectTransform, 32, 330, 736, 56);
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
                verdict.text = right ? $"Right. That is from errand {r.from}."
                                     : $"Not quite. The answer: {r.options[r.answer]}. That is from errand {r.from}.";
                verdict.color = right ? Theme.TealDeep : Theme.Danger;
                if (right) Sfx.Chime(); else Sfx.Ruin();
                for (int j = 0; j < buttons.Length; j++)
                {
                    buttons[j].interactable = false;
                    buttons[j].transform.Find("Fill").GetComponent<Image>().color = j == r.answer ? Theme.Surface2 : j == index ? Theme.DangerSoft : Theme.Panel;
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
