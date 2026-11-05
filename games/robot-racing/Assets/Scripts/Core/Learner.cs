using System;
using System.Collections.Generic;
using System.Linq;

// The rule book the robot writes itself: for every situation it has met, how
// much each move paid, and so the move it will make there. The method under
// the hood is never named on screen; what the student sees is the attempt
// count, the points per attempt as a curve, the sensors' readings and this book.
public class RuleBook
{
    public readonly Dictionary<string, float[]> pages = new Dictionary<string, float[]>();

    public float[] Of(string key)
    {
        if (!pages.TryGetValue(key, out var v)) { v = new float[Moves.Count]; pages[key] = v; }
        return v;
    }

    public bool Knows(string key) => pages.ContainsKey(key);

    // The best allowed move in a situation; -1 when the situation is unknown.
    public int Best(string key, bool[] allowed)
    {
        if (!pages.TryGetValue(key, out var v)) return -1;
        int best = -1;
        for (int m = 0; m < Moves.Count; m++) if (allowed[m] && (best < 0 || v[m] > v[best])) best = m;
        return best;
    }

    public RuleBook Copy()
    {
        var c = new RuleBook();
        foreach (var kv in pages) c.pages[kv.Key] = (float[])kv.Value.Clone();
        return c;
    }
}

public class Attempt
{
    public int index;
    public float points;
    public bool finished, crashed;
    public int steps, touches;
    public float explore;
}

// One training run: the settings, the book it builds, the curve it draws. The
// run screen calls Tick() as many times a frame as the speed control allows.
public class Training
{
    public readonly Settings s;
    public readonly string label;
    public readonly RuleBook book = new RuleBook();
    public readonly List<Attempt> curve = new List<Attempt>();
    public readonly Car car = new Car();
    public Track track;
    public int[] readings = new int[Sensors.Count];
    public string key = "";
    public int lastMove = Moves.Hold;
    public float explore, attemptPoints, lastReward;
    public int firstFinish = -1, settled = -1;
    public bool done, stopped;
    readonly Random rng;
    int trackIndex;
    const float Keep = 0.95f; // how much the next situation's promise counts
    const int MaxSteps = 90;

    public Training(Settings settings, string label = "")
    {
        s = settings;
        this.label = label;
        rng = new Random(s.seed);
        explore = s.explore;
        track = Track.Get(s.training[0]);
        car.Reset(track);
        readings = Driving.Sense(track, car);
        key = Driving.Key(readings, s.sensors);
    }

    public int Attempts => curve.Count;
    public Attempt Last => curve.Count > 0 ? curve[curve.Count - 1] : null;
    public float Best => curve.Count > 0 ? curve.Max(a => a.points) : 0;
    public int Finishes(int lastN) => curve.Skip(Math.Max(0, curve.Count - lastN)).Count(a => a.finished);
    public float Progress => s.attempts > 0 ? Math.Min(1f, curve.Count / (float)s.attempts) : 1f;

    // One move. True when the attempt ended.
    public bool Tick()
    {
        if (done) return false;
        var values = book.Of(key);
        int move = Choose(values);
        var o = Driving.Step(track, car, move, s.topSpeed);
        float r = s.moment + o.progress * (o.progress > 0 ? s.toward : -s.away) + s.speedBonus * car.speed;
        if (o.crashed) r += s.crash;
        if (o.finished) r += s.finish;
        if (car.steps >= MaxSteps && !o.crashed && !o.finished) car.crashed = true; // out of time counts as a failed attempt
        lastReward = r;
        attemptPoints += r;
        lastMove = move;

        var next = Driving.Sense(track, car);
        string nextKey = Driving.Key(next, s.sensors);
        float promise = car.crashed || car.finished ? 0 : BestValue(nextKey);
        values[move] += s.rewrite * (r + Keep * promise - values[move]);
        readings = next;
        key = nextKey;

        if (!car.crashed && !car.finished) return false;
        EndAttempt();
        return true;
    }

    int Choose(float[] values)
    {
        var allowed = Enumerable.Range(0, Moves.Count).Where(m => s.moves[m]).ToList();
        if (allowed.Count == 0) return Moves.Hold;
        if (rng.NextDouble() < explore) return allowed[rng.Next(allowed.Count)];
        int best = allowed[0];
        foreach (int m in allowed) if (values[m] > values[best]) best = m;
        // Unknown ground, all zeros: pick at random rather than always the first.
        if (allowed.All(m => values[m] == values[best])) return allowed[rng.Next(allowed.Count)];
        return best;
    }

