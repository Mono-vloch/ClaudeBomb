using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// LUMA, the learning coach. Claude stays the teammate inside the story; LUMA
// steps outside it to name what the player just learned, explain a strike or a
// blocked share, hint (never answer) when they're stuck, and recap the learning
// outcomes at the end.
//
// LUMA lives in the chat: the 3D character is always visible in the chat header,
// and when it has something to say it animates, reads the line aloud and posts it
// in the chat (in cyan; the player's messages are slate, Claude's terracotta).
// Each tip is said once, and the clock is held while LUMA talks, so listening never
// costs time. Lines play one after another; their voices are recorded files in
// Resources/LumaVoice, one per line id (regenerate with Claude Bomb > Generate LUMA
// Voice). Before the clock starts, LUMA gives the guided tour (Tour, which uses
// Speak); at the end it reads the recap on the debrief screen (LoseSequence).
public class LoomaCoach : MonoBehaviour
{
    public static LoomaCoach Instance { get; private set; }

    [SerializeField] ChatPanel chat;
    [SerializeField] Camera lumaCamera;          // films the 3D LUMA on its stage
    [SerializeField] LumaRig luma;
    [SerializeField] RectTransform lumaView;     // where LUMA appears in the chat header
    [SerializeField] float stuckHintSeconds = 120f;

    // Everything LUMA says, by id. The id names the line's voice file. {0} is a
    // value filled in at the time (a date); it's left out of the spoken version.
    public static readonly Dictionary<string, string> Lines = new Dictionary<string, string>
    {
        ["tour-hello"] = "Hi, I'm LUMA, your coach. Before the clock starts, let me show you around.",
        ["tour-device"] = "This is the device. Five numbered modules, each with a light: red while it's live, green once it's safe.",
        ["tour-casing"] = "Look at every side. The back and the bottom carry the stamp, the power cells and three lamps. Most rules depend on them.",
        ["tour-claude"] = "This is Claude, the expert on the radio. It knows how to defuse the device, but it can't see it. Describe exactly what you see.",
        ["tour-manuals"] = "The device's manual pages are on the table. They're in Japanese: you can't read them, but Claude can.",
        ["tour-clock"] = "Here are the clock and your three strikes. I stop the clock whenever I talk, so listening never costs you time.",
        ["tour-controls"] = "Turn the device with these arrows, or drag with the right mouse button. Press Esc any time for the menu and the help.",
        ["tour-go"] = "That's everything. The clock starts now. Good luck!",
        ["tour-skipped"] = "Tour skipped. Press Esc any time for the menu, the help and the tour, and I'm here if you get stuck.",
        ["stuck-1"] = "Stuck? Pick one module, give Claude what it needs, and describe exactly what you see.",
        ["stuck-2"] = "Not sure what Claude needs? Ask it: \"What do you need from me?\"",
        ["stuck-3"] = "Unsure about an answer? Ask Claude \"Where does it say that?\" and find that rule number on your page.",
        ["follow-up"] = "You answered Claude's question, so now it can act. Good teamwork: give it exactly what it asks for.",
        ["clear"] = "Clear description! Module, casing and details in order: Claude could act on it right away.",
        ["vague"] = "Claude needed more details. It can't see the bomb: give the module, the casing and what you see, in order.",
        ["no-page"] = "Claude only knows what you give it. Without this module's manual page, it can't help.",
        ["over-blackout"] = "You blacked out some rules too. Hide only the private details, or Claude can't read what it needs.",
        ["blackout"] = "Clean blackout: names and IDs hidden, every rule still readable. Share what the task needs, nothing private.",
        ["newest"] = "Good call: you shared the newest version ({0}). Always check the date before you share a document.",
        ["privacy"] = "This page still shows names and ID numbers. Never give an AI private details it doesn't need: black them out first.",
        ["privacy-short"] = "Black out the private details first: names, IDs, phone numbers and codes.",
        ["strike-interlock"] = "Two linked switches were up at the same time. Ask Claude for a safe order, then flip one switch at a time.",
        ["strike-no-manual"] = "You acted before Claude had this module's manual. Get the page to Claude first, then follow its instruction.",
        ["strike-old-manual"] = "Claude followed an old version of the page ({0}). Check the dates and share the newest page.",
        ["strike-description"] = "Claude's answer is only as good as your description. Re-check what you reported, or ask: \"Where does it say that?\"",
        ["recap-win"] = "Device defused! Here's what you practised.",
        ["recap-lose"] = "The device went off. Here's what you practised, and what to try next time.",
    };

    // What the voice says for a line: the text without formatting or filled-in values.
    public static string Spoken(string id) =>
        Regex.Replace(Regex.Replace(Lines[id], @"\s*\(\{0\}\)", ""), "<[^>]+>", "").Replace(" - ", ", ");

    // Tips already said in this browser session - kept across Restart, so a tip
    // really comes once.
    static readonly HashSet<string> shownTips = new HashSet<string>();

