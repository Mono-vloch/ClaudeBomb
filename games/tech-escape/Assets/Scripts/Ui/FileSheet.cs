using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The file: everything the course says about the level, on one sheet, for whoever
// wants it. The screens show the facility and one spoken line; the words live here:
// the idea, what the student fixes, the planned mistake and why it fails, the
// facility's record, the takeaway, the question for work, how a room works, and
// whatever the screen that opened it adds (the attack's full lines, the checks).
// Click anywhere off the sheet to close it.
public static class FileSheet
{
    static RectTransform open;
    public static bool IsOpen => open != null;

    public const string Room =
        "The room as it was handed over: every asset, account, message, rule or step is a card on the room's prop (on the board, in the rack, " +
        "in the sorter, at the gate), with its name on a plate, a knob for each decision and a lamp: teal when the row is set right, amber when " +
        "not. The board on the wall and the lamps at the top of the screen show the same. Tap a card to bring it close: it turns to face you, its " +
        "full name and line at the top and one row per decision with its value (\"Owner: Omar\"); tap a row to turn it, and the knob on the " +
        "board shows the value it was turned to. The room's one-off decisions are a card of their own. Rows that must be ordered are a rack: drag " +
        "a module up or down. A record that must be signed has a pad beside the board. Something not on the list yet waits in a corner of the " +
        "room: turn the camera (drag across the room, or the arrow keys), and it glints as the camera passes it; tap it and it flies to the board.\n\n" +
        "Then lift the lockdown: the facility runs the attack against whatever you built. The attacker walks the corridor door by door. A door " +
        "that gives way goes scarlet, and he comes back out with what he took and runs; a door that holds flashes teal, and he tries the handle, " +
        "gives up and walks away. What got through goes to the record the exit will ask for. The first attack on a room as it was handed over is " +
        "a rehearsal: it shows the door break and counts for nothing; the next run counts. The monitor on the room's wall shows the corridor " +
        "camera: the attacker comes to the door while you work and tries the handle until you are ready.";


    public static void Toggle(App app, Transform screen, params (string head, string body)[] extra)
    {
        if (open != null) { Close(); return; }
        if (screen == null) return;
        var l = app.room;
        var sections = new List<(string head, string body)>();
        if (l != null)
        {
            sections.Add(($"LEVEL {l.id}  ·  {l.room.ToUpperInvariant()}  ·  {l.title.ToUpperInvariant()}", l.kicker));
            sections.Add(("THE IDEA", l.learn));
            sections.Add(("YOU FIX", l.fixes));
            if (!string.IsNullOrEmpty(l.mistake)) sections.Add(("FIX IT  ·  " + l.mistake.ToUpperInvariant(), l.why));
        }
        sections.AddRange(extra);
        string record = app.session.record.Count == 0 ? "Nothing yet: the record starts in tier 2, one line per room."
                      : string.Join("\n", app.session.record.Select(r => r.gap == "held" ? $"{r.room}: held" : $"{r.room}: {r.gap}, reached {r.reached}"));
        sections.Add(("THE FACILITY'S RECORD  ·  WHAT THE EXIT WILL ASK FOR", record));
        if (l != null)
        {
            string standards = l.number == 2 ? " (The method comes from ISO/IEC 27002 and NIST CSF 2.0.)" : l.number == 13 ? " (The phases come from NIST SP 800-61.)" : l.number == 15 ? " (The register follows ISO/IEC 27005.)" : "";
            sections.Add(("THE TAKEAWAY", l.takeaway + standards));
            sections.Add(("ASK BACK AT WORK", l.ask));
        }
        sections.Add(("HOW A ROOM WORKS", Room));

        open = Ui.Node("File", screen);
        Ui.Stretch(open);
        var dim = Ui.Box(open, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.55f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(Close);
        var card = Ui.Float(open, "Sheet", 360, 80, 1200, 920);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, "THE FILE", Theme.TealDeep);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 40, 28, 400, 20);
        var title = Ui.Text(t, l != null ? l.title : "The facility", 30, Theme.Ink, true);
        Ui.Place(title.rectTransform, 40, 50, 1000, 40);
        title.enableWordWrapping = false; title.overflowMode = TextOverflowModes.Ellipsis;
        var close = Ui.Button(t, "Close", Close, Ui.ButtonStyle.Secondary, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 40 - 100, 36, 100, 40);
        var scroll = Ui.Scroll(t, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 40, 108, 1120, 780);
        float y = 0;
        foreach (var (head, body) in sections)
        {
            if (string.IsNullOrWhiteSpace(body)) continue;
            var h = Ui.Eyebrow(content, head, Theme.TealDeep);
            h.font = Theme.Bold;
            Ui.Place(h.rectTransform, 0, y, 1080, 20);
            var text = Ui.Text(content, body, 17, Theme.Ink);
            float th = text.GetPreferredValues(body, 1060, 0).y + 4;
            Ui.Place(text.rectTransform, 0, y + 26, 1060, th);
            y += 26 + th + 28;
        }
        content.sizeDelta = new Vector2(0, y + 10);
        Sfx.Tick();
    }

    public static void Close()
    {
        if (open != null) Object.Destroy(open.gameObject);
        open = null;
    }
}
