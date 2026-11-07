using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Maaloomatiia.Games;
using Maaloomatiia.Games.Claude;

public class ClaudeReply
{
    public bool ok;
    public string text = "";
    public string error;
    public long inputTokens, outputTokens;
}

// The browser version's way to Claude: the Maaloomatiia Games platform's key keeper
// (/api/claude on the game's own site), through the kit's Relay. The platform signed
// the player in before the game loaded and the browser carries that sign-in with every
// call, so the game never sees a key or a code. Up go one agent's model, effort,
// instructions and remembered turns; back comes a reply in the Messages API's shape
// with the keeper's token counts. If the keeper says the sign-in has run out, the
// player goes back to the menu to sign in again (Platform.SignInAgain) and the menu
// returns them to this game.
public static class ClaudeProxy
{
    // The conversation as the keeper wants it: user and assistant turns, ending with the user's.
    // Built as JObjects, not anonymous types: the web build strips an anonymous type's
    // getters, so Newtonsoft wrote every message as {} and the keeper refused each call
    // ("messages must alternate"), which left the agents standing (found 7 Nov 2026).
    public static JArray Messages(List<(string user, string assistant)> history, string userTurn)
    {
        var messages = new JArray();
        foreach (var (u, a) in history)
        {
            messages.Add(Message("user", u));
            messages.Add(Message("assistant", a));
        }
        messages.Add(Message("user", userTurn));
        return messages;
    }

    static JObject Message(string role, string content) => new JObject { ["role"] = role, ["content"] = content ?? "" };

    // What goes up, as JSON. The smoke test reads it; Send lets the Relay build the same.
    public static string Body(string model, string effort, string system, List<(string user, string assistant)> history, string userTurn) =>
        new JObject { ["model"] = model, ["effort"] = effort, ["system"] = system, ["messages"] = Messages(history, userTurn) }.ToString(Formatting.None);

    public static IEnumerator Send(string model, string effort, string system, List<(string user, string assistant)> history, string userTurn, Action<ClaudeReply> done)
    {
        RelayReply relay = null;
        yield return Relay.Send(model, effort, system, Messages(history, userTurn), r => relay = r);
        var reply = new ClaudeReply { ok = relay.ok, text = relay.text, error = relay.error, inputTokens = relay.inputTokens, outputTokens = relay.outputTokens };
        if (relay.unauthorized)
        {
            reply.error = "Your sign-in has run out. Back to the menu to sign in again.";
            Platform.SignInAgain();
        }
        else if (relay.busy) reply.error = "Too many calls at once. The robot waits a turn.";
        if (reply.ok)
        {
            // The keeper does not always report usage; estimate rather than show zero.
            if (reply.inputTokens == 0)
                reply.inputTokens = Observation.EstimateTokens(system + userTurn) + history.Sum(h => Observation.EstimateTokens(h.user + h.assistant));
            if (reply.outputTokens == 0) reply.outputTokens = Observation.EstimateTokens(reply.text);
        }
        done?.Invoke(reply);
    }

    // Asks the keeper whether it is there, signed in and able to reach Claude, without
    // calling Claude. A refused sign-in sends the player back to the menu straight away.
    public static IEnumerator Check(Action<bool> done)
    {
        RelayReply relay = null;
        yield return Relay.Check(r => relay = r);
        if (relay.unauthorized) Platform.SignInAgain();
        done?.Invoke(relay.ok && relay.claude);
    }
}
