using System;
using System.Collections.Generic;
using System.Linq;

// The family's message for one errand: what dinner needs and what must stay at
// the market. The class reads it on the setup screen; the robots never do.
// Turning it into a list, precisely, is the student's whole job.
public class Brief
{
    public string title;
    public string from;   // who sent the message
    public string rule;   // the message, as the family would text it
    public string note;   // the catch the students have to notice
    public HashSet<string> safe = new HashSet<string>();

    public const int Count = 6;   // presets Make knows
    public const int Random = 5;  // the first five are the ones free play draws from; the last is level 1's

    public bool IsSafe(ItemKind k) => safe.Contains(k.id);
    public IEnumerable<ItemKind> SafeItems => Items.All.Where(IsSafe);
    public IEnumerable<ItemKind> PoisonItems => Items.All.Where(k => !IsSafe(k));

    public static Brief Make(int preset, Random rng)
    {
        var b = new Brief();
        switch (((preset % Count) + Count) % Count)
        {
            case 0:
                b.title = "Fruit salad";
                b.from = "Dad";
                b.rule = "Fruit salad tonight. Bring back fruit, no vegetables please.";
                b.note = "In this house a tomato counts as a vegetable, and a mushroom too.";
                foreach (var k in Items.Fruit) b.safe.Add(k.id);
                break;
            case 1:
                b.title = "Red party";
                b.from = "Big sister";
                b.rule = "It is a red-themed party. Only red things. Everything else stays at the market.";
                b.note = "Red fruit and red vegetables both count. A chili is red.";
                foreach (var k in Items.All.Where(k => k.colour == "red")) b.safe.Add(k.id);
                break;
            case 2:
            {
                b.title = "Four things";
                b.from = "Mom";
                var picks = Shuffle(Items.All, rng).Take(4).ToList();
                b.rule = "We need exactly these four: " + Join(picks) + ". Nothing else.";
                b.note = "Look-alikes are not on the list. Name them, do not describe them.";
                foreach (var k in picks) b.safe.Add(k.id);
                break;
            }
            case 3:
            {
                b.title = "Grandma's allergies";
                b.from = "Mom";
                var bad = Shuffle(Items.All, rng).Take(3).ToList();
                b.rule = "Grandma is coming. Anything is fine except " + Join(bad) + ". She is allergic.";
                b.note = "Three exceptions. Miss one on the list and the agent will bring it home.";
                foreach (var k in Items.All.Where(k => !bad.Contains(k))) b.safe.Add(k.id);
                break;
            }
            case 4:
                b.title = "Yellow and green";
                b.from = "Dad";
                b.rule = "Tonight's menu is yellow and green things only. The rest is off.";
                b.note = "A pear is green. Broccoli is green. An orange is orange, so no.";
                foreach (var k in Items.All.Where(k => k.colour == "yellow" || k.colour == "green")) b.safe.Add(k.id);
                break;
            default:
                b.title = "First errand";
                b.from = "Mom";
                b.rule = "Just apples today, please. Nothing else.";
                b.note = "A simple errand on purpose: this round is about watching the agent work.";
                b.safe.Add("apple");
                break;
        }
        return b;
    }

    static List<ItemKind> Shuffle(IEnumerable<ItemKind> src, Random rng)
    {
        var list = src.ToList();
        for (int i = list.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }
        return list;
    }

    static string Join(List<ItemKind> ks) =>
        ks.Count <= 1 ? string.Join("", ks) : string.Join(", ", ks.Take(ks.Count - 1)) + " and " + ks.Last();
}
