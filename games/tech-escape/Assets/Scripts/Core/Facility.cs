using System;
using System.Collections.Generic;
using System.Linq;

public enum Practised { Yes, Missed, NotNeeded }

// The running case: the Harbor Permits Office, a public-sector service that
// issues permits. Its systems are the facility's rooms; its people handle
// sensitive information without being security staff. The course fixes only
// that much; the rest is the game's.
public static class Facility
{
    public const string Service = "Harbor Permits Office";
    public static readonly string[] People = { "Dana, clerk", "Omar, supervisor", "Lina, IT administrator", "Nour, finance", "Sami, contractor (left in June)" };

    // The rooms, in the order the course opens them, each at a door of the corridor.
    public static readonly string[] Rooms =
    {
        "Risk Room", "Suspicious Inbox", "Password Vault", "Malware Containment", "Identity Chamber", "Patch Laboratory",
        "Network Maze", "Firewall Control", "Encryption Chamber", "Cloud Chamber", "Operations Center", "Backup Vault",
    };
    public static int RoomIndex(string room) => Array.IndexOf(Rooms, room);
}

// One thing the student decides about an item: a choice among options.
public class Field
{
    public string key, label;
    public string[] options;
    public bool once;   // asked once for the whole room, not per item
}

// One row of the room: an asset, an account, a message, a rule, a step.
public class Item
{
    public string key, name, detail;
    public bool hidden;          // not listed until the student adds it (the asset nobody listed)
    public bool decoy;           // can be added, but should not be
    public Item(string key, string name, string detail = "") { this.key = key; this.name = name; this.detail = detail; }
}

// What the student has decided in a room.
public class RoomState
{
    public readonly Dictionary<string, int> picks = new Dictionary<string, int>(); // "item/field" -> option
    public readonly List<string> order = new List<string>();                      // item keys, in the student's order
    public readonly HashSet<string> added = new HashSet<string>();
    public bool signed; public string signedBy = ""; public string signedOn = "";
    public string text = "";

    public int Pick(string item, string field) => picks.TryGetValue(item + "/" + field, out int v) ? v : -1;
    public bool Is(string item, string field, int option) => Pick(item, field) == option;
    public void Set(string item, string field, int option) => picks[item + "/" + field] = option;
    public int Position(string item) => order.IndexOf(item);
    public RoomState Clone()
    {
        var c = new RoomState { signed = signed, signedBy = signedBy, signedOn = signedOn, text = text };
        foreach (var kv in picks) c.picks[kv.Key] = kv.Value;
        c.order.AddRange(order);
        foreach (var a in added) c.added.Add(a);
        return c;
    }
}

// What the facility's attack found: one line per thing tried, and whether it held.
public class Attack
{
    public readonly List<(string room, string gap, string reached, bool held)> lines = new List<(string, string, string, bool)>();
    public string summary = "";
    public void Through(string room, string gap, string reached) => lines.Add((room, gap, reached, false));
    public void Held(string room, string what) => lines.Add((room, "held", what, true));
    public bool AnyThrough => lines.Any(l => !l.held);
}

public class Recall { public string question; public string[] options; public int answer; public string from; }

// One level: a room, what the student fixes there, the planned mistake, the
// attack the facility runs, and the checks. From Docs/curriculum/tech-escape.md.
public class Room
{
    public int number, tier;
    public string id, title, room, kicker, luma, learn, fixes, live, takeaway, ask;
    public string mistake, why;                 // Fix it: the name and the explanation
    public Item[] items = new Item[0];
    public Field[] fields = new Field[0];
    public bool ordered;                        // the rows are to be put in order
    public bool signable;                       // a dated, signed record
    public bool freeText; public string textPrompt;
    public Dictionary<string, int> ready = new Dictionary<string, int>();   // "item/field" -> option, the setup that works
    public Dictionary<string, int> broken = new Dictionary<string, int>();  // the setup with the mistake in it
    public string[] readyOrder, brokenOrder;
    public Recall recall;
    public Func<RoomState, Session, Attack> attack;
    public Func<RoomState, Session, Attack, List<(Practised result, string label)>> check;
    public bool Capstone => title.StartsWith("Capstone");
    public bool Exit => tier == 3;

    public RoomState Preset(Dictionary<string, int> source, string[] order)
    {
        var s = new RoomState();
        foreach (var kv in source) s.picks[kv.Key] = kv.Value;
        if (kv2(source)) foreach (var k in source.Keys) { var item = k.Split('/')[0]; var it = items.FirstOrDefault(i => i.key == item); if (it != null && it.hidden) s.added.Add(item); }
        s.order.AddRange(order ?? items.Where(i => !i.hidden).Select(i => i.key));
        return s;
    }
    static bool kv2(Dictionary<string, int> d) => d.Count > 0;

    public RoomState Blank()
    {
        var s = new RoomState();
        s.order.AddRange(items.Where(i => !i.hidden).Select(i => i.key));
        return s;
    }

    // Whether one row (an item, or "room" for the one-off decisions) is set right, judged by the
    // room's own attack and checks: in the setup that works, with only this row as the player has
    // it (its picks, its place in the order, whether it is on the board), does anything more get
    // through or fail? So any answer the checks accept is right, not only the one in the setup.
    public bool RowRight(RoomState st, Session x, string key)
    {
        if (attack == null || ready.Count == 0) return true;
        var good = Preset(ready, readyOrder);
        good.signed = st.signed; good.signedBy = st.signedBy; good.signedOn = st.signedOn; good.text = st.text;
        int baseline = Badness(good, x);
        var probe = good.Clone();
        foreach (var f in fields)
        {
            string k = key + "/" + f.key;
            if (st.picks.TryGetValue(k, out int v)) probe.picks[k] = v; else probe.picks.Remove(k);
        }
        var item = items.FirstOrDefault(i => i.key == key);
        if (item != null && item.hidden)
        {
            if (st.added.Contains(key)) { probe.added.Add(key); if (!probe.order.Contains(key)) probe.order.Add(key); }
            else { probe.added.Remove(key); probe.order.Remove(key); }
        }
        if (ordered && probe.order.Contains(key) && st.order.Contains(key))
        {
            probe.order.Remove(key);
            probe.order.Insert(Math.Min(st.order.IndexOf(key), probe.order.Count), key);
        }
        return Badness(probe, x) <= baseline;
    }

    int Badness(RoomState s, Session x)
    {
        try
        {
            var a = attack(s, x);
            int bad = a.lines.Count(l => !l.held);
            if (check != null) bad += check(s, x, a).Count(c => c.result == Practised.Missed);
            return bad;
        }
        catch (Exception) { return 0; }
    }
}

// The whole campaign's memory: each room's last state, and the record (the spine).
public class Session
{
    public readonly Dictionary<string, RoomState> rooms = new Dictionary<string, RoomState>();
    public readonly List<(string room, string gap, string reached)> record = new List<(string, string, string)>();
    public RoomState Of(string id) => rooms.TryGetValue(id, out var s) ? s : null;
    public bool Closed(string id, Func<RoomState, bool> test) { var s = Of(id); return s != null && test(s); }
}
