using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief of errand 1.6, a moment then a door: the agent at the door, the medal with as
// many stars as checks met, LUMA's one line, Next and Again. For every task what the class
// chose, what the fixed rule did and cost, what the agent did and cost, and the better choice
// with the reason, behind Details. The report to the platform is as it was.
public class DecideDebrief
{
    public static DecideDebrief Current;
    public readonly RectTransform root;
    readonly App app;
    readonly App.DecideRun run;
    readonly List<(bool ok, string label)> checks;
    readonly int ticked;
    RectTransform sheet;
    static readonly Color Gold = Theme.Hex("#E0B23A"), Silver = Theme.Hex("#AAB4BE"), Bronze = Theme.Hex("#C48A5A");

    public DecideDebrief(Transform parent, App app, App.DecideRun run)
    {
        Current = this;
        this.app = app; this.run = run;
        root = Ui.Node("DecideDebrief", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        var lesson = app.lesson;

        var tasks = Decide.Tasks;
        bool ruleWhereItWorked = Enumerable.Range(0, tasks.Length).Where(i => tasks[i].right == Decide.Automation).All(i => run.choices[i] == Decide.Automation);
        bool agentWhereNeeded = Enumerable.Range(0, tasks.Length).Where(i => tasks[i].right == Decide.Agent).All(i => run.choices[i] == Decide.Agent);
        bool neitherMarked = Enumerable.Range(0, tasks.Length).Where(i => tasks[i].right == Decide.Neither).All(i => run.choices[i] == Decide.Neither);
        checks = new List<(bool ok, string label)>
        {
            (ruleWhereItWorked, "Chose a simple automation where one did the job"),
            (agentWhereNeeded, "Chose an agent where the task needed judgment"),
            (neitherMarked, "Marked the task for a person as neither"),
        };
        ticked = checks.Count(c => c.ok);
        string result = ticked == checks.Count ? "complete" : ticked * 2 >= checks.Count ? "partial" : "incomplete";
        app.SetStars(lesson.number, ticked);
        app.lastMedal = ticked; app.lastMedalLevel = lesson.number;
        app.StartCoroutine(Platform.ReportProgress($"{lesson.id}:{result}", ticked, string.Join("; ", checks.Select(c => $"{c.label}: {(c.ok ? "done" : "missed")}"))));

        // The world: the door at dusk, the medal on its podium.
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        var stage = new Arena(app.brief, app.configs.ConvertAll(c => c.Clone()), app.budget, 61);
        var hero = stage.agents[0];
        {
            var inward = Market.Inward(hero.home);
            var step = new Vector2Int(Mathf.RoundToInt(inward.x), -Mathf.RoundToInt(inward.z));
            var side = new Vector2Int(step.y, step.x);
            for (int d = 0; d <= 3; d++) for (int s = -2; s <= 2; s++) { var p = hero.home + step * d + side * s; if (Arena.InBounds(p)) stage.RemoveItem(p); }
        }
        app.world.Build(stage, true);
        app.world.PlaceAtDoor(hero, true, 0.55f);
        app.world.DoorLamp(hero);
        Color colour = ticked == 3 ? Gold : ticked == 2 ? Silver : ticked == 1 ? Bronze : Theme.Faint;
        app.world.Podium(ticked, colour, hero);
        app.world.ShowDoorView(frame);
        app.atmosphere.MenuBlur(false);
        GameMenu.Icon(root);
        app.StartCoroutine(Reveal(ticked));
        if (Luma.Lines.ContainsKey("take-" + lesson.number)) app.StartCoroutine(SayLater("take-" + lesson.number, 2.4f));

        var next = Ui.Button(root, "Next", app.NextLevel, Ui.ButtonStyle.Accent, 22);
        Ui.Place(next.GetComponent<RectTransform>(), Ui.W - 40 - 240, 996, 240, 64);
        var again = Ui.Button(root, "Again", app.ShowSetup, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(again.GetComponent<RectTransform>(), Ui.W - 40 - 240 - 14 - 140, 1004, 140, 56);
        var details = Ui.Button(root, "Details", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(details.GetComponent<RectTransform>(), 40, 1004, 140, 56);
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

    void ToggleSheet()
    {
        if (sheet != null) { Object.Destroy(sheet.gameObject); sheet = null; return; }
        var lesson = app.lesson;
        const float W = 760f, DW = W - 56;
        sheet = Ui.Node("Details", root);
        Ui.Place(sheet, Ui.W - W, 0, W, Ui.H);
        var bg = Ui.Box(sheet, "Glass", Ui.DarkFill, false); Ui.Stretch(bg.rectTransform);
        var keyline = Ui.Box(sheet, "Keyline", Theme.Teal, false); Ui.Place(keyline.rectTransform, 0, 0, 3, Ui.H);
        var tag = Ui.Eyebrow(sheet, $"DETAILS  ·  ERRAND {lesson.id}", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 28, 26, 500, 22);
        var close = Ui.Button(sheet, "Close", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), W - 112, 18, 88, 40);
        var scroll = Ui.Scroll(sheet, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 20, 70, W - 40, Ui.H - 90);
        float y = 0;
        long agentTokens = run.results.Count > 0 ? (long)run.results.Average(r => r.agent.TokensUsed) : 0;
        var measured = Ui.Text(content, $"Agents: {agentTokens:N0} tokens a task on average. Fixed rules: none.", 16, Color.white, true);
        Ui.Place(measured.rectTransform, 8, y, DW, 24); y += 30;
        var course = Ui.Text(content, "The course says an agent costs about four times a chat to run; the figure above is this game's own measurement.", 15, Theme.Faint);
        float ch = course.GetPreferredValues(course.text, DW, 0).y + 4;
        Ui.Place(course.rectTransform, 8, y, DW, ch); y += ch + 14;
        var whys = new List<(string head, string body)>();
        var tasks = Decide.Tasks;
        for (int i = 0; i < tasks.Length; i++)
        {
            var task = tasks[i];
            int chosen = run.choices[i];
            bool rightCall = chosen == task.right;
            var r = run.results.FirstOrDefault(x => x.task == task);
            float h = r.task != null ? 150 : 100;
            var card = Ui.DarkCard(content, "Task " + (i + 1));
            Ui.Place(card.rectTransform, 0, y, W - 40, h);
            var t = card.transform;
            var num = Ui.Text(t, (i + 1).ToString(), 26, rightCall ? Theme.Aqua : Theme.DangerText, true, TextAlignmentOptions.Center);
            Ui.Place(num.rectTransform, 10, 12, 40, 36);
            var msg = Ui.Text(t, task.message, 15, Color.white, true);
            Ui.Place(msg.rectTransform, 56, 12, W - 120, 46);
            var choice = Ui.Text(t, $"Your call: {Decide.Choices[Mathf.Max(0, chosen)].ToLowerInvariant()}  ·  {(rightCall ? "right" : "better: " + Decide.Choices[task.right].ToLowerInvariant())}", 14, rightCall ? Theme.Teal : Theme.DangerText, true);
            Ui.Place(choice.rectTransform, 56, 60, W - 120, 22);
            if (r.task != null)
            {
                Column(t, 56, 88, "THE FIXED RULE", r.rule, true);
                Column(t, 56 + 320, 88, "THE AGENT", r.agent, false);
            }
            else
            {
                var none = Ui.Text(t, "Not run: no agent and no rule should take this one.", 14, Theme.Faint);
                Ui.Place(none.rectTransform, 56, 86, W - 120, 20);
            }
            whys.Add(($"{i + 1}  ·  {task.message.ToUpperInvariant()}", $"The better call: {Decide.Choices[task.right].ToLowerInvariant()}. {task.why}"));
            y += h + 10;
        }
        foreach (var (ok, label) in checks)
        {
            var lamp = Ui.Box(content, "Lamp", ok ? Theme.Aqua : Theme.Danger, false);
            Ui.Place(lamp.rectTransform, 10, y + 6, 12, 12); lamp.raycastTarget = false;
            var lab = Ui.Text(content, Labels.Of(label), 15, Color.white);
            Ui.Place(lab.rectTransform, 34, y, DW, 22);
            y += 26;
        }
        y += 10;
        var stars = Ui.Text(content, $"{ticked} of 3 stars  ·  {app.TotalStars} in all  ·  {app.Rank}", 16, Theme.Teal, true);
        Ui.Place(stars.rectTransform, 8, y, DW, 24); y += 34;
        var take = Ui.Text(content, lesson.takeaway, 16, Color.white);
        float th = take.GetPreferredValues(lesson.takeaway, DW, 0).y + 4;
        Ui.Place(take.rectTransform, 8, y, DW, th); y += th + 10;
        var ask = Ui.Text(content, lesson.ask, 15, Theme.Faint);
        float ah = ask.GetPreferredValues(lesson.ask, DW, 0).y + 4;
        Ui.Place(ask.rectTransform, 8, y, DW, ah); y += ah + 16;
        whys.Add(("THE CHECKS", string.Join("\n", checks.Select(c => (c.ok ? "Done: " : "Missed: ") + c.label))));
        var file = Ui.Button(content, "The file", () => app.ShowFile(whys.ToArray()), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(file.GetComponent<RectTransform>(), 8, y, 140, 46); y += 56;
        content.sizeDelta = new Vector2(0, y + 20);
    }

    static void Column(Transform t, float x, float y, string head, AgentState a, bool rule)
    {
        var eyebrow = Ui.Eyebrow(t, head, rule ? Theme.Faint : Theme.Teal);
        Ui.Place(eyebrow.rectTransform, x, y, 300, 20);
        string outcome = !a.alive ? $"Sent home on turn {a.eliminatedTurn}: bought {Items.Named(a.ateItem)}"
                       : a.outOfTokens ? $"Ran out of tokens with {a.score} in the basket"
                       : $"Came home with {a.score} wanted item{(a.score == 1 ? "" : "s")}";
        var line = Ui.Text(t, outcome, 14, a.alive ? Color.white : Theme.DangerText, true);
        Ui.Place(line.rectTransform, x, y + 20, 300, 40);
        var cost = Ui.Text(t, rule ? "0 tokens, $0: a fixed rule, no AI" : $"{a.TokensUsed:N0} tokens, ${a.CostUsd:0.0000} on {Models.All[a.config.model].label}", 13, Theme.Faint);
        Ui.Place(cost.rectTransform, x, y + 42, 300, 20);
    }
}
