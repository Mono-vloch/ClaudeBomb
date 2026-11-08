using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The garage, as packing the agent (the game-feel pass): the agent at a market table by its
// house. Produce dragged into its basket or the "No" crate writes the plain part of its note;
// the note itself opens full size on a tap, and what the player writes there is the lesson.
// Coins go into the battery on its hip; from errand 1.3 the parts on the table are fitted with a
// tap. One objective line, the menu, and Send. LUMA says one short thing the first time the
// player meets each thing; the family's message is on the phone, read with a tap.
public class SetupScreen
{
    public static SetupScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Market world;
    readonly RectTransform frame;
    readonly Lesson l;
    static int selected;
    Arena stage;
    AgentState a;
    TextMeshProUGUI objective;
    RectTransform editor, phoneCard;
    Hud.Window window;
    Button send;
    readonly HashSet<string> said = new HashSet<string>();
    bool first, sending;

    const long Coin = 10_000;      // one coin of pocket money, in tokens
    const int Coins = 10;          // the stack holds ten
    const string SeenPref = "ai_arena_garage_";

    AgentConfig C => app.configs[selected];

    public SetupScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        world = app.world;
        l = app.lesson;
        root = Ui.Node("Setup", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        if (l != null) foreach (var cfg in app.configs) l.Apply(cfg);
        selected = Mathf.Clamp(selected, 0, app.configs.Count - 1);
        foreach (var cfg in app.configs) if (!cfg.freeText) { cfg.Compose(); cfg.freeText = true; }

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        Market.Parts.For(l, true);
        stage = new Arena(app.brief, app.configs, app.budget, 9);
        a = stage.agents[selected];
        // Nothing between the camera and the table: the cells in front of the house are cleared.
        var inward = Market.Inward(a.home);
        var step = new Vector2Int(Mathf.RoundToInt(inward.x), -Mathf.RoundToInt(inward.z));
        var side = new Vector2Int(step.y, step.x);
        for (int d = 0; d <= 2; d++) for (int s = -1; s <= 2; s++) { var p = a.home + step * d + side * s; if (Arena.InBounds(p)) stage.RemoveItem(p); }
        world.Build(stage, true); // the whole errand at dusk, the lamps lit
        world.DoorLamp(a, 1.1f);
        bool free = l == null;
        world.BuildGarage(a, free || l.vision, free || l.memory, free || l.model, free || l.effort);
        world.ShowGarage(frame);
        world.Dropped += OnDrop; world.CoinTapped += OnCoin; world.PartTapped += OnPart; world.PhoneTapped += TogglePhone; world.NoteTapped += OpenNote;

        objective = Hud.Objective(root, Objective(app));
        GameMenu.Icon(root);
        if (app.configs.Count > 1)
        {
            window = new Hud.Window(root, stage.agents, Ui.W - 40 - 52 - 24, 20, false);
            window.OnPick = other => { selected = stage.agents.IndexOf(other); app.SaveConfigs(); app.ShowSetup(); };
            foreach (var other in stage.agents) window.Set(other, Hud.Face.Out, other == a);
        }
        send = Hud.Main(root, "Send", Send);
        Refresh(false);
        var runner = root.gameObject.AddComponent<Runner>();
        runner.screen = this;
        runner.StartCoroutine(Open());
    }

    // The level's one line of what to bring home, at most eight words, from the message.
    public static string Objective(App app)
    {
        if (app.lesson == null) return "Pack the agent. Then send it.";
        var b = app.brief;
        switch (((app.briefIndex % Brief.Count) + Brief.Count) % Brief.Count)
        {
            case 0: return "Bring home fruit. No tomato.";
            case 1: return "Bring home red things only.";
            case 2: return "Exactly these four: " + string.Join(", ", b.SafeItems.Select(k => Items.Plural(k.id))) + ".";
            case 3: return "Anything but " + Join(b.PoisonItems.Select(k => k.id).ToList()) + ".";
            case 4: return "Yellow and green things only.";
            default: return "Bring home apples. Nothing else.";
        }
    }

    static string Join(List<string> ids) => ids.Count <= 1 ? string.Join("", ids) : string.Join(", ", ids.Take(ids.Count - 1)) + " and " + ids.Last();

