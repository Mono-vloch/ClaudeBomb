using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The garage: where the agent is fitted out before it leaves. The agent stands in
// the market, big, and its four dials hang off it as tags: tap the antenna to switch
// brains (the bulb swaps and the agent pops), the eyes for how far it sees, its back for
// what it remembers, the antenna's glow for effort; a dial an errand has not opened wears
// a lock. In a dark glass drawer on the right (640 of the 1920 units, a third, folding to
// a tab) the one thing the player writes: the list the agent will read, word for word,
// and under it the pocket money as a wallet of coins, and the button that sends it. The
// agents run by themselves; this is the last word before they do.
public class SetupScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Market world;
    readonly RectTransform strip, editor, frame, tags, drawer;
    const float DrawerW = 640f, EdW = 584f; // the drawer, and the width of what is in it
    bool folded;
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
        Ui.Place(frame, 0, 110, Ui.W - DrawerW, Ui.H - 110);
        selected = Mathf.Clamp(selected, 0, app.configs.Count - 1);

        // Dark glass along the top of the world: the errand, the way back, the file, the settings.
        var top = Ui.Box(root, "Top", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.82f), false);
        Ui.Place(top.rectTransform, 0, 0, Ui.W - DrawerW, 104);
        top.raycastTarget = false;
        var eyebrow = Ui.Eyebrow(root, lesson != null ? $"ERRAND {lesson.id}  ·  THE GARAGE" : "FREE PLAY  ·  THE GARAGE", Theme.Teal);
        Ui.Place(eyebrow.rectTransform, 40, 18, 600, 22);
        var title = Ui.Text(root, lesson != null ? lesson.title : "Fit out the agent", 32, Theme.Ink, true);
        Ui.Place(title.rectTransform, 40, 44, 760, 50);
        title.enableWordWrapping = false; title.overflowMode = TextOverflowModes.Ellipsis;
        float bx = Ui.W - DrawerW - 24;
        var settings = Ui.Button(root, "Settings", () => SettingsSheet.Toggle(app, root, Reload), Ui.ButtonStyle.Quiet, 16);
        bx -= 112; Ui.Place(settings.GetComponent<RectTransform>(), bx, 32, 108, 44);
        var file = Ui.Button(root, "The file", () => app.ShowFile(), Ui.ButtonStyle.Quiet, 16);
        bx -= 108; Ui.Place(file.GetComponent<RectTransform>(), bx, 32, 104, 44);
        if (lesson != null)
        {
            var tour = Ui.Button(root, "Tour", () => { if (touring == null) touring = app.StartCoroutine(RunTour()); }, Ui.ButtonStyle.Quiet, 16);
            bx -= 84; Ui.Place(tour.GetComponent<RectTransform>(), bx, 32, 80, 44);
            var back = Ui.Button(root, "Briefing", app.ShowLesson, Ui.ButtonStyle.Quiet, 16);
            bx -= 114; Ui.Place(back.GetComponent<RectTransform>(), bx, 32, 110, 44);
        }

        // The mission: the family's message, on glass over the world.
        message = Ui.Soft(root, "Message");
        Ui.Place(message.rectTransform, 40, 124, 780, 112);
        var tag = Ui.Eyebrow(message.transform, "", Theme.TealDeep);
        Ui.Place(tag.rectTransform, 20, 12, 600, 22);
        briefTitle = tag;
        briefRule = Ui.Text(message.transform, "", 21, Theme.Ink);
        Ui.Place(briefRule.rectTransform, 20, 38, 740, 34);
        briefRule.overflowMode = TextOverflowModes.Ellipsis;
        briefNote = Ui.Text(message.transform, "", 16, Theme.Text2);
        Ui.Place(briefNote.rectTransform, 20, 76, 740, 26);
        briefNote.enableWordWrapping = false; briefNote.overflowMode = TextOverflowModes.Ellipsis;
        if (lesson == null)
        {
            var newBrief = Ui.Button(root, "New message", () => { app.NewBrief(); ShowBrief(); BuildWorld(); }, Ui.ButtonStyle.Secondary, 16);
            Ui.Place(newBrief.GetComponent<RectTransform>(), 832, 124, 170, 44);
        }
        ShowBrief();

        // The drawer: the list, the wallet, the button that sends the agent.
        drawer = Ui.Node("Drawer", root);
        Ui.Place(drawer, Ui.W - DrawerW, 0, DrawerW, Ui.H);
        var glass = Ui.Box(drawer, "Glass", Theme.Panel, false);
        Ui.Stretch(glass.rectTransform);
        var edge = Ui.Box(drawer, "Edge", new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.5f), false);
        Ui.Place(edge.rectTransform, 0, 0, 2, Ui.H);
        edge.raycastTarget = false;
        Button tab = null;
        tab = Ui.Button(drawer, "›", () => { Fold(!folded); Ui.SetLabel(tab, folded ? "‹" : "›"); }, Ui.ButtonStyle.Secondary, 30);
        Ui.Place(tab.GetComponent<RectTransform>(), -52, 470, 48, 120);
        send = Ui.Button(drawer, app.configs.Count > 1 ? "DEPLOY AGENTS" : "DEPLOY THE AGENT", () => { foreach (var c in app.configs) c.Compose(); app.SaveConfigs(); app.StartRound(); }, Ui.ButtonStyle.Accent, 22);
        Ui.Place(send.GetComponent<RectTransform>(), 28, 1000, EdW, 60);

        // The dials, on the agent.
        tags = Ui.Node("Tags", root);
        Ui.Stretch(tags);

        // The agents, one chip each, only when there is more than one (a class).
        strip = Ui.Node("Strip", root);
        Ui.Place(strip, 40, 1010, 1200, 56);

        editor = Ui.Node("Editor", drawer);
        Ui.Place(editor, 0, 0, DrawerW, 990);

        BuildWorld();
        RefreshStrip();
        BuildEditor();
        BuildTags();
        Coach();
        var follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);

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
        coachLine = app.luma.Line(root, 40, 880, 780, (broken ? "Fix it: " + start.name + ". " : "") + start.why, true, false, 17);
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
        Tag(0, new Vector2(80, -30), "BRAIN", Models.All[c.model].label, free || l.model, Ladder.ModelLevel, () => { c.model = (c.model + 1) % Models.All.Length; Dressed(); });
        Tag(0, new Vector2(80, 72), "EFFORT", Efforts.Labels[c.effort], free || l.effort, Ladder.EffortLevel, () => { c.effort = (c.effort + 1) % Efforts.Labels.Length; Dressed(); });
        Tag(1, new Vector2(-330, -90), "SIGHT", $"{c.vision} cell{(c.vision > 1 ? "s" : "")}", free || l.vision, Ladder.VisionLevel, () => { c.vision = c.vision % 3 + 1; Dressed(); });
        Tag(2, new Vector2(-330, 10), "MEMORY", MemoryModes.Labels[c.memory], free || l.memory, Ladder.MemoryLevel, () => { c.memory = (c.memory + 1) % MemoryModes.Labels.Length; Dressed(); });
        PlaceTags();
    }

    void Tag(int part, Vector2 offset, string name, string value, bool open, int opensAt, System.Action turn)
    {
        var card = Ui.Glass(tags, name, 0, 0, 220, 62, true, open ? Theme.Teal : new Color(1f, 1f, 1f, 0.25f));
        var n = Ui.Eyebrow(card.transform, open ? name : name + "  ·  OPENS AT 1." + opensAt, open ? Theme.Teal : Theme.Faint);
        Ui.Place(n.rectTransform, 16, 8, 200, 20);
        n.enableWordWrapping = false; n.overflowMode = TextOverflowModes.Ellipsis;
        var v = Ui.Text(card.transform, value, 18, open ? Color.white : Theme.Faint, true);
        Ui.Place(v.rectTransform, 16, 30, 200, 26);
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
            Ui.Place(t.rect, Mathf.Clamp(p.x + t.offset.x, 16, Ui.W - (folded ? 56 : DrawerW) - 236), Mathf.Clamp(p.y + t.offset.y, 250, Ui.H - 140), 220, 62);
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
            pill.GetComponentInChildren<TextMeshProUGUI>().color = Theme.Chalk;
            var dot = Ui.Box(pill.transform, "Dot", Theme.Team[c.colour]);
            Ui.Place(dot.rectTransform, 14, h / 2 - 7, 14, 14);
            dot.raycastTarget = false;
            var label = pill.GetComponentInChildren<TextMeshProUGUI>();
            label.text = string.IsNullOrWhiteSpace(c.name) ? "(unnamed)" : c.name;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontSize = 16;
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
        var t = editor;
        const float x0 = 28;

        // The agent's name and colour: the player's character.
        var nameLabel = Ui.Eyebrow(t, "YOUR AGENT", Theme.Muted);
        Ui.Place(nameLabel.rectTransform, x0, 22, 300, 22);
        var name = Ui.Input(t, "Its name", false, 18);
        Ui.Place(name.GetComponent<RectTransform>(), x0, 50, 220, 46);
        name.text = c.name;
        name.onValueChanged.AddListener(v => { c.name = v; });
        name.onEndEdit.AddListener(_ => RefreshStrip());
        for (int i = 0; i < Theme.Team.Length; i++)
        {
            int index = i;
            var ring = Ui.Box(t, "Swatch", c.colour == i ? Theme.Chalk : new Color(1f, 1f, 1f, 0.12f));
            Ui.Place(ring.rectTransform, x0 + 236 + i * 35, 56, 33, 33);
            var swatch = Ui.Box(ring.transform, "Colour", Theme.Team[i]);
            Ui.Stretch(swatch.rectTransform, 3, 3, 3, 3);
            var b = ring.gameObject.AddComponent<Button>();
            b.targetGraphic = swatch;
            b.onClick.AddListener(() => { c.colour = index; world.RefreshRobot(Chosen); RefreshStrip(); BuildEditor(); BuildTags(); });
        }

        // The list
        listArea = Ui.Node("List", t);
        Ui.Place(listArea, 16, 106, DrawerW - 32, 740);
        var listLabel = Ui.Eyebrow(t, "THE LIST  ·  EVERYTHING IT WILL KNOW", Theme.TealDeep);
        Ui.Place(listLabel.rectTransform, x0, 116, 400, 22);
        listLabel.enableWordWrapping = false;
        var mode = Ui.Button(t, c.freeText ? "Build it from parts" : "Write it yourself", () =>
        {
            if (c.freeText) { c.freeText = false; c.Compose(); } else { c.freeText = true; }
            BuildEditor();
        }, Ui.ButtonStyle.Secondary, 16);
        Ui.Place(mode.GetComponent<RectTransform>(), x0 + EdW - 196, 106, 196, 38);

        if (c.freeText) BuildWords(t, c);
        else { c.Compose(); BuildParts(t, c); }

        // The wallet: coins of pocket money, and how far they go.
        var fuelLabel = Ui.Eyebrow(t, "POCKET MONEY  ·  TAP THE COINS", Theme.TealDeep);
        Ui.Place(fuelLabel.rectTransform, x0, 856, 400, 22);
        wallet = Ui.Node("Wallet", t);
        Ui.Place(wallet, x0, 850, EdW, 90);
        BuildWallet();
        fuel = Ui.Bar(t, Theme.Border, Theme.Aqua, EdW, 10);
        Ui.Place(fuel.track.rectTransform, x0, 944, EdW, 10);
        estimate = Ui.Text(t, "", 16, Theme.Text2);
        Ui.Place(estimate.rectTransform, x0, 960, EdW, 26);
        estimate.enableWordWrapping = false; estimate.overflowMode = TextOverflowModes.Ellipsis;
        RefreshEstimate();
    }

    // Ten coins; the ones the agent takes are lit. Tap a coin to take that many.
    void BuildWallet()
    {
        Ui.Clear(wallet);
        int taken = Mathf.Clamp((int)(app.budget / Coin), 1, Coins);
        float step = (EdW - 52) / (Coins - 1);
        for (int i = 0; i < Coins; i++)
        {
            int n = i + 1;
            bool on = i < taken;
            var coin = Ui.Box(wallet, "Coin", on ? Theme.Amber : Theme.Surface3);
            Ui.Place(coin.rectTransform, i * step, 34, 52, 52);
            var inner = Ui.Box(coin.transform, "Face", on ? Theme.Hex("#FFD27A") : Theme.Surface1);
            Ui.Stretch(inner.rectTransform, 5, 5, 5, 5);
            inner.raycastTarget = false;
            var k = Ui.Text(inner.transform, "10k", 16, on ? Theme.Hex("#7A5A10") : Theme.Muted, true, TextAlignmentOptions.Center);
            Ui.Stretch(k.rectTransform);
            var b = coin.gameObject.AddComponent<Button>();
            b.targetGraphic = coin;
            b.onClick.AddListener(() => { Sfx.Chime(); app.budget = n * Coin; BuildWallet(); RefreshEstimate(); });
        }
        var total = Ui.Text(wallet, $"{taken * Coin:N0} tokens", 18, Theme.Ink, true, TextAlignmentOptions.TopRight);
        Ui.Place(total.rectTransform, EdW - 240, 4, 240, 26);
    }

    // The list from parts: tap the goods on the shelf, pick the rule for an empty
    // view, add your own words. The words the agent reads are shown underneath.
    void BuildParts(Transform t, AgentConfig c)
    {
        const float x0 = 28;
        var all = Items.All;
        const int cols = 5;
        float slot = EdW / cols;
        for (int i = 0; i < all.Length; i++)
        {
            var k = all[i];
            float x = x0 + (i % cols) * slot, y = 154 + (i / cols) * 104;
            bool buy = c.buy.Contains(k.id), never = c.never.Contains(k.id);
            var ring = Ui.Box(t, "Ring", buy ? Theme.Aqua : never ? Theme.Danger : Theme.Surface3);
            Ui.Place(ring.rectTransform, x + slot / 2 - 28, y, 56, 56);
            var inner = Ui.Box(ring.transform, "Fill", buy ? Theme.Surface2 : never ? Theme.DangerSoft : Theme.Surface1);
            Ui.Stretch(inner.rectTransform, 4, 4, 4, 4);
            var icon = Ui.Icon(ring.transform, k.id, 36);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icon.rectTransform.anchoredPosition = Vector2.zero;
            var label = Ui.Text(t, k.id, 16, buy ? Theme.TealDeep : never ? Theme.Danger : Theme.Text2, buy || never, TextAlignmentOptions.Center);
            Ui.Place(label.rectTransform, x - 4, y + 60, slot + 8, 22);
            label.enableWordWrapping = false;
            if (buy || never)
            {
                var state = Ui.Text(t, buy ? "BUY" : "NEVER", 16, buy ? Theme.TealDeep : Theme.Danger, true, TextAlignmentOptions.Center);
                state.characterSpacing = 3;
                Ui.Place(state.rectTransform, x, y + 80, slot, 20);
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
        Ui.Place(ruleLabel.rectTransform, x0, 474, 400, 22);
        var rule = new Ui.Segmented(t, AgentConfig.FallbackLabels, c.fallback, EdW, 56, 16);
        Ui.Place(rule.root, x0, 500, EdW, 56);
        rule.OnSelect = i => { c.fallback = i; c.Compose(); RefreshRead(); };

        var wordsLabel = Ui.Eyebrow(t, "YOUR OWN WORDS", Theme.Muted);
        Ui.Place(wordsLabel.rectTransform, x0, 572, 400, 22);
        var words = Ui.Input(t, "For example: in this house a tomato counts as a vegetable.", false, 16);
        Ui.Place(words.GetComponent<RectTransform>(), x0, 598, EdW, 46);
        words.text = c.words;
        words.onValueChanged.AddListener(v => { c.words = v; c.Compose(); RefreshRead(); });

        var readsLabel = Ui.Eyebrow(t, "WHAT THE AGENT WILL READ", Theme.Muted);
        Ui.Place(readsLabel.rectTransform, x0, 660, 360, 22);
        readsTokens = Ui.Text(t, "", 16, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(readsTokens.rectTransform, x0 + EdW - 220, 660, 220, 22);
        var readsCard = Ui.Soft(t, "Reads");
        Ui.Place(readsCard.rectTransform, x0, 688, EdW, 154);
        reads = Ui.Text(readsCard.transform, "", 16, Theme.Ink);
        Ui.Stretch(reads.rectTransform, 14, 10, 14, 10);
        reads.overflowMode = TextOverflowModes.Ellipsis;
        RefreshRead();
    }

    // The list in the player's own words, as it will be read.
    void BuildWords(Transform t, AgentConfig c)
    {
        readsTokens = Ui.Text(t, "", 16, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(readsTokens.rectTransform, 28 + EdW - 220, 812, 220, 22);
        var field = Ui.Input(t, "For example: Buy apples, bananas and pears. Never buy tomatoes, potatoes or mushrooms. If nothing on this list is in view, keep walking.", true, 18);
        Ui.Place(field.GetComponent<RectTransform>(), 28, 154, EdW, 650);
        field.text = c.instructions;
        field.onValueChanged.AddListener(v => { c.instructions = v; RefreshRead(); });
        reads = null;
        RefreshRead();
    }

    // The drawer folds to a tab and the agent takes the whole stage; open again to write.
    void Fold(bool fold)
    {
        folded = fold;
        float from = drawer.anchoredPosition.x, to = fold ? Ui.W - 56f : Ui.W - DrawerW;
        app.StartCoroutine(Ui.Animate(0.3f, k =>
        {
            if (drawer == null) return;
            drawer.anchoredPosition = new Vector2(Mathf.Lerp(from, to, k), drawer.anchoredPosition.y);
            Ui.Place(frame, 0, 110, Mathf.Lerp(from, to, k), Ui.H - 110);
        }, Ui.EaseOut));
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
