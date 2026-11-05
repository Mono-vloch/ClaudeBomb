using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief of errand 1.6: for every task, what the class chose, what the fixed
// rule did and what it cost, what the agent did and what it cost, and the better
// choice with the reason. The game's own measured figure sits next to the
// course's "about four times a chat".
public class DecideDebrief
{
    public readonly RectTransform root;

    public DecideDebrief(Transform parent, App app, App.DecideRun run)
    {
        root = Ui.Node("DecideDebrief", parent);
        Ui.Stretch(root);
        app.Paper(Theme.Page);
        var lesson = app.lesson;

        var title = Ui.Text(root, $"Debrief · Errand {lesson.id}", 48, Theme.Ink, true);
        Ui.Place(title.rectTransform, 80, 44, 1000, 60);

        // The checks: the class's choices against what each task needed.
        var tasks = Decide.Tasks;
        bool ruleWhereItWorked = Enumerable.Range(0, tasks.Length).Where(i => tasks[i].right == Decide.Automation).All(i => run.choices[i] == Decide.Automation);
        bool agentWhereNeeded = Enumerable.Range(0, tasks.Length).Where(i => tasks[i].right == Decide.Agent).All(i => run.choices[i] == Decide.Agent);
        bool neitherMarked = Enumerable.Range(0, tasks.Length).Where(i => tasks[i].right == Decide.Neither).All(i => run.choices[i] == Decide.Neither);
        var checks = new List<(bool ok, string label)>
        {
            (ruleWhereItWorked, "Chose a simple automation where one did the job"),
            (agentWhereNeeded, "Chose an agent where the task needed judgment"),
            (neitherMarked, "Marked the task for a person as neither"),
        };
        int ticked = checks.Count(c => c.ok);
        string result = ticked == checks.Count ? "complete" : ticked * 2 >= checks.Count ? "partial" : "incomplete";
        app.SetStars(lesson.number, ticked);
        app.StartCoroutine(Platform.ReportProgress($"{lesson.id}:{result}", ticked, string.Join("; ", checks.Select(c => $"{c.label}: {(c.ok ? "done" : "missed")}"))));

        long agentTokens = run.results.Count > 0 ? (long)run.results.Average(r => r.agent.TokensUsed) : 0;
        var measured = Ui.Text(root, $"The course says an agent costs about four times a chat. Measured here: the agents spent {agentTokens:N0} tokens a task on average; every fixed rule spent none.", 17, Theme.Text2);
        Ui.Place(measured.rectTransform, 80, 110, 1500, 30);

        // One row per task.
        float y = 156, h = 118;
        for (int i = 0; i < tasks.Length; i++)
        {
            var task = tasks[i];
            int chosen = run.choices[i];
            bool rightCall = chosen == task.right;
            var card = Ui.Card(root, "Row", rightCall ? Theme.Panel : Theme.DangerSoft, rightCall ? Theme.BorderTeal : Theme.Danger);
            Ui.Place(card.rectTransform, 80, y, 1760, h);
            var t = card.transform;
            var num = Ui.Text(t, (i + 1).ToString(), 30, rightCall ? Theme.Aqua : Theme.Danger, true, TextAlignmentOptions.Center);
            Ui.Place(num.rectTransform, 12, 34, 56, 50);
            var msg = Ui.Text(t, task.message, 16, Theme.Ink, true);
            Ui.Place(msg.rectTransform, 80, 14, 520, 60);
            var choice = Ui.Text(t, $"Your call: {Decide.Choices[chosen].ToLowerInvariant()}  ·  {(rightCall ? "right" : "better: " + Decide.Choices[task.right].ToLowerInvariant())}", 14, rightCall ? Theme.TealDeep : Theme.Danger, true);
            Ui.Place(choice.rectTransform, 80, 78, 520, 22);

            var r = run.results.FirstOrDefault(x => x.task == task);
            if (r.task != null)
            {
                Column(t, 620, "THE FIXED RULE", r.rule, true);
                Column(t, 960, "THE AGENT", r.agent, false);
            }
            else
            {
                var none = Ui.Text(t, "Not run: no agent and no rule should take this one.", 14, Theme.Muted);
                Ui.Place(none.rectTransform, 620, 40, 640, 40);
            }
            var why = Ui.Text(t, task.why, 13, Theme.Text2);
            Ui.Place(why.rectTransform, 1300, 14, 440, h - 24);
            y += h + 8;
        }

        // The checks, the takeaway, the question.
        float cy = y + 6;
        var checksCard = Ui.Card(root, "Checks", Theme.Panel, Theme.Border1);
        Ui.Place(checksCard.rectTransform, 80, cy, 520, 110);
        float ly = 12;
        foreach (var (ok, label) in checks)
        {
            var word = Ui.Text(checksCard.transform, ok ? "Done" : "Missed", 13, ok ? Theme.TealDeep : Theme.Danger, true);
            Ui.Place(word.rectTransform, 16, ly + 1, 56, 22);
            var lab = Ui.Text(checksCard.transform, label, 14, Theme.Ink);
            Ui.Place(lab.rectTransform, 76, ly, 440, 22);
            ly += 28;
        }
        app.luma.Say("recap");
        Luma.Card(root, 620, cy, 1220, 110, lesson.takeaway, 18, "Ask as the agent's manager: " + lesson.ask);

        var next = Ui.Button(root, $"Next errand: {Ladder.Get(lesson.number + 1).title}", app.NextLevel, Ui.ButtonStyle.Primary, 18);
        Ui.Place(next.GetComponent<RectTransform>(), 80, 990, 420, 56);
        var again = Ui.Button(root, "Decide again", app.ShowSetup, Ui.ButtonStyle.Secondary, 17);
        Ui.Place(again.GetComponent<RectTransform>(), 520, 990, 200, 56);
        var home = Ui.Button(root, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 17);
        Ui.Place(home.GetComponent<RectTransform>(), 740, 990, 180, 56);
        if (Platform.IsWeb)
        {
            var menu = Ui.Button(root, "Games menu", Platform.GoToMenu, Ui.ButtonStyle.Quiet, 17);
            Ui.Place(menu.GetComponent<RectTransform>(), 940, 990, 180, 56);
        }
        Ui.Emblem(root);
    }

    // What one kind did on a task: the basket, or sent home, and what it cost.
    static void Column(Transform t, float x, string head, AgentState a, bool rule)
    {
        var eyebrow = Ui.Eyebrow(t, head, rule ? Theme.Muted : Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, x, 14, 300, 20);
        string outcome = !a.alive ? $"Sent home on turn {a.eliminatedTurn}: bought {Items.Named(a.ateItem)}"
                       : a.outOfTokens ? $"Ran out of tokens with {a.score} in the basket"
                       : $"Came home with {a.score} wanted item{(a.score == 1 ? "" : "s")}";
        var line = Ui.Text(t, outcome, 15, a.alive ? Theme.Ink : Theme.Danger, true);
        Ui.Place(line.rectTransform, x, 38, 320, 44);
        var cost = Ui.Text(t, rule ? "0 tokens, $0: a fixed rule, no AI" : $"{a.TokensUsed:N0} tokens, ${a.CostUsd:0.0000} on {Models.All[a.config.model].label}", 13, Theme.Muted);
        Ui.Place(cost.rectTransform, x, 84, 320, 20);
    }
}
