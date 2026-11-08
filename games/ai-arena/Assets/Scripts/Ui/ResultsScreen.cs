using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief is a moment, then a door (the game-feel pass): the agent at the front door at
// dusk with its basket, the family's window lit, the medal rising on a podium beside it with
// its stars, LUMA's one line, Next and a quiet Again. The checks, the agents' figures, the
// costs, the lists, the stars in all and the rank sit behind Details, closed by default. The
// report to the platform and the stars banked are as they were.
public class ResultsScreen
{
    public static ResultsScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Arena arena;
    readonly Simulation sim;
    RectTransform sheet, plans;
    readonly List<(AgentState a, int medal, List<(Practised result, string label)> checks)> rows = new List<(AgentState, int, List<(Practised, string)>)>();
    readonly List<(string head, string body)> extras = new List<(string head, string body)>();
    int bestMedal;

    static readonly Color Gold = Theme.Hex("#E0B23A"), Silver = Theme.Hex("#AAB4BE"), Bronze = Theme.Hex("#C48A5A");
    public static readonly string[] MedalNames = { "", "BRONZE", "SILVER", "GOLD" };

    public ResultsScreen(Transform parent, App app, Arena arena, Simulation sim)
    {
        Current = this;
        this.app = app; this.arena = arena; this.sim = sim;
        var lesson = app.lesson;
        bool capstone = lesson != null && lesson.number == Ladder.Count, counts = lesson != null;
        root = Ui.Node("Results", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        var ranked = arena.agents.OrderByDescending(a => a.alive ? 1 : 0).ThenByDescending(a => a.score).ThenByDescending(a => a.eliminatedTurn).ToList();
        foreach (var a in ranked)
        {
            var list = lesson != null ? lesson.Check(a, arena) : null;
            int medal = Medal(a, list);
            bestMedal = Mathf.Max(bestMedal, medal);
            rows.Add((a, medal, list));
            // The platform keeps the result (web only): the errand's checks for each agent, once per level that counts.
            if (counts)
            {
                int ticked = list.Count(c => c.result == Practised.Yes), needed = list.Count(c => c.result != Practised.NotNeeded);
                string result = ticked == needed ? "complete" : ticked * 2 >= needed ? "partial" : "incomplete";
                string detail = $"{a.Name}: " + string.Join("; ", list.Where(c => c.result != Practised.NotNeeded).Select(c => $"{c.label}: {(c.result == Practised.Yes ? "done" : "missed")}"));
                app.StartCoroutine(Platform.ReportProgress($"{lesson.id}:{result}", medal, detail));
            }
            string full = Verdicts.Of(a, arena, sim);
            string partFull = capstone ? Verdicts.Part(a) : null;
            if (partFull != null) full += "\nWhich part did it? " + partFull;
            string checksFull = list == null ? "" : string.Join("\n", list.Select(c => (c.result == Practised.Yes ? "Done: " : c.result == Practised.Missed ? "Missed: " : "Not needed: ") + c.label));
            extras.Add((a.Name.ToUpperInvariant(), full + (checksFull == "" ? "" : "\n\n" + checksFull)));
        }
        if (lesson == null) app.StartCoroutine(Platform.ReportProgress("free:finished", 0, ranked.Count > 0 ? $"{ranked[0].Name} first" : null));
        if (counts) app.SetStars(lesson.number, bestMedal);
        app.lastMedal = lesson != null ? bestMedal : -1;
        app.lastMedalLevel = lesson != null ? lesson.number : 0;

        // The world: dusk, every agent at its door; the best one a step up with its basket.
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        var hero = ranked[0];
        {
            var inward = Market.Inward(hero.home);
            var step = new Vector2Int(Mathf.RoundToInt(inward.x), -Mathf.RoundToInt(inward.z));
            var side = new Vector2Int(step.y, step.x);
            for (int d = 0; d <= 3; d++) for (int s = -2; s <= 2; s++) { var p = hero.home + step * d + side * s; if (Arena.InBounds(p)) arena.RemoveItem(p); }
        }
        app.world.Build(arena, true);
        foreach (var a in arena.agents)
        {
            app.world.PlaceAtDoor(a, true, a == hero ? 0.55f : 0.4f);
            var bot = app.world.BotOf(a);
            if (bot == null) continue;
            bot.ClearGoods();
            foreach (var item in a.eaten.Take(9)) bot.AddGoods(item, ItemModels.Colour(item));
            if (!a.alive) bot.SetOut();
        }
        app.world.DoorLamp(hero);
        Color colour = bestMedal == 3 ? Gold : bestMedal == 2 ? Silver : bestMedal == 1 ? Bronze : Theme.Faint;
        string onStep = lesson != null && lesson.number == Ladder.ModelLevel && hero.score > 0 ? $"${hero.CostUsd / hero.score:0.0000} an item" : null; // the number that is the lesson
        app.world.Podium(bestMedal, colour, hero, ranked.Skip(1).Take(2).ToList(), onStep);
        app.world.TipOut(hero, id => arena.brief.IsSafe(Items.Get(id)));
        app.world.FamilySpot(hero);
        app.world.FamilyWave(0.9f); // the agent is home
        app.world.ShowDoorView(frame);
        app.atmosphere.MenuBlur(false);
        GameMenu.Icon(root);
        app.StartCoroutine(Reveal(bestMedal));
        if (lesson != null && Luma.Lines.ContainsKey("take-" + lesson.number)) app.StartCoroutine(SayLater("take-" + lesson.number, 2.4f));

        // The door: Next, a quiet Again, and Details.
        bool last = lesson != null && lesson.number >= Ladder.Count;
        var next = Ui.Button(root, lesson == null ? "Set up again" : last ? "Free play" : "Next", lesson == null ? app.ShowSetup : last ? app.StartFree : (System.Action)app.NextLevel, Ui.ButtonStyle.Accent, 22);
        Ui.Place(next.GetComponent<RectTransform>(), Ui.W - 40 - 240, 996, 240, 64);
        if (lesson != null)
        {
            var again = Ui.Button(root, "Again", app.ShowSetup, Ui.ButtonStyle.GhostOnDark, 18);
            Ui.Place(again.GetComponent<RectTransform>(), Ui.W - 40 - 240 - 14 - 140, 1004, 140, 56);
        }
        var details = Ui.Button(root, "Details", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(details.GetComponent<RectTransform>(), 40, 1004, 140, 56);
    }

    // Gold: every check done. Silver: at least half. Bronze: came home with dinner.
    static int Medal(AgentState a, List<(Practised result, string label)> checks)
    {
        if (checks == null) return a.alive ? 1 : 0;
        int ticked = checks.Count(c => c.result == Practised.Yes), needed = checks.Count(c => c.result != Practised.NotNeeded);
        return needed > 0 && ticked == needed ? 3 : ticked * 2 >= needed && needed > 0 ? 2 : a.alive ? 1 : 0;
    }

    IEnumerator Reveal(int medal)
    {
        yield return new WaitForSecondsRealtime(0.55f);
        if (root == null) yield break;
        if (medal > 0) Sfx.Sting(Sfx.Mood.Fanfare); else Sfx.Tick();
        yield return app.world.RaisePodium(medal);
    }

    IEnumerator SayLater(string id, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (root != null) app.luma.Cue(root, id, 40, 920, 820, stay: true);
    }

    // ---- Details: the checks, the agents, the costs, the lists, behind one button ----

    void ToggleSheet()
    {
        if (sheet != null) { Object.Destroy(sheet.gameObject); sheet = null; return; }
        var l = app.lesson;
        const float W = 760f, DW = W - 56;
        sheet = Ui.Node("Details", root);
        Ui.Place(sheet, Ui.W - W, 0, W, Ui.H);
        var bg = Ui.Box(sheet, "Glass", Ui.DarkFill, false);
        Ui.Stretch(bg.rectTransform);
        var keyline = Ui.Box(sheet, "Keyline", Theme.Teal, false);
        Ui.Place(keyline.rectTransform, 0, 0, 3, Ui.H);
        var tag = Ui.Eyebrow(sheet, l != null ? $"DETAILS  ·  ERRAND {l.id}" : "DETAILS", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 28, 26, 500, 22);
        var close = Ui.Button(sheet, "Close", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), W - 112, 18, 88, 40);
        var scroll = Ui.Scroll(sheet, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 20, 70, W - 40, Ui.H - 90);
        float y = 0;
        var msg = Ui.Text(content, $"{arena.brief.from}: {arena.brief.rule}", 17, Color.white, true);
        float mh = msg.GetPreferredValues(msg.text, DW, 0).y + 4;
        Ui.Place(msg.rectTransform, 8, y, DW, mh); y += mh + 6;
        var note = Ui.Text(content, "The catch: " + arena.brief.note, 16, Theme.Faint);
        float nh = note.GetPreferredValues(note.text, DW, 0).y + 4;
        Ui.Place(note.rectTransform, 8, y, DW, nh); y += nh + 14;
        foreach (var (a, medal, checks) in rows)
        {
            int n = checks?.Count ?? 0;
            float h = 96 + n * 26 + 10;
            var card = Ui.DarkCard(content, a.Name);
            Ui.Place(card.rectTransform, 0, y, W - 40, h);
            var t = card.transform;
            var dot = Ui.Dot(t, Theme.Team[a.config.colour], 16); Ui.Place(dot.rectTransform, 16, 14, 16, 16);
            var name = Ui.Text(t, a.Name, 20, Color.white, true); Ui.Place(name.rectTransform, 42, 8, 400, 30);
            if (l != null && medal > 0)
            {
                var badge = Ui.Text(t, MedalNames[medal], 13, medal == 3 ? Gold : medal == 2 ? Silver : Bronze, true, TextAlignmentOptions.TopRight);
                badge.characterSpacing = 3; Ui.Place(badge.rectTransform, W - 40 - 160, 14, 140, 20);
            }
            var story = Ui.Text(t, Verdicts.Short(a) + (app.lesson != null && app.lesson.number == Ladder.Count && Verdicts.PartShort(a) != null ? "  ·  " + Verdicts.PartShort(a) : ""), 16, a.alive ? Theme.Text2 : Theme.DangerText);
            Ui.Place(story.rectTransform, 42, 38, W - 100, 24);
            story.enableWordWrapping = false; story.overflowMode = TextOverflowModes.Ellipsis;
            float bx = 42;
            foreach (var item in a.eaten.Take(10))
            {
                bool bad = !arena.brief.IsSafe(Items.Get(item));
                if (bad) { var ring = Ui.Box(t, "Ring", Theme.Danger); Ui.Place(ring.rectTransform, bx - 3, 63, 30, 30); }
                var icon = Ui.Icon(t, item, 24); Ui.Place(icon.rectTransform, bx, 66, 24, 24);
                bx += 30;
            }
            var cost = Ui.Text(t, a.config.IsRule ? "0 tokens  ·  $0  ·  a fixed rule" : $"{a.TokensUsed:N0} tokens  ·  ${a.CostUsd:0.000}  ·  {Models.All[a.config.model].label}", 14, Theme.Faint, false, TextAlignmentOptions.TopRight);
            Ui.Place(cost.rectTransform, W - 40 - 380, 68, 360, 20);
            float cy = 96;
            if (checks != null)
                foreach (var (result, label) in checks)
                {
                    var lamp = Ui.Box(t, "Lamp", result == Practised.Yes ? Theme.Aqua : result == Practised.Missed ? Theme.Danger : Theme.Border, false);
                    Ui.Place(lamp.rectTransform, 18, cy + 6, 12, 12); lamp.raycastTarget = false;
                    var lab = Ui.Text(t, Labels.Of(label), 15, result == Practised.NotNeeded ? Theme.Faint : Color.white);
                    Ui.Place(lab.rectTransform, 42, cy, W - 100, 22);
                    lab.enableWordWrapping = false; lab.overflowMode = TextOverflowModes.Ellipsis;
                    cy += 26;
                }
            y += h + 10;
        }
        if (l != null)
        {
            var stars = Ui.Text(content, $"{bestMedal} of 3 stars  ·  {app.TotalStars} in all  ·  {app.Rank}", 16, Theme.Teal, true);
            Ui.Place(stars.rectTransform, 8, y, DW, 24); y += 34;
        }
        var lh = Ui.Eyebrow(content, "THE LISTS", Theme.Teal); Ui.Place(lh.rectTransform, 8, y, DW, 20); y += 26;
        foreach (var a in arena.agents)
        {
            string text = $"{a.Name}: " + (string.IsNullOrWhiteSpace(a.config.instructions) ? "(no list at all)" : a.config.instructions.Trim());
            var body = Ui.Text(content, text, 15, a.alive ? Color.white : Theme.DangerText);
            float h = body.GetPreferredValues(text, DW, 0).y + 4;
            Ui.Place(body.rectTransform, 8, y, DW, h); y += h + 8;
        }
        y += 8;
        if (l != null)
        {
            var take = Ui.Text(content, l.takeaway, 16, Color.white);
            float th = take.GetPreferredValues(l.takeaway, DW, 0).y + 4;
            Ui.Place(take.rectTransform, 8, y, DW, th); y += th + 10;
            var ask = Ui.Text(content, l.ask, 15, Theme.Faint);
            float ah = ask.GetPreferredValues(l.ask, DW, 0).y + 4;
            Ui.Place(ask.rectTransform, 8, y, DW, ah); y += ah + 16;
            if (l.number < Ladder.Count)
            {
                var nextL = Ladder.Get(l.number + 1);
                var unlock = Ui.Text(content, $"Next: {nextL.id} {nextL.title.Replace("Capstone: ", "")}", 15, Theme.Teal, true);
                Ui.Place(unlock.rectTransform, 8, y, DW, 24); y += 34;
            }
            if (l.number == Ladder.Count)
            {
                var pl = Ui.Button(content, "If-then plans", ShowPlans, Ui.ButtonStyle.GhostOnDark, 16);
                Ui.Place(pl.GetComponent<RectTransform>(), 8, y, 200, 46); y += 56;
            }
        }
        var file = Ui.Button(content, "The file", () => app.ShowFile(extras.ToArray()), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(file.GetComponent<RectTransform>(), 8, y, 140, 46); y += 56;
        content.sizeDelta = new Vector2(0, y + 20);
    }

    // The capstone's two if-then plans, per student, about their own work.
    void ShowPlans()
    {
        if (plans != null) { Object.Destroy(plans.gameObject); plans = null; return; }
        plans = Ui.Node("Plans", root);
        Ui.Stretch(plans);
        var dim = Ui.Box(plans, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.45f), false);
        Ui.Stretch(dim.rectTransform);
        var card = Ui.Box(plans, "Card", Ui.DarkFill);
        Ui.Place(card.rectTransform, 360, 160, 1200, 760);
        var t = card.transform;
        var title = Ui.Text(t, "Your two if-then plans", 26, Color.white, true);
        Ui.Place(title.rectTransform, 32, 26, 800, 36);
        var sub = Ui.Text(t, "Two plans about your own work with AI agents, to look at again at the follow-up session. Pick your agent, write, save.", 15, Theme.Faint);
        Ui.Place(sub.rectTransform, 32, 62, 1000, 24);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(plans.gameObject); plans = null; }, Ui.ButtonStyle.GhostOnDark, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 32 - 100, 26, 100, 40);
        var fields = Ui.Node("Fields", t);
        Ui.Place(fields, 32, 150, 1136, 560);
        int chosen = 0;
        void Build()
        {
            Ui.Clear(fields);
            var a = arena.agents[chosen];
            string key = $"ai_arena_plans_{app.lesson.id}_{a.Name}";
            var saved = PlayerPrefs.GetString(key, "\n").Split('\n');
            var inputs = new TMP_InputField[2];
            for (int i = 0; i < 2; i++)
            {
                var label = Ui.Eyebrow(fields, i == 0 ? "PLAN 1  ·  IF ..., THEN I ..." : "PLAN 2  ·  IF ..., THEN I ...", Theme.Teal);
                Ui.Place(label.rectTransform, 0, i * 220, 600, 20);
                inputs[i] = Ui.DarkInput(fields, "If an agent at work meets a case its instructions do not cover, then I ...", true, 18);
                Ui.Place(inputs[i].GetComponent<RectTransform>(), 0, i * 220 + 26, 1136, 160);
                inputs[i].text = i < saved.Length ? saved[i] : "";
            }
            var save = Ui.Button(fields, "Save", () =>
            {
                PlayerPrefs.SetString(key, inputs[0].text.Replace("\n", " ") + "\n" + inputs[1].text.Replace("\n", " "));
                PlayerPrefs.Save();
                Sfx.Buy();
            }, Ui.ButtonStyle.Accent, 16);
            Ui.Place(save.GetComponent<RectTransform>(), 0, 460, 140, 46);
        }
        float px = 32;
        for (int i = 0; i < arena.agents.Count; i++)
        {
            int index = i;
            var a = arena.agents[i];
            var pill = Ui.Button(t, a.Name, () => { chosen = index; Build(); }, Ui.ButtonStyle.GhostOnDark, 14);
            Ui.Place(pill.GetComponent<RectTransform>(), px, 100, 150, 40);
            px += 160;
        }
        Build();
    }
}
