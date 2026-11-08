using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief is a moment, then a door: the corridor outside the room, its door shut and teal
// (or hanging open and scarlet), the attacker turning away, a podium before it with the medal
// rising on its step and its stars, LUMA's one line, Next and a quiet Again. The attack's lines,
// the checks, the stars in all and the rank, the question for work and the capstone's plans sit
// behind Details, closed by default. The stars banked and the report to the platform are as
// they were.
public class ResultsScreen
{
    public static ResultsScreen Current;
    public readonly RectTransform root;
    readonly App app;
    RectTransform sheet, plans;
    int medal;
    List<(Practised result, string label)> checks;
    readonly List<(string head, string body)> extras = new List<(string head, string body)>();
    static readonly Color Gold = Theme.Hex("#E0B23A"), Silver = Theme.Hex("#AAB4BE"), Bronze = Theme.Hex("#C48A5A");

    public ResultsScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        var l = app.room;
        var a = app.lastAttack;
        root = Ui.Node("Results", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The checks, and the medal they earn.
        checks = l.check(app.state, app.session, a);
        int ticked = checks.Count(c => c.result == Practised.Yes), needed = checks.Count(c => c.result != Practised.NotNeeded);
        medal = needed > 0 && ticked == needed ? 3 : ticked * 2 >= needed && needed > 0 ? 2 : a != null && !a.AnyThrough ? 1 : 0;
        app.SetStars(l.number, medal);
        string result = ticked == needed ? "complete" : ticked * 2 >= needed ? "partial" : "incomplete";
        app.StartCoroutine(Platform.ReportProgress($"{l.id}:{result}", medal, string.Join("; ", checks.Where(c => c.result != Practised.NotNeeded).Select(c => $"{c.label}: {(c.result == Practised.Yes ? "done" : "missed")}"))));
        if (a != null) extras.Add(("THE ATTACK  ·  WHAT GOT THROUGH, WHAT HELD", string.Join("\n", a.lines.Select(x => (x.held ? "Held: " : "Through: ") + x.room + ", " + (x.held ? x.reached : x.gap + ": " + x.reached))) + "\n\n" + a.summary));
        extras.Add(("THE CHECKS", string.Join("\n", checks.Select(c => (c.result == Practised.Yes ? "Done: " : c.result == Practised.Missed ? "Missed: " : "Not needed: ") + c.label))));

        // The world: the corridor outside this room's door as the attack left it.
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        app.world.Build();
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        int here = Facility.RoomIndex(l.room);
        bool through = false;
        if (a != null)
            foreach (var line in a.lines)
            {
                int index = Facility.RoomIndex(line.room);
                if (index >= 0) app.world.SetDoor(index, line.held ? Building.DoorState.Held : Building.DoorState.Breached);
                if (index == here && !line.held) through = true;
            }
        if (a != null && a.AnyThrough && a.lines.All(x => Facility.RoomIndex(x.room) != here)) through = true;
        if (l.Exit && a != null && !a.AnyThrough) app.world.SetExit(true);
        Color colour = medal == 3 ? Gold : medal == 2 ? Silver : medal == 1 ? Bronze : Theme.Faint;
        app.world.ShowDebrief(frame, here, through, medal, colour);
        app.atmosphere.MenuBlur(false);
        Sfx.Ambient(true);
        GameMenu.Icon(root);
        app.StartCoroutine(Reveal());
        app.StartCoroutine(SayLater("take-" + l.number, 2.2f));

        // The door: Next, a quiet Again, and Details.
        bool last = l.number >= Rooms.Count;
        var next = Ui.Button(root, last ? "Title screen" : "Next", last ? app.ShowTitle : (System.Action)app.NextLevel, Ui.ButtonStyle.Accent, 22);
        Ui.Place(next.GetComponent<RectTransform>(), Ui.W - 40 - 240, 996, 240, 64);
        var again = Ui.Button(root, "Again", app.ShowRoom, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(again.GetComponent<RectTransform>(), Ui.W - 40 - 240 - 14 - 140, 1004, 140, 56);
        var details = Ui.Button(root, "Details", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(details.GetComponent<RectTransform>(), 40, 1004, 140, 56);
    }

    IEnumerator Reveal()
    {
        yield return new WaitForSecondsRealtime(0.55f);
        if (root == null) yield break;
        if (medal > 0) Sfx.Sting(Sfx.Mood.Fanfare); else Sfx.Tick();
        yield return app.world.RaisePodium(medal);
    }

    IEnumerator SayLater(string id, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (root != null) app.luma.Cue(root, id, 40, 892, 660, stay: true); // bottom left, narrow: clear of the podium and the door's sign
    }

    // ---- Details: the attack's lines, the checks, the stars, the takeaway, behind one button ----

    void ToggleSheet()
    {
        if (sheet != null) { Object.Destroy(sheet.gameObject); sheet = null; return; }
        var l = app.room;
        var a = app.lastAttack;
        const float W = 760f, DW = W - 56;
        sheet = Ui.Node("Details", root);
        Ui.Place(sheet, Ui.W - W, 0, W, Ui.H);
        var bg = Ui.Box(sheet, "Glass", Theme.Ink, false); // opaque: nothing behind shows through
        bg.raycastTarget = true;
        Ui.Stretch(bg.rectTransform);
        var keyline = Ui.Box(sheet, "Keyline", Theme.Teal, false);
        Ui.Place(keyline.rectTransform, 0, 0, 3, Ui.H);
        var tag = Ui.Eyebrow(sheet, $"DETAILS  ·  LEVEL {l.id}  ·  {l.room.ToUpperInvariant()}", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 28, 26, 600, 22);
        var close = Ui.Button(sheet, "Close", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), W - 112 - 76, 24, 88, 44); // clear of the menu icon
        var scroll = Ui.Scroll(sheet, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 20, 70, W - 40, Ui.H - 90);
        float y = 0;
        if (a != null)
        {
            var card = Ui.DarkCard(content, "Attack");
            float h = 40 + a.lines.Count * 30 + 40;
            Ui.Place(card.rectTransform, 0, y, W - 40, h);
            var ah = Ui.Eyebrow(card.transform, "THE ATTACK  ·  WHAT IT TRIED", Theme.Teal); Ui.Place(ah.rectTransform, 16, 10, 500, 22);
            float ay = 40;
            foreach (var line in a.lines)
            {
                var lamp = Ui.Box(card.transform, "Lamp", line.held ? Theme.Aqua : Theme.Danger, false);
                Ui.Place(lamp.rectTransform, 18, ay + 6, 12, 12); lamp.raycastTarget = false;
                var tx = Ui.Text(card.transform, line.held ? $"<b>Held</b>  ·  {line.room}" : $"<b>Through</b>  ·  {line.room}  ·  {line.gap}", 16, line.held ? Color.white : Look.Breached);
                Ui.Place(tx.rectTransform, 42, ay, W - 100, 24);
                tx.enableWordWrapping = false; tx.overflowMode = TextOverflowModes.Ellipsis;
                ay += 30;
            }
            var sm = Ui.Text(card.transform, a.summary, 16, Color.white, true);
            Ui.Place(sm.rectTransform, 16, ay + 6, W - 72, 26);
            y += h + 12;
        }
        {
            var card = Ui.DarkCard(content, "Checks");
            float h = 40 + checks.Count * 30 + 10;
            Ui.Place(card.rectTransform, 0, y, W - 40, h);
            var ch = Ui.Eyebrow(card.transform, "THE CHECKS", Theme.Teal); Ui.Place(ch.rectTransform, 16, 10, 400, 22);
            float cy = 40;
            foreach (var (result, label) in checks)
            {
                var lamp = Ui.Box(card.transform, "Lamp", result == Practised.Yes ? Theme.Aqua : result == Practised.Missed ? Theme.Danger : Theme.Border, false);
                Ui.Place(lamp.rectTransform, 18, cy + 6, 12, 12); lamp.raycastTarget = false;
                var lab = Ui.Text(card.transform, Labels.Of(label), 16, result == Practised.NotNeeded ? Theme.Faint : Color.white);
                Ui.Place(lab.rectTransform, 42, cy, W - 100, 24);
                lab.enableWordWrapping = false; lab.overflowMode = TextOverflowModes.Ellipsis;
                cy += 30;
            }
            y += h + 12;
        }
        var stars = Ui.Text(content, $"{medal} of 3 stars here  ·  {app.TotalStars} stars in all  ·  rank: {app.Rank}", 16, Theme.Teal, true);
        Ui.Place(stars.rectTransform, 8, y, DW, 24); y += 34;
        // The takeaway is LUMA's line on this screen: not repeated here. The question for work stays.
        var ask = Ui.Text(content, l.ask, 17, Look.Chalk);
        float akh = ask.GetPreferredValues(l.ask, DW, 0).y + 4;
        Ui.Place(ask.rectTransform, 8, y, DW, akh); y += akh + 16;
        if (l.number < Rooms.Count)
        {
            var nextL = Rooms.Get(l.number + 1);
            var unlock = Ui.Text(content, $"Next: {nextL.id} {nextL.room}: {nextL.title.Replace("Capstone: ", "")}", 16, Theme.Teal, true);
            Ui.Place(unlock.rectTransform, 8, y, DW, 24); y += 34;
        }
        if (l.Capstone)
        {
            var pl = Ui.Button(content, "If-then plans", ShowPlans, Ui.ButtonStyle.GhostOnDark, 16);
            Ui.Place(pl.GetComponent<RectTransform>(), 8, y, 200, 46); y += 56;
        }
        var file = Ui.Button(content, "The file", () => app.ShowFile(extras.ToArray()), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(file.GetComponent<RectTransform>(), 8, y, 140, 46); y += 56;
        content.sizeDelta = new Vector2(0, y + 20);
    }

    // The capstone's two if-then plans, about the student's own work.
    void ShowPlans()
    {
        if (plans != null) { Object.Destroy(plans.gameObject); plans = null; return; }
        plans = Ui.Node("Plans", root);
        Ui.Stretch(plans);
        var dim = Ui.Box(plans, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.45f), false);
        Ui.Stretch(dim.rectTransform);
        var card = Ui.Box(plans, "Card", Ui.DarkFill);
        Ui.Place(card.rectTransform, 360, 200, 1200, 680);
        var t = card.transform;
        var title = Ui.Text(t, "Your two if-then plans", 26, Color.white, true);
        Ui.Place(title.rectTransform, 32, 26, 800, 36);
        var sub = Ui.Text(t, "Two plans about your own work, to look at again at the follow-up session. Write, save.", 16, Theme.Faint);
        Ui.Place(sub.rectTransform, 32, 62, 1000, 24);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(plans.gameObject); plans = null; }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 32 - 100, 26, 100, 40);
        string key = $"tech_escape_plans_{app.room.id}";
        var saved = PlayerPrefs.GetString(key, "\n").Split('\n');
        var inputs = new TMP_InputField[2];
        for (int i = 0; i < 2; i++)
        {
            var label = Ui.Eyebrow(t, i == 0 ? "PLAN 1  ·  IF ..., THEN I ..." : "PLAN 2  ·  IF ..., THEN I ...", Theme.Teal);
            Ui.Place(label.rectTransform, 32, 110 + i * 220, 600, 20);
            inputs[i] = Ui.DarkInput(t, "If a message at work asks me to sign in somewhere, then I ...", true, 18);
            Ui.Place(inputs[i].GetComponent<RectTransform>(), 32, 136 + i * 220, 1136, 160);
            inputs[i].text = i < saved.Length ? saved[i] : "";
        }
        var save = Ui.Button(t, "Save", () => { PlayerPrefs.SetString(key, inputs[0].text.Replace("\n", " ") + "\n" + inputs[1].text.Replace("\n", " ")); PlayerPrefs.Save(); Sfx.Buy(); }, Ui.ButtonStyle.Primary, 16);
        Ui.Place(save.GetComponent<RectTransform>(), 32, 600, 140, 46);
    }
}
