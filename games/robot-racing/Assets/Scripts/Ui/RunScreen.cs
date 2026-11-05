using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The run: the robot trains on the track in three dimensions on the left, the
// curve of points per attempt underneath, and on the right what the student must
// always see: the attempt count, each sensor's reading, the move it just made and
// what it earned, the chance of a new move, and the rule book it is writing.
// Then the race on the test track, and for the capstone one real run. Training
// runs many attempts a second with a visible speed control.
public class RunScreen
{
    public static RunScreen Current;
    public enum Phase { Training, Racing, Real, Done }

    public readonly RectTransform root;
    readonly App app;
    readonly List<(Settings s, string label)> queue;
    int which;
    Training training;
    Race race;
    public Phase phase = Phase.Training;
    readonly Chart chart;
    readonly RectTransform labelsLayer, bannerLayer, bookContent;
    readonly TextMeshProUGUI turnText, phaseText, pointsText, exploreText, moveText, bookCount, coach, missionText;
    readonly TextMeshProUGUI[] sensorText = new TextMeshProUGUI[Sensors.Count];
    readonly (Image track, Image fill)[] sensorBar = new (Image, Image)[Sensors.Count];
    readonly Button pause, results, stop;
    readonly Ui.Segmented speed;
    readonly Follower follower;
    readonly Queue<(string text, Color colour, float seconds)> banners = new Queue<(string, Color, float)>();
    int speedIndex = 2;
    bool paused, saidFirst, saidSettled, saidLoophole;
    float acc, bookTimer, chartTimer;
    string shownKey = "";
    static readonly float[] Interval = { 1f / 5, 1f / 40, 1f / 400, 0f };

