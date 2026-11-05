using System;
using System.Collections.Generic;

// What a student decides before the round starts. Every field is one LLM concept,
// and every field is a constraint the agent then has to live with.
[Serializable]
public class AgentConfig
{
    public string name = "Agent";
    public int colour = 0;          // index into Theme.Team
    public string instructions = ""; // the system prompt, verbatim: what the robot reads
    public int model = 2;           // index into Models.All
    public int vision = 1;          // cells seen in every direction: 1, 2 or 3
    public int memory = 1;          // index into MemoryModes.All
    public int effort = 0;          // index into Efforts.All
    public int kind = Agent;        // an agent, or a simple automation: a fixed rule, no AI (errand 1.6)

    // The list as the garage builds it: the goods to buy and to leave, the rule for
    // when nothing on the list is in view, and the student's own words. Compose()
    // turns them into the instructions. With freeText the instructions are
    // written directly instead.
    public List<string> buy = new List<string>();
    public List<string> never = new List<string>();
    public int fallback = 0;        // index into Fallbacks
    public string words = "";
    public bool freeText;

    public static readonly string[] Fallbacks =
    {
        "", "If nothing on this list is in view, keep walking.", "If nothing on this list is in view, walk toward the middle of the market.", "If nothing on this list is in view, stay where you are.",
    };
    public static readonly string[] FallbackLabels = { "No rule", "Keep walking", "Walk to the middle", "Stay put" };

    public const int Agent = 0, Automation = 1;
    public bool IsRule => kind == Automation;

    // The instructions, from the parts; nothing when the student writes them directly.
    public void Compose()
    {
        if (freeText) return;
        var parts = new List<string>();
        if (buy.Count > 0) parts.Add("Buy " + Join(buy) + ".");
        if (never.Count > 0) parts.Add("Never buy " + Join(never) + ".");
        if (fallback > 0 && fallback < Fallbacks.Length) parts.Add(Fallbacks[fallback]);
        if (!string.IsNullOrWhiteSpace(words)) parts.Add(words.Trim());
        instructions = string.Join(" ", parts);
    }

    static string Join(List<string> ids)
    {
        var names = new List<string>();
        foreach (var id in ids) names.Add(Items.Plural(id));
        return names.Count <= 1 ? string.Join("", names) : string.Join(", ", names.GetRange(0, names.Count - 1)) + " and " + names[names.Count - 1];
    }

    // A ready-made list, written in words: the parts are cleared and the words are read as they are.
    public void Write(string text)
    {
        freeText = true;
        instructions = text;
        buy.Clear(); never.Clear(); fallback = 0; words = "";
    }

    public AgentConfig Clone()
    {
        var c = (AgentConfig)MemberwiseClone();
        c.buy = new List<string>(buy);
        c.never = new List<string>(never);
        return c;
    }
}

[Serializable]
public class AgentConfigList { public List<AgentConfig> agents = new List<AgentConfig>(); }

public class ModelInfo
{
    public string id, label, blurb;
    public double inputPerMillion, outputPerMillion; // US dollars, first-party API prices
}

public static class Models
{
    public static readonly ModelInfo[] All =
    {
        new ModelInfo { id = "claude-haiku-5-5",  label = "Haiku 5.5",  blurb = "fast and cheap",     inputPerMillion = 0.10, outputPerMillion = 0.50 },
        new ModelInfo { id = "claude-sonnet-5-5", label = "Sonnet 5.5", blurb = "balanced",           inputPerMillion = 2.00, outputPerMillion = 10.0 },
        new ModelInfo { id = "claude-opus-5-5",   label = "Opus 5.5",   blurb = "most capable",       inputPerMillion = 4.00, outputPerMillion = 20.0 },
    };
}

public static class MemoryModes
{
    public const int None = 0, Short = 1, Long = 2, Notebook = 3;
    public static readonly string[] Labels = { "None", "Last 3 turns", "Last 10 turns", "Notebook" };
    public static readonly string[] Blurbs =
    {
        "Each turn starts from nothing.",
        "The last 3 exchanges ride along in the prompt.",
        "The last 10 exchanges ride along: more context, more tokens.",
        "No history, but the agent may write itself a note that it sees every turn.",
    };
    public static int Turns(int mode) => mode == Short ? 3 : mode == Long ? 10 : 0;
}

public static class Efforts
{
    public static readonly string[] Ids = { "low", "medium", "high" };
    public static readonly string[] Labels = { "Low", "Medium", "High" };
}
