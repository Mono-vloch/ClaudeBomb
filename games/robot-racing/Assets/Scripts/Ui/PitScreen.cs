using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The pit lane: the car on its grid, the track beyond, and everything the level opens as a
// thing in the world (the game-feel pass): the rewards on the shelf to place on the track, the
// dials to turn, the parts on the car to fit or remove, the track cards on the board. One line
// says what to do; the fuel gauge takes the player's guess for the first finish; the menu
// holds the rest. The robot runs by itself: Train is the last word before it does.
public class PitScreen
{
    public static PitScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Settings s;
    readonly RectTransform frame;
    readonly Lesson l;
    TMP_InputField prediction;
    TextMeshProUGUI note, objective;
    Hud.Gauge gauge;
    const string SeenRewards = "robot_racing_seen_";

    public PitScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        s = app.settings;
        l = app.lesson;
        root = Ui.Node("Pit", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        app.world.Build(Track.Get(s.training[0]), false);
        app.world.SetSensors(s.sensors);
        if (l == null || l.sensors) app.world.ShowSensors(true); // the beams, long, while the sensors are the thing to set

        var (kinds, fixedKinds) = Kinds(l, s);
        app.world.BuildMarkers(s, kinds, Changed, fixedKinds, nameParts: l == null || !(l.sensors && l.moves), litKinds: Lit(l));
        if (l == null || l.sensors) app.world.ShowBendRay(true);
        app.world.ShowDimMarkers(s.finish > 0);

        objective = Hud.Objective(root, Objective(l, s));
        GameMenu.Icon(root);
        // The fuel gauge, full, with the guess for the first finish as a notch to drag.
        gauge = new Hud.Gauge(root, Ui.W - 40 - 52 - 24 - 92, 20);
        gauge.Guess = app.session != null && app.session.guess >= 0 ? app.session.guess : 0.4f;
        gauge.Draggable = true;
        gauge.OnGuess = g => { if (app.session != null) app.session.guess = g; };
        if (app.session != null && app.session.guess < 0) app.session.guess = gauge.Guess;
        note = Ui.Text(root, "", 16, Theme.Danger, true);
        Ui.Place(note.rectTransform, Ui.W - 40 - 520, 964, 520, 24);
        note.alignment = TextAlignmentOptions.TopRight;
        if (l != null && l.prediction)
        {
            prediction = Ui.DarkInput(root, l.predictionPrompt, true, 16);
            Ui.Place(prediction.GetComponent<RectTransform>(), Ui.W - 40 - 620, 856, 620, 84); // on the right, over the Train button, clear of the console and LUMA's line
            prediction.text = app.session.prediction ?? "";
        }
        Hud.Main(root, "Train", Train);

        var runner = root.gameObject.AddComponent<Runner>();
        runner.StartCoroutine(Open());
    }

    // What this level puts in the world, and what race control keeps dim.
    public static (List<string> kinds, List<string> fixedKinds) Kinds(Lesson l, Settings s)
    {
        var kinds = new List<string>(); var fixedKinds = new List<string>();
        bool free = l == null;
        if (free || l.points) kinds.AddRange(Circuit.Rewards);
        else if (l.flag) { kinds.Add("finish"); fixedKinds.AddRange(new[] { "crash", "toward", "away", "moment" }); }
        if (free || l.attempts || l.knobs) { kinds.Add("fuel"); if (free || l.number == 2 || s.stopAtFirstFinish) kinds.Add("stopcap"); }
        if (free || l.explore) { kinds.Add("die"); if (free || l.number == 8 || s.exploreOffAfterFinish) kinds.Add("nevercap"); kinds.Add("fall"); kinds.Add("floor"); }
        if (free || l.knobs) { kinds.Add("rewrite"); kinds.Add("seed"); }
        if (free || l.measure) kinds.Add("measure");
        if (free || l.sensors) for (int i = 0; i < Sensors.Count; i++) kinds.Add("sense" + i);
        if (free || l.moves) { for (int i = 0; i < Moves.Count; i++) kinds.Add("move" + i); kinds.Add("limit"); }
        if (free || l.tracks) foreach (var t in Track.All) kinds.Add("track:" + t.id);
        return (kinds, fixedKinds);
    }

    // The knobs the level is about, lit on the desk; the rest stand dim. Null: all of them.
    static IEnumerable<string> Lit(Lesson l) => l == null ? null : l.number switch
    {
        2 => new[] { "fuel", "stopcap" }, 8 => new[] { "die", "nevercap", "fall", "floor" }, 10 => new string[0], 11 => new[] { "measure" }, 12 => new[] { "rewrite", "die", "fuel" }, 13 => new[] { "fuel" }, _ => null,
    };

