using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// LUMA, the learning coach. Claude stays the teammate inside the story; LUMA
// steps outside it to name what the player just learned, explain a strike or a
// blocked share, hint (never answer) when they're stuck, and recap the learning
// outcomes at the end.
//
// LUMA lives in the chat: the 3D character is always visible in the chat header,
// and when it has something to say it animates, reads the line aloud and posts it
// in the chat in blue (the player is white, Claude green). Each tip is said once,
// and the clock is held while LUMA talks, so listening never costs time. Lines
// play one after another; their voices are recorded files in Resources/LumaVoice,
// one per line id (regenerate with Claude Bomb > Generate LUMA Voice).
// The game clock starts when LUMA's introduction is over or skipped.
public class LoomaCoach : MonoBehaviour
{
    public static LoomaCoach Instance { get; private set; }

    [SerializeField] ChatPanel chat;
    [SerializeField] Camera lumaCamera;          // films the 3D LUMA on its stage
    [SerializeField] LumaRig luma;
    [SerializeField] RectTransform lumaView;     // where LUMA appears in the chat header
    [SerializeField] TMP_FontAsset titleFont;
    [SerializeField] TMP_FontAsset bodyFont;
    [SerializeField] Color lumaColor = new Color(0.42f, 0.70f, 1f);
    [SerializeField] float stuckHintSeconds = 120f;

    static readonly Color Accent = new Color(0.30f, 0.80f, 0.75f);
    const float RecapWidth = 900f;

    // Everything LUMA says, by id. The id names the line's voice file. {0} is a
    // value filled in at the time (a date); it's left out of the spoken version.
    public static readonly Dictionary<string, string> Lines = new Dictionary<string, string>
    {
        ["intro-mission"] = "<b>Mission:</b> defuse the bomb in front of you. Only Claude, in the chat, knows how - but it can't see the bomb. It only knows what you tell it.",
        ["intro-goal"] = "<b>Goal:</b> solve all five modules. You have three lives, and the 10-minute clock starts as soon as I'm done talking.",
        ["intro-howto"] = "<b>How to play:</b> turn the bomb with the arrows (bottom right) and find the pages under MANUALS (bottom left). Full help: the gear icon.",
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
        ["recap-win"] = "<b>Bomb defused!</b> Here's what you practised:",
        ["recap-lose"] = "<b>The bomb went off.</b> Here's what you practised:",
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
    bool claudeAsked, awaitingAnswer, sharedOldManual, introRunning;
    float sinceProgress;
    int stuckHints;

    AudioSource voice;
    Coroutine speaker;
    GameObject skipButton, recap;
    TMP_Text recapText;

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
        BuildRecap();
        var gm = GameManager.Instance;
        gm.OnStrike += HandleStrike;
        gm.OnModuleSolved += HandleModuleSolved;
        gm.OnGameEnded += HandleGameEnded;
        if (PaperDesk.Instance != null)
        {
            PaperDesk.Instance.PageShared += HandlePageShared;
            PaperDesk.Instance.ShareBlocked += HandleShareBlocked;
        }
        StartCoroutine(Intro());
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
        var kb = Keyboard.current;
        if (introRunning && kb != null && kb.escapeKey.wasPressedThisFrame) SkipIntro();

        // Stuck: no progress (solve, share, or an instruction from Claude) for a while.
        var gm = GameManager.Instance;
        if (gm.State != GameManager.RunState.Playing || !gm.ClockStarted || GamePause.IsPaused || speaker != null) return;
        sinceProgress += Time.deltaTime;
        if (sinceProgress < stuckHintSeconds * (stuckHints + 1)) return;
        stuckHints++;
        if (stuckHints <= 3) TipOnce("stuck-" + stuckHints);
    }

    // ---- What happens in the game ----

    IEnumerator Intro()
    {
        yield return null;
        while (!ClaudeClient.Ready) yield return null; // after the access code, if one is needed
        if (!shownTips.Add("intro"))
        {
            GameManager.Instance.StartClock(); // heard it already (Restart): straight to the game
            yield break;
        }
        introRunning = true;
        skipButton.SetActive(true);
        Say("intro-mission", LumaRig.Mood.Wave);
        Say("intro-goal");
        Say("intro-howto");
    }

    // The introduction is over (or skipped): the countdown begins.
    void EndIntro()
    {
        if (!introRunning) return;
        introRunning = false;
        skipButton.SetActive(false);
        GameManager.Instance.StartClock();
    }

