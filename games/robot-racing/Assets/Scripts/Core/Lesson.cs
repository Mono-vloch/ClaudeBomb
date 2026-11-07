using System;
using System.Collections.Generic;
using System.Linq;

public enum Practised { Yes, Missed, NotNeeded }

// A warm-up question from an earlier level: no score; a wrong answer shows the right one.
public class Recall { public string question; public string[] options; public int answer; public string from; }

// A ready-made setup: for "Watch the pro" (it learns) or "Fix the broken bot" (the
// level's planned mistake). Some mistakes need two robots, run one after the other.
public class Setup
{
    public string name, why;
    public Settings a, b;
    public string aName = "The ready robot", bName;
}

// One question on the debrief: a choice, or one line of text. "rulebook" asks
// about a situation from the robot's own book, so the answer is only known then.
public class Question
{
    public string key, prompt;
    public string[] options;
    public int answer = -1;
    public bool rulebook;
    public bool IsText => options == null && !rulebook;
}

// Everything one level gathers: the runs, the races, the real run, the answers.
public class Session
{
    public Lesson lesson;
    public Settings settings;
    public readonly List<Training> runs = new List<Training>();
    public readonly List<Race> races = new List<Race>();
    public Race real;
    public readonly Dictionary<string, int> picks = new Dictionary<string, int>();
    public readonly Dictionary<string, string> texts = new Dictionary<string, string>();
    public string prediction = "";
    public Training Last => runs.LastOrDefault();
    public Race LastRace => races.LastOrDefault();
    public bool Picked(string key, int answer) => picks.TryGetValue(key, out int p) && p == answer;
    public bool Wrote(string key) => texts.TryGetValue(key, out string t) && !string.IsNullOrWhiteSpace(t) && t.Trim().Length >= 8;
    public int Right(params string[] keys) => keys.Count(k => lesson.quiz.Any(q => q.key == k && !q.IsText && !q.rulebook && Picked(k, q.answer)));
    public bool RaceOk(Race r, int of = 5) => r != null && r.Finishes * 5 >= of * 3; // most of the laps
}

// One step of the ladder, from Docs/curriculum/robot-racing.md.
public class Lesson
{
    public int number, tier;
    public string id, title, kicker, luma, learn, live, takeaway, ask;
    public string[] training = { "bend" };
    public string test = "bend";
    public bool sensors, moves, points, explore, knobs, tracks, measure, attempts; // what the student may set
    public bool flag;              // 1.1: the finish flag alone, before the points table opens at 2.3
    public bool prediction;        // one line before training: what will it do?
    public string predictionPrompt = "Before you run: what will this robot do?";
    public bool realRun;           // 3.4: one lap on the real track, no learning
    public int laps = 5;           // the race after training
    public Recall recall;
    public Setup ready, broken;
    public Question[] quiz = new Question[0];
    public Func<Session, List<(Practised result, string label)>> check;

    public bool Capstone => title.StartsWith("Capstone");

    // Everything a level has not opened is fixed at the sensible value.
    public static Settings Fixed => new Settings();

    public void Apply(Settings s)
    {
        var f = Fixed;
        if (!sensors) s.sensors = (bool[])f.sensors.Clone();
        if (!moves) { s.moves = (bool[])f.moves.Clone(); s.topSpeed = f.topSpeed; }
        if (!points) { s.finish = f.finish; s.crash = f.crash; s.toward = f.toward; s.away = f.away; s.moment = f.moment; s.speedBonus = f.speedBonus; }
        if (!explore) { s.explore = f.explore; s.exploreMin = f.exploreMin; s.exploreFall = f.exploreFall; s.exploreOffAfterFinish = false; }
        if (!knobs) { s.rewrite = f.rewrite; s.seed = f.seed; }
        if (!attempts && !knobs) s.attempts = f.attempts;
        if (!tracks) { s.training = (string[])training.Clone(); s.test = test; }
        if (!measure) s.measure = 0;
        s.stopAtFirstFinish = false;
    }

    // The level's own settings, fresh.
    public Settings Start()
    {
        var s = Fixed;
        s.training = (string[])training.Clone();
        s.test = test;
        return s;
    }

    public List<(Practised result, string label)> Check(Session x) => check(x);
}

public static class Ladder
{
    public const int AttemptsLevel = 2, SensorsLevel = 4, MovesLevel = 5, PointsLevel = 6, ExploreLevel = 8, TracksLevel = 10, MeasureLevel = 11, KnobsLevel = 12;
    static Practised Of(bool ok) => ok ? Practised.Yes : Practised.Missed;

