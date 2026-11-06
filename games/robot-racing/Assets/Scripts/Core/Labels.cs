using System.Collections.Generic;

// Short forms of the curriculum's check labels, for the debrief's lamps: the full
// label stays in the file and in the report the platform keeps. Anything not
// listed here is cut at its colon or its fifth word.
public static class Labels
{
    static readonly Dictionary<string, string> Short = new Dictionary<string, string>
    {
        ["Named the five parts"] = "Named the five parts",
        ["Said what the robot knows at the start: nothing"] = "Knows nothing at the start",
        ["Trained a robot to a finish"] = "Trained it to a finish",
        ["Found why the first setup never improved"] = "Found why it never improved",
        ["Trained until the curve settled"] = "Trained until it settled",
        ["The robot finished most of several races, not one"] = "Finished most races",
        ["Read the rule book for one situation"] = "Read the rule book",
        ["Read the curve correctly"] = "Read the curve",
        ["Sorted the six problems"] = "Sorted the six problems",
        ["Explained why the robot is not supervised learning"] = "Why it is not supervised",
        ["The robot got past both bends"] = "Got past both bends",
        ["Explained why the first robot could not learn it"] = "Why the first robot could not",
        ["The robot finished"] = "Finished",
        ["A constraint was set"] = "Set a constraint",
        ["The rule book uses slow down where it matters"] = "Slows down where it matters",
        ["Set finish, crash and on-the-way points"] = "Set the three points",
        ["Learned within the attempt budget"] = "Learned within the budget",
        ["The crash costs more than any single move can earn"] = "A crash costs more than a move",
        ["Predicted what the robot would do before running"] = "Predicted first",
        ["The fixed robot finished"] = "The fixed robot finished",
        ["Explained why a curve can rise while the robot never finishes"] = "Why the curve lies",
        ["Set a high chance that falls"] = "A high chance that falls",
        ["The robot's line improved after its first finish"] = "The line improved",
        ["The curve settled"] = "The curve settled",
        ["Finished within the attempt budget"] = "Finished within the budget",
        ["No loophole exploited"] = "No loophole",
        ["Compared the prediction with what happened"] = "Compared the prediction",
        ["The sensors on are the ones the track needed"] = "The right sensors on",
        ["The test track was never trained on"] = "A test track never trained on",
        ["The robot finished the test track"] = "Finished the test track",
        ["Explained what the one-track robot had learned"] = "What the one-track robot learned",
        ["Picked the measure that fits the stated cost"] = "The measure fits the cost",
        ["Explained in one line why finish rate lied"] = "Why finish rate lied",
        ["Changed one knob per run"] = "One knob per run",
        ["Kept the seed"] = "Kept the seed",
        ["Named the knob that helped"] = "Named the knob that helped",
        ["Recognized the rewrite speed set too high"] = "Spotted the rewrite too high",
        ["Trained within the budget"] = "Trained within the budget",
        ["The test track passed"] = "Passed the test track",
        ["The knobs were set one at a time"] = "Knobs one at a time",
        ["The real run finished, or the difference that broke it was named"] = "The real run, or what broke it",
        ["The measure chosen matched the stated cost"] = "The measure fits the cost",
    };

    public static string Of(string full)
    {
        if (string.IsNullOrEmpty(full)) return "";
        if (Short.TryGetValue(full, out var s)) return s;
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
