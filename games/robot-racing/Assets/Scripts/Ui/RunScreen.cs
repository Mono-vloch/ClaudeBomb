using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The run: the robot trains on the track, the camera low behind it, and the world shows
// the learning: every attempt leaves its ghost line on the asphalt, each move's points
// float up from the car, the fuel gauge drains with the attempts. The training is computed
// first and played back (Replay): every attempt at race speed, a thirty-second montage that
// slows down for the first attempt, the first finish, the settle and the last, or straight
// to the end. Then the race on the test track at race speed, the lap on the board by the
// grid. The curve, the rule book and the readings are behind Details, closed by default.
public class RunScreen
{
    public static RunScreen Current;
    public enum Phase { Training, Racing, Real, Done }

    public readonly RectTransform root;
    readonly App app;
    readonly List<(Settings s, string label)> queue;
    int which;
    Replay replay;
    Training training;
    Race race;
    public Phase phase = Phase.Training;
    readonly RectTransform frame, labelsLayer, bannerLayer;
    readonly Hud.Gauge gauge;
    readonly TextMeshProUGUI lapText, objective;
    readonly Button skip, details, camera, results;
    readonly Follower follower;
    readonly Queue<(string text, string sub, Color colour, float seconds, bool big)> banners = new Queue<(string, string, Color, float, bool)>();
    RectTransform sheet;
    Chart chart;
    RectTransform bookContent;
    TextMeshProUGUI readingsText;
    int pace;              // 0 every attempt, 1 montage, 2 max
    bool paused, showing, skipping, overview, saidFirst, saidSettled, saidLoophole, saidGuess;
    int attemptAt, stepAt;  // where the replay is
    float stepTimer, attemptTimer, sheetTimer;
    const float RaceStep = 0.1f;   // seconds per move in the race
    const float WatchStep = 0.2f;  // seconds per move when an attempt is watched
    const float Montage = 30f;     // seconds the montage takes, whatever the budget