    static Settings On(string[] training, string test, Action<Settings> change = null)
    {
        var s = new Settings { training = training, test = test };
        change?.Invoke(s);
        return s;
    }

    static readonly Question[] FiveParts =
    {
        new Question { key = "p1", prompt = "The one making the decisions.", options = new[] { "The robot", "The track", "What it senses", "What it does", "The points" }, answer = 0 },
        new Question { key = "p2", prompt = "The world it drives in.", options = new[] { "The robot", "The track", "What it senses", "What it does", "The points" }, answer = 1 },
        new Question { key = "p3", prompt = "What it knows about this moment.", options = new[] { "The robot", "The track", "What it senses", "What it does", "The points" }, answer = 2 },
        new Question { key = "p4", prompt = "Speed up, slow down, turn.", options = new[] { "The robot", "The track", "What it senses", "What it does", "The points" }, answer = 3 },
        new Question { key = "p5", prompt = "A number that says how good or bad the move was.", options = new[] { "The robot", "The track", "What it senses", "What it does", "The points" }, answer = 4 },
    };

    public static readonly Lesson[] All =
    {
        new Lesson
        {
            number = 1, tier = 1, id = "1.1", title = "Who is driving?",
            kicker = "Nobody. It senses, moves, gets points, and goes again.",
            luma = "Hi, I'm LUMA. Nobody drives this robot. Press run and watch it crash, then crash later, then finish.",
            learn = "Nobody drives. Every moment the robot senses, chooses a move, makes it, and gets points. It starts knowing nothing and moves at random; " +
                    "the moves that earned points it makes more often. That is all.\n\nThe five parts: the robot, the track, what it senses, what it does, the points. Watch them on the run.",
            training = new[] { "bend" }, test = "bend", flag = true,
            ready = new Setup { name = "Watch the pro", a = On(new[] { "bend" }, "bend", s => s.attempts = 120),
                why = "A ready robot on a short track. The finish pays, a crash costs, every cell closer to the finish pays a little. Watch attempt 1, then attempt 100." },
            broken = new Setup { name = "No points at all", a = On(new[] { "bend" }, "bend", s => { s.finish = 0; s.crash = 0; s.toward = 0; s.away = 0; s.moment = 0; s.attempts = 120; }),
                why = "Every move earns zero. Attempt 100 looks like attempt 1, because the points are the only thing it learns from. Turn the finish points on and run again." },
            live = "Nobody is steering. The moves are random at first; the ones that earned points come back more often. The curve is this attempt's points.",
            quiz = FiveParts.Concat(new[]
            {
                new Question { key = "start", prompt = "What does the robot know at the start?", options = new[] { "Nothing", "The shape of the track", "Which moves are good" }, answer = 0 },
                new Question { key = "why", prompt = "Why did the no-points robot never improve?", options = new[] { "Every move earned zero, so there was nothing to learn from", "The track was too long", "It needed more sensors" }, answer = 0 },
            }).ToArray(),
            takeaway = "The robot learns from the points, and from nothing else.",
            ask = "What do we reward, and what do we hope for?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.Right("p1", "p2", "p3", "p4", "p5") >= 4), "Named the five parts"),
                (Of(x.Picked("start", 0)), "Said what the robot knows at the start: nothing"),
                (Of(x.Picked("why", 0)), "Found why the first setup never improved"),
                (Of(x.Last != null && x.Last.firstFinish > 0), "Trained a robot to a finish"),
            },
        },
        new Lesson
        {
            number = 2, tier = 1, id = "1.2", title = "Is it learning, or just lucky?",
            kicker = "One good attempt proves nothing. Learning is the curve going up and staying up.",
            luma = "A random robot finishes sometimes. Now you decide how many attempts it trains before the race, and read the rule book it writes itself.",
            learn = "A random robot finishes sometimes, so one finish proves nothing. Learning shows as the total points per attempt rising and staying up.\n\n" +
                    "What it has learned is a rule book it wrote itself: for each situation it has met, the move that paid. This level opens how many attempts to train before the race.",
            training = new[] { "twobends" }, test = "twobends", attempts = true,
            recall = new Recall { question = "What does the robot know at the start?", options = new[] { "Nothing", "The shape of the track", "Which moves are good" }, answer = 0, from = "1.1" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "twobends" }, "twobends", s => s.attempts = 250),
                why = "Trained for 250 attempts: enough for the curve to settle. Then five races with no new moves and no learning." },
            broken = new Setup { name = "Stopping at the first finish", a = On(new[] { "twobends" }, "twobends", s => { s.attempts = 400; s.stopAtFirstFinish = true; }),
                why = "Training stops the first time it finishes. One lucky attempt, and off to the race, where it crashes. Train until the curve settles, and race again." },
            live = "Watch the curve, not one attempt. When it rises and stays up, the robot has learned. The rule book on the right is what it learned.",
            quiz = new[] { new Question { key = "book", prompt = "Read the rule book. In this situation, what would the robot do?", rulebook = true } },
            takeaway = "One good attempt proves nothing. Learning is the curve going up and staying up.",
            ask = "When we say a system \"works\", how many attempts did we watch?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.Last != null && x.Last.settled > 0), "Trained until the curve settled"),
                (Of(x.RaceOk(x.LastRace)), "The robot finished most of several races, not one"),
                (Of(x.Picked("book", 1)), "Read the rule book for one situation"),
            },
        },
        new Lesson
        {
            number = 3, tier = 1, id = "1.3", title = "Capstone: What kind of learning is this?",
            kicker = "No answer key and no examples. Only tries, points, and more tries.",
            luma = "A new track, a ready robot. Train it, say when it has learned, name the five parts, and sort six problems into the three kinds of learning.",
            learn = "Three kinds of learning. Studying with an answer key: supervised. Finding groups with no key: unsupervised. Learning to ride a bike: reinforcement, " +
                    "which is this robot. It was never shown a correct lap. It tried, got points, and tried again.\n\nTrain it on the hairpin, read its curve, then sort the problems.",
            training = new[] { "hairpin" }, test = "hairpin", attempts = true,
            recall = new Recall { question = "What shows that the robot has learned?", options = new[] { "The curve rising and staying up", "One finish", "A full rule book" }, answer = 0, from = "1.2" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "hairpin" }, "hairpin", s => s.attempts = 300), why = "A ready setup that learns the hairpin. Read its curve and say when it has learned." },
            live = "A new track, the same loop: sense, move, points, again. Nobody shows it a correct lap.",
            quiz = FiveParts.Concat(new[]
            {
                new Question { key = "learned", prompt = "Has this robot learned the hairpin?", options = new[] { "Yes: its curve rose and stayed up", "No: its curve never settled", "One finish is enough to say yes" }, answer = 0 },
                new Question { key = "s1", prompt = "Sorting spam email from the rest, with thousands of labeled examples.", options = new[] { "Supervised", "Unsupervised", "Reinforcement" }, answer = 0 },
                new Question { key = "s2", prompt = "Grouping customers who behave alike, with no groups given.", options = new[] { "Supervised", "Unsupervised", "Reinforcement" }, answer = 1 },
                new Question { key = "s3", prompt = "Predicting a used car's price from past sales.", options = new[] { "Supervised", "Unsupervised", "Reinforcement" }, answer = 0 },
                new Question { key = "s4", prompt = "Spotting unusual network traffic, without knowing what unusual looks like.", options = new[] { "Supervised", "Unsupervised", "Reinforcement" }, answer = 1 },
                new Question { key = "s5", prompt = "Predicting which loans will default, from loans that did and did not.", options = new[] { "Supervised", "Unsupervised", "Reinforcement" }, answer = 0 },
                new Question { key = "s6", prompt = "This robot, learning the track.", options = new[] { "Supervised", "Unsupervised", "Reinforcement" }, answer = 2 },
                new Question { key = "notsup", prompt = "In one line: why is the robot not supervised learning?" },
            }).ToArray(),
            takeaway = "No answer key and no examples. Only tries, points, and more tries.",
            ask = "Which of our problems at work has an answer key, and which only has tries and results?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.Right("p1", "p2", "p3", "p4", "p5") >= 4), "Named the five parts"),
                (Of(x.Last != null && ((x.Last.settled > 0 && x.Picked("learned", 0)) || (x.Last.settled <= 0 && x.Picked("learned", 1)))), "Read the curve correctly"),
                (Of(x.Right("s1", "s2", "s3", "s4", "s5", "s6") >= 5), "Sorted the six problems"),
                (Of(x.Wrote("notsup")), "Explained why the robot is not supervised learning"),
            },
        },
        new Lesson
        {
            number = 4, tier = 2, id = "2.1", title = "What does it sense?",
            kicker = "The robot's world is its sensors. What it cannot sense, it cannot learn.",
            luma = "Now you choose which sensors are on. Two moments its sensors cannot tell apart are one situation to it.",
            learn = "The robot's situation is only what its sensors report. Two moments its sensors cannot tell apart are one situation to it, and it makes the same move in both. " +
                    "A wall it cannot sense does not exist until it hits it.\n\nThis level opens the sensors. This track has a left bend and then a right one.",
            training = new[] { "bothways" }, test = "bothways", sensors = true,
            recall = new Recall { question = "Which of the three kinds of learning has no answer key?", options = new[] { "Learning to ride a bike: reinforcement", "Studying with an answer key: supervised", "Finding groups: unsupervised" }, answer = 0, from = "1.3" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "bothways" }, "bothways", s => { s.sensors = new[] { true, true, true, true, false }; s.attempts = 250; }),
                why = "Front, left, right and speed on. At the left bend the right sensor reads wall and the left reads clear; at the right bend the other way round. Two situations, two moves." },
            broken = new Setup { name = "No sensor facing the bend", a = On(new[] { "bothways" }, "bothways", s => { s.sensors = new[] { true, false, false, true, false }; s.attempts = 250; }),
                why = "A forward sensor only. The left bend and the right bend look the same: wall ahead. One situation, one move, and one of the bends is a crash every time. Switch on the sensors that face the bends." },
            live = "The bars on the right are the sensors. Watch the rule book: with the side sensors on, one situation became two.",
            quiz = new[] { new Question { key = "why", prompt = "Why could the front-only robot not learn both bends?", options = new[] { "The two bends looked the same to it, so it made the same move at both", "It was too slow", "The track was too long" }, answer = 0 } },
            takeaway = "The robot's world is its sensors. What it cannot sense, it cannot learn.",
            ask = "What does our system actually measure, and what do we assume it sees?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.RaceOk(x.LastRace)), "The robot got past both bends"),
                (Of(x.Picked("why", 0)), "Explained why the first robot could not learn it"),
            },
        },
        new Lesson
        {
            number = 5, tier = 2, id = "2.2", title = "What may it do?",
            kicker = "It can only learn the moves you allowed.",
            luma = "Now you choose the moves it may make, and one thing it must never do. A move you left out is a bend it can never take.",
            learn = "The robot can only learn moves it is allowed. A move missing from the set is a bend it can never take. A constraint is a move it must never make, " +
                    "whatever the points say: a top speed, for example.\n\nThis level opens the moves and the constraint. The chicane leaves one clear cell after the bend.",
            training = new[] { "chicane" }, test = "chicane", moves = true,
            recall = new Recall { question = "What happens to two situations the sensors cannot tell apart?", options = new[] { "They are one situation to the robot: it makes the same move in both", "The robot slows down", "The robot crashes at once" }, answer = 0, from = "2.1" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "chicane" }, "chicane", s => { s.attempts = 250; s.constraintSet = true; }),
                why = "All five moves, top speed 2. Watch the rule book use slow down before the chicane, where two cells at full speed would be a wall." },
            broken = new Setup { name = "A robot that cannot slow down", a = On(new[] { "chicane" }, "chicane", s => { s.moves = new[] { true, false, false, true, true }; s.attempts = 250; }),
                why = "Speed up and turns only: it can never slow down. It learns the straight and crashes at the chicane on every attempt, because full speed needs two clear cells. Add slow down and watch where the book uses it." },
            live = "Watch the rule book at the chicane: the moves it may make are the only moves it can learn.",
            takeaway = "It can only learn the moves you allowed.",
            ask = "Which options have we given the system, and which did we forget?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.RaceOk(x.LastRace)), "The robot finished"),
                (Of(x.settings != null && x.settings.constraintSet), "A constraint was set"),
                (Of(x.Last != null && x.Last.book.pages.Any(p => x.Last.book.Best(p.Key, x.Last.s.moves) == Moves.SlowDown)), "The rule book uses slow down where it matters"),
            },
        },
        new Lesson
        {
            number = 6, tier = 2, id = "2.3", title = "What earns points?",
            kicker = "Points only at the end leave the robot in the dark. Small points on the way light the path.",
            luma = "Now you write the points table. Points only at the finish leave the robot in the dark for hundreds of attempts.",
            learn = "Points only at the end leave the robot in the dark for thousands of attempts: nothing tells it which moves helped. Small points on the way light the path. " +
                    "A crash must cost more than any shortcut earns.\n\nThis level opens the points table: the finish, a crash, each step toward the finish, each step away, each moment spent.",
            training = new[] { "hairpin" }, test = "hairpin", points = true,
            recall = new Recall { question = "What is a constraint?", options = new[] { "A move the robot must never make, whatever the points say", "A sensor", "The finish line" }, answer = 0, from = "2.2" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "hairpin" }, "hairpin", s => s.attempts = 250),
                why = "The finish pays 100, a crash costs 100, every cell closer pays 2 and every cell further costs 2. The path is lit." },
            broken = new Setup { name = "Finish points only", a = On(new[] { "hairpin" }, "hairpin", s => { s.toward = 0; s.away = 0; s.moment = 0; s.crash = 0; s.attempts = 250; }),
                why = "100 at the finish and nothing else. The curve stays flat far past the budget: in the dark, a random walk rarely reaches the end. Add the small on-the-way points and compare attempts to the first finish." },
            live = "Compare attempts to the first finish, table by table. The debrief puts every table you tried side by side.",
            takeaway = "Points only at the end leave the robot in the dark. Small points on the way light the path.",
            ask = "Where do we only measure the end result, and leave the people in the dark on the way?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.settings != null && x.settings.finish > 0 && x.settings.crash < 0 && x.settings.toward > 0), "Set finish, crash and on-the-way points"),
                (Of(x.Last != null && x.Last.settled > 0), "Learned within the attempt budget"),
                (Of(x.settings != null && -x.settings.crash > x.settings.TopPointsPerMove), "The crash costs more than any single move can earn"),
            },
        },
        new Lesson
        {
            number = 7, tier = 2, id = "2.4", title = "What does it learn when you reward the wrong thing?",
            kicker = "It optimizes exactly what you reward, which is not always what you meant.",
            luma = "This table pays for speed. Before you run it, write one line: what will the robot do? Then watch the curve rise while the lap count stays at zero.",
            learn = "The robot does not know what you meant. It finds the cheapest way to the number. A loophole in the table is a lap it will never drive.\n\n" +
                    "The plaza is an open square: room to drive in circles, if circling pays. Before each run, predict what the robot will do.",
            training = new[] { "plaza" }, test = "plaza", points = true, prediction = true,
            predictionPrompt = "Before you run: what will this robot do with this points table?",
            recall = new Recall { question = "Why did the finish-only robot stay in the dark?", options = new[] { "It got no points until the finish, so nothing told it which moves helped", "It had no sensors", "The track was too short" }, answer = 0, from = "2.3" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "plaza" }, "plaza", s => s.attempts = 250), why = "Points for progress, none for speed. It crosses the plaza and finishes." },
            broken = new Setup { name = "The speed bonus", a = On(new[] { "plaza" }, "plaza", s => { s.toward = 0; s.away = 0; s.moment = 0; s.speedBonus = 3; s.attempts = 250; }),
                why = "Three points per moment for every unit of speed, and nothing for the finish line's direction. It drives circles in the plaza at full speed forever, and its curve rises. The curve measures the points, not the job. Close the loophole." },
            live = "Watch two numbers: the curve, and the laps finished. A curve can rise while the laps stay at zero.",
            quiz = new[] { new Question { key = "rise", prompt = "How can a curve rise while the robot never finishes?", options = new[] { "The curve measures the points, and the points can be earned without finishing", "The curve measures laps", "The robot was lucky" }, answer = 0 } },
            takeaway = "It optimizes exactly what you reward, which is not always what you meant.",
            ask = "Where do we reward the number and hope for the goal?",
            check = x => new List<(Practised, string)>
            {
                (Of(!string.IsNullOrWhiteSpace(x.prediction) && x.prediction.Trim().Length >= 8), "Predicted what the robot would do before running"),
                (Of(x.RaceOk(x.LastRace)), "The fixed robot finished"),
                (Of(x.Picked("rise", 0)), "Explained why a curve can rise while the robot never finishes"),
            },
        },
        new Lesson
        {
            number = 8, tier = 2, id = "2.5", title = "Should it try something new?",
            kicker = "Try a lot early, settle later.",
            luma = "Now you set how often it tries something new. Two ready robots: one never tries anything new after its first finish, one never stops trying.",
            learn = "A robot that only repeats the first thing that worked is stuck with a slow line. A robot that keeps trying new things forever never settles. " +
                    "Try a lot early, settle later.\n\nThis level opens the chance it tries something new, and how that chance falls over the attempts.",
            training = new[] { "twobends" }, test = "twobends", explore = true,
            recall = new Recall { question = "What does a rising curve measure?", options = new[] { "The points per attempt, which is not always the job", "The laps finished", "The speed" }, answer = 0, from = "2.4" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "twobends" }, "twobends", s => s.attempts = 250), why = "Nine in ten moves are new at the start, falling to one in twenty. It explores early and settles." },
            broken = new Setup
            {
                name = "Never again, and forever", aName = "Never again", bName = "Forever",
                a = On(new[] { "twobends" }, "twobends", s => { s.exploreOffAfterFinish = true; s.attempts = 250; }),
                b = On(new[] { "twobends" }, "twobends", s => { s.explore = 0.5f; s.exploreMin = 0.5f; s.exploreFall = 1f; s.attempts = 250; }),
                why = "Never again: it stops trying anything new after its first finish and keeps a slow, wide line. Forever: it tries something new on every second move to the end, and its curve never settles. Set a high chance that falls.",
            },
            live = "The number on the right is the chance of a new move, live. Compare the two curves.",
            takeaway = "Try a lot early, settle later.",
            ask = "When did we last try something other than what worked first?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.settings != null && x.settings.explore >= 0.5f && x.settings.exploreFall < 1f && x.settings.exploreMin < x.settings.explore && !x.settings.exploreOffAfterFinish), "Set a high chance that falls"),
                (Of(x.Last != null && x.Last.firstFinish > 0 && x.Last.curve.Skip(x.Last.firstFinish).Any(a => a.points > x.Last.curve[x.Last.firstFinish - 1].points + 1)), "The robot's line improved after its first finish"),
                (Of(x.Last != null && x.Last.settled > 0), "The curve settled"),
            },
        },
        new Lesson
        {
            number = 9, tier = 2, id = "2.6", title = "Capstone: Your robot, your rules",
            kicker = "You define what it senses, what it may do and what earns points. It figures out the rest.",
            luma = "The grand circuit, everything open, no help. Write one line on what your robot will do, then train it and race.",
            learn = "Everything at once: the sensors, the moves and the constraint, the points table, how much it explores. The grand circuit has a plaza that pays for a loophole " +
                    "and a chicane that punishes full speed.\n\nBefore you run, write one line on what the robot will do. Then the class race.",
            training = new[] { "grand" }, test = "grand", sensors = true, moves = true, points = true, explore = true, attempts = true, prediction = true,
            predictionPrompt = "Before you run: how will your robot behave on the grand circuit?",
            recall = new Recall { question = "What goes wrong when the robot never tries anything new after its first finish?", options = new[] { "It keeps a slow line and never improves", "It crashes", "It forgets the track" }, answer = 0, from = "2.5" },
            live = "Everything is in play. If it circles the plaza, ask the table why. If it crashes at the chicane, ask the moves.",
            quiz = new[] { new Question { key = "matched", prompt = "Did it do what you predicted? In one line: what matched, or what differed and why." } },
            takeaway = "You define what it senses, what it may do and what earns points. It figures out the rest, exactly as you wrote it.",
            ask = "What did we define for our system, and what did it figure out that we did not mean?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.Last != null && x.Last.settled > 0 && x.Last.settled <= x.Last.s.attempts), "Finished within the attempt budget"),
                (Of(x.RaceOk(x.LastRace) && (x.settings == null || x.settings.speedBonus <= x.settings.toward)), "No loophole exploited"),
                (Of(x.Wrote("matched")), "Compared the prediction with what happened"),
                (Of(x.settings != null && x.settings.sensors[Sensors.Front] && x.settings.sensors[Sensors.Left] && x.settings.sensors[Sensors.Right]), "The sensors on are the ones the track needed"),
            },
        },
        new Lesson
        {
            number = 10, tier = 3, id = "3.1", title = "Did it learn the track, or learn to drive?",
            kicker = "The only proof it learned to drive is a track it has never seen.",
            luma = "Now you choose the training tracks and a test track it never trains on. A perfect lap on the training track proves it knows that track.",
            learn = "A perfect lap on the training track proves the robot knows that track. The only proof it learned to drive is a track it has never seen. The test track is never trained on.\n\n" +
                    "This level opens the training tracks and the test track. The test bend turns right; one training track only ever turns left.",
            training = new[] { "leftonly", "twobends", "bend" }, test = "rightbend", tracks = true, attempts = true,
            recall = new Recall { question = "Name the setting you change to make the robot try more new moves.", options = new[] { "The chance of a new move", "The top speed", "The finish points" }, answer = 0, from = "2.5" },
            ready = new Setup { name = "Watch the pro", a = On(new[] { "leftonly", "twobends", "bend" }, "rightbend", s => s.attempts = 400), why = "Trained on three tracks with bends both ways. On the test bend, a right bend it has never driven, it still turns." },
            broken = new Setup { name = "One track only", a = On(new[] { "leftonly" }, "rightbend", s => s.attempts = 400),
                why = "Trained on left turns only, it laps that track perfectly. On the test track it meets a right bend for the first time: it memorized, it did not learn to drive. Train it on several tracks and test again." },
            live = "The curve is the training tracks. The race is the test track, never trained on. Watch the first bend that is different.",
            quiz = new[] { new Question { key = "memo", prompt = "What had the one-track robot actually learned?", options = new[] { "That one track: it memorized it, and a right bend was new to it", "Nothing at all", "How to drive anywhere" }, answer = 0 } },
            takeaway = "A robot that learned the track memorized. A robot that learned to drive passes a track it has never seen.",
            ask = "What has our system only ever been tested on?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.settings != null && !x.settings.training.Contains(x.settings.test)), "The test track was never trained on"),
                (Of(x.RaceOk(x.LastRace)), "The robot finished the test track"),
                (Of(x.Picked("memo", 0)), "Explained what the one-track robot had learned"),
            },
        },
        new Lesson
        {
            number = 11, tier = 3, id = "3.2", title = "Which number tells the truth?",
            kicker = "Pick the measure that matches what a mistake costs.",
            luma = "Two ready robots, every measure side by side. On the real track a wall touch means a repair. Which number do you judge by?",
            learn = "Finishes, crashes, wall touches, lap time: each measure tells a different story about the same robot. The one to judge by is the one that matches what a mistake costs.\n\n" +
                    "On this track a wall touch costs a repair. One robot finishes more and scrapes the wall; the other finishes less and never touches it.",
            training = new[] { "hairpin" }, test = "hairpin", measure = true, laps = 10,
            recall = new Recall { question = "Why is the test track never trained on?", options = new[] { "To measure how it does on a track it has never seen", "To save time", "Because it is harder" }, answer = 0, from = "3.1" },
            broken = new Setup
            {
                name = "Judged by finishes alone", aName = "The scraper", bName = "The careful one",
                a = On(new[] { "hairpin" }, "hairpin", s => { s.speedBonus = 1; s.attempts = 300; }),
                b = On(new[] { "hairpin" }, "hairpin", s => { s.topSpeed = 1; s.constraintSet = true; s.attempts = 300; }),
                why = "The scraper is paid a little for speed: it finishes often and scrapes the wall on the way. The careful one has a top speed of 1: slower, sometimes out of time, never a scrape. Judged by finishes, the scraper wins. Judged by what a wall touch costs, it does not.",
            },
            live = "Every measure for every robot, side by side on the debrief. Decide what a mistake costs before you decide who won.",
            quiz = new[]
            {
                new Question { key = "measure", prompt = "A wall touch costs a repair. Which measure do you judge by?", options = Measures.Names, answer = 2 },
                new Question { key = "lied", prompt = "In one line: why did finish rate lie?" },
            },
            takeaway = "Pick the measure that matches what a mistake costs.",
            ask = "Which number do we report, and what does it hide?",
            check = x => new List<(Practised, string)>
            {
                (Of(x.Picked("measure", 2) || (x.settings != null && x.settings.measure == 2)), "Picked the measure that fits the stated cost"),
                (Of(x.Wrote("lied")), "Explained in one line why finish rate lied"),
            },
        },
        new Lesson
        {
            number = 12, tier = 3, id = "3.3", title = "Which knob, and how do you know?",
            kicker = "Change one thing, keep the rest the same, compare.",
            luma = "The ready experiment changed three knobs between two runs and got better. Which knob did it? Nobody can tell. Change one at a time, on the same seed.",
            learn = "The knobs are set before training: how fast it rewrites its rule book, how long it keeps trying new things, how many attempts it gets. The same luck twice, the same seed, " +
                    "is the only way to compare two runs fairly.\n\nChange one knob, keep the rest, compare. A rewrite speed too high jumps and never settles; too low settles far past the budget.",
            training = new[] { "twobends" }, test = "twobends", knobs = true, attempts = true, explore = true,
            recall = new Recall { question = "Which measure did finish rate hide?", options = new[] { "Wall touches per lap", "The attempt count", "The seed" }, answer = 0, from = "3.2" },
            broken = new Setup
            {
                name = "Three knobs at once", aName = "Run A", bName = "Run B",
                a = On(new[] { "twobends" }, "twobends", s => { s.rewrite = 0.1f; s.explore = 0.9f; s.exploreFall = 0.995f; s.attempts = 120; s.seed = 7; }),
                b = On(new[] { "twobends" }, "twobends", s => { s.rewrite = 0.4f; s.explore = 0.7f; s.exploreFall = 0.98f; s.attempts = 300; s.seed = 7; }),
                why = "Run B is better than run A, and three knobs changed between them: the rewrite speed, how fast new moves fall, and the attempts. Which one did it? Nobody can tell. Rerun from A with one knob changed at a time, on the same seed.",
            },
            live = "Every run is listed with its knobs and its seed. A difference you can trace to one knob is a difference you learned something from.",
            quiz = new[]
            {
                new Question { key = "knob", prompt = "Which knob helped the most in your runs?", options = new[] { "The rewrite speed", "How new moves fall", "The attempts" } },
                new Question { key = "high", prompt = "One curve jumps up and down and never settles. The other settles, far past the attempt budget. Which one is the rewrite speed set too high?", options = new[] { "The one that jumps and never settles", "The one that settles late" }, answer = 0 },
            },
            takeaway = "Change one thing, keep the rest the same, compare.",
            ask = "When we changed three things and it got better, what did we learn?",
            check = x =>
            {
                var own = x.runs.Where(r => r.label == "").ToList(); // the student's own runs, after the ready pair
                bool oneAtATime = own.Count >= 2 && own.Zip(own.Skip(1), (p, q) => p.s.Differences(q.s).Count(d => d != "seed") <= 1).All(b => b);
                bool sameSeed = own.Count >= 2 && own.All(r => r.s.seed == own[0].s.seed);
                int helped = Helped(own);
                return new List<(Practised, string)>
                {
                    (Of(oneAtATime), "Changed one knob per run"),
                    (Of(sameSeed), "Kept the seed"),
                    (Of(helped >= 0 && x.Picked("knob", helped)), "Named the knob that helped"),
                    (Of(x.Picked("high", 0)), "Recognized the rewrite speed set too high"),
                };
            },
        },
        new Lesson
        {
            number = 13, tier = 3, id = "3.4", title = "Capstone: From the simulator to the real track",
            kicker = "Train where crashing is cheap. Test on what it has never seen. Expect the real track to be different.",
            luma = "Train within the budget, pass the test track, then one real run: a track the simulator never had, with a difference in it. Write what could break first.",
            learn = "Crashing a real car a thousand times is not an option, so the learning happens in the simulator. Then one real run, where the track is a little different " +
                    "from anything the simulator had.\n\nTrain within 300 attempts on the given seed, pass the test bend, write one line on what could break, and take the real run: one lap, no learning.",
            training = new[] { "bend", "twobends", "leftonly" }, test = "rightbend", sensors = true, moves = true, points = true, explore = true, knobs = true, attempts = true, measure = true, realRun = true, prediction = true,
            predictionPrompt = "Before the real run: what could break on a track the simulator never had?",
            recall = new Recall { question = "How do you tell which of three changed knobs helped?", options = new[] { "Rerun with one changed at a time, on the same seed", "Look at the best curve", "Ask the robot" }, answer = 0, from = "3.3" },
            live = "The simulator is where crashing is cheap. The real track has oil the simulator never had, and the real run learns nothing.",
            quiz = new[]
            {
                new Question { key = "broke", prompt = "The real run: what was different, and did it break the robot? One line." },
                new Question { key = "cost", prompt = "On the real track a crash is what costs. Which measure matches?", options = Measures.Names, answer = 1 },
            },
            takeaway = "Train where crashing is cheap. Test on what it has never seen. Expect the real track to be different.",
            ask = "Where did our system learn, and how different is the place it will be used?",
            check = x =>
            {
                var own = x.runs.ToList();
                bool oneAtATime = own.Count < 2 || own.Zip(own.Skip(1), (p, q) => p.s.Differences(q.s).Count <= 1).All(b => b);
                return new List<(Practised, string)>
                {
                    (Of(x.Last != null && x.Last.s.attempts <= 300), "Trained within the budget"),
                    (Of(x.RaceOk(x.LastRace)), "The test track passed"),
                    (Of(oneAtATime), "The knobs were set one at a time"),
                    (Of((x.real != null && x.real.Finishes > 0) || x.Wrote("broke")), "The real run finished, or the difference that broke it was named"),
                    (Of(x.Picked("cost", 1) || (x.settings != null && x.settings.measure == 1)), "The measure chosen matched the stated cost"),
                };
            },
        },
    };

    // 3.3: among the student's runs, the one knob whose change settled the curve soonest. -1 when no run changed one knob.
    static int Helped(List<Training> own)
    {
        int best = -1; float bestGain = 0;
        for (int i = 1; i < own.Count; i++)
        {
            var d = own[i - 1].s.Differences(own[i].s).Where(x => x != "seed").ToList();
            if (d.Count != 1) continue;
            float before = own[i - 1].settled > 0 ? own[i - 1].settled : own[i - 1].s.attempts * 2, after = own[i].settled > 0 ? own[i].settled : own[i].s.attempts * 2;
            float gain = before - after;
            if (gain > bestGain) { bestGain = gain; best = d[0] == "rewrite speed" ? 0 : d[0] == "new moves" ? 1 : d[0] == "attempts" ? 2 : -1; }
        }
        return best;
    }

    public static int Count => All.Length;
    public static Lesson Get(int number) => number >= 1 && number <= All.Length ? All[number - 1] : null;
    public static readonly string[] TierNames = { "How a machine learns from reward", "Design how it learns", "Does it really work?" };
}
