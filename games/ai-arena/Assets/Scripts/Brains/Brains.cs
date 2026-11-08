using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

// A brain turns an observation into a Decision. The real one asks Claude; the
// stand-in lets the game run on a machine with no Claude access at all.
public interface IBrain
{
    IEnumerator Decide(AgentState a, string system, string userTurn, Action<Decision> done);
}

public static class Brains
{
    public static IBrain ForCurrentAccess() =>
        ClaudeAccess.Mode == ClaudeMode.Offline ? new StandInBrain() : (IBrain)new ClaudeBrain();
}

public class ClaudeBrain : IBrain
{
    public IEnumerator Decide(AgentState a, string system, string userTurn, Action<Decision> done)
    {
        string model = Models.All[a.config.model].id;
        string effort = Efforts.Ids[a.config.effort];
        ClaudeReply reply = null;

        if (ClaudeAccess.Mode == ClaudeMode.Proxy)
        {
            yield return ClaudeProxy.Send(model, effort, system, a.history, userTurn, r => reply = r);
        }
        else
        {
            var task = ClaudeCode.Ask(model, effort, system, a.history, userTurn);
            yield return new WaitUntil(() => task.IsCompleted);
            reply = task.IsFaulted ? new ClaudeReply { error = task.Exception?.GetBaseException().Message } : task.Result;
        }

        var d = reply.ok ? Decision.Parse(reply.text) : new Decision();
        d.error = reply.error;
        d.inputTokens = reply.inputTokens;
        d.outputTokens = reply.outputTokens;
        done(d);
    }
}

// A simple automation: a fixed rule, no AI. It takes exactly the items its rule
// names, walks to the nearest one it can see, and otherwise wanders without
// stepping onto any other stall. It costs no tokens and cannot decide anything.
// Errand 1.6 runs it against an agent on the same market.
public class RuleBrain : IBrain
{
    public IEnumerator Decide(AgentState a, string system, string userTurn, Action<Decision> done)
    {
        yield return new WaitForSeconds(0.1f);
        var takes = new HashSet<string>(Items.All.Where(k => Verdicts.Named(a.config.instructions, k.id)).Select(k => k.id));
        var seen = new List<(ItemKind kind, Vector2Int at)>();
        foreach (Match m in Regex.Matches(userTurn, @"- (\w+) at \((\d+), (\d+)\)"))
        {
            var kind = Items.Get(m.Groups[1].Value);
            if (kind != null) seen.Add((kind, new Vector2Int(int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value))));
        }
        var d = new Decision { usable = true };
        var targets = seen.Where(s => takes.Contains(s.kind.id)).OrderBy(s => Mathf.Abs(s.at.x - a.pos.x) + Mathf.Abs(s.at.y - a.pos.y)).ToList();
        var rng = new System.Random(a.index * 7919 + a.score * 31 + userTurn.Length);
        bool Other(string dir)
        {
            var next = a.pos + Arena.Step(dir);
            var there = seen.FirstOrDefault(s => s.at == next).kind;
            return there != null && !takes.Contains(there.id);
        }
        if (targets.Count > 0)
        {
            var t = targets[0];
            int dx = t.at.x - a.pos.x, dy = t.at.y - a.pos.y;
            bool horizontal = Mathf.Abs(dx) > Mathf.Abs(dy) || (Mathf.Abs(dx) == Mathf.Abs(dy) && rng.Next(2) == 0);
            string first = horizontal ? (dx > 0 ? "east" : "west") : (dy > 0 ? "south" : "north");
            string second = !horizontal ? (dx > 0 ? "east" : dx < 0 ? "west" : null) : (dy > 0 ? "south" : dy < 0 ? "north" : null);
            d.move = !Other(first) ? first : second != null && !Other(second) ? second : "stay";
            d.think = $"Nearest {t.kind.id} at ({t.at.x}, {t.at.y}). Going there.";
        }
        else
        {
            var options = new List<string>();
            foreach (var dir in new[] { "north", "south", "east", "west" })
            {
                var next = a.pos + Arena.Step(dir);
                if (!Arena.InBounds(next)) continue;
                var there = seen.FirstOrDefault(s => s.at == next).kind;
                if (there == null || takes.Contains(there.id)) options.Add(dir);
            }
            d.move = options.Count == 0 ? "stay" : options[rng.Next(options.Count)];
            d.think = "Nothing the rule names in view. Walking.";
        }
        done(d);
    }
}

// Reads the instructions the way a very literal machine would: an item named in
// the same sentence as a warning word is avoided, any other item named is wanted,
// and if no item is named at all, everything looks like food. Then it walks
// towards the nearest wanted thing it can see. It exists so the game can be tried
// without Claude; its reasoning is nothing like Claude's.
public class StandInBrain : IBrain
{
    static readonly string[] Warnings = { "avoid", "poison", "never", "not ", "don't", "do not", "dangerous", "unsafe", "kill", "eliminat", "except", "allerg" };

