using System;
using System.Collections.Generic;
using System.Linq;

// Pure functions: given a DeviceState, return the correct answer for a module.
// Every rule here must match the manual text (ClaudeClient.systemPrompt) exactly —
// change one, change the other, in the same commit.
public static class Rules
{
    public static int WireAnswer(DeviceState d)
    {
        int n(string c) => d.wires.Count(w => w == c);
        char last = d.casing.stamp[d.casing.stamp.Length - 1];
        if (n("white") == 0 && d.casing.cells >= 2) return 1;
        if (d.wires[3] == "black" && char.IsDigit(last)) return 4;
        if (n("red") == 1 && d.casing.sync) return Array.IndexOf(d.wires, "red") + 1;
        if (n("blue") >= 2) return Array.LastIndexOf(d.wires, "blue") + 1;
        if (!d.casing.hold) return 2;
        return 3;
    }

    // Positions (1-4) to press, in order, so the glyphs land in the chosen set's order.
    public static int[] GlyphAnswer(DeviceState d)
    {
        var order = new int[4];
        for (int i = 0; i < 4; i++)
        {
            int glyphId = d.glyphPressOrder[i];
            order[i] = Array.IndexOf(d.glyphAtPosition, glyphId) + 1;
        }
        return order;
    }

    public static string[] SequenceAnswer(DeviceState d)
    {
        var low  = new Dictionary<string, string> { { "red", "blue" }, { "blue", "green" }, { "green", "red" }, { "yellow", "yellow" } };
        var high = new Dictionary<string, string> { { "red", "yellow" }, { "blue", "red" }, { "green", "blue" }, { "yellow", "green" } };
        var map = d.casing.cells >= 2 ? high : low;
        return d.sequence.Select(c => map[c]).ToArray();
    }

    public static int GridAnswer(DeviceState d)
    {
        string best = null; int bestN = -1, bestFirst = 99;
        foreach (var c in new[] { "red", "blue", "yellow" })
        {
            int n = d.grid.Count(x => x == c);
            if (n == 0) continue;
            int first = Array.IndexOf(d.grid, c);
            if (n > bestN || (n == bestN && first < bestFirst)) { best = c; bestN = n; bestFirst = first; }
        }
        if (d.grid[4] == best) return 5;
        return d.casing.hold ? Array.LastIndexOf(d.grid, best) + 1
                              : Array.IndexOf(d.grid, best) + 1;
    }

    // Five switches. Even cell count -> 1,3,5 up, 2,4 down. Odd -> 2,4 up, 1,3,5 down.
    // VENT inverts the whole pattern. Always alternating, so neither interlock pair
    // (2&3, 4&5) is ever both-up in a valid target (checked live in ModuleE_Switches).
    public static bool[] SwitchTarget(DeviceState d)
    {
        bool[] t = d.casing.cells % 2 == 0
            ? new[] { true, false, true, false, true }
            : new[] { false, true, false, true, false };
        if (d.casing.vent) t = t.Select(x => !x).ToArray();
        return t;
    }
}
