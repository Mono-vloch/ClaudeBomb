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
        app.Paper(Color.clear);
        var l = app.lesson;
        // The market the tasks would run in, turning slowly and sharp, the tasks in a dark
        // glass drawer on the right (660 of the 1920 units, a third): the question is which
        // of them needs an agent in it.
        var window = Ui.Node("Window", root);
        Ui.Place(window, 0, 0, Ui.W - DrawerW, Ui.H);
        app.world.Build(new Arena(app.brief, app.configs.ConvertAll(c => c.Clone()), app.budget, 61), false);
        if (Market.Real) app.world.ShowOrbitOf(window); else app.world.ShowMarket(window);
        app.atmosphere.MenuBlur(false);
        var drawer = Ui.Node("Drawer", root);
        Ui.Place(drawer, Ui.W - DrawerW, 0, DrawerW, Ui.H);
        var glass = Ui.Box(drawer, "Glass", Theme.Panel, false);
        Ui.Stretch(glass.rectTransform);
        var edge = Ui.Box(drawer, "Edge", new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.5f), false);
        Ui.Place(edge.rectTransform, 0, 0, 2, Ui.H);
        edge.raycastTarget = false;
        choices = Enumerable.Repeat(-1, Decide.Tasks.Length).ToArray();

        var eyebrow = Ui.Eyebrow(drawer, $"ERRAND {l.id}  ·  DECIDE, TASK BY TASK", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 26, 22, W, 22);
        var title = Ui.Text(drawer, l.title, 30, Theme.Ink, true);
        Ui.Place(title.rectTransform, 26, 46, W, 44);
        title.enableWordWrapping = false; title.overflowMode = TextOverflowModes.Ellipsis;
        var back = Ui.Button(drawer, "Briefing", app.ShowLesson, Ui.ButtonStyle.Secondary, 16);
        Ui.Place(back.GetComponent<RectTransform>(), 26, 98, 120, 40);
        var file = Ui.Button(drawer, "The file", () => app.ShowFile(("HOW THE TASKS RUN",
            "Then the shopping tasks run with both kinds on the same market, a fixed rule against an agent, so you can see the cost and the result side by side. " +
            "The rule for each task was written in advance; it takes exactly the things it names.")), Ui.ButtonStyle.Secondary, 16);
        Ui.Place(file.GetComponent<RectTransform>(), 156, 98, 120, 40);
        var home = Ui.Button(drawer, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 16);
        Ui.Place(home.GetComponent<RectTransform>(), 286, 98, 150, 40);
        run = Ui.Button(drawer, "RUN THE SHOPPING TASKS", () => app.StartDecide(choices), Ui.ButtonStyle.Accent, 20);
        Ui.Place(run.GetComponent<RectTransform>(), 26, 1006, W, 56);
        run.interactable = false;

        app.luma.Line(root, 40, 960, 820, "Mark each task: an agent, a simple automation, or neither.", false, true, 17);

        rows = Ui.Node("Rows", drawer);
        Ui.Place(rows, 26, 154, W, 840);
        Build();
    }

    const float DrawerW = 660f, W = 608f;

    void Build()
    {
        Ui.Clear(rows);
        float y = 0;
        for (int i = 0; i < Decide.Tasks.Length; i++)
        {
            var task = Decide.Tasks[i];
            int index = i;
            var card = Ui.Card(rows, "Task", Theme.Panel, choices[i] >= 0 ? Theme.BorderTeal : Theme.Border1);
            var t = card.transform;
            var from = Ui.Eyebrow(t, $"{i + 1}  ·  A MESSAGE FROM {task.from.ToUpperInvariant()}", Theme.TealDeep);
            Ui.Place(from.rectTransform, 16, 10, W - 32, 20);
            var msg = Ui.Text(t, task.message, 17, Theme.Ink);
            float mh = msg.GetPreferredValues(task.message, W - 32, 0).y;
            Ui.Place(msg.rectTransform, 16, 34, W - 32, mh);
            var note = Ui.Text(t, task.note, 16, Theme.Text2);
            Ui.Place(note.rectTransform, 16, 36 + mh, W - 32, 22);
            note.enableWordWrapping = false; note.overflowMode = TextOverflowModes.Ellipsis;
            float by = 64 + mh;
            for (int c = 0; c < Decide.Choices.Length; c++)
            {
                int choice = c;
                bool on = choices[i] == c;
                var b = Ui.Button(t, Decide.Choices[c], () => { choices[index] = choice; Build(); }, on ? Ui.ButtonStyle.Primary : Ui.ButtonStyle.Secondary, 16);
                Ui.Place(b.GetComponent<RectTransform>(), 16 + c * ((W - 32 - 16) / 3f + 8), by, (W - 32 - 16) / 3f, 38);
            }
            float h = by + 38 + 10;
            Ui.Place(card.rectTransform, 0, y, W, h);
            y += h + 8;
        }
        run.interactable = choices.All(c => c >= 0);
    }
}
