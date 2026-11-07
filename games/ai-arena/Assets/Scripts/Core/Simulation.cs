using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Runs one round: every active agent decides at the same time, the moves resolve
// together, the board animates, repeat. Nobody steers once this starts.
public class Simulation : MonoBehaviour
{
    public Arena arena;
    public int totalTurns = 30;
    public int turn;
    public float speed = 2f;
    public bool paused;
    public Func<bool> Holding;     // the screen is still showing the last turn (a real walk takes real time)
    public bool finished;
    public string phase = "";

    public class Move { public AgentState agent; public Vector2Int from, to; public ItemKind ate; }
    public readonly List<Move> lastMoves = new List<Move>();

    public event Action OnTurnResolved; // the board animates the moves
    public event Action OnChanged;      // the side panel refreshes
    public event Action OnFinished;

    IBrain brain;
    readonly IBrain rule = new RuleBrain(); // for a robot that is a simple automation, not an agent

    public void Begin(Arena arena, int turns)
    {
        this.arena = arena;
        totalTurns = turns;
        brain = Brains.ForCurrentAccess();
        StartCoroutine(Run());
    }

    IBrain BrainOf(AgentState a) => a.config.IsRule ? rule : brain;

    public void Abort()
    {
        StopAllCoroutines();
        finished = true;
        phase = "stopped";
        OnFinished?.Invoke();
    }

    IEnumerator Run()
    {
        var systems = arena.agents.ToDictionary(a => a.index, a => Observation.SystemPrompt(a));
        for (turn = 1; turn <= totalTurns; turn++)
        {
            while (paused) yield return null;
            if (!arena.agents.Any(a => a.Active)) break;

            phase = "thinking";
            OnChanged?.Invoke();

            var decisions = new Dictionary<int, Decision>();
            var observations = new Dictionary<int, string>();
            int pending = 0;
            foreach (var a in arena.agents.Where(a => a.Active))
            {
                var agent = a;
                string user = Observation.Turn(arena, agent, turn, totalTurns);
                observations[agent.index] = user;
                agent.lastObservation = user;
                pending++;
                StartCoroutine(Guarded(BrainOf(agent).Decide(agent, systems[agent.index], user, d => { decisions[agent.index] = d; pending--; }),
                                       ex => { decisions[agent.index] = new Decision { error = ex.Message }; pending--; }));
            }
            float started = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => pending <= 0 || Time.realtimeSinceStartup - started > 150f);

            Resolve(decisions, observations);
            phase = "moving";
            OnTurnResolved?.Invoke();
            OnChanged?.Invoke();
            yield return new WaitForSeconds(0.9f / Mathf.Max(0.25f, speed));
            float held = 0f;
            while (Holding != null && Holding() && held < 30f) { held += Time.deltaTime; yield return null; }

            if (turn % 2 == 0 && arena.ItemCount < 30) arena.SpawnItems(2);
        }
        finished = true;
        phase = "done";
        OnFinished?.Invoke();
    }

    // A brain that throws must not leave the turn waiting on a reply that never comes.
    static IEnumerator Guarded(IEnumerator inner, Action<Exception> onFault)
    {
        while (true)
        {
            object current;
            try
            {
                if (!inner.MoveNext()) yield break;
                current = inner.Current;
            }
            catch (Exception ex) { Debug.LogException(ex); onFault(ex); yield break; }
            yield return current;
        }
    }

    void Resolve(Dictionary<int, Decision> decisions, Dictionary<int, string> observations)
    {
        lastMoves.Clear();
        var active = arena.agents.Where(a => a.Active).ToList();
        var targets = new Dictionary<int, Vector2Int>();

        foreach (var a in active)
        {
            if (!decisions.TryGetValue(a.index, out var d) || d == null) d = new Decision { error = "No answer in time" };
            a.inputTokens += d.inputTokens;
            a.outputTokens += d.outputTokens;
            a.lastMove = d.move;
            a.lastError = d.error;
            if (d.usable)
            {
                a.lastThought = string.IsNullOrWhiteSpace(d.think) ? "(no reason given)" : d.think.Trim();
                a.Remember(observations[a.index], d.raw);
                if (d.note != null) a.notebook = d.note;
            }
            else
            {
                a.confusedTurns++;
                a.lastThought = d.error ?? "Reply could not be read as a move";
                a.log.Add($"Turn {turn}: lost the turn ({a.lastThought})");
            }
            var step = Arena.Step(d.move);
            var to = a.pos + step;
            if (step != Vector2Int.zero && !Arena.InBounds(to)) { to = a.pos; a.blockedTurns++; a.log.Add($"Turn {turn}: walked into the market's edge"); }
            if (step.y < 0) a.facing = 0; else if (step.x > 0) a.facing = 1; else if (step.y > 0) a.facing = 2; else if (step.x < 0) a.facing = 3;
            targets[a.index] = to;
        }

        // Two robots cannot share a cell. Among several heading for one cell, one
        // picked at random gets it; and nobody can enter a cell whose occupant stays put.
        var stayers = arena.agents.Where(a => a.alive && !a.Active).Select(a => a.pos).ToHashSet();
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var group in targets.GroupBy(t => t.Value).Where(g => g.Count() > 1).ToList())
            {
                var ids = group.Select(g => g.Key).ToList();
                int keep = ids.Any(i => arena.agents[i].pos == group.Key) ? ids.First(i => arena.agents[i].pos == group.Key) : ids[arena.rng.Next(ids.Count)];
                foreach (int i in ids.Where(i => i != keep)) { targets[i] = arena.agents[i].pos; arena.agents[i].blockedTurns++; changed = true; }
            }
            foreach (var kv in targets.ToList())
            {
                var a = arena.agents[kv.Key];
                if (kv.Value == a.pos) continue;
                bool occupied = stayers.Contains(kv.Value) || active.Any(o => o.index != a.index && o.pos == kv.Value && targets[o.index] == o.pos);
                if (occupied) { targets[a.index] = a.pos; a.blockedTurns++; changed = true; }
            }
        }

        foreach (var a in active)
        {
            var m = new Move { agent = a, from = a.pos, to = targets[a.index] };
            if (m.to != a.pos)
            {
                a.pos = m.to;
                var item = arena.ItemAt(m.to);
                if (item != null)
                {
                    arena.RemoveItem(m.to);
                    m.ate = item;
                    a.eaten.Add(item.id);
                    if (arena.brief.IsSafe(item)) { a.score++; a.log.Add($"Turn {turn}: bought {Items.Named(item.id)}, on the list"); }
                    else { a.alive = false; a.eliminatedTurn = turn; a.ateItem = item.id; a.log.Add($"Turn {turn}: bought {Items.Named(item.id)}. Ruins dinner. Sent home."); }
                }
            }
            lastMoves.Add(m);
            if (a.alive && a.TokensLeft == 0) { a.outOfTokens = true; a.log.Add($"Turn {turn}: out of tokens"); }
        }
    }
}
