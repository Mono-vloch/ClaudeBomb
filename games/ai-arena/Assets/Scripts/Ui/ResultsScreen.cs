using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief: the agents are home. The ranking with each basket and a medal,
// two plain sentences per agent on what happened and why, what each one did
// against the errand's checks, LUMA's recap with the one idea to take away, and
// the message next to every list. After Your run the errand's stars are banked
// and the next errand's unlock is revealed; the capstone also names the part
// of the agent that failed and asks for the two if-then plans.
public class ResultsScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Arena arena;
    RectTransform lists, plans;

    static readonly Color Gold = Theme.Hex("#E0B23A"), Silver = Theme.Hex("#AAB4BE"), Bronze = Theme.Hex("#C48A5A");
    static readonly string[] MedalNames = { "", "BRONZE", "SILVER", "GOLD" };

    public ResultsScreen(Transform parent, App app, Arena arena, Simulation sim)
    {
        this.app = app; this.arena = arena;
        var lesson = app.lesson;
        bool training = app.Training, capstone = lesson != null && lesson.number == Ladder.Count, counts = lesson != null && !training;
        root = Ui.Node("Results", parent);
        Ui.Stretch(root);
        app.Paper(Theme.Page);

        var title = Ui.Text(root, lesson != null ? $"Debrief · Errand {lesson.id} · {app.StepName}" : "The agents are home", 48, Theme.Ink, true);
        Ui.Place(title.rectTransform, 80, 44, 1200, 60);
        var sub = Ui.Text(root, $"{arena.brief.from}: {arena.brief.rule}  Turns played: {Mathf.Min(sim.turn, sim.totalTurns)} of {sim.totalTurns}.", 18, Theme.Text2);
        Ui.Place(sub.rectTransform, 80, 110, 1600, 30);

        var ranked = arena.agents
            .OrderByDescending(a => a.alive ? 1 : 0)
            .ThenByDescending(a => a.score)
            .ThenByDescending(a => a.eliminatedTurn)
            .ToList();

        int n = ranked.Count;
        float recap = lesson != null ? 150 : 0;
        float y = 156, h = Mathf.Min(122, (1080 - 156 - 130 - recap - 10 * (n - 1)) / n);
        bool checks = lesson != null && h >= 100;
        int bestMedal = 0;
        foreach (var a in ranked)
        {
            List<(Practised result, string label)> list = lesson != null ? lesson.Check(a, arena) : null;
            int medal = Medal(a, list);
            bestMedal = Mathf.Max(bestMedal, medal);

            // The platform keeps the result (web only): the errand's checks for each agent, once per level that counts.
            if (counts)
            {
                int ticked = list.Count(c => c.result == Practised.Yes), needed = list.Count(c => c.result != Practised.NotNeeded);
                string result = ticked == needed ? "complete" : ticked * 2 >= needed ? "partial" : "incomplete";
                string detail = $"{a.Name}: " + string.Join("; ", list.Where(c => c.result != Practised.NotNeeded).Select(c => $"{c.label}: {(c.result == Practised.Yes ? "done" : "missed")}"));
                app.StartCoroutine(Platform.ReportProgress($"{lesson.id}:{result}", medal, detail));
            }

            int rank = ranked.IndexOf(a);
            var card = Ui.Card(root, "Row", Theme.Panel, rank == 0 && a.alive ? Theme.BorderTeal : Theme.Border1);
            Ui.Place(card.rectTransform, 80, y, 1760, h);
            var t = card.transform;

            var place = Ui.Text(t, (rank + 1).ToString(), 40, rank == 0 && a.alive ? Theme.Aqua : Theme.Muted, true, TextAlignmentOptions.Center);
            Ui.Place(place.rectTransform, 10, h / 2 - 26, 70, 52);
            var dot = Ui.Dot(t, Theme.Team[a.config.colour], 22);
            Ui.Place(dot.rectTransform, 92, h >= 100 ? 24 : h / 2 - 11, 22, 22);
            var name = Ui.Text(t, a.Name, 26, Theme.Ink, true);
            Ui.Place(name.rectTransform, 126, h >= 100 ? 14 : h / 2 - 20, 280, 40);
            name.enableWordWrapping = false;
            if (h >= 100)
            {
                // The basket: what it bought, in order. The thing that ruined dinner is ringed in red.
                var basket = Ui.Icon(t, "basket", 30);
                Ui.Place(basket.rectTransform, 126, 60, 30, 30);
                float bx = 164;
                foreach (var item in a.eaten.Take(9))
                {
                    bool bad = !arena.brief.IsSafe(Items.Get(item));
                    if (bad)
                    {
                        var ring = Ui.Dot(t, Theme.Danger, 34);
                        Ui.Place(ring.rectTransform, bx - 3, 58, 34, 34);
                        var inner = Ui.Dot(t, Theme.DangerSoft, 30);
                        Ui.Place(inner.rectTransform, bx - 1, 60, 30, 30);
                    }
                    var icon = Ui.Icon(t, item, 26);
                    Ui.Place(icon.rectTransform, bx + 1, 62, 26, 26);
                    bx += 32;
                }
                if (a.eaten.Count == 0)
                {
                    var empty = Ui.Text(t, "empty", 14, Theme.Muted);
                    Ui.Place(empty.rectTransform, 166, 66, 100, 20);
                }
            }
            var score = Ui.Text(t, a.alive ? a.score.ToString() : "home early", a.alive ? 30 : 18, a.alive ? Theme.Ink : Theme.Danger, true, TextAlignmentOptions.MidlineRight);
            Ui.Place(score.rectTransform, 400, 0, 150, h);
            if (lesson != null && medal > 0)
            {
                var pill = Ui.Box(t, "Medal", medal == 3 ? Gold : medal == 2 ? Silver : Bronze);
                Ui.Place(pill.rectTransform, 580, h / 2 - 13, 80, 26);
                var mt = Ui.Text(pill.transform, MedalNames[medal], 11, Color.white, true, TextAlignmentOptions.Center);
                mt.characterSpacing = 3;
                Ui.Stretch(mt.rectTransform);
            }

            string story = Verdicts.Of(a, arena, sim);
            if (capstone) { string part = Verdicts.Part(a); if (part != null) story += "\nWhich part did it? " + part; }
            var verdict = Ui.Text(t, story, h < 80 ? 14 : checks ? 15 : 17, Theme.Text2);
            Ui.Place(verdict.rectTransform, 680, 8, checks ? 500 : 840, h - 16);
            verdict.alignment = TextAlignmentOptions.MidlineLeft;

            if (checks)
            {
                float cy = 12;
                foreach (var (result, label) in list)
                {
                    // A word as well as a colour, so the result reads without the colour.
                    var word = Ui.Text(t, result == Practised.Yes ? "Done" : result == Practised.Missed ? "Missed" : "n/a", 13,
                                       result == Practised.Yes ? Theme.TealDeep : result == Practised.Missed ? Theme.Danger : Theme.Muted, true);
                    Ui.Place(word.rectTransform, 1194, cy + 1, 56, 22);
                    var lab = Ui.Text(t, label, 14, result == Practised.NotNeeded ? Theme.Muted : Theme.Ink);
                    Ui.Place(lab.rectTransform, 1254, cy, 290, 22);
                    lab.enableWordWrapping = false;
                    lab.overflowMode = TextOverflowModes.Ellipsis;
                    cy += 24;
                }
            }

            var cost = Ui.Text(t, a.config.IsRule ? "0 tokens\n$0\nfixed rule" : $"{a.TokensUsed:N0} tokens\n${a.CostUsd:0.000}\n{Models.All[a.config.model].label}", 15, Theme.Muted, false, TextAlignmentOptions.MidlineRight);
            Ui.Place(cost.rectTransform, 1550, 8, 190, h - 16);
            y += h + 10;
        }

        if (lesson != null)
        {
            if (counts) app.SetStars(lesson.number, bestMedal);
            app.luma.Say("recap");
            bool unlock = counts && lesson.number < Ladder.Count;
            Luma.Card(root, 80, y + 4, unlock ? 1320 : 1760, 136, lesson.takeaway + (capstone ? " " + Ladder.FiveWays : ""), 18, "Ask as the agent's manager: " + lesson.ask);
            if (unlock) Unlock(y + 4, Ladder.Get(lesson.number + 1), bestMedal);

            float bx = 80;
            Button Add(string label, System.Action act, Ui.ButtonStyle style, float w)
            {
                var b = Ui.Button(root, label, act, style, 18);
                Ui.Place(b.GetComponent<RectTransform>(), bx, 990, w, 56);
                bx += w + 16;
                return b;
            }
            if (training && app.step == App.Step.SeeItWork)
            {
                if (lesson.broken != null) Add("Next: Fix the broken bot", () => { app.SetStep(App.Step.FixIt); app.ShowSetup(); }, Ui.ButtonStyle.Primary, 320);
                Add("Next: Your run", () => { app.SetStep(App.Step.YourTurn); app.ShowSetup(); }, lesson.broken != null ? Ui.ButtonStyle.Secondary : Ui.ButtonStyle.Primary, 220);
                Add("Watch it again", app.ShowSetup, Ui.ButtonStyle.Secondary, 200);
            }
            else if (training)
            {
                Add("Fix the list, deploy again", app.ShowSetup, Ui.ButtonStyle.Primary, 320);
                Add("Next: Your run", () => { app.SetStep(App.Step.YourTurn); app.ShowSetup(); }, Ui.ButtonStyle.Secondary, 220);
            }
            else
            {
                bool last = lesson.number >= Ladder.Count;
                Add(last ? "Free play" : $"Next errand: {Ladder.Get(lesson.number + 1).title}", last ? app.StartFree : (System.Action)app.NextLevel, Ui.ButtonStyle.Primary, last ? 200 : 460);
                Add("Same errand again", app.ShowSetup, Ui.ButtonStyle.Secondary, 240);
                if (capstone) Add("If-then plans", ShowPlans, Ui.ButtonStyle.Secondary, 200);
            }
            Add("The lists", ShowLists, Ui.ButtonStyle.Secondary, 150);
            Add("Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 170);
            if (Platform.IsWeb) Add("Games menu", Platform.GoToMenu, Ui.ButtonStyle.Quiet, 170);
        }
        else
        {
            var tweak = Ui.Button(root, "Change the agents", app.ShowSetup, Ui.ButtonStyle.Primary, 18);
            Ui.Place(tweak.GetComponent<RectTransform>(), 80, 990, 300, 56);
            var again = Ui.Button(root, "Same agents, new message", () => { app.NewBrief(); app.ShowSetup(); }, Ui.ButtonStyle.Secondary, 18);
            Ui.Place(again.GetComponent<RectTransform>(), 400, 990, 340, 56);
            var show = Ui.Button(root, "The lists", ShowLists, Ui.ButtonStyle.Secondary, 18);
            Ui.Place(show.GetComponent<RectTransform>(), 760, 990, 160, 56);
            var home = Ui.Button(root, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 18);
            Ui.Place(home.GetComponent<RectTransform>(), 940, 990, 180, 56);
            if (Platform.IsWeb)
            {
                var menu = Ui.Button(root, "Games menu", Platform.GoToMenu, Ui.ButtonStyle.Quiet, 18);
                Ui.Place(menu.GetComponent<RectTransform>(), 1140, 990, 180, 56);
            }
            app.StartCoroutine(Platform.ReportProgress("free:finished", 0, ranked.Count > 0 ? $"{ranked[0].Name} first" : null));
        }
        Ui.Emblem(root);
    }

    // Gold: every check done. Silver: at least half. Bronze: came home with dinner.
    static int Medal(AgentState a, List<(Practised result, string label)> checks)
    {
        if (checks == null) return a.alive ? 1 : 0;
        int ticked = checks.Count(c => c.result == Practised.Yes), needed = checks.Count(c => c.result != Practised.NotNeeded);
        return needed > 0 && ticked == needed ? 3 : ticked * 2 >= needed && needed > 0 ? 2 : a.alive ? 1 : 0;
    }

    // What the next errand opens, revealed here: the reward for this one.
    void Unlock(float y, Lesson next, int stars)
    {
        var card = Ui.Card(root, "Unlock", Theme.TealDeep, Theme.TealDeep);
        Ui.Place(card.rectTransform, 1420, y, 420, 136);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"UNLOCKED FOR ERRAND {next.id}", Theme.Teal);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 20, 14, 380, 20);
        string what = next.number == Ladder.VisionLevel ? "How far it sees: the vision dial opens."
                    : next.number == Ladder.MemoryLevel ? "Memory: none, the last turns, or a notebook it writes."
                    : next.number == Ladder.ModelLevel ? "Brain and effort: which Claude, and how hard it thinks."
                    : next.number == Ladder.DecideLevel ? "The choice: an agent, a fixed rule, or neither."
                    : next.number == Ladder.Count ? "Everything open, and no help: the capstone."
                    : next.title;
        var body = Ui.Text(t, what, 17, Color.white, true);
        Ui.Place(body.rectTransform, 20, 38, 380, 56);
        var starsLine = Ui.Text(t, $"{stars} of 3 stars banked for this errand  ·  {app.TotalStars} in all  ·  {app.Rank}", 13, Theme.Teal);
        Ui.Place(starsLine.rectTransform, 20, 100, 380, 24);
    }

    // On the platform's site: out of this game, back to the menu of games.
    // The family's message next to every agent's list: where the gap was.
    void ShowLists()
    {
        if (lists != null) { Object.Destroy(lists.gameObject); lists = null; return; }
        lists = Ui.Node("Lists", root);
        Ui.Stretch(lists);
        var dim = Ui.Box(lists, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.45f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(() => { Object.Destroy(lists.gameObject); lists = null; });
        var card = Ui.Float(lists, "Card", 260, 110, 1400, 860);
        var t = card.transform;
        var title = Ui.Text(t, "The message, and what each agent was told", 26, Theme.Ink, true);
        Ui.Place(title.rectTransform, 32, 26, 1000, 36);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(lists.gameObject); lists = null; }, Ui.ButtonStyle.Secondary, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 1400 - 32 - 100, 26, 100, 40);
        var msg = Ui.Soft(t, "Message");
        Ui.Place(msg.rectTransform, 32, 76, 1336, 96);
        var from = Ui.Eyebrow(msg.transform, $"A MESSAGE FROM {arena.brief.from.ToUpperInvariant()}", Theme.TealDeep);
        Ui.Place(from.rectTransform, 22, 14, 600, 22);
        var rule = Ui.Text(msg.transform, arena.brief.rule, 21, Theme.Ink);
        Ui.Place(rule.rectTransform, 22, 40, 1290, 30);
        var catchNote = Ui.Text(msg.transform, "The catch: " + arena.brief.note, 14, Theme.Text2);
        Ui.Place(catchNote.rectTransform, 22, 68, 1290, 22);
        var scroll = Ui.Scroll(t, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 32, 190, 1336, 640);
        float y = 0;
        foreach (var a in arena.agents)
        {
            string text = string.IsNullOrWhiteSpace(a.config.instructions) ? "(no list at all)" : a.config.instructions.Trim();
            var body = Ui.Text(content, text, 17, Theme.Ink);
            float h = body.GetPreferredValues(text, 1240, 0).y;
            var row = Ui.Card(content, "Row", a.alive ? Theme.Panel : Theme.DangerSoft, a.alive ? Theme.Border1 : Theme.Danger);
            Ui.Place(row.rectTransform, 0, y, 1336, h + 70);
            row.transform.SetAsFirstSibling();
            var dot = Ui.Dot(row.transform, Theme.Team[a.config.colour], 16);
            Ui.Place(dot.rectTransform, 20, 18, 16, 16);
            var name = Ui.Text(row.transform, $"{a.Name}  ·  {a.Status}", 17, a.alive ? Theme.Ink : Theme.Danger, true);
            Ui.Place(name.rectTransform, 46, 14, 1200, 24);
            body.transform.SetParent(row.transform, false);
            Ui.Place(body.rectTransform, 46, 44, 1240, h);
            y += h + 80;
        }
        content.sizeDelta = new Vector2(0, y);
    }

    // The capstone's two if-then plans, per student, about their own work. Kept
    // on this machine until the course's follow-up session.
    void ShowPlans()
    {
        if (plans != null) { Object.Destroy(plans.gameObject); plans = null; return; }
        plans = Ui.Node("Plans", root);
        Ui.Stretch(plans);
        var dim = Ui.Box(plans, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.45f), false);
        Ui.Stretch(dim.rectTransform);
        var card = Ui.Float(plans, "Card", 360, 160, 1200, 760);
        var t = card.transform;
        var title = Ui.Text(t, "Your two if-then plans", 26, Theme.Ink, true);
        Ui.Place(title.rectTransform, 32, 26, 800, 36);
        var sub = Ui.Text(t, "Two plans about your own work with AI agents, to look at again at the follow-up session. Pick your agent, write, save.", 15, Theme.Text2);
        Ui.Place(sub.rectTransform, 32, 62, 1000, 24);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(plans.gameObject); plans = null; }, Ui.ButtonStyle.Secondary, 15);
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
                var label = Ui.Eyebrow(fields, i == 0 ? "PLAN 1  ·  IF ..., THEN I ..." : "PLAN 2  ·  IF ..., THEN I ...", Theme.TealDeep);
                Ui.Place(label.rectTransform, 0, i * 220, 600, 20);
                inputs[i] = Ui.Input(fields, "If an agent at work meets a case its instructions do not cover, then I ...", true, 18);
                Ui.Place(inputs[i].GetComponent<RectTransform>(), 0, i * 220 + 26, 1136, 160);
                inputs[i].text = i < saved.Length ? saved[i] : "";
            }
            var save = Ui.Button(fields, "Save", () =>
            {
                PlayerPrefs.SetString(key, inputs[0].text.Replace("\n", " ") + "\n" + inputs[1].text.Replace("\n", " "));
                PlayerPrefs.Save();
                Sfx.Buy();
            }, Ui.ButtonStyle.Primary, 16);
            Ui.Place(save.GetComponent<RectTransform>(), 0, 460, 140, 46);
        }
        float px = 32;
        for (int i = 0; i < arena.agents.Count; i++)
        {
            int index = i;
            var a = arena.agents[i];
            var pill = Ui.Button(t, a.Name, () => { chosen = index; Build(); }, Ui.ButtonStyle.Secondary, 14);
            Ui.Place(pill.GetComponent<RectTransform>(), px, 100, 150, 40);
            px += 160;
        }
        Build();
    }
}