    enum Outcome { ClearDescription, AnsweredFollowUp, CleanBlackout, NewestManual }
    readonly HashSet<Outcome> achieved = new HashSet<Outcome>();
    readonly List<string> strikeCauses = new List<string>();
    readonly Queue<(string id, string text, LumaRig.Mood mood)> queue = new Queue<(string, string, LumaRig.Mood)>();
    const LumaRig.Mood Talk = LumaRig.Mood.Talk, Cheer = LumaRig.Mood.Cheer, Concern = LumaRig.Mood.Concern;
    bool claudeAsked, awaitingAnswer, sharedOldManual;
    float sinceProgress;
    int stuckHints;

    AudioSource voice;
    Coroutine speaker;

    // The live 3D LUMA, for the tour and the debrief to show too.
    public Texture View => lumaCamera != null ? lumaCamera.targetTexture : null;
    public RectTransform ViewSlot => lumaView;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        voice = gameObject.AddComponent<AudioSource>();
        voice.playOnAwake = false;
        voice.spatialBlend = 0f; // LUMA speaks to the player, not from somewhere in the scene
        BuildLumaView();
        var gm = GameManager.Instance;
        gm.OnStrike += HandleStrike;
        gm.OnModuleSolved += HandleModuleSolved;
        gm.OnGameEnded += HandleGameEnded;
        if (PaperDesk.Instance != null)
        {
            PaperDesk.Instance.PageShared += HandlePageShared;
            PaperDesk.Instance.ShareBlocked += HandleShareBlocked;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReleaseClock(this);
            GameManager.Instance.OnStrike -= HandleStrike;
            GameManager.Instance.OnModuleSolved -= HandleModuleSolved;
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
        }
        if (PaperDesk.Instance != null)
        {
            PaperDesk.Instance.PageShared -= HandlePageShared;
            PaperDesk.Instance.ShareBlocked -= HandleShareBlocked;
        }
    }

    void Update()
    {
        // Stuck: no progress (solve, share, or an instruction from Claude) for a while.
        var gm = GameManager.Instance;
        if (gm.State != GameManager.RunState.Playing || !gm.ClockStarted || GamePause.IsPaused || speaker != null) return;
        sinceProgress += Time.deltaTime;
        if (sinceProgress < stuckHintSeconds * (stuckHints + 1)) return;
        stuckHints++;
        if (stuckHints <= 3) TipOnce("stuck-" + stuckHints);
    }

    // ---- What happens in the game ----

    // Claude's hidden reply tag (see ChatPanel): what the last exchange achieved.
    public void OnClaudeReply(string tag)
    {
        switch (tag)
        {
            case "ACT":
                sinceProgress = 0f;
                if (awaitingAnswer)
                {
                    achieved.Add(Outcome.AnsweredFollowUp);
                    TipOnce("follow-up", Cheer);
                }
                else
                {
                    achieved.Add(Outcome.ClearDescription);
                    TipOnce("clear", Cheer);
                }
                awaitingAnswer = false;
                break;
            case "ASK":
                claudeAsked = true;
                awaitingAnswer = true;
                TipOnce("vague", Concern);
                break;
            case "PAGE":
                TipOnce("no-page", Concern);
                break;
        }
    }

    void HandlePageShared(Paper p)
    {
        sinceProgress = 0f;
        if (p.HidesNonSecrets)
            TipOnce("over-blackout", Concern);
        else if (p.HasSecrets)
        {
            achieved.Add(Outcome.CleanBlackout);
            TipOnce("blackout", Cheer);
        }

        if (!PaperDesk.Instance.HasOlderVersion(p)) return;
        if (p.current)
        {
            achieved.Add(Outcome.NewestManual);
            TipOnce("newest", Cheer, p.date);
        }
        else sharedOldManual = true; // no tip now - the strike that follows teaches it
    }

    // Sharing refused because private information is still visible. Said every
    // time (it's the reason nothing was sent), the full explanation only the first.
    void HandleShareBlocked()
    {
        Say(shownTips.Add("privacy") ? "privacy" : "privacy-short", Concern);
    }

    void HandleModuleSolved(ModuleId module)
    {
        sinceProgress = 0f;
    }

    // One line on why the strike happened and what to do differently. The fatal
    // strike gets none - the recap covers it while the bomb goes off.
    void HandleStrike(ModuleId module)
    {
        var gm = GameManager.Instance;
        var shared = PaperDesk.Instance.SharedPages.Where(p => p.moduleId == module).ToList();
        string id, cause, date = null;
        if (module == ModuleId.Switches) { id = "strike-interlock"; cause = "switch interlock"; }
        else if (shared.Count == 0) { id = "strike-no-manual"; cause = "acted without the manual"; }
        else if (!shared.Any(p => p.current)) { id = "strike-old-manual"; cause = "old manual"; date = shared[shared.Count - 1].date; }
        else { id = "strike-description"; cause = "description"; }
        strikeCauses.Add(cause);

        if (gm.Strikes < gm.MaxStrikes) TipOnce(id, Concern, date); // each cause explained once
    }

    void HandleGameEnded(GameManager.RunState state)
    {
        StopSpeaking();
    }

    // ---- The recap, shown on the debrief screen ----

    public enum Practised { Yes, Missed, NotNeeded }

    public List<(Practised result, string label)> Recap()
    {
        Practised Of(Outcome o) => achieved.Contains(o) ? Practised.Yes : Practised.Missed;
        return new List<(Practised, string)>
        {
            (Of(Outcome.ClearDescription), "Described a module so Claude could act right away"),
            claudeAsked || achieved.Contains(Outcome.AnsweredFollowUp)
                ? (Of(Outcome.AnsweredFollowUp), "Answered Claude's follow-up question")
                : (Practised.NotNeeded, "Claude never needed to ask a follow-up"),
            (Of(Outcome.CleanBlackout), "Blacked out private information correctly"),
            (Of(Outcome.NewestManual), sharedOldManual ? "Chose the newest manual (an old one was shared)" : "Chose the newest manual version"),
        };
    }

    public string StrikeSummary() => strikeCauses.Count == 0 ? "No strikes."
        : $"Strikes: {strikeCauses.Count} (" + string.Join(", ", strikeCauses.GroupBy(c => c)
              .Select(g => g.Count() > 1 ? $"{g.Key} x{g.Count()}" : g.Key)) + ")";

    // LUMA's line over the recap, with the matching mood.
    public string ReadRecap(GameManager.RunState state)
    {
        string id = state == GameManager.RunState.Won ? "recap-win" : "recap-lose";
        if (luma != null) luma.SetMood(state == GameManager.RunState.Won ? Cheer : Concern);
        PlayVoice(id);
        return Lines[id];
    }

    // ---- Talking ----

    void TipOnce(string id, LumaRig.Mood mood = Talk, string value = null)
    {
        if (shownTips.Add(id)) Say(id, mood, value);
    }

    // Lines are said one at a time, in order.
    public void Say(string id, LumaRig.Mood mood = Talk, string value = null)
    {
        if (GameManager.Instance.State != GameManager.RunState.Playing) return;
        queue.Enqueue((id, string.Format(Lines[id], value), mood));
        if (speaker == null) speaker = StartCoroutine(SpeakQueue());
    }

    IEnumerator SpeakQueue()
    {
        var gm = GameManager.Instance;
        while (queue.Count > 0)
        {
            var line = queue.Dequeue();
            chat.AddLumaNote(line.text);
            if (luma != null) luma.SetMood(line.mood);
            gm.HoldClock(this);

            // Realtime waits: the help screen freezes game time, the voice keeps going.
            if (PlayVoice(line.id)) while (voice.isPlaying) yield return null;
            else yield return new WaitForSecondsRealtime(1.5f + line.text.Length * 0.03f);

            gm.ReleaseClock(this);
            if (luma != null) luma.SetMood(LumaRig.Mood.Idle);
            yield return new WaitForSecondsRealtime(0.35f);
        }
        speaker = null;
    }

    // For the tour: says one line aloud with a mood, without posting it in the chat.
    public IEnumerator Speak(string id, LumaRig.Mood mood)
    {
        if (luma != null) luma.SetMood(mood);
        if (PlayVoice(id)) while (voice.isPlaying) yield return null;
        else yield return new WaitForSecondsRealtime(1.5f + Lines[id].Length * 0.03f);
        if (luma != null) luma.SetMood(LumaRig.Mood.Idle);
    }

    // Stops a line said with Speak (the player moved on).
    public void Hush()
    {
        if (voice != null) voice.Stop();
        if (luma != null) luma.SetMood(LumaRig.Mood.Idle);
    }

    public void SetMood(LumaRig.Mood mood)
    {
        if (luma != null) luma.SetMood(mood);
    }

    void StopSpeaking()
    {
        if (speaker != null) StopCoroutine(speaker);
        speaker = null;
        if (voice != null) voice.Stop();
        if (GameManager.Instance != null) GameManager.Instance.ReleaseClock(this);
        if (luma != null) luma.SetMood(LumaRig.Mood.Idle);
    }

    bool PlayVoice(string id)
    {
        voice.Stop();
        if (!Settings.LumaVoice) return false; // the menu turned her voice off: the line shows, on a timer
        var clip = Resources.Load<AudioClip>("LumaVoice/" + id);
        if (clip == null) return false; // no recording for this line yet: the text still shows
        voice.clip = clip;
        voice.Play();
        return true;
    }

    // ---- UI construction ----

    // The live 3D LUMA in the chat header, on a teal-tinted card.
    void BuildLumaView()
    {
        if (lumaCamera == null || lumaView == null) return;
        var rt = new RenderTexture(440, 500, 24, RenderTextureFormat.ARGB32) { name = "LumaView", antiAliasing = 4 };
        lumaCamera.targetTexture = rt;
        lumaCamera.enabled = true; // always on: LUMA idles in the chat the whole game

        var card = Ui.NewPanel("LumaCard", lumaView, Ui.Surface2, 10);
        card.raycastTarget = false;
        Ui.Stretch(card.rectTransform);
        var view = Ui.NewRect("Luma3D", lumaView).gameObject.AddComponent<RawImage>();
        view.texture = rt;
        view.raycastTarget = false;
        Ui.Stretch(view.rectTransform);
    }
}
