using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

// Plain sentences on what happened to a robot and why, in the words of the
// lessons: the list, what it sees, memory, brain, budget. Read on the debrief,
// and by LUMA while the robots shop.
public static class Verdicts
{
    static readonly string[] Warnings = { "avoid", "poison", "never", "not ", "don't", "do not", "dangerous", "unsafe", "kill", "eliminat", "stay away", "except", "no ", "leave", "skip", "ruin", "allerg" };
    static readonly Regex Fallback = new Regex(@"\b(if|when|otherwise|else|nothing|none|explore|wander|no [a-z]+ in view)\b", RegexOptions.IgnoreCase);

    // Whether the list names an item ("cherry", "cherries", "grapes").
    public static bool Named(string instructions, string itemId)
    {
        string s = (instructions ?? "").ToLowerInvariant();
        return s.Contains(itemId) || s.Contains(itemId.TrimEnd('s')) || s.Contains(Items.Plural(itemId));
    }
    public static int ItemsNamed(string instructions) => Items.All.Count(k => Named(instructions, k.id));
    public static bool HasWarning(string instructions) { string s = (instructions ?? "").ToLowerInvariant(); return Warnings.Any(s.Contains); }
    public static bool HasFallback(string instructions) => Fallback.IsMatch(instructions ?? "");

    // Two plain sentences on what happened and why: this is where the lesson lands.
    public static string Of(AgentState a, Arena arena, Simulation sim)
    {
        var sb = new StringBuilder();
        if (!a.alive)
        {
            sb.Append($"Sent home on turn {a.eliminatedTurn} with {Items.Named(a.ateItem)} in the basket, which ruins dinner. ").Append(WhyEliminated(a));
            if (a.lastThought != "") sb.Append($" Its last text: \"{Trim(a.lastThought, 110)}\"");
        }
        else if (a.outOfTokens)
        {
            sb.Append($"Ran out of tokens with {a.score} wanted item{(a.score == 1 ? "" : "s")} in the basket. ").Append(CostLine(a, sim.turn));
        }
        else
        {
            sb.Append($"Came home with {a.score} wanted item{(a.score == 1 ? "" : "s")}");
            sb.Append(a.eaten.Count > 0 ? $": {string.Join(", ", a.eaten.GroupBy(e => e).Select(g => g.Count() > 1 ? $"{g.Count()} {Items.Plural(g.Key)}" : Items.Named(g.Key)))}. " : ". ");
            if (a.eaten.Count == 0) sb.Append("It never bought anything: safe, but did it ever see something on its list? Check how far it sees and what the list says. ");
            if (a.confusedTurns > 0) sb.Append($"Lost {a.confusedTurns} turn{(a.confusedTurns == 1 ? "" : "s")} to texts that were not a move. ");
            if (a.blockedTurns > 2) sb.Append($"Bumped into the market's edge or other agents {a.blockedTurns} times. ");
        }
        return sb.ToString().Trim();
    }

    // Why the wrong thing went into the basket, traced to the list.
    public static string WhyEliminated(AgentState a)
    {
        string instr = (a.config.instructions ?? "").ToLowerInvariant();
        var kind = Items.Get(a.ateItem);
        if (string.IsNullOrWhiteSpace(instr)) return "It had no list at all, so everything looked like dinner.";
        if (Named(instr, a.ateItem)) return $"The list did mention \"{a.ateItem}\": look at how. Was it clearly on the leave-it side, or just in the same sentence?";
        if (kind != null && instr.Contains(kind.family == ItemFamily.Fruit ? "fruit" : "vegetable"))
            return $"The list spoke of {(kind.family == ItemFamily.Fruit ? "fruit" : "vegetables")} in general, so the agent had to decide for itself what counts. Naming the items removes the guesswork.";
        return $"Nothing on the list mentioned {Items.Named(a.ateItem)}. The agent met a case its list did not cover, guessed, and guessed wrong.";
    }

    // Which part of the agent let it down, for the capstone's debrief: the job
    // description, what it saw, what it remembered, its brain or its budget.
    // Null when nothing did.
    public static string Part(AgentState a)
    {
        string instr = (a.config.instructions ?? "").ToLowerInvariant();
        if (!a.alive)
            return Named(instr, a.ateItem) && HasWarning(instr)
                ? "Its brain: the list did say to leave it, and the agent bought it anyway. A bigger brain or more effort reads a list more carefully."
                : "The job description: the list did not cover what it met, so it guessed.";
        if (a.outOfTokens) return "Its budget: the pocket money ran out before the errand ended. Every dial above spends some, every turn.";
        if (a.score == 0)
            return a.blockedTurns > 3 || a.config.memory == MemoryModes.None
                ? "What it remembered: it kept walking the same ground. A memory or a notebook would have told it where it had been."
                : "What it saw: nothing on the list ever came into view. It sees too little, or the list did not say where to walk.";
        return null;
    }

    // What one turn cost, dial by dial.
    public static string CostLine(AgentState a, int turns)
    {
        long perTurn = a.TokensUsed / Mathf.Max(1, turns);
        return $"Each turn cost about {perTurn:N0} tokens: {Observation.EstimateTokens(a.config.instructions):N0} for the list, sight {a.config.vision}, " +
               $"memory \"{MemoryModes.Labels[a.config.memory].ToLowerInvariant()}\". A shorter list or less memory would have lasted the errand.";
    }

    static string Trim(string s, int n) => s.Length <= n ? s : s.Substring(0, n - 1) + "…";
}
