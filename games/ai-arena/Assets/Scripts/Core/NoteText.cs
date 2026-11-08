using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// The note pinned to the agent is its list, word for word (the system prompt). In the
// garage the world shows what the note says: produce named in a "buy" sentence sits in the
// basket, produce named in a "never" sentence in the "No" crate, the rest on the table. A
// drop rewrites only the plain part of the note ("Buy apples and pears." / "Never buy
// tomatoes."); the catch and the rule for an empty view stay in the player's own words, in
// the same text, because the level's checks read them there.
public static class NoteText
{
    public enum Zone { Table, Basket, Crate }

    static readonly Regex Rule = new Regex(@"\b(if|when|otherwise|else|nothing|none|explore|wander|in view)\b", RegexOptions.IgnoreCase);
    static readonly string[] NeverStarts = { "never", "don't", "do not", "no ", "not ", "leave", "avoid", "skip", "stay away", "nothing", "none" };
    static readonly string[] NeverWords = { " except", "allerg", "never buy", "don't buy", "do not buy", "stay away", "no other", "ruin", "leave " };

    public static IEnumerable<string> Sentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;
        foreach (var p in Regex.Split(text.Trim(), @"(?<=[.!?])\s+|\n+"))
            if (p.Trim().Length > 0) yield return p.Trim();
    }

    public static bool IsRule(string sentence) => Rule.IsMatch(sentence);

    public static bool IsNever(string sentence)
    {
        string s = sentence.Trim().ToLowerInvariant();
        return NeverStarts.Any(s.StartsWith) || NeverWords.Any(s.Contains);
    }

    static bool Names(string sentence, string id)
    {
        string s = sentence.ToLowerInvariant();
        return Regex.IsMatch(s, $@"\b{Regex.Escape(id)}\b") || Regex.IsMatch(s, $@"\b{Regex.Escape(Items.Plural(id))}\b") || Regex.IsMatch(s, $@"\b{Regex.Escape(id.TrimEnd('s'))}\b");
    }

    // What the note says, read the way the garage shows it.
    public static (HashSet<string> buy, HashSet<string> never) Parse(string text)
    {
        var buy = new HashSet<string>(); var never = new HashSet<string>();
        foreach (var s in Sentences(text))
        {
            if (IsRule(s) && !IsPlain(s, out _, out _)) continue;
            bool no = IsNever(s);
            foreach (var k in Items.All) if (Names(s, k.id)) (no ? never : buy).Add(k.id);
        }
        buy.ExceptWith(never);
        return (buy, never);
    }

    public static Zone Of(string text, string id)
    {
        var (buy, never) = Parse(text);
        return never.Contains(id) ? Zone.Crate : buy.Contains(id) ? Zone.Basket : Zone.Table;
    }

    // A plain sentence is one the garage writes itself: "Buy X, Y and Z." or "Never buy X."
    public static bool IsPlain(string sentence, out bool never, out List<string> ids)
    {
        never = false; ids = new List<string>();
        var m = Regex.Match(sentence.Trim(), @"^(Buy|Never buy) (.+?)\.?$");
        if (!m.Success) return false;
        never = m.Groups[1].Value == "Never buy";
        foreach (var raw in Regex.Split(m.Groups[2].Value, @",\s*|\s+and\s+"))
        {
            string w = raw.Trim().ToLowerInvariant();
            if (w.Length == 0) continue;
            var k = Items.All.FirstOrDefault(x => x.id == w || Items.Plural(x.id) == w);
            if (k == null) { ids.Clear(); return false; }
            ids.Add(k.id);
        }
        return ids.Count > 0;
    }

    static string Join(List<string> ids)
    {
        var names = ids.Select(Items.Plural).ToList();
        return names.Count <= 1 ? string.Join("", names) : string.Join(", ", names.Take(names.Count - 1)) + " and " + names.Last();
    }

    static string Plain(bool never, List<string> ids) => (never ? "Never buy " : "Buy ") + Join(ids) + ".";

    // The note after a drop: the item leaves every plain sentence, then joins the plain
    // sentence of its new place, or starts one. Everything else in the note is left as it is.
    public static string Place(string text, string id, Zone zone)
    {
        var sentences = Sentences(text).ToList();
        for (int i = sentences.Count - 1; i >= 0; i--)
        {
            if (!IsPlain(sentences[i], out bool never, out var ids)) continue;
            if (!ids.Remove(id)) continue;
            if (ids.Count == 0) sentences.RemoveAt(i); else sentences[i] = Plain(never, ids);
        }
        if (zone != Zone.Table)
        {
            bool wantNever = zone == Zone.Crate;
            int at = -1;
            for (int i = 0; i < sentences.Count; i++)
                if (IsPlain(sentences[i], out bool never, out var ids) && never == wantNever) { ids.Add(id); sentences[i] = Plain(never, ids); at = i; break; }
            if (at < 0)
            {
                int insert = 0;
                if (wantNever) { for (int i = 0; i < sentences.Count; i++) if (IsPlain(sentences[i], out bool never, out _) && !never) insert = i + 1; }
                sentences.Insert(insert, Plain(wantNever, new List<string> { id }));
            }
        }
        return string.Join(" ", sentences);
    }
}
