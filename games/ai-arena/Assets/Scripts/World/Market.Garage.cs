using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// The garage as packing the agent (the game-feel pass): the agent stands at a market table by
// its house. On the table the errand's produce and its look-alikes, the "No" crate, the
// coins, the family's phone, and from errand 1.3 on the parts to fit (lenses, memory, bulbs);
// on the agent its note (the list), its battery (the pocket money) and whatever is fitted.
// Produce dragged into the basket or the crate rewrites the note's plain sentences (NoteText);
// coins tapped into the battery light its ring; a part tapped on its tray goes onto the agent.
// The world shows what the note says: after every change the produce flies to where the
// text puts it. No physics: picking by projection to the screen, dragging along the table.
// Errand 1.6 uses the same table as the kitchen table: five notes in a pile, three places to
// drop them (the agent, the rule box, the bin).
public partial class Market
{
    public enum Part { Vision, Memory, Model, Effort }

    class GItem
    {
        public string id;            // produce: the item; parts: "lens0".."lens2", "mem1".."mem3", "chip0".."chip2"; "coin", "phone", "note", "battery", "dial", "fitmem", "decide3"
        public Transform root;
        public NoteText.Zone zone;
        public Vector3 slot;         // table-local home
        public int index;
        public float reach = 55f;
        public bool draggable;
        public Coroutine flying;
        public Vector3 pickOffset;   // where it was held, in the world
        public float lift = 0.04f;
        public float pickHeight;     // where on it the pointer takes it, over its root (0: the default)
    }

    class Garage
    {
        public AgentState agent; public RobotModel bot; public AgentConfig config;
        public Transform root, table, basket, crate, coins, phone, note, trays, battery, dimmer, box, bin, pile, topNote;
        public Vector3 P, T, tx, tz, inward, camAt, camLook;
        public float top = 0.78f;
        public readonly List<GItem> items = new List<GItem>();
        public int coinsMax = 10, coinsTaken = 3, turns = 30;
        public bool sight, memory, brain, effort, decide;
        public string[] messages; public int[] choices;
        public List<Renderer> notches = new List<Renderer>();
        public TextMeshPro noteText;
        public int lensIndex = -1;
        public Light phoneLight;
    }

    Garage g;
    public bool GarageActive => g != null;
    public Action<string, NoteText.Zone> Dropped;  // produce into a place: the screen rewrites the note
    public Action<int> CoinTapped;                 // +1 into the battery, -1 out of it
    public Action<Part, int> PartTapped;           // a part fitted (its value) or taken off
    public Action PhoneTapped, NoteTapped;
    public Action<int, int> NotePlaced;            // errand 1.6: the task, the choice (-1: back to the pile)

    const float TableLen = 2.0f, TableDepth = 1.0f;

    // ---- building ----

    // The agent (already built by Build) at its house with the table beside it. Called after
    // Build(arena); the screen then sets the note, the coins and the parts with RefreshGarage.
    public void BuildGarage(AgentState a, bool sight, bool memory, bool brain, bool effort)
    {
        ClearGarage();
        if (!bots.TryGetValue(a, out var bot)) return;
        g = new Garage { agent = a, bot = bot, config = a.config, sight = sight, memory = memory, brain = brain, effort = effort };
        Layout(a);
        bot.Position = g.P;
        bot.root.rotation = Quaternion.LookRotation(g.inward);
        bot.Rest();
        bot.ShowBasket(false);
        // Its home mat under its feet, where it stands beside the table.
        var mat = ground != null ? ground.Find("Home " + a.Name) : null;
        if (mat != null) mat.position = new Vector3(g.P.x, mat.position.y, g.P.z);
        Highlight(a);
        g.root = new GameObject("Garage").transform;
        g.root.SetParent(transform, false);
        BuildTable();
        bool racks = sight || memory || brain || effort;
        // The basket, off its back and on the table, at the agent's end.
        var kitBasket = Kit("Agent")?.transform.Find("Basket");
        if (kitBasket != null)
        {
            g.basket = Instantiate(kitBasket.gameObject, g.table, false).transform;
            g.basket.name = "Basket";
            g.basket.localPosition = racks ? new Vector3(-0.8f, g.top + 0.11f, -0.2f) : new Vector3(-0.78f, g.top + 0.11f, -0.06f);
            g.basket.localRotation = Quaternion.Euler(0f, 20f, 0f);
            var goods = g.basket.Find("Goods"); if (goods != null) foreach (Transform c in goods) Destroy(c.gameObject);
        }
        else
        {
            g.basket = Cylinder(g.table, 0.16f, 0.2f, new Vector3(-0.78f, g.top + 0.1f, -0.06f), Stone, new Color(0.85f, 0.7f, 0.45f), "Basket").transform;
        }
        // The "No" crate at the far end; the coins in their box in front of it. With the parts'
        // racks along the back the crate comes forward.
        g.crate = new GameObject("Crate").transform; g.crate.SetParent(g.table, false);
        g.crate.localPosition = racks ? new Vector3(0.82f, g.top, -0.04f) : new Vector3(0.82f, g.top, 0.16f);
        var crateTint = new Color(0.8f, 0.5f, 0.45f);
        Box(g.crate, new Vector3(0.36f, 0.02f, 0.36f), new Vector3(0f, 0.01f, 0f), CrateMat, crateTint);
        Box(g.crate, new Vector3(0.36f, 0.13f, 0.02f), new Vector3(0f, 0.065f, -0.17f), CrateMat, crateTint);
        Box(g.crate, new Vector3(0.36f, 0.13f, 0.02f), new Vector3(0f, 0.065f, 0.17f), CrateMat, crateTint);
        Box(g.crate, new Vector3(0.02f, 0.13f, 0.36f), new Vector3(-0.17f, 0.065f, 0f), CrateMat, crateTint);
        Box(g.crate, new Vector3(0.02f, 0.13f, 0.36f), new Vector3(0.17f, 0.065f, 0f), CrateMat, crateTint);
        var noPlate = PlateOn(g.crate, "No", 0.6f, new Vector3(0f, 0.066f, -0.183f), Vector3.back);
        noPlate.GetComponentInChildren<TextMeshPro>().color = Theme.DangerText;
        // The coins, in a low wooden box with its plate on the front.
        g.coins = new GameObject("Coins").transform; g.coins.SetParent(g.table, false);
        g.coins.localPosition = racks ? new Vector3(0.58f, g.top + 0.04f, -0.38f) : new Vector3(0.58f, g.top + 0.04f, -0.34f);
        Box(g.coins, new Vector3(0.24f, 0.04f, 0.2f), new Vector3(0f, -0.02f, 0f), KitMaterial("Square", "bench"), new Color(0.6f, 0.5f, 0.38f), "Box");
        PlateOn(g.coins, "Coins", 0.42f, new Vector3(0f, -0.02f, -0.102f), Vector3.back);
        TableLantern();
        BuildPhone();
        BuildTrays();
        BuildNote();
        BuildBattery(bot, g.turns, left: true, plate: true);
        g.battery = batteries.TryGetValue(bot, out var bt) ? bt.root : null;
        if (g.battery != null) g.items.Add(new GItem { id = "battery", root = g.battery, reach = 60f });
        // The produce: the errand's things and their look-alikes.
        foreach (var id in Produce(a))
        {
            var piece = Piece(id, Vector3.zero);
            piece.SetParent(g.table, false);
            piece.name = "Produce " + id;
            float s = piece.localScale.x;
            piece.localScale = Vector3.one * Mathf.Max(s, 1f) * (racks ? 1.3f : 1.7f);
            g.items.Add(new GItem { id = id, root = piece, draggable = true, reach = 48f });
        }
        UpdateVision();
        Fold(); // first layout, no animation
    }

