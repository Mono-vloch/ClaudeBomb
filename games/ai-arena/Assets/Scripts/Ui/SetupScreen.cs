using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The garage: where the agent is fitted out before it leaves. The agent stands in
// the market on the left, big, and its four dials hang off it as tags: tap the
// antenna to switch brains (the ball swaps and the agent pops), the eyes for how far
// it sees, its back for what it remembers, the antenna's glow for effort; a dial an
// errand has not opened wears a lock. On the right the one thing the player writes:
// the list the agent will read, word for word, and under it the pocket money as a
// wallet of coins. The agents run by themselves; this is the last word before they do.
public class SetupScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Market world;
    readonly RectTransform strip, editor, frame, tags;
    readonly TextMeshProUGUI briefTitle, briefRule, briefNote;
    readonly Image message;
    readonly Button send;
    RectTransform coachLine;
    int selected;
    Arena stage; // the market behind, with these agents on it

    const long Coin = 10_000;      // one coin of pocket money, in tokens
    const int Coins = 10;          // the wallet holds up to ten
    static readonly Vector2Int Spot = new Vector2Int(4, 5); // where the chosen agent stands for its picture: the middle, the camera south of it

    public SetupScreen(Transform parent, App app)
    {
        this.app = app;
        world = app.world;
        root = Ui.Node("Setup", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear); // the market shows through
        var lesson = app.lesson;
        if (lesson != null) foreach (var cfg in app.configs) lesson.Apply(cfg);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 60, 290, 700, 700);
        selected = Mathf.Clamp(selected, 0, app.configs.Count - 1);

        // Frosted glass along the top: the errand, the way back, the file, the settings, deploy.
        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.86f), false);
        Ui.Place(top.rectTransform, -400, -300, 2800, 436);
        top.raycastTarget = false;
        var eyebrow = Ui.Eyebrow(root, lesson != null ? $"ERRAND {lesson.id}  ·  THE GARAGE" : "FREE PLAY  ·  THE GARAGE", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 60, 24, 600, 20);
        var title = Ui.Text(root, lesson != null ? lesson.title : "Fit out the agent", 34, Theme.Ink, true);
        Ui.Place(title.rectTransform, 60, 48, 1000, 50);
        title.enableWordWrapping = false;
        float bx = 1140;
        if (lesson != null)
        {
            var back = Ui.Button(root, "Briefing", app.ShowLesson, Ui.ButtonStyle.Quiet, 15);
            Ui.Place(back.GetComponent<RectTransform>(), bx, 44, 100, 42); bx += 104;
            var tour = Ui.Button(root, "Tour", () => { if (touring == null) touring = app.StartCoroutine(RunTour()); }, Ui.ButtonStyle.Quiet, 15);
            Ui.Place(tour.GetComponent<RectTransform>(), bx, 44, 70, 42); bx += 74;
        }
        var file = Ui.Button(root, "The file", () => app.ShowFile(), Ui.ButtonStyle.Quiet, 15);
        Ui.Place(file.GetComponent<RectTransform>(), bx, 44, 90, 42); bx += 94;
        var settings = Ui.Button(root, "Settings", () => SettingsSheet.Toggle(app, root, Reload), Ui.ButtonStyle.Quiet, 15);
        Ui.Place(settings.GetComponent<RectTransform>(), bx, 44, 100, 42);
        send = Ui.Button(root, app.configs.Count > 1 ? "DEPLOY AGENTS" : "DEPLOY THE AGENT", () => { foreach (var c in app.configs) c.Compose(); app.SaveConfigs(); app.StartRound(); }, Ui.ButtonStyle.Accent, 20);
        Ui.Place(send.GetComponent<RectTransform>(), 1580, 32, 280, 64);

        // The mission: the family's message
        message = Ui.Soft(root, "Message");
        Ui.Place(message.rectTransform, 60, 152, 1800, 108);
        var tag = Ui.Eyebrow(message.transform, "", Theme.TealDeep);
        Ui.Place(tag.rectTransform, 24, 12, 900, 22);
        briefTitle = tag;
        briefRule = Ui.Text(message.transform, "", 22, Theme.Ink);
        Ui.Place(briefRule.rectTransform, 24, 36, 1500, 36);
        briefNote = Ui.Text(message.transform, "", 14, Theme.Text2);
        Ui.Place(briefNote.rectTransform, 24, 72, 1500, 26);
        if (lesson == null)
        {
            var newBrief = Ui.Button(message.transform, "New message", () => { app.NewBrief(); ShowBrief(); BuildWorld(); }, Ui.ButtonStyle.Secondary, 14);
            Ui.Place(newBrief.GetComponent<RectTransform>(), 1616, 32, 160, 40);
        }
        ShowBrief();

        // The dials, on the agent.
        tags = Ui.Node("Tags", root);
        Ui.Stretch(tags);

        // The agents, one chip each, only when there is more than one (a class).
        strip = Ui.Node("Strip", root);
        Ui.Place(strip, 60, 1000, 700, 60);

        editor = Ui.Node("Editor", root);
        Ui.Place(editor, 780, 290, 1080, 770);

        BuildWorld();
        RefreshStrip();
        BuildEditor();
        BuildTags();
        Coach();
        var follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);
        Ui.Emblem(root);

        if (lesson != null && lesson.number == 1 && !Tour.Seen) touring = app.StartCoroutine(RunTour());
    }

    void ShowBrief()
    {
        briefTitle.text = $"A MESSAGE FROM {app.brief.from.ToUpperInvariant()}";
        briefRule.text = app.brief.rule;
        briefNote.text = "The catch: " + app.brief.note;
    }

    // The level opened with a list already written: LUMA says why it works, or what is
    // wrong with it, once, over the agent; the words stay in the file.
    void Coach()
    {
        if (coachLine != null) Object.Destroy(coachLine.gameObject);
        coachLine = null;
        var start = app.StartingSetup;
        if (start == null) return;
        bool broken = app.lesson.broken != null;
        coachLine = app.luma.Line(root, 60, 880, 700, (broken ? "Fix it: " + start.name + ". " : "") + start.why, true, false, 15);
    }

    // The settings closed: the roster may have changed.
    void Reload()
    {
        selected = Mathf.Clamp(selected, 0, app.configs.Count - 1);
        Ui.SetLabel(send, app.configs.Count > 1 ? "DEPLOY AGENTS" : "DEPLOY THE AGENT");
        BuildWorld();
        RefreshStrip();
        BuildEditor();
        BuildTags();
    }

    // ---- the market behind ------------------------------------------------------

    void BuildWorld()
    {
        stage = new Arena(app.brief, app.configs, app.budget, 9);
        // Nothing between the camera and the agent: the cells in front of it are cleared.
        for (int x = Spot.x - 1; x <= Spot.x + 2; x++)
            for (int y = Spot.y; y <= Arena.Size - 2; y++)
                stage.RemoveItem(new Vector2Int(x, y));
        Place();
        world.Build(stage, false);
        Show();
    }

    // The chosen agent stands up front, facing us; the others wait at their houses.
    void Place()
    {
        for (int i = 0; i < stage.agents.Count; i++)
        {
            var a = stage.agents[i];
            a.pos = i == selected ? Spot : a.home;
            a.facing = i == selected ? 2 : a.home.y == 0 ? 2 : a.home.x == Arena.Size - 1 ? 3 : a.home.y == Arena.Size - 1 ? 0 : 1;
        }
    }

    AgentState Chosen => stage.agents[selected];

    void Show()
    {
        world.RefreshBots(true);
        world.ShowPortrait(frame, Chosen);
    }

    // ---- the tags on the agent ----------------------------------------------------

    class Hook { public RectTransform rect; public Image dot; public int part; public Vector2 offset; }
    readonly List<Hook> tagList = new List<Hook>();

    // Four tags, one per dial, each hanging off the part of the agent it is: tap to turn
    // it (the agent pops), or a lock with the errand that opens it.
    void BuildTags()
    {
        Ui.Clear(tags);
        tagList.Clear();
        var c = Current;
        var l = app.lesson;
        bool free = l == null;
        // Beside the parts, never over them: the brain and the effort to the right of the
        // antenna, the sight and the memory to the left of the eyes and the back.
        Tag(0, new Vector2(70, -74), "BRAIN", Models.All[c.model].label, free || l.model, Ladder.ModelLevel, () => { c.model = (c.model + 1) % Models.All.Length; Dressed(); });
        Tag(0, new Vector2(70, -10), "EFFORT", Efforts.Labels[c.effort], free || l.effort, Ladder.EffortLevel, () => { c.effort = (c.effort + 1) % Efforts.Labels.Length; Dressed(); });
        Tag(1, new Vector2(-290, -28), "SIGHT", $"{c.vision} cell{(c.vision > 1 ? "s" : "")}", free || l.vision, Ladder.VisionLevel, () => { c.vision = c.vision % 3 + 1; Dressed(); });
        Tag(2, new Vector2(-250, -4), "MEMORY", MemoryModes.Labels[c.memory], free || l.memory, Ladder.MemoryLevel, () => { c.memory = (c.memory + 1) % MemoryModes.Labels.Length; Dressed(); });
        PlaceTags();
    }

    void Tag(int part, Vector2 offset, string name, string value, bool open, int opensAt, System.Action turn)
    {
        var card = Ui.Glass(tags, name, 0, 0, 180, 56, true, open ? Theme.Teal : new Color(1f, 1f, 1f, 0.25f));
        var n = Ui.Eyebrow(card.transform, open ? name : name + "  ·  OPENS AT 1." + opensAt, open ? Theme.Teal : Theme.Faint);
        Ui.Place(n.rectTransform, 16, 8, 160, 16);
        n.enableWordWrapping = false;
        var v = Ui.Text(card.transform, value, 17, open ? Color.white : Theme.Faint, true);
        Ui.Place(v.rectTransform, 16, 26, 160, 24);
        v.enableWordWrapping = false; v.overflowMode = TextOverflowModes.Ellipsis;
        if (open)
        {
            card.raycastTarget = true;
            var b = card.gameObject.AddComponent<Button>();
            b.targetGraphic = card;
            b.onClick.AddListener(() => { Sfx.Tick(); turn(); });
        }
        var dot = Ui.Box(tags, "Dot", open ? Theme.Teal : new Color(1f, 1f, 1f, 0.4f));
        Ui.Place(dot.rectTransform, 0, 0, 12, 12);
        dot.raycastTarget = false;
        tagList.Add(new Hook { rect = card.rectTransform, dot = dot, part = part, offset = offset });
    }

    // Keeps the tags on the agent's parts after the camera has moved.
    void PlaceTags()
    {
        if (stage == null) return;
        foreach (var t in tagList)
        {
            bool on = world.PartOnScreen(Chosen, t.part, out var s);
            t.rect.gameObject.SetActive(on); t.dot.gameObject.SetActive(on);
            if (!on) continue;
            var p = app.StagePoint(s);
            Ui.Place(t.dot.rectTransform, p.x - 6, p.y - 6, 12, 12);
            Ui.Place(t.rect, p.x + t.offset.x, p.y + t.offset.y, 180, 56);
        }
    }

    // A dial moved: the agent wears it and pops, the tags and the arithmetic follow.
    void Dressed()
    {
        world.Dress(Chosen);
        world.Punch(Chosen);
        BuildTags();
        RefreshEstimate();
    }

    class Follower : MonoBehaviour
    {
        SetupScreen screen;
        public void Bind(SetupScreen s) { screen = s; s.app.view.Framed += s.PlaceTags; }
        void OnDestroy() { if (screen != null) screen.app.view.Framed -= screen.PlaceTags; }
    }

    // ---- the tour and the roster ---------------------------------------------------

    Coroutine touring;

    IEnumerator<object> RunTour()
    {
        if (listArea == null || estimate == null) yield break;
        app.onLeave = () => { if (touring != null) app.StopCoroutine(touring); touring = null; };
        yield return Tour.Run(app, root, new[]
        {
            (message.rectTransform, "tour-1"),
            (listArea, "tour-2"),
            (estimate.rectTransform, "tour-3"),
            (send.GetComponent<RectTransform>(), "tour-4"),
        });
        touring = null;
        app.onLeave = null;
    }

    void Select(int index)
    {
        app.SaveConfigs();
        selected = index;
        Place();
        Show();
        RefreshStrip();
        BuildEditor();
        BuildTags();
    }

    // One chip per agent, when there is more than one: the chosen one is lit.
    void RefreshStrip()
    {
        Ui.Clear(strip);
        int n = app.configs.Count;
        if (n <= 1) return;
        const float w = 128, h = 44, gap = 10;
        for (int i = 0; i < n; i++)
        {
            int index = i;
            var c = app.configs[i];
            bool on = i == selected;
            var pill = Ui.Button(strip, "", () => Select(index), Ui.ButtonStyle.Secondary, 15);
            var rt = pill.GetComponent<RectTransform>();
            Ui.Place(rt, i % 5 * (w + gap), i / 5 * (h + 8), w, h);
            pill.GetComponent<Image>().color = on ? Theme.Team[c.colour] : Theme.Border;
            pill.transform.Find("Fill").GetComponent<Image>().color = on ? Theme.Surface2 : Theme.Panel;
            var dot = Ui.Box(pill.transform, "Dot", Theme.Team[c.colour]);
            Ui.Place(dot.rectTransform, 14, h / 2 - 7, 14, 14);
            dot.raycastTarget = false;
            var label = pill.GetComponentInChildren<TextMeshProUGUI>();
            label.text = string.IsNullOrWhiteSpace(c.name) ? "(unnamed)" : c.name;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontSize = 15;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            Ui.Stretch(label.rectTransform, 34, 0, 10, 0);
        }
    }

    // ---- the list and the wallet -------------------------------------------------------

    TextMeshProUGUI estimate, reads, readsTokens;
    RectTransform listArea, wallet;
    (Image track, Image fill) fuel;
    AgentConfig Current => app.configs[selected];

    void BuildEditor()
    {
        Ui.Clear(editor);
        var c = Current;
        var card = Ui.Float(editor, "Card", 0, 0, 1080, 770);
        var t = card.transform;

        // The agent's name and colour: the player's character.
        var nameLabel = Ui.Eyebrow(t, "YOUR AGENT", Theme.Muted);
        Ui.Place(nameLabel.rectTransform, 28, 16, 200, 20);
        var name = Ui.Input(t, "Its name", false, 20);
        Ui.Place(name.GetComponent<RectTransform>(), 28, 38, 300, 48);
        name.text = c.name;
        name.onValueChanged.AddListener(v => { c.name = v; });
        name.onEndEdit.AddListener(_ => RefreshStrip());
        for (int i = 0; i < Theme.Team.Length; i++)
        {
            int index = i;
            var ring = Ui.Box(t, "Swatch", c.colour == i ? Theme.Ink : Theme.Panel);
            Ui.Place(ring.rectTransform, 350 + i * 44, 42, 40, 40);
            var swatch = Ui.Box(ring.transform, "Colour", Theme.Team[i]);
            Ui.Stretch(swatch.rectTransform, 4, 4, 4, 4);
            var b = ring.gameObject.AddComponent<Button>();
            b.targetGraphic = swatch;
            b.onClick.AddListener(() => { c.colour = index; world.RefreshRobot(Chosen); RefreshStrip(); BuildEditor(); BuildTags(); });
        }

        // The list
        listArea = Ui.Node("List", t);
        Ui.Place(listArea, 16, 96, 1048, 420);
        var listLabel = Ui.Eyebrow(t, "THE LIST  ·  EVERYTHING IT WILL KNOW", Theme.TealDeep);
        Ui.Place(listLabel.rectTransform, 28, 104, 800, 20);
        var mode = Ui.Button(t, c.freeText ? "Build it from parts" : "Write it yourself", () =>
        {
            if (c.freeText) { c.freeText = false; c.Compose(); } else { c.freeText = true; }
            BuildEditor();
        }, Ui.ButtonStyle.Quiet, 13);
        Ui.Place(mode.GetComponent<RectTransform>(), 870, 96, 182, 34);

        if (c.freeText) BuildWords(t, c);
        else { c.Compose(); BuildParts(t, c); }

        // The wallet: coins of pocket money, and how far they go.
        var fuelLabel = Ui.Eyebrow(t, "POCKET MONEY  ·  TAP THE COINS", Theme.TealDeep);
        Ui.Place(fuelLabel.rectTransform, 28, 540, 500, 20);
        wallet = Ui.Node("Wallet", t);
        Ui.Place(wallet, 28, 566, 1024, 60);
        BuildWallet();
        fuel = Ui.Bar(t, Theme.Border, Theme.Aqua, 1024, 12);
        Ui.Place(fuel.track.rectTransform, 28, 640, 1024, 12);
        estimate = Ui.Text(t, "", 15, Theme.Text2);
        Ui.Place(estimate.rectTransform, 28, 660, 1024, 30);
        RefreshEstimate();
    }

    // Ten coins; the ones the agent takes are lit. Tap a coin to take that many.
    void BuildWallet()
    {
        Ui.Clear(wallet);
        int taken = Mathf.Clamp((int)(app.budget / Coin), 1, Coins);
        for (int i = 0; i < Coins; i++)
        {
            int n = i + 1;
            bool on = i < taken;
            var coin = Ui.Box(wallet, "Coin", on ? Theme.Amber : Theme.Surface3);
            Ui.Place(coin.rectTransform, i * 66, 4, 52, 52);
            var inner = Ui.Box(coin.transform, "Face", on ? Theme.Hex("#FFD27A") : Theme.Panel);
            Ui.Stretch(inner.rectTransform, 5, 5, 5, 5);
            inner.raycastTarget = false;
            var k = Ui.Text(inner.transform, "10k", 13, on ? Theme.Hex("#7A5A10") : Theme.Muted, true, TextAlignmentOptions.Center);
            Ui.Stretch(k.rectTransform);
            var b = coin.gameObject.AddComponent<Button>();
            b.targetGraphic = coin;
            b.onClick.AddListener(() => { Sfx.Chime(); app.budget = n * Coin; BuildWallet(); RefreshEstimate(); });
        }
        var total = Ui.Text(wallet, $"{taken * Coin:N0} tokens", 17, Theme.Ink, true, TextAlignmentOptions.MidlineRight);
        Ui.Place(total.rectTransform, 680, 4, 344, 52);
    }

    // The list from parts: tap the goods on the shelf, pick the rule for an empty
    // view, add your own words. The words the agent reads are shown underneath.
    void BuildParts(Transform t, AgentConfig c)
    {
        var all = Items.All;
        float slot = 1024f / all.Length;
        for (int i = 0; i < all.Length; i++)
        {
            var k = all[i];
            float x = 28 + i * slot;
            bool buy = c.buy.Contains(k.id), never = c.never.Contains(k.id);
            var ring = Ui.Box(t, "Ring", buy ? Theme.Aqua : never ? Theme.Danger : Theme.Surface3);
            Ui.Place(ring.rectTransform, x + slot / 2 - 27, 132, 54, 54);
            var inner = Ui.Box(ring.transform, "Fill", buy ? Theme.Surface2 : never ? Theme.DangerSoft : Theme.Panel);
            Ui.Stretch(inner.rectTransform, 4, 4, 4, 4);
            var icon = Ui.Icon(ring.transform, k.id, 34);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icon.rectTransform.anchoredPosition = Vector2.zero;
            var label = Ui.Text(t, k.id, 11, buy ? Theme.TealDeep : never ? Theme.Danger : Theme.Text2, buy || never, TextAlignmentOptions.Center);
            Ui.Place(label.rectTransform, x, 188, slot, 16);
            label.enableWordWrapping = false;
            if (buy || never)
            {
                var state = Ui.Text(t, buy ? "BUY" : "NEVER", 9, buy ? Theme.TealDeep : Theme.Danger, true, TextAlignmentOptions.Center);
                state.characterSpacing = 3;
                Ui.Place(state.rectTransform, x, 202, slot, 14);
            }
            var btn = ring.gameObject.AddComponent<Button>();
            btn.targetGraphic = inner;
            string id = k.id;
            btn.onClick.AddListener(() =>
            {
                Sfx.Tick();
                if (c.buy.Remove(id)) c.never.Add(id);
                else if (!c.never.Remove(id)) c.buy.Add(id);
                c.Compose();
                BuildEditor();
            });
        }

        var ruleLabel = Ui.Eyebrow(t, "IF NOTHING IS IN VIEW", Theme.Muted);
        Ui.Place(ruleLabel.rectTransform, 28, 226, 500, 20);
        var rule = new Ui.Segmented(t, AgentConfig.FallbackLabels, c.fallback, 560, 38, 13);
        Ui.Place(rule.root, 28, 248, 560, 38);
        rule.OnSelect = i => { c.fallback = i; c.Compose(); RefreshRead(); };

        var wordsLabel = Ui.Eyebrow(t, "YOUR OWN WORDS", Theme.Muted);
        Ui.Place(wordsLabel.rectTransform, 610, 226, 460, 20);
        var words = Ui.Input(t, "For example: in this house a tomato counts as a vegetable.", false, 14);
        Ui.Place(words.GetComponent<RectTransform>(), 610, 248, 442, 38);
        words.text = c.words;
        words.onValueChanged.AddListener(v => { c.words = v; c.Compose(); RefreshRead(); });

        var readsLabel = Ui.Eyebrow(t, "WHAT THE AGENT WILL READ", Theme.Muted);
        Ui.Place(readsLabel.rectTransform, 28, 298, 400, 20);
        readsTokens = Ui.Text(t, "", 13, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(readsTokens.rectTransform, 852, 298, 200, 20);
        var readsCard = Ui.Soft(t, "Reads");
        Ui.Place(readsCard.rectTransform, 28, 320, 1024, 196);
        reads = Ui.Text(readsCard.transform, "", 15, Theme.Ink);
        Ui.Stretch(reads.rectTransform, 16, 10, 16, 10);
        RefreshRead();
    }

    // The list in the player's own words, as it will be read.
    void BuildWords(Transform t, AgentConfig c)
    {
        readsTokens = Ui.Text(t, "", 13, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(readsTokens.rectTransform, 660, 104, 200, 20);
        var field = Ui.Input(t, "For example: Buy apples, bananas and pears. Never buy tomatoes, potatoes or mushrooms. If nothing on this list is in view, keep walking.", true, 18);
        Ui.Place(field.GetComponent<RectTransform>(), 28, 134, 1024, 382);
        field.text = c.instructions;
        field.onValueChanged.AddListener(v => { c.instructions = v; RefreshRead(); });
        reads = null;
        RefreshRead();
    }

    void RefreshRead()
    {
        var c = Current;
        if (reads != null) reads.text = string.IsNullOrWhiteSpace(c.instructions) ? "(nothing yet: the agent would leave with no list at all)" : c.instructions;
        if (readsTokens != null) readsTokens.text = $"about {Observation.EstimateTokens(c.instructions):N0} tokens";
        RefreshEstimate();
    }

    // The arithmetic the players should be doing in their heads: what one turn costs,
    // and how many turns the pocket money buys.
    void RefreshEstimate()
    {
        if (estimate == null) return;
        var c = Current;
        long list = Observation.EstimateTokens(c.instructions);
        long fixedPart = 200 + list;
        long perView = 40 + 14 * (2 * c.vision + 1) * (2 * c.vision + 1) / 3;
        long memory = MemoryModes.Turns(c.memory) * (perView + 40) + (c.memory == MemoryModes.Notebook ? 40 : 0);
        long output = 60 + 120 * c.effort;
        long perTurn = fixedPart + perView + memory + output;
        long turnsAffordable = perTurn > 0 ? app.budget / perTurn : 0;
        Ui.SetBar(fuel, app.turns > 0 ? Mathf.Clamp01((float)turnsAffordable / app.turns) : 0);
        fuel.fill.color = turnsAffordable >= app.turns ? Theme.Aqua : Theme.Amber;
        string lasts = turnsAffordable >= app.turns ? $"the whole errand of {app.turns} turns" : $"about {turnsAffordable} of {app.turns} turns";
        estimate.text = $"About {perTurn:N0} tokens a turn  ·  enough for {lasts}";
    }
}
