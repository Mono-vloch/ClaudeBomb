using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The errand, live (the game-feel pass): the street behind the chosen agent with the market
// ahead of it. The world shows the state: the battery on its hip drains, its thought is a
// small bubble in plain words, the clock on the square and at the top runs toward dinner, the
// family's window at the top right shows one face per agent. Three controls (the menu, the
// camera, Call home) and, when it is over, Debrief. Banners mark the big moments; LUMA says
// one short thing when something happens.
public class ArenaScreen
{
    public static ArenaScreen Current { get; private set; }
    public readonly RectTransform root;
    readonly App app;
    readonly Arena arena;
    readonly Simulation sim;
    readonly Market world;
    readonly RectTransform frame, labelsLayer, bannerLayer;
    readonly TextMeshProUGUI objective;
    readonly Hud.Clock clock;
    readonly Hud.Window window;
    readonly Hud.LoopIcons loop;
    readonly Button camera, callHome, results;
    readonly Follower follower;
    RectTransform feed, bubble, thoughtCard;
    TextMeshProUGUI feedLabel;
    Image recDot;
    AgentState bubbleFor;
    string bubbleFull = "";
    int viewIndex;
    readonly int turns;
    readonly HashSet<AgentState> announced = new HashSet<AgentState>();
    readonly Queue<(string text, string sub, Color colour, float seconds, bool big)> banners = new Queue<(string, string, Color, float, bool)>();
    bool showing, saidScore, saidOut, saidOff, saidTokens;
    readonly bool firstErrand;
    public bool Started { get; private set; }
    public string CurrentBanner { get; private set; } = "";
    public bool BannerUp => showing || banners.Count > 0;
    public bool CalledHome { get; private set; }

    public ArenaScreen(Transform parent, App app, Arena arena, Simulation sim)
    {
        this.app = app; this.arena = arena; this.sim = sim;
        Current = this;
        root = Ui.Node("Arena", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        turns = app.decide != null ? 10 : app.turns;
        firstErrand = app.decide == null && app.lesson != null && app.lesson.number == 1;

        Market.MemoryCards = app.decide == null && (app.lesson == null || app.lesson.memory);
        world = app.world;
        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        world.Build(arena, false);
        world.BatteriesFor(turns);
        if (Market.Real) { world.Highlight(arena.agents[0]); world.ShowStreet(frame); } else world.ShowMarket(frame);
        sim.Holding = () => world.Animating; // a real walk takes real time

        objective = Hud.Objective(root, app.decide != null ? "A fixed rule against an agent." : SetupScreen.Objective(app));
        clock = new Hud.Clock(root, Ui.W / 2 - 46, 20);
        window = new Hud.Window(root, arena.agents, Ui.W - 40 - 52 - 24, 20, false);
        window.OnPick = Choose;
        if (firstErrand) loop = new Hud.LoopIcons(root, Ui.W / 2 - 74, 124);
        GameMenu.Icon(root);
        camera = Hud.Small(root, "Camera", 40, 120, CycleView);
        callHome = Hud.Small(root, "Call home", 172, 140, () => { if (!sim.finished) { CalledHome = true; sim.Abort(); } });
        bool lastTask = app.decide == null || app.decide.RunNumber >= Decide.ShoppingTasks;
        results = Hud.Main(root, lastTask ? "Debrief" : "Next task", () => app.ShowResults(arena, sim));
        results.gameObject.SetActive(false);

        labelsLayer = Ui.Node("Labels", root);
        Ui.Stretch(labelsLayer);
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);
        BuildFeed();

        follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);
        sim.OnChanged += Refresh;
        sim.OnTurnResolved += () => follower.StartCoroutine(Turn());
        sim.OnTurnResolved += Announce;
        sim.OnTurnResolved += Watch;
        sim.OnFinished += Finished;
        sim.OnChanged += () => { if (sim.phase == "thinking" && !sim.finished) follower.StartCoroutine(Loop()); };
        world.Picked += Choose;
        world.Highlight(arena.agents[0]);
        Refresh();

