using System.Collections.Generic;
using System.Linq;

// Errand 1.6: does this task need an agent? Five family messages. For each one
// the class decides: an agent, a simple automation (a fixed rule, no AI), or
// neither. Then every shopping task is run with both kinds on the same market,
// side by side, so the cost and the result of each can be compared.
public class DecideTask
{
    public string from, message, note;
    public int right;              // Decide.Agent, Automation or Neither
    public string why;             // the reason, on the debrief
    public HashSet<string> safe;   // what the family actually wants
    public string[] rule;          // the fixed rule, written in advance: the items it takes
    public string list;            // the agent's job description, written from the message

    public bool Runs => right != Decide.Neither;

    public Brief Brief() => new Brief { title = "Task", from = from, rule = message, note = note, safe = new HashSet<string>(safe) };

    public string RuleText => "Fixed rule: take " + string.Join(", ", rule.Select(Items.Plural)) + ". Nothing else.";
}

public static class Decide
{
    public const int Agent = 0, Automation = 1, Neither = 2;
    public static readonly string[] Choices = { "An agent", "A simple automation", "Neither" };

    static HashSet<string> Set(params string[] ids) => new HashSet<string>(ids);
    static string[] Fruit => Items.Fruit.Select(k => k.id).ToArray();

    public static readonly DecideTask[] Tasks =
    {
        new DecideTask
        {
            from = "Mom", message = "Just apples today, please. Nothing else.",
            note = "One named thing, no exceptions.",
            right = Automation,
            why = "One named thing and no exceptions: a fixed rule does it for nothing. An agent does it too, and pays tokens on every turn.",
            safe = Set("apple"), rule = new[] { "apple" },
            list = "Buy apples. Nothing else. If no apples are in view, keep walking.",
        },
        new DecideTask
        {
            from = "Dad", message = "We need exactly lemons, corn, pears and potatoes. Nothing else.",
            note = "A fixed list of names.",
            right = Automation,
            why = "A fixed list of names is a fixed rule. Nothing to decide.",
            safe = Set("lemon", "corn", "pear", "potato"), rule = new[] { "lemon", "corn", "pear", "potato" },
            list = "Buy lemons, corn, pears and potatoes, nothing else. If none of them is in view, keep walking.",
        },
        new DecideTask
        {
            from = "Dad", message = "Fruit salad tonight: fruit only, no vegetables. And in this house a tomato is a vegetable.",
            note = "A category, with a catch.",
            right = Agent,
            why = "\"Fruit, but not the tomato\" needs judgment. The rule was written before the catch and takes the tomato; an agent can be told.",
            safe = new HashSet<string>(Fruit),
            rule = Fruit.Concat(new[] { "tomato" }).ToArray(), // whoever wrote the rule knew their botany, not this house
            list = "Buy fruit for a fruit salad: apples, bananas, oranges, strawberries, grapes, lemons, cherries and pears. " +
                   "In this house a tomato is a vegetable: never buy tomatoes, or any other vegetable. If nothing on the list is in view, keep walking.",
        },
        new DecideTask
        {
            from = "Big sister", message = "A colorful fruit bowl for the guests: at least three different fruits, no vegetables. And no strawberries, one guest is allergic.",
            note = "Different kinds, and an allergy to work around.",
            right = Agent,
            why = "\"Different kinds\" and an allergy to work around: judgment. The rule takes strawberries like any other fruit.",
            safe = new HashSet<string>(Fruit.Where(f => f != "strawberry")),
            rule = Fruit,
            list = "Buy fruit for a fruit bowl, as many different kinds as you can: apples, bananas, oranges, grapes, lemons, cherries and pears. " +
                   "Never strawberries: a guest is allergic. No vegetables. If nothing on the list is in view, keep walking.",
        },
        new DecideTask
        {
            from = "Mom", message = "Should we have the party at home or at the restaurant on Saturday? Decide for us.",
            note = "Not a shopping task.",
            right = Neither,
            why = "A family decision. Neither a rule nor an agent should take it: a person decides, then sends the list.",
            safe = Set(), rule = new string[0], list = "",
        },
    };

    public static int ShoppingTasks => Tasks.Count(t => t.Runs);
}
