using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing before a room: a warm-up question from an earlier level (no score),
// LUMA's opening line, the idea in plain words, what the student will fix, and
// the facility room by room with what the record says about each.
public class LessonScreen
{
    public readonly RectTransform root;

    public LessonScreen(Transform parent, App app)
    {
        var l = app.room;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Theme.Page);

        var eyebrow = Ui.Eyebrow(root, $"BRIEFING  ·  TIER {l.tier}: {Rooms.TierNames[l.tier - 1].ToUpperInvariant()}  ·  {l.room.ToUpperInvariant()}", Theme.Text2);
        Ui.Place(eyebrow.rectTransform, 160, 72, 1000, 22);

        var ladder = Ui.Eyebrow(root, "THE LADDER  ·  CLICK A STEP TO JUMP THERE", Theme.Muted);
        Ui.Place(ladder.rectTransform, 1160, 40, 600, 20);
        int i = 0;
        foreach (var step in Rooms.All)
        {
            bool current = step.number == l.number, done = step.number <= app.LevelsDone;
            var card = Ui.Card(root, "Step", current ? Theme.TealDeep : done ? Theme.Surface2 : Theme.Panel, current ? Theme.TealDeep : done ? Theme.BorderTeal : Theme.Border);
            Ui.Place(card.rectTransform, 1160 + (i % 10) * 60, 62 + (i / 10) * 46, 54, 40);
            var n = Ui.Text(card.transform, step.id, 12, current ? Color.white : done ? Theme.TealDeep : Theme.Muted, true, TextAlignmentOptions.Center);
            Ui.Stretch(n.rectTransform);
            if (!current)
            {
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                int number = step.number;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
            i++;
        }

        var level = Ui.Eyebrow(root, $"LEVEL {l.id} OF 3.4", Theme.TealDeep);
        level.font = Theme.Bold;
        Ui.Place(level.rectTransform, 160, 130, 600, 22);
        var title = Ui.Text(root, l.title, 52, Theme.Ink, true);
        Ui.Place(title.rectTransform, 156, 156, 980, 80);
        var kicker = Ui.Text(root, l.kicker, 27, Theme.Aqua, true);
        Ui.Place(kicker.rectTransform, 160, 246, 960, 84);

        Luma.Card(root, 160, 340, 960, 110, app.luma.Say("intro-" + l.number), 17);

        var learn = Ui.Text(root, l.learn, 18, Theme.Text2);
        Ui.Place(learn.rectTransform, 160, 464, 960, 220);

        var msg = Ui.Soft(root, "Fixes");
        Ui.Place(msg.rectTransform, 160, 700, 960, 190);
        var from = Ui.Eyebrow(msg.transform, "YOU FIX", Theme.TealDeep);
        Ui.Place(from.rectTransform, 24, 16, 900, 22);
        var fixes = Ui.Text(msg.transform, l.fixes, 19, Theme.Ink);
        Ui.Place(fixes.rectTransform, 24, 42, 912, 70);
        if (!string.IsNullOrEmpty(l.mistake))
        {
            var fix = Ui.Eyebrow(msg.transform, "FIX IT: " + l.mistake.ToUpperInvariant(), Theme.Danger);
            Ui.Place(fix.rectTransform, 24, 118, 900, 22);
            var why = Ui.Text(msg.transform, l.why, 14, Theme.Text2);
            Ui.Place(why.rectTransform, 24, 140, 912, 44);
        }

        // The facility, room by room: what the record says.
        var parts = Ui.Float(root, "Facility", 1160, 170, 600, 690);
        var pt = Ui.Text(parts.transform, "The facility, room by room", 22, Theme.Ink, true);
        Ui.Place(pt.rectTransform, 26, 22, 540, 30);
        var ps = Ui.Text(parts.transform, "Every room you leave with a gap is a link the attacker will use later. The exit asks for the chain.", 14, Theme.Text2);
        Ui.Place(ps.rectTransform, 26, 54, 540, 40);
        float y = 104;
        for (int r = 0; r < Facility.Rooms.Length; r++)
        {
            string name = Facility.Rooms[r];
            bool here = name == l.room;
            var line = app.session.record.FindAll(x => x.room == name);
            bool held = line.Count > 0 && line.TrueForAll(x => x.gap == "held"), gap = line.Count > 0 && !held;
            var row = Ui.Card(parts.transform, name, here ? Theme.Surface2 : Theme.Panel, here ? Theme.BorderTeal : Theme.Border1);
            Ui.Place(row.rectTransform, 20, y, 560, 42);
            var dot = Ui.Dot(row.transform, held ? Theme.Aqua : gap ? Theme.Danger : Theme.Border, 12);
            Ui.Place(dot.rectTransform, 16, 15, 12, 12);
            var n = Ui.Text(row.transform, name, 16, Theme.Ink, true);
            Ui.Place(n.rectTransform, 38, 10, 260, 24);
            var s = Ui.Text(row.transform, held ? "held" : gap ? "a gap: " + line[0].gap : here ? "this level" : "not yet", 12, held ? Theme.TealDeep : gap ? Theme.Danger : Theme.Muted, false, TextAlignmentOptions.TopRight);
            Ui.Place(s.rectTransform, 280, 12, 264, 22);
            s.enableWordWrapping = false; s.overflowMode = TextOverflowModes.Ellipsis;
            y += 46;
        }

        var go = Ui.Button(root, "Enter the room", app.ShowRoom, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 160, 926, 320, 64);
        var home = Ui.Button(root, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 500, 926, 200, 64);
        Ui.Emblem(root);

        if (l.recall != null) Recall(app, l.recall);
    }

    void Recall(App app, Recall r)
    {
        var overlay = Ui.Node("Recall", root);
        Ui.Stretch(overlay);
        var dim = Ui.Box(overlay, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.72f), false);
        Ui.Stretch(dim.rectTransform);
        dim.raycastTarget = true;
        var card = Ui.Float(overlay, "Card", 560, 300, 800, 440);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"WARM-UP  ·  FROM LEVEL {r.from}  ·  NO SCORE", Theme.TealDeep);
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
                verdict.text = right ? $"Right. That is from level {r.from}." : $"Not quite. The answer: {r.options[r.answer]}. That is from level {r.from}.";
                verdict.color = right ? Theme.TealDeep : Theme.Danger;
                for (int j = 0; j < buttons.Length; j++)
                {
                    buttons[j].interactable = false;
                    buttons[j].transform.Find("Fill").GetComponent<Image>().color = j == r.answer ? Theme.Surface2 : j == index ? Theme.DangerSoft : Theme.Panel;
                }
                go.gameObject.SetActive(true);
            }, Ui.ButtonStyle.Secondary, 17);
            Ui.Place(buttons[i].GetComponent<RectTransform>(), 32, 150 + i * 56, 736, 48);
            var label = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.MidlineLeft;
            Ui.Stretch(label.rectTransform, 20, 0, 20, 0);
        }
    }
}
