using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief: the facility turning slowly in a window, the door that gave way still
// scarlet; the attack's lines (what got through, what held), the level's checks ticked
// or missed, the medal revealed with its stars, LUMA's takeaway and the question to
// take back to work, the record so far, the next room's unlock, and on a capstone the
// two if-then plans.
public class ResultsScreen
{
    public readonly RectTransform root;
    readonly App app;
    RectTransform plans;
    static readonly Color Gold = Theme.Hex("#E0B23A"), Silver = Theme.Hex("#AAB4BE"), Bronze = Theme.Hex("#C48A5A");
    static readonly string[] MedalNames = { "", "BRONZE", "SILVER", "GOLD" };

    public ResultsScreen(Transform parent, App app)
    {
        this.app = app;
        var l = app.room;
        var a = app.lastAttack;
        bool counts = true;
        root = Ui.Node("Results", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The world behind the screen: the facility as the attack left it, turning slowly, soft.
        var window = Ui.Node("Window", root);
        Ui.Place(window, 1000, 100, 860, 300);
        app.world.Build();
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        if (a != null)
            foreach (var line in a.lines)
            {
                int index = Facility.RoomIndex(line.room);
                if (index >= 0) app.world.SetDoor(index, line.held ? Building.DoorState.Held : Building.DoorState.Breached);
            }
        if (l.Exit && a != null && !a.AnyThrough) app.world.SetExit(true);
        app.world.ShowOrbit(window);
        app.atmosphere.MenuBlur(true, 36f, 80f); // the facility (24 units off) sharp, the far end soft
        Sfx.Ambient(true);
        var scrim = Ui.Box(root, "Scrim", new Color(1f, 1f, 1f, 0.14f), false);
        Ui.Stretch(scrim.rectTransform);
        scrim.raycastTarget = false;

        var title = Ui.Text(root, $"Debrief · Level {l.id}", 44, Theme.Ink, true);
        Ui.Place(title.rectTransform, 80, 36, 900, 56);
        var sub = Ui.Text(root, $"{l.room}: {l.title}", 18, Theme.Text2);
        Ui.Place(sub.rectTransform, 80, 100, 880, 28);

        // What the attack found
        var found = Ui.Card(root, "Attack", Theme.Panel, Theme.Border1);
        Ui.Place(found.rectTransform, 80, 140, 860, 290);
        var fh = Ui.Eyebrow(found.transform, "THE ATTACK", Theme.TealDeep); Ui.Place(fh.rectTransform, 16, 10, 800, 16);
        var extras = new List<(string head, string body)>(); // the attack's full lines and the checks, for the file
        float fy = 32;
        if (a != null)
        {
            foreach (var line in a.lines.Take(6))
            {
                var lamp = Ui.Box(found.transform, "Lamp", line.held ? Theme.Aqua : Theme.Danger, false);
                Ui.Place(lamp.rectTransform, 18, fy + 5, 12, 12);
                lamp.raycastTarget = false;
                var tx = Ui.Text(found.transform, line.held ? $"<b>Held</b>  ·  {line.room}" : $"<b>Through</b>  ·  {line.room}  ·  {line.gap}", 14, line.held ? Theme.Ink : Theme.Danger);
                Ui.Place(tx.rectTransform, 42, fy, 800, 24);
                tx.enableWordWrapping = false; tx.overflowMode = TextOverflowModes.Ellipsis;
                fy += 34;
            }
            extras.Add(("THE ATTACK  ·  WHAT GOT THROUGH, WHAT HELD", string.Join("\n", a.lines.Select(x => (x.held ? "Held: " : "Through: ") + x.room + ", " + (x.held ? x.reached : x.gap + ": " + x.reached))) + "\n\n" + a.summary));
        }
        var sm = Ui.Text(found.transform, a?.summary ?? "", 15, Theme.Ink, true);
        Ui.Place(sm.rectTransform, 16, 258, 820, 26);

        // The checks
        var checks = l.check(app.state, app.session, a);
        int ticked = checks.Count(c => c.result == Practised.Yes), needed = checks.Count(c => c.result != Practised.NotNeeded);
        int medal = needed > 0 && ticked == needed ? 3 : ticked * 2 >= needed && needed > 0 ? 2 : a != null && !a.AnyThrough ? 1 : 0;
        var card = Ui.Card(root, "Checks", Theme.Panel, Theme.Border1);
        Ui.Place(card.rectTransform, 1000, 420, 860, 300);
        var ch = Ui.Eyebrow(card.transform, "THE CHECKS", Theme.TealDeep); Ui.Place(ch.rectTransform, 16, 10, 400, 16);
        float cy = 34;
        foreach (var (result, label) in checks)
        {
            // A lamp and a short label; the full label is in the file.
            var lamp = Ui.Box(card.transform, "Lamp", result == Practised.Yes ? Theme.Aqua : result == Practised.Missed ? Theme.Danger : Theme.Border, false);
            Ui.Place(lamp.rectTransform, 18, cy + 6, 12, 12);
            lamp.raycastTarget = false;
            var lab = Ui.Text(card.transform, Labels.Of(label), 16, result == Practised.NotNeeded ? Theme.Muted : Theme.Ink);
            Ui.Place(lab.rectTransform, 42, cy, 800, 26);
            lab.enableWordWrapping = false; lab.overflowMode = TextOverflowModes.Ellipsis;
            cy += checks.Count > 5 ? 38 : 46;
        }
        extras.Add(("THE CHECKS", string.Join("\n", checks.Select(c => (c.result == Practised.Yes ? "Done: " : c.result == Practised.Missed ? "Missed: " : "Not needed: ") + c.label))));

        if (counts)
        {
            app.SetStars(l.number, medal);
            string result = ticked == needed ? "complete" : ticked * 2 >= needed ? "partial" : "incomplete";
            app.StartCoroutine(Platform.ReportProgress($"{l.id}:{result}", medal, string.Join("; ", checks.Where(c => c.result != Practised.NotNeeded).Select(c => $"{c.label}: {(c.result == Practised.Yes ? "done" : "missed")}"))));
        }

        app.luma.Say("recap");
        bool unlock = counts && l.number < Rooms.Count;
        app.luma.Line(root, 80, 446, 860, l.takeaway, false, true, 17);
        Medal(medal, a, l);
        if (unlock) Unlock(736, Rooms.Get(l.number + 1), medal);

        float bx = 80;
        Button Add(string label, System.Action act, Ui.ButtonStyle style, float w)
        {
            var b = Ui.Button(root, label, act, style, 17);
            Ui.Place(b.GetComponent<RectTransform>(), bx, 990, w, 56);
            bx += w + 14;
            return b;
        }
        {
            bool last = l.number >= Rooms.Count;
            Add(last ? "The exit: title screen" : $"Next: {Rooms.Get(l.number + 1).id} {Rooms.Get(l.number + 1).title.Replace("Capstone: ", "")}", last ? app.ShowTitle : (System.Action)app.NextLevel, Ui.ButtonStyle.Primary, last ? 260 : 520);
            Add("This room again", app.ShowRoom, Ui.ButtonStyle.Secondary, 200);
            if (l.Capstone) Add("If-then plans", ShowPlans, Ui.ButtonStyle.Secondary, 180);
        }
        Add("The file", () => app.ShowFile(extras.ToArray()), Ui.ButtonStyle.Secondary, 130);
        Add("Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 160);
        if (Platform.IsWeb) Add("Games menu", Platform.GoToMenu, Ui.ButtonStyle.Quiet, 160);
        Ui.Emblem(root);
    }

    // The medal, revealed over the turning facility: the badge comes in with an overshoot,
    // the stars light one by one, confetti over the exit and a fanfare for silver and gold.
    void Medal(int medal, Attack a, Room l)
    {
        var cap = Ui.Glass(root, "Window caption", 1000, 100, 860, 40);
        int through = a?.lines.Count(x => !x.held) ?? 0, held = a?.lines.Count(x => x.held) ?? 0;
        var caption = Ui.Eyebrow(cap.transform, $"{l.room.ToUpperInvariant()}  ·  THROUGH {through}  ·  HELD {held}", Theme.Teal);
        Ui.Place(caption.rectTransform, 20, 12, 820, 18);
        Color colour = medal == 3 ? Gold : medal == 2 ? Silver : medal == 1 ? Bronze : Theme.Ink;
        var badge = Ui.Glass(root, "Medal", 1230, 215, 400, 150, true, colour);
        var b = badge.transform;
        var tag = Ui.Eyebrow(b, "THE MEDAL", Theme.Teal);
        Ui.Place(tag.rectTransform, 0, 14, 400, 18); tag.alignment = TextAlignmentOptions.Top;
        var name = Ui.Text(b, medal > 0 ? MedalNames[medal] : "NO MEDAL YET", medal > 0 ? 44 : 28, medal > 0 ? colour : Look.Chalk, true, TextAlignmentOptions.Center);
        name.characterSpacing = 6;
        Ui.Place(name.rectTransform, 0, 34, 400, 60);
        var stars = new Image[3];
        for (int s = 0; s < 3; s++)
        {
            stars[s] = Ui.Box(b, "Star", s < medal ? colour : new Color(1, 1, 1, 0.16f));
            Ui.Place(stars[s].rectTransform, 155 + s * 32, 98, 26, 26);
            stars[s].raycastTarget = false;
        }
        var rank = Ui.Text(b, $"{medal} of 3 stars  ·  {app.TotalStars} in all  ·  {app.Rank}", 13, Look.Chalk, false, TextAlignmentOptions.Center);
        Ui.Place(rank.rectTransform, 0, 122, 400, 20);
        app.StartCoroutine(Reveal(badge, stars, medal, colour));
    }

    System.Collections.IEnumerator Reveal(Image badge, Image[] stars, int medal, Color colour)
    {
        var group = Ui.Group(badge);
        group.alpha = 0f;
        badge.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        badge.rectTransform.anchoredPosition = new Vector2(1230 + 200, -(215 + 75));
        foreach (var s in stars) s.transform.localScale = Vector3.zero;
        yield return new WaitForSecondsRealtime(0.55f);
        if (badge == null) yield break;
        if (medal > 0) Sfx.Sting(Sfx.Mood.Fanfare); else Sfx.Tick();
        yield return Ui.Animate(0.45f, k =>
        {
            if (badge == null) return;
            group.alpha = Mathf.Min(1f, k * 3f);
            badge.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, Ui.EaseOutBack(k));
        });
        for (int s = 0; s < medal; s++)
        {
            int i = s;
            Sfx.Chime();
            yield return Ui.Animate(0.22f, k => { if (stars[i] != null) stars[i].transform.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, Ui.EaseOutBack(k)); });
            if (medal >= 2 && app.world.gameObject.activeSelf) app.effects.Confetti(app.world.Bounds.center + Vector3.up * 1.5f, 30);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        for (int s = medal; s < 3; s++) if (stars[s] != null) stars[s].transform.localScale = Vector3.one;
    }

    void Unlock(float y, Room next, int stars)
    {
        var card = Ui.Card(root, "Unlock", Theme.TealDeep, Theme.TealDeep);
        Ui.Place(card.rectTransform, 1000, y, 860, 100);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"UNLOCKED  ·  LEVEL {next.id}", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 20, 12, 600, 20);
        var body = Ui.Text(t, $"{next.room}: {next.title}", 18, Color.white, true);
        Ui.Place(body.rectTransform, 20, 36, 820, 30);
        body.enableWordWrapping = false; body.overflowMode = TextOverflowModes.Ellipsis;
        var starsLine = Ui.Text(t, $"{stars} of 3 stars banked  ·  {app.TotalStars} in all  ·  {app.Rank}", 13, Theme.Teal);
        Ui.Place(starsLine.rectTransform, 20, 68, 820, 24);
    }

