using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The debrief's questions, answered on the world (the game-feel pass): the car back on its
// grid, the track beyond. The question is the objective line; the things to tap glow with a
// ring on the ground: the car, the track, its sensors' rays, its wheels, the points floating
// over the track, or signs beside the grid carrying a question's answers. A question that
// needs more words than the line holds stands on a board above the signs; a question that
// wants a line of text gets the one text field. LUMA says nothing until the answer, then
// "Right." or "Not quite." The answers are stored as the chips stored them.
public class QuizScreen
{
    public static QuizScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Session x;
    readonly List<Question> questions;
    readonly RectTransform frame;
    readonly List<Transform> signs = new List<Transform>();
    readonly TextMeshProUGUI objective;
    Transform board;
    Transform points;
    TMP_InputField input;
    Button done;
    int at;
    bool busy;
    string[] currentOptions; int currentRight;

    static readonly string[] Parts = { "The robot", "The track", "What it senses", "What it does", "The points" };
    static readonly string[] PartTargets = { "car", "track", "senses", "wheels", "points" };

    public static bool Fits(Lesson l) => l != null && l.quiz.Length > 0;
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
        app.world.ShowPlacing(frame);
        app.world.cam.Snap();

        objective = Hud.Objective(root, "");
        GameMenu.Icon(root);
        var runner = root.gameObject.AddComponent<Runner>();
        runner.Bind(this);
        runner.StartCoroutine(Ask());
    }

    Question Q => at < questions.Count ? questions[at] : null;

    // The question as a question, at most eight words, for the objective line. The prompt
    // itself stands on the board when the line cannot hold it.
    static string Short(Lesson l, Question q)
    {
        switch (q.key)
        {
            case "p1": return "Who makes the decisions?";
            case "p2": return "Where does it drive?";
            case "p3": return "What does it know right now?";
            case "p4": return "What lets it speed up, slow, turn?";
            case "p5": return "Which number says how good a move was?";
            case "start": return "What does the robot know at the start?";
            case "why": return l.number == 1 ? "Why did it never improve?" : "Why could it not learn both bends?";
            case "book": return "What would the robot do here?";
            case "learned": return "Has it learned the hairpin?";
            case "notsup": return "Why is this not supervised learning?";
            case "rise": return "How can the curve rise without a finish?";
            case "matched": return "What matched your prediction, what differed?";
            case "memo": return "What had the one-track robot learned?";
            case "measure": return "A wall touch costs a repair. Judge by?";
            case "lied": return "Why did finish rate lie?";
            case "knob": return "Which knob helped the most?";
            case "high": return "Which curve had the rewrite too high?";
            case "broke": return "What was different on the real track?";
            case "cost": return "A crash costs. Which measure matches?";
        }
        if (q.key.Length == 2 && q.key[0] == 's' && char.IsDigit(q.key[1])) return "Which kind of learning is this?";
        return q.prompt;
    }

    IEnumerator Ask()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        while (at < questions.Count)
        {
            var q = questions[at];
            Clear();
            busy = false;
            objective.text = Short(app.lesson, q);
            string prompt = q.prompt;
            string[] options = q.options;
            int right = q.answer;
            if (q.rulebook)
            {
                // The situation from the robot's own book; the five moves as signs.
                var book = x.Last?.book;
                var page = book != null ? book.pages.Where(p => book.Best(p.Key, x.Last.s.moves) >= 0 && p.Value.Any(v => v != 0)).Select(p => p.Key).FirstOrDefault() : null;
                if (page != null) { prompt = Driving.Words(Driving.Parse(page), x.Last.s.sensors); right = book.Best(page, x.Last.s.moves); }
                else { prompt = "The book is empty."; right = -2; }
                options = Moves.Names;
            }
            currentOptions = options; currentRight = right;
            if (IsParts(q))
            {
                app.world.ShowSensors(true);
                if (points == null) points = app.world.WorldPlate("+2", Vector3.zero, 2.6f, Theme.Teal);
                var t = app.world.track; var along = D(t.startHeading);
                points.position = app.world.Cell(t.start) + along * 1.6f + Vector3.up * 0.5f;
                points.gameObject.SetActive(true);
                app.world.candidatePoints = points.position;
                app.world.LightCandidates(true);
                // Close on the car and the stretch of track ahead of it.
                app.world.ShowPlacing(frame, app.world.Car.position - along * 0.6f, points.position, app.world.Cell(t.start) + along * 2.4f);
                app.world.cam.Snap();
            }
            else if (q.IsText)
            {
                app.world.ShowSensors(false);
                if (points != null) points.gameObject.SetActive(false);
                app.world.LightCandidates(false);
                Board(prompt);
                app.world.ShowPlacing(frame, BoardAt(), BoardAt() + Vector3.up * 2.2f, BoardAt() + Vector3.right * 1.3f, BoardAt() - Vector3.right * 1.3f);
                app.world.cam.Snap();
                input = Ui.DarkInput(root, "One line.", false, 18);
                Ui.Place(input.GetComponent<RectTransform>(), 40, 940, 900, 52);
                input.text = x.texts.TryGetValue(q.key, out var t0) ? t0 : "";
                done = Hud.Main(root, "Done", () => { if (input.text.Trim().Length >= 8) AnswerText(input.text); else Sfx.Ruin(); });
            }
            else
            {
                app.world.ShowSensors(false);
                if (points != null) points.gameObject.SetActive(false);
                app.world.LightCandidates(false);
                // The board carries what the line cannot: the rule book's situation, or a long prompt.
                if (q.rulebook || (Short(app.lesson, q) != q.prompt && Luma.Words(prompt) > 12)) Board(prompt);
                var post = SignAt(0);
                signs.AddRange(app.world.Signpost(options, post));
                float top = signs.Count > 0 ? signs[0].position.y - post.y + 0.4f : 2f;
                app.world.ShowSignpost(frame, post, top, board != null ? BoardAt() + Vector3.up * 1.5f : (Vector3?)null);
                app.world.GlowPlates(signs);
            }
            while (at < questions.Count && questions[at] == q) yield return null;
            yield return new WaitForSecondsRealtime(1.2f);
        }
        Clear();
        app.world.ShowSensors(false);
        app.world.LightCandidates(false);
        app.ShowResults();
    }

    static Vector3 D(int heading) { var d = Track.Dir(heading); return new Vector3(d.x, 0f, -d.y); }

    Vector3 SignAt(int i)
    {
        var t = app.world.track;
        var along = D(t.startHeading);
        int pit = app.world.PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4;
        var outward = D(pit);
        // The signpost stands on the pit lane beside the grid.
        return app.world.Cell(t.start) + outward * (0.5f + 1.2f * Circuit.Metre) + along * (0.9f + i * 1.7f);
    }

    Vector3 BoardAt()
    {
        var t = app.world.track;
        var along = D(t.startHeading);
        int pit = app.world.PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4;
        return app.world.Cell(t.start) + D(pit) * (0.5f + 4.8f * Circuit.Metre) + along * 2.2f;
    }

    void Board(string text) { board = app.world.Board(text, BoardAt()); }

    void Clear()
    {
        if (signs.Count > 0 && signs[0] != null && signs[0].parent != null) Object.Destroy(signs[0].parent.gameObject);
        foreach (var s in signs) if (s != null) Object.Destroy(s.gameObject); signs.Clear();
        if (board != null) { Object.Destroy(board.gameObject); board = null; }
        if (input != null) { Object.Destroy(input.gameObject); input = null; }
        if (done != null) { Object.Destroy(done.gameObject); done = null; }
        app.world.GlowPlates(null);
    }

    void Tap(Vector2 screen)
    {
        var q = Q;
        if (q == null || busy || q.IsText) return;
        int picked = -1;
        if (IsParts(q))
        {
            if (points != null && points.gameObject.activeSelf)
            {
                var s = app.world.ScreenOf(points.position);
                if (s.z > 0 && Vector2.Distance(screen, s) < 70f) picked = 4;
            }
            if (picked < 0) picked = System.Array.IndexOf(PartTargets, app.world.PartAt(screen));
        }
        else
        {
            float best = 90f;
            for (int i = 0; i < signs.Count; i++)
            {
                var s = app.world.ScreenOf(signs[i].position);
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
        bool right = q.rulebook ? index == currentRight : index == q.answer;
        x.picks[q.key] = q.rulebook ? (right ? 1 : 0) : index;
        if (IsParts(q)) app.world.FlashPart(PartTargets[Mathf.Clamp(index, 0, 4)], right);
        else if (index < signs.Count) app.world.FlashAt(signs[index].position, right);
        if (right) Sfx.Chime(); else Sfx.Ruin();
        // A question with no right answer (which knob helped: the runs decide) is neither.
        bool graded = q.rulebook ? currentRight >= 0 : q.answer >= 0;
        if (graded) app.luma.Cue(root, right ? "cue-right" : "cue-wrong");
        if (!right && graded && IsParts(q)) app.world.FlashPart(PartTargets[Mathf.Clamp(q.answer, 0, 4)], true);
        if (!right && graded && !IsParts(q) && currentRight >= 0 && currentRight < signs.Count) Lift(signs[currentRight]);
        at++;
    }

    void AnswerText(string text)
    {
        var q = Q;
        if (q == null || busy) return;
        busy = true;
        x.texts[q.key] = text;
        Sfx.Chime();
        at++;
    }

    void Lift(Transform sign) { if (sign != null) app.world.StartCoroutine(DoLift(sign)); }
    IEnumerator DoLift(Transform sign)
    {
        float t = 0;
        while (t < 0.8f && sign != null) { t += Time.unscaledDeltaTime; sign.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(t / 0.8f * Mathf.PI)); yield return null; }
        if (sign != null) sign.localScale = Vector3.one;
    }

    // The walk answers every question right (a text question with a line).
    public void AutoAnswer()
    {
        var q = Q; if (q == null) return;
        if (q.IsText) AnswerText("The robot was never shown a correct lap; it tried and got points.");
        else Answer(q.rulebook ? Mathf.Max(0, currentRight) : Mathf.Max(0, q.answer));
    }

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
