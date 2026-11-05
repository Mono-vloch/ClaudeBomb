using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief: the attack's lines (what got through, what held), the level's
// checks ticked or missed, a medal, LUMA's takeaway and the question to take back
// to work, the record so far, the next room's unlock, and on a capstone the two
// if-then plans.
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
        bool training = app.Training, counts = !training;
        root = Ui.Node("Results", parent);
        Ui.Stretch(root);
        app.Paper(Theme.Page);

        var title = Ui.Text(root, $"Debrief · Level {l.id} · {app.StepName}", 44, Theme.Ink, true);
        Ui.Place(title.rectTransform, 80, 40, 1300, 56);
        var sub = Ui.Text(root, $"{l.room}: {l.title}", 18, Theme.Text2);
        Ui.Place(sub.rectTransform, 80, 100, 1600, 28);

        // What the attack found
        var found = Ui.Card(root, "Attack", Theme.Panel, Theme.Border1);
        Ui.Place(found.rectTransform, 80, 140, 860, 330);
        var fh = Ui.Eyebrow(found.transform, "THE ATTACK  ·  WHAT GOT THROUGH, WHAT HELD", Theme.TealDeep); Ui.Place(fh.rectTransform, 16, 10, 800, 16);
        float fy = 32;
        if (a != null)
            foreach (var line in a.lines.Take(7))
            {
                var w = Ui.Text(found.transform, line.held ? "Held" : "Through", 13, line.held ? Theme.TealDeep : Theme.Danger, true);
                Ui.Place(w.rectTransform, 16, fy + 1, 70, 20);
                var tx = Ui.Text(found.transform, (line.held ? line.reached : line.gap + ": " + line.reached), 13, Theme.Ink);
                Ui.Place(tx.rectTransform, 90, fy, 750, 40);
                tx.overflowMode = TextOverflowModes.Ellipsis;
                fy += 40;
            }
        var sm = Ui.Text(found.transform, a?.summary ?? "", 14, Theme.Ink, true);
        Ui.Place(sm.rectTransform, 16, 300, 820, 24);

        // The checks
        var checks = l.check(app.state, app.session, a);
        int ticked = checks.Count(c => c.result == Practised.Yes), needed = checks.Count(c => c.result != Practised.NotNeeded);
        int medal = needed > 0 && ticked == needed ? 3 : ticked * 2 >= needed && needed > 0 ? 2 : a != null && !a.AnyThrough ? 1 : 0;
        var card = Ui.Card(root, "Checks", Theme.Panel, Theme.Border1);
        Ui.Place(card.rectTransform, 960, 140, 880, 330);
        var ch = Ui.Eyebrow(card.transform, "THE CHECKS  ·  WHAT YOU DID", Theme.TealDeep); Ui.Place(ch.rectTransform, 16, 10, 400, 16);
        if (medal > 0)
        {
            var pill = Ui.Box(card.transform, "Medal", medal == 3 ? Gold : medal == 2 ? Silver : Bronze);
            Ui.Place(pill.rectTransform, 780, 8, 84, 24);
            var mt = Ui.Text(pill.transform, MedalNames[medal], 11, Color.white, true, TextAlignmentOptions.Center); mt.characterSpacing = 3; Ui.Stretch(mt.rectTransform);
        }
        float cy = 34;
        foreach (var (result, label) in checks)
        {
            var word = Ui.Text(card.transform, result == Practised.Yes ? "Done" : result == Practised.Missed ? "Missed" : "n/a", 14, result == Practised.Yes ? Theme.TealDeep : result == Practised.Missed ? Theme.Danger : Theme.Muted, true);
            Ui.Place(word.rectTransform, 16, cy + 1, 60, 22);
            var lab = Ui.Text(card.transform, label, 15, Theme.Ink);
            Ui.Place(lab.rectTransform, 84, cy, 780, 44);
            cy += checks.Count > 5 ? 44 : 52;
        }

        if (counts)
        {
            app.SetStars(l.number, medal);
            string result = ticked == needed ? "complete" : ticked * 2 >= needed ? "partial" : "incomplete";
            app.StartCoroutine(Platform.ReportProgress($"{l.id}:{result}", medal, string.Join("; ", checks.Where(c => c.result != Practised.NotNeeded).Select(c => $"{c.label}: {(c.result == Practised.Yes ? "done" : "missed")}"))));
        }

        // The record so far: the spine the exit will ask for.
        var rec = Ui.Card(root, "Record", Theme.Surface1, Theme.Border);
        Ui.Place(rec.rectTransform, 80, 486, 860, 150);
        var rh = Ui.Eyebrow(rec.transform, "THE FACILITY'S RECORD  ·  WHAT THE EXIT WILL ASK FOR", Theme.Muted); Ui.Place(rh.rectTransform, 16, 10, 800, 16);
        string record = app.session.record.Count == 0 ? "Nothing yet: the record starts in tier 2, one line per room."
                      : string.Join("\n", app.session.record.Take(5).Select(r => r.gap == "held" ? $"{r.room}: held" : $"{r.room}: {r.gap}, reached {r.reached}"));
        var rt = Ui.Text(rec.transform, record, 13, Theme.Text2);
        Ui.Place(rt.rectTransform, 16, 30, 828, 116);
        rt.overflowMode = TextOverflowModes.Ellipsis;

        app.luma.Say("recap");
        bool unlock = counts && l.number < Rooms.Count;
        string standards = l.number == 2 ? " (The method comes from ISO/IEC 27002 and NIST CSF 2.0.)" : l.number == 13 ? " (The phases come from NIST SP 800-61.)" : l.number == 15 ? " (The register follows ISO/IEC 27005.)" : "";
        Luma.Card(root, 960, 486, unlock ? 460 : 880, 150, l.takeaway + standards, 15, "Ask back at work: " + l.ask);
        if (unlock) Unlock(486, Rooms.Get(l.number + 1), medal);

        float bx = 80;
        Button Add(string label, System.Action act, Ui.ButtonStyle style, float w)
        {
            var b = Ui.Button(root, label, act, style, 17);
            Ui.Place(b.GetComponent<RectTransform>(), bx, 990, w, 56);
            bx += w + 14;
            return b;
        }
        if (training && app.step == App.Step.SeeItWork)
        {
            if (l.broken.Count > 0) Add("Next: Fix the broken room", () => { app.SetStep(App.Step.FixIt); app.ShowRoom(); }, Ui.ButtonStyle.Primary, 300);
            Add("Next: Your run", () => { app.SetStep(App.Step.YourTurn); app.ShowRoom(); }, l.broken.Count > 0 ? Ui.ButtonStyle.Secondary : Ui.ButtonStyle.Primary, 200);
        }
        else if (training)
        {
            Add("Fix it again", app.ShowRoom, Ui.ButtonStyle.Primary, 180);
            Add("Next: Your run", () => { app.SetStep(App.Step.YourTurn); app.ShowRoom(); }, Ui.ButtonStyle.Secondary, 200);
        }
        else
        {
            bool last = l.number >= Rooms.Count;
            Add(last ? "The exit: title screen" : $"Next: {Rooms.Get(l.number + 1).id} {Rooms.Get(l.number + 1).title.Replace("Capstone: ", "")}", last ? app.ShowTitle : (System.Action)app.NextLevel, Ui.ButtonStyle.Primary, last ? 260 : 520);
            Add("This room again", app.ShowRoom, Ui.ButtonStyle.Secondary, 200);
            if (l.Capstone) Add("If-then plans", ShowPlans, Ui.ButtonStyle.Secondary, 180);
        }
        Add("Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 160);
        if (Platform.IsWeb) Add("Games menu", Platform.GoToMenu, Ui.ButtonStyle.Quiet, 160);
        Ui.Emblem(root);
    }

    void Unlock(float y, Room next, int stars)
    {
        var card = Ui.Card(root, "Unlock", Theme.TealDeep, Theme.TealDeep);
        Ui.Place(card.rectTransform, 1420, y, 420, 150);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"UNLOCKED  ·  LEVEL {next.id}", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 20, 14, 380, 20);
        var body = Ui.Text(t, $"{next.room}: {next.title}", 17, Color.white, true);
        Ui.Place(body.rectTransform, 20, 38, 380, 70);
        var starsLine = Ui.Text(t, $"{stars} of 3 stars banked  ·  {app.TotalStars} in all  ·  {app.Rank}", 13, Theme.Teal);
        Ui.Place(starsLine.rectTransform, 20, 114, 380, 24);
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