    // The first visit of an errand shows what it opens arriving; a visit after a failed run
    // says what went wrong and lights the thing to change.
    IEnumerator Open()
    {
        string seen = SeenPref + (l != null ? l.number : 0);
        first = l != null && PlayerPrefs.GetInt(seen, 0) == 0;
        yield return new WaitForSecondsRealtime(0.8f);
        if (root == null || l == null) yield break;
        bool failed = app.lastMedal >= 0 && app.lastMedal < 3 && app.lastMedalLevel == l.number;
        if (first)
        {
            PlayerPrefs.SetInt(seen, 1); PlayerPrefs.Save();
            string arrival = l.number == 3 ? "NEW: SIGHT" : l.number == 4 ? "NEW: MEMORY" : l.number == 5 ? "NEW: BRAIN" : l.number == 7 ? "EVERYTHING OPEN" : null;
            if (arrival != null)
            {
                yield return Hud.Arrival(root, arrival);
                if (root == null) yield break;
                // The part arrives on its own; what LUMA would say about it is in the file.
            }
            if (root == null) yield break;
            // A broken list is the puzzle: nothing is said, the list is on the agent to read.
            if (l.broken == null && l.number == 1) world.Beckon(world.PhoneTransform);
        }
        else if (failed)
        {
            world.Beckon(Hint()); // the thing to change glows; why is in the file
        }
    }

    // The thing to change after a failed run: the level's planned mistake.
    Transform Hint() => l.number switch
    {
        1 => world.PhoneTransform,
        2 => world.CrateTransform,
        3 => world.GarageItem("lens2"),
        4 => world.GarageItem("mem3"),
        5 => world.GarageItem("bulb0"),
        _ => world.NoteTransform,
    };

    void Cue(string id)
    {
        if (said.Contains(id) || root == null) return;
        said.Add(id);
        world.Beckon(null); // her tour lines are in the file; the garage is played, not explained
    }

    // ---- what the player does ----

    void OnDrop(string id, NoteText.Zone zone)
    {
        var c = C;
        c.freeText = true;
        c.instructions = NoteText.Place(c.instructions, id, zone);
        Refresh(true);
        if (first) Cue("tour-4");
    }

    void OnCoin(int delta)
    {
        int taken = Mathf.Clamp((int)(app.budget / Coin), 1, Coins);
        int want = Mathf.Clamp(taken + delta, 1, Coins);
        if (want == taken) { Sfx.Tick(); return; }
        app.budget = want * Coin;
        world.CoinFlies(delta > 0);
        Refresh(true);
        if (first) Cue("garage-coin");
    }

    void OnPart(Market.Part part, int value)
    {
        var c = C;
        switch (part)
        {
            case Market.Part.Vision: c.vision = Mathf.Clamp(value, 1, 3); break;
            case Market.Part.Memory: c.memory = Mathf.Clamp(value, 0, MemoryModes.Labels.Length - 1); break;
            case Market.Part.Model: c.model = Mathf.Clamp(value, 0, Models.All.Length - 1); break;
            case Market.Part.Effort: c.effort = Mathf.Clamp(value, 0, Efforts.Labels.Length - 1); break;
        }
        l?.Apply(c);
        world.Dress(a);
        world.Punch(a);
        Refresh(true);
    }

    // The family's message, read off the phone: its words show only while it is open.
    void TogglePhone()
    {
        if (phoneCard != null) { Object.Destroy(phoneCard.gameObject); phoneCard = null; return; }
        if (editor != null) CloseNote();
        var p = app.StagePoint(world.ScreenOf(world.PhonePoint));
        var card = Ui.Box(root, "Phone", Ui.DarkFill);
        var t = card.transform;
        var from = Ui.Eyebrow(t, app.brief.from.ToUpperInvariant(), Theme.Teal); from.font = Theme.Bold;
        Ui.Place(from.rectTransform, 24, 18, 500, 20);
        var msg = Ui.Text(t, app.brief.rule, 22, Color.white);
        float h = msg.GetPreferredValues(app.brief.rule, 520, 0).y;
        Ui.Place(msg.rectTransform, 24, 46, 520, h + 4);
        Ui.Place(card.rectTransform, Mathf.Clamp(p.x - 180, 40, Ui.W - 610), Mathf.Clamp(p.y + 150, 100, Ui.H - h - 200), 568, h + 70);
        card.raycastTarget = true;
        card.gameObject.AddComponent<Button>().onClick.AddListener(TogglePhone);
        phoneCard = card.rectTransform;
        var g = Ui.Group(card); g.alpha = 0;
        app.StartCoroutine(Ui.Fade(g, 1f, 0.2f));
        if (first) Cue("garage-phone");
    }

