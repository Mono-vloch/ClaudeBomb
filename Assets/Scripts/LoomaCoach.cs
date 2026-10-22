using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Looma, the learning coach. Claude stays the teammate inside the story; Looma
// steps outside it to name what the player just learned, explain a strike or a
// blocked share, hint (never answer) when they're stuck, and recap the learning
// outcomes at the end. The course team's rules: one or two lines, each tip shown
// once, the clock paused while Looma is on screen, and the player dismisses it.
// Builds its own UI at runtime; set `portrait` once Looma's art arrives.
public class LoomaCoach : MonoBehaviour
{
    public static LoomaCoach Instance { get; private set; }

    [SerializeField] Sprite portrait;            // Looma's art - a placeholder is drawn while empty
    [SerializeField] TMP_FontAsset titleFont;
    [SerializeField] TMP_FontAsset bodyFont;
    [SerializeField] RectTransform chatArea;     // cards sit over the bomb, left of the chat
    [SerializeField] float stuckHintSeconds = 120f;

    static readonly Color Accent = new Color(0.30f, 0.80f, 0.75f);
    static readonly Color CardColor = new Color(0.11f, 0.12f, 0.15f, 0.97f);
    const float CardWidth = 900f;

    // Tips already shown in this browser session - kept across Restart, so a tip
    // really appears only once.
    static readonly HashSet<string> shownTips = new HashSet<string>();

    enum Outcome { ClearDescription, AnsweredFollowUp, CleanBlackout, NewestManual }
    readonly HashSet<Outcome> achieved = new HashSet<Outcome>();
    readonly List<string> strikeCauses = new List<string>();
    readonly Queue<string> queue = new Queue<string>();
    bool claudeAsked, awaitingAnswer, sharedOldManual, cardOpen;
    float sinceProgress;
    int stuckHints;

    GameObject blocker, card;
    RectTransform cardRect;
    float tipX;
    TMP_Text cardText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildUI();
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
        GamePause.Release(this);
        if (GameManager.Instance != null)
        {
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
        if (cardOpen)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                Dismiss();
            return;
        }