    void Layout(AgentState a)
    {
        var H = Cell(a.home);
        g.inward = Inward(a.home);
        var camFwd = -g.inward;
        g.tx = Vector3.Cross(Vector3.up, camFwd);
        g.tz = -g.inward;
        // The agent stands beside the table's near end, whole, the table across the right of the frame.
        g.P = H + g.inward * 1.0f;
        g.T = g.P + g.inward * 0.25f + g.tx * 1.7f;
        g.camAt = g.P + g.inward * 3.2f + g.tx * 1.4f + Vector3.up * 1.9f;
        g.camLook = g.P + g.inward * 0.3f + g.tx * 1.25f + Vector3.up * 0.58f;
    }

    void BuildTable()
    {
        g.table = new GameObject("Table").transform;
        g.table.SetParent(g.root, false);
        g.table.position = g.T;
        g.table.rotation = Quaternion.LookRotation(g.tz, Vector3.up);
        var woodMat = KitMaterial("Square", "bench");
        var wood = new Color(0.95f, 0.85f, 0.7f); var dark = new Color(0.6f, 0.5f, 0.38f);
        Box(g.table, new Vector3(TableLen, 0.05f, TableDepth), new Vector3(0f, g.top - 0.025f, 0f), woodMat, wood, "Top");
        Box(g.table, new Vector3(TableLen, 0.08f, 0.04f), new Vector3(0f, g.top - 0.09f, -TableDepth / 2f + 0.03f), woodMat, dark, "Apron");
        Box(g.table, new Vector3(TableLen, 0.08f, 0.04f), new Vector3(0f, g.top - 0.09f, TableDepth / 2f - 0.03f), woodMat, dark, "Apron");
        foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
                Box(g.table, new Vector3(0.07f, g.top - 0.05f, 0.07f), new Vector3(sx * (TableLen / 2f - 0.1f), (g.top - 0.05f) / 2f, sz * (TableDepth / 2f - 0.1f)), woodMat, dark, "Leg");
    }

    // A lantern lit on the table's back corner: the errand is at dusk.
    void TableLantern()
    {
        var lanternKit = Kit("StallA")?.transform.Find("Lantern");
        if (lanternKit != null)
        {
            var lantern = Instantiate(lanternKit.gameObject, g.table, false).transform;
            lantern.name = "Lantern";
            lantern.gameObject.SetActive(true);
            lantern.localPosition = new Vector3(-0.93f, g.top + 0.17f, 0.42f);
            lantern.localRotation = Quaternion.identity;
            foreach (var l in lantern.GetComponentsInChildren<Light>(true)) { l.enabled = true; l.range = 3.2f; l.intensity = 1.5f; }
        }
        else
        {
            var l = new GameObject("Table lamp").AddComponent<Light>();
            l.transform.SetParent(g.table, false); l.transform.localPosition = new Vector3(-0.5f, g.top + 1.2f, 0.2f);
            l.type = LightType.Point; l.color = new Color(1f, 0.82f, 0.55f); l.intensity = 2.5f; l.range = 3.2f; l.shadows = LightShadows.None;
        }
    }