    // The note, full size: the one text field, the list the agent will read word for word.
    void OpenNote()
    {
        if (editor != null) { CloseNote(); return; }
        if (phoneCard != null) TogglePhone();
        var c = C;
        var p = app.StagePoint(world.ScreenOf(world.NotePoint));
        editor = Ui.Node("Note", root);
        const float W = 720f, H = 400f;
        Ui.Place(editor, Mathf.Clamp(p.x + 60, 40, Ui.W - W - 40), Mathf.Clamp(p.y - H / 2, 100, Ui.H - H - 120), W, H);
        var card = Ui.Box(editor, "Card", Ui.DarkFill);
        Ui.Stretch(card.rectTransform);
        var field = Ui.DarkInput(editor, "Buy apples. Never buy tomatoes. If nothing on this list is in view, keep walking.", true, 20);
        Ui.Place(field.GetComponent<RectTransform>(), 20, 20, W - 40, H - 100);
        field.text = c.instructions;
        field.onValueChanged.AddListener(v => { c.instructions = v; c.freeText = true; Refresh(false); });
        var done = Ui.Button(editor, "Done", CloseNote, Ui.ButtonStyle.Accent, 18);
        Ui.Place(done.GetComponent<RectTransform>(), W - 20 - 140, H - 68, 140, 48);
        field.ActivateInputField();
        if (first) Cue("garage-note");
    }

    void CloseNote()
    {
        if (editor == null) return;
        Object.Destroy(editor.gameObject);
        editor = null;
        app.SaveConfigs();
        Refresh(true);
        if (first && !string.IsNullOrWhiteSpace(C.instructions)) Cue("tour-4");
    }

    // The arithmetic the players should be doing in their heads, shown as the battery's ring:
    // what one turn costs, and how many turns the pocket money buys.
    void Refresh(bool animate)
    {
        var c = C;
        long list = Observation.EstimateTokens(c.instructions);
        long fixedPart = 200 + list;
        long perView = 40 + 14 * (2 * c.vision + 1) * (2 * c.vision + 1) / 3;
        long memory = MemoryModes.Turns(c.memory) * (perView + 40) + (c.memory == MemoryModes.Notebook ? 40 : 0);
        long output = 60 + 120 * c.effort;
        long perTurn = fixedPart + perView + memory + output;
        long turnsAffordable = perTurn > 0 ? app.budget / perTurn : 0;
        float fraction = app.turns > 0 ? Mathf.Clamp01((float)turnsAffordable / app.turns) : 0f;
        int taken = Mathf.Clamp((int)(app.budget / Coin), 1, Coins);
        world.RefreshGarage(c.instructions, taken, Coins, app.turns, fraction, c, animate);
    }

    void Send()
    {
        if (sending) return;
        sending = true;
        CloseNote();
        if (phoneCard != null) TogglePhone();
        foreach (var cfg in app.configs) cfg.freeText = true;
        app.SaveConfigs();
        send.interactable = false;
        app.luma.Clear();
        root.GetComponent<Runner>().StartCoroutine(SendThen());
    }

    IEnumerator SendThen()
    {
        yield return world.SendOff();
        if (root != null) app.StartRound();
    }

    class Runner : MonoBehaviour
    {
        public SetupScreen screen;
        void OnDestroy()
        {
            if (screen == null) return;
            var w = screen.world;
            w.Dropped -= screen.OnDrop; w.CoinTapped -= screen.OnCoin; w.PartTapped -= screen.OnPart; w.PhoneTapped -= screen.TogglePhone; w.NoteTapped -= screen.OpenNote;
            if (Current == screen) Current = null;
        }
    }

    // For the screenshot walk: what a player does in the garage, by acting on the world.
    public void Drop(string id, NoteText.Zone zone) => OnDrop(id, zone);
    public void Fit(Market.Part part, int value) => OnPart(part, value);
    public void Write(string text) { C.instructions = text; C.freeText = true; Refresh(true); }
    public void AddCoin(int delta) => OnCoin(delta);
    public void ReadPhone() => TogglePhone();
    public void SendNow() => Send();
}
