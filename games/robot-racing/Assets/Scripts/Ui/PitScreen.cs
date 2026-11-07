using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The pit lane: the car on its grid, the track beyond, and the rewards as things to place
// on it (the game-feel pass). On 1.1 the chequered flag alone leans on the pit wall; from
// 2.3 the shelf holds the cones, the chevrons, the clock post and the speed sign too. Drag
// one onto the track, tap it to turn its number. One line says what to do; the menu holds
// the rest. The robot runs by itself: Train is the last word before it does.
public class PitScreen
{
    public static PitScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Settings s;
    readonly RectTransform frame;
    TMP_InputField prediction;
    TextMeshProUGUI note, objective;
    const string SeenRewards = "robot_racing_seen_rewards";

    public PitScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        s = app.settings;
        var l = app.lesson;
        root = Ui.Node("Pit", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        app.world.Build(Track.Get(s.training[0]), false);
        app.world.SetSensors(s.sensors);

        // The rewards this level has: the flag alone on 1.1, the whole shelf when the points are open.
        var kinds = new List<string>();
        bool free = l == null;
        if (free || l.points) kinds.AddRange(new[] { "finish", "crash", "toward", "away", "moment", "bonus" });
        else if (l.flag) kinds.Add("finish");
        app.world.BuildMarkers(s, kinds, Changed);

        objective = Hud.Objective(root, Objective(l, s));
        GameMenu.Icon(root);
        note = Ui.Text(root, "", 16, Theme.Danger, true);
        Ui.Place(note.rectTransform, Ui.W - 40 - 520, 964, 520, 24);
        note.alignment = TextAlignmentOptions.TopRight;
        if (l != null && l.prediction)
        {
            // The level asks for a line before the run: the one text field of this game.
            prediction = Ui.DarkInput(root, l.predictionPrompt, true, 16);
            Ui.Place(prediction.GetComponent<RectTransform>(), 40, 880, 620, 84);
            prediction.text = app.session.prediction ?? "";
        }
        Hud.Main(root, "Train", Train);

        var runner = root.gameObject.AddComponent<Runner>();
        runner.StartCoroutine(Open(l));
    }

    // The level's one line of what to do here, at most eight words. On 1.1 it names the one
    // act until it is done.
    static string Objective(Lesson l, Settings s)
    {
        if (l == null) return "Set up the robot.";
        return l.number switch
        {
            1 => s.finish > 0 ? "Get it to the finish." : "Put the flag on the finish line.",
            6 => "Light the path to the finish.",
            _ => l.points ? "Set the points. Then train." : "Set it up. Then train.",
        };
    }

    // The car first, close; then the camera pulls back to the track and the shelf. A level
    // that has just opened its rewards shows them arriving; a run that found no finish lights
    // what to place.
    IEnumerator Open(Lesson l)
    {
        app.world.ShowPortrait(frame);
        app.world.FreezeMarkers(true);
        yield return new WaitForSecondsRealtime(l != null && l.number == 6 && PlayerPrefs.GetInt(SeenRewards, 0) == 0 ? 1.4f : 1.8f);
        if (root == null) yield break;
        app.world.ShowPlacing(frame);
        yield return new WaitForSecondsRealtime(0.9f);
        if (root == null) yield break;
        app.world.FreezeMarkers(false);
        bool trained = app.session != null && app.session.runs.Count > 0;
        bool noFinish = trained && app.session.Last.firstFinish < 0;
        if (l != null && l.number == 6 && PlayerPrefs.GetInt(SeenRewards, 0) == 0)
        {
            PlayerPrefs.SetInt(SeenRewards, 1); PlayerPrefs.Save();
            yield return Moment("NEW: REWARDS", Theme.Teal);
            app.luma.Cue(root, "cue-dark");
        }
        else if (l != null && l.number == 1 && noFinish)
        {
            app.luma.Cue(root, "cue-flag");
            app.world.Beckon("finish", true);
        }
        else if (l != null && l.flag && !l.points && s.finish <= 0) app.world.Beckon("finish", true); // the one thing to do here
        else if (l != null && l.number == 6 && noFinish)
        {
            app.luma.Cue(root, "cue-path");
            app.world.Beckon("toward", true);
        }
    }

    void Changed()
    {
        app.world.Beckon("finish", false);
        app.world.Beckon("toward", false);
        if (note != null) note.text = "";
        if (objective != null && app.lesson != null) objective.text = Objective(app.lesson, s);
        // On 1.1 the flag alone is the player's: it turns the points on as a whole. Everything
        // the level has not opened returns to the ready robot's values (a crash costs, each
        // cell closer pays a little); off the finish, every move earns zero again.
        var l = app.lesson;
        if (l != null && l.flag && !l.points)
        {
            if (s.finish > 0) { var f = Lesson.Fixed; s.crash = f.crash; s.toward = f.toward; s.away = f.away; s.moment = f.moment; }
            else s.crash = s.toward = s.away = s.moment = 0f;
        }
    }

    void Train()
    {
        if (app.lesson != null && app.lesson.prediction && (prediction == null || prediction.text.Trim().Length < 8))
        {
            note.text = "Write your prediction first.";
            return;
        }
        if (prediction != null) app.session.prediction = prediction.text;
        app.world.FreezeMarkers(true);
        app.StartTraining();
    }

    // A banner over the world: two words, in with an overshoot, gone.
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
