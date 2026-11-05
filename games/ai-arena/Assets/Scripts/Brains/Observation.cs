using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Turns the board into words. This is the agent's entire world: what is here is
// all it can know, and every line costs tokens.
public static class Observation
{
    // The part that never changes during a round, so the API can cache it.
    public static string SystemPrompt(AgentState a)
    {
        var sb = new StringBuilder();
        sb.Append("You are an AI agent sent shopping for a family. The market is a ").Append(Arena.Size).Append(" by ").Append(Arena.Size)
          .Append(" grid of cells, some with a stall. Coordinates are (x, y): x runs 0 (west) to ").Append(Arena.Size - 1)
          .Append(" (east), y runs 0 (north) to ").Append(Arena.Size - 1).Append(" (south). Other agents shop here too.\n");
        sb.Append("Each turn you receive what you can see and must answer with one line of JSON and nothing else:\n");
        sb.Append("{\"think\": \"<one short text home>\", \"move\": \"north\" | \"south\" | \"east\" | \"west\" | \"stay\"}\n");
        if (a.config.memory == MemoryModes.Notebook)
            sb.Append("You may add \"note\": \"<text>\" to replace your notebook. The notebook is the only thing you remember between turns; it is shown to you every turn.\n");
        sb.Append("Moving onto a stall buys its item into your basket. Some items are wanted tonight and count; others ruin dinner and end your errand. ");
        sb.Append("You are not told which are which: the shopping list below is your only guide. ");
        sb.Append("Stalls may exist beyond what you can see. You cannot move off the market or onto another agent.\n\n");
        sb.Append("YOUR SHOPPING LIST, from the family:\n");
        sb.Append(string.IsNullOrWhiteSpace(a.config.instructions) ? "(none given)" : a.config.instructions.Trim());
        return sb.ToString();
    }

    public static string Turn(Arena arena, AgentState a, int turn, int totalTurns)
    {
        var sb = new StringBuilder();
        sb.Append($"Turn {turn} of {totalTurns}. You are at ({a.pos.x}, {a.pos.y}). In the basket: {a.score} wanted item{(a.score == 1 ? "" : "s")}. Tokens left: {a.TokensLeft:N0}.\n");
        int r = a.config.vision;
        var seen = new List<string>();
        for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                var p = a.pos + new Vector2Int(dx, dy);
                if (!Arena.InBounds(p)) continue;
                var item = arena.ItemAt(p);
                var other = arena.AgentAt(p);
                if (item != null) seen.Add($"- {item.id} at ({p.x}, {p.y}), {Relative(dx, dy)}");
                if (other != null) seen.Add($"- agent \"{other.Name}\" at ({p.x}, {p.y}), {Relative(dx, dy)}");
            }
        sb.Append($"You can see {r} cell{(r > 1 ? "s" : "")} in every direction.\n");
        if (seen.Count == 0) sb.Append("No stalls in view, only empty cells.\n");
        else { sb.Append("In view:\n"); foreach (var s in seen) sb.Append(s).Append('\n'); }
        sb.Append($"Edges: north {a.pos.y}, south {Arena.Size - 1 - a.pos.y}, east {Arena.Size - 1 - a.pos.x}, west {a.pos.x} cells away.\n");
        if (a.config.memory == MemoryModes.Notebook)
            sb.Append("Your notebook: ").Append(string.IsNullOrWhiteSpace(a.notebook) ? "(empty)" : a.notebook.Trim()).Append('\n');
        return sb.ToString();
    }

    static string Relative(int dx, int dy)
    {
        var parts = new List<string>();
        if (dy < 0) parts.Add($"north {-dy}");
        if (dy > 0) parts.Add($"south {dy}");
        if (dx > 0) parts.Add($"east {dx}");
        if (dx < 0) parts.Add($"west {-dx}");
        return string.Join(" ", parts);
    }

    // A rough count for the offline brain and the setup screen. Real usage comes
    // back from the API.
    public static long EstimateTokens(string text) => string.IsNullOrEmpty(text) ? 0 : (text.Length + 3) / 4;
}