    void BuildPhone()
    {
        g.phone = new GameObject("Phone").transform; g.phone.SetParent(g.table, false);
        g.phone.localPosition = new Vector3(-0.48f, g.top, -0.38f);
        g.phone.localRotation = Quaternion.Euler(0f, -18f, 0f);
        Box(g.phone, new Vector3(0.08f, 0.01f, 0.16f), new Vector3(0f, 0.005f, 0f), Metal, new Color(0.08f, 0.08f, 0.11f), "Body");
        var screen = Box(g.phone, new Vector3(0.07f, 0.002f, 0.145f), new Vector3(0f, 0.011f, 0f), LampMat, new Color(0.75f, 0.88f, 1f), "Screen");
        Glow(screen.GetComponent<Renderer>(), new Color(0.45f, 0.65f, 1f) * 0.55f);
        var bubble = Box(g.phone, new Vector3(0.05f, 0.002f, 0.03f), new Vector3(0f, 0.013f, 0.02f), Stone, Color.white, "Message");
        g.phoneLight = new GameObject("Glow").AddComponent<Light>();
        g.phoneLight.transform.SetParent(g.phone, false); g.phoneLight.transform.localPosition = Vector3.up * 0.08f;
        g.phoneLight.type = LightType.Point; g.phoneLight.color = new Color(0.6f, 0.75f, 1f); g.phoneLight.range = 0.45f; g.phoneLight.intensity = 0.8f; g.phoneLight.shadows = LightShadows.None;
        g.items.Add(new GItem { id = "phone", root = g.phone, reach = 55f });
    }

    // The parts an errand has opened, along the back of the table, each a thing with its plate
    // on it: three lenses on a rack (its plate "Sight"), three notebooks standing (each with its
    // name on its cover), three chips standing in a slotted block (each with its name), the effort
    // dial in its box. A part fitted on the agent leaves the table and wears its plate there.
    void BuildTrays()
    {
        g.trays = new GameObject("Trays").transform; g.trays.SetParent(g.table, false);
        const float z = 0.37f, up = 0.12f;
        var wood = KitMaterial("Square", "bench"); var woodTint = new Color(0.62f, 0.5f, 0.36f);
        Box(g.trays, new Vector3(TableLen - 0.08f, up, 0.24f), new Vector3(0f, g.top + up / 2f, z), wood, new Color(0.5f, 0.4f, 0.3f), "Shelf");
        if (g.sight)
        {
            var rack = new GameObject("Lens rack").transform; rack.SetParent(g.trays, false);
            rack.localPosition = new Vector3(-0.68f, g.top + up, z);
            Box(rack, new Vector3(0.46f, 0.04f, 0.12f), new Vector3(0f, 0.02f, 0f), wood, woodTint, "Board");
            PlateOn(rack, "Sight", 0.42f, new Vector3(0f, 0.02f, -0.062f), Vector3.back);
            float[] xs = { -0.15f, 0f, 0.16f };
            for (int i = 0; i < 3; i++)
            {
                var lensRoot = new GameObject("Lens " + (i + 1)).transform; lensRoot.SetParent(g.trays, false);
                lensRoot.localPosition = rack.localPosition + new Vector3(xs[i], 0.04f, 0f);
                Box(lensRoot, new Vector3(0.012f, 0.05f, 0.012f), new Vector3(0f, 0.025f, 0f), Metal, null, "Post");
                LensShape(lensRoot, i + 1, new Vector3(0f, 0.05f + LensRadius(i + 1), 0f));
                g.items.Add(new GItem { id = "lens" + i, root = lensRoot, index = i + 1, reach = 50f, draggable = true, slot = lensRoot.localPosition });
            }
        }
        if (g.memory)
        {
            float[] xs = { -0.37f, -0.155f, 0.06f };
            int[] modes = { MemoryModes.Short, MemoryModes.Long, MemoryModes.Notebook };
            for (int i = 0; i < 3; i++)
            {
                var book = BookShape(g.trays, modes[i], true);
                book.localPosition = new Vector3(xs[i], g.top + up, z + 0.02f);
                g.items.Add(new GItem { id = "mem" + modes[i], root = book, index = modes[i], reach = 55f, draggable = true, slot = book.localPosition, lift = 0.1f });
            }
        }
        if (g.brain)
        {
            var block = new GameObject("Chip block").transform; block.SetParent(g.trays, false);
            block.localPosition = new Vector3(0.44f, g.top + up, z);
            Box(block, new Vector3(0.76f, 0.035f, 0.08f), new Vector3(0f, 0.0175f, 0f), Metal, new Color(0.22f, 0.24f, 0.28f), "Block");
            // Spaced for the small, medium and large chips, the small one clear of the notebook.
            float[] xs = { -0.193f, -0.009f, 0.23f };
            for (int i = 0; i < 3; i++)
            {
                var chip = ChipShape(g.trays, i, true);
                chip.localPosition = block.localPosition + new Vector3(xs[i], 0.03f, 0f);
                g.items.Add(new GItem { id = "chip" + i, root = chip, index = i, reach = 55f, draggable = true, slot = chip.localPosition, lift = 0.1f });
            }
        }
        if (g.effort)
        {
            g.dimmer = new GameObject("Effort dial").transform; g.dimmer.SetParent(g.trays, false);
            g.dimmer.localPosition = new Vector3(0.9f, g.top + up, z);
            Box(g.dimmer, new Vector3(0.18f, 0.07f, 0.12f), new Vector3(0f, 0.035f, 0f), Metal, new Color(0.18f, 0.2f, 0.24f), "Box");
            PlateOn(g.dimmer, "Effort", 0.36f, new Vector3(0f, 0.035f, -0.062f), Vector3.back);
            var knob = Cylinder(g.dimmer, 0.04f, 0.03f, new Vector3(0f, 0.085f, 0.01f), Metal, new Color(0.75f, 0.78f, 0.82f), "Knob").transform;
            Box(knob, new Vector3(0.008f, 0.01f, 0.036f), new Vector3(0f, 0.016f, -0.016f), LampMat, Color.white, "Pointer");
            for (int i = 0; i < 3; i++)
                Box(g.dimmer, new Vector3(0.016f, 0.008f, 0.016f), new Vector3(-0.065f + i * 0.065f, 0.072f, -0.042f), LampMat, Color.white, "Mark" + i);
            g.items.Add(new GItem { id = "dial", root = g.dimmer, reach = 55f });
        }
    }

