using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Errand 1.6: five family messages. For each one the class marks what it needs:
// an agent, a simple automation (a fixed rule, no AI), or neither. Then every
// shopping task runs with both kinds on the same market, side by side.
public class DecideScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly int[] choices;
    readonly Button run;
    readonly RectTransform rows;

    public DecideScreen(Transform parent, App app)
    {
        this.app = app;
        root = Ui.Node("Decide", parent);
        Ui.Stretch(root);
        app.Paper(Theme.Page);
        var l = app.lesson;
        choices = Enumerable.Repeat(-1, Decide.Tasks.Length).ToArray();

        var eyebrow = Ui.Eyebrow(root, $"ERRAND {l.id}  ·  DECIDE, TASK BY TASK", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 80, 40, 600, 22);
        var title = Ui.Text(root, l.title, 40, Theme.Ink, true);
        Ui.Place(title.rectTransform, 80, 62, 1100, 52);
        var back = Ui.Button(root, "Briefing", app.ShowLesson, Ui.ButtonStyle.Quiet, 16);
        Ui.Place(back.GetComponent<RectTransform>(), 1200, 64, 110, 44);
        run = Ui.Button(root, "RUN THE SHOPPING TASKS", () => app.StartDecide(choices), Ui.ButtonStyle.Accent, 18);
        Ui.Place(run.GetComponent<RectTransform>(), 1520, 54, 340, 62);
        run.interactable = false;

        Luma.Card(root, 80, 130, 1780, 96,
            "Mark each task: an agent, a simple automation, or neither. Then the shopping tasks run with both kinds on the same market, " +
            "a fixed rule against an agent, so you can see the cost and the result side by side. The rule for each task was written in advance; it takes exactly the things it names.", 16);

        rows = Ui.Node("Rows", root);
        Ui.Place(rows, 80, 244, 1780, 700);
        Build();

        var home = Ui.Button(root, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 16);
        Ui.Place(home.GetComponent<RectTransform>(), 80, 980, 160, 48);
        Ui.Emblem(root);
    }

    void Build()
    {
        Ui.Clear(rows);
        float y = 0;
        for (int i = 0; i < Decide.Tasks.Length; i++)
        {
            var task = Decide.Tasks[i];
            int index = i;
            var card = Ui.Card(rows, "Task", Theme.Panel, choices[i] >= 0 ? Theme.BorderTeal : Theme.Border1);
            Ui.Place(card.rectTransform, 0, y, 1780, 126);
            var t = card.transform;
            var num = Ui.Text(t, (i + 1).ToString(), 34, Theme.Muted, true, TextAlignmentOptions.Center);
            Ui.Place(num.rectTransform, 14, 36, 60, 50);
            var from = Ui.Eyebrow(t, $"A MESSAGE FROM {task.from.ToUpperInvariant()}", Theme.TealDeep);
            Ui.Place(from.rectTransform, 90, 18, 400, 20);
            var msg = Ui.Text(t, task.message, 20, Theme.Ink);
            Ui.Place(msg.rectTransform, 90, 42, 1000, 56);
            var note = Ui.Text(t, task.note, 13, Theme.Text2);
            Ui.Place(note.rectTransform, 90, 96, 1000, 20);
            for (int c = 0; c < Decide.Choices.Length; c++)
            {
                int choice = c;
                bool on = choices[i] == c;
                var b = Ui.Button(t, Decide.Choices[c], () => { choices[index] = choice; Build(); }, on ? Ui.ButtonStyle.Primary : Ui.ButtonStyle.Secondary, 14);
                Ui.Place(b.GetComponent<RectTransform>(), 1120 + c * 216, 40, 204, 46);
            }
            y += 136;
        }
        run.interactable = choices.All(c => c >= 0);
    }
}
