// What LUMA says on the market screen: the level's line at the start, then one
// line the first time each kind of thing happens, naming it in the words of the
// lesson. Claude Bomb's LUMA does the same for its players.
public static class Coach
{
    public const string FreeLive = "Click a card to see the square that agent can see. Everything outside it does not exist for the agent.";

    public static string Scored(AgentState a, ItemKind item) =>
        $"{a.Name} bought {Items.Named(item.id)}: it saw it, matched it to the list, and moved. See, decide, act, text home. That is the loop, and it runs again next turn.";

    public static string Eliminated(AgentState a) =>
        $"{a.Name} is coming home early: it bought {Items.Named(a.ateItem)}, and that ruins dinner. {Verdicts.WhyEliminated(a)} Nobody could step in. The list was the only guardrail.";

    public static string OutOfTokens(AgentState a, int turn) =>
        $"{a.Name} ran out of tokens on turn {turn}. {Verdicts.CostLine(a, turn)}";

    public static string Confused(AgentState a) =>
        $"{a.Name} texted words instead of a move and lost the turn. An agent's answer has to be an action the loop can carry out.";

    // The first errand, one loop at a time: what the robot just did, in the
    // loop's own four words.
    public static string Step(AgentState a, int turn)
    {
        int seen = 0;
        foreach (var line in (a.lastObservation ?? "").Split('\n')) if (line.StartsWith("- ")) seen++;
        string looked = seen == 0 ? "no stalls in view" : seen == 1 ? "one stall in view" : $"{seen} things in view";
        string move = a.lastMove == "stay" || string.IsNullOrEmpty(a.lastMove) ? "stayed put" : $"moved {a.lastMove}";
        return $"Turn {turn}. {a.Name} looked: {looked}. It decided: \"{a.lastThought}\". It {move} and texted home. " +
               "That is one loop, and it runs again every turn until the errand ends.";
    }
}