    void ShowPlans()
    {
        if (plans != null) { Object.Destroy(plans.gameObject); plans = null; return; }
        plans = Ui.Node("Plans", root);
        Ui.Stretch(plans);
        var dim = Ui.Box(plans, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.45f), false);
        Ui.Stretch(dim.rectTransform);
        var card = Ui.Float(plans, "Card", 360, 200, 1200, 680);
        var t = card.transform;
        var title = Ui.Text(t, "Your two if-then plans", 26, Theme.Ink, true);
        Ui.Place(title.rectTransform, 32, 26, 800, 36);
        var sub = Ui.Text(t, "Two plans about your own work, to look at again at the follow-up session. Write, save.", 15, Theme.Text2);
        Ui.Place(sub.rectTransform, 32, 62, 1000, 24);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(plans.gameObject); plans = null; }, Ui.ButtonStyle.Secondary, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 32 - 100, 26, 100, 40);
        string key = $"tech_escape_plans_{app.room.id}";
        var saved = PlayerPrefs.GetString(key, "\n").Split('\n');
        var inputs = new TMP_InputField[2];
        for (int i = 0; i < 2; i++)
        {
            var label = Ui.Eyebrow(t, i == 0 ? "PLAN 1  ·  IF ..., THEN I ..." : "PLAN 2  ·  IF ..., THEN I ...", Theme.TealDeep);
            Ui.Place(label.rectTransform, 32, 110 + i * 220, 600, 20);
            inputs[i] = Ui.Input(t, "If a message at work asks me to sign in somewhere, then I ...", true, 18);
            Ui.Place(inputs[i].GetComponent<RectTransform>(), 32, 136 + i * 220, 1136, 160);
            inputs[i].text = i < saved.Length ? saved[i] : "";
        }
        var save = Ui.Button(t, "Save", () => { PlayerPrefs.SetString(key, inputs[0].text.Replace("\n", " ") + "\n" + inputs[1].text.Replace("\n", " ")); PlayerPrefs.Save(); Sfx.Buy(); }, Ui.ButtonStyle.Primary, 16);
        Ui.Place(save.GetComponent<RectTransform>(), 32, 600, 140, 46);
    }
}
