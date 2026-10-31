using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Scrolling message list, input field, send button. Talks only to IAdvisor - it does
// not know or care whether that's FakeAdvisor or the real ClaudeClient. Locks input
// while a reply is in flight.
//
// Messages are bubbles: the player's on the right in navy, Claude's on the left in grey,
// LUMA's notes across the whole width in teal. The panel is light, in the Maaloomatiia
// style, with the OO emblem in its footer. It was laid out in the scene; Awake gives it
// the game's look (Ui) and its final layout, before PaperDesk measures it in Start.
public class ChatPanel : MonoBehaviour
{
    [SerializeField] MonoBehaviour advisorSource; // drag FakeAdvisor or ClaudeClient here - must implement IAdvisor
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button sendButton;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] RectTransform contentParent; // the ScrollRect's Content

    // Claude starts each reply with a hidden tag - [ACT] an instruction, [ASK] it
    // needs more from the operator, [PAGE] it needs a manual page, [INFO] anything
    // else - so Looma can react to how the conversation goes. Never shown.
    static readonly Regex ReplyTag = new Regex(@"^\s*\[(ACT|ASK|PAGE|INFO)\]\s*", RegexOptions.IgnoreCase);

    const float HeaderHeight = 265f, InputHeight = 112f, FooterHeight = 34f;

    // The whole panel, for the screens that lay themselves out beside it.
    public RectTransform Area => (RectTransform)scrollRect.transform.parent;

    IAdvisor advisor;
    TMP_Text pendingMessage; // Claude's bubble while the reply is on its way
    GameObject pendingDots;
    TMP_Text status;
    Image statusDot;
    bool busy;

    void Awake()
    {
        advisor = advisorSource as IAdvisor;
        if (advisor == null)
            Debug.LogError("[ChatPanel] Advisor Source does not implement IAdvisor.");
        Restyle();
    }

    void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        inputField.onSubmit.AddListener(_ => OnSendClicked());
        SetBusy(false);
    }

    void Update()
    {
        // The status light breathes while Claude is thinking.
        if (busy && statusDot != null)
            statusDot.color = Ui.Aqua.WithAlpha(0.25f + 0.75f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)));
    }

    void OnSendClicked()
    {
        string text = inputField.text.Trim();
        var desk = PaperDesk.Instance;
        bool hasPages = desk != null && desk.HasAttachments;
        if ((string.IsNullOrEmpty(text) && !hasPages) || advisor == null) return;

        // Attached manual pages go out with this message. A page that still shows
        // private information is blocked here, before anything is sent - Looma
        // explains why and the typed text stays in the box.
        string pagesNote = "";
        if (hasPages && !desk.TryShareAttachments(out pagesNote)) return;

        SoundManager.Instance.PlayMessageSound();
        string shown = hasPages ? $"<color=#9EAEBB><i>{pagesNote}</i></color>" + (text != "" ? "\n" + text : "") : text;
        AddBubble("You", Ui.Faint, Ui.Ink, shown, right: true, ink: Color.white);
        inputField.text = "";
        SetBusy(true);
        ShowPending();

        StartCoroutine(advisor.Send(hasPages ? $"{pagesNote}\n{text}" : text, OnReplyReceived));
    }

    // A quiet line across the chat, e.g. when the radio channel opens.
    public void AddDivider(string text)
    {
        var label = Ui.NewLabel("Divider", contentParent, text, 12f, Ui.Text2, TextAlignmentOptions.Center);
        label.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;
        ScrollToBottom();
    }

    // A note from LUMA, the coach (LoomaCoach).
    public void AddLumaNote(string text) => AddBubble("LUMA", Ui.TealDeep, Ui.Surface2, text, right: false, wide: true);

    void OnReplyReceived(string reply)
    {
        string tag = "";
        var match = ReplyTag.Match(reply);
        if (match.Success)
        {
            tag = match.Groups[1].Value.ToUpperInvariant();
            reply = reply.Substring(match.Length);
        }
        reply = reply.Replace("\u2014", " - ").Replace("\u2013", " - "); // the brand writes no em or en dashes

        SoundManager.Instance.PlayMessageSound();
        if (pendingMessage != null)
        {
            Destroy(pendingDots);
            pendingMessage.gameObject.SetActive(true);
            SetText(pendingMessage, reply, wide: false);
        }
        else AddBubble("Claude", Ui.Text2, Ui.Surface3, reply, right: false);

        pendingMessage = null;
        SetBusy(false);
        // The typing dots just grew into the full reply: scroll again, or the end of a
        // long reply stays hidden behind the input box.
        ScrollToBottom();

        if (tag != "" && LoomaCoach.Instance != null) LoomaCoach.Instance.OnClaudeReply(tag);
    }

    // ---- Bubbles ----

    // One message: a row that holds the bubble on the left or the right; the bubble
    // holds the sender's name and the text, no wider than most of the panel.
    TMP_Text AddBubble(string sender, Color nameColor, Color bubbleColor, string text, bool right, bool wide = false, Color? ink = null)
    {
        var row = Ui.NewRect(sender + " message", contentParent);
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = right ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        rowLayout.childControlWidth = rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = wide;
        rowLayout.childForceExpandHeight = false;

        var bubble = Ui.NewPanel("Bubble", row, bubbleColor, 8);
        bubble.raycastTarget = false;
        var layout = bubble.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 10, 13);
        layout.spacing = 4f;
        layout.childAlignment = right ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = wide;
        layout.childForceExpandHeight = false;

        var name = Ui.NewLabel("Name", bubble.transform, sender, 12f, nameColor, right ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft);
        name.textWrappingMode = TextWrappingModes.NoWrap;

        var body = Ui.NewText("Text", bubble.transform, "", 19f, ink ?? Ui.Ink);
        body.lineSpacing = 4f;
        body.gameObject.AddComponent<LayoutElement>();
        SetText(body, text, wide);

        ScrollToBottom();
        return body;
    }

    // Sets a bubble's text and its width: as wide as the text needs, at most the cap.
    void SetText(TMP_Text body, string text, bool wide)
    {
        body.text = text;
        float max = Mathf.Max(200f, contentParent.rect.width * 0.82f - 32f);
        var element = body.GetComponent<LayoutElement>();
        element.preferredWidth = wide ? 4000f : Mathf.Min(body.GetPreferredValues(text, max, 0f).x + 2f, max);
    }

    // Claude's bubble before the reply arrives: three dots that pulse in turn.
    void ShowPending()
    {
        pendingMessage = AddBubble("Claude", Ui.Text2, Ui.Surface3, "", right: false);
        pendingMessage.gameObject.SetActive(false);

        var dots = Ui.NewRect("Dots", pendingMessage.transform.parent);
        var layout = dots.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 7f;
        layout.padding = new RectOffset(0, 0, 6, 4);
        layout.childControlWidth = layout.childControlHeight = false;
        var images = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            images[i] = Ui.NewIcon("Dot", dots, Ui.Circle, Ui.Aqua, 10f);
            var size = images[i].gameObject.AddComponent<LayoutElement>();
            size.preferredWidth = size.preferredHeight = 10f;
        }
        pendingDots = dots.gameObject;
        StartCoroutine(PulseDots(images));
        ScrollToBottom();
    }

    IEnumerator PulseDots(Image[] dots)
    {
        while (dots[0] != null)
        {
            for (int i = 0; i < dots.Length; i++)
                dots[i].color = Ui.Aqua.WithAlpha(0.25f + 0.75f * Mathf.Max(0f, Mathf.Sin(Time.unscaledTime * 6f - i * 0.9f)));
            yield return null;
        }
    }

    // Rebuild the content's layout now so the new height is known before scrolling,
    // then pin to the bottom once more next frame, after TMP has settled the
    // final height of any wrapped lines.
    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        scrollRect.verticalNormalizedPosition = 0f;
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void SetBusy(bool busy)
    {
        this.busy = busy;
        inputField.interactable = !busy;
        sendButton.interactable = !busy;
        if (!busy) inputField.ActivateInputField();
        if (status != null) status.text = busy ? "CLAUDE IS THINKING..." : "DISPOSAL EXPERT - ON THE RADIO";
        if (statusDot != null && !busy) statusDot.color = Ui.Aqua;
    }

    // ---- The panel's look ----

    // The header's pieces are found by name: they were laid out in the scene before
    // this styling existed.
    void Restyle()
    {
        var panel = Area;
        if (panel.TryGetComponent(out Image background))
        {
            background.sprite = null; // the scene's grey panel sprite would tint the white
            background.color = Color.white; // opaque: the device isn't drawn behind it
        }
        var edge = Ui.NewImage("Edge", panel, Ui.Border2);
        edge.raycastTarget = false;
        edge.rectTransform.anchorMin = Vector2.zero;
        edge.rectTransform.anchorMax = new Vector2(0f, 1f);
        edge.rectTransform.pivot = new Vector2(0f, 0.5f);
        edge.rectTransform.sizeDelta = new Vector2(1f, 0f);

        // Header: who is on the radio, and whether they're thinking.
        var header = panel.Find("Chat");
        var title = header != null ? header.Find("ChatText")?.GetComponent<TMP_Text>() : null;
        if (title != null)
        {
            title.text = "Claude";
            title.font = Ui.Display;
            title.fontSize = 28f;
            title.fontStyle = FontStyles.Normal;
            title.characterSpacing = 0f;
            title.color = Ui.Ink;
            title.alignment = TextAlignmentOptions.TopLeft;
            title.raycastTarget = false; // it reaches over the gear button
            TopStrip(title.rectTransform, 46f, -18f, 40f, 80f);

            statusDot = Ui.NewIcon("Status", header, Ui.Circle, Ui.Aqua, 12f);
            Ui.Place(statusDot.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -30f), new Vector2(12f, 12f));
            status = Ui.NewLabel("StatusText", header, "", 12f, Ui.Text2);
            TopStrip(status.rectTransform, 47f, -58f, 20f);
        }

        var luma = header != null ? (RectTransform)header.Find("LumaView") : null;
        if (luma != null)
        {
            Ui.Place(luma, new Vector2(0f, 1f), new Vector2(16f, -94f), new Vector2(140f, 160f));
            var name = Ui.NewHeading("LumaName", header, "LUMA", 20f, Ui.TealDeep);
            TopStrip(name.rectTransform, 172f, -126f, 26f);
            var role = Ui.NewText("LumaRole", header, "Your coach. I'll speak up when I have a tip, and stop the clock while I do.", 16f, Ui.Text2);
            TopStrip(role.rectTransform, 172f, -158f, 80f);
        }

        var line = Ui.NewImage("HeaderLine", panel, Ui.Border2);
        line.raycastTarget = false;
        TopStrip(line.rectTransform, 0f, -HeaderHeight, 1f, 0f);

        var gear = panel.Find("SettingsBtn");
        if (gear != null && gear.TryGetComponent(out Image gearImage))
        {
            gearImage.color = Ui.Text2;
            ((RectTransform)gear).sizeDelta = new Vector2(40f, 40f);
            gear.gameObject.AddComponent<ButtonFeel>();
        }

        // Messages fill the space between the header and the input box.
        var scroll = (RectTransform)scrollRect.transform;
        scroll.anchorMin = Vector2.zero;
        scroll.anchorMax = Vector2.one;
        scroll.offsetMin = new Vector2(4f, InputHeight + FooterHeight + 4f);
        scroll.offsetMax = new Vector2(-4f, -HeaderHeight - 2f);
        if (scroll.TryGetComponent(out Image scrollBackground)) scrollBackground.color = Color.clear;
        if (contentParent.TryGetComponent(out VerticalLayoutGroup list))
        {
            list.padding = new RectOffset(12, 12, 14, 14);
            list.spacing = 10f;
            list.childAlignment = TextAnchor.UpperLeft;
            list.childControlWidth = list.childControlHeight = true;
            list.childForceExpandWidth = true;
            list.childForceExpandHeight = false;
        }

        // The input box and a round send button beside it.
        var input = (RectTransform)inputField.transform.parent;
        input.sizeDelta = new Vector2(input.sizeDelta.x, InputHeight);
        input.anchoredPosition = new Vector2(input.anchoredPosition.x, FooterHeight);
        var inputLine = Ui.NewImage("InputLine", input, Ui.Border2);
        inputLine.raycastTarget = false;
        TopStrip(inputLine.rectTransform, 0f, 0f, 1f, 0f);
        var field = (RectTransform)inputField.transform;
        field.anchorMin = Vector2.zero;
        field.anchorMax = Vector2.one;
        field.offsetMin = new Vector2(14f, 14f);
        field.offsetMax = new Vector2(-90f, -14f);
        Ui.Style(inputField, "Describe what you see...", 19f);

        var send = (RectTransform)sendButton.transform;
        Ui.Place(send, new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(66f, 66f));
        Ui.Style(sendButton, Ui.ButtonStyle.Primary);
        foreach (var label in sendButton.GetComponentsInChildren<TMP_Text>(true)) label.gameObject.SetActive(false);
        Ui.NewIcon("Arrow", send, Ui.Triangle, Color.white, 34f).rectTransform.anchoredPosition = new Vector2(3f, 0f);

        // The footer: who made it, and the OO emblem bottom-right, as on every Maaloomatiia page.
        var maker = Ui.NewText("Maker", panel, "Maaloomatiia Academy", 13f, Ui.Text2, TextAlignmentOptions.MidlineLeft);
        Ui.Place(maker.rectTransform, Vector2.zero, new Vector2(16f, 0f), new Vector2(300f, FooterHeight));
        var emblem = Ui.NewEmblem(panel, dark: false, 20f);
        emblem.rectTransform.anchorMin = emblem.rectTransform.anchorMax = emblem.rectTransform.pivot = new Vector2(1f, 0f);
        emblem.rectTransform.anchoredPosition = new Vector2(-16f, 7f);
    }

    // A strip along the top of its parent: insets from the left and right, top
    // (negative, downward) and height.
    static void TopStrip(RectTransform rt, float left, float top, float height, float right = 20f)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(left, top);
        rt.sizeDelta = new Vector2(-left - right, height);
    }
}
