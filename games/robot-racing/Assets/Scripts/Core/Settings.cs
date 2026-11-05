using System;
using System.Collections.Generic;
using System.Linq;

// Everything a student sets before training: what the robot senses, what it may
// do, what earns and loses points, how much it tries new things, the knobs set
// before training, the tracks, and the measure to judge it by. A level opens
// these one at a time; the rest stay fixed at a sensible value.
[Serializable]
public class Settings
{
    public bool[] sensors = { true, true, true, true, false };   // front, left, right, speed, bend ahead
    public bool[] moves = { true, true, true, true, true };      // speed up, hold, slow down, left, right
    public int topSpeed = 2;                                     // the constraint
    public bool constraintSet;                                   // whether the student (or a setup) set it on purpose

    // The points table
    public float finish = 100, crash = -100, toward = 2, away = -2, moment = -0.2f, speedBonus = 0;

    // The chance of trying something new: where it starts, the floor, how it falls per attempt
    public float explore = 0.9f, exploreMin = 0.05f, exploreFall = 0.985f;
    public bool exploreOffAfterFinish;   // the "never again" mistake

    // The knobs set before training
    public float rewrite = 0.3f;         // how fast it rewrites its rule book (the learning rate)
    public int attempts = 150;
    public int seed = 7;
    public bool stopAtFirstFinish;       // the 1.2 mistake

    public string[] training = { "bend" };
    public string test = "bend";
    public int measure = 0;              // Measures

    public Settings Clone()
    {
        var c = (Settings)MemberwiseClone();
        c.sensors = (bool[])sensors.Clone(); c.moves = (bool[])moves.Clone(); c.training = (string[])training.Clone();
        return c;
    }

    public int SensorsOn => sensors.Count(b => b);
    public int MovesOn => moves.Count(b => b);
    public bool Allowed(int move) => moves[move];
    public float TopPointsPerMove => Math.Max(Math.Max(toward, speedBonus * topSpeed), 0);

    // The knobs, as a line, to tell two runs apart.
    public string Knobs => $"rewrite {rewrite:0.00}, new moves {explore:0.0} falling {Fall(exploreFall)}, {attempts} attempts, seed {seed}";
    public static string Fall(float f) => f >= 1 ? "never" : f >= 0.995f ? "slowly" : f >= 0.98f ? "fast" : "at once";

    // What differs between two settings, in words; empty when nothing does.
    public List<string> Differences(Settings o)
    {
        var d = new List<string>();
        if (!sensors.SequenceEqual(o.sensors)) d.Add("sensors");
        if (!moves.SequenceEqual(o.moves) || topSpeed != o.topSpeed) d.Add("moves");
        if (finish != o.finish || crash != o.crash || toward != o.toward || away != o.away || moment != o.moment || speedBonus != o.speedBonus) d.Add("points");
        if (explore != o.explore || exploreMin != o.exploreMin || exploreFall != o.exploreFall || exploreOffAfterFinish != o.exploreOffAfterFinish) d.Add("new moves");
        if (rewrite != o.rewrite) d.Add("rewrite speed");
        if (attempts != o.attempts) d.Add("attempts");
        if (seed != o.seed) d.Add("seed");
        if (!training.SequenceEqual(o.training) || test != o.test) d.Add("tracks");
        return d;
    }
}

public static class Measures
{
    public static readonly string[] Names = { "Finish rate", "Crashes per lap", "Wall touches per lap", "Lap time" };
    public static readonly string[] Blurbs =
    {
        "How many of the laps it finished.", "How often it crashed, per lap driven.", "How often it scraped a wall, per lap.", "Moves from the start to the finish, on the laps it finished.",
    };
}