    public RunScreen(Transform parent, App app, List<(Settings s, string label)> runs)
    {
        Current = this;
        this.app = app;
        pace = Mathf.Clamp(app.pace, 0, 2);
        queue = runs;
        var l = app.lesson;
        root = Ui.Node("Run", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        StartRun();

        objective = Hud.Objective(root, l != null ? Objective(l) : "Watch it learn.");
        ObjectiveForRun();
        gauge = new Hud.Gauge(root, Ui.W - 40 - 52 - 24 - 92, 20);
        if (app.session != null && app.session.guess >= 0) gauge.Guess = app.session.guess;
        GameMenu.Icon(root);
        lapText = Ui.Text(root, "", 24, Theme.Teal, true, TextAlignmentOptions.Center);
        Ui.Place(lapText.rectTransform, Ui.W / 2 - 160, 30, 320, 40);
        lapText.characterSpacing = 4;

        camera = Hud.Small(root, "Camera", 40, 120, () => { overview = !overview; View(); });
        details = Hud.Small(root, "Details", 172, 120, ToggleSheet);
        // The main action: skip ahead while it trains, the debrief once the race is over.
        skip = Hud.Main(root, "Skip ahead", SkipAhead);
        results = Hud.Main(root, "Debrief", app.FinishLevel);
        results.gameObject.SetActive(false);

        labelsLayer = Ui.Node("Labels", root);
        Ui.Stretch(labelsLayer);
        labelsLayer.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);

        follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);
        follower.StartCoroutine(Opening());
        Sfx.EngineAudible = true;
    }

    static string Objective(Lesson l) => l.number switch
    {
        1 => "Get it to the finish.", 2 => "Watch the curve settle.", 3 => "Watch it learn the hairpin.", 4 => "Watch what it can sense.", 5 => "Watch what it can do.",
        6 => "Light the path to the finish.", 7 => "Watch the points. Does it finish?", 8 => "Watch how often it tries new moves.", 9 => "Watch it learn, then race.",
        10 => "Watch it train, then the test track.", 11 => "Watch the laps. Judge by the measure.", 12 => "Watch the two curves.", 13 => "Watch it train, then the real run.", _ => "Watch it learn.",
    };

    // A level that runs two robots names the one on the track.
    void ObjectiveForRun()
    {
        if (objective == null) return;
        var l = app.lesson;
        if (!string.IsNullOrEmpty(training.label)) objective.text = $"{training.label}: watch it learn.";
        else objective.text = l != null ? Objective(l) : "Watch it learn.";
    }

    // The levels whose lesson is the curve keep it on a board by the grid.
    bool CurveInWorld => app.lesson != null && (app.lesson.number == 2 || app.lesson.number == 7 || app.lesson.number == 8 || app.lesson.number == 12);

    void View()
    {
        if (overview) app.world.ShowOverviewAgain(); else app.world.ShowFollow(frame);
    }

    void StartRun()
    {
        var (s, label) = queue[which];
        training = new Training(s, label);
        replay = new Replay(training);          // the whole run, computed now
        app.world.Build(training.track, false);
        app.world.LapBoard("");
        if (CurveInWorld) app.world.CurveBoard(true);
        // A level whose lesson is the curve watches from above, the board by the grid in view.
        overview = CurveInWorld || !Circuit.HasKit;
        if (overview) app.world.ShowOverview(frame); else app.world.ShowFollow(frame);
        phase = Phase.Training;
        saidGuess = false;
        ObjectiveForRun();
        attemptAt = 0; stepAt = 0; stepTimer = attemptTimer = 0f;
        saidFirst = saidSettled = saidLoophole = skipping = false;
        Pose(0, 0, 0f);
    }

    public void SetPace(int i) { pace = Mathf.Clamp(i, 0, 2); }
    public bool RaceRunning => phase == Phase.Racing && !paused && race != null && race.car.steps > 1;
    public bool BannerUp => showing || banners.Count > 0;
    public int AttemptShown => attemptAt + 1;
    public bool Paused { set => paused = value; }

    IEnumerator Opening()
    {
        paused = true;
        yield return new WaitForSecondsRealtime(0.4f);
        yield return Moment("ATTEMPT 1", "", Theme.Teal, 0.7f, false);
        app.luma.Cue(root, "cue-nobody");
        paused = false;
        while (true)
        {
            if (banners.Count > 0) { var (text, sub, colour, seconds, big) = banners.Dequeue(); showing = true; yield return Moment(text, sub, colour, seconds, big); showing = false; }
            else yield return null;
        }
    }

    // ---- every frame ----

    void Tick(float dt)
    {
        if (paused || GameMenu.Paused || phase == Phase.Done) { Sfx.Engine(0f); return; }
        if (phase == Phase.Training) { Train(dt); return; }
        if (phase == Phase.Racing || phase == Phase.Real) RaceTick(dt);
    }

    // The montage: how long the attempt at hand gets on the screen.
    float AttemptSeconds(AttemptRecord a)
    {
        if (pace == 0 || replay.IsMoment(a.index)) return a.steps.Count * WatchStep;
        int moments = new[] { 1, replay.FirstFinish, replay.Settled, replay.Count }.Where(i => i > 0).Distinct().Count();
        float momentTime = replay.attempts.Where(x => replay.IsMoment(x.index)).Sum(x => x.steps.Count * WatchStep);
        int others = Mathf.Max(1, replay.Count - moments);
        return Mathf.Max(0.02f, (Montage - momentTime) / others);
    }

    void Train(float dt)
    {
        if (attemptAt >= replay.Count) { EndTraining(); return; }
        if (pace == 2 || skipping) { FinishAllAttempts(); EndTraining(); return; }
        var a = replay.attempts[attemptAt];
        float seconds = AttemptSeconds(a);
        bool watched = seconds >= a.steps.Count * WatchStep * 0.5f;
        if (watched)
        {
            // Step by step: the car drives, the points float, the camera follows.
            float per = seconds / a.steps.Count;
            stepTimer += dt;
            while (stepTimer >= per && stepAt < a.steps.Count)
            {
                stepTimer -= per;
                var st = a.steps[stepAt];
                if (Mathf.Abs(st.reward) >= 0.9f || st.crashed || st.finished) app.world.PointsFloat(st.reward, st.crashed ? Theme.Danger : st.finished ? Theme.Teal : st.reward > 0 ? Theme.Teal : Theme.Amber);
                stepAt++;
                if (stepAt >= a.steps.Count) { EndAttempt(a, true); break; }
            }
            if (attemptAt < replay.Count && stepAt < a.steps.Count) Pose(attemptAt, stepAt, Mathf.Clamp01(stepTimer / per));
            Sfx.EngineAudible = true;
        }
        else
        {
            // A flash: the whole attempt at once, its ghost line and its end.
            Sfx.EngineAudible = false;
            attemptTimer += dt;
            int shown = 0;
            while (attemptTimer >= seconds && attemptAt < replay.Count && shown++ < 40)
            {
                attemptTimer -= seconds;
                var cur = replay.attempts[attemptAt];
                if (replay.IsMoment(cur.index)) { stepAt = 0; stepTimer = 0f; break; } // the next one is watched
                EndAttempt(cur, false);
                seconds = attemptAt < replay.Count ? AttemptSeconds(replay.attempts[attemptAt]) : 1f;
            }
            if (attemptAt < replay.Count) { var cur = replay.attempts[attemptAt]; Pose(attemptAt, cur.steps.Count - 1, 1f); }
        }
        gauge.Set(1f - Mathf.Clamp01(attemptAt / (float)training.s.attempts));
    }

    void EndAttempt(AttemptRecord a, bool watched)
    {
        var r = a.result;
        if (watched) { if (r.finished) app.world.FinishFlash(); else app.world.Crash(true); }
        app.world.AddGhost(a.path, r.finished);
        if (CurveInWorld) app.world.CurveDraw(replay.attempts.Take(attemptAt + 1).Select(x => x.result.points).ToList(), training.s.finish);
        if (r.finished && !saidFirst) { saidFirst = true; Announce("FIRST FINISH", "", Theme.Amber, 1.2f, true); follower.StartCoroutine(CueThen("cue-first", GuessCue(a.index))); }
        if (replay.Settled > 0 && a.index >= replay.Settled && !saidSettled) { saidSettled = true; Announce("SETTLED", "", Theme.Teal, 1.2f, true); app.luma.Cue(root, "cue-settled"); }
        if (!saidLoophole && r.points > training.s.finish * 0.6f && !r.finished && training.s.speedBonus > 0) { saidLoophole = true; Announce("REWARD HACKED", "", Theme.Danger, 1.6f, true); }
        attemptAt++; stepAt = 0; stepTimer = 0f;
    }

    void FinishAllAttempts()
    {
        while (attemptAt < replay.Count)
        {
            var a = replay.attempts[attemptAt];
            app.world.AddGhost(a.path, a.result.finished);
            if (a.result.finished && !saidFirst) saidFirst = true;
            attemptAt++;
        }
        if (CurveInWorld) app.world.CurveDraw(replay.attempts.Select(x => x.result.points).ToList(), training.s.finish);
        gauge.Set(0f, true);
        var last = replay.attempts[replay.Count - 1];
        Pose(replay.Count - 1, last.steps.Count - 1, 1f);
    }

    void Pose(int attempt, int step, float k)
    {
        if (attempt >= replay.Count) return;
        var a = replay.attempts[attempt];
        var st = a.steps[Mathf.Clamp(step, 0, a.steps.Count - 1)];
        app.world.Pose(st.from, st.to, st.heading, st.speed, k, st.readings, training.s.sensors);
    }

    void EndTraining()
    {
        if (training.firstFinish < 0 && app.lesson != null && app.lesson.number == 1) app.luma.Cue(root, "cue-zero");
        else if (training.firstFinish < 0 && !saidGuess && app.session != null && app.session.guess >= 0) { saidGuess = true; app.luma.Cue(root, "guess-never"); }
        StartRace();
    }

    // The player's guess on the fuel gauge against the first finish: close when within a
    // tenth of the tank either way.
    string GuessCue(int firstFinish)
    {
        if (saidGuess || app.session == null || app.session.guess < 0 || which > 0) return null;
        saidGuess = true;
        float guessed = app.session.guess * training.s.attempts;
        float off = (firstFinish - guessed) / Mathf.Max(1f, training.s.attempts);
        return Mathf.Abs(off) <= 0.1f ? "guess-close" : off < 0 ? "guess-early" : "guess-late";
    }

    IEnumerator CueThen(string first, string then)
    {
        app.luma.Cue(root, first);
        if (then == null) yield break;
        yield return new WaitForSecondsRealtime(0.5f);
        while (app.luma.Busy) yield return null;
        yield return new WaitForSecondsRealtime(0.6f);
        if (root != null) app.luma.Cue(root, then);
    }

    public void SkipAhead()
    {
        if (phase == Phase.Training) skipping = true;
    }

    // ---- the race ----

    void StartRace()
    {
        banners.Clear();
        phase = Phase.Racing;
        race = new Race(training, Track.Get(training.s.test), lapsWanted, training.s.seed + 1);
        app.world.Build(race.track, false);
        app.world.LapBoard($"LAP 1/{lapsWanted}");
        if (Circuit.HasKit) app.world.ShowFollow(frame); else app.world.ShowOverview(frame);
        skip.gameObject.SetActive(false);
        follower.StartCoroutine(RaceIntro());
    }
    int lapsWanted => app.lesson != null ? app.lesson.laps : 5;

    IEnumerator RaceIntro()
    {
        paused = true;
        while (banners.Count > 0 || showing) yield return null;
        app.world.ShowCountdown(frame, 2.6f);
        yield return new WaitForSecondsRealtime(0.3f);
        lapText.text = $"LAP 1/{lapsWanted}";
        yield return app.world.StartLights(0.7f);
        Announce("GO", "", Theme.Teal, 0.6f, true);
        app.luma.Cue(root, "cue-race");
        paused = false;
    }

    void RaceTick(float dt)
    {
        stepTimer += dt;
        while (stepTimer >= RaceStep)
        {
            stepTimer -= RaceStep;
            bool lapEnded = race.Tick();
            if (lapEnded)
            {
                var lap = race.laps.Last();
                if (lap.finished) { app.world.Finished(true); app.world.LapBoardFlash(Theme.Teal); }
                else { app.world.Crash(true); app.world.LapBoardFlash(Theme.Danger); }
                if (race.done) { EndRace(); return; }
                string text = $"LAP {Mathf.Min(race.Lap, lapsWanted)}/{lapsWanted}";
                lapText.text = text; app.world.LapBoard(text);
            }
            if (race.done) break;
        }
        var c = race.car;
        app.world.Pose(c.from, c.pos, c.heading, c.speed, Mathf.Clamp01(stepTimer / RaceStep), race.readings, training.s.sensors);
    }

    void EndRace()
    {
        var last = race.laps.Last();
        if (last.finished && (phase == Phase.Real || which + 1 >= queue.Count))
        {
            paused = true;
            app.world.FinishMoment(1.8f, () => { paused = false; EndRaceNow(); }, overviewAfter: false);
            return;
        }
        // A race that ended on the wall: the car where it stopped, seen close.
        app.world.ShowCarOrbit(frame, 1.75f, 17f);
        EndRaceNow();
    }

    void EndRaceNow()
    {
        if (phase == Phase.Real) { app.session.real = race; phase = Phase.Done; Finish(); return; }
        app.session.runs.Add(training);
        app.session.races.Add(race);
        which++;
        if (which < queue.Count) { StartRun(); skip.gameObject.SetActive(true); return; }
        if (app.lesson != null && app.lesson.realRun) { StartReal(); return; }
        phase = Phase.Done;
        Finish();
    }

    void StartReal()
    {
        phase = Phase.Real;
        race = new Race(training, Track.Get("real"), 1, training.s.seed + 2);
        app.world.Build(race.track, false);
        app.world.LapBoard("LAP 1/1");
        if (Circuit.HasKit) app.world.ShowFollow(frame); else app.world.ShowOverview(frame);
        Announce("THE REAL RUN", "", Theme.Danger, 1.6f, true);
        app.luma.Cue(root, "cue-real");
    }

    void Finish()
    {
        int home = app.session.races.Count > 0 ? app.session.LastRace.Finishes : 0;
        var real = app.session.real;
        if (real != null) Announce("THE REAL RUN", real.Finishes > 0 ? "finished" : "crashed on the oil", real.Finishes > 0 ? Theme.Teal : Theme.Danger, 1.8f, true);
        else Announce("RACE OVER", $"{home} of {lapsWanted} laps", home * 2 >= lapsWanted ? Theme.Teal : Theme.Amber, 1.6f, true);
        results.gameObject.SetActive(true);
        Sfx.Engine(0f);
    }

    // ---- Details: the curve, the rule book, the readings, behind one button ----

    void ToggleSheet()
    {
        if (sheet != null) { Object.Destroy(sheet.gameObject); sheet = null; return; }
        const float W = 640f, DW = W - 56;
        sheet = Ui.Node("Details", root);
        Ui.Place(sheet, Ui.W - W, 0, W, Ui.H);
        var bg = Ui.Box(sheet, "Glass", Ui.DarkFill, false);
        Ui.Stretch(bg.rectTransform);
        var keyline = Ui.Box(sheet, "Keyline", Theme.Teal, false);
        Ui.Place(keyline.rectTransform, 0, 0, 3, Ui.H);
        var tag = Ui.Eyebrow(sheet, "DETAILS", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 28, 26, 300, 22);
        var close = Ui.Button(sheet, "Close", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), W - 112, 18, 88, 40);
        chart = new Chart(sheet, 20, 70, W - 40, 236, "POINTS PER ATTEMPT  ·  THE CURVE", dark: true);
        readingsText = Ui.Text(sheet, "", 16, Color.white);
        Ui.Place(readingsText.rectTransform, 28, 324, DW, 150);
        var bl = Ui.Eyebrow(sheet, "THE RULE BOOK  ·  THE MOVE THAT PAID", Theme.Teal); Ui.Place(bl.rectTransform, 28, 484, DW, 22);
        var scroll = Ui.Scroll(sheet, "Scroll", out bookContent);
        Ui.Place(scroll.GetComponent<RectTransform>(), 20, 514, W - 40, Ui.H - 514 - 24);
        Ui.Place(frame, 0, 0, Ui.W - W, Ui.H);
        View();
        sheetTimer = 99f;
        RefreshSheet();
    }

    void RefreshSheet()
    {
        if (sheet == null) return;
        chart.Draw(app.session.runs.Concat(new[] { training }).ToList());
        var s = training.s;
        int shown = Mathf.Clamp(phase == Phase.Training ? attemptAt : replay.Count - 1, 0, replay.Count - 1);
        var a = replay.attempts[shown];
        var st = a.steps[Mathf.Clamp(stepAt, 0, a.steps.Count - 1)];
        var words = Driving.Words(st.readings, s.sensors);
        readingsText.text = phase == Phase.Training
            ? $"Attempt {a.index} of {s.attempts}  ·  seed {s.seed}\nSenses: {words}\nMove: {Moves.Names[st.move]}  ·  {st.reward:+0.#;-0.#}\nNew move chance {st.explore:0.00}  ·  {a.book.pages.Count} situations"
            : $"The race  ·  the rule book as it is  ·  {race.unknown} unknown situations\nSenses: {Driving.Words(race.readings, s.sensors)}";
        Ui.Clear(bookContent);
        var book = phase == Phase.Training ? a.book : training.book;
        var pages = book.pages.ToList();
        string now = phase == Phase.Training ? st.key : Driving.Key(race.readings, s.sensors);
        pages.Sort((x, y) => x.Key == now ? -1 : y.Key == now ? 1 : string.CompareOrdinal(x.Key, y.Key));
        float y = 0;
        foreach (var page in pages.Take(40))
        {
            bool current = page.Key == now;
            int best = book.Best(page.Key, s.moves);
            var row = Ui.Box(bookContent, "Row", current ? new Color(0.07f, 0.32f, 0.36f, 0.95f) : Ui.RowFill, false);
            Ui.Place(row.rectTransform, 0, y, 600, 32);
            var sit = Ui.Text(row.transform, Driving.Words(Driving.Parse(page.Key), s.sensors), 16, current ? Color.white : Theme.Faint, current);
            Ui.Place(sit.rectTransform, 12, 5, 430, 24);
            sit.enableWordWrapping = false; sit.overflowMode = TextOverflowModes.Ellipsis;
            var mv = Ui.Text(row.transform, best >= 0 ? Moves.Names[best] : "?", 16, Theme.Teal, true, TextAlignmentOptions.TopRight);
            Ui.Place(mv.rectTransform, 440, 5, 148, 24);
            y += 36;
        }
        if (pages.Count == 0) { var empty = Ui.Text(bookContent, "Nothing yet", 16, Theme.Faint); Ui.Place(empty.rectTransform, 12, 5, 600, 24); y = 32; }
        bookContent.sizeDelta = new Vector2(0, y);
    }

    // ---- moments ----

    public void Announce(string text, string sub, Color colour, float seconds, bool big) => banners.Enqueue((text, sub, colour, seconds, big));

    static float OutBack(float k) { const float c = 1.70158f; k -= 1f; return 1f + (c + 1f) * k * k * k + c * k * k; }

    IEnumerator Moment(string text, string sub, Color colour, float seconds, bool big)
    {
        float worldW = sheet != null ? Ui.W - 640 : Ui.W;
        Image flash = null;
        if (big)
        {
            flash = Ui.Box(bannerLayer, "Flash", new Color(colour.r, colour.g, colour.b, 0f), false);
            Ui.Place(flash.rectTransform, 0, 0, worldW, Ui.H);
            flash.raycastTarget = false;
        }
        var card = Ui.Box(bannerLayer, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        float h = big ? (string.IsNullOrEmpty(sub) ? 150 : 180) : 92;
        float w = big ? 760 : 620;
        Ui.Place(card.rectTransform, (worldW - w) / 2, 420 - h / 2, w, h);
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", colour, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, h);
        var t = Ui.Text(card.transform, text, big ? (text.Length <= 2 ? 96 : 60) : 36, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = big ? 6 : 4;
        Ui.Place(t.rectTransform, 20, big ? 14 : 20, w - 40, big ? 100 : 52);
        t.enableWordWrapping = false;
        if (!string.IsNullOrEmpty(sub))
        {
            var st = Ui.Text(card.transform, sub, big ? 20 : 16, Look.AsphaltLine, false, TextAlignmentOptions.Center);
            Ui.Place(st.rectTransform, 20, h - (big ? 52 : 32), w - 40, 26);
            st.enableWordWrapping = false; st.overflowMode = TextOverflowModes.Ellipsis;
        }
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(worldW / 2, -420);
        if (big) Sfx.Sting(colour == Theme.Danger ? Sfx.Mood.Alarm : text == "GO" ? Sfx.Mood.Go : Sfx.Mood.Fanfare); else Sfx.Tick();
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
        void Update()
        {
            if (screen == null) return;
            screen.Tick(Time.deltaTime);
            screen.sheetTimer += Time.deltaTime;
            if (screen.sheet != null && screen.sheetTimer > 0.5f) { screen.sheetTimer = 0f; screen.RefreshSheet(); }
        }
        void OnDestroy() { if (Current == screen) Current = null; }
    }
}
