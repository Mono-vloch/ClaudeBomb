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
    readonly TextMeshProUGUI turnText, phaseText, pointsText, exploreText, moveText, bookCount, missionText, rewardText, bestText, lapText;
    RectTransform coachLine;
    readonly TextMeshProUGUI[] sensorText = new TextMeshProUGUI[Sensors.Count];
    readonly (Image track, Image fill)[] sensorBar = new (Image, Image)[Sensors.Count];
    readonly Button pause, results, stop;
    readonly Ui.Segmented viewToggle;
    readonly Follower follower;
    readonly Queue<(string text, string sub, Color colour, float seconds, bool big)> banners = new Queue<(string, string, Color, float, bool)>();
    readonly List<Vector2Int> path = new List<Vector2Int>();
    int speedIndex = 2;
    bool paused, saidFirst, saidSettled, saidLoophole, showing;
    float acc, bookTimer, chartTimer;
    string shownKey = "";
    static readonly float[] Interval = { 1f / 5, 1f / 40, 1f / 400, 0f };

    public RunScreen(Transform parent, App app, List<(Settings s, string label)> runs)
    {
        Current = this;
        this.app = app;
        speedIndex = app.speedIndex;
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
        // The mission HUD, dark glass over the world: the level and the step, the attempt,
        // the reward score, the best, the lap.
        var hud1 = Ui.Glass(root, "Hud", 40, 16, 500, 96);
        var levelTag = Ui.Eyebrow(hud1.transform, l != null ? $"LEVEL {l.id}" : "FREE PLAY", Theme.Teal);
        levelTag.font = Theme.Bold;
        Ui.Place(levelTag.rectTransform, 20, 10, 460, 18);
        turnText = Ui.Text(hud1.transform, "", 28, Color.white, true);
        Ui.Place(turnText.rectTransform, 20, 28, 460, 40);
        turnText.enableWordWrapping = false;
        phaseText = Ui.Text(hud1.transform, "", 12, Look.AsphaltLine);
        Ui.Place(phaseText.rectTransform, 20, 68, 460, 22);
        phaseText.enableWordWrapping = false; phaseText.overflowMode = TextOverflowModes.Ellipsis;
        var hud2 = Ui.Glass(root, "Score", 550, 16, 440, 96);
        TextMeshProUGUI Stat(string name, float x, float w)
        {
            var e = Ui.Eyebrow(hud2.transform, name, Theme.Teal);
            Ui.Place(e.rectTransform, x, 12, w, 16);
            var v = Ui.Text(hud2.transform, "", 26, Color.white, true);
            Ui.Place(v.rectTransform, x, 32, w, 40);
            v.enableWordWrapping = false; v.overflowMode = TextOverflowModes.Ellipsis;
            return v;
        }
        rewardText = Stat("REWARD", 20, 150);
        bestText = Stat("BEST", 180, 130);
        lapText = Stat("LAP", 320, 110);

        viewToggle = new Ui.Segmented(root, new[] { "The track", "Follow" }, 0, 260, 40, 14);
        Ui.Place(viewToggle.root, 40, 1032, 260, 40);
        viewToggle.OnSelect = i => { if (i == 0) app.world.ShowOverviewAgain(); else app.world.ShowFollow(frame); };
        Sfx.EngineAudible = speedIndex <= 1;
        pause = Ui.Button(root, "Pause", () => { paused = !paused; Ui.SetLabel(pause, paused ? "Resume" : "Pause"); if (paused) Sfx.Engine(0f); }, Ui.ButtonStyle.Secondary, 14);
        Ui.Place(pause.GetComponent<RectTransform>(), 312, 1032, 90, 40);
        stop = Ui.Button(root, "Stop, race now", RaceNow, Ui.ButtonStyle.Quiet, 14);
        Ui.Place(stop.GetComponent<RectTransform>(), 410, 1032, 150, 40);
        var settings = Ui.Button(root, "Settings", () => SettingsSheet.Toggle(app, root, null), Ui.ButtonStyle.Quiet, 14); // the speed, among others
        Ui.Place(settings.GetComponent<RectTransform>(), 570, 1032, 100, 40);

        chart = new Chart(root, 40, 716, 940, 304, "POINTS PER ATTEMPT  ·  THE CURVE");
        labelsLayer = Ui.Node("Labels", root);
        Ui.Stretch(labelsLayer);
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);

        // The mission, at the top of the column.
        var mission = Ui.Soft(root, "Mission");
        Ui.Place(mission.rectTransform, 1020, 28, 840, 58);
        var missionTag = Ui.Eyebrow(mission.transform, l != null ? $"LEVEL {l.id}" : "FREE PLAY", Theme.TealDeep);
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
        var ml = Ui.Eyebrow(moveCard.transform, "THE MOVE", Theme.TealDeep); Ui.Place(ml.rectTransform, 16, 10, 380, 16);
        moveText = Ui.Text(moveCard.transform, "", 15, Theme.Ink);
        Ui.Place(moveText.rectTransform, 16, 34, 378, 160);

        // The rule book it writes itself.
        var book = Ui.Card(root, "Book", Theme.Panel, Theme.Border1);
        Ui.Place(book.rectTransform, 1020, 446, 840, 436);
        var bl = Ui.Eyebrow(book.transform, "THE RULE BOOK  ·  THE SITUATION, THE MOVE THAT PAID", Theme.TealDeep); Ui.Place(bl.rectTransform, 16, 10, 800, 16);
        var scroll = Ui.Scroll(book.transform, "Scroll", out bookContent);
        Ui.Place(scroll.GetComponent<RectTransform>(), 16, 32, 808, 392);

        string opening = app.luma.Say(l != null ? "live-" + l.number : "welcome");
        SayLine(opening, true);

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

    // LUMA's line at the foot of the column: the lead of what she has to say, typed as she
    // says it, gone once she is done. The whole of it is in the file.
    void SayLine(string text, bool stay = false)
    {
        if (coachLine != null) Object.Destroy(coachLine.gameObject);
        coachLine = app.luma.Line(root, 1020, 896, 840, Labels.Lead(text), false, stay, 15);
    }

    void Mission()
    {
        string run = queue.Count > 1 ? $"{(string.IsNullOrEmpty(training.label) ? "Run " + (which + 1) : training.label)} ({which + 1} of {queue.Count})  ·  " : "";
        missionText.text = phase == Phase.Training ? $"{run}Training on {training.track.name.ToLowerInvariant()}: {training.s.attempts} attempts, seed {training.s.seed}"
                         : phase == Phase.Racing ? $"{run}The race on {race.track.name.ToLowerInvariant()}: {race.laps.Count}/{lapsWanted} laps"
                         : phase == Phase.Real ? "The real run: one lap, no learning" : "Done.";
    }
    int lapsWanted => app.lesson != null ? app.lesson.laps : 5;

    public void SetSpeed(int i) { speedIndex = i; Sfx.EngineAudible = i <= 1; }
    public void RaceNow() { if (phase == Phase.Training) training.done = true; }

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
            if (path.Count == 0) path.Add(training.car.from);
            bool ended = training.Tick();
            if (path.Count == 0 || path[path.Count - 1] != training.car.pos) path.Add(training.car.pos);
            if (ended)
            {
                var a = training.Last;
                if (watchable || speedIndex == 1) { if (a.finished) app.world.Finished(watchable); else app.world.Crash(watchable); }
                app.world.AddGhost(path, a.finished);
                path.Clear();
                if (a.finished && !saidFirst) { saidFirst = true; Announce("FIRST FINISH", $"attempt {a.index}", Theme.Amber, 1.4f, true); SayLine($"First finish at attempt {a.index}. One finish proves nothing: watch whether the curve stays up."); }
                if (training.settled > 0 && !saidSettled) { saidSettled = true; Announce("THE CURVE SETTLED", $"attempt {training.settled}: most attempts finish now", Theme.Teal, 1.5f, true); SayLine($"The curve settled at attempt {training.settled}: most attempts finish now. That is learning, not luck."); }
                if (!saidLoophole && a.points > training.s.finish * 0.6f && !a.finished && training.s.speedBonus > 0) { saidLoophole = true; Announce("REWARD HACKED", "points, no lap: it found the cheapest way to the number", Theme.Danger, 2.4f, true); SayLine("Look: the curve rises and the lap count stays at zero. It found the cheapest way to the number, and the number was not the job."); }
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
                if (watchable || speedIndex == 1) { if (lap.finished) app.world.Finished(watchable); else app.world.Crash(watchable); }
                if (watchable) Announce(lap.finished ? $"LAP {lap.index}" : $"LAP {lap.index}: CRASHED", lap.finished ? $"finished in {lap.steps} moves" : "", lap.finished ? Theme.Teal : Theme.Danger, 0.9f, false);
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
        SayLine($"The race: {lapsWanted} laps on {race.track.name.ToLowerInvariant()} with the rule book as it is. No new moves, no learning.");
        Mission();
        follower.StartCoroutine(RaceIntro());
    }

    // The grid: the camera flies in behind the car while the countdown runs, then the chase.
    IEnumerator RaceIntro()
    {
        paused = true;
        Ui.SetLabel(pause, "Pause");
        // A teaching moment still on its way (the first finish, the curve settling, the
        // wrong reward) is shown first: at the fast speeds the training ends under it.
        while (banners.Count > 0 || showing) yield return null;
        app.world.ShowCountdown(app.world.frame, 2.6f);
        viewToggle.Set(1, false);
        yield return new WaitForSecondsRealtime(0.3f);
        foreach (var n in new[] { "3", "2", "1" }) Announce(n, "", Theme.Teal, 0.5f, true);
        Announce("GO", $"{lapsWanted} laps, the rule book as it is", Theme.Teal, 0.9f, true);
        yield return new WaitForSecondsRealtime(2.3f);
        paused = false;
    }

    void EndRace()
    {
        var last = race.laps.Last();
        if (last.finished && (phase == Phase.Real || which + 1 >= queue.Count))
        {
            // The last lap of the last race: the slow-motion turn around the car first.
            paused = true;
            app.world.FinishMoment(1.8f, () => { paused = false; EndRaceNow(); });
            return;
        }
        EndRaceNow();
    }

    void EndRaceNow()
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
        Announce("THE REAL RUN", "one lap, no learning, on a track the simulator never had", Theme.Danger, 1.8f, true);
        SayLine("One lap on the real track, no learning. The oil on the straight is something the simulator never had.");
        Mission();
    }

    void Finish()
    {
        int home = app.session.races.Count > 0 ? app.session.LastRace.Finishes : 0;
        Announce(home * 2 >= lapsWanted ? "RACE OVER" : "BACK TO THE PIT LANE", home * 2 >= lapsWanted ? $"{home} of {lapsWanted} laps finished" : "most laps did not finish", home * 2 >= lapsWanted ? Theme.Teal : Theme.Amber, 1.8f, true);
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
            var bestLap = training.curve.Where(a => a.finished).Select(a => a.steps).DefaultIfEmpty(0).Min();
            rewardText.text = $"{training.attemptPoints:+0;-0;0}";
            bestText.text = $"{training.Best:0}";
            lapText.text = bestLap > 0 ? $"{bestLap} mv" : "–";
            turnText.text = $"Attempt {Mathf.Min(training.Attempts + 1, training.s.attempts)} of {training.s.attempts}";
            phaseText.text = training.firstFinish < 0 ? "No finish yet" : training.settled < 0 ? $"First finish at {training.firstFinish}" : $"Settled at {training.settled}";
            pointsText.text = $"This attempt {training.attemptPoints:0}  ·  best {training.Best:0}  ·  last move {training.lastReward:+0.#;-0.#}";
            exploreText.text = $"New move chance {training.explore:0.00}";
            bookCount.text = $"{training.book.pages.Count} situations";
            moveText.text = $"{Moves.Names[training.lastMove]}  ·  {training.lastReward:+0.#;-0.#}\n\nSpeed {training.car.speed}  ·  {training.car.steps} moves\n" + (training.car.crashed ? "Crashed." : training.car.finished ? "Finished." : "Driving.");
        }
        else if (race != null)
        {
            var bestLap = race.laps.Where(a => a.finished).Select(a => a.steps).DefaultIfEmpty(0).Min();
            rewardText.text = $"{race.Finishes}/{race.laps.Count}";
            bestText.text = bestLap > 0 ? $"{bestLap} mv" : "–";
            lapText.text = $"{race.car.steps} mv";
            turnText.text = phase == Phase.Real ? "The real run" : $"Lap {Mathf.Min(race.Lap, lapsWanted)} of {lapsWanted}";
            phaseText.text = $"Finished {race.Finishes}, crashed {race.Crashes}, wall touches {race.Touches:0.0} a lap, lap time {race.LapTime:0} moves";
            pointsText.text = $"The rule book as it is  ·  unknown situations {race.unknown}";
            exploreText.text = "";
            bookCount.text = $"{training.book.pages.Count} situations";
            moveText.text = $"Speed {race.car.speed}  ·  {race.car.steps} moves\n" + (race.car.crashed ? "Crashed." : race.car.finished ? "Finished." : "Driving.");
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
            var empty = Ui.Text(bookContent, "Nothing yet", 13, Theme.Muted);
            Ui.Place(empty.rectTransform, 10, 4, 600, 22);
            y = 30;
        }
        bookContent.sizeDelta = new Vector2(0, y);
    }

    // ---- moments -----------------------------------------------------------------------

    public void Announce(string text, string sub, Color colour, float seconds, bool big) => banners.Enqueue((text, sub, colour, seconds, big));

    IEnumerator Countdown()
    {
        paused = true;
        yield return new WaitForSeconds(0.3f);
        yield return Moment("ATTEMPT 1", "it starts knowing nothing", Theme.Teal, 0.8f, false);
        paused = false;
        Ui.SetLabel(pause, "Pause");
        while (true)
        {
            if (banners.Count > 0) { var (text, sub, colour, seconds, big) = banners.Dequeue(); showing = true; yield return Moment(text, sub, colour, seconds, big); showing = false; }
            else yield return null;
        }
    }

    static float OutBack(float k) { const float c = 1.70158f; k -= 1f; return 1f + (c + 1f) * k * k * k + c * k * k; }

    // A moment over the world: dark glass, the words in the moment's colour, in with an
    // overshoot; a big one flashes the frame first and carries a line underneath.
    IEnumerator Moment(string text, string sub, Color colour, float seconds, bool big)
    {
        Image flash = null;
        if (big)
        {
            flash = Ui.Box(bannerLayer, "Flash", new Color(colour.r, colour.g, colour.b, 0f), false);
            Ui.Place(flash.rectTransform, 40, 104, 940, 600);
            flash.raycastTarget = false;
        }
        var card = Ui.Box(bannerLayer, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        float h = big ? (string.IsNullOrEmpty(sub) ? 150 : 180) : 92;
        float w = big ? 760 : 620;
        Ui.Place(card.rectTransform, 40 + (940 - w) / 2, 404 - h / 2, w, h);
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", colour, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, h);
        var t = Ui.Text(card.transform, text, big ? (text.Length <= 2 ? 96 : 60) : 36, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = big ? 6 : 4;
        Ui.Place(t.rectTransform, 20, big ? 14 : 20, w - 40, big ? 100 : 52);
        t.enableWordWrapping = false;
        if (!string.IsNullOrEmpty(sub))
        {
            var st = Ui.Text(card.transform, sub, big ? 18 : 14, Look.AsphaltLine, false, TextAlignmentOptions.Center);
            Ui.Place(st.rectTransform, 20, h - (big ? 52 : 32), w - 40, 26);
            st.enableWordWrapping = false; st.overflowMode = TextOverflowModes.Ellipsis;
        }
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(510, -404);
        if (big) Sfx.Sting(colour == Theme.Danger ? Sfx.Mood.Alarm : text == "GO" ? Sfx.Mood.Go : text.Length <= 1 ? Sfx.Mood.Beep : Sfx.Mood.Fanfare); else Sfx.Tick();
        yield return Ui.Animate(big ? 0.34f : 0.2f, k =>
        {
            group.alpha = Mathf.Min(1f, k * 3f);
            float sc = big ? Mathf.LerpUnclamped(0.6f, 1f, OutBack(k)) : Mathf.Lerp(1.12f, 1f, k);
            card.rectTransform.localScale = Vector3.one * sc;
            if (flash != null) flash.color = new Color(colour.r, colour.g, colour.b, 0.45f * Mathf.Max(0f, 1f - k * 2.5f));
        });
        if (flash != null) Object.Destroy(flash.gameObject);
        yield return new WaitForSecondsRealtime(seconds);
        yield return Ui.Animate(0.22f, k => { group.alpha = 1 - k; card.rectTransform.localScale = Vector3.one * (1f - 0.08f * k); });
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
