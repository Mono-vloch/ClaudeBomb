using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The file: everything the course says about the errand, on one dark sheet, for whoever
// wants it. The screens show the market and one spoken line; the words live here: the idea,
// the message and its catch, the ready and the broken lists and their reasons, LUMA's full
// lines, the takeaway, the question for the agent's manager, the story, how the garage works,
// and whatever the screen that opened it adds. Click anywhere off the sheet to close it.
public static class FileSheet
{
    static RectTransform open;
    public static bool IsOpen => open != null;

    public const string Garage =
        "The table: drag a thing into the agent's basket to put \"Buy it\" on its note, into the No crate for \"Never buy it\", back onto the table to take it off. " +
        "Tap the note to write the list yourself, word for word: the catch and the rule for when nothing on the list is in view are yours to write. " +
        "What the note says is what the agent reads; nothing else reaches it. The phone holds the family's message; the agent never sees it.\n\n" +
        "The coins are its pocket money, in tokens: tap one into the battery on its hip, tap the battery to take one back. The battery's ring shows how many turns " +
        "of the errand the money buys at this list's cost: the list, the view, the memory riding along and the answer all cost tokens, every turn.\n\n" +
        "From errand 1.3 the parts are on the table. A lens: how far it sees, in cells, in every direction; more in view, more words every turn. " +
        "Memory: a stack of three or ten turns that ride along in the prompt, or a notebook it writes itself. A bulb: which Claude runs it (Haiku, Sonnet, Opus); " +
        "bigger is better and pricier per token. The dimmer at the antenna: how hard it thinks at each stall. Tap a part on the table to fit it; tap the memory on the agent to take it off.";

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
            sections.Add(("LUMA", l.luma + "\n\n" + l.live));
        }
        if (b != null && !(l != null && l.decide)) sections.Add(($"THE MESSAGE FROM {b.from.ToUpperInvariant()}", b.rule + "\n\nThe catch: " + b.note));
        if (l != null && l.decide)
            sections.Add(("THE FIVE TASKS", string.Join("\n", Decide.Tasks.Select((t, i) => $"{i + 1}. {t.from}: {t.message}  ({t.note})"))));
        if (l?.ready != null) sections.Add(("THE READY LIST", l.ready.Fill(b) + "\n\n" + l.ready.why));
        if (l?.broken != null) sections.Add(("THE BROKEN LIST  ·  " + l.broken.name.ToUpperInvariant(), l.broken.Fill(b) + "\n\n" + l.broken.why));
        sections.AddRange(extra);
        if (l != null)
        {
            sections.Add(("THE TAKEAWAY", l.takeaway + (l.number == Ladder.Count ? " " + Ladder.FiveWays : "")));
            sections.Add(("ASK AS THE AGENT'S MANAGER", l.ask));
        }
        sections.Add(("HOW TO PLAY", Garage));
        sections.Add(("THE STORY", string.Join("\n\n", Enumerable.Range(1, 6).Select(i => Luma.Lines["story-" + i]))));

        open = Ui.Node("File", screen);
        Ui.Stretch(open);
        var dim = Ui.Box(open, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.55f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(Close);
        var card = Ui.Box(open, "Sheet", Ui.DarkFill);
        Ui.Place(card.rectTransform, 360, 80, 1200, 920);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, "THE FILE", Theme.Teal);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 40, 28, 400, 20);
        var title = Ui.Text(t, l != null ? l.title : "Free play", 30, Color.white, true);
        Ui.Place(title.rectTransform, 40, 50, 1000, 40);
        title.enableWordWrapping = false; title.overflowMode = TextOverflowModes.Ellipsis;
        var close = Ui.Button(t, "Close", Close, Ui.ButtonStyle.GhostOnDark, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 40 - 100, 36, 100, 40);
        var scroll = Ui.Scroll(t, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 40, 108, 1120, 780);
        float y = 0;
        foreach (var (head, body) in sections)
        {
            if (string.IsNullOrWhiteSpace(body)) continue;
            var h = Ui.Eyebrow(content, head, Theme.Teal);
            h.font = Theme.Bold;
            Ui.Place(h.rectTransform, 0, y, 1080, 20);
            var text = Ui.Text(content, body, 17, Color.white);
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
