using System.Collections.Generic;
using UnityEngine;

// A training run, computed in one go and kept as a log, so the screen can play it back at
// any pace: every attempt at race speed, a thirty-second montage that slows for the moments,
// or straight to the end. The learning is the same Training as before; only the watching
// moved. One step is one move of the car; one attempt is its steps, its path and the rule
// book as it stood when the attempt ended.
public class StepRecord
{
    public Vector2Int from, to;
    public int heading, speed, move;
    public int[] readings;
    public float reward, explore;
    public bool crashed, finished;
    public string key;
}

public class AttemptRecord
{
    public int index;
    public readonly List<StepRecord> steps = new List<StepRecord>();
    public readonly List<Vector2Int> path = new List<Vector2Int>();
    public Attempt result;
    public RuleBook book;        // as it stood after this attempt
    public float explore;
}

public class Replay
{
    public readonly Training training;
    public readonly List<AttemptRecord> attempts = new List<AttemptRecord>();
    public int FirstFinish => training.firstFinish;
    public int Settled => training.settled;
    public int Count => attempts.Count;

    public Replay(Training t)
    {
        training = t;
        int guard = 0;
        while (!t.done && guard++ < 200000)
        {
            var a = new AttemptRecord { index = t.curve.Count + 1 };
            a.path.Add(t.car.pos);
            while (true)
            {
                var step = new StepRecord { from = t.car.pos };
                bool ended = t.Tick();
                step.to = t.car.pos; step.heading = t.car.heading; step.speed = t.car.speed;
                step.move = t.lastMove; step.reward = t.lastReward; step.explore = t.explore;
                step.crashed = t.car.crashed && !t.car.finished; step.finished = t.car.finished;
                step.readings = (int[])t.readings.Clone(); step.key = t.key;
                a.steps.Add(step);
                if (a.path.Count == 0 || a.path[a.path.Count - 1] != step.to) a.path.Add(step.to);
                if (ended || t.done) break;
                if (a.steps.Count > 400) break; // never: an attempt ends within 90 moves
            }
            a.result = t.Last;
            a.book = t.book.Copy();
            a.explore = t.explore;
            attempts.Add(a);
            if (t.done) break;
        }
    }

    // Is this attempt one the montage slows down for: the first, the first finish, the settle, the last.
    public bool IsMoment(int index) => index == 1 || index == FirstFinish || index == Settled || index == attempts.Count;
}
