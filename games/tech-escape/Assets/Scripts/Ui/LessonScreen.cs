using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing before a room: this level's door in the lockdown corridor on the
// right, lit amber, sharp; on the left, in dark glass, the level's question, its
// headline, LUMA's opening line spoken and typed, what the student fixes, the
// planned mistake's name, and the twelve rooms as lamps (held, a gap, this one,
// not yet). Everything the course says beyond that is in the file.
public class LessonScreen
{
    public readonly RectTransform root;

    public LessonScreen(Transform parent, App app)
    {
        var l = app.room;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The world: this level's door in the corridor, on the right, sharp.
        var window = Ui.Node("Window", root);
        Ui.Place(window, 900, 150, 980, 880);
        app.world.Build();
        int here = Facility.RoomIndex(l.room);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == here ? Building.DoorState.Current : i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowDoor(window, here);
        app.atmosphere.MenuBlur(false);
        Sfx.Ambient(true);

        // The briefing, left, in dark glass over the facility.
        var panel = Ui.Glass(root, "Brief", 40, 40, 840, 860, false);
        var b = panel.transform;
        var eyebrow = Ui.Eyebrow(b, $"LEVEL {l.id} OF 3.4  ·  {l.room.ToUpperInvariant()}  ·  THE BRIEFING", Theme.Teal);
        eyebrow.font = Theme.Bold;
        Ui.Place(eyebrow.rectTransform, 40, 36, 760, 22);
        var title = Ui.Text(b, l.title, 44, Color.white, true);
        Ui.Place(title.rectTransform, 36, 64, 780, 110);
        var kicker = Ui.Text(b, l.kicker, 23, Theme.Teal, true);
        Ui.Place(kicker.rectTransform, 40, 178, 760, 70);

        app.luma.Line(b, 40, 258, 760, app.luma.Say("intro-" + l.number), true, true, 18);

        // What the student fixes, and the planned mistake's name.
        var msg = Ui.Box(b, "Fixes", new Color(1f, 1f, 1f, 0.08f));
        Ui.Place(msg.rectTransform, 40, 376, 760, 124);
        msg.raycastTarget = false;
        var from = Ui.Eyebrow(msg.transform, "YOU FIX", Theme.Teal);
        Ui.Place(from.rectTransform, 24, 14, 700, 22);
        var fixes = Ui.Text(msg.transform, l.fixes, 18, Color.white);
        Ui.Place(fixes.rectTransform, 24, 38, 712, 60);
        if (!string.IsNullOrEmpty(l.mistake))
        {
            var fix = Ui.Eyebrow(msg.transform, "FIX IT  ·  " + l.mistake.ToUpperInvariant(), Look.Breached);
            fix.font = Theme.Bold;
            Ui.Place(fix.rectTransform, 24, 98, 700, 22);
        }

        // The facility, room by room: a lamp each. Held teal, a gap red, this level amber, not yet dim.
        var roomsTag = Ui.Eyebrow(b, "THE FACILITY, ROOM BY ROOM", Theme.Teal);
        Ui.Place(roomsTag.rectTransform, 40, 524, 760, 20);
        for (int r = 0; r < Facility.Rooms.Length; r++)
        {
            string name = Facility.Rooms[r];
            bool isHere = name == l.room;
            var line = app.session.record.FindAll(x => x.room == name);
            bool held = line.Count > 0 && line.TrueForAll(x => x.gap == "held"), gap = line.Count > 0 && !held;
            float x = 40 + (r % 3) * 256, y = 552 + (r / 3) * 48;
            var chip = Ui.Box(b, name, isHere ? Theme.Teal : new Color(1f, 1f, 1f, held || gap ? 0.14f : 0.05f));
            Ui.Place(chip.rectTransform, x, y, 240, 40);
            chip.raycastTarget = false;
            var lamp = Ui.Box(chip.transform, "Lamp", held ? Theme.Aqua : gap ? Look.Breached : isHere ? Theme.Ink : new Color(1f, 1f, 1f, 0.2f), false);
            Ui.Place(lamp.rectTransform, 14, 14, 12, 12);
            lamp.raycastTarget = false;
            var n = Ui.Text(chip.transform, name, 16, isHere ? Theme.Ink : held || gap ? Color.white : Theme.Faint, true, TextAlignmentOptions.MidlineLeft);
            Ui.Place(n.rectTransform, 36, 0, 200, 40);
            n.enableWordWrapping = false; n.overflowMode = TextOverflowModes.Ellipsis;
        }

        var go = Ui.Button(b, "Enter the room", app.ShowRoom, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 40, 760, 320, 64);
        var file = Ui.Button(b, "The file", () => app.ShowFile(), Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(file.GetComponent<RectTransform>(), 380, 760, 130, 64);
        var home = Ui.Button(b, "Title screen", app.ShowTitle, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 530, 760, 170, 64);

        // The ladder, over the window: the nineteen levels. Any other step jumps there.
        var strip = Ui.Glass(root, "Ladder", 920, 40, 960, 82, false);
        var ladder = Ui.Eyebrow(strip.transform, "THE LADDER", Theme.Teal);
        Ui.Place(ladder.rectTransform, 20, 10, 600, 20);
        int k = 0;
        foreach (var step in Rooms.All)
        {
            bool current = step.number == l.number, done = step.number <= app.LevelsDone;
            var card = Ui.Card(strip.transform, "Step", current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.08f), current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.3f));
            Ui.Place(card.rectTransform, 16 + k * 49, 34, 45, 38);
            var num = Ui.Text(card.transform, step.id, 16, current ? Theme.Ink : Color.white, true, TextAlignmentOptions.Center);
            Ui.Stretch(num.rectTransform);
            if (!current)
            {
                card.raycastTarget = true;
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                int number = step.number;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
            k++;
        }
        Ui.Emblem(root);

        // The warm-up question (Recall) is off: the briefing opens straight onto the level.
        // To bring it back: if (l.recall != null) Recall(app, l.recall);
    }

