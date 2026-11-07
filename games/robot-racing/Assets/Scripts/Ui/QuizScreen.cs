using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The debrief's questions, answered on the world (the game-feel pass): the car back on its
// grid, the track beyond. LUMA asks one question at a time; the player taps the answer: the
// car, the track, its sensors' rays, its wheels, the points floating over the track, or one
// of three signs on the pit wall carrying a question's answers. The answers are stored as the
// chips stored them, so the checks and the report do not change. Questions this screen
// cannot put on the world (the rule book, a line of text) fall back to the old list.
public class QuizScreen
{
    public static QuizScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Session x;
    readonly List<Question> questions;
    readonly RectTransform frame;
    readonly List<Transform> signs = new List<Transform>();
    TextMeshPro points;
    int at;
    bool busy;

    static readonly string[] Parts = { "The robot", "The track", "What it senses", "What it does", "The points" };
    static readonly string[] PartTargets = { "car", "track", "senses", "wheels", "points" };

    // Can every question of the level be answered on the world?
    public static bool Fits(Lesson l) => l != null && l.quiz.Length > 0 && l.quiz.All(q => !q.IsText && !q.rulebook && (IsParts(q) || q.options.Length <= 3));
    static bool IsParts(Question q) => q.options != null && q.options.SequenceEqual(Parts);

    public QuizScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        x = app.session;
        var l = app.lesson;
        questions = l.quiz.ToList();
        root = Ui.Node("Quiz", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        app.world.Build(Track.Get(l.training[0]), false);
        app.world.SetSensors(x.settings.sensors);
        var t0 = app.world.track;
        app.world.ShowPlacing(frame, SignAt(0), SignAt(2), app.world.Cell(t0.start) + D(t0.startHeading) * 2.2f);
        app.world.cam.Snap();

        Hud.Objective(root, "Tap the answer.");
        GameMenu.Icon(root);
        var runner = root.gameObject.AddComponent<Runner>();
        runner.Bind(this);
        runner.StartCoroutine(Ask());
    }

    Question Q => at < questions.Count ? questions[at] : null;

    IEnumerator Ask()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        while (at < questions.Count)
        {
            var q = questions[at];
            ClearSigns();
            busy = false;
            string id = $"q-{app.lesson.number}-{q.key}";
            if (Luma.Lines.ContainsKey(id)) app.luma.Cue(root, id, 40, 960, 820, stay: true);
            else app.luma.Line(root, 40, 960, 820, q.prompt, true, true, 18, type: false);
            if (IsParts(q))
            {
                app.world.ShowSensors(true);
                app.world.LightCandidates(true);
                if (points == null) points = app.world.WorldLabel("+2", app.world.Cell(app.world.track.start) + Vector3.up * 0.55f + Vector3.right * 0f, 3f, Theme.Teal);
                var t = app.world.track; var along = D(t.startHeading);
                points.transform.position = app.world.Cell(t.start) + along * 1.6f + Vector3.up * 0.5f;
                points.gameObject.SetActive(true);
                app.world.candidatePoints = points.transform.position;
            }
            else
            {
                app.world.ShowSensors(false);
                app.world.LightCandidates(false);
                if (points != null) points.gameObject.SetActive(false);
                Signs(q);
            }
            while (at < questions.Count && questions[at] == q) yield return null;
            yield return new WaitForSecondsRealtime(0.9f);
        }
        ClearSigns();
        app.world.ShowSensors(false);
        app.world.LightCandidates(false);
        app.ShowResults();
    }

    static Vector3 D(int heading) { var d = Track.Dir(heading); return new Vector3(d.x, 0f, -d.y); }

    // Three signs in a row beside the grid, on the pit side, one per answer.
    Vector3 SignAt(int i)
    {
        var t = app.world.track;
        var along = D(t.startHeading);
        int pit = app.world.PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4;
        var outward = D(pit);
        return app.world.Cell(t.start) + outward * (0.5f + 1.1f * Circuit.Metre) + along * (0.2f + i * 1.7f);
    }

    void Signs(Question q)
    {
        for (int i = 0; i < q.options.Length; i++) signs.Add(app.world.Sign(q.options[i], SignAt(i)));
    }

    void ClearSigns() { foreach (var s in signs) if (s != null) Object.Destroy(s.gameObject); signs.Clear(); }

    // A tap: which answer is under it, if any.
    void Tap(Vector2 screen)
    {
        var q = Q;
        if (q == null || busy) return;
        int picked = -1;
        if (IsParts(q))
        {
            if (points != null && points.gameObject.activeSelf)
            {
                var s = app.world.ScreenOf(points.transform.position);
                if (s.z > 0 && Vector2.Distance(screen, s) < 70f) picked = 4;
            }
            if (picked < 0)
            {
                string part = app.world.PartAt(screen);
                picked = System.Array.IndexOf(PartTargets, part);
            }
        }
        else
        {
            float best = 90f;
            for (int i = 0; i < signs.Count; i++)
            {
                var s = app.world.ScreenOf(signs[i].position + Vector3.up * 1.0f);
                if (s.z > 0 && Vector2.Distance(screen, s) < best) { best = Vector2.Distance(screen, s); picked = i; }
            }
        }
        if (picked < 0) return;
        Answer(picked);
    }

    public void Answer(int index)
    {
        var q = Q;
        if (q == null || busy) return;
        busy = true;
        x.picks[q.key] = index;
        bool right = index == q.answer;
        if (IsParts(q)) app.world.FlashPart(PartTargets[Mathf.Clamp(index, 0, 4)], right);
        else if (index < signs.Count) app.world.FlashPart("car", right);
        if (right) Sfx.Chime(); else Sfx.Ruin();
        if (!right && IsParts(q)) app.world.FlashPart(PartTargets[Mathf.Clamp(q.answer, 0, 4)], true);
        if (!right && !IsParts(q) && q.answer >= 0 && q.answer < signs.Count) Lift(signs[q.answer]);
        at++;
    }

    void Lift(Transform sign) { if (sign != null) app.world.StartCoroutine(DoLift(sign)); }
    IEnumerator DoLift(Transform sign)
    {
        float t = 0;
        while (t < 0.8f && sign != null) { t += Time.unscaledDeltaTime; sign.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(t / 0.8f * Mathf.PI)); yield return null; }
        if (sign != null) sign.localScale = Vector3.one;
    }

    // The walk answers every question right.
    public void AutoAnswer() { var q = Q; if (q != null) Answer(q.answer); }

    class Runner : MonoBehaviour
    {
        QuizScreen screen;
        public void Bind(QuizScreen s) { screen = s; }
        void Update()
        {
            if (screen == null || GameMenu.Paused) return;
            if (Input.GetMouseButtonDown(0) && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) screen.Tap(Input.mousePosition);
        }
        void OnDestroy() { if (Current == screen) Current = null; }
    }
}
