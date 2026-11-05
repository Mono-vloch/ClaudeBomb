using System;
using System.Collections.Generic;
using System.Linq;

public enum Practised { Yes, Missed, NotNeeded }

// One recall question, asked before a level starts: from an earlier level, no
// score, and a wrong answer shows the right one and where it came from.
public class Recall
{
    public string question;
    public string[] options;
    public int answer;   // index into options
    public string from;  // the level it comes from, "1.2"
}

// A ready-made setup: the list and the dials every robot gets for the "See it
// work" step (it does the job) or the "Fix it" step (it has the level's planned
// mistake in it). {items} and {avoid} are filled from the errand's message.
public class Setup
{
    public string name;         // "See it work", or the mistake: "A job description with a category word"
    public string instructions;
    public int model = Lesson.FixedModel, vision = Lesson.FixedVision, memory = Lesson.FixedMemory, effort = Lesson.FixedEffort;
    public string why;          // what LUMA says about it: why it works, or why it fails

    public string Fill(Brief brief)
    {
        string items = Names(brief.SafeItems), avoid = Names(brief.PoisonItems);
        return instructions.Replace("{items}", items).Replace("{avoid}", avoid);
    }

    static string Names(IEnumerable<ItemKind> kinds)
    {
        var names = kinds.Select(k => Items.Plural(k.id)).ToList();
        return names.Count <= 1 ? string.Join("", names) : string.Join(", ", names.Take(names.Count - 1)) + " and " + names.Last();
    }
}

// One step of the ladder: the question it answers, the idea in plain words, the
// family's message it is played on, which dials the students may touch, the
// recall question before it, the ready setups for See it work and Fix it, what
// LUMA says while the robots shop, and how the debrief checks each robot.
// Seven errands in tier 1, one idea each, in the order the course teaches them
// (Docs/curriculum/ai-arena.md): first what an agent is at all, then one of its
// parts per level, then whether a task needs an agent, then everything at once.
public class Lesson
{
    public int number;
    public string id;         // the curriculum's "1.2"
    public string title;      // the level's question
    public string kicker;     // the headline, in plain words
    public string luma;       // what LUMA says when the level opens (recorded)
    public string learn;      // the idea: under 80 words
    public int briefPreset;   // Brief.Make
    public bool model, vision, memory, effort; // the dials the students may set
    public bool decide;       // level 1.6: the student decides agent, automation or neither, per task
    public Recall recall;     // null on the first level
    public Setup ready;       // See it work; null when the level gives no help
    public Setup broken;      // Fix it; null when there is nothing to fix
    public string live;       // LUMA, while the robots shop (recorded)
    public string takeaway;   // one sentence, on the debrief
    public string ask;        // the question to ask as the robot's manager
    public Func<AgentState, Arena, List<(Practised result, string label)>> check;

    // The locked dials: the same sensible robot for everyone, so only the
    // level's idea is in play.
    public const int FixedModel = 0, FixedVision = 2, FixedMemory = MemoryModes.Short, FixedEffort = 0;

    public void Apply(AgentConfig c)
    {
        if (!model) c.model = FixedModel;
        if (!vision) c.vision = FixedVision;
        if (!memory) c.memory = FixedMemory;
        if (!effort) c.effort = FixedEffort;
    }

    public List<(Practised result, string label)> Check(AgentState a, Arena arena) => check(a, arena);
}

public static class Ladder
{
    // The level at which each dial opens, and the one that is played without lists.
    public const int VisionLevel = 3, MemoryLevel = 4, ModelLevel = 5, EffortLevel = 5, DecideLevel = 6;

    static Practised Of(bool ok) => ok ? Practised.Yes : Practised.Missed;