    // The warm-up: one question from an earlier level, over the briefing. No score.
    void Recall(App app, Recall r)
    {
        var overlay = Ui.Node("Recall", root);
        Ui.Stretch(overlay);
        var dim = Ui.Box(overlay, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.72f), false);
        Ui.Stretch(dim.rectTransform);
        dim.raycastTarget = true;
        var card = Ui.Box(overlay, "Card", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.96f));
        Ui.Place(card.rectTransform, 560, 300, 800, 440);
        var keyline = Ui.Box(card.transform, "Keyline", Theme.Teal, false);
        Ui.Place(keyline.rectTransform, 0, 16, 4, 408);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"WARM-UP  ·  FROM LEVEL {r.from}  ·  NO SCORE", Theme.Teal);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 32, 28, 600, 22);
        var q = Ui.Text(t, r.question, 26, Color.white, true);
        Ui.Place(q.rectTransform, 32, 60, 736, 80);
        var verdict = Ui.Text(t, "", 20, Color.white, true);
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
                verdict.color = right ? Theme.Teal : Theme.Danger;
                if (right) Sfx.Chime(); else Sfx.Ruin();
                for (int j = 0; j < buttons.Length; j++)
                {
                    buttons[j].interactable = false;
                    buttons[j].transform.Find("Fill").GetComponent<Image>().color = j == r.answer ? Theme.Ink2 : Theme.Ink;
                    buttons[j].GetComponent<Image>().color = j == r.answer ? Theme.Aqua : j == index ? Theme.Danger : new Color(1f, 1f, 1f, 0.2f);
                    buttons[j].GetComponentInChildren<TextMeshProUGUI>().color = j == r.answer ? Theme.Teal : j == index ? Theme.Danger : Theme.Faint;
                }
                go.gameObject.SetActive(true);
            }, Ui.ButtonStyle.GhostOnDark, 18);
            Ui.Place(buttons[i].GetComponent<RectTransform>(), 32, 150 + i * 56, 736, 48);
            var label = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.MidlineLeft;
            Ui.Stretch(label.rectTransform, 20, 0, 20, 0);
        }
    }
}