    // The level's one line of what to do here, at most eight words.
    static string Objective(Lesson l, Settings s)
    {
        if (l == null) return "Set up the robot.";
        return l.number switch
        {
            1 => s.finish > 0 ? "Get it to the finish." : "Put the flag on the finish line.",
            2 => "Fill the tank. Train until it settles.",
            3 => "Train it on the hairpin.",
            4 => "Fit the sensors it needs.",
            5 => "Give it the moves it needs.",
            6 => "Light the path to the finish.",
            7 => "Close the loophole in the points.",
            8 => "Set how often it tries something new.",
            9 => "Set it up. Then race.",
            10 => "Pick the tracks. One is the test.",
            11 => "Pick the measure to judge by.",
            12 => "Change one knob. Keep the seed.",
            13 => "Train within the budget. Then the real run.",
            _ => "Set it up. Then train.",
        };
    }

    // The car first, close; then the camera pulls back to what the level opens. A level that
    // has just opened something shows it arriving; a run that failed lights what to fix.
    IEnumerator Open()
    {
        app.world.ShowPortrait(frame);
        app.world.FreezeMarkers(true);
        string seen = SeenRewards + (l != null ? l.number : 0);
        bool first = l != null && PlayerPrefs.GetInt(seen, 0) == 0;
        yield return new WaitForSecondsRealtime(first ? 1.4f : 1.8f);
        if (root == null) yield break;
        app.world.ShowPlacing(frame);
        yield return new WaitForSecondsRealtime(0.9f);
        if (root == null) yield break;
        app.world.FreezeMarkers(false);
        bool trained = app.session != null && app.session.runs.Count > 0;
        bool failed = trained && (app.session.Last.firstFinish < 0 || !app.session.RaceOk(app.session.LastRace));
        if (l == null) yield break;
        if (first)
        {
            PlayerPrefs.SetInt(seen, 1); PlayerPrefs.Save();
            string arrival = Arrival(l);
            if (arrival != null) yield return Moment(arrival, Theme.Teal);
            if (l.number == 6) app.luma.Cue(root, "cue-dark");
            else if (Luma.Lines.ContainsKey("pit-" + l.number)) app.luma.Cue(root, "pit-" + l.number);
        }
        else if (failed)
        {
            if (Luma.Lines.ContainsKey("why-" + l.number)) { app.luma.Cue(root, "why-" + l.number); yield return new WaitForSecondsRealtime(3.5f); if (root == null) yield break; }
            if (Luma.Lines.ContainsKey("fix-" + l.number)) app.luma.Cue(root, "fix-" + l.number);
            app.world.Beckon(Hint(l), true);
        }
        if (l.flag && !l.points && s.finish <= 0) app.world.Beckon("finish", true);
    }

    // What arrives on a level that opens a part, as two words over the world.
    static string Arrival(Lesson l) => l.number switch
    {
        2 => "NEW: THE TANK", 4 => "NEW: SENSORS", 5 => "NEW: MOVES", 6 => "NEW: REWARDS", 8 => "NEW: NEW MOVES", 10 => "NEW: TRACKS", 11 => "NEW: THE MEASURE", 12 => "NEW: KNOBS", _ => null,
    };

    // The marker to light after a failed run: the level's planned mistake.
    static string Hint(Lesson l) => l.number switch
    {
        1 => "finish", 2 => "stopcap", 4 => "sense1", 5 => "move2", 6 => "toward", 7 => "bonus", 8 => "nevercap", 10 => "track:bend", 11 => "measure", 12 => "rewrite", _ => "fuel",
    };

    void Changed()
    {
        app.world.Beckon("", false);
        if (note != null) note.text = "";
        if (objective != null && l != null) objective.text = Objective(l, s);
        // On 1.1 the flag alone is the player's: it turns the points on as a whole. Race
        // control's points show, dim, once it is on the finish, and go when it comes off.
        if (l != null && l.flag && !l.points)
        {
            if (s.finish > 0) { var f = Lesson.Fixed; s.crash = f.crash; s.toward = f.toward; s.away = f.away; s.moment = f.moment; }
            else s.crash = s.toward = s.away = s.moment = 0f;
            app.world.ShowDimMarkers(s.finish > 0);
        }
    }

    void Train()
    {
        if (l != null && l.prediction && (prediction == null || prediction.text.Trim().Length < 8))
        {
            note.text = "Write your prediction first.";
            return;
        }
        if (prediction != null) app.session.prediction = prediction.text;
        if (app.session != null) app.session.guess = gauge.Guess;
        app.world.FreezeMarkers(true);
        app.StartTraining();
    }

    IEnumerator Moment(string text, Color colour)
    {
        var card = Ui.Box(root, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        Ui.Place(card.rectTransform, (Ui.W - 640) / 2, 380, 640, 120);
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", colour, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, 120);
        var t = Ui.Text(card.transform, text, 48, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = 6;
        Ui.Stretch(t.rectTransform, 20, 0, 20, 0);
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(Ui.W / 2, -440);
        Sfx.Sting(Sfx.Mood.Fanfare);
        yield return Ui.Animate(0.34f, k => { group.alpha = Mathf.Min(1f, k * 3f); card.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, Ui.EaseOutBack(k)); });
        yield return new WaitForSecondsRealtime(1.4f);
        yield return Ui.Animate(0.22f, k => { group.alpha = 1 - k; });
        Object.Destroy(card.gameObject);
    }

    class Runner : MonoBehaviour { void OnDestroy() { if (Current != null && Current.root == null) Current = null; } }
}