    public IEnumerator Decide(AgentState a, string system, string userTurn, Action<Decision> done)
    {
        yield return new WaitForSeconds(0.15f + 0.1f * a.config.effort);
        var d = new Decision { usable = true };
        d.inputTokens = Observation.EstimateTokens(system + userTurn) + a.history.Sum(h => Observation.EstimateTokens(h.user + h.assistant));
        d.outputTokens = 40 + 60 * a.config.effort;

        var (wanted, avoided) = ReadInstructions(a.config.instructions);
        var seen = new List<(ItemKind kind, Vector2Int at)>();
        foreach (Match m in Regex.Matches(userTurn, @"- (\w+) at \((\d+), (\d+)\)"))
        {
            var kind = Items.Get(m.Groups[1].Value);
            if (kind != null) seen.Add((kind, new Vector2Int(int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value))));
        }
        var targets = seen.Where(s => !avoided.Contains(s.kind.id) && (wanted.Count == 0 || wanted.Contains(s.kind.id)))
                          .OrderBy(s => Mathf.Abs(s.at.x - a.pos.x) + Mathf.Abs(s.at.y - a.pos.y)).ToList();
        var rng = new System.Random(a.index * 7919 + a.score * 31 + userTurn.Length);
        bool Forbidden(string dir)
        {
            var next = a.pos + Arena.Step(dir);
            var there = seen.FirstOrDefault(s => s.at == next).kind;
            return there != null && (avoided.Contains(there.id) || (wanted.Count > 0 && !wanted.Contains(there.id)));
        }
        if (targets.Count > 0)
        {
            var t = targets[0];
            int dx = t.at.x - a.pos.x, dy = t.at.y - a.pos.y;
            bool horizontal = Mathf.Abs(dx) > Mathf.Abs(dy) || (Mathf.Abs(dx) == Mathf.Abs(dy) && rng.Next(2) == 0);
            string first = horizontal ? (dx > 0 ? "east" : "west") : (dy > 0 ? "south" : "north");
            string second = !horizontal ? (dx > 0 ? "east" : dx < 0 ? "west" : null) : (dy > 0 ? "south" : dy < 0 ? "north" : null);
            d.move = !Forbidden(first) ? first : second != null && !Forbidden(second) ? second : "stay";
            d.think = $"I see {Items.Named(t.kind.id)} at ({t.at.x}, {t.at.y}). Going for it.";
        }
        else
        {
            var options = new List<string>();
            foreach (var dir in new[] { "north", "south", "east", "west" })
            {
                var next = a.pos + Arena.Step(dir);
                if (!Arena.InBounds(next)) continue;
                var there = seen.FirstOrDefault(s => s.at == next).kind;
                bool forbidden = there != null && (avoided.Contains(there.id) || (wanted.Count > 0 && !wanted.Contains(there.id)));
                if (!forbidden) options.Add(dir);
            }
            d.move = options.Count == 0 ? "stay" : options[rng.Next(options.Count)];
            d.think = seen.Count > 0 ? "Nothing on my list in view. Walking on." : "No stalls in view. Walking on.";
        }
        done(d);
    }

    static (HashSet<string> wanted, HashSet<string> avoided) ReadInstructions(string text)
    {
        var wanted = new HashSet<string>();
        var avoided = new HashSet<string>();
        if (string.IsNullOrWhiteSpace(text)) return (wanted, avoided);
        foreach (var sentence in Regex.Split(text.ToLowerInvariant(), @"[.\n;:!]"))
        {
            bool warning = Warnings.Any(w => sentence.Contains(w));
            foreach (var k in Items.All)
            {
                if (!sentence.Contains(k.id) && !sentence.Contains(k.id.TrimEnd('s'))) continue;
                if (warning) avoided.Add(k.id); else wanted.Add(k.id);
            }
            if (sentence.Contains("fruit")) foreach (var k in Items.Fruit) (warning ? avoided : wanted).Add(k.id);
            if (sentence.Contains("vegetable")) foreach (var k in Items.Vegetables) (warning ? avoided : wanted).Add(k.id);
            foreach (var colour in new[] { "red", "yellow", "green", "orange", "purple", "brown" })
                if (Regex.IsMatch(sentence, $@"\b{colour}\b"))
                    foreach (var k in Items.All.Where(k => k.colour == colour)) (warning ? avoided : wanted).Add(k.id);
        }
        wanted.ExceptWith(avoided);
        return (wanted, avoided);
    }
}