        // The agents wait at their houses until the countdown has run.
        sim.paused = true;
        follower.StartCoroutine(Countdown());
    }

    // ---- the agent's eye, framed as a camera feed ----

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
        feed.gameObject.SetActive(false);
    }

    void RefreshFeed()
    {
        if (feed == null || !feed.gameObject.activeSelf) return;
        var a = world.Highlighted;
        feedLabel.text = a != null ? $"REC   AGENT CAM  /  {a.Name.ToUpperInvariant()}" : "REC   AGENT CAM";
    }

    // The camera: the street, the agent's eye, the market from above.
    void CycleView() { SelectView((viewIndex + 1) % 3); }

    public void SelectView(int i)
    {
        viewIndex = i;
        var v = i == 0 ? Market.View.Street : i == 1 ? Market.View.RobotsEye : Market.View.Market;
        if (!Market.Real && v == Market.View.Street) v = Market.View.Market;
        world.SetView(v);
        if (feed != null) feed.gameObject.SetActive(v == Market.View.RobotsEye);
        RefreshFeed();
    }

    // ---- moments ----

    public void Announce(string text, string sub, Color colour, float seconds, bool big) => banners.Enqueue((text, sub, colour, seconds, big));

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.3f);
        world.ShowCountdown(world.frame, 2.6f);
        foreach (var n in new[] { "3", "2", "1" }) Announce(n, "", Theme.Teal, 0.5f, true);
        Announce(arena.agents.Count > 1 ? "AGENTS OUT" : "AGENT OUT", "", Theme.Teal, 0.9f, true);
        follower.StartCoroutine(Banners());
        yield return new WaitForSecondsRealtime(2.3f);
        world.BackToOverview();
        sim.paused = false;
        Started = true;
    }

    IEnumerator Banners()
    {
        while (true)
        {
            if (banners.Count > 0)
            {
                var (text, sub, colour, seconds, big) = banners.Dequeue();
                showing = true; CurrentBanner = text;
                yield return Hud.Moment(bannerLayer, text, sub, colour, seconds, big);
                showing = false; CurrentBanner = "";
            }
            else yield return null;
        }
    }

    // The loop on the agent itself: it looks on LOOK, the antenna pulses on DECIDE, it walks on
    // MOVE; on the first errand three small icons light in turn under the clock.
    IEnumerator Loop()
    {
        loop?.Light(0);
        world.Beat(world.Highlighted);
        world.LookAround(world.Highlighted);
        yield return new WaitForSecondsRealtime(0.45f);
        if (sim.phase == "thinking") loop?.Light(1);
        while (sim.phase == "thinking" && !sim.finished) yield return null;
        loop?.Light(2);
        yield return new WaitForSecondsRealtime(0.9f / Mathf.Max(0.5f, sim.speed));
        if (sim.phase != "thinking") loop?.Light(-1);
    }

    // After every turn: whoever was sent home or ran dry gets a banner, once.
    void Announce()
    {
        foreach (var m in sim.lastMoves)
            if (m.ate != null && !m.agent.alive && announced.Add(m.agent)) Announce("SENT HOME", $"{m.agent.Name} bought {Items.Named(m.ate.id)}.", Theme.Danger, 1.8f, true);
        foreach (var a in arena.agents)
            if (a.alive && a.outOfTokens && announced.Add(a)) Announce("OUT OF TOKENS", a.Name, Theme.Amber, 1.6f, true);
    }

    // LUMA, one short thing when something happens, in the words of the lesson.
    void Watch()
    {
        if (app.decide != null)
        {
            if (sim.turn == 1 && app.decide.RunNumber == 1) app.luma.Cue(root, "round-6");
            return;
        }
        var l = app.lesson;
        if (sim.turn == 1)
        {
            if (l == null || l.number == 1) app.luma.Cue(root, "cue-nobody");
            else if (Luma.Lines.ContainsKey("round-" + l.number)) app.luma.Cue(root, "round-" + l.number);
            return;
        }
        var dead = sim.lastMoves.FirstOrDefault(m => m.ate != null && !m.agent.alive);
        if (dead != null && !saidOut) { saidOut = true; follower.StartCoroutine(CueLater(Luma.FirstSentence(Coach.Eliminated(dead.agent)), 2.6f)); return; }
        var broke = arena.agents.FirstOrDefault(a => a.outOfTokens);
        if (broke != null && !saidTokens) { saidTokens = true; follower.StartCoroutine(CueLater(Luma.FirstSentence(Coach.OutOfTokens(broke, sim.turn)), 2.4f)); return; }
        if (l != null && l.number == 1 && !saidOff && !CalledHome && sim.turn >= sim.totalTurns / 2) { saidOff = true; app.luma.Cue(root, "cue-off"); return; }
        var scored = sim.lastMoves.FirstOrDefault(m => m.ate != null && m.agent.alive);
        if (scored != null && !saidScore && (l == null || l.number == 1)) { saidScore = true; var line = Luma.FirstSentence(Coach.Scored(scored.agent, scored.ate)); if (line != null) app.luma.Text(root, line); }
    }

    IEnumerator CueLater(string text, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (root != null && text != null) app.luma.Text(root, text);
    }

    void Finished()
    {
        Refresh();
        results.gameObject.SetActive(true);
        camera.gameObject.SetActive(false);
        callHome.gameObject.SetActive(false);
        if (thoughtCard != null) { Object.Destroy(thoughtCard.gameObject); thoughtCard = null; }
        if (bubble != null) { Object.Destroy(bubble.gameObject); bubble = null; bubbleFor = null; }
        int home = arena.agents.Count(a => a.alive), all = arena.agents.Count;
        // The result stays up until the debrief: what the basket holds, and whether it came home.
        int bought = arena.agents.Where(a => a.alive).Sum(a => a.score);
        string sub = bought > 0 ? $"{bought} on the list in the basket" : "";
        const float Stay = 100000f;
        if (CalledHome) Announce("CALLED HOME", sub, Theme.Amber, Stay, true);
        else if (home == all) { Sfx.Sting(Sfx.Mood.Fanfare); world.Celebrate(null); Announce(all > 1 ? "ALL HOME" : "HOME", sub, Theme.Teal, Stay, true); }
        else if (home == 0) Announce(all > 1 ? "NOBODY CAME HOME" : "SENT HOME", "", Theme.Danger, Stay, true);
        else Announce($"{home} OF {all} HOME", sub, Theme.Amber, Stay, true);
    }

    // ---- the chosen agent ----

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

    // Claude's thought, as the player sees it: the first sentence, the coordinates and the
    // brain's name taken out. What Claude wrote and what the agent reads are unchanged; the
    // whole of it is one tap away.
    public static string Short(string thought)
    {
        if (string.IsNullOrWhiteSpace(thought)) return "";
        string s = Regex.Replace(thought, @"^\s*(Stand-in brain|Rule)\s*:\s*", "", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\s+(is\s+)?at\s*\(\s*-?\d+\s*,\s*-?\d+\s*\)", "");
        s = Regex.Replace(s, @"\s*\(\s*-?\d+\s*,\s*-?\d+\s*\)", "");
        s = Regex.Replace(s, @"\s{2,}", " ").Trim();
        var parts = Regex.Split(s, @"(?<=[.!?])\s+");
        if (parts.Length > 0)
        {
            s = parts[0];
            // A first sentence left too short by the cut keeps the next one: "Nearest apple. Going there."
            if (Luma.Words(s) < 3 && parts.Length > 1) s += " " + parts[1];
        }
        if (s.Length > 0) s = char.ToUpperInvariant(s[0]) + s.Substring(1);
        return s;
    }

    void ShowBubble(AgentState a, string text)
    {
        if (bubble != null) Object.Destroy(bubble.gameObject);
        bubble = null; bubbleFor = null;
        if (thoughtCard != null) { Object.Destroy(thoughtCard.gameObject); thoughtCard = null; }
        if (a == null || !a.alive || string.IsNullOrEmpty(text)) return;
        bubbleFull = text;
        string shown = Short(text);
        if (shown.Length == 0) return;
        var t = Ui.Bubble(labelsLayer, shown, 16, 300, out bubble);
        bubbleFor = a;
        var card = bubble.Find("Card").GetComponent<Image>();
        card.raycastTarget = true;
        card.gameObject.AddComponent<Hud.Tap>().act = ToggleThought;
        var g = Ui.Group(bubble); g.alpha = 0;
        follower.StartCoroutine(Ui.Fade(g, 1f, 0.2f));
        PlaceBubble();
    }

    // The whole thought, in a card beside the bubble; tapped away.
    void ToggleThought()
    {
        if (thoughtCard != null) { Object.Destroy(thoughtCard.gameObject); thoughtCard = null; return; }
        if (bubble == null) return;
        var card = Ui.Box(labelsLayer, "Thought", Ui.DarkFill);
        var body = Ui.Text(card.transform, bubbleFull, 18, Color.white);
        float h = body.GetPreferredValues(bubbleFull, 520, 0).y + 40;
        Ui.Place(body.rectTransform, 20, 20, 520, h - 40);
        float x = Mathf.Clamp(bubble.anchoredPosition.x, 40, Ui.W - 600), y = Mathf.Clamp(-bubble.anchoredPosition.y + bubble.sizeDelta.y + 10, 100, Ui.H - h - 100);
        Ui.Place(card.rectTransform, x, y, 560, h);
        card.raycastTarget = true;
        card.gameObject.AddComponent<Hud.Tap>().act = ToggleThought;
        thoughtCard = card.rectTransform;
        Sfx.Tick();
    }

    void PlaceBubble()
    {
        if (bubble == null) return;
        if (bubbleFor == null || !world.OnScreen(bubbleFor, out _, out var head)) { bubble.gameObject.SetActive(false); return; }
        bubble.gameObject.SetActive(true);
        var h = app.StagePoint(head);
        float width = bubble.sizeDelta.x, height = bubble.sizeDelta.y;
        float x = Mathf.Clamp(h.x - 30, 24, Ui.W - width - 24);
        bool above = h.y - height - 8 >= 100;
        float y = above ? h.y - height - 8 : Mathf.Clamp(h.y + 10, 100, Ui.H - height - 80);
        if (!above) x = Mathf.Clamp(h.x + 90, 24, Ui.W - width - 24);
        bubble.Find("Tail").gameObject.SetActive(above);
        bubble.Find("Card").GetComponent<RectTransform>().offsetMin = above ? new Vector2(0, 10) : Vector2.zero;
        Ui.Place(bubble, x, y, width, height);
        bubble.Find("Tail").GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Clamp(h.x - x - 9, 16, width - 34), 12);
    }

    class Follower : MonoBehaviour
    {
        ArenaScreen screen;
        public void Bind(ArenaScreen s) { screen = s; s.app.view.Framed += s.PlaceBubble; }
        void OnDestroy() { if (screen != null) { screen.app.view.Framed -= screen.PlaceBubble; screen.world.Picked -= screen.Choose; if (Current == screen) Current = null; } }
    }

    void Refresh()
    {
        if (world.Highlighted == null || !world.Highlighted.alive)
        {
            var next = arena.agents.FirstOrDefault(x => x.Active) ?? arena.agents.FirstOrDefault(x => x.alive);
            if (next != null) world.Highlight(next);
        }
        world.thinking = sim.phase == "thinking" && !sim.finished;
        int shown = Mathf.Clamp(sim.turn, 0, turns);
        float fraction = turns > 0 ? (float)shown / turns : 0f;
        bool amber = turns - shown <= 5 && !sim.finished;
        clock.Set(fraction, amber);
        world.SetClock(fraction, amber);
        foreach (var a in arena.agents)
        {
            var state = !a.alive ? Hud.Face.SentHome : sim.finished ? Hud.Face.Home : Hud.Face.Out;
            window.Set(a, state, world.Highlighted == a);
            world.SetBattery(a, a.config.IsRule ? 1f : a.budget > 0 ? (float)a.TokensLeft / a.budget : 0f);
        }
        if (recDot != null) recDot.color = new Color(Theme.Danger.r, Theme.Danger.g, Theme.Danger.b, Time.unscaledTime % 1.2f < 0.7f ? 1f : 0.15f);
        RefreshFeed();
    }
}