    // Stops the voice and puts the rest of the introduction in the chat at once,
    // so it can still be read - then the game starts.
    void SkipIntro()
    {
        if (!introRunning) return;
        StopSpeaking();
        while (queue.Count > 0) chat.AddNote("<b>LUMA:</b> " + queue.Dequeue().text, lumaColor);
        EndIntro();
    }

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
        StartCoroutine(Recap(state));
    }

    IEnumerator Recap(GameManager.RunState state)
    {
        if (state == GameManager.RunState.Lost) yield return new WaitForSeconds(3.2f); // after explosion + smoke

        string Line(Outcome o, string label) =>
            (achieved.Contains(o) ? "<color=#5BD08B>YES</color>" : "<color=#E36A4D>MISSED</color>") + "    " + label;

        string id = state == GameManager.RunState.Won ? "recap-win" : "recap-lose";
        var lines = new List<string>
        {
            Lines[id],
            Line(Outcome.ClearDescription, "Described a module so Claude could act right away"),
            claudeAsked || achieved.Contains(Outcome.AnsweredFollowUp)
                ? Line(Outcome.AnsweredFollowUp, "Answered Claude's follow-up question")
                : "<color=#9A9A9A>-</color>         Claude never needed to ask a follow-up",
            Line(Outcome.CleanBlackout, "Blacked out private information correctly"),
            Line(Outcome.NewestManual, sharedOldManual ? "Chose the newest manual (an old one was shared)" : "Chose the newest manual version"),
            strikeCauses.Count == 0 ? "No strikes."
                : $"Strikes: {strikeCauses.Count} (" + string.Join(", ", strikeCauses.GroupBy(c => c)
                      .Select(g => g.Count() > 1 ? $"{g.Key} x{g.Count()}" : g.Key)) + ")",
        };
        recapText.text = "<size=30><b>LUMA'S RECAP</b></size>\n" + string.Join("\n", lines);
        recap.SetActive(true);
        if (luma != null) luma.SetMood(state == GameManager.RunState.Won ? Cheer : Concern);
        PlayVoice(id);
    }

    // ---- Talking ----

    void TipOnce(string id, LumaRig.Mood mood = Talk, string value = null)
    {
        if (shownTips.Add(id)) Say(id, mood, value);
    }

    // Lines are said one at a time, in order.
    void Say(string id, LumaRig.Mood mood = Talk, string value = null)
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
            chat.AddNote("<b>LUMA:</b> " + line.text, lumaColor);
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
        EndIntro();
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
        var clip = Resources.Load<AudioClip>("LumaVoice/" + id);
        if (clip == null) return false; // no recording for this line yet: the text still shows
        voice.clip = clip;
        voice.Play();
        return true;
    }

    // ---- UI construction ----

    // The live 3D LUMA in the chat header, plus the intro's SKIP button beside it.
    void BuildLumaView()
    {
        if (lumaCamera != null && lumaView != null)
        {
            var rt = new RenderTexture(440, 500, 24, RenderTextureFormat.ARGB32) { name = "LumaView", antiAliasing = 4 };
            lumaCamera.targetTexture = rt;
            lumaCamera.enabled = true; // always on: LUMA idles in the chat the whole game
            var view = new GameObject("Luma3D", typeof(RectTransform)).AddComponent<RawImage>();
            view.transform.SetParent(lumaView, false);
            view.texture = rt;
            view.raycastTarget = false;
            Stretch(view.rectTransform);
        }

        var parent = lumaView != null ? lumaView.parent : chat.transform;
        var button = NewImage("SkipIntro", parent, new Color(0.32f, 0.34f, 0.38f));
        var brt = button.rectTransform;
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(1f, 1f);
        brt.anchoredPosition = new Vector2(-16f, -160f);
        brt.sizeDelta = new Vector2(200f, 44f);
        button.gameObject.AddComponent<Button>().onClick.AddListener(() =>
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            SkipIntro();
        });
        var label = NewText("Label", button.transform, "SKIP INTRO", titleFont, 18, TextAlignmentOptions.Center);
        label.color = new Color(0.93f, 0.93f, 0.9f);
        Stretch(label.rectTransform);
        skipButton = button.gameObject;
        skipButton.SetActive(false);
    }

    // The end-of-game recap, under the result and the Restart button.
    void BuildRecap()
    {
        var canvasGo = new GameObject("LumaRecapCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 25; // over the end screen
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var card = NewImage("Recap", canvasGo.transform, new Color(0.11f, 0.12f, 0.15f, 0.97f));
        var rt = card.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 12f);
        rt.sizeDelta = new Vector2(RecapWidth, 440f);
        var outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = Accent;
        outline.effectDistance = new Vector2(2f, -2f);
        recap = card.gameObject;

        recapText = NewText("Text", recap.transform, "", bodyFont, 24, TextAlignmentOptions.TopLeft);
        recapText.color = new Color(0.93f, 0.93f, 0.9f);
        var trt = recapText.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(40f, 70f);
        trt.offsetMax = new Vector2(-40f, -30f);
        recapText.enableAutoSizing = true;
        recapText.fontSizeMin = 16;
        recapText.fontSizeMax = 24;

        var button = NewImage("Close", recap.transform, new Color(0.95f, 0.94f, 0.9f));
        var brt = button.rectTransform;
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(1f, 0f);
        brt.anchoredPosition = new Vector2(-24f, 18f);
        brt.sizeDelta = new Vector2(200f, 46f);
        button.gameObject.AddComponent<Button>().onClick.AddListener(() => { voice.Stop(); recap.SetActive(false); });
        var label = NewText("Label", button.transform, "GOT IT", titleFont, 20, TextAlignmentOptions.Center);
        label.color = new Color(0.13f, 0.13f, 0.15f);
        Stretch(label.rectTransform);

        recap.SetActive(false);
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static TMP_Text NewText(string name, Transform parent, string text, TMP_FontAsset font, float size, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.alignment = alignment;
        t.raycastTarget = false;
        t.text = text;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
