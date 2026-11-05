using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing before a room: a warm-up question from an earlier level (no score),
// LUMA's opening line, the idea in plain words, what the student will fix, and the
// facility in lockdown in a window, this level's door lit amber, with the ladder
// of levels over it and the record room by room under it.
public class LessonScreen
{
    public readonly RectTransform root;

    public LessonScreen(Transform parent, App app)
    {
        var l = app.room;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The world: this level's door in the lockdown corridor, framed in a window top right; soft behind the cards.
        var window = Ui.Node("Window", root);
        Ui.Place(window, 1000, 150, 860, 340);
        app.world.Build();
        int here = Facility.RoomIndex(l.room);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == here ? Building.DoorState.Current : i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowDoor(window, here);
        app.atmosphere.MenuBlur(true, 42f, 90f); // the door (34 units off) sharp, the corridor beyond it soft
        Sfx.Ambient(true);
        var scrim = Ui.Box(root, "Scrim", new Color(1f, 1f, 1f, 0.14f), false);
        Ui.Stretch(scrim.rectTransform);
        scrim.raycastTarget = false;

        // The brief, left.
        var brief = Ui.Float(root, "Brief", 60, 60, 900, 960);
        var b = brief.transform;
        var eyebrow = Ui.Eyebrow(b, $"BRIEFING  ·  TIER {l.tier}: {Rooms.TierNames[l.tier - 1].ToUpperInvariant()}  ·  {l.room.ToUpperInvariant()}", Theme.Text2);
        Ui.Place(eyebrow.rectTransform, 40, 32, 820, 22);
        var level = Ui.Eyebrow(b, $"LEVEL {l.id} OF 3.4", Theme.TealDeep);
        level.font = Theme.Bold;
        Ui.Place(level.rectTransform, 40, 78, 600, 22);
        var title = Ui.Text(b, l.title, 40, Theme.Ink, true);
        Ui.Place(title.rectTransform, 36, 102, 830, 100);
        var kicker = Ui.Text(b, l.kicker, 23, Theme.Aqua, true);
        Ui.Place(kicker.rectTransform, 40, 208, 820, 64);

        Luma.Card(b, 40, 276, 820, 110, app.luma.Say("intro-" + l.number), 17);

        var learn = Ui.Text(b, l.learn, 17, Theme.Text2);
        Ui.Place(learn.rectTransform, 40, 400, 820, 160);

        var msg = Ui.Soft(b, "Fixes");
        Ui.Place(msg.rectTransform, 40, 570, 820, 276);
        var from = Ui.Eyebrow(msg.transform, "YOU FIX", Theme.TealDeep);
        Ui.Place(from.rectTransform, 24, 16, 780, 22);
        var fixes = Ui.Text(msg.transform, l.fixes, 18, Theme.Ink);
        Ui.Place(fixes.rectTransform, 24, 42, 772, 90);
        if (!string.IsNullOrEmpty(l.mistake))
        {
            var fix = Ui.Eyebrow(msg.transform, "FIX IT: " + l.mistake.ToUpperInvariant(), Theme.Danger);
            Ui.Place(fix.rectTransform, 24, 150, 780, 22);
            var why = Ui.Text(msg.transform, l.why, 14, Theme.Text2);
            Ui.Place(why.rectTransform, 24, 174, 772, 90);
        }

        var go = Ui.Button(b, "Enter the room", app.ShowRoom, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 40, 866, 320, 64);
        var home = Ui.Button(b, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 380, 866, 200, 64);

        // The ladder, over the window: the nineteen levels. Any other step jumps there.
        var strip = Ui.Glass(root, "Ladder", 1000, 60, 860, 82, false);
        var ladder = Ui.Eyebrow(strip.transform, "THE LADDER  ·  CLICK A STEP TO JUMP THERE", Theme.Teal);
        Ui.Place(ladder.rectTransform, 20, 10, 600, 20);
        int n = 0;
        foreach (var step in Rooms.All)
        {
            bool current = step.number == l.number, done = step.number <= app.LevelsDone;
            var card = Ui.Card(strip.transform, "Step", current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.08f), current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.3f));
            Ui.Place(card.rectTransform, 16 + n * 44, 34, 40, 38);
            var num = Ui.Text(card.transform, step.id, 11, current ? Theme.Ink : Color.white, true, TextAlignmentOptions.Center);
            Ui.Stretch(num.rectTransform);
            if (!current)
            {
                card.raycastTarget = true;
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                int number = step.number;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
            n++;
        }
        var caption = Ui.Glass(root, "Caption", 1000, 440, 860, 50);
        var ct = Ui.Text(caption.transform, $"the facility in lockdown  ·  level {l.id}  ·  the {l.room}'s door is lit amber", 14, Look.Chalk, false, TextAlignmentOptions.MidlineLeft);
        Ui.Place(ct.rectTransform, 20, 0, 820, 50);
        ct.enableWordWrapping = false; ct.overflowMode = TextOverflowModes.Ellipsis;

        // The facility, room by room: what the record says.
        var parts = Ui.Float(root, "Facility", 1000, 510, 860, 510);
        var pt = Ui.Text(parts.transform, "The facility, room by room", 22, Theme.Ink, true);
        Ui.Place(pt.rectTransform, 26, 20, 800, 30);
        var ps = Ui.Text(parts.transform, "Every room you leave with a gap is a link the attacker will use later. The exit asks for the chain.", 14, Theme.Text2);
        Ui.Place(ps.rectTransform, 26, 52, 800, 40);
        float y = 96;
        for (int r = 0; r < Facility.Rooms.Length; r++)
        {
            string name = Facility.Rooms[r];
            bool isHere = name == l.room;
            var line = app.session.record.FindAll(x => x.room == name);
            bool held = line.Count > 0 && line.TrueForAll(x => x.gap == "held"), gap = line.Count > 0 && !held;
            var row = Ui.Card(parts.transform, name, isHere ? Theme.Surface2 : Theme.Panel, isHere ? Theme.BorderTeal : Theme.Border1);
            Ui.Place(row.rectTransform, 20, y, 820, 31);
            var lamp = Ui.Box(row.transform, "Lamp", held ? Theme.Aqua : gap ? Theme.Danger : isHere ? Theme.Amber : Theme.Border, false);
            Ui.Place(lamp.rectTransform, 14, 10, 11, 11);
            lamp.raycastTarget = false;
            var rn = Ui.Text(row.transform, name, 15, Theme.Ink, true);
            Ui.Place(rn.rectTransform, 36, 5, 300, 24);
            var s = Ui.Text(row.transform, held ? "held" : gap ? "a gap: " + line[0].gap : isHere ? "this level" : "not yet", 12, held ? Theme.TealDeep : gap ? Theme.Danger : isHere ? Theme.Amber : Theme.Muted, false, TextAlignmentOptions.TopRight);
            Ui.Place(s.rectTransform, 340, 7, 464, 22);
            s.enableWordWrapping = false; s.overflowMode = TextOverflowModes.Ellipsis;
            y += 34;
        }
        Ui.Emblem(root);

        if (l.recall != null) Recall(app, l.recall);
    }

    // The warm-up: one question from an earlier level, over the briefing. No score.
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
                if (right) Sfx.Chime(); else Sfx.Ruin();
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
