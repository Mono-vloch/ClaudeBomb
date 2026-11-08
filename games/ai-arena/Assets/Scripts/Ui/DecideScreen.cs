using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Errand 1.6 (the game-feel pass): the kitchen table at home. Five family messages in a pile;
// the top one is readable. Three places to drop it: the agent standing by the table, the
// rule box on the counter (a simple automation), the bin (neither). A dropped note can be
// picked up again. Then every shopping task runs with both kinds on the same market. The
// choices are stored as the buttons stored them; the hints went to the file.
public class DecideScreen
{
    public static DecideScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly int[] choices;
    readonly Button run;

    public DecideScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        root = Ui.Node("Decide", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        choices = Enumerable.Repeat(-1, Decide.Tasks.Length).ToArray();

        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        var stage = new Arena(app.brief, app.configs, app.budget, 61);
        var a = stage.agents[0];
        var inward = Market.Inward(a.home);
        var step = new Vector2Int(Mathf.RoundToInt(inward.x), -Mathf.RoundToInt(inward.z));
        var side = new Vector2Int(step.y, step.x);
        for (int d = 0; d <= 2; d++) for (int s = -1; s <= 2; s++) { var p = a.home + step * d + side * s; if (Arena.InBounds(p)) stage.RemoveItem(p); }
        app.world.Build(stage, false);
        app.world.BuildKitchen(a, Decide.Tasks.Select(t => t.message).ToArray(), choices);
        app.world.ShowGarage(frame);
        app.world.NotePlaced += OnPlaced;
        app.atmosphere.MenuBlur(false);

        Hud.Objective(root, "Who does each task?");
        GameMenu.Icon(root);
        run = Hud.Main(root, "Run them", () => app.StartDecide(choices));
        run.interactable = false;
        app.luma.Cue(root, "decide");
        var runner = root.gameObject.AddComponent<Runner>();
        runner.screen = this;
    }

    void OnPlaced(int task, int choice)
    {
        choices[task] = choice;
        app.world.RefreshKitchen(true);
        run.interactable = choices.All(c => c >= 0);
    }

    // For the screenshot walk: a note dropped in a place.
    public void Place(int task, int choice) => OnPlaced(task, choice);
    public bool Ready => choices.All(c => c >= 0);

    class Runner : MonoBehaviour
    {
        public DecideScreen screen;
        void OnDestroy() { if (screen != null) { screen.app.world.NotePlaced -= screen.OnPlaced; if (Current == screen) Current = null; } }
    }
}
