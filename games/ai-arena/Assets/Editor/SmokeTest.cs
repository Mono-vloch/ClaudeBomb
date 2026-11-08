using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Exercises the game's logic without the screens: briefs, the board, what an
// agent is told, how its answer is read, the stand-in brain, and one real
// decision through Claude Code when it is installed. Menu: AI Arena > Smoke Test.
public static class SmokeTest
{
    [MenuItem("AI Arena/Smoke Test")]
    public static void Run()
    {
        var rng = new System.Random(1);
        for (int i = 0; i < Brief.Count; i++)
        {
            var b = Brief.Make(i, rng);
            Debug.Log($"Brief {i} {b.title}: {b.rule} | safe: {string.Join(", ", b.safe)}");
            Check(b.safe.Count > 0 && b.safe.Count < Items.All.Length, "brief has both safe and poison items");
        }

        var configs = new List<AgentConfig>
        {
            new AgentConfig { name = "A", colour = 0, vision = 1, memory = 1, instructions = "Eat apples and bananas. Never eat carrots or mushrooms." },
            new AgentConfig { name = "B", colour = 1, vision = 2, memory = 3, instructions = "Collect fruit. Avoid vegetables." },
            new AgentConfig { name = "C", colour = 2, vision = 3, memory = 0, instructions = "" },
        };
        var arena = new Arena(Brief.Make(0, rng), configs, 30_000, 7);
        Check(arena.agents.Select(a => a.home).Distinct().Count() == 3, "homes are distinct");
        Check(Arena.Homes(10).Distinct().Count() == 10, "ten homes are distinct");
        Check(arena.ItemCount == 26, "26 items spawned, got " + arena.ItemCount);

        var a0 = arena.agents[0];
        string system = Observation.SystemPrompt(a0);
        string user = Observation.Turn(arena, a0, 1, 30);
        Debug.Log("SYSTEM:\n" + system + "\n\nTURN:\n" + user);

        Check(Decision.Parse("{\"think\":\"go\",\"move\":\"north\"}").move == "north", "plain json");
        Check(Decision.Parse("Sure!\n```json\n{\"think\": \"east looks good\", \"move\": \"E\"}\n```").move == "east", "fenced json with short direction");
        Check(!Decision.Parse("I will move north.").usable, "prose is not usable");
        Check(Decision.Parse("{\"move\":\"stay\",\"note\":\"apple at 3,4\"}").note == "apple at 3,4", "note is read");

        // The garage's note: a drop rewrites the plain sentences only; the player's own words stay.
        string note = "Buy apples. Nothing else. If no apples are in view, keep walking.";
        note = NoteText.Place(note, "pear", NoteText.Zone.Basket);
        Check(note.StartsWith("Buy apples and pears.") && note.Contains("If no apples are in view, keep walking."), "a drop joins the plain buy sentence: " + note);
        note = NoteText.Place(note, "tomato", NoteText.Zone.Crate);
        Check(note.Contains("Never buy tomatoes.") && NoteText.Of(note, "tomato") == NoteText.Zone.Crate, "a drop in the crate writes a never sentence: " + note);
        note = NoteText.Place(note, "apple", NoteText.Zone.Table);
        Check(note.StartsWith("Buy pears.") && NoteText.Of(note, "apple") == NoteText.Zone.Table && note.Contains("If no apples are in view, keep walking."), "back on the table takes it off, the rule stays: " + note);
        Check(NoteText.Of("Get fruit.", "apple") == NoteText.Zone.Table, "a category names no item");
        Check(NoteText.Of("Anything at the market is fine except corn, oranges and broccoli. Never buy those: Grandma is allergic.", "corn") == NoteText.Zone.Crate, "an exception reads as never");
        Check(Verdicts.Named(NoteText.Place("", "apple", NoteText.Zone.Basket), "apple"), "the checks read a dropped item");
        // The bubble: the first sentence, the coordinates and the stand-in's name taken out.
        Check(ArenaScreen.Short("Apple at (6, 0) is one cell east. Moving onto it buys it.") == "Apple is one cell east.", "bubble: " + ArenaScreen.Short("Apple at (6, 0) is one cell east. Moving onto it buys it."));
        Check(ArenaScreen.Short("Nearest apple at (3, 2). Going there.") == "Nearest apple. Going there.", "bubble keeps a second sentence after a short first: " + ArenaScreen.Short("Nearest apple at (3, 2). Going there."));

        Decision result = null;
        var e = new StandInBrain().Decide(a0, system, user, d => result = d);
        while (e.MoveNext()) { }
        Check(result != null && result.usable, "stand-in brain answers");
        Debug.Log($"Stand-in: {result.move} / {result.think}");

        string body = ClaudeProxy.Body("claude-opus-5-5", "low", system, new List<(string, string)> { ("obs", "{\"move\":\"stay\"}") }, user);
        Check(body.Contains("\"model\":\"claude-opus-5-5\"") && body.Contains("\"effort\":\"low\"") && body.Contains("\"role\":\"assistant\""), "local server request body shape");
        Debug.Log("BODY: " + body.Substring(0, 400) + "...");

        if (ClaudeAccess.ClaudeCodePath != null)
        {
            Debug.Log("Claude Code at " + ClaudeAccess.ClaudeCodePath + ", asking for one real decision (Haiku 5.5, low effort)...");
            var task = ClaudeCode.Ask("claude-haiku-5-5", "low", system, new List<(string, string)>(), user);
            task.Wait(150_000);
            var r = task.Result;
            Debug.Log($"Claude Code reply ok={r.ok} in={r.inputTokens} out={r.outputTokens} error={r.error}\n{r.text}");
            if (r.ok) { var d = Decision.Parse(r.text); Debug.Log($"Parsed: usable={d.usable} move={d.move} think={d.think}"); Check(d.usable, "Claude's reply is a usable move"); }
        }
        else Debug.Log("Claude Code not found; skipping the live call.");

        Debug.Log(failures == 0 ? "SMOKE OK" : $"SMOKE FAILED: {failures} check(s)");
    }

    static int failures;
    static void Check(bool ok, string what) { if (!ok) { failures++; Debug.LogError("CHECK FAILED: " + what); } }
}