        // Stuck: no progress (solve, share, or an instruction from Claude) for a while.
        if (GameManager.Instance.State != GameManager.RunState.Playing || GamePause.IsPaused) return;
        sinceProgress += Time.deltaTime;
        if (sinceProgress < stuckHintSeconds * (stuckHints + 1)) return;
        stuckHints++;
        if (stuckHints == 1) TipOnce("stuck-1", "Stuck? Pick one module, give Claude what it needs, and describe exactly what you see.");
        else if (stuckHints == 2) TipOnce("stuck-2", "Not sure what Claude needs? Ask it: \"What do you need from me?\"");
        else if (stuckHints == 3) TipOnce("stuck-3", "Unsure about an answer? Ask Claude \"Where does it say that?\" and find that rule number on your page.");
    }

    // ---- What happens in the game ----

    IEnumerator Intro()
    {
        yield return null;
        while (!ClaudeClient.Ready) yield return null; // after the access code, if one is needed
        if (!shownTips.Add("intro")) yield break;
        Show("<b>Mission:</b> defuse the bomb in front of you. Only Claude, in the chat, knows how - but it can't see the bomb. It only knows what you tell it.");
        Show("<b>Goal:</b> solve all five modules. You have three lives, and the clock only starts after your first module.");
        Show("<b>How to play:</b> turn the bomb with the arrows (bottom right) and find the pages under MANUALS (bottom left). Full help: the gear icon.");
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
                    TipOnce("follow-up", "You answered Claude's question, so now it can act. Good teamwork: give it exactly what it asks for.");
                }
                else
                {
                    achieved.Add(Outcome.ClearDescription);
                    TipOnce("clear", "Clear description! Module, casing and details in order: Claude could act on it right away.");
                }
                awaitingAnswer = false;
                break;
            case "ASK":
                claudeAsked = true;
                awaitingAnswer = true;
                TipOnce("vague", "Claude needed more details. It can't see the bomb: give the module, the casing and what you see, in order.");
                break;
            case "PAGE":
                TipOnce("no-page", "Claude only knows what you give it. Without this module's manual page, it can't help.");
                break;
        }
    }

    void HandlePageShared(Paper p)
    {
        sinceProgress = 0f;
        if (p.HidesNonSecrets)
            TipOnce("over-blackout", "You blacked out some rules too. Hide only the private details, or Claude can't read what it needs.");
        else if (p.HasSecrets)
        {
            achieved.Add(Outcome.CleanBlackout);
            TipOnce("blackout", "Clean blackout: names and IDs hidden, every rule still readable. Share what the task needs, nothing private.");
        }

        if (!PaperDesk.Instance.HasOlderVersion(p)) return;
        if (p.current)
        {
            achieved.Add(Outcome.NewestManual);
            TipOnce("newest", $"Good call: you shared the newest version ({p.date}). Always check the date before you share a document.");
        }
        else sharedOldManual = true; // no tip now - the strike that follows teaches it
    }

    // Sharing refused because private information is still visible. Shown every
    // time (it's the reason nothing was sent), the full explanation only the first.
    void HandleShareBlocked()
    {
        if (shownTips.Add("privacy"))
            Show("This page still shows names and ID numbers. Never give an AI private details it doesn't need: black them out first.");
        else
            Show("Black out the private details first: names, IDs, phone numbers and codes.");
    }

    void HandleModuleSolved(ModuleId module)
    {
        sinceProgress = 0f;
        TipOnce("clock", "First module done! The clock is running now.");
    }

    // One line on why the strike happened and what to do differently. The fatal
    // strike gets no card - the recap covers it while the bomb goes off.
    void HandleStrike(ModuleId module)
    {
        var gm = GameManager.Instance;
        var shared = PaperDesk.Instance.SharedPages.Where(p => p.moduleId == module).ToList();
        string cause, line;
        if (module == ModuleId.Switches)
        {
            cause = "switch interlock";
            line = "Two linked switches were up at the same time. Ask Claude for a safe order, then flip one switch at a time.";
        }
        else if (shared.Count == 0)
        {
            cause = "acted without the manual";
            line = "You acted before Claude had this module's manual. Get the page to Claude first, then follow its instruction.";
        }
        else if (!shared.Any(p => p.current))
        {
            var old = shared[shared.Count - 1];
            cause = "old manual";
            line = $"Claude followed the page you shared, dated {old.date} - an old version. Check the dates and share the newest page.";
        }
        else
        {
            cause = "description";
            line = "Claude's answer is only as good as your description. Re-check what you reported, or ask: \"Where does it say that?\"";
        }
        strikeCauses.Add(cause);

        if (gm.Strikes < gm.MaxStrikes) TipOnce("strike-" + cause, line); // each cause explained once
    }

    void HandleGameEnded(GameManager.RunState state)
    {
        // The end screen runs on the clock (explosion, smoke): close any card, unfreeze.
        queue.Clear();
        CloseCard();
        StartCoroutine(Recap(state));
    }

    IEnumerator Recap(GameManager.RunState state)
    {
        if (state == GameManager.RunState.Lost) yield return new WaitForSeconds(3.2f); // after explosion + smoke

        string Line(Outcome o, string label) =>
            (achieved.Contains(o) ? "<color=#5BD08B>YES</color>" : "<color=#E36A4D>MISSED</color>") + "    " + label;

        var lines = new List<string>
        {
            state == GameManager.RunState.Won ? "<b>Bomb defused!</b> Here's what you practised:" : "<b>The bomb went off.</b> Here's what you practised:",
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
        OpenCard("<size=30><b>LOOMA'S RECAP</b></size>\n" + string.Join("\n", lines), recap: true);
    }

    // ---- Showing cards ----

    void TipOnce(string id, string text)
    {
        if (shownTips.Add(id)) Show(text);
    }

    void Show(string text)
    {
        if (GameManager.Instance.State != GameManager.RunState.Playing) return;
        queue.Enqueue(text);
        if (!cardOpen) ShowNext();
    }

    void ShowNext()
    {
        if (queue.Count == 0) { CloseCard(); return; }
        OpenCard(queue.Dequeue(), recap: false);
        GamePause.Hold(this);
    }

    void Dismiss()
    {
        // A clicked GOT IT stays selected; Enter would then "click" it again and skip a card.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        if (queue.Count > 0) ShowNext();
        else CloseCard();
    }

    void OpenCard(string text, bool recap)
    {
        cardText.text = text;
        // Tips sit over the bomb, above the rotate arrows. The recap sits centred
        // under the end screen's result and buttons, so Restart stays visible.
        cardRect.sizeDelta = new Vector2(CardWidth, recap ? 440f : 200f);
        cardRect.anchoredPosition = recap ? new Vector2((1920f - CardWidth) / 2f, 12f) : new Vector2(tipX, 270f);
        blocker.SetActive(!recap); // the recap leaves the end screen's Restart reachable
        card.SetActive(true);
        cardOpen = true;
        // Typing in the chat while Looma is up would turn the pause into free time.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void CloseCard()
    {
        card.SetActive(false);
        blocker.SetActive(false);
        cardOpen = false;
        GamePause.Release(this);
    }

    // ---- UI construction ----

    void BuildUI()
    {
        var canvasGo = new GameObject("LoomaCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 25; // over the chat and the manual table
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Dims the game and swallows clicks, so the only way on is to dismiss Looma.
        var dim = NewImage("Blocker", canvasGo.transform, new Color(0f, 0f, 0f, 0.35f));
        Stretch(dim.rectTransform);
        blocker = dim.gameObject;

        float chatWidth = chatArea != null ? chatArea.rect.width : 590f;
        float bombAreaWidth = 1920f - chatWidth;
        var cardImg = NewImage("Card", canvasGo.transform, CardColor);
        cardRect = cardImg.rectTransform;
        cardRect.anchorMin = cardRect.anchorMax = cardRect.pivot = Vector2.zero;
        tipX = (bombAreaWidth - CardWidth) / 2f;
        cardRect.anchoredPosition = new Vector2(tipX, 270f); // above the rotate arrows
        cardRect.sizeDelta = new Vector2(CardWidth, 200f);
        var outline = cardImg.gameObject.AddComponent<Outline>();
        outline.effectColor = Accent;
        outline.effectDistance = new Vector2(2f, -2f);
        card = cardImg.gameObject;

        var face = NewImage("Portrait", card.transform, Color.white);
        face.sprite = portrait != null ? portrait : PlaceholderPortrait();
        face.preserveAspect = true;
        var frt = face.rectTransform;
        frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(0f, 1f);
        frt.anchoredPosition = new Vector2(24f, -24f);
        frt.sizeDelta = new Vector2(120f, 120f);
        if (portrait == null)
        {
            var initial = NewText("Initial", face.transform, "L", titleFont, 64, TextAlignmentOptions.Center);
            initial.color = new Color(0.08f, 0.18f, 0.18f);
            Stretch(initial.rectTransform);
        }

        var nameText = NewText("Name", card.transform, "LOOMA", titleFont, 20, TextAlignmentOptions.TopLeft);
        nameText.color = Accent;
        var nrt = nameText.rectTransform;
        nrt.anchorMin = nrt.anchorMax = nrt.pivot = new Vector2(0f, 1f);
        nrt.anchoredPosition = new Vector2(168f, -22f);
        nrt.sizeDelta = new Vector2(300f, 30f);

        cardText = NewText("Text", card.transform, "", bodyFont, 24, TextAlignmentOptions.TopLeft);
        cardText.color = new Color(0.93f, 0.93f, 0.9f);
        var trt = cardText.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(168f, 70f);
        trt.offsetMax = new Vector2(-28f, -56f);
        cardText.enableAutoSizing = true;
        cardText.fontSizeMin = 16;
        cardText.fontSizeMax = 24;

        var button = NewImage("GotIt", card.transform, new Color(0.95f, 0.94f, 0.9f));
        var brt = button.rectTransform;
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(1f, 0f);
        brt.anchoredPosition = new Vector2(-24f, 18f);
        brt.sizeDelta = new Vector2(200f, 46f);
        button.gameObject.AddComponent<Button>().onClick.AddListener(Dismiss);
        var label = NewText("Label", button.transform, "GOT IT", titleFont, 20, TextAlignmentOptions.Center);
        label.color = new Color(0.13f, 0.13f, 0.15f);
        Stretch(label.rectTransform);

        card.SetActive(false);
        blocker.SetActive(false);
    }

    // A round teal face until the real Looma art is set.
    static Sprite PlaceholderPortrait()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - r + 0.5f) * (x - r + 0.5f) + (y - r + 0.5f) * (y - r + 0.5f));
                byte a = (byte)(Mathf.Clamp01(r - d) * 255f); // soft 1-pixel edge
                pixels[y * size + x] = new Color32(77, 204, 191, a);
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
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
