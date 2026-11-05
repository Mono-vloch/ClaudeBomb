using System;
using System.Text;
using UnityEngine;

// The five moves the robot may make, the five sensors it may have on, the car
// itself, and one step of driving: the move applied, the car advanced by its
// speed, a crash or a finish noticed. Nobody drives: a Learner picks the move.
public static class Moves
{
    public const int SpeedUp = 0, Hold = 1, SlowDown = 2, Left = 3, Right = 4, Count = 5;
    public static readonly string[] Names = { "Speed up", "Hold", "Slow down", "Turn left", "Turn right" };
    public static readonly string[] Short = { "faster", "hold", "slower", "left", "right" };
}

public static class Sensors
{
    public const int Front = 0, Left = 1, Right = 2, Speed = 3, Bend = 4, Count = 5;
    public static readonly string[] Names = { "Front", "Left", "Right", "Speed", "Bend ahead" };
    public static readonly string[] Blurbs =
    {
        "How many clear cells ahead, up to 3.", "How many clear cells to the left, up to 3.", "How many clear cells to the right, up to 3.",
        "Its own speed: 0, 1 or 2.", "Whether the road turns within two cells.",
    };
    public const int Far = 3;

    // What one sensor reads, as the robot would say it.
    public static string Describe(int sensor, int value) => sensor switch
    {
        Speed => value == 0 ? "stopped" : value == 1 ? "slow" : "fast",
        Bend => value == 1 ? "bend ahead" : "straight ahead",
        _ => value >= Far ? "clear" : value == 0 ? "wall" : value + " cell" + (value == 1 ? "" : "s"),
    };
}

public class Car
{
    public Vector2Int pos;
    public int heading, speed;
    public bool crashed, finished;
    public int steps, touches;
    public Vector2Int from; // where this step started, for the picture

    public void Reset(Track t)
    {
        pos = from = t.start; heading = t.startHeading; speed = 0;
        crashed = finished = false; steps = touches = 0;
    }
}

public struct Outcome
{
    public bool crashed, finished, touched, slid;
    public int progress; // road cells gained toward the finish, negative when lost
}

public static class Driving
{
    // Every sensor's reading, whether it is on or not.
    public static int[] Sense(Track t, Car c)
    {
        var r = new int[Sensors.Count];
        r[Sensors.Front] = Clear(t, c.pos, c.heading);
        r[Sensors.Left] = Clear(t, c.pos, (c.heading + 3) % 4);
        r[Sensors.Right] = Clear(t, c.pos, (c.heading + 1) % 4);
        r[Sensors.Speed] = c.speed;
        // A bend: a wall within two cells ahead, and a way out to a side there.
        int ahead = r[Sensors.Front];
        r[Sensors.Bend] = 0;
        if (ahead <= 2)
        {
            var at = c.pos + Track.Dir(c.heading) * ahead;
            if (Clear(t, at, (c.heading + 3) % 4) > 0 || Clear(t, at, (c.heading + 1) % 4) > 0) r[Sensors.Bend] = 1;
        }
        return r;
    }

    static int Clear(Track t, Vector2Int from, int heading)
    {
        int n = 0;
        var p = from;
        while (n < Sensors.Far) { p += Track.Dir(heading); if (!t.Road(p)) break; n++; }
        return n;
    }

    // The situation, as the robot knows it: only the sensors that are on.
    public static string Key(int[] readings, bool[] on)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Sensors.Count; i++) if (on[i]) sb.Append(i).Append('=').Append(readings[i]).Append(' ');
        return sb.Length == 0 ? "(no sensors)" : sb.ToString().TrimEnd();
    }

    // The same, in words, for the rule book.
    public static string Words(int[] readings, bool[] on)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Sensors.Count; i++)
        {
            if (!on[i]) continue;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(Sensors.Names[i].ToLowerInvariant()).Append(' ').Append(Sensors.Describe(i, readings[i]));
        }
        return sb.Length == 0 ? "no sensors on: every moment looks the same" : sb.ToString();
    }

    public static int[] Parse(string key)
    {
        var r = new int[Sensors.Count];
        for (int i = 0; i < r.Length; i++) r[i] = -1;
        foreach (var part in key.Split(' '))
        {
            var kv = part.Split('=');
            if (kv.Length == 2 && int.TryParse(kv[0], out int s) && int.TryParse(kv[1], out int v) && s >= 0 && s < r.Length) r[s] = v;
        }
        return r;
    }

    // One step: the move, then the car travels its speed in cells, one at a time.
    public static Outcome Step(Track t, Car c, int move, int topSpeed)
    {
        var o = new Outcome();
        int before = t.Distance(c.pos);
        switch (move)
        {
            case Moves.SpeedUp: c.speed = Math.Min(c.speed + 1, topSpeed); break;
            case Moves.SlowDown: c.speed = Math.Max(c.speed - 1, 0); break;
            case Moves.Left: c.heading = (c.heading + 3) % 4; break;
            case Moves.Right: c.heading = (c.heading + 1) % 4; break;
        }
        c.from = c.pos;
        int travel = c.speed;
        if (t.Oil(c.pos) && c.speed > 0) { travel++; o.slid = true; } // oil: it slides one cell further than it meant to
        for (int i = 0; i < travel; i++)
        {
            var next = c.pos + Track.Dir(c.heading);
            if (!t.Road(next)) { c.crashed = true; o.crashed = true; break; }
            c.pos = next;
            if (c.speed >= 2 && (Clear(t, c.pos, (c.heading + 3) % 4) == 0 || Clear(t, c.pos, (c.heading + 1) % 4) == 0)) { c.touches++; o.touched = true; }
            if (t.Finish(c.pos)) { c.finished = true; o.finished = true; break; }
        }
        c.steps++;
        int after = t.Distance(c.pos);
        if (before >= 0 && after >= 0) o.progress = before - after;
        return o;
    }
}
