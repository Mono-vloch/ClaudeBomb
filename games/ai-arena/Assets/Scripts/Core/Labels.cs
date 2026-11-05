using System.Collections.Generic;

// Short forms of the curriculum's check labels, for the debrief's lamps: the full
// label stays in the file and in the report the platform keeps. Anything not
// listed here is cut at its colon or its fifth word.
public static class Labels
{
    static readonly Dictionary<string, string> Short = new Dictionary<string, string>
    {
        ["Wrote a list"] = "Wrote a list",
        ["Said what to buy: apples"] = "Named apples",
        ["Said what to leave at the market"] = "Said what to leave",
        ["Came home with dinner"] = "Came home",
        ["Named the items, not just \"fruit\""] = "Named the items",
        ["Covered the catch: the tomato"] = "Covered the tomato",
        ["Said what to do when nothing on the list is in view"] = "A rule for nothing in view",
        ["Found something on the list"] = "Found a listed item",
        ["Told it what to do when nothing is in view"] = "A rule for nothing in view",
        ["Lasted the budget"] = "Lasted the budget",
        ["Bought something on the list"] = "Bought a listed item",
        ["Wrote in its notebook"] = "Wrote in its notebook",
        ["No notebook to write in"] = "No notebook",
        ["Bought at least one thing on the list"] = "Bought a listed item",
        ["Nothing in the basket, so no cost per item"] = "Empty basket",
        ["Named the items and the exceptions"] = "Named items and exceptions",
        ["Said what to do when the list does not fit"] = "A rule for nothing in view",
        ["Chose a simple automation where one did the job"] = "A rule where a rule did",
        ["Chose an agent where the task needed judgment"] = "An agent where it needed one",
        ["Marked the task for a person as neither"] = "Neither, for the person's call",
    };

    public static string Of(string full)
    {
        if (string.IsNullOrEmpty(full)) return "";
        if (Short.TryGetValue(full, out var s)) return s;
        if (full.StartsWith("Cheapest item of the round: ")) return "Cheapest item: " + full.Substring("Cheapest item of the round: ".Length);
        if (full.StartsWith("Cost per item: ")) return full;
        int colon = full.IndexOf(':');
        if (colon > 0 && colon <= 30) return full.Substring(0, colon);
        var words = full.Split(' ');
        return words.Length <= 5 ? full : string.Join(" ", words, 0, 5) + "…";
    }

    // The first sentence or two of a longer line, for a coach strip: at least sixty
    // characters, whole sentences, the rest left to the file or the debrief.
    public static string Lead(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        int at = 0;
        while (at < text.Length)
        {
            int dot = text.IndexOf(". ", at);
            if (dot < 0) return text;
            at = dot + 1;
            if (at >= 60) return text.Substring(0, at);
        }
        return text;
    }
}