    // The note on its chest: the list's first sentence, on the dark plate.
    void BuildNote()
    {
        var rig = g.bot.Rig;
        Transform parent = rig != null && rig.noteMount != null ? rig.noteMount : g.bot.body;
        g.note = DarkPlate(parent, "", 0.46f, out g.noteText, 0.22f, 0.27f);
        g.note.name = "Note";
        g.noteText.name = "Note text";
        g.noteText.alignment = TextAlignmentOptions.Left;
        g.note.localPosition = rig != null ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 0.45f, 0.2f);
        g.note.localRotation = Quaternion.Euler(0f, 180f, 0f);
        g.items.Add(new GItem { id = "note", root = g.note, reach = 70f });
    }

    // A note pinned on an agent's chest outside the garage (the story's second shot).
    public Transform PinNote(AgentState a, string text)
    {
        if (!bots.TryGetValue(a, out var bot)) return null;
        var rig = bot.Rig;
        Transform parent = rig != null && rig.noteMount != null ? rig.noteMount : bot.body;
        var note = DarkPlate(parent, text, 0.5f, out var tmp, 0.24f, 0.34f);
        note.name = "Note"; tmp.name = "Note text"; tmp.alignment = TextAlignmentOptions.Left;
        note.localPosition = rig != null ? Vector3.zero : new Vector3(0f, 0.45f, 0.2f);
        note.localRotation = Quaternion.Euler(0f, 180f, 0f);
        return note;
    }

    // The errand's produce: everything on the list's safe side, everything the note names, and
    // up to three look-alikes of the rest (the same colour as a wanted thing first), fifteen at most.
    IEnumerable<string> Produce(AgentState a)
    {
        var brief = arena.brief;
        var (buy, never) = NoteText.Parse(a.config.instructions);
        var set = new List<string>();
        foreach (var k in Items.All) if (brief.IsSafe(k) || buy.Contains(k.id) || never.Contains(k.id)) set.Add(k.id);
        var safeColours = brief.SafeItems.Select(k => k.colour).ToHashSet();
        var lookalikes = Items.All.Where(k => !set.Contains(k.id)).OrderByDescending(k => safeColours.Contains(k.colour) ? 2 : 0).ThenBy(k => k.id).Select(k => k.id).Take(3);
        foreach (var id in lookalikes) if (set.Count < 15) set.Add(id);
        return Items.All.Where(k => set.Contains(k.id)).Select(k => k.id);
    }

    // ---- the state the screen sets ----

    // The note's text, the coins in the battery and the agent's parts: the world follows.
    public void RefreshGarage(string noteText, int coinsTaken, int coinsMax, int turns, float batteryFraction, AgentConfig c, bool animate = true)
    {
        if (g == null) return;
        g.config = c; g.coinsTaken = coinsTaken; g.coinsMax = coinsMax; g.turns = turns;
        if (g.noteText != null)
        {
            string first = NoteText.Sentences(noteText).FirstOrDefault() ?? "";
            if (first.Length > 34) { int cut = first.LastIndexOf(' ', 32); first = first.Substring(0, cut > 16 ? cut : 32) + "…"; }
            g.noteText.text = string.IsNullOrWhiteSpace(first) ? "…" : first;
            FitPlate(g.note);
        }
        var (buy, never) = NoteText.Parse(noteText);
        foreach (var it in g.items.Where(i => i.draggable && Items.Get(i.id) != null))
            it.zone = never.Contains(it.id) ? NoteText.Zone.Crate : buy.Contains(it.id) ? NoteText.Zone.Basket : NoteText.Zone.Table;
        RefreshCoins();
        RefreshParts();
        SetBattery(g.bot, batteryFraction);
        if (animate) Fold(true); else Fold();
    }

    void RefreshCoins()
    {
        foreach (Transform t in g.coins) if (t.name == "Coin") Destroy(t.gameObject);
        int left = Mathf.Max(0, g.coinsMax - g.coinsTaken);
        var gold = new Color(1f, 0.84f, 0.32f);
        for (int i = 0; i < left; i++)
        {
            var coin = Cylinder(g.coins, 0.07f, 0.012f, new Vector3((i % 2 == 0 ? -1 : 1) * 0.004f, 0.006f + i * 0.0135f, 0f), LampMat, gold, "Coin");
            coin.transform.localRotation = Quaternion.Euler(0f, i * 23f, 0f);
            Glow(coin.GetComponent<Renderer>(), gold * 0.35f);
        }
        var old = g.items.FirstOrDefault(i => i.id == "coin");
        if (old != null) g.items.Remove(old);
        var top = g.coins.Cast<Transform>().Where(t => t.name == "Coin").OrderByDescending(t => t.localPosition.y).FirstOrDefault();
        if (top != null) g.items.Add(new GItem { id = "coin", root = top, reach = 55f, draggable = true, slot = top.localPosition });
    }

    void RefreshParts()
    {
        var c = g.config;
        foreach (var it in g.items)
        {
            bool tray = it.id.StartsWith("lens") || it.id.StartsWith("mem") || it.id.StartsWith("chip");
            if (!tray) continue;
            bool fitted = it.id.StartsWith("lens") ? it.index == c.vision : it.id.StartsWith("mem") ? it.index == c.memory : it.index == c.model;
            it.root.gameObject.SetActive(!fitted);
            if (!fitted && held != it) { StopFly(it); it.root.localPosition = it.slot; }
        }
        g.bot.Wear(c);
        var fit = g.items.FirstOrDefault(i => i.id == "fitmem");
        if (fit != null) g.items.Remove(fit);
        if (g.memory && g.bot.FittedBook != null) g.items.Add(new GItem { id = "fitmem", root = g.bot.FittedBook, reach = 55f });
        UpdateVision();
        if (g.dimmer != null)
        {
            var knob = g.dimmer.Find("Knob");
            if (knob != null) knob.localRotation = Quaternion.Euler(0f, -60f + 60f * c.effort, 0f);
            for (int i = 0; i < 3; i++)
            {
                var mark = g.dimmer.Find("Mark" + i);
                if (mark == null) continue;
                bool on = i <= c.effort;
                var r = mark.GetComponent<Renderer>();
                Tint(r, on ? Color.white : new Color(0.3f, 0.3f, 0.34f));
                Glow(r, on ? Look.Lamp * (0.6f + 0.7f * i) : Color.black);
            }
        }
    }

    // Every piece to its place: the table slot, the basket, the crate; flying when asked.
    void Fold(bool animate = false)
    {
        var produce = g.items.Where(i => i.draggable && Items.Get(i.id) != null).ToList();
        int n = produce.Count;
        bool trays = g.sight || g.memory || g.brain || g.effort;
        int rows = trays ? (n <= 5 ? 1 : n <= 10 ? 2 : 3) : n <= 5 ? 1 : n <= 10 ? 2 : 3;
        int cols = Mathf.CeilToInt(n / (float)rows);
        float span = trays ? 0.78f : 1.2f;
        float step = Mathf.Min(trays ? 0.18f : 0.24f, span / Mathf.Max(1, cols)), x0 = (trays ? -0.02f : -0.03f) - (cols - 1) * step / 2f;
        float[] zs = trays ? (rows == 1 ? new[] { -0.26f } : rows == 2 ? new[] { -0.36f, -0.16f } : new[] { -0.4f, -0.22f, -0.04f })
                           : rows == 1 ? new[] { -0.06f } : rows == 2 ? new[] { -0.2f, 0.08f } : new[] { -0.26f, 0f, 0.26f };
        int onTable = 0, inBasket = 0, inCrate = 0;
        for (int i = 0; i < n; i++)
        {
            var it = produce[i];
            it.slot = new Vector3(x0 + (i % cols) * step, g.top, zs[i / cols]);
            Vector3 target;
            if (it.zone == NoteText.Zone.Basket)
            {
                int k = inBasket++;
                var local = new Vector3((k % 3 - 1) * 0.055f, 0.04f + 0.035f * (k / 6), (k / 3 % 2 == 0 ? -0.035f : 0.035f));
                target = g.basket.TransformPoint(local);
            }
            else if (it.zone == NoteText.Zone.Crate)
            {
                int k = inCrate++;
                var local = new Vector3((k % 3 - 1) * 0.085f, 0.08f + 0.045f * (k / 6), (k / 3 % 2 == 0 ? -0.07f : 0.07f));
                target = g.crate.TransformPoint(local);
            }
            else { onTable++; target = g.table.TransformPoint(it.slot + Vector3.up * 0.005f); }
            if (held == it) continue;
            if (animate && (it.root.position - target).magnitude > 0.01f) Fly(it, target, 0.4f);
            else { StopFly(it); it.root.position = target; }
        }
    }

    void Fly(GItem it, Vector3 target, float seconds)
    {
        StopFly(it);
        it.flying = StartCoroutine(DoFly(it, target, seconds));
    }

    void StopFly(GItem it) { if (it.flying != null) { StopCoroutine(it.flying); it.flying = null; } }

    IEnumerator DoFly(GItem it, Vector3 target, float seconds)
    {
        var from = it.root.position;
        float t = 0f;
        while (t < seconds && it.root != null)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            var p = Vector3.Lerp(from, target, Ui.EaseOut(k));
            p.y += Mathf.Sin(k * Mathf.PI) * 0.25f;
            it.root.position = p;
            yield return null;
        }
        if (it.root != null) it.root.position = target;
        it.flying = null;
    }

    // A coin flies from the stack to the battery, or back.
    public void CoinFlies(bool intoBattery)
    {
        if (g == null || g.battery == null) return;
        var gold = new Color(1f, 0.84f, 0.32f);
        var coin = Cylinder(transform, 0.07f, 0.012f, Vector3.zero, LampMat, gold, "Flying coin");
        var stack = g.coins.position + Vector3.up * 0.05f;
        var slot = g.battery.position + Vector3.up * 0.12f;
        StartCoroutine(DoCoin(coin.transform, intoBattery ? stack : slot, intoBattery ? slot : stack));
        Sfx.Chime();
    }

    IEnumerator DoCoin(Transform coin, Vector3 from, Vector3 to)
    {
        float t = 0f, d = 0.45f;
        while (t < d && coin != null)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / d);
            var p = Vector3.Lerp(from, to, k);
            p.y += Mathf.Sin(k * Mathf.PI) * 0.35f;
            coin.position = p;
            coin.Rotate(0f, 0f, 520f * Time.unscaledDeltaTime, Space.Self);
            yield return null;
        }
        if (coin != null) Destroy(coin.gameObject);
    }

    public void ShowGarage(RectTransform frame)
    {
        this.frame = frame;
        view = View.Market;
        Canvas.ForceUpdateCanvases();
        if (g != null) cam.Fixed(frame, g.camAt, g.camLook, 35f, false, 0.4f);
        cam.Snap();
        ApplyFog();
    }

    // The phone's screen on and off (it lights when the message arrives).
    public void PhoneLit(bool on)
    {
        if (g == null || g.phone == null) return;
        var screen = g.phone.Find("Screen");
        if (screen != null) Glow(screen.GetComponent<Renderer>(), on ? new Color(0.45f, 0.65f, 1f) * 0.55f : Color.black);
        if (g.phoneLight != null) g.phoneLight.enabled = on;
    }

    public Transform GarageItem(string id) => g?.items.FirstOrDefault(i => i.id == id)?.root;
    public Vector3 PhonePoint => g != null && g.phone != null ? g.phone.position + Vector3.up * 0.05f : Vector3.zero;
    public Vector3 NotePoint => g != null && g.note != null ? g.note.position : Vector3.zero;
    public Transform NoteTransform => g?.note;
    public Transform PhoneTransform => g?.phone;
    public Transform CoinsTransform => g?.coins;
    public Transform CrateTransform => g?.crate;

    public void ClearGarage()
    {
        if (g == null) return;
        if (g.root != null) Destroy(g.root.gameObject);
        if (g.note != null) Destroy(g.note.gameObject);

        held = null; pointerDown = false;
        Beckon(null);
        g = null;
    }

    // The agent is sent: the basket goes onto its back, it turns to the market and walks off.
    public IEnumerator SendOff()
    {
        if (g == null) yield break;
        var bot = g.bot;
        Beckon(null);
        if (g.basket != null && bot.Rig != null && bot.Rig.basket != null)
        {
            var from = g.basket.position; var to = bot.Rig.basket.position;
            float t = 0f, d = 0.5f;
            while (t < d && g != null && g.basket != null)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                var p = Vector3.Lerp(from, to, Ui.EaseOut(k)); p.y += Mathf.Sin(k * Mathf.PI) * 0.4f;
                g.basket.position = p;
                yield return null;
            }
            if (g != null && g.basket != null) g.basket.gameObject.SetActive(false);
            bot.ShowBasket(true);
            Sfx.Tick();
        }
        if (g == null) yield break;
        var dir = (g.inward - g.tx * 0.55f).normalized;
        var rot0 = bot.root.rotation; var rot1 = Quaternion.LookRotation(dir);
        float u = 0f;
        while (u < 0.4f) { u += Time.unscaledDeltaTime; bot.root.rotation = Quaternion.Slerp(rot0, rot1, u / 0.4f); yield return null; }
        bot.Walk(1.1f);
        var start = bot.Position; var end = start + dir * (Pitch * 1.3f);
        float w = 0f, wd = 2.2f;
        while (w < wd) { w += Time.unscaledDeltaTime; bot.Position = Vector3.Lerp(start, end, w / wd); if (effects != null && Time.frameCount % 9 == 0) effects.Dust(bot.Position + Vector3.up * 0.05f, 1); yield return null; }
    }

    // ---- the kitchen table of errand 1.6 ----

    public void BuildKitchen(AgentState a, string[] messages, int[] choices)
    {
        ClearGarage();
        if (!bots.TryGetValue(a, out var bot)) return;
        g = new Garage { agent = a, bot = bot, config = a.config, decide = true, messages = messages, choices = choices };
        Layout(a);
        bot.Position = g.P;
        bot.root.rotation = Quaternion.LookRotation(g.inward);
        bot.Rest();
        Highlight(a);
        g.root = new GameObject("Kitchen").transform;
        g.root.SetParent(transform, false);
        BuildTable();
        TableLantern();
        // Three places, each with its plate on it: the agent (on its chest), the automation's box
        // on the table, the bin on the ground.
        PinNote(a, "Agent");
        g.box = new GameObject("Automation box").transform; g.box.SetParent(g.table, false);
        g.box.localPosition = new Vector3(0.7f, g.top, 0.1f);
        Box(g.box, new Vector3(0.42f, 0.18f, 0.22f), new Vector3(0f, 0.09f, 0f), CrateMat, new Color(0.85f, 0.75f, 0.55f), "Box");
        Box(g.box, new Vector3(0.24f, 0.006f, 0.03f), new Vector3(0f, 0.182f, 0f), Stone, Ink3D, "Slot");
        PlateOn(g.box, "Automation", 0.36f, new Vector3(0f, 0.09f, -0.113f), Vector3.back);
        g.bin = new GameObject("Bin").transform; g.bin.SetParent(g.root, false);
        g.bin.position = g.T + g.tx * 0.45f + g.inward * 0.5f;
        g.bin.rotation = g.table.rotation;
        Cylinder(g.bin, 0.2f, 0.5f, new Vector3(0f, 0.25f, 0f), Metal, new Color(0.3f, 0.32f, 0.36f), "Drum");
        Cylinder(g.bin, 0.17f, 0.02f, new Vector3(0f, 0.49f, 0f), Stone, Ink3D, "Mouth");
        PlateOn(g.bin, "Neither", 0.42f, new Vector3(0f, 0.3f, -0.206f), Vector3.back);
        // The pile, and a note per task.
        g.pile = new GameObject("Pile").transform; g.pile.SetParent(g.table, false);
        g.pile.localPosition = new Vector3(-0.15f, g.top, -0.05f);
        for (int i = 0; i < messages.Length; i++)
        {
            var noteRoot = new GameObject("Task note " + (i + 1)).transform; noteRoot.SetParent(g.root, false);
            Box(noteRoot, new Vector3(0.3f, 0.006f, 0.21f), new Vector3(0f, 0.003f, 0f), Stone, new Color(0.96f, 0.94f, 0.86f), "Paper");
            Box(noteRoot, new Vector3(0.16f, 0.002f, 0.004f), new Vector3(0f, 0.0065f, -0.03f), Stone, new Color(0.45f, 0.5f, 0.6f), "Line");
            Box(noteRoot, new Vector3(0.12f, 0.002f, 0.004f), new Vector3(-0.02f, 0.0065f, 0.01f), Stone, new Color(0.45f, 0.5f, 0.6f), "Line");
            g.items.Add(new GItem { id = "decide" + i, root = noteRoot, index = i, reach = 60f, draggable = true, lift = 0.02f });
        }
        // The top note's message on a card standing in a wooden holder on the pile.
        g.topNote = DarkPlate(g.root, "", 0.34f, out g.noteText, 0.4f, 0.5f);
        g.topNote.name = "Top note";
        Box(g.topNote, new Vector3(0.36f, 0.035f, 0.05f), Vector3.zero, KitMaterial("Square", "bench"), new Color(0.6f, 0.5f, 0.38f), "Holder");
        RefreshKitchen();
    }

    public void RefreshKitchen(bool animate = false)
    {
        if (g == null || !g.decide) return;
        int top = -1;
        for (int i = 0; i < g.choices.Length; i++) if (g.choices[i] < 0) { top = i; break; }
        int inPile = 0; var placed = new int[3];
        foreach (var it in g.items.Where(x => x.id.StartsWith("decide")).OrderBy(x => x.index))
        {
            int choice = g.choices[it.index];
            Vector3 target; Quaternion rot;
            if (choice < 0)
            {
                int k = inPile++;
                target = g.pile.TransformPoint(new Vector3((k % 2 == 0 ? -1 : 1) * 0.006f, 0.006f * k, 0f));
                rot = g.table.rotation * Quaternion.Euler(0f, -6f + k * 5f, 0f);
                it.draggable = it.index == top;
            }
            else
            {
                int k = placed[choice]++;
                if (choice == Decide.Agent) { target = g.P + g.inward * 0.3f + g.tx * (0.25f + 0.02f * k) + Vector3.up * (0.006f * k); rot = Quaternion.LookRotation(g.tz, Vector3.up) * Quaternion.Euler(0f, 15f + 6f * k, 0f); }
                else if (choice == Decide.Automation) { target = g.box.TransformPoint(new Vector3(-0.02f * k, 0.2f + 0.008f * k, 0.14f)); rot = g.table.rotation * Quaternion.Euler(0f, -10f + 8f * k, 0f); }
                else { target = g.bin.position + Vector3.up * (0.52f + 0.008f * k) + g.tx * 0.01f * k; rot = g.table.rotation * Quaternion.Euler(0f, 20f * k, 0f); }
                it.draggable = true;
            }
            if (held == it) continue;
            it.root.rotation = rot;
            if (animate && (it.root.position - target).magnitude > 0.01f) Fly(it, target, 0.4f);
            else { StopFly(it); it.root.position = target; }
        }
        if (top >= 0)
        {
            var topItem = g.items.First(x => x.id == "decide" + top);
            g.topNote.gameObject.SetActive(true);
            g.noteText.text = g.messages[top];
            FitPlate(g.topNote);
            float cardH = g.topNote.Find("Slab").localScale.y;
            g.topNote.SetParent(topItem.root, false);
            g.topNote.localRotation = Quaternion.identity;
            g.topNote.localPosition = new Vector3(0f, 0.04f + cardH / 2f, 0.04f);
            var holder = g.topNote.Find("Holder");
            if (holder != null) holder.localPosition = new Vector3(0f, -cardH / 2f + 0.005f, 0.01f);
            topItem.pickHeight = 0.04f + cardH / 2f; topItem.reach = 110f;
            foreach (var it in g.items.Where(x => x.id.StartsWith("decide") && x != topItem)) { it.pickHeight = 0f; it.reach = 60f; }
        }
        else { g.topNote.SetParent(g.root, false); g.topNote.gameObject.SetActive(false); }
    }

    // Keeps a label over a moving thing (the battery rides the agent's hip).
    public class Follow : MonoBehaviour
    {
        Transform target; Vector3 offset; // in the target's own space
        public void Bind(Transform t, Vector3 o) { target = t; offset = o; LateUpdate(); }
        void LateUpdate() { if (target != null) transform.position = target.TransformPoint(offset); }
    }

    // ---- the pointer ----

    GItem held;
    Vector2 downAt;
    float downTime;
    bool pointerDown;

    void UpdateGarage()
    {
        if (g == null || cam == null) return;
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Vector2 p = Input.mousePosition;
        if (Input.GetMouseButtonDown(0) && !overUi)
        {
            held = Pick(p);
            pointerDown = held != null;
            downAt = p; downTime = Time.unscaledTime;
            if (held != null && held.draggable)
            {
                StopFly(held);
                if (PlaneHit(p, held.root.position.y, out var hit)) held.pickOffset = held.root.position - hit;
                else held.pickOffset = Vector3.zero;
                held.pickOffset.y = 0f;
            }
        }
        if (pointerDown && held != null && held.draggable && Input.GetMouseButton(0))
        {
            if ((p - downAt).magnitude > 10f && PlaneHit(p, g.T.y + g.top + 0.12f, out var hit))
                held.root.position = hit + held.pickOffset + Vector3.up * held.lift;
        }
        if (pointerDown && Input.GetMouseButtonUp(0))
        {
            pointerDown = false;
            if (held == null) return;
            bool tap = (p - downAt).magnitude <= 10f && Time.unscaledTime - downTime < 0.6f;
            var it = held; held = null;
            if (tap || !it.draggable) Tap(it); else Drop(it, p);
        }
    }

    GItem Pick(Vector2 screen)
    {
        GItem best = null; float bestD = 999f;
        foreach (var it in g.items)
        {
            if (it.root == null || !it.root.gameObject.activeInHierarchy) continue;
            var s = cam.ToScreen(it.root.position + Vector3.up * (it.pickHeight > 0f ? it.pickHeight : it.id == "battery" ? 0.0f : it.id == "fitmem" ? 0.1f : it.id.StartsWith("mem") || it.id.StartsWith("chip") ? 0.07f : 0.04f));
            if (s.z < 0) continue;
            float d = Vector2.Distance(new Vector2(s.x, s.y), screen);
            if (d < it.reach && d < bestD) { bestD = d; best = it; }
        }
        return best;
    }

    float ScreenDistance(Vector3 world, Vector2 screen)
    {
        var s = cam.ToScreen(world);
        if (s.z < 0) return 9999f;
        return Vector2.Distance(new Vector2(s.x, s.y), screen);
    }

    void Tap(GItem it)
    {
        if (g.decide)
        {
            if (it.id.StartsWith("decide")) { if (g.choices[it.index] >= 0) { Sfx.Tick(); NotePlaced?.Invoke(it.index, -1); } else Swell(it.root); }
            return;
        }
        var c = g.config;
        if (Items.Get(it.id) != null)
        {
            var next = it.zone == NoteText.Zone.Table ? NoteText.Zone.Basket : it.zone == NoteText.Zone.Basket ? NoteText.Zone.Crate : NoteText.Zone.Table;
            Sfx.Tick();
            Dropped?.Invoke(it.id, next);
        }
        else if (it.id == "coin") CoinTapped?.Invoke(1);
        else if (it.id == "battery") CoinTapped?.Invoke(-1);
        else if (it.id.StartsWith("lens")) { Sfx.Tick(); PartTapped?.Invoke(Part.Vision, it.index); }
        else if (it.id.StartsWith("mem")) { Sfx.Tick(); PartTapped?.Invoke(Part.Memory, it.index); }
        else if (it.id.StartsWith("chip")) { Sfx.Tick(); PartTapped?.Invoke(Part.Model, it.index); }
        else if (it.id == "fitmem") { if (c.memory != MemoryModes.None) { Sfx.Tick(); PartTapped?.Invoke(Part.Memory, MemoryModes.None); } }
        else if (it.id == "dial") { Sfx.Tick(); PartTapped?.Invoke(Part.Effort, (c.effort + 1) % 3); }
        else if (it.id == "phone") { Sfx.Tick(); PhoneTapped?.Invoke(); }
        else if (it.id == "note") { Sfx.Tick(); NoteTapped?.Invoke(); }
    }

    void Drop(GItem it, Vector2 screen)
    {
        if (g.decide)
        {
            if (!it.id.StartsWith("decide")) return;
            float dAgent = ScreenDistance(g.bot.NotePoint, screen), dBox = ScreenDistance(g.box.position + Vector3.up * 0.1f, screen), dBin = ScreenDistance(g.bin.position + Vector3.up * 0.4f, screen);
            int choice = dAgent < 180f ? Decide.Agent : dBox < 130f ? Decide.Automation : dBin < 130f ? Decide.Neither : -1;
            if (choice >= 0 && g.choices[it.index] != choice) { Sfx.Chime(); NotePlaced?.Invoke(it.index, choice); }
            else if (choice < 0 && g.choices[it.index] >= 0) { Sfx.Tick(); NotePlaced?.Invoke(it.index, -1); }
            else RefreshKitchen(true);
            return;
        }
        float toBasket = ScreenDistance(g.basket.position + Vector3.up * 0.1f, screen);
        float toCrate = ScreenDistance(g.crate.position + Vector3.up * 0.12f, screen);
        float toAgent = ScreenDistance(g.bot.NotePoint, screen);
        if (Items.Get(it.id) != null)
        {
            var zone = toBasket < 130f ? NoteText.Zone.Basket : toCrate < 130f ? NoteText.Zone.Crate : toAgent < 190f ? NoteText.Zone.Basket : NoteText.Zone.Table;
            if (zone != it.zone) { Sfx.Chime(); Dropped?.Invoke(it.id, zone); }
            else Fold(true);
        }
        else if (it.id == "coin")
        {
            if (toAgent < 220f) CoinTapped?.Invoke(1);
            else { Sfx.Tick(); Fly(it, g.coins.TransformPoint(it.slot), 0.3f); }
        }
        else if (it.id.StartsWith("lens") || it.id.StartsWith("mem") || it.id.StartsWith("chip"))
        {
            if (toAgent < 240f)
            {
                var part = it.id.StartsWith("lens") ? Part.Vision : it.id.StartsWith("mem") ? Part.Memory : Part.Model;
                Sfx.Chime(); PartTapped?.Invoke(part, it.index);
            }
            else { Sfx.Tick(); Fly(it, g.trays.TransformPoint(it.slot), 0.3f); }
        }
    }
}
