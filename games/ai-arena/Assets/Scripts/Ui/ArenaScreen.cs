using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The errand, live: the market in three dimensions, at street level behind the chosen
// agent (the realism pass: the agent is the hero of the frame, the overview only when the
// player asks for it); a dark glass drawer on the right, 600 of the 1920 units (31 %), that
// folds away, with the mission and one card per agent (its basket, its pocket money
// draining, the last thing it texted home) and LUMA underneath. A countdown sends the
// agents out; banners mark the big moments (sent home, out of tokens, home). The market
// can be watched from the street, through the chosen agent's eyes or from above. On the
// first errand the loop runs one turn at a time.
public class ArenaScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Arena arena;
    readonly Simulation sim;
    readonly Market world;
    readonly TextMeshProUGUI turnText, phaseText, homeText, basketText, tokensText;
    readonly Image[] loopChips = new Image[3];
    readonly TextMeshProUGUI[] loopWords = new TextMeshProUGUI[3];
    bool showing;
    readonly Button pause, results;
    readonly RectTransform cards, labelsLayer, bannerLayer, drawer, frame;
    readonly float cardsHeight;
    bool folded;
    const float DrawerW = 600f, CardW = 552f;
    public static ArenaScreen Current { get; private set; }
    readonly Ui.Segmented viewToggle;
    // The view, as the player would pick it (0 street, 1 agent's eye, 2 the market from above): for the screenshot walk.
    public void SelectView(int i) => viewToggle.Set(i);
    RectTransform feed;            // the agent's eye as a camera feed: corner brackets, a REC dot, the agent's name
    TextMeshProUGUI feedLabel, feedSight;
    Image recDot;
    float WorldW => folded ? Ui.W - 56f : Ui.W - DrawerW; // the world's share of the stage
    readonly Dictionary<AgentState, Card> cardOf = new Dictionary<AgentState, Card>();
    readonly Dictionary<AgentState, RectTransform> labelOf = new Dictionary<AgentState, RectTransform>();
    readonly HashSet<AgentState> announced = new HashSet<AgentState>();
    readonly Queue<(string text, string sub, Color colour, float seconds, bool big)> banners = new Queue<(string, string, Color, float, bool)>();
    readonly Follower follower;
    RectTransform coachLine, knows, bubble;
    AgentState bubbleFor;
    bool saidScore, saidOut, saidTokens, saidConfused;
    bool firstErrand; // the first turn is read out as one loop
    string phaseBase = "";

    class Card
    {
        public Image outer, fill;
        public TextMeshProUGUI score, status, thought, tokens;
        public (Image track, Image fill) bar;
    }

    public ArenaScreen(Transform parent, App app, Arena arena, Simulation sim)
    {
        this.app = app; this.arena = arena; this.sim = sim;
        Current = this;
        root = Ui.Node("Arena", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear); // the market shows through

        // The market, behind everything, in the part of the stage the drawer leaves.
        world = app.world;
        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, WorldW, Ui.H);
        world.Build(arena, false);
        if (Market.Real) { world.Highlight(arena.agents[0]); world.ShowStreet(frame); } else world.ShowMarket(frame);
        sim.Holding = () => world.Animating; // a real walk takes real time

        // The mission HUD, dark glass over the market: the errand and the step, the turn,
        // the loop's three words lighting one after the other, and the score.
        var hud1 = Ui.Glass(root, "Hud", 40, 16, 520, 100);
        string where = app.decide != null ? $"TASK {app.decide.RunNumber} OF {Decide.ShoppingTasks}" : app.lesson != null ? $"ERRAND {app.lesson.id}" : "FREE PLAY";
        var levelTag = Ui.Eyebrow(hud1.transform, where, Theme.Teal);
        levelTag.font = Theme.Bold;
        Ui.Place(levelTag.rectTransform, 20, 10, 480, 20);
        turnText = Ui.Text(hud1.transform, "", 28, Color.white, true);
        Ui.Place(turnText.rectTransform, 20, 30, 480, 40);
        turnText.enableWordWrapping = false;
        phaseText = Ui.Text(hud1.transform, "", 16, Look.Chalk);
        Ui.Place(phaseText.rectTransform, 20, 70, 490, 24);
        phaseText.enableWordWrapping = false; phaseText.overflowMode = TextOverflowModes.Ellipsis;
        var hud2 = Ui.Glass(root, "Loop", 570, 16, 420, 100);
        var loopTag = Ui.Eyebrow(hud2.transform, "THE LOOP  ·  EVERY TURN", Theme.Teal);
        Ui.Place(loopTag.rectTransform, 20, 10, 380, 20);
        string[] words = { "LOOK", "DECIDE", "MOVE" };
        for (int i = 0; i < 3; i++)
        {
            loopChips[i] = Ui.Box(hud2.transform, "Chip", new Color(1f, 1f, 1f, 0.1f));
            Ui.Place(loopChips[i].rectTransform, 20 + i * 126, 40, 116, 44);
            loopChips[i].raycastTarget = false;
            loopWords[i] = Ui.Text(loopChips[i].transform, words[i], 16, Look.Chalk, true, TextAlignmentOptions.Center);
            loopWords[i].characterSpacing = 3;
            Ui.Stretch(loopWords[i].rectTransform);
        }
        var hud3 = Ui.Glass(root, "Score", 40, 124, 520, 76);
        TextMeshProUGUI Stat(string name, float x, float w)
        {
            var e = Ui.Eyebrow(hud3.transform, name, Theme.Teal);
            Ui.Place(e.rectTransform, x, 10, w, 20);
            var v = Ui.Text(hud3.transform, "", 22, Color.white, true);
            Ui.Place(v.rectTransform, x, 32, w, 34);
            v.enableWordWrapping = false; v.overflowMode = TextOverflowModes.Ellipsis;
            return v;
        }
        homeText = Stat("HOME", 20, 140);
        basketText = Stat("IN THE BASKETS", 170, 190);
        tokensText = Stat("TOKENS LEFT", 370, 140);

        // The view: the street behind the chosen agent, what it sees, or the whole market from
        // above (only when asked for); the pause; calling them home; the settings.
        var views = Market.Real ? new[] { "Street", "Agent's eye", "The market" } : new[] { "The market", "Agent's eye" };
        viewToggle = new Ui.Segmented(root, views, 0, Market.Real ? 450 : 300, 44, 16);
        Ui.Place(viewToggle.root, 40, 1020, Market.Real ? 450 : 300, 44);
        viewToggle.OnSelect = i =>
        {
            var v = Market.Real ? (i == 0 ? Market.View.Street : i == 1 ? Market.View.RobotsEye : Market.View.Market) : (i == 0 ? Market.View.Market : Market.View.RobotsEye);
            world.SetView(v);
            if (feed != null) feed.gameObject.SetActive(v == Market.View.RobotsEye);
            RefreshFeed();
        };
        BuildFeed();
        float bx = Market.Real ? 502 : 352;
        pause = Ui.Button(root, "Pause", () => { sim.paused = !sim.paused; Ui.SetLabel(pause, sim.paused ? "Resume" : "Pause"); }, Ui.ButtonStyle.Secondary, 16);
        Ui.Place(pause.GetComponent<RectTransform>(), bx, 1020, 104, 44);
        var stop = Ui.Button(root, "Call them home", () => { if (!sim.finished) sim.Abort(); }, Ui.ButtonStyle.Quiet, 16);
        Ui.Place(stop.GetComponent<RectTransform>(), bx + 114, 1020, 180, 44);
        var settings = Ui.Button(root, "Settings", () => SettingsSheet.Toggle(app, root, null), Ui.ButtonStyle.Quiet, 16); // the speed, among others
        Ui.Place(settings.GetComponent<RectTransform>(), bx + 304, 1020, 116, 44);

        // The agents' names, under their feet; the bubble over the chosen one.
        labelsLayer = Ui.Node("Labels", root);
        Ui.Stretch(labelsLayer);
        foreach (var a in arena.agents)
        {
            var label = Ui.Box(labelsLayer, "Label " + a.Name, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.82f));
            label.raycastTarget = false;
            var t = Ui.Text(label.transform, a.Name, 16, Theme.Chalk, true, TextAlignmentOptions.Center);
            Ui.Stretch(t.rectTransform, 6, 0, 6, 0);
            t.enableWordWrapping = false;
            labelOf[a] = label.rectTransform;
        }
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);

        // The drawer: dark glass down the right side, folding to a tab.
        drawer = Ui.Node("Drawer", root);
        Ui.Place(drawer, Ui.W - DrawerW, 0, DrawerW, Ui.H);
        var glass = Ui.Box(drawer, "Glass", Theme.Panel, false);
        Ui.Stretch(glass.rectTransform);
        glass.raycastTarget = true;
        var edge = Ui.Box(drawer, "Edge", new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.5f), false);
        Ui.Place(edge.rectTransform, 0, 0, 2, Ui.H);
        edge.raycastTarget = false;
        Button tab = null;
        tab = Ui.Button(drawer, "›", () => { Fold(!folded); Ui.SetLabel(tab, folded ? "‹" : "›"); }, Ui.ButtonStyle.Secondary, 30);
        Ui.Place(tab.GetComponent<RectTransform>(), -52, 470, 48, 120);

        // The mission, at the top of the drawer.
        var mission = Ui.Soft(drawer, "Mission");
        Ui.Place(mission.rectTransform, 24, 24, CardW, 108);
        var missionTag = Ui.Eyebrow(mission.transform, $"MISSION  ·  {arena.brief.from.ToUpperInvariant()}  ·  {where}", Theme.TealDeep);
        Ui.Place(missionTag.rectTransform, 16, 12, CardW - 32, 22);
        missionTag.enableWordWrapping = false; missionTag.overflowMode = TextOverflowModes.Ellipsis;
        var missionText = Ui.Text(mission.transform, arena.brief.rule, 18, Theme.Ink);
        Ui.Place(missionText.rectTransform, 16, 40, CardW - 32, 60);
        missionText.overflowMode = TextOverflowModes.Ellipsis;

        // LUMA gets the bottom of the column when the cards leave room for her.
        bool withCoach = arena.agents.Count <= 6;
        cardsHeight = withCoach ? 640 : 830;
        cards = Ui.Node("Cards", drawer);
        Ui.Place(cards, 24, 148, CardW, cardsHeight);
        BuildCards();
        if (withCoach)
        {
            firstErrand = app.decide == null && app.lesson != null && app.lesson.number == 1;
            string opening = app.decide != null
                ? $"Task {app.decide.RunNumber} of {Decide.ShoppingTasks}: \"{arena.brief.rule}\" The gray agent is a fixed rule: no AI, no tokens. The teal one is an agent with a list. Watch the basket and the cost."
                : firstErrand ? "Watch your agent. After its first turn I will read out the loop it just ran."
                : app.luma.Say(app.lesson != null ? "live-" + app.lesson.number : "free");
            SayLine(opening, true);
            sim.OnTurnResolved += Watch;
        }

        bool lastTask = app.decide == null || app.decide.RunNumber >= Decide.ShoppingTasks;
        results = Ui.Button(drawer, lastTask ? "See the debrief" : "Next task", () => app.ShowResults(arena, sim), Ui.ButtonStyle.Accent, 20);
        Ui.Place(results.GetComponent<RectTransform>(), 24, 1000, CardW, 56);
        results.gameObject.SetActive(false);

        follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);
        follower.StartCoroutine(Deciding());

        sim.OnChanged += Refresh;
        sim.OnTurnResolved += () => follower.StartCoroutine(Turn());
        sim.OnTurnResolved += Announce;
        sim.OnFinished += () =>
        {
            Refresh();
            results.gameObject.SetActive(true);
            pause.interactable = false;
            int home = arena.agents.Count(a => a.alive), all = arena.agents.Count;
            if (home == all) { Sfx.Sting(Sfx.Mood.Fanfare); world.Celebrate(null); Announce("ALL HOME", $"{all} of {all} agents back with dinner", Theme.Teal, 2.4f, true); }
            else if (home == 0) Announce("NOBODY CAME HOME", "every list had a gap in it", Theme.Danger, 2.4f, true);
            else Announce($"{home} OF {all} HOME", "the rest were sent home early", Theme.Amber, 2.2f, true);
        };
        sim.OnChanged += () => { if (sim.phase == "thinking" && !sim.finished) follower.StartCoroutine(Loop()); };
        world.Picked += Choose;
        world.Highlight(arena.agents[0]);
        Refresh();

        // The agents wait at their houses until the countdown has run.
        sim.paused = true;
        follower.StartCoroutine(Countdown());
    }

    // ---- the agent's eye, framed as Claude Bomb frames its camera feed -------------------

    void BuildFeed()
    {
        feed = Ui.Node("Feed", frame);
        Ui.Stretch(feed, 40, 214, 40, 90);
        var line = new Color(1f, 1f, 1f, 0.55f);
        foreach (var (ax, ay) in new[] { (0f, 1f), (1f, 1f), (0f, 0f), (1f, 0f) })
            foreach (bool horizontal in new[] { true, false })
            {
                var stroke = Ui.Box(feed, "Corner", line, false);
                stroke.raycastTarget = false;
                var rt = stroke.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(ax, ay);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = horizontal ? new Vector2(48, 3) : new Vector2(3, 48);
            }
        recDot = Ui.Box(feed, "Rec", Theme.Danger);
        recDot.raycastTarget = false;
        var rr = recDot.rectTransform; rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0, 1); rr.anchoredPosition = new Vector2(22, -22); rr.sizeDelta = new Vector2(14, 14);
        feedLabel = Ui.Text(feed, "", 16, Color.white, true);
        var fl = feedLabel.rectTransform; fl.anchorMin = fl.anchorMax = fl.pivot = new Vector2(0, 1); fl.anchoredPosition = new Vector2(46, -18); fl.sizeDelta = new Vector2(600, 24);
        feedLabel.characterSpacing = 4; feedLabel.enableWordWrapping = false;
        feedSight = Ui.Text(feed, "", 16, new Color(1f, 1f, 1f, 0.75f), false);
        var fs = feedSight.rectTransform; fs.anchorMin = fs.anchorMax = fs.pivot = new Vector2(0, 0); fs.anchoredPosition = new Vector2(22, 16); fs.sizeDelta = new Vector2(600, 24);
        feedSight.characterSpacing = 4; feedSight.enableWordWrapping = false;
        feed.gameObject.SetActive(false);
    }

    void RefreshFeed()
    {
        if (feed == null || !feed.gameObject.activeSelf) return;
        var a = world.Highlighted;
        feedLabel.text = a != null ? $"REC   AGENT CAM  /  {a.Name.ToUpperInvariant()}" : "REC   AGENT CAM";
        feedSight.text = a != null ? $"SIGHT  /  {a.config.vision} CELL{(a.config.vision > 1 ? "S" : "")}" : "";
    }

    // ---- moments -----------------------------------------------------------------------

    public void Announce(string text, string sub, Color colour, float seconds, bool big) => banners.Enqueue((text, sub, colour, seconds, big));

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.3f);
        world.ShowCountdown(world.frame, 2.6f);
        foreach (var n in new[] { "3", "2", "1" }) Announce(n, "", Theme.Teal, 0.5f, true);
        Announce("AGENTS OUT", "nobody can help them now", Theme.Teal, 0.9f, true);
        follower.StartCoroutine(Banners());
        yield return new WaitForSecondsRealtime(2.3f);
        world.BackToOverview();
        sim.paused = false;
        Ui.SetLabel(pause, "Pause");
    }

    IEnumerator Banners()
    {
        while (true)
        {
            if (banners.Count > 0) { var (text, sub, colour, seconds, big) = banners.Dequeue(); showing = true; yield return Moment(text, sub, colour, seconds, big); showing = false; }
            else yield return null;
        }
    }

    static float OutBack(float k) { const float c = 1.70158f; k -= 1f; return 1f + (c + 1f) * k * k * k + c * k * k; }

    // A moment over the market: dark glass, the words in the moment's colour, in with an
    // overshoot; a big one flashes the frame first and carries a line underneath.
    IEnumerator Moment(string text, string sub, Color colour, float seconds, bool big)
    {
        Image flash = null;
        if (big)
        {
            flash = Ui.Box(bannerLayer, "Flash", new Color(colour.r, colour.g, colour.b, 0f), false);
            Ui.Place(flash.rectTransform, 0, 0, WorldW, Ui.H);
            flash.raycastTarget = false;
        }
        var card = Ui.Box(bannerLayer, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        float h = big ? (string.IsNullOrEmpty(sub) ? 150 : 180) : 92;
        float w = big ? 780 : 620;
        Ui.Place(card.rectTransform, (WorldW - w) / 2, 330 - h / 2, w, h); // the upper third: the agent stays in view
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", colour, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, h);
        var t = Ui.Text(card.transform, text, big ? (text.Length <= 2 ? 96 : 56) : 36, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = big ? 6 : 4;
        Ui.Place(t.rectTransform, 20, big ? 14 : 20, w - 40, big ? 100 : 52);
        t.enableWordWrapping = false;
        if (!string.IsNullOrEmpty(sub))
        {
            var st = Ui.Text(card.transform, sub, big ? 19 : 16, Look.Chalk, false, TextAlignmentOptions.Center);
            Ui.Place(st.rectTransform, 20, h - (big ? 52 : 32), w - 40, 26);
            st.enableWordWrapping = false; st.overflowMode = TextOverflowModes.Ellipsis;
        }
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(WorldW / 2, -330);
        if (big) Sfx.Sting(colour == Theme.Danger ? Sfx.Mood.Alarm : text == "AGENTS OUT" ? Sfx.Mood.Go : text.Length <= 1 ? Sfx.Mood.Beep : colour == Theme.Amber ? Sfx.Mood.Alarm : Sfx.Mood.Fanfare); else Sfx.Tick();
        yield return Ui.Animate(big ? 0.34f : 0.2f, k =>
        {
            group.alpha = Mathf.Min(1f, k * 3f);
            float sc = big ? Mathf.LerpUnclamped(0.6f, 1f, OutBack(k)) : Mathf.Lerp(1.12f, 1f, k);
            card.rectTransform.localScale = Vector3.one * sc;
            if (flash != null) flash.color = new Color(colour.r, colour.g, colour.b, 0.45f * Mathf.Max(0f, 1f - k * 2.5f));
        });
        if (flash != null) Object.Destroy(flash.gameObject);
        yield return new WaitForSecondsRealtime(seconds);
        yield return Ui.Animate(0.22f, k => { group.alpha = 1 - k; card.rectTransform.localScale = Vector3.one * (1f - 0.08f * k); });
        Object.Destroy(card.gameObject);
    }

    // The loop's three words light one after the other every turn: LOOK while the
    // observation is written (a ring flashes under the chosen agent), DECIDE while the
    // agents think, MOVE while they walk.
    IEnumerator Loop()
    {
        Light(0);
        world.Beat(world.Highlighted);
        world.LookAround(world.Highlighted);
        yield return new WaitForSecondsRealtime(0.45f);
        if (sim.phase == "thinking") Light(1);
        while (sim.phase == "thinking" && !sim.finished) yield return null;
        Light(2);
        yield return new WaitForSecondsRealtime(0.9f / Mathf.Max(0.5f, sim.speed));
        if (sim.phase != "thinking") Light(-1);
    }

    void Light(int which)
    {
        for (int i = 0; i < 3; i++)
        {
            if (loopChips[i] == null) continue;
            bool on = i == which;
            loopChips[i].color = on ? Theme.Teal : new Color(1f, 1f, 1f, 0.1f);
            loopWords[i].color = on ? Theme.Ink : Look.Chalk;
            loopChips[i].transform.localScale = Vector3.one * (on ? 1.06f : 1f);
        }
    }

    // After every turn: whoever was sent home or ran dry gets a banner, once.
    void Announce()
    {
        foreach (var m in sim.lastMoves)
            if (m.ate != null && !m.agent.alive && announced.Add(m.agent)) Announce("SENT HOME", $"{m.agent.Name} bought {Items.Named(m.ate.id)}: that ruins dinner", Theme.Danger, 1.8f, true);
        foreach (var a in arena.agents)
            if (a.alive && a.outOfTokens && announced.Add(a)) Announce("OUT OF TOKENS", $"{a.Name} has no money left for another turn", Theme.Amber, 1.6f, true);
    }

    // ---- the chosen agent ---------------------------------------------------------------

    void Choose(AgentState a)
    {
        world.Highlight(a);
        RefreshFeed();
        ShowBubble(a, a.lastThought);
        Refresh();
    }

    IEnumerator Turn()
    {
        yield return world.Animate(sim.lastMoves.ToList(), Market.StepSeconds(sim.speed));
        if (world.Highlighted != null && world.Highlighted.alive) ShowBubble(world.Highlighted, world.Highlighted.lastThought);
    }

    // What the chosen agent last texted home, in a bubble over its head.
    void ShowBubble(AgentState a, string text)
    {
        if (bubble != null) Object.Destroy(bubble.gameObject);
        bubble = null;
        bubbleFor = null;
        if (a == null || !a.alive || string.IsNullOrEmpty(text)) return;
        Ui.Bubble(labelsLayer, text, 16, 340, out bubble);
        bubbleFor = a;
        var g = Ui.Group(bubble); g.alpha = 0;
        follower.StartCoroutine(Ui.Fade(g, 1f, 0.2f));
        PlaceLabels();
    }

    // Keeps the names and the bubble on the agents, after the camera has moved.
    void PlaceLabels()
    {
        foreach (var kv in labelOf)
        {
            bool on = world.OnScreen(kv.Key, out var feet, out _);
            kv.Value.gameObject.SetActive(on);
            if (!on) continue;
            var p = app.StagePoint(feet);
            Ui.Place(kv.Value, p.x - 70, p.y + 4, 140, 26);
        }
        if (bubble == null) return;
        if (bubbleFor == null || !world.OnScreen(bubbleFor, out _, out var head)) { bubble.gameObject.SetActive(false); return; }
        bubble.gameObject.SetActive(true);
        var h = app.StagePoint(head);
        float width = bubble.sizeDelta.x, height = bubble.sizeDelta.y;
        float x = Mathf.Clamp(h.x - 30, 24, WorldW - width - 12);
        bool above = h.y - height - 8 >= 210;
        float y = above ? h.y - height - 8 : Mathf.Clamp(h.y + 10, 210, Ui.H - height - 80);
        if (!above) x = Mathf.Clamp(h.x + 90, 24, WorldW - width - 12); // beside the head, not over the agent
        bubble.Find("Tail").gameObject.SetActive(above);
        bubble.Find("Card").GetComponent<RectTransform>().offsetMin = above ? new Vector2(0, 10) : Vector2.zero;
        Ui.Place(bubble, x, y, width, height);
        bubble.Find("Tail").GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Clamp(h.x - x - 9, 16, width - 34), 12);
    }

    // Lives on the screen's object, so it goes when the screen goes: keeps the
    // labels on the agents after every camera move, runs the screen's coroutines,
    // and lets go of the world's events at the end.
    class Follower : MonoBehaviour
    {
        ArenaScreen screen;
        public void Bind(ArenaScreen s) { screen = s; s.app.view.Framed += s.PlaceLabels; }
        void OnDestroy() { if (screen != null) { screen.app.view.Framed -= screen.PlaceLabels; screen.world.Picked -= screen.Choose; } }
    }

    void BuildCards()
    {
        int n = arena.agents.Count;
        float gap = 10, h = Mathf.Min(190, (cardsHeight - gap * (n - 1)) / n);
        bool compact = h < 150;
        for (int i = 0; i < n; i++)
        {
            var a = arena.agents[i];
            var card = new Card();
            card.outer = Ui.Card(cards, "Card", Theme.Panel, Theme.Border1);
            Ui.Place(card.outer.rectTransform, 0, i * (h + gap), CardW, h);
            card.fill = card.outer.transform.Find("Fill").GetComponent<Image>();
            var t = card.outer.transform;
            var btn = card.outer.gameObject.AddComponent<Button>();
            btn.targetGraphic = card.outer;
            var agent = a;
            btn.onClick.AddListener(() => Choose(agent));

            var dot = Ui.Dot(t, Theme.Team[a.config.colour], 18);
            Ui.Place(dot.rectTransform, 18, 18, 18, 18);
            var name = Ui.Text(t, a.Name + (a.config.IsRule ? "  ·  a fixed rule, no AI" : ""), compact ? 20 : 22, Theme.Ink, true);
            Ui.Place(name.rectTransform, 46, 10, 370, 30);
            name.enableWordWrapping = false; name.overflowMode = TextOverflowModes.Ellipsis;
            card.score = Ui.Text(t, "0", compact ? 30 : 40, Theme.Ink, true, TextAlignmentOptions.TopRight);
            Ui.Place(card.score.rectTransform, CardW - 110, 4, 92, 50);
            var basket = Ui.Icon(t, "basket", compact ? 26 : 30);
            Ui.Place(basket.rectTransform, CardW - 150, 12, compact ? 26 : 30, compact ? 26 : 30);
            card.status = Ui.Text(t, "", 16, Theme.Muted);
            Ui.Place(card.status.rectTransform, 46, compact ? 38 : 44, CardW - 170, 24);
            card.status.enableWordWrapping = false; card.status.overflowMode = TextOverflowModes.Ellipsis;
            card.bar = Ui.Bar(t, Theme.Border, Theme.Aqua, 180, 8);
            Ui.Place(card.bar.track.rectTransform, CardW - 198, h - 24, 180, 8);
            card.tokens = Ui.Text(t, "", 16, Theme.Muted, false, TextAlignmentOptions.TopRight);
            Ui.Place(card.tokens.rectTransform, CardW - 290, h - 50, 272, 22);
            card.tokens.enableWordWrapping = false;
            card.thought = Ui.Text(t, "", 16, Theme.Text2);
            Ui.Place(card.thought.rectTransform, 46, compact ? 64 : 74, CardW - 64, compact ? h - 116 : h - 128);
            card.thought.overflowMode = TextOverflowModes.Ellipsis;
            var know = Ui.Button(t, "What it knows", () => ShowKnows(agent), Ui.ButtonStyle.Secondary, 16);
            Ui.Place(know.GetComponent<RectTransform>(), 40, h - 48, 170, 36);
            cardOf[a] = card;
        }
    }

    // The drawer folds to a tab and the world takes the whole stage; open again for the cards.
    void Fold(bool fold)
    {
        folded = fold;
        float from = drawer.anchoredPosition.x, to = fold ? Ui.W - 56f : Ui.W - DrawerW;
        follower.StartCoroutine(Ui.Animate(0.3f, k =>
        {
            if (drawer == null) return;
            drawer.anchoredPosition = new Vector2(Mathf.Lerp(from, to, k), drawer.anchoredPosition.y);
            Ui.Place(frame, 0, 0, Mathf.Lerp(Ui.W - (fold ? DrawerW : 56f), WorldW, k), Ui.H);
        }, Ui.EaseOut));
    }

    // The whole text handed to Claude for this agent's last turn, in the drawer over the
    // cards: the list and the rules, what it remembers, and this turn's view.
    void ShowKnows(AgentState a)
    {
        if (knows != null) Object.Destroy(knows.gameObject);
        if (folded) Fold(false);
        knows = Ui.Node("Knows", drawer);
        Ui.Place(knows, 0, 0, DrawerW, Ui.H);
        var card = Ui.Box(knows, "Card", new Color(0.03f, 0.055f, 0.13f, 0.98f), false);
        Ui.Stretch(card.rectTransform);
        card.raycastTarget = true;
        var t = card.transform;
        var title = Ui.Text(t, $"What {a.Name} knows right now", 24, Theme.Ink, true);
        Ui.Place(title.rectTransform, 24, 24, 420, 64);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(knows.gameObject); knows = null; }, Ui.ButtonStyle.Secondary, 16);
        Ui.Place(close.GetComponent<RectTransform>(), DrawerW - 24 - 100, 24, 100, 40);
        var sub = Ui.Text(t, a.config.IsRule ? "A fixed rule reads nothing. It takes exactly the things it names." : "This is the whole text handed to Claude for the last turn. Nothing else reaches the agent.", 16, Theme.Text2);
        Ui.Place(sub.rectTransform, 24, 92, CardW, 48);
        var scroll = Ui.Scroll(t, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 24, 150, CardW, Ui.H - 174);
        string text = a.config.IsRule ? a.config.instructions : KnowsText(a);
        var body = Ui.Text(content, text, 16, Theme.Ink);
        float h = body.GetPreferredValues(text, CardW - 16, 0).y + 20;
        Ui.Place(body.rectTransform, 0, 0, CardW - 16, h);
        body.rectTransform.anchorMin = new Vector2(0, 1); body.rectTransform.anchorMax = new Vector2(1, 1);
        content.sizeDelta = new Vector2(0, h);
    }

    static string KnowsText(AgentState a)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("THE LIST AND THE RULES (the same every turn)\n\n").Append(Observation.SystemPrompt(a));
        long memoryTokens = a.history.Sum(h => Observation.EstimateTokens(h.user + h.assistant));
        sb.Append("\n\n\nWHAT IT REMEMBERS");
        if (a.history.Count == 0 && string.IsNullOrWhiteSpace(a.notebook)) sb.Append("\n\nNothing. Every turn starts from the list and this turn's view.");
        else
        {
            if (!string.IsNullOrWhiteSpace(a.notebook)) sb.Append("\n\nIts notebook: ").Append(a.notebook.Trim());
            if (a.history.Count > 0)
            {
                sb.Append($"\n\n{a.history.Count} remembered turn{(a.history.Count == 1 ? "" : "s")}, about {memoryTokens:N0} tokens riding along every turn:");
                foreach (var h in a.history)
                {
                    string seen = h.user.Length > 220 ? h.user.Substring(0, 219) + "…" : h.user;
                    sb.Append("\n\n> ").Append(seen.Trim().Replace("\n", "\n> ")).Append("\n< ").Append(h.assistant.Trim());
                }
            }
        }
        sb.Append("\n\n\nTHIS TURN\n\n").Append(string.IsNullOrEmpty(a.lastObservation) ? "(the errand has not started)" : a.lastObservation);
        return sb.ToString();
    }

    // While the agents are deciding, the phase line breathes.
    IEnumerator Deciding()
    {
        int dots = 0;
        while (true)
        {
            if (recDot != null) recDot.color = new Color(Theme.Danger.r, Theme.Danger.g, Theme.Danger.b, Time.unscaledTime % 1.2f < 0.7f ? 1f : 0.15f);
            if (sim.phase == "thinking" && !sim.finished && phaseText != null)
            {
                dots = (dots + 1) % 4;
                phaseText.text = phaseBase + new string('.', dots);
            }
            yield return new WaitForSecondsRealtime(0.35f);
        }
    }

    // LUMA's line at the foot of the column: the lead of what she has to say, typed as she
    // says it, gone once she is done. The whole of it is on the debrief and in the file.
    void SayLine(string text, bool stay = false)
    {
        if (coachLine != null) Object.Destroy(coachLine.gameObject);
        coachLine = app.luma.Line(drawer, 24, 800, CardW, Labels.Lead(text), false, stay, 16);
    }

    // The first time each kind of thing happens, LUMA names it in the words of
    // the lesson: an agent sent home first, then the budget, a lost turn, a buy.
    // On the first errand the first turn is read out as one loop first.
    void Watch()
    {
        if (knows != null) { Object.Destroy(knows.gameObject); knows = null; }
        if (firstErrand)
        {
            var who = world.Highlighted != null && world.Highlighted.Active ? world.Highlighted : arena.agents.FirstOrDefault(x => x.Active);
            if (sim.turn == 1 && who != null) { SayLine(Coach.Step(who, 1)); return; }
            if (sim.turn == 2) { firstErrand = false; SayLine(app.luma.Say("live-1")); return; }
        }
        var dead = sim.lastMoves.FirstOrDefault(m => m.ate != null && !m.agent.alive);
        if (dead != null && !saidOut) { saidOut = true; SayLine(Coach.Eliminated(dead.agent)); return; }
        var broke = arena.agents.FirstOrDefault(a => a.outOfTokens);
        if (broke != null && !saidTokens) { saidTokens = true; SayLine(Coach.OutOfTokens(broke, sim.turn)); return; }
        var confused = arena.agents.FirstOrDefault(a => a.confusedTurns > 0);
        if (confused != null && !saidConfused) { saidConfused = true; SayLine(Coach.Confused(confused)); return; }
        var scored = sim.lastMoves.FirstOrDefault(m => m.ate != null && m.agent.alive);
        if (scored != null && !saidScore) { saidScore = true; SayLine(Coach.Scored(scored.agent, scored.ate)); }
    }

    void Refresh()
    {
        if (world.Highlighted == null || !world.Highlighted.alive)
        {
            var next = arena.agents.FirstOrDefault(x => x.Active) ?? arena.agents.FirstOrDefault(x => x.alive);
            if (next != null) world.Highlight(next);
        }
        world.thinking = sim.phase == "thinking" && !sim.finished;
        int shown = Mathf.Clamp(sim.turn, 0, sim.totalTurns);
        turnText.text = sim.finished ? $"Home after {shown} turns" : $"Turn {shown} of {sim.totalTurns}";
        int alive = arena.agents.Count(a => a.alive), running = arena.agents.Count(a => a.Active);
        phaseBase = sim.finished ? $"{alive} of {arena.agents.Count} agents came home with dinner."
                  : sim.phase == "thinking" ? $"{running} agent{(running == 1 ? " is" : "s are")} looking at the stalls and deciding"
                  : "Walking.";
        phaseText.text = phaseBase;
        homeText.text = $"{alive} / {arena.agents.Count}";
        basketText.text = arena.agents.Sum(a => a.score).ToString();
        long left = arena.agents.Where(a => !a.config.IsRule).Sum(a => a.TokensLeft);
        tokensText.text = left >= 1000 ? $"{left / 1000f:0.0}k" : left.ToString();
        foreach (var a in arena.agents)
        {
            var c = cardOf[a];
            bool hi = world.Highlighted == a;
            c.outer.color = hi ? Theme.Team[a.config.colour] : Theme.Border1;
            c.fill.color = !a.alive ? Theme.DangerSoft : a.outOfTokens ? Theme.AmberSoft : Theme.Panel;
            c.score.text = a.score.ToString();
            c.score.color = a.alive ? Theme.Chalk : Theme.Muted;
            c.status.text = a.Status + (a.Active && !a.config.IsRule ? $"  ·  {Models.All[a.config.model].label}, sees {a.config.vision}, {MemoryModes.Labels[a.config.memory].ToLowerInvariant()} memory" : "");
            c.status.color = !a.alive ? Theme.DangerText : a.outOfTokens ? Theme.Amber : Theme.Muted;
            Ui.SetBar(c.bar, a.budget > 0 ? (float)a.TokensLeft / a.budget : 0);
            c.bar.fill.color = a.TokensLeft < a.budget * 0.2f ? Theme.Amber : Theme.Aqua;
            c.tokens.text = a.config.IsRule ? "0 tokens: no AI" : $"{a.TokensLeft:N0} tokens left";
            if (c.thought != null) c.thought.text = a.lastThought == "" ? "" : "“" + a.lastThought + "”";
        }
        RefreshFeed();
    }
}
