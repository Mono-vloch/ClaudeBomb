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
        app.Paper(Theme.Page);

        var eyebrow = Ui.Eyebrow(root, "MISSION BRIEFING  ·  TIER 1: WHAT AN AGENT IS", Theme.Text2);
        Ui.Place(eyebrow.rectTransform, 160, 72, 800, 22);

        // The ladder, top right: where this errand sits. Any other step jumps there.
        var ladder = Ui.Eyebrow(root, "THE LADDER  ·  CLICK A STEP TO JUMP THERE", Theme.Muted);
        Ui.Place(ladder.rectTransform, 1260, 40, 500, 20);
        for (int i = 1; i <= Ladder.Count; i++)
        {
            bool current = i == l.number, done = i <= app.LevelsDone;
            var step = Ui.Card(root, "Step", current ? Theme.TealDeep : done ? Theme.Surface2 : Theme.Panel, current ? Theme.TealDeep : done ? Theme.BorderTeal : Theme.Border);
            Ui.Place(step.rectTransform, 1260 + (i - 1) * 58, 62, 50, 50);
            var n = Ui.Text(step.transform, "1." + i, 15, current ? Color.white : done ? Theme.TealDeep : Theme.Muted, true, TextAlignmentOptions.Center);
            Ui.Stretch(n.rectTransform);
            if (!current)
            {
                var button = step.gameObject.AddComponent<Button>();
                button.targetGraphic = step;
                int number = i;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
        }

        var level = Ui.Eyebrow(root, $"ERRAND {l.id} OF {Ladder.Count}", Theme.TealDeep);
        level.font = Theme.Bold;
        Ui.Place(level.rectTransform, 160, 130, 600, 22);
        var title = Ui.Text(root, l.title, 58, Theme.Ink, true);
        Ui.Place(title.rectTransform, 156, 156, 1060, 80);
        var kicker = Ui.Text(root, l.kicker, 30, Theme.Aqua, true);
        Ui.Place(kicker.rectTransform, 160, 246, 1040, 84);

        Luma.Card(root, 160, 340, 1040, 110, app.luma.Say("intro-" + l.number), 18);

        var learn = Ui.Text(root, l.learn, 19, Theme.Text2);
        Ui.Place(learn.rectTransform, 160, 464, 1040, 176);

        // The mission: the family's message, as the garage will show it.
        var msg = Ui.Soft(root, "Message");
        Ui.Place(msg.rectTransform, 160, 652, 1040, 250);
        var from = Ui.Eyebrow(msg.transform, $"MISSION  ·  A MESSAGE FROM {app.brief.from.ToUpperInvariant()}  ·  {app.brief.title.ToUpperInvariant()}", Theme.TealDeep);
        Ui.Place(from.rectTransform, 24, 16, 900, 22);
        var rule = Ui.Text(msg.transform, app.brief.rule, 24, Theme.Ink);
        Ui.Place(rule.rectTransform, 24, 42, 990, 64);
        var note = Ui.Text(msg.transform, "The catch: " + app.brief.note, 15, Theme.Text2);
        Ui.Place(note.rectTransform, 24, 108, 990, 24);
        var sells = Ui.Eyebrow(msg.transform, "AT THE MARKET", Theme.TealDeep);
        Ui.Place(sells.rectTransform, 24, 138, 300, 22);
        Ui.Catalogue(msg.transform, 24, 160, 990, 28, Theme.Ink);

        // The robot's parts, right: what each dial is and whether it is open yet.
        var parts = Ui.Float(root, "Parts", 1260, 130, 500, 692);
        var pt = Ui.Text(parts.transform, "Your agent, part by part", 22, Theme.Ink, true);
        Ui.Place(pt.rectTransform, 26, 22, 440, 30);
        var ps = Ui.Text(parts.transform, "Each errand opens one more dial. The ones with a dot are this errand's.", 15, Theme.Text2);
        Ui.Place(ps.rectTransform, 26, 54, 440, 40);
        float y = 102;
        Part(parts.transform, ref y, "The list", "Its job description: everything it knows about tonight.", "You write it", true, l.number <= 2);
        Part(parts.transform, ref y, "How far it sees", "Its context: the stalls in view, turned into words each turn.",
             l.vision ? "You choose: 1 to 3 cells" : $"Fixed at {Lesson.FixedVision} cells · opens at errand 1.{Ladder.VisionLevel}", l.vision, l.number == Ladder.VisionLevel);
        Part(parts.transform, ref y, "Memory", "What rides along from earlier turns, if anything.",
             l.memory ? "You choose" : $"Fixed: {MemoryModes.Labels[Lesson.FixedMemory].ToLowerInvariant()} · opens at errand 1.{Ladder.MemoryLevel}", l.memory, l.number == Ladder.MemoryLevel);
        Part(parts.transform, ref y, "Brain", "Which Claude runs it, and what each of its tokens costs.",
             l.model ? "You choose" : $"Fixed: {Models.All[Lesson.FixedModel].label} · opens at errand 1.{Ladder.ModelLevel}", l.model, l.number == Ladder.ModelLevel);
        Part(parts.transform, ref y, "Effort", "How hard it thinks at each stall.",
             l.effort ? "You choose" : $"Fixed: {Efforts.Labels[Lesson.FixedEffort].ToLowerInvariant()} · opens at errand 1.{Ladder.EffortLevel}", l.effort, l.number == Ladder.EffortLevel);
        Part(parts.transform, ref y, l.decide ? "Agent, or a fixed rule" : "Pocket money",
             l.decide ? "Whether the task needs an agent at all, or a simple automation, or neither." : "The tokens it may spend on the errand. Every dial above spends some, every turn.",
             l.decide ? "You decide, task by task" : "Set in the garage", true, l.decide || l.number == Ladder.Count);

        var go = Ui.Button(root, l.decide ? "Decide the tasks" : "Go to the garage", app.ShowSetup, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 160, 926, 320, 64);
        var home = Ui.Button(root, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 500, 926, 200, 64);
        Ui.Emblem(root);

        if (l.recall != null) Recall(app, l.recall);
    }

    static void Part(Transform t, ref float y, string name, string what, string state, bool open, bool lit)
    {
        var row = Ui.Card(t, name, lit ? Theme.Surface2 : Theme.Panel, lit ? Theme.BorderTeal : Theme.Border1);
        Ui.Place(row.rectTransform, 20, y, 460, 88);
        if (lit)
        {
            var dot = Ui.Dot(row.transform, Theme.Aqua, 10);
            Ui.Place(dot.rectTransform, 18, 20, 10, 10);
        }
        var n = Ui.Text(row.transform, name, 19, Theme.Ink, true);
        Ui.Place(n.rectTransform, lit ? 34 : 18, 12, 220, 26);
        var s = Ui.Text(row.transform, state, 13, open ? Theme.TealDeep : Theme.Muted, true, TextAlignmentOptions.TopRight);
        Ui.Place(s.rectTransform, 200, 14, 244, 40);
        var w = Ui.Text(row.transform, what, 14, Theme.Text2);
        Ui.Place(w.rectTransform, 18, 44, 424, 40);
        y += 96;
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