    public static readonly Lesson[] All =
    {
        new Lesson
        {
            number = 1, id = "1.1", title = "What does an agent do on its own?",
            kicker = "You write the list. It shops alone. You cannot step in.",
            luma = "Hi, I'm LUMA, your coach. Today you send your first agent. Write its list, send it, and watch it shop without you.",
            learn = "An agent is AI that works on its own. It looks at what is in front of it, decides, acts, reports back, and goes again until the job is done. " +
                    "Your agent is one. You write its shopping list before it leaves. Once the door closes, the only thing steering it is what you wrote.\n\n" +
                    "This errand is simple on purpose: apples, nothing else. Watch the loop, not the list.",
            briefPreset = 5,
            ready = new Setup
            {
                name = "See it work",
                instructions = "Buy apples. Nothing else. If no apples are in view, keep walking.",
                why = "This list names the one thing to buy, says what to leave, and says what to do when nothing is in view. Watch the loop run: look, decide, act, text home.",
            },
            live = "Watch the text in quotes on each card: that is the agent telling you what it saw and what it decided. Nobody is steering. " +
                   "The one control you have left is Call them home, the off switch.",
            takeaway = "An agent decides its own next step. Everything it does traces back to the list you wrote before it left.",
            ask = "If the agent meets something your list does not cover, does it guess or does it stop?",
            check = (a, arena) => new List<(Practised, string)>
            {
                (Of(!string.IsNullOrWhiteSpace(a.config.instructions)), "Wrote a list"),
                (Of(Verdicts.Named(a.config.instructions, "apple")), "Said what to buy: apples"),
                (Of(Verdicts.HasWarning(a.config.instructions)), "Said what to leave at the market"),
                (Of(a.alive), "Came home with dinner"),
            },
        },
        new Lesson
        {
            number = 2, id = "1.2", title = "What exactly is its job?",
            kicker = "A vague list does not come home with a bad basket. It comes home with a tomato in the fruit salad.",
            luma = "Same agent, trickier dinner. This time the catch is a tomato, and only your list can save the fruit salad.",
            learn = "The list is the agent's job description, what an AI calls its system prompt: the only thing it will ever know about tonight. It cannot call " +
                    "you from the market. So a word like \"fruit\" makes it decide for itself what counts as fruit, and a tomato is where that goes wrong.\n\n" +
                    "Name the things. Cover the catch. Say what to do when nothing on the list is in view.",
            briefPreset = 0,
            recall = new Recall
            {
                question = "What steers the agent once the run starts?",
                options = new[] { "Only what was written before it left", "The teacher, turn by turn", "The other agents" },
                answer = 0, from = "1.1",
            },
            ready = new Setup
            {
                name = "See it work",
                instructions = "Buy {items}. Never buy a tomato or a mushroom, and no other vegetable. If nothing on this list is in view, keep walking.",
                why = "This list names the fruit one by one, names the two look-alikes to leave, and says what to do when nothing fits. There is nothing left for the agent to guess.",
            },
            broken = new Setup
            {
                name = "A job description with a category word",
                instructions = "Get fruit.",
                why = "This list says the category and nothing else. The agent has to decide for itself what counts as fruit, and in this house a tomato does not. " +
                      "Send it, watch, then name the items and the exception, and send again.",
            },
            live = "The catch tonight is the tomato. Which lists named it, and which said \"vegetables\" and hoped?",
            takeaway = "Name the things. Cover the exceptions. Say what to do when the list does not fit.",
            ask = "Where in our own instructions to an AI do we say \"and if none of this applies, do this\"?",
            check = (a, arena) => new List<(Practised, string)>
            {
                (Of(Verdicts.ItemsNamed(a.config.instructions) >= 4), "Named the items, not just \"fruit\""),
                (Of(Verdicts.Named(a.config.instructions, "tomato")), "Covered the catch: the tomato"),
                (Of(Verdicts.HasFallback(a.config.instructions)), "Said what to do when nothing on the list is in view"),
                (Of(a.alive), "Came home with dinner"),
            },
        },
        new Lesson
        {
            number = 3, id = "1.3", title = "What does it see?",
            kicker = "The agent only knows the stalls in front of it right now.",
            luma = "Now you choose how far the agent sees. More stalls in view means better choices, and more tokens on every turn.",
            learn = "Every turn the agent gets a few lines of text: the stalls within its sight. That text is its context. Nothing outside it exists " +
                    "for the agent, however obvious it is to you watching the whole market.\n\n" +
                    "This level opens how far it sees. See further and it chooses better, but every extra stall is more words every turn, " +
                    "and every word costs tokens, its pocket money.",
            briefPreset = 1,
            recall = new Recall
            {
                question = "Which two of these are parts of a good job description?",
                options = new[] { "The task, and what to do when nothing fits", "The weather, and the time of day", "The agent's name, and its color" },
                answer = 0, from = "1.2",
            },
            ready = new Setup
            {
                name = "See it work",
                instructions = "Buy only red things: {items}. Leave everything else at the market. If nothing red is in view, walk toward the middle of the market.",
                vision = 2,
                why = "This list names the red things, and it says what to do when none is in view, so the agent never stands still. It sees two cells: enough to find them, not so much that every turn is expensive.",
            },
            broken = new Setup
            {
                name = "Nothing on the list in view, and no rule for that",
                instructions = "Buy only red things: {items}. Leave everything else at the market.",
                vision = 1,
                why = "This agent sees one cell and has no rule for when nothing is in view, so it stands still or wanders. " +
                      "Add a rule for \"nothing in view\", or let it see further, and compare what each costs per turn.",
            },
            live = "Click an agent's card to see the square it can actually see. Everything outside that square does not exist for it.",
            takeaway = "Context is what the agent is handed at that moment, nothing more. More context means better choices and a higher cost on every turn.",
            ask = "What does our agent actually get to see when it works, and what are we assuming it knows?",
            check = (a, arena) => new List<(Practised, string)>
            {
                (Of(a.score > 0), "Found something on the list"),
                (Of(Verdicts.HasFallback(a.config.instructions)), "Told it what to do when nothing is in view"),
                (Of(!a.outOfTokens), "Lasted the budget"),
                (Of(a.alive), "Came home with dinner"),
            },
        },
        new Lesson
        {
            number = 4, id = "1.4", title = "What does it remember?",
            kicker = "Every turn starts from nothing, unless you give it a memory.",
            luma = "Now you choose what the agent remembers. Nothing, the last few turns, or a notebook it writes itself.",
            learn = "An AI model forgets everything between two calls. What the agent \"remembers\" is whatever goes back into its next prompt: the last " +
                    "few turns word for word, which is complete but grows, or a notebook it writes itself, which is short but holds only what it chose to write.\n\n" +
                    "This level opens memory. Watch an agent with none walk the same aisle twice, and watch the tokens of one carrying ten turns of history.",
            briefPreset = 2,
            recall = new Recall
            {
                question = "Why does seeing further cost more on every turn?",
                options = new[] { "Every stall in view is more words in the prompt, and every word is tokens", "The agent walks more slowly", "The market charges for looking" },
                answer = 0, from = "1.3",
            },
            ready = new Setup
            {
                name = "See it work",
                instructions = "Buy exactly these, one of each: {items}. Nothing else. Write in your notebook what you already have. If nothing on the list is in view, keep walking.",
                memory = MemoryModes.Notebook,
                why = "The list names the four things, and the notebook lets the agent write down what it already has, so it does not buy a second one. A notebook is a few words a turn; ten turns of history would be hundreds.",
            },
            broken = new Setup
            {
                name = "No memory",
                instructions = "Buy exactly these, one of each: {items}. Nothing else. If nothing on the list is in view, keep walking.",
                memory = MemoryModes.None,
                why = "With no memory, every turn starts from nothing: it walks the same ground twice and may buy a second of something it already has. " +
                      "Give it a memory, send it again, and compare the tokens that memory costs.",
            },
            live = "Memory is not free: every remembered turn rides along in the next prompt. Compare the token bars of agents with different memory.",
            takeaway = "An agent remembers only what is put back into its prompt. A notebook is cheaper than a transcript, if the agent writes the right things in it.",
            ask = "When our agent runs tomorrow, what will it remember from today, and where is that written down?",
            check = (a, arena) => new List<(Practised, string)>
            {
                (Of(a.score > 0), "Bought something on the list"),
                (Of(!a.outOfTokens), "Lasted the budget"),
                a.config.memory == MemoryModes.Notebook ? (Of(!string.IsNullOrWhiteSpace(a.notebook)), "Wrote in its notebook")
                                                        : (Practised.NotNeeded, "No notebook to write in"),
                (Of(a.alive), "Came home with dinner"),
            },
        },
        new Lesson
        {
            number = 5, id = "1.5", title = "Which brain does the job?",
            kicker = "A bigger brain is not always a better buy. It is always pricier.",
            luma = "Now you choose the agent's brain and how hard it thinks. Watch the cost, not just the basket.",
            learn = "The model is the agent's brain. Haiku is quick and cheap; Opus is the most capable and forty times the price per token. Effort is how " +
                    "long it thinks at each stall: more thinking means better choices, more output tokens and slower turns.\n\n" +
                    "This level opens both. The debrief shows what each agent really cost. Pick the smallest brain that does the errand.",
            briefPreset = 3,
            recall = new Recall
            {
                question = "What does the agent remember between two turns if it has no notebook?",
                options = new[] { "Nothing, unless the last turns are put back into its prompt", "Everything it has ever seen", "The family's message" },
                answer = 0, from = "1.4",
            },
            ready = new Setup
            {
                name = "See it work",
                instructions = "Anything at the market is fine except {avoid}. Never buy those: Grandma is allergic. If nothing else is in view, keep walking.",
                model = 0, effort = 0,
                why = "A clear list with the three exceptions named, on the smallest brain at the least effort. It does the job at the lowest cost per item in the class.",
            },
            broken = new Setup
            {
                name = "The biggest brain on an easy job",
                instructions = "Anything at the market is fine except {avoid}. Never buy those: Grandma is allergic. If nothing else is in view, keep walking.",
                model = 2, effort = 2,
                why = "Opus at high effort does this errand too, at many times the price per item. Send it, then try a smaller brain and compare the cost per item on the debrief.",
            },
            live = "Every card shows the brain it runs on. At the debrief, compare what each item in the basket cost, not just how many.",
            takeaway = "Pick the smallest model and the least effort that do the job. The cost is per turn, and the turns add up.",
            ask = "For each task we give an AI: does it need the big model, or just a clearer list?",
            check = (a, arena) =>
            {
                double Cpp(AgentState x) => x.score > 0 ? x.CostUsd / x.score : double.MaxValue;
                var best = arena.agents.Where(x => x.score > 0).OrderBy(Cpp).FirstOrDefault();
                var list = new List<(Practised, string)>
                {
                    (Of(a.alive), "Came home with dinner"),
                    (Of(a.score > 0), "Bought at least one thing on the list"),
                    (Of(!a.outOfTokens), "Lasted the budget"),
                };
                list.Add(a.score == 0 ? (Practised.Missed, "Nothing in the basket, so no cost per item")
                       : a == best ? (Practised.Yes, $"Cheapest item of the round: ${Cpp(a):0.0000}")
                       : (Practised.NotNeeded, $"Cost per item: ${Cpp(a):0.0000}"));
                return list;
            },
        },
        new Lesson
        {
            number = 6, id = "1.6", title = "Does this task need an agent?",
            kicker = "A fixed rule costs nothing and never surprises you. It cannot decide anything.",
            luma = "Now you decide before writing any list: does this task need an agent at all? A fixed rule costs nothing. An agent costs tokens on every turn.",
            learn = "An agent costs about four times a chat to run; this errand shows you its own measured cost. A fixed rule, a simple automation such as " +
                    "\"go to the nearest apple\", costs almost nothing and never surprises you. But it cannot decide anything.\n\n" +
                    "Start with the simplest thing that does the job, and use an agent only where a fixed rule will not do. Some tasks need neither: a decision only a person should take.",
            briefPreset = 5,
            decide = true,
            recall = new Recall
            {
                question = "Which costs more per item: the biggest brain, or a clearer job description?",
                options = new[] { "The biggest brain: it costs per token, on every turn", "A clearer job description: it has more words", "They cost the same" },
                answer = 0, from = "1.5",
            },
            live = "On the left a fixed rule, no AI, no tokens. On the right an agent with a list. Same market, same task. Watch the basket and the cost.",
            takeaway = "Start with the simplest solution. Use an agent only where fixed rules will not do.",
            ask = "Which of our \"AI agent\" ideas is really a simple automation?",
            check = (a, arena) => new List<(Practised, string)> { (Of(a.alive), "Came home with dinner") },
        },
        new Lesson
        {
            number = 7, id = "1.7", title = "Capstone: Who is accountable?",
            kicker = "Your agent has pocket money. Every choice costs some, every turn.",
            luma = "Everything is open and there is no help this time. Every dial costs tokens on every turn. Do the arithmetic before you send the agent.",
            learn = "Everything at once: the list, how far it sees, what it remembers, which brain, how hard it thinks. Each one is a dial, and each " +
                    "dial costs tokens on every single turn. The agent that comes home with dinner and money left is the one whose manager did the arithmetic.\n\n" +
                    "You delegated the errand. You did not delegate the accountability: every ruined dinner traces back to a choice made before the agent left.",
            briefPreset = 4,
            model = true, vision = true, memory = true, effort = true,
            recall = new Recall
            {
                question = "What can a simple automation never do?",
                options = new[] { "Decide: it follows its fixed rule whatever happens", "Cost money", "Finish the job" },
                answer = 0, from = "1.6",
            },
            live = "Everything is in play. When an agent comes home early, ask which dial did it: the list, its sight, its memory, its brain or its budget.",
            takeaway = "You delegated the task, not the accountability. Every ruined dinner traces back to a choice you made before the agent left.",
            ask = "Before we let an agent run on its own at work: what is its list, what does it see, what does it remember, what does it cost per run, and who checks it?",
            check = (a, arena) => new List<(Practised, string)>
            {
                (Of(Verdicts.ItemsNamed(a.config.instructions) >= 3 && Verdicts.HasWarning(a.config.instructions)), "Named the items and the exceptions"),
                (Of(Verdicts.HasFallback(a.config.instructions)), "Said what to do when the list does not fit"),
                (Of(!a.outOfTokens), "Lasted the budget"),
                (Of(a.alive), "Came home with dinner"),
            },
        },
    };

    static Ladder()
    {
        // Levels 3, 4 and 5 open their own dial on top of the ones before.
        All[2].vision = true;
        All[3].vision = All[3].memory = true;
        All[4].vision = All[4].memory = All[4].model = All[4].effort = true;
    }

    public static int Count => All.Length;
    public static Lesson Get(int number) => number >= 1 && number <= All.Length ? All[number - 1] : null;

    // The five ways an agent goes wrong: the subject of tier 2, named at the capstone.
    public const string FiveWays = "Next, in tier 2, the five ways an agent goes wrong: it guesses instead of asking, it does more than it was asked, " +
                                   "it goes round in circles, it obeys a hidden instruction, and it drifts.";
}
