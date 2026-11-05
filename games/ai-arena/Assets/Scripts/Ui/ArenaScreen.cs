using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The errand, live: the market in three dimensions on the left, the mission and
// one card per agent on the right with its basket, its pocket money draining and
// the last thing it texted home, and LUMA underneath. A countdown sends the
// agents out; banners mark the big moments (sent home, out of tokens, home). The
// market can be watched from above or through the chosen agent's eyes. On the
// first errand the loop runs one turn at a time.
public class ArenaScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Arena arena;
    readonly Simulation sim;
    readonly Market world;
    readonly TextMeshProUGUI turnText, phaseText;
    readonly Button pause, results;
    readonly RectTransform cards, labelsLayer, bannerLayer;
    readonly float cardsHeight;
    readonly Dictionary<AgentState, Card> cardOf = new Dictionary<AgentState, Card>();
    readonly Dictionary<AgentState, RectTransform> labelOf = new Dictionary<AgentState, RectTransform>();
    readonly HashSet<AgentState> announced = new HashSet<AgentState>();
    readonly Queue<(string text, Color colour, float seconds)> banners = new Queue<(string, Color, float)>();
    readonly Follower follower;
    TextMeshProUGUI coach;
    RectTransform knows, bubble;
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
        root = Ui.Node("Arena", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear); // the market shows through

        // The market, behind everything, in the left part of the stage.
        world = app.world;
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 40, 104, 940, 920);
        world.Build(arena, false);
        world.ShowMarket(frame);

        // The cards sit on frosted glass over the park; so does the header.
        var column = Ui.Box(root, "Column", new Color(1, 1, 1, 0.84f), false);
        Ui.Place(column.rectTransform, 1000, -300, 1600, 1700);
        column.raycastTarget = false;
        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.8f));
        Ui.Place(top.rectTransform, 40, 16, 950, 82);
        top.raycastTarget = false;

        turnText = Ui.Text(root, "", 30, Theme.Ink, true);
        Ui.Place(turnText.rectTransform, 60, 24, 360, 40);
        turnText.enableWordWrapping = false;
        phaseText = Ui.Text(root, "", 16, Theme.Text2);
        Ui.Place(phaseText.rectTransform, 60, 62, 400, 26);

        var speedLabel = Ui.Text(root, "Speed", 14, Theme.Text2, false, TextAlignmentOptions.MidlineRight);
        Ui.Place(speedLabel.rectTransform, 436, 36, 60, 44);
        var speed = new Ui.Segmented(root, new[] { "1x", "2x", "4x" }, 1, 168, 44, 15);
        Ui.Place(speed.root, 504, 36, 168, 44);
        speed.OnSelect = i => sim.speed = i == 0 ? 1 : i == 1 ? 2 : 4;

        pause = Ui.Button(root, "Pause", () => { sim.paused = !sim.paused; Ui.SetLabel(pause, sim.paused ? "Resume" : "Pause"); }, Ui.ButtonStyle.Secondary, 15);
        Ui.Place(pause.GetComponent<RectTransform>(), 684, 36, 100, 44);
        var stop = Ui.Button(root, "Call them home", () => { if (!sim.finished) sim.Abort(); }, Ui.ButtonStyle.Quiet, 15);
        Ui.Place(stop.GetComponent<RectTransform>(), 790, 36, 190, 44);

        // The view: the whole market, or what the chosen agent sees.
        var viewToggle = new Ui.Segmented(root, new[] { "The market", "Agent's eye" }, 0, 300, 40, 14);
        Ui.Place(viewToggle.root, 40, 1032, 300, 40);
        viewToggle.OnSelect = i => world.SetView(i == 0 ? Market.View.Market : Market.View.RobotsEye);
        var viewHint = Ui.Text(root, "Agent's eye: from behind the chosen agent. A stall it cannot see is a gray box, because for the agent it does not exist.", 13, Theme.Text2);
        Ui.Place(viewHint.rectTransform, 352, 1034, 630, 40);

        // The agents' names, under their feet; the bubble over the chosen one.
        labelsLayer = Ui.Node("Labels", root);
        Ui.Stretch(labelsLayer);
        foreach (var a in arena.agents)
        {
            var label = Ui.Box(labelsLayer, "Label " + a.Name, new Color(1, 1, 1, 0.88f));
            label.raycastTarget = false;
            var t = Ui.Text(label.transform, a.Name, 14, Theme.Ink, true, TextAlignmentOptions.Center);
            Ui.Stretch(t.rectTransform, 6, 0, 6, 0);
            t.enableWordWrapping = false;
            labelOf[a] = label.rectTransform;
        }
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);

        // The mission, at the top of the column.
        var mission = Ui.Soft(root, "Mission");
        Ui.Place(mission.rectTransform, 1020, 28, 840, 58);
        string where = app.decide != null ? $"TASK {app.decide.RunNumber} OF {Decide.ShoppingTasks}" : app.lesson != null ? $"ERRAND {app.lesson.id}  ·  {app.StepName.ToUpperInvariant()}" : "FREE PLAY";
        var missionTag = Ui.Eyebrow(mission.transform, $"MISSION  ·  {arena.brief.from.ToUpperInvariant()}  ·  {where}", Theme.TealDeep);
        Ui.Place(missionTag.rectTransform, 16, 8, 800, 18);
        var missionText = Ui.Text(mission.transform, arena.brief.rule, 15, Theme.Ink);
        Ui.Place(missionText.rectTransform, 16, 28, 808, 24);
        missionText.enableWordWrapping = false;
        missionText.overflowMode = TextOverflowModes.Ellipsis;

        // LUMA gets the bottom of the column when the cards leave room for her.
        bool withCoach = arena.agents.Count <= 6;
        cardsHeight = withCoach ? 760 : 930;
        cards = Ui.Node("Cards", root);
        Ui.Place(cards, 1020, 98, 840, cardsHeight);
        BuildCards();
        if (withCoach)
        {
            firstErrand = app.decide == null && app.lesson != null && app.lesson.number == 1 && app.step == App.Step.YourTurn;
            var setup = app.CurrentSetup;
            string opening = app.decide != null
                ? $"Task {app.decide.RunNumber} of {Decide.ShoppingTasks}: \"{arena.brief.rule}\" The gray agent is a fixed rule: no AI, no tokens. The teal one is an agent with a list. Watch the basket and the cost."
                : firstErrand ? "Watch your agent. After its first turn I will read out the loop it just ran."
                : setup != null && app.step == App.Step.SeeItWork ? "Watch the pro. " + setup.why
                : setup != null && app.step == App.Step.FixIt ? "Fix it: " + setup.name + ". " + setup.why
                : app.luma.Say(app.lesson != null ? "live-" + app.lesson.number : "free");
            coach = Luma.Card(root, 1020, 872, 840, 148, opening, 16, null, app.luma);
            sim.OnTurnResolved += Watch;
        }

        bool lastTask = app.decide == null || app.decide.RunNumber >= Decide.ShoppingTasks;
        results = Ui.Button(root, lastTask ? "See the debrief" : "Next task", () => app.ShowResults(arena, sim), Ui.ButtonStyle.Primary, 19);
        Ui.Place(results.GetComponent<RectTransform>(), 1620, 1026, 240, 46);
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
            banners.Enqueue((home == all ? "ALL HOME" : home == 0 ? "NOBODY CAME HOME" : $"{home} OF {all} HOME", home == all ? Theme.TealDeep : home == 0 ? Theme.Danger : Theme.Ink, 2.2f));
        };
        world.Picked += Choose;
        world.Highlight(arena.agents[0]);
        Refresh();
        Ui.Emblem(root);

        // The agents wait at their houses until the countdown has run.
        sim.paused = true;
        follower.StartCoroutine(Countdown());
    }

    // ---- banners -----------------------------------------------------------------------

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.4f);
        foreach (var n in new[] { "3", "2", "1" }) yield return Banner(n, Theme.Ink, 0.5f);
        yield return Banner("AGENTS OUT", Theme.TealDeep, 0.9f);
        sim.paused = false;
        Ui.SetLabel(pause, "Pause");
        follower.StartCoroutine(Banners());
    }

    IEnumerator Banners()
    {
        while (true)
        {
            if (banners.Count > 0) { var (text, colour, seconds) = banners.Dequeue(); yield return Banner(text, colour, seconds); }
            else yield return null;
        }
    }

    // A big word across the market, in and out.
    IEnumerator Banner(string text, Color colour, float seconds)
    {
        var card = Ui.Box(bannerLayer, "Banner", new Color(1, 1, 1, 0.92f));
        Ui.Place(card.rectTransform, 110, 470, 820, 120);
        card.raycastTarget = false;
        var t = Ui.Text(card.transform, text, 56, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = 4;
        Ui.Stretch(t.rectTransform);
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(110 + 410, -(470 + 60));
        yield return Ui.Animate(0.18f, k => { group.alpha = k; card.rectTransform.localScale = Vector3.one * (1.15f - 0.15f * k); }, Ui.EaseOut);
        yield return new WaitForSecondsRealtime(seconds);
        yield return Ui.Animate(0.25f, k => group.alpha = 1 - k);
        Object.Destroy(card.gameObject);
    }

    // After every turn: whoever was sent home or ran dry gets a banner, once.
    void Announce()
    {
        foreach (var m in sim.lastMoves)
            if (m.ate != null && !m.agent.alive && announced.Add(m.agent)) banners.Enqueue(($"SENT HOME  ·  {m.agent.Name}", Theme.Danger, 1.4f));
        foreach (var a in arena.agents)
            if (a.alive && a.outOfTokens && announced.Add(a)) banners.Enqueue(($"OUT OF TOKENS  ·  {a.Name}", Theme.Amber, 1.4f));
    }

    // ---- the chosen agent ---------------------------------------------------------------

    void Choose(AgentState a)
    {
        world.Highlight(a);
        ShowBubble(a, a.lastThought);
        Refresh();
    }

    IEnumerator Turn()
    {
        yield return world.Animate(sim.lastMoves.ToList(), 0.45f / Mathf.Max(0.5f, sim.speed));
        if (world.Highlighted != null && world.Highlighted.alive) ShowBubble(world.Highlighted, world.Highlighted.lastThought);
    }

    // What the chosen agent last texted home, in a bubble over its head.
    void ShowBubble(AgentState a, string text)
    {
        if (bubble != null) Object.Destroy(bubble.gameObject);
        bubble = null;
        bubbleFor = null;
        if (a == null || !a.alive || string.IsNullOrEmpty(text)) return;
        Ui.Bubble(labelsLayer, text, 15, 320, out bubble);
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
            Ui.Place(kv.Value, p.x - 60, p.y + 4, 120, 22);
        }
        if (bubble == null) return;
        if (bubbleFor == null || !world.OnScreen(bubbleFor, out _, out var head)) { bubble.gameObject.SetActive(false); return; }
        bubble.gameObject.SetActive(true);
        var h = app.StagePoint(head);
        float width = bubble.sizeDelta.x, height = bubble.sizeDelta.y;
        float x = Mathf.Clamp(h.x - 30, 24, 1000 - width - 12);
        bool above = h.y - height - 8 >= 104;
        float y = above ? h.y - height - 8 : h.y + 60;
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
        float gap = 10, h = Mathf.Min(150, (cardsHeight - gap * (n - 1)) / n);
        bool compact = h < 110;
        for (int i = 0; i < n; i++)
        {
            var a = arena.agents[i];
            var card = new Card();
            card.outer = Ui.Card(cards, "Card", Theme.Panel, Theme.Border1);
            Ui.Place(card.outer.rectTransform, 0, i * (h + gap), 840, h);
            card.fill = card.outer.transform.Find("Fill").GetComponent<Image>();
            var t = card.outer.transform;
            var btn = card.outer.gameObject.AddComponent<Button>();
            btn.targetGraphic = card.outer;
            var agent = a;
            btn.onClick.AddListener(() => Choose(agent));

            var dot = Ui.Dot(t, Theme.Team[a.config.colour], 18);
            Ui.Place(dot.rectTransform, 18, 18, 18, 18);
            var name = Ui.Text(t, a.Name + (a.config.IsRule ? "  ·  a fixed rule, no AI" : ""), compact ? 19 : 22, Theme.Ink, true);
            Ui.Place(name.rectTransform, 46, 10, 460, 30);
            name.enableWordWrapping = false;
            card.score = Ui.Text(t, "0", compact ? 30 : 40, Theme.Ink, true, TextAlignmentOptions.TopRight);
            Ui.Place(card.score.rectTransform, 700, 6, 120, 50);
            var basket = Ui.Icon(t, "basket", compact ? 26 : 32);
            Ui.Place(basket.rectTransform, compact ? 672 : 662, 12, compact ? 26 : 32, compact ? 26 : 32);
            card.status = Ui.Text(t, "", 14, Theme.Muted);
            Ui.Place(card.status.rectTransform, 46, compact ? 34 : 40, 560, 22);
            card.status.enableWordWrapping = false;
            card.bar = Ui.Bar(t, Theme.Border, Theme.Aqua, 240, 8);
            Ui.Place(card.bar.track.rectTransform, 580, compact ? h - 22 : h - 26, 240, 8);
            card.tokens = Ui.Text(t, "", 13, Theme.Muted, false, TextAlignmentOptions.TopRight);
            Ui.Place(card.tokens.rectTransform, 560, compact ? h - 44 : h - 50, 260, 20);
            card.tokens.enableWordWrapping = false;
            if (!compact || h > 80)
            {
                card.thought = Ui.Text(t, "", compact ? 14 : 16, Theme.Text2);
                Ui.Place(card.thought.rectTransform, 46, compact ? 56 : 66, 500, h - (compact ? 60 : 110));
                card.thought.overflowMode = TextOverflowModes.Ellipsis;
            }
            if (!compact)
            {
                var know = Ui.Button(t, "What it knows", () => ShowKnows(agent), Ui.ButtonStyle.Quiet, 14);
                Ui.Place(know.GetComponent<RectTransform>(), 40, h - 42, 130, 32);
            }
            cardOf[a] = card;
        }
    }

    // The whole text handed to Claude for this agent's last turn, over the
    // market: the list and the rules, what it remembers, and this turn's view.
    void ShowKnows(AgentState a)
    {
        if (knows != null) Object.Destroy(knows.gameObject);
        knows = Ui.Node("Knows", root);
        Ui.Place(knows, 60, 110, 920, 910);
        var card = Ui.Float(knows, "Card", 0, 0, 920, 910);
        var t = card.transform;
        var title = Ui.Text(t, $"What {a.Name} knows right now", 24, Theme.Ink, true);
        Ui.Place(title.rectTransform, 28, 22, 700, 32);
        var sub = Ui.Text(t, a.config.IsRule ? "A fixed rule reads nothing. It takes exactly the things it names." : "This is the whole text handed to Claude for the last turn. Nothing else reaches the agent.", 15, Theme.Text2);
        Ui.Place(sub.rectTransform, 28, 58, 760, 24);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(knows.gameObject); knows = null; }, Ui.ButtonStyle.Secondary, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 920 - 28 - 100, 22, 100, 40);
        var scroll = Ui.Scroll(t, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 28, 96, 864, 790);
        string text = a.config.IsRule ? a.config.instructions : KnowsText(a);
        var body = Ui.Text(content, text, 16, Theme.Ink);
        float h = body.GetPreferredValues(text, 840, 0).y + 20;
        Ui.Place(body.rectTransform, 0, 0, 840, h);
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
            if (sim.phase == "thinking" && !sim.finished && phaseText != null)
            {
                dots = (dots + 1) % 4;
                phaseText.text = phaseBase + new string('.', dots);
            }
            yield return new WaitForSecondsRealtime(0.35f);
        }
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
            if (sim.turn == 1 && who != null) { coach.text = Coach.Step(who, 1); return; }
            if (sim.turn == 2) { firstErrand = false; coach.text = app.luma.Say("live-1"); return; }
        }
        var dead = sim.lastMoves.FirstOrDefault(m => m.ate != null && !m.agent.alive);
        if (dead != null && !saidOut) { saidOut = true; coach.text = Coach.Eliminated(dead.agent); return; }
        var broke = arena.agents.FirstOrDefault(a => a.outOfTokens);
        if (broke != null && !saidTokens) { saidTokens = true; coach.text = Coach.OutOfTokens(broke, sim.turn); return; }
        var confused = arena.agents.FirstOrDefault(a => a.confusedTurns > 0);
        if (confused != null && !saidConfused) { saidConfused = true; coach.text = Coach.Confused(confused); return; }
        var scored = sim.lastMoves.FirstOrDefault(m => m.ate != null && m.agent.alive);
        if (scored != null && !saidScore) { saidScore = true; coach.text = Coach.Scored(scored.agent, scored.ate); }
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
        foreach (var a in arena.agents)
        {
            var c = cardOf[a];
            bool hi = world.Highlighted == a;
            c.outer.color = hi ? Theme.Team[a.config.colour] : Theme.Border1;
            c.fill.color = !a.alive ? Theme.DangerSoft : a.outOfTokens ? Theme.AmberSoft : Theme.Panel;
            c.score.text = a.score.ToString();
            c.score.color = a.alive ? Theme.Ink : Theme.Muted;
            c.status.text = a.Status + (a.Active && !a.config.IsRule ? $"  ·  {Models.All[a.config.model].label}, sees {a.config.vision}, {MemoryModes.Labels[a.config.memory].ToLowerInvariant()} memory" : "");
            c.status.color = !a.alive ? Theme.Danger : a.outOfTokens ? Theme.Amber : Theme.Muted;
            Ui.SetBar(c.bar, a.budget > 0 ? (float)a.TokensLeft / a.budget : 0);
            c.bar.fill.color = a.TokensLeft < a.budget * 0.2f ? Theme.Amber : Theme.Aqua;
            c.tokens.text = a.config.IsRule ? "0 tokens: no AI" : $"{a.TokensLeft:N0} tokens left";
            if (c.thought != null) c.thought.text = a.lastThought == "" ? "" : "“" + a.lastThought + "”";
        }
    }
}