    float BestValue(string k)
    {
        if (!book.pages.TryGetValue(k, out var v)) return 0;
        float best = float.MinValue;
        for (int m = 0; m < Moves.Count; m++) if (s.moves[m]) best = Math.Max(best, v[m]);
        return best == float.MinValue ? 0 : best;
    }

    void EndAttempt()
    {
        var a = new Attempt { index = curve.Count + 1, points = attemptPoints, finished = car.finished, crashed = car.crashed && !car.finished, steps = car.steps, touches = car.touches, explore = explore };
        curve.Add(a);
        if (a.finished && firstFinish < 0) firstFinish = a.index;
        if (settled < 0 && curve.Count >= 20 && Finishes(20) >= 16) settled = a.index;
        if (s.exploreOffAfterFinish && a.finished) explore = 0;
        else explore = Math.Max(s.exploreMin, explore * s.exploreFall);
        if (s.stopAtFirstFinish && a.finished) stopped = true;
        if (curve.Count >= s.attempts || stopped) { done = true; return; }
        trackIndex = (trackIndex + 1) % s.training.Length;
        track = Track.Get(s.training[trackIndex]);
        car.Reset(track);
        attemptPoints = 0;
        readings = Driving.Sense(track, car);
        key = Driving.Key(readings, s.sensors);
    }

    // The curve in a readable form: the trailing finish rate at each attempt.
    public static string Describe(Training t) =>
        t.curve.Count == 0 ? "no attempts yet"
        : $"{t.curve.Count} attempts, first finish at {(t.firstFinish > 0 ? t.firstFinish.ToString() : "never")}, settled at {(t.settled > 0 ? t.settled.ToString() : "never")}, best {t.Best:0} points";
}

// A race: laps on a track with the book as it is, no new moves, no learning.
public class Race
{
    public readonly Track track;
    public readonly Training from;
    public readonly List<Attempt> laps = new List<Attempt>();
    public readonly Car car = new Car();
    public int[] readings = new int[Sensors.Count];
    public bool done;
    public bool learning;   // the real run learns nothing
    readonly int lapsWanted;
    readonly Random rng;
    const int MaxSteps = 90;

    public Race(Training from, Track track, int laps, int seed)
    {
        this.from = from; this.track = track; lapsWanted = laps;
        rng = new Random(seed);
        car.Reset(track);
        readings = Driving.Sense(track, car);
    }

    public int Lap => laps.Count + 1;
    public int Finishes => laps.Count(l => l.finished);
    public int Crashes => laps.Count(l => l.crashed);
    public float Touches => laps.Count == 0 ? 0 : (float)laps.Sum(l => l.touches) / laps.Count;
    public float LapTime => laps.Any(l => l.finished) ? (float)laps.Where(l => l.finished).Average(l => l.steps) : 0;
    public float FinishRate => laps.Count == 0 ? 0 : (float)Finishes / laps.Count;
    public float CrashesPerLap => laps.Count == 0 ? 0 : (float)Crashes / laps.Count;
    public float Measure(int which) => which == 0 ? FinishRate : which == 1 ? CrashesPerLap : which == 2 ? Touches : LapTime;
    public int unknown; // moves made in situations the book had never met

    public bool Tick()
    {
        if (done) return false;
        string key = Driving.Key(readings, from.s.sensors);
        int move = from.book.Best(key, from.s.moves);
        if (move < 0) { var allowed = Enumerable.Range(0, Moves.Count).Where(m => from.s.moves[m]).ToList(); move = allowed[rng.Next(allowed.Count)]; unknown++; }
        var o = Driving.Step(track, car, move, from.s.topSpeed);
        if (car.steps >= MaxSteps && !car.crashed && !car.finished) car.crashed = true;
        readings = Driving.Sense(track, car);
        if (!car.crashed && !car.finished) return false;
        laps.Add(new Attempt { index = laps.Count + 1, finished = car.finished, crashed = car.crashed && !car.finished, steps = car.steps, touches = car.touches });
        if (laps.Count >= lapsWanted) { done = true; return true; }
        car.Reset(track);
        readings = Driving.Sense(track, car);
        return true;
    }
}