    public RunScreen(Transform parent, App app, List<(Settings s, string label)> runs)
    {
        Current = this;
        this.app = app;
        queue = runs;
        var l = app.lesson;
        root = Ui.Node("Run", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 40, 104, 940, 600);
        StartRun(frame);

        var column = Ui.Box(root, "Column", new Color(1, 1, 1, 0.84f), false);
        Ui.Place(column.rectTransform, 1000, -300, 1600, 1700);
        column.raycastTarget = false;
        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.8f));
        Ui.Place(top.rectTransform, 40, 16, 950, 82);
        top.raycastTarget = false;

        turnText = Ui.Text(root, "", 28, Theme.Ink, true);
        Ui.Place(turnText.rectTransform, 60, 24, 370, 40);
        turnText.enableWordWrapping = false;
        phaseText = Ui.Text(root, "", 15, Theme.Text2);
        Ui.Place(phaseText.rectTransform, 60, 62, 370, 26);
        var speedLabel = Ui.Text(root, "Speed", 14, Theme.Text2, false, TextAlignmentOptions.MidlineRight);
        Ui.Place(speedLabel.rectTransform, 430, 36, 50, 44);
        speed = new Ui.Segmented(root, new[] { "1x", "10x", "100x", "Max" }, speedIndex, 292, 44, 13);
        Ui.Place(speed.root, 486, 36, 292, 44);
        speed.OnSelect = i => speedIndex = i;
        pause = Ui.Button(root, "Pause", () => { paused = !paused; Ui.SetLabel(pause, paused ? "Resume" : "Pause"); }, Ui.ButtonStyle.Secondary, 14);
        Ui.Place(pause.GetComponent<RectTransform>(), 786, 36, 86, 44);
        stop = Ui.Button(root, "Stop, race now", () => { if (phase == Phase.Training) { training.done = true; } }, Ui.ButtonStyle.Quiet, 14);
        Ui.Place(stop.GetComponent<RectTransform>(), 876, 36, 110, 44);

        var viewToggle = new Ui.Segmented(root, new[] { "The track", "Follow" }, 0, 260, 40, 14);
        Ui.Place(viewToggle.root, 40, 1032, 260, 40);
        viewToggle.OnSelect = i => { if (i == 0) app.world.ShowOverviewAgain(); else app.world.ShowFollow(frame); };

        chart = new Chart(root, 40, 716, 940, 304, "POINTS PER ATTEMPT  ·  THE CURVE");
        labelsLayer = Ui.Node("Labels", root);
        Ui.Stretch(labelsLayer);
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);

        // The mission, at the top of the column.
        var mission = Ui.Soft(root, "Mission");
        Ui.Place(mission.rectTransform, 1020, 28, 840, 58);
        var missionTag = Ui.Eyebrow(mission.transform, l != null ? $"LEVEL {l.id}  ·  {app.StepName.ToUpperInvariant()}" : "FREE PLAY", Theme.TealDeep);
        Ui.Place(missionTag.rectTransform, 16, 8, 800, 18);
        missionText = Ui.Text(mission.transform, "", 15, Theme.Ink);
        Ui.Place(missionText.rectTransform, 16, 28, 808, 24);
        missionText.enableWordWrapping = false;
        missionText.overflowMode = TextOverflowModes.Ellipsis;

        // The HUD: the attempt, the points, the chance of a new move.
        var hud = Ui.Card(root, "Hud", Theme.Panel, Theme.Border1);
        Ui.Place(hud.rectTransform, 1020, 98, 840, 118);
        pointsText = Ui.Text(hud.transform, "", 16, Theme.Ink);
        Ui.Place(pointsText.rectTransform, 18, 14, 560, 50);
        exploreText = Ui.Text(hud.transform, "", 14, Theme.Text2);
        Ui.Place(exploreText.rectTransform, 18, 66, 800, 40);
        bookCount = Ui.Text(hud.transform, "", 14, Theme.TealDeep, true, TextAlignmentOptions.TopRight);
        Ui.Place(bookCount.rectTransform, 580, 14, 240, 24);

        // The sensors, live.
        var senses = Ui.Card(root, "Senses", Theme.Panel, Theme.Border1);
        Ui.Place(senses.rectTransform, 1020, 228, 410, 206);
        var sl = Ui.Eyebrow(senses.transform, "WHAT IT SENSES", Theme.TealDeep); Ui.Place(sl.rectTransform, 16, 10, 300, 16);
        for (int i = 0; i < Sensors.Count; i++)
        {
            var name = Ui.Text(senses.transform, Sensors.Names[i], 13, Theme.Ink, true);
            Ui.Place(name.rectTransform, 16, 34 + i * 34, 110, 20);
            sensorBar[i] = Ui.Bar(senses.transform, Theme.Border, Theme.Aqua, 140, 8);
            Ui.Place(sensorBar[i].track.rectTransform, 130, 40 + i * 34, 140, 8);
            sensorText[i] = Ui.Text(senses.transform, "", 13, Theme.Text2);
            Ui.Place(sensorText[i].rectTransform, 284, 34 + i * 34, 120, 20);
        }
        // The move it just made, and what it earned.
        var moveCard = Ui.Card(root, "Move", Theme.Panel, Theme.Border1);
        Ui.Place(moveCard.rectTransform, 1450, 228, 410, 206);
        var ml = Ui.Eyebrow(moveCard.transform, "WHAT IT DID, AND WHAT IT EARNED", Theme.TealDeep); Ui.Place(ml.rectTransform, 16, 10, 380, 16);
        moveText = Ui.Text(moveCard.transform, "", 15, Theme.Ink);
        Ui.Place(moveText.rectTransform, 16, 34, 378, 160);

        // The rule book it writes itself.
        var book = Ui.Card(root, "Book", Theme.Panel, Theme.Border1);
        Ui.Place(book.rectTransform, 1020, 446, 840, 390);
        var bl = Ui.Eyebrow(book.transform, "THE RULE BOOK IT WROTE ITSELF  ·  FOR EACH SITUATION, THE MOVE THAT PAID", Theme.TealDeep); Ui.Place(bl.rectTransform, 16, 10, 800, 16);
        var scroll = Ui.Scroll(book.transform, "Scroll", out bookContent);
        Ui.Place(scroll.GetComponent<RectTransform>(), 16, 32, 808, 346);

        string opening = app.CurrentSetup != null ? (app.step == App.Step.FixIt ? "Fix it: " + app.CurrentSetup.name + ". " : "Watch the pro. ") + app.CurrentSetup.why
                       : app.luma.Say(l != null ? "live-" + l.number : "welcome");
        coach = Luma.Card(root, 1020, 848, 840, 150, opening, 15, null, app.luma);

        results = Ui.Button(root, "See the debrief", app.FinishLevel, Ui.ButtonStyle.Primary, 18);
        Ui.Place(results.GetComponent<RectTransform>(), 1620, 1016, 240, 46);
        results.gameObject.SetActive(false);

        follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);
        Mission();
        Refresh();
        follower.StartCoroutine(Countdown());
        Ui.Emblem(root);
    }

    void StartRun(RectTransform frame)
    {
        var (s, label) = queue[which];
        training = new Training(s, label);
        app.world.Build(training.track, false);
        app.world.ShowOverview(frame);
        phase = Phase.Training;
        saidFirst = saidSettled = saidLoophole = false;
    }

    void Mission()
    {
        string run = queue.Count > 1 ? $"{(string.IsNullOrEmpty(training.label) ? "Run " + (which + 1) : training.label)} ({which + 1} of {queue.Count})  ·  " : "";
        missionText.text = phase == Phase.Training ? $"{run}Training on {training.track.name.ToLowerInvariant()}: {training.s.attempts} attempts, seed {training.s.seed}"
                         : phase == Phase.Racing ? $"{run}The race on {race.track.name.ToLowerInvariant()}: {race.laps.Count}/{lapsWanted} laps, no new moves, no learning"
                         : phase == Phase.Real ? "The real run: one lap on a track the simulator never had. No learning." : "Done.";
    }
    int lapsWanted => app.lesson != null ? app.lesson.laps : 5;

    public void SetSpeed(int i) { speedIndex = i; speed.Set(i, false); }

    // ---- every frame ---------------------------------------------------------------------

    void Tick(float dt)
    {
        if (paused) return;
        if (phase == Phase.Done) return;
        float interval = Interval[speedIndex];
        if (interval <= 0)
        {
            float start = Time.realtimeSinceStartup;
            int n = 0;
            while (Time.realtimeSinceStartup - start < 0.008f && n++ < 20000 && Step(false)) { }
            acc = 0;
        }
        else
        {
            acc += dt;
            while (acc >= interval) { acc -= interval; if (!Step(speedIndex == 0)) { acc = 0; break; } }
        }
        Pose(interval > 0 ? Mathf.Clamp01(acc / interval) : 1f);
        chartTimer += dt;
        if (chartTimer > 0.4f) { chartTimer = 0; chart.Draw(app.session.runs.Concat(new[] { training }).ToList()); }
        bookTimer += dt;
        if (bookTimer > 0.5f) { bookTimer = 0; Book(); }
        Refresh();
    }

    // One move of whatever is running. False when it is over.
    bool Step(bool watchable)
    {
        if (phase == Phase.Training)
        {
            if (training.done) { StartRace(); return false; }
            bool ended = training.Tick();
            if (ended)
            {
                var a = training.Last;
                if (watchable || speedIndex == 1) { if (a.finished) app.world.Finished(); else app.world.Crash(); }
                if (a.finished && !saidFirst) { saidFirst = true; banners.Enqueue(("FIRST FINISH", Theme.Amber, 1.2f)); coach.text = $"First finish at attempt {a.index}. One finish proves nothing: watch whether the curve stays up."; }
                if (training.settled > 0 && !saidSettled) { saidSettled = true; banners.Enqueue(("THE CURVE SETTLED", Theme.TealDeep, 1.2f)); coach.text = $"The curve settled at attempt {training.settled}: most attempts finish now. That is learning, not luck."; }
                if (!saidLoophole && a.points > training.s.finish * 0.6f && !a.finished && training.s.speedBonus > 0) { saidLoophole = true; banners.Enqueue(("POINTS, NO LAP", Theme.Danger, 1.4f)); coach.text = "Look: the curve rises and the lap count stays at zero. It found the cheapest way to the number, and the number was not the job."; }
                if (training.done) { StartRace(); return false; }
            }
            return true;
        }
        if (phase == Phase.Racing || phase == Phase.Real)
        {
            bool lapEnded = race.Tick();
            if (lapEnded)
            {
                var lap = race.laps.Last();
                if (watchable || speedIndex == 1) { if (lap.finished) app.world.Finished(); else app.world.Crash(); }
                if (watchable) banners.Enqueue((lap.finished ? $"LAP {lap.index}: FINISHED IN {lap.steps} MOVES" : $"LAP {lap.index}: CRASHED", lap.finished ? Theme.TealDeep : Theme.Danger, 0.9f));
                if (race.done) { EndRace(); return false; }
            }
            return true;
        }
        return false;
    }

    void StartRace()
    {
        banners.Clear();
        phase = Phase.Racing;
        race = new Race(training, Track.Get(training.s.test), lapsWanted, training.s.seed + 1);
        if (race.track != training.track || true) { app.world.Build(race.track, false); app.world.ShowOverview(app.world.frame); }
        if (speedIndex > 1) SetSpeed(1);
        banners.Enqueue(("THE RACE", Theme.Ink, 1.2f));
        coach.text = $"The race: {lapsWanted} laps on {race.track.name.ToLowerInvariant()} with the rule book as it is. No new moves, no learning.";
        Mission();
    }

    void EndRace()
    {
        if (phase == Phase.Real) { app.session.real = race; phase = Phase.Done; Finish(); return; }
        app.session.runs.Add(training);
        app.session.races.Add(race);
        which++;
        if (which < queue.Count) { StartRun(app.world.frame); Mission(); SetSpeed(2); return; }
        if (app.lesson != null && app.lesson.realRun) { StartReal(); return; }
        phase = Phase.Done;
        Finish();
    }

    void StartReal()
    {
        phase = Phase.Real;
        race = new Race(training, Track.Get("real"), 1, training.s.seed + 2);
        app.world.Build(race.track, false);
        app.world.ShowOverview(app.world.frame);
        SetSpeed(0);
        banners.Enqueue(("THE REAL RUN", Theme.Danger, 1.4f));
        coach.text = "One lap on the real track, no learning. The oil on the straight is something the simulator never had.";
        Mission();
    }

    void Finish()
    {
        int home = app.session.races.Count > 0 ? app.session.LastRace.Finishes : 0;
        banners.Enqueue((home * 2 >= lapsWanted ? "RACE OVER" : "BACK TO THE PIT LANE", Theme.Ink, 1.6f));
        results.gameObject.SetActive(true);
        pause.interactable = stop.interactable = false;
        Mission();
    }

    void Pose(float k)
    {
        var c = phase == Phase.Training ? training.car : race?.car;
        if (c == null) return;
        var readings = phase == Phase.Training ? training.readings : race.readings;
        app.world.Pose(c.from, c.pos, c.heading, c.speed, k, readings, training.s.sensors);
    }

    void Refresh()
    {
        if (phase == Phase.Training)
        {
            turnText.text = $"Attempt {Mathf.Min(training.Attempts + 1, training.s.attempts)} of {training.s.attempts}";
            phaseText.text = training.firstFinish < 0 ? "No finish yet. Random moves, and the points sorting them." : training.settled < 0 ? $"First finish at {training.firstFinish}. Waiting for the curve to settle." : $"Settled at {training.settled}.";
            pointsText.text = $"This attempt: {training.attemptPoints:0} points  ·  best attempt: {training.Best:0}  ·  last move earned {training.lastReward:+0.#;-0.#}";
            exploreText.text = $"Chance of a new move right now: {training.explore:0.00}  ·  the moves that paid come back more often; the rest it tries at this chance.";
            bookCount.text = $"{training.book.pages.Count} situations in the book";
            moveText.text = $"{Moves.Names[training.lastMove]}  ·  earned {training.lastReward:+0.#;-0.#}\n\nSpeed {training.car.speed}, {training.car.steps} moves this attempt.\n" + (training.car.crashed ? "Crashed." : training.car.finished ? "Finished." : "Driving.");
        }
        else if (race != null)
        {
            turnText.text = phase == Phase.Real ? "The real run" : $"Lap {Mathf.Min(race.Lap, lapsWanted)} of {lapsWanted}";
            phaseText.text = $"Finished {race.Finishes}, crashed {race.Crashes}, wall touches {race.Touches:0.0} a lap, lap time {race.LapTime:0} moves";
            pointsText.text = $"No new moves, no learning: the rule book as it is. Unknown situations met: {race.unknown}.";
            exploreText.text = "";
            bookCount.text = $"{training.book.pages.Count} situations in the book";
            moveText.text = $"Speed {race.car.speed}, {race.car.steps} moves this lap.\n" + (race.car.crashed ? "Crashed." : race.car.finished ? "Finished." : "Driving.");
        }
        var r = phase == Phase.Training ? training.readings : race?.readings;
        for (int i = 0; i < Sensors.Count; i++)
        {
            bool on = training.s.sensors[i];
            int v = r != null ? r[i] : 0;
            sensorText[i].text = on ? Sensors.Describe(i, v) : "off";
            sensorText[i].color = on ? Theme.Ink : Theme.Muted;
            float max = i == Sensors.Speed ? 2 : i == Sensors.Bend ? 1 : Sensors.Far;
            Ui.SetBar(sensorBar[i], on ? v / max : 0);
            sensorBar[i].fill.color = !on ? Theme.Border : i == Sensors.Front || i == Sensors.Left || i == Sensors.Right ? (v == 0 ? Theme.Danger : v == 1 ? Theme.Amber : Theme.Aqua) : Theme.Aqua;
        }
    }

    // The rule book: every situation met, and the move that pays there, the
    // current situation first.
    void Book()
    {
        Ui.Clear(bookContent);
        var s = training.s;
        var pages = training.book.pages.ToList();
        string now = phase == Phase.Training ? training.key : Driving.Key(race.readings, s.sensors);
        pages.Sort((a, b) => a.Key == now ? -1 : b.Key == now ? 1 : string.CompareOrdinal(a.Key, b.Key));
        float y = 0;
        foreach (var page in pages.Take(40))
        {
            bool current = page.Key == now;
            int best = training.book.Best(page.Key, s.moves);
            var row = Ui.Box(bookContent, "Row", current ? Theme.Surface2 : Theme.Panel, false);
            Ui.Place(row.rectTransform, 0, y, 800, 28);
            var sit = Ui.Text(row.transform, Driving.Words(Driving.Parse(page.Key), s.sensors), 13, current ? Theme.Ink : Theme.Text2, current);
            Ui.Place(sit.rectTransform, 10, 4, 560, 22);
            sit.enableWordWrapping = false; sit.overflowMode = TextOverflowModes.Ellipsis;
            var mv = Ui.Text(row.transform, best >= 0 ? Moves.Names[best] : "?", 13, Theme.TealDeep, true, TextAlignmentOptions.TopRight);
            Ui.Place(mv.rectTransform, 580, 4, 210, 22);
            y += 30;
        }
        if (pages.Count == 0)
        {
            var empty = Ui.Text(bookContent, "Nothing yet. It starts knowing nothing.", 13, Theme.Muted);
            Ui.Place(empty.rectTransform, 10, 4, 600, 22);
            y = 30;
        }
        bookContent.sizeDelta = new Vector2(0, y);
    }

    // ---- banners -----------------------------------------------------------------------

    IEnumerator Countdown()
    {
        paused = true;
        yield return new WaitForSeconds(0.3f);
        yield return Banner("ATTEMPT 1", Theme.Ink, 0.7f);
        paused = false;
        Ui.SetLabel(pause, "Pause");
        while (true)
        {
            if (banners.Count > 0) { var (text, colour, seconds) = banners.Dequeue(); yield return Banner(text, colour, seconds); }
            else yield return null;
        }
    }

    IEnumerator Banner(string text, Color colour, float seconds)
    {
        var card = Ui.Box(bannerLayer, "Banner", new Color(1, 1, 1, 0.92f));
        Ui.Place(card.rectTransform, 110, 360, 820, 110);
        card.raycastTarget = false;
        var t = Ui.Text(card.transform, text, 46, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = 4;
        Ui.Stretch(t.rectTransform);
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(110 + 410, -(360 + 55));
        yield return Ui.Animate(0.18f, k => { group.alpha = k; card.rectTransform.localScale = Vector3.one * (1.15f - 0.15f * k); }, Ui.EaseOut);
        yield return new WaitForSecondsRealtime(seconds);
        yield return Ui.Animate(0.25f, k => group.alpha = 1 - k);
        Object.Destroy(card.gameObject);
    }

    class Follower : MonoBehaviour
    {
        RunScreen screen;
        public void Bind(RunScreen s) { screen = s; }
        void Update() { screen?.Tick(Time.deltaTime); }
        void OnDestroy() { if (Current == screen) Current = null; }
    }
}
