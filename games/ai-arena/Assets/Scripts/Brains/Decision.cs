using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// What comes back from a brain each turn, plus what it cost.
public class Decision
{
    public string move = "stay";    // north, south, east, west or stay
    public string think = "";
    public string note;             // only when the agent keeps a notebook
    public bool usable;             // false: the reply could not be read as a move
    public string error;            // transport or API trouble, for the log
    public string raw = "";
    public long inputTokens, outputTokens;

    static readonly Dictionary<string, string> MoveWords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["north"] = "north", ["n"] = "north", ["up"] = "north",
        ["south"] = "south", ["s"] = "south", ["down"] = "south",
        ["east"] = "east", ["e"] = "east", ["right"] = "east",
        ["west"] = "west", ["w"] = "west", ["left"] = "west",
        ["stay"] = "stay", ["wait"] = "stay", ["none"] = "stay", ["stop"] = "stay",
    };

    // Reads the first JSON object in the reply. Anything around it (a sentence,
    // a code fence) is ignored; a reply with no readable move costs the turn.
    public static Decision Parse(string text)
    {
        var d = new Decision { raw = text ?? "" };
        if (string.IsNullOrWhiteSpace(text)) return d;
        int open = text.IndexOf('{'), close = text.LastIndexOf('}');
        if (open < 0 || close <= open) return d;
        try
        {
            var o = JObject.Parse(text.Substring(open, close - open + 1));
            string move = (o["move"] ?? o["action"] ?? o["direction"])?.ToString()?.Trim() ?? "";
            if (!MoveWords.TryGetValue(move, out d.move)) { d.move = "stay"; return d; }
            d.think = o["think"]?.ToString() ?? o["thought"]?.ToString() ?? "";
            d.note = o["note"]?.ToString();
            d.usable = true;
        }
        catch (Exception) { }
        return d;
    }
}
