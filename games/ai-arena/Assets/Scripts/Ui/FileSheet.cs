using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The file: everything the course says about the errand, on one sheet, for whoever
// wants it. The screens show the market and one spoken line; the words live here:
// the idea, the message and its catch, the training runs' reasons, the takeaway, the
// question for the agent's manager, how the garage works, and whatever the screen
// that opened it adds (the debrief's full checks and stories). Click anywhere off
// the sheet to close it.
public static class FileSheet
{
    static RectTransform open;

    public const string Garage =
        "The shelf: tap a thing once to buy it, twice to never buy it, again to clear it. The rule for an empty view says what the agent does when nothing " +
        "on its list is in sight. Your own words carry the catch and anything else. What the agent will read is shown underneath, word for word: nothing " +
        "else reaches it.\n\n" +
        "The dials hang off the agent as tags: tap one to turn it. Brain: which Claude runs it. Bigger: better choices, pricier tokens. How far it sees: cells in every direction. More in view, more words every turn. " +
        "Memory: each turn starts from nothing, or the last 3 or 10 exchanges ride along in the prompt, or the agent writes itself a note it sees every turn. " +
        "Effort: how hard it thinks at each stall. More: better, slower, pricier. A dial that is locked opens at a later errand; until then every agent gets " +
        "the same, so only this errand's idea is in play.\n\n" +
        "Pocket money: what one turn costs is the list, plus the view, plus the memory riding along, plus the answer. The gauge shows how many turns the " +
        "money buys. Real usage comes back from Claude and will differ.";

    public static void Toggle(App app, Transform screen, params (string head, string body)[] extra)
    {
        if (open != null) { Close(); return; }
        if (screen == null) return;
        var l = app.lesson;
        var b = app.brief;
        var sections = new List<(string head, string body)>();
        if (l != null)
        {
            sections.Add(($"ERRAND {l.id}  ·  {l.title.ToUpperInvariant()}", l.kicker));
            sections.Add(("THE IDEA", l.learn));
        }
        if (b != null && !(l != null && l.decide)) sections.Add(($"THE MESSAGE FROM {b.from.ToUpperInvariant()}", b.rule + "\n\nThe catch: " + b.note));
        if (l?.ready != null) sections.Add(("WATCH THE PRO", l.ready.why));
        if (l?.broken != null) sections.Add(("FIX THE BROKEN BOT  ·  " + l.broken.name.ToUpperInvariant(), l.broken.why));
        sections.AddRange(extra);
        if (l != null)
        {
            sections.Add(("THE TAKEAWAY", l.takeaway + (l.number == Ladder.Count ? " " + Ladder.FiveWays : "")));
            sections.Add(("ASK AS THE AGENT'S MANAGER", l.ask));
        }
        sections.Add(("HOW THE GARAGE WORKS", Garage));

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
        var title = Ui.Text(t, l != null ? l.title : "Free play", 30, Theme.Ink, true);
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
