using System.Collections.Generic;
using UnityEngine;

// Everything that happens to one agent during a round. Built fresh from its
// AgentConfig when the round starts.
public class AgentState
{
    public AgentConfig config;
    public int index;

    public Vector2Int home;
    public Vector2Int pos;
    public int facing = 2; // 0 north, 1 east, 2 south, 3 west; only for the sprite

    public int score;
    public bool alive = true;
    public bool outOfTokens;
    public int eliminatedTurn = -1;
    public string ateItem;             // what killed it
    public readonly List<string> eaten = new List<string>();

    public long budget;
    public long inputTokens, outputTokens;
    public long TokensUsed => inputTokens + outputTokens;
    public long TokensLeft => System.Math.Max(0, budget - TokensUsed);
    public double CostUsd => (inputTokens * Models.All[config.model].inputPerMillion
                            + outputTokens * Models.All[config.model].outputPerMillion) / 1_000_000.0;

    public string lastThought = "";
    public string lastMove = "";
    public string lastObservation = ""; // the text it was handed this turn: everything it knew
    public string notebook = "";
    public int confusedTurns;          // replies that were not usable JSON
    public int blockedTurns;           // tried to walk into an edge or another robot
    public string lastError;

    // The exchanges this agent remembers, oldest first. Trimmed to its memory mode.
    public readonly List<(string user, string assistant)> history = new List<(string, string)>();
    public readonly List<string> log = new List<string>();

    public string Name => config.name;
    public bool Active => alive && !outOfTokens;

    public string Status =>
        !alive ? $"Sent home on turn {eliminatedTurn}: bought {Items.Named(ateItem)}"
        : outOfTokens ? "Out of tokens"
        : "Shopping";

    public void Remember(string user, string assistant)
    {
        int keep = MemoryModes.Turns(config.memory);
        if (keep == 0) return;
        history.Add((user, assistant));
        while (history.Count > keep) history.RemoveAt(0);
    }
}
