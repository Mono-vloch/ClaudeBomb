using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The garage: where a robot is fitted out before it leaves. The chosen robot
// stands in the market on the left, big, and its loadout is on the right: the
// list, built by tapping the goods on the shelf (buy, never) and picking what to
// do when nothing is in view, with the exact words the robot will read shown
// underneath; the four dials as upgrade cards, locked until their errand; and the
// pocket money as a fuel gauge. Every dial shows on the robot at once. An errand
// opens on its training runs (Watch the pro, Fix the broken bot) before Your run.
public class SetupScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Market world;
    readonly RectTransform strip, editor, frame;
    readonly TextMeshProUGUI briefTitle, briefRule, briefNote, caption;
    RectTransform coachLine;
    readonly Image message;
    readonly Button send;
    int selected;
    Arena stage; // the market behind, with these robots on it

    static readonly int[] TurnOptions = { 20, 30, 40 };
    static readonly long[] BudgetOptions = { 10_000, 30_000, 100_000 };
    static readonly Vector2Int Spot = new Vector2Int(4, 5); // where the chosen robot stands for its picture: the middle, the camera south of it

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
        Ui.Place(frame, 60, 300, 680, 580);
        selected = Mathf.Clamp(selected, 0, app.configs.Count - 1);

        // Frosted glass along the top: the errand, the mission settings, deploy, and the training runs.
        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.86f), false);
        Ui.Place(top.rectTransform, -400, -300, 2800, 460);
        top.raycastTarget = false;

        var eyebrow = Ui.Eyebrow(root, lesson != null ? $"ERRAND {lesson.id}  ·  THE GARAGE" : "FREE PLAY  ·  THE GARAGE", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 60, 24, 600, 20);
        var title = Ui.Text(root, lesson != null ? lesson.title : "Fit out the agents", 30, Theme.Ink, true);
        Ui.Place(title.rectTransform, 60, 46, 860, 40);
        title.enableWordWrapping = false;
        if (lesson != null)
        {
            var back = Ui.Button(root, "Briefing", app.ShowLesson, Ui.ButtonStyle.Quiet, 15);
            Ui.Place(back.GetComponent<RectTransform>(), 930, 44, 100, 42);
            var tour = Ui.Button(root, "Tour", () => { if (touring == null) touring = app.StartCoroutine(RunTour()); }, Ui.ButtonStyle.Quiet, 15);
            Ui.Place(tour.GetComponent<RectTransform>(), 1034, 44, 70, 42);
        }
        var file = Ui.Button(root, "The file", () => app.ShowFile(), Ui.ButtonStyle.Quiet, 15);
        Ui.Place(file.GetComponent<RectTransform>(), 1108, 44, 90, 42);

        // Mission settings, top right, and the deploy button.
        var turnsLabel = Ui.Eyebrow(root, "TURNS", Theme.Muted);
        Ui.Place(turnsLabel.rectTransform, 1216, 24, 160, 20);
        var turns = new Ui.Segmented(root, TurnOptions.Select(t => t.ToString()).ToArray(), System.Array.IndexOf(TurnOptions, app.turns), 160, 42, 14);
        Ui.Place(turns.root, 1216, 46, 160, 42);
        turns.OnSelect = i => { app.turns = TurnOptions[i]; RefreshEstimate(); };
        var budgetLabel = Ui.Eyebrow(root, "POCKET MONEY", Theme.Muted);
        Ui.Place(budgetLabel.rectTransform, 1392, 24, 200, 20);
        var budget = new Ui.Segmented(root, new[] { "10k", "30k", "100k" }, System.Array.IndexOf(BudgetOptions, app.budget), 192, 42, 14);
        Ui.Place(budget.root, 1392, 46, 192, 42);
        budget.OnSelect = i => { app.budget = BudgetOptions[i]; RefreshEstimate(); };
        send = Ui.Button(root, "DEPLOY AGENTS", () => { foreach (var c in app.configs) c.Compose(); app.SaveConfigs(); app.StartRound(); }, Ui.ButtonStyle.Accent, 20);
        Ui.Place(send.GetComponent<RectTransform>(), 1600, 32, 260, 64);

        // The training runs, then Your run.
        if (lesson != null && lesson.ready != null)
        {
            var labels = new List<string> { "Watch the pro" };
            var steps = new List<App.Step> { App.Step.SeeItWork };
            if (lesson.broken != null) { labels.Add("Fix the broken bot"); steps.Add(App.Step.FixIt); }
            labels.Add("Your run"); steps.Add(App.Step.YourTurn);
            var stepsLabel = Ui.Eyebrow(root, "THE RUNS", Theme.Muted);
            Ui.Place(stepsLabel.rectTransform, 60, 104, 100, 20);
            var seg = new Ui.Segmented(root, labels.ToArray(), steps.IndexOf(app.step), 560, 40, 14);
            Ui.Place(seg.root, 150, 100, 560, 40);
            seg.OnSelect = i => { app.SetStep(steps[i]); Reload(); };
        }

        // The mission: the family's message
        message = Ui.Soft(root, "Message");
        Ui.Place(message.rectTransform, 60, 176, 1800, 108);
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

        // Under the picture: what it shows, then the robots, one pill each.
        var captionCard = Ui.Box(root, "Caption", new Color(1, 1, 1, 0.86f));
        Ui.Place(captionCard.rectTransform, 60, 884, 680, 62);
        captionCard.raycastTarget = false;
        caption = Ui.Text(captionCard.transform, "", 13, Theme.Text2, false, TextAlignmentOptions.MidlineLeft);
        Ui.Stretch(caption.rectTransform, 14, 0, 14, 0);
        var robotsLabel = Ui.Eyebrow(root, "AGENTS", Theme.TealDeep);
        Ui.Place(robotsLabel.rectTransform, 60, 956, 400, 20);
        strip = Ui.Node("Strip", root);
        Ui.Place(strip, 60, 980, 680, 100);

        editor = Ui.Node("Editor", root);
        Ui.Place(editor, 780, 300, 1080, 760);

        BuildWorld();
        RefreshStrip();
        BuildEditor();
        Coach();
        Ui.Emblem(root);

        if (lesson != null && lesson.number == 1 && !Tour.Seen) touring = app.StartCoroutine(RunTour());
    }

    // On a training run LUMA says why the ready list works, or what the broken one
    // has in it, once, over the picture; the words stay in the file.
    void Coach()
    {
        if (coachLine != null) Object.Destroy(coachLine.gameObject);
        coachLine = null;
        var setup = app.CurrentSetup;
        if (setup == null) return;
        coachLine = app.luma.Line(root, 60, 300, 680, (app.step == App.Step.FixIt ? "Fix it: " + setup.name + ". " : "") + setup.why, true, false, 15);
    }

    void ShowBrief()
    {
        briefTitle.text = $"A MESSAGE FROM {app.brief.from.ToUpperInvariant()}";
        briefRule.text = app.brief.rule;
        briefNote.text = "The catch: " + app.brief.note;
    }

    // A training run was chosen, or Your run: the robots have new lists.
    void Reload()
    {
        RefreshStrip();
        BuildEditor();
        world.RefreshRobot(Chosen);
        world.Dress(Chosen);
        Caption();
        Coach();
    }

    // ---- the market behind ------------------------------------------------------

    void BuildWorld()
    {
        stage = new Arena(app.brief, app.configs, app.budget, 9);
        // Nothing between the camera and the robot: the cells in front of it are cleared.
        for (int x = Spot.x - 1; x <= Spot.x + 2; x++)
            for (int y = Spot.y; y <= Arena.Size - 2; y++)
                stage.RemoveItem(new Vector2Int(x, y));
        Place();
        world.Build(stage, false);
        Show();
    }

    // The chosen robot stands up front, facing us; the others wait at their houses.
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
        Caption();
    }

    void Caption()
    {
        if (caption == null) return;
        var c = Current;
        var setup = app.CurrentSetup;
        if (setup != null)
        {
            caption.text = app.step == App.Step.FixIt ? "Fix it: " + setup.name : "Watch the pro: a ready list in every agent";
            return;
        }
        string memory = c.memory == MemoryModes.Notebook ? "a notebook in its hand" : MemoryModes.Turns(c.memory) == 0 ? "nothing between turns" : $"the last {MemoryModes.Turns(c.memory)} turns behind it";
        caption.text = $"Sees {c.vision} cell{(c.vision > 1 ? "s" : "")} around it  ·  {Models.All[c.model].label} on the antenna  ·  carries {memory}  ·  {Efforts.Labels[c.effort].ToLowerInvariant()} effort";
    }

    // ---- the robots ----------------------------------------------------------------

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

    void AddAgent()
    {
        if (app.configs.Count >= 10) return;
        var used = app.configs.Select(c => c.colour).ToHashSet();
        int colour = Enumerable.Range(0, Theme.Team.Length).FirstOrDefault(i => !used.Contains(i));
        var cfg = new AgentConfig { name = "Agent " + (app.configs.Count + 1), colour = colour };
        app.lesson?.Apply(cfg);
        var setup = app.CurrentSetup;
        if (setup != null) { cfg.Write(setup.Fill(app.brief)); cfg.model = setup.model; cfg.vision = setup.vision; cfg.memory = setup.memory; cfg.effort = setup.effort; app.lesson.Apply(cfg); }
        app.AddRobot(cfg);
        selected = app.configs.Count - 1;
        app.SaveConfigs();
        BuildWorld();
        RefreshStrip();
        BuildEditor();
    }

    void RemoveAgent()
    {
        if (app.configs.Count <= 2) return;
        app.RemoveRobot(selected);
        selected = Mathf.Clamp(selected, 0, app.configs.Count - 1);
        app.SaveConfigs();
        BuildWorld();
        RefreshStrip();
        BuildEditor();
    }

    void Select(int index)
    {
        app.SaveConfigs();
        selected = index;
        Place();
        Show();
        RefreshStrip();
        BuildEditor();
    }

    // One pill per robot, five to a row, and one more to add a robot.
    void RefreshStrip()
    {
        Ui.Clear(strip);
        const float w = 128, h = 44, gap = 10;
        int n = app.configs.Count;
        for (int i = 0; i <= n; i++)
        {
            float x = i % 5 * (w + gap), y = i / 5 * (h + 8);
            if (i == n)
            {
                if (n >= 10) break;
                var add = Ui.Button(strip, "+ Add an agent", AddAgent, Ui.ButtonStyle.Secondary, 14);
                Ui.Place(add.GetComponent<RectTransform>(), x, y, w, h);
                break;
            }
            int index = i;
            var c = app.configs[i];
            bool on = i == selected;
            var pill = Ui.Button(strip, "", () => Select(index), Ui.ButtonStyle.Secondary, 15);
            var rt = pill.GetComponent<RectTransform>();
            Ui.Place(rt, x, y, w, h);
            pill.GetComponent<Image>().color = on ? Theme.Team[c.colour] : Theme.Border;
            pill.transform.Find("Fill").GetComponent<Image>().color = on ? Theme.Surface2 : Theme.Panel;
            var dot = Ui.Dot(pill.transform, Theme.Team[c.colour], 14);
            Ui.Place(dot.rectTransform, 14, h / 2 - 7, 14, 14);
            var label = pill.GetComponentInChildren<TextMeshProUGUI>();
            label.text = string.IsNullOrWhiteSpace(c.name) ? "(unnamed)" : c.name;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontSize = 15;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            Ui.Stretch(label.rectTransform, 34, 0, 10, 0);
        }
    }

    // ---- the loadout -------------------------------------------------------------------

    TextMeshProUGUI estimate, reads, readsTokens;
    RectTransform listArea;
    (Image track, Image fill) fuel;
    AgentConfig Current => app.configs[selected];

    void BuildEditor()
    {
        Ui.Clear(editor);
        var c = Current;
        var card = Ui.Float(editor, "Card", 0, 0, 1080, 760);
        var t = card.transform;

        // Name, color, remove
        var nameLabel = Ui.Eyebrow(t, "AGENT NAME", Theme.Muted);
        Ui.Place(nameLabel.rectTransform, 28, 16, 200, 20);
        var name = Ui.Input(t, "Agent name", false, 20);
        Ui.Place(name.GetComponent<RectTransform>(), 28, 38, 300, 48);
        name.text = c.name;
        name.onValueChanged.AddListener(v => { c.name = v; });
        name.onEndEdit.AddListener(_ => RefreshStrip());

        var colourLabel = Ui.Eyebrow(t, "COLOR", Theme.Muted);
        Ui.Place(colourLabel.rectTransform, 350, 16, 200, 20);
        for (int i = 0; i < Theme.Team.Length; i++)
        {
            int index = i;
            var ring = Ui.Box(t, "Swatch", c.colour == i ? Theme.Ink : Theme.Panel, false);
            ring.sprite = Theme.Circle;
            Ui.Place(ring.rectTransform, 350 + i * 44, 40, 40, 40);
            var swatch = Ui.Dot(ring.transform, Theme.Team[i], 32);
            Ui.Stretch(swatch.rectTransform, 4, 4, 4, 4);
            var b = ring.gameObject.AddComponent<Button>();
            b.targetGraphic = swatch;
            b.onClick.AddListener(() => { c.colour = index; world.RefreshRobot(Chosen); RefreshStrip(); BuildEditor(); });
        }
        if (app.configs.Count > 2)
        {
            var remove = Ui.Button(t, "Remove", RemoveAgent, Ui.ButtonStyle.Danger, 14);
            Ui.Place(remove.GetComponent<RectTransform>(), 920, 38, 132, 48);
        }

        // The list
        listArea = Ui.Node("List", t);
        Ui.Place(listArea, 16, 96, 1048, 396);
        var listLabel = Ui.Eyebrow(t, "THE LIST", Theme.TealDeep);
        Ui.Place(listLabel.rectTransform, 28, 104, 800, 20);
        var mode = Ui.Button(t, c.freeText ? "Build it from parts" : "Write it yourself", () =>
        {
            if (c.freeText) { c.freeText = false; c.Compose(); } else { c.freeText = true; }
            BuildEditor();
        }, Ui.ButtonStyle.Quiet, 13);
        Ui.Place(mode.GetComponent<RectTransform>(), 870, 96, 182, 34);

        if (c.freeText) BuildWords(t, c);
        else { c.Compose(); BuildParts(t, c); }

        // The four dials: upgrade cards. An errand opens them one at a time; the rest
        // are locked so only that errand's idea is in play. Each shows on the robot at once.
        var l = app.lesson;
        Dial(t, 28, 500, "Brain", "Which Claude runs it. Bigger: better choices, pricier tokens.",
             Models.All.Select(m => m.label).ToArray(), c.model, i => { c.model = i; Dressed(); }, l == null || l.model, Ladder.ModelLevel);
        Dial(t, 290, 500, "How far it sees", "Cells in every direction. More in view, more words every turn.",
             new[] { "1 cell", "2 cells", "3 cells" }, c.vision - 1, i => { c.vision = i + 1; Dressed(); }, l == null || l.vision, Ladder.VisionLevel);
        Dial(t, 552, 500, "Memory", MemoryModes.Blurbs[c.memory],
             MemoryModes.Labels, c.memory, i => { c.memory = i; Dressed(); BuildEditor(); }, l == null || l.memory, Ladder.MemoryLevel);
        Dial(t, 814, 500, "Effort", "How hard it thinks at each stall. More: better, slower, pricier.",
             Efforts.Labels, c.effort, i => { c.effort = i; Dressed(); }, l == null || l.effort, Ladder.EffortLevel);

        // The fuel gauge: what one turn costs, and how far the pocket money goes.
        var fuelLabel = Ui.Eyebrow(t, "POCKET MONEY", Theme.TealDeep);
        Ui.Place(fuelLabel.rectTransform, 28, 680, 500, 20);
        fuel = Ui.Bar(t, Theme.Border, Theme.Aqua, 1024, 12);
        Ui.Place(fuel.track.rectTransform, 28, 704, 1024, 12);
        estimate = Ui.Text(t, "", 14, Theme.Text2);
        Ui.Place(estimate.rectTransform, 28, 722, 1024, 34);
        RefreshEstimate();
    }

    // The list from parts: tap the goods on the shelf, pick the rule for an empty
    // view, add your own words. The words the robot reads are shown underneath.
    void BuildParts(Transform t, AgentConfig c)
    {
        var all = Items.All;
        float slot = 1024f / all.Length;
        for (int i = 0; i < all.Length; i++)
        {
            var k = all[i];
            float x = 28 + i * slot;
            bool buy = c.buy.Contains(k.id), never = c.never.Contains(k.id);
            var ring = Ui.Dot(t, buy ? Theme.Aqua : never ? Theme.Danger : Theme.Surface3, 54);
            Ui.Place(ring.rectTransform, x + slot / 2 - 27, 146, 54, 54);
            var inner = Ui.Dot(ring.transform, buy ? Theme.Surface2 : never ? Theme.DangerSoft : Theme.Panel, 46);
            Ui.Stretch(inner.rectTransform, 4, 4, 4, 4);
            var icon = Ui.Icon(ring.transform, k.id, 34);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icon.rectTransform.anchoredPosition = Vector2.zero;
            var label = Ui.Text(t, k.id, 11, buy ? Theme.TealDeep : never ? Theme.Danger : Theme.Text2, buy || never, TextAlignmentOptions.Center);
            Ui.Place(label.rectTransform, x, 202, slot, 16);
            label.enableWordWrapping = false;
            if (buy || never)
            {
                var state = Ui.Text(t, buy ? "BUY" : "NEVER", 9, buy ? Theme.TealDeep : Theme.Danger, true, TextAlignmentOptions.Center);
                state.characterSpacing = 3;
                Ui.Place(state.rectTransform, x, 216, slot, 14);
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
        Ui.Place(ruleLabel.rectTransform, 28, 240, 500, 20);
        var rule = new Ui.Segmented(t, AgentConfig.FallbackLabels, c.fallback, 560, 38, 13);
        Ui.Place(rule.root, 28, 262, 560, 38);
        rule.OnSelect = i => { c.fallback = i; c.Compose(); RefreshRead(); };

        var wordsLabel = Ui.Eyebrow(t, "YOUR OWN WORDS", Theme.Muted);
        Ui.Place(wordsLabel.rectTransform, 610, 240, 460, 20);
        var words = Ui.Input(t, "For example: in this house a tomato counts as a vegetable.", false, 14);
        Ui.Place(words.GetComponent<RectTransform>(), 610, 262, 442, 38);
        words.text = c.words;
        words.onValueChanged.AddListener(v => { c.words = v; c.Compose(); RefreshRead(); });

        var readsLabel = Ui.Eyebrow(t, "WHAT THE AGENT WILL READ", Theme.Muted);
        Ui.Place(readsLabel.rectTransform, 28, 312, 400, 20);
        readsTokens = Ui.Text(t, "", 13, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(readsTokens.rectTransform, 852, 312, 200, 20);
        var readsCard = Ui.Soft(t, "Reads");
        Ui.Place(readsCard.rectTransform, 28, 334, 1024, 150);
        reads = Ui.Text(readsCard.transform, "", 15, Theme.Ink);
        Ui.Stretch(reads.rectTransform, 16, 10, 16, 10);
        RefreshRead();
    }

    // The list in the student's own words, as it will be read.
    void BuildWords(Transform t, AgentConfig c)
    {
        readsTokens = Ui.Text(t, "", 13, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(readsTokens.rectTransform, 660, 132, 200, 20);
        var field = Ui.Input(t, "For example: Buy apples, bananas and pears. Never buy tomatoes, potatoes or mushrooms. If nothing on this list is in view, keep walking.", true, 18);
        Ui.Place(field.GetComponent<RectTransform>(), 28, 134, 1024, 350);
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

    // A dial moved: the robot wears it, the picture and the arithmetic follow.
    void Dressed()
    {
        world.Dress(Chosen);
        Caption();
        RefreshEstimate();
    }

    // An upgrade card: the options stacked, or a lock with the errand that opens it.
    void Dial(Transform t, float x, float y, string label, string blurb, string[] options, int selectedIndex, System.Action<int> onSelect, bool open, int opensAt)
    {
        var card = Ui.Card(t, label, open ? Theme.Panel : Theme.Surface1, open ? Theme.Border1 : Theme.Border);
        Ui.Place(card.rectTransform, x, y, 238, 170);
        var ct = card.transform;
        var l = Ui.Text(ct, label, 16, open ? Theme.Ink : Theme.Muted, true);
        Ui.Place(l.rectTransform, 14, 10, 160, 24);
        if (open)
        {
            float oy = 40, oh = options.Length > 3 ? 26 : 30;
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                bool on = i == selectedIndex;
                var b = Ui.Button(ct, options[i], () => onSelect(index), Ui.ButtonStyle.Secondary, 12);
                Ui.Place(b.GetComponent<RectTransform>(), 14, oy + i * (oh + 4), 210, oh);
                b.GetComponent<Image>().color = on ? Theme.BorderTeal : Theme.Border;
                b.transform.Find("Fill").GetComponent<Image>().color = on ? Theme.Surface2 : Theme.Panel;
                b.GetComponentInChildren<TextMeshProUGUI>().color = on ? Theme.TealDeep : Theme.Ink;
            }
        }
        else
        {
            var lockTag = Ui.Eyebrow(ct, "LOCKED", Theme.Muted);
            lockTag.font = Theme.Bold;
            Ui.Place(lockTag.rectTransform, 160, 14, 70, 20);
            var fixedText = Ui.Text(ct, options[selectedIndex], 15, Theme.TealDeep, true);
            Ui.Place(fixedText.rectTransform, 14, 44, 210, 24);
            var opens = Ui.Text(ct, $"Opens at errand 1.{opensAt}", 12, Theme.Text2);
            Ui.Place(opens.rectTransform, 14, 72, 210, 24);
        }
    }

    // The arithmetic the students should be doing in their heads: what one turn
    // costs, and how many turns the pocket money buys.
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
        estimate.text = $"About {perTurn:N0} tokens a turn  ·  {app.budget:N0} tokens buys {lasts}";
    }
}
