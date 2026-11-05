using System.Collections.Generic;
using System.Linq;

public enum ItemFamily { Fruit, Vegetable }

// One kind of thing that can lie on the board. The id doubles as the sprite name
// (Resources/Sprites/<id>) and as the word the agents see in their observations.
public class ItemKind
{
    public string id;
    public ItemFamily family;
    public string colour; // the word a colour-based brief uses

    public ItemKind(string id, ItemFamily family, string colour) { this.id = id; this.family = family; this.colour = colour; }
    public override string ToString() => id;
}

public static class Items
{
    public static readonly ItemKind[] All =
    {
        new ItemKind("apple",      ItemFamily.Fruit,     "red"),
        new ItemKind("banana",     ItemFamily.Fruit,     "yellow"),
        new ItemKind("orange",     ItemFamily.Fruit,     "orange"),
        new ItemKind("strawberry", ItemFamily.Fruit,     "red"),
        new ItemKind("grapes",     ItemFamily.Fruit,     "purple"),
        new ItemKind("lemon",      ItemFamily.Fruit,     "yellow"),
        new ItemKind("cherry",     ItemFamily.Fruit,     "red"),
        new ItemKind("pear",       ItemFamily.Fruit,     "green"),
        new ItemKind("carrot",     ItemFamily.Vegetable, "orange"),
        new ItemKind("potato",     ItemFamily.Vegetable, "brown"),
        new ItemKind("broccoli",   ItemFamily.Vegetable, "green"),
        new ItemKind("mushroom",   ItemFamily.Vegetable, "brown"),
        new ItemKind("chili",      ItemFamily.Vegetable, "red"),
        new ItemKind("tomato",     ItemFamily.Vegetable, "red"),
        new ItemKind("corn",       ItemFamily.Vegetable, "yellow"),
    };

    public static ItemKind Get(string id) => All.FirstOrDefault(k => k.id == id);

    // "an apple", "a carrot", "grapes": for the log lines and the verdicts.
    public static string Named(string id) => id == "grapes" ? "grapes" : ("aeiou".IndexOf(id[0]) >= 0 ? "an " : "a ") + id;
    public static string Plural(string id) => id switch
    {
        "grapes" or "corn" or "broccoli" => id,
        "potato" or "tomato" => id + "es",
        "chili" => "chilies",
        _ => id.EndsWith("y") ? id.Substring(0, id.Length - 1) + "ies" : id + "s",
    };
    public static IEnumerable<ItemKind> Fruit => All.Where(k => k.family == ItemFamily.Fruit);
    public static IEnumerable<ItemKind> Vegetables => All.Where(k => k.family == ItemFamily.Vegetable);
}
