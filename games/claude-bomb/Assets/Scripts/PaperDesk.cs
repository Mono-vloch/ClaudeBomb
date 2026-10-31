using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// The manual table and everything the player does with the pages: the papers on
// the table, the close-up reader with its black marker, and dragging a page into
// the chat. A page that still shows private information is blocked from sharing;
// Looma (LoomaCoach) explains why, and also reads what was shared to recognise a
// clean blackout or the newest version. Builds its own UI at runtime, so the
// scene only needs this component and its references.
public class PaperDesk : MonoBehaviour
{
    public static PaperDesk Instance { get; private set; }

    [SerializeField] ClaudeClient claude;
    [SerializeField] TMP_FontAsset paperFont;   // page text - must cover Japanese (Noto Sans JP)
    [SerializeField] RectTransform chatArea;    // dropping a page anywhere on this attaches it
    [SerializeField] RectTransform chatInput;   // the attachment bar sits just above this
    [SerializeField] RectTransform chatScroll;  // shortened to make room for the attachment bar
    [SerializeField] bool openAtStart = true;

    // A page went out to Claude (once per page per share).
    public event Action<Paper> PageShared;
    // Sharing was refused because private information is still visible.
    public event Action ShareBlocked;

    static readonly Color DeskColor = new Color(0.16f, 0.12f, 0.09f, 0.97f);
    static readonly Color PaperColor = new Color(0.96f, 0.93f, 0.86f);
    static readonly Color OldPaperColor = new Color(0.89f, 0.83f, 0.66f); // older copies have yellowed
    static readonly Color InkColor = new Color(0.13f, 0.13f, 0.15f);
    static readonly Color HintColor = new Color(0.88f, 0.85f, 0.77f);

    List<Paper> papers;
    readonly List<Paper> attached = new List<Paper>();
    readonly List<Paper> shared = new List<Paper>();
    readonly Dictionary<Paper, TMP_Text> cardTexts = new Dictionary<Paper, TMP_Text>();
    readonly List<Image> redactionBars = new List<Image>();

    Canvas deskCanvas, topCanvas;
    GameObject table, reader, manualsButton, attachBar;
    Vector2 scrollMinDefault, scrollMinWithBar;
    Image readerPaper;
    TMP_Text readerHeader, readerBody, attachText;
    RectTransform dragGhost;
    Paper reading;
    List<Paper.Segment> readingSegments;
    bool markerPaint; // what a marker stroke paints: true = black out, false = remove the black

    public bool HasAttachments => attached.Count > 0;
    // A page is open up close (the HUD steps aside for it).
    public bool ReaderOpen => reader != null && reader.activeInHierarchy;
    // The MANUALS button on the device view, for the tour to point at.
    public RectTransform ManualsButton => manualsButton != null ? (RectTransform)manualsButton.transform : null;
    public IReadOnlyList<Paper> SharedPages => shared;

    // Whether the table also holds a superseded copy of this page's module.
    public bool HasOlderVersion(Paper p) => papers.Any(o => o.moduleId == p.moduleId && !o.current);
    public Paper NewestPageFor(ModuleId module) => papers.FirstOrDefault(p => p.moduleId == module && p.current);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        papers = Paper.LoadAll();
        deskCanvas = CreateCanvas("PaperDeskCanvas", -1);  // under the chat, which stays usable
        topCanvas = CreateCanvas("PaperDeskTopCanvas", 30); // the drag ghost goes over everything

        BuildTable();
        BuildReader();
        BuildAttachBar();
        manualsButton = NewButton("ManualsButton", deskCanvas.transform, "Manuals",
            Vector2.zero, new Vector2(30, 30), new Vector2(240, 64), () => SetTableOpen(true)).gameObject;
        manualsButton.GetComponent<Image>().color = Color.white;

        GameManager.Instance.OnGameEnded += HandleGameEnded;
        SetTableOpen(openAtStart);
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnGameEnded -= HandleGameEnded;
    }

    // ---- Sharing pages with Claude ----

    // False when blocked: a page with private information still visible never
    // reaches the chat, and Looma tells the player why.
    public bool Attach(Paper p)
    {
        if (p == null) return false;
        if (p.HasVisibleSecrets) { ShareBlocked?.Invoke(); return false; }

        if (!attached.Contains(p)) attached.Add(p);
        RefreshAttachBar();
        return true;
    }

    // Called by ChatPanel when Send is pressed with pages attached. Shares them with
    // Claude and returns the note that goes in front of the message. Returns false
    // (nothing is sent) if private information became visible again after attaching.
    public bool TryShareAttachments(out string note)
    {
        note = "";
        if (attached.Any(p => p.HasVisibleSecrets)) { ShareBlocked?.Invoke(); return false; }

        foreach (var p in attached)
        {
            if (claude != null) claude.SharePage(p.id, p.ToEnglish());
            if (!shared.Contains(p)) shared.Add(p);
            RefreshCard(p);
            PageShared?.Invoke(p);
        }
        note = "[Shared " + string.Join(" and ", attached.Select(p => $"the page {p.Label} ({p.date})")) + "]";
        attached.Clear();
        RefreshAttachBar();
        return true;
    }

    void ClearAttachments()
    {
        attached.Clear();
        RefreshAttachBar();
    }

    // The bar only appears - and only takes room from the chat - while a page is
    // waiting to be sent. Nothing hints at sharing before that: finding out that
    // Claude needs the pages is part of the game (Claude asks for them).
    void RefreshAttachBar()
    {
        if (attachBar == null) return;
        bool any = attached.Count > 0;
        attachBar.SetActive(any);
        if (chatScroll != null) chatScroll.offsetMin = any ? scrollMinWithBar : scrollMinDefault;
        if (any) attachText.text = "<color=#0E7A6E><b>Attached</b></color>   " + string.Join(",  ", attached.Select(p => p.Label));
    }

    void HandleGameEnded(GameManager.RunState state)
    {
        deskCanvas.gameObject.SetActive(false);
        if (attachBar != null) attachBar.SetActive(false);
    }

    // ---- Table, reader and black marker ----

    void SetTableOpen(bool open)
    {
        table.SetActive(open);
        manualsButton.SetActive(!open);
        if (!open) reader.SetActive(false);
    }

    public void OpenReader(Paper p)
    {
        reading = p;
        readingSegments = p.Segments.ToList();
        readerPaper.color = p.current ? PaperColor : OldPaperColor;
        readerHeader.text = $"<size=44><b>MODULE {p.module}</b></size>      <size=28>Rev {p.revision}  ·  {p.date}</size>\n<size=34>{p.titleJp}</size>";
        reader.SetActive(true); // before refreshing: an inactive text keeps the previous page's layout
        RefreshReader();
    }

    void CloseReader() => reader.SetActive(false);

    // Every segment is a <link> so the marker can find it. Blacked-out segments are
    // covered by opaque black rectangles, one per line the segment spans - TMP's own
    // <mark> highlight renders semi-transparent, which would leave secrets readable.
    void RefreshReader()
    {
        var sb = new StringBuilder();
        int i = 0;
        foreach (var line in reading.lines)
        {
            foreach (var s in line)
                sb.Append("<link=\"").Append(i++).Append("\">").Append(s.jp).Append("</link>");
            sb.Append('\n');
        }
        readerBody.text = sb.ToString();
        readerBody.ForceMeshUpdate(true, true);

        int bars = 0;
        var info = readerBody.textInfo;
        for (int l = 0; l < info.linkCount; l++)
        {
            var link = info.linkInfo[l];
            int seg = int.Parse(link.GetLinkID());
            if (seg >= readingSegments.Count || !readingSegments[seg].redacted) continue;

            int lineNumber = -1;
            float left = 0, right = 0;
            for (int c = link.linkTextfirstCharacterIndex; c < link.linkTextfirstCharacterIndex + link.linkTextLength; c++)
            {
                var ch = info.characterInfo[c];
                if (ch.lineNumber != lineNumber)
                {
                    if (lineNumber >= 0) PlaceBar(bars++, lineNumber, left, right);
                    lineNumber = ch.lineNumber;
                    left = ch.bottomLeft.x;
                }
                right = ch.topRight.x;
            }
            if (lineNumber >= 0) PlaceBar(bars++, lineNumber, left, right);
        }
        for (int b = bars; b < redactionBars.Count; b++) redactionBars[b].gameObject.SetActive(false);
    }

    // Bar b covers [left, right] on one text line, full line height, in the text's local space.
    void PlaceBar(int b, int lineNumber, float left, float right)
    {
        if (b == redactionBars.Count)
        {
            var bar = NewImage("Redaction", readerBody.transform, Color.black);
            bar.raycastTarget = false; // the marker must still reach the text underneath
            redactionBars.Add(bar);
        }
        var line = readerBody.textInfo.lineInfo[lineNumber];
        var rt = redactionBars[b].rectTransform;
        rt.anchorMin = rt.anchorMax = readerBody.rectTransform.pivot; // anchor on the text's local origin
        rt.pivot = Vector2.zero;
        rt.anchoredPosition = new Vector2(left - 3, line.descender);
        rt.sizeDelta = new Vector2(right - left + 6, line.ascender - line.descender);
        redactionBars[b].gameObject.SetActive(true);
    }

    public void MarkerDown(Vector2 screenPos)
    {
        int seg = SegmentAt(screenPos);
        markerPaint = seg < 0 || !readingSegments[seg].redacted;
        if (seg >= 0) Paint(seg);
    }

    public void MarkerDrag(Vector2 screenPos)
    {
        int seg = SegmentAt(screenPos);
        if (seg >= 0 && readingSegments[seg].redacted != markerPaint) Paint(seg);
    }

    void Paint(int seg)
    {
        readingSegments[seg].redacted = markerPaint;
        RefreshReader();
    }

    int SegmentAt(Vector2 screenPos)
    {
        int link = TMP_TextUtilities.FindIntersectingLink(readerBody, screenPos, null);
        if (link < 0) return -1;
        int seg = int.Parse(readerBody.textInfo.linkInfo[link].GetLinkID());
        return seg < readingSegments.Count ? seg : -1;
    }

    public void BeginCardDrag(Paper p, Vector2 screenPos)
    {
        var img = NewImage("DragGhost", topCanvas.transform, p.current ? PaperColor : OldPaperColor);
        img.raycastTarget = false;
        dragGhost = img.rectTransform;
        dragGhost.sizeDelta = new Vector2(240, 72);
        AddShadow(img.gameObject);
        var label = NewText("Label", dragGhost, p.Label, paperFont, 24, InkColor, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        MoveCardDrag(screenPos);
    }

    public void MoveCardDrag(Vector2 screenPos)
    {
        if (dragGhost == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)topCanvas.transform, screenPos, null, out var local);
        dragGhost.localPosition = local;
    }

    public void EndCardDrag(Paper p, Vector2 screenPos)
    {
        if (dragGhost != null) Destroy(dragGhost.gameObject);
        dragGhost = null;
        if (chatArea != null && RectTransformUtility.RectangleContainsScreenPoint(chatArea, screenPos, null))
            Attach(p);
    }

    // ---- UI construction ----

    void BuildTable()
    {
        float chatWidth = chatArea != null ? chatArea.rect.width : 0f;
        var bg = NewImage("Table", deskCanvas.transform, DeskColor);
        Stretch(bg.rectTransform, Vector2.zero, new Vector2(-chatWidth, 0));
        table = bg.gameObject;
        // The desk lamp: warm light in the middle of the table, dark toward the edges.
        var lamp = Ui.NewGlow("Lamp", table.transform, HintColor.WithAlpha(0.12f), Vector2.zero);
        Stretch(lamp.rectTransform, new Vector2(-200, -300), new Vector2(200, 200));
        var edges = Ui.NewImage("Vignette", table.transform, Ui.Ink.WithAlpha(0.7f), Ui.Vignette);
        edges.raycastTarget = false;
        Stretch(edges.rectTransform, Vector2.zero, Vector2.zero);

        var title = NewText("Title", table.transform, "Manuals", Ui.Display, 42, Color.white, TextAlignmentOptions.TopLeft);
        Place(title.rectTransform, new Vector2(0, 1), new Vector2(48, -34), new Vector2(600, 60));

        var hint = NewText("Hint", table.transform,
            "These pages are in Japanese: you can't read them, but Claude can.\n" +
            "Click a page to look at it up close and black out private information.",
            paperFont, 22, HintColor, TextAlignmentOptions.TopLeft);
        var hr = hint.rectTransform;
        hr.anchorMin = new Vector2(0, 1);
        hr.anchorMax = new Vector2(1, 1);
        hr.pivot = new Vector2(0, 1);
        hr.offsetMin = new Vector2(48, -210);
        hr.offsetMax = new Vector2(-430, -100);

        NewButton("Back", table.transform, "Back to the device", new Vector2(1, 1), new Vector2(-40, -36), new Vector2(300, 58), () => SetTableOpen(false), Ui.ButtonStyle.GhostOnDark);

        Vector2[] slots =
        {
            new Vector2(-480, 150), new Vector2(-160, 150), new Vector2(160, 150), new Vector2(480, 150),
            new Vector2(-320, -220), new Vector2(0, -220), new Vector2(320, -220),
        };
        var shuffled = papers.OrderBy(_ => Random.value).ToList();
        for (int i = 0; i < shuffled.Count; i++)
            BuildCard(shuffled[i], slots[i % slots.Length]);
    }

    void BuildCard(Paper p, Vector2 pos)
    {
        var img = NewImage("Card " + p.id, table.transform, p.current ? PaperColor : OldPaperColor);
        Place(img.rectTransform, new Vector2(0.5f, 0.5f), pos, new Vector2(260, 340));
        img.rectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-4f, 4f));
        AddShadow(img.gameObject);

        var text = NewText("Text", img.transform, "", paperFont, 18, InkColor, TextAlignmentOptions.TopLeft);
        Stretch(text.rectTransform, new Vector2(18, 16), new Vector2(-18, -16));
        text.overflowMode = TextOverflowModes.Truncate;
        cardTexts[p] = text;
        RefreshCard(p);

        img.gameObject.AddComponent<PaperCard>().Init(this, p);
    }

    void RefreshCard(Paper p)
    {
        string preview = string.Join("\n", p.lines.Take(4).Select(line => string.Concat(line.Select(s => s.jp))));
        string tag = shared.Contains(p) ? "<size=17><color=#0E7A6E><b>Shared with Claude</b></color></size>\n" : "";
        cardTexts[p].text = $"{tag}<size=34><b>MODULE {p.module}</b></size>\n<size=21>Rev {p.revision}  ·  {p.date}</size>\n" +
                            $"<size=26>{p.titleJp}</size>\n\n<size=13><color=#555555>{preview}</color></size>";
    }

    void BuildReader()
    {
        var dim = NewImage("Reader", table.transform, Ui.Ink.WithAlpha(0.98f));
        Stretch(dim.rectTransform, Vector2.zero, Vector2.zero);
        reader = dim.gameObject;

        readerPaper = NewImage("Paper", reader.transform, PaperColor);
        Place(readerPaper.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-170, 0), new Vector2(760, 1010));
        AddShadow(readerPaper.gameObject);

        readerHeader = NewText("Header", readerPaper.transform, "", paperFont, 24, InkColor, TextAlignmentOptions.TopLeft);
        var hr = readerHeader.rectTransform;
        hr.anchorMin = new Vector2(0, 1);
        hr.anchorMax = new Vector2(1, 1);
        hr.pivot = new Vector2(0.5f, 1);
        hr.offsetMin = new Vector2(44, -170);
        hr.offsetMax = new Vector2(-44, -34);

        readerBody = NewText("Body", readerPaper.transform, "", paperFont, 24, InkColor, TextAlignmentOptions.TopLeft);
        Stretch(readerBody.rectTransform, new Vector2(44, 40), new Vector2(-44, -180));
        readerBody.enableAutoSizing = true;
        readerBody.fontSizeMin = 14;
        readerBody.fontSizeMax = 24;
        readerBody.paragraphSpacing = 14;
        readerBody.raycastTarget = true; // the black marker needs pointer events
        readerBody.gameObject.AddComponent<RedactableText>().Init(this);

        // Beside the page: what to do with it, and how the marker works.
        var tools = Ui.NewCard("Tools", reader.transform, Color.white, out var toolsBackground, 20);
        Place(tools, new Vector2(0.5f, 0.5f), new Vector2(420, 40), new Vector2(380, 620));
        NewButton("Attach", tools, "Attach to chat", new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(320, 64), () =>
        {
            if (Attach(reading)) CloseReader();
        }, Ui.ButtonStyle.Primary);
        NewButton("Back", tools, "Back to the table", new Vector2(0.5f, 1f), new Vector2(0, -110), new Vector2(320, 60), CloseReader);
        var markerTitle = Ui.NewHeading("MarkerTitle", tools, "Black marker", 20f, Ui.Ink);
        Place(markerTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -204), new Vector2(320, 26));
        var hint = NewText("Hint", tools,
            "Press on any text to black it out, or drag across several lines. Press again to remove the black.\n\n" +
            "Before you share a page, black out private information: names, ID numbers, phone numbers and codes.",
            paperFont, 20, Ui.Text2, TextAlignmentOptions.TopLeft);
        hint.lineSpacing = 6;
        Place(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -240), new Vector2(320, 360));

        reader.SetActive(false);
    }

    void BuildAttachBar()
    {
        if (chatArea == null || chatInput == null) return;
        const float barHeight = 46f, gap = 4f;

        if (chatScroll != null)
        {
            scrollMinDefault = chatScroll.offsetMin;
            scrollMinWithBar = scrollMinDefault + new Vector2(0, barHeight + gap);
        }

        var corners = new Vector3[4];
        chatInput.GetWorldCorners(corners);
        float inputTop = chatArea.InverseTransformPoint(corners[1]).y - chatArea.rect.yMin;

        var bar = Ui.NewPanel("AttachBar", chatArea, Ui.Surface2, 6);
        var rt = bar.rectTransform;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.offsetMin = new Vector2(6, inputTop + gap);
        rt.offsetMax = new Vector2(-6, inputTop + gap + barHeight);
        // Clicking an attached page opens it again, e.g. to fix what is blacked out.
        bar.gameObject.AddComponent<Button>().onClick.AddListener(() =>
        {
            if (attached.Count == 0) return;
            SetTableOpen(true);
            OpenReader(attached[0]);
        });
        attachBar = bar.gameObject;

        var accent = Ui.NewPanel("Accent", bar.transform, Ui.Teal, 3);
        accent.raycastTarget = false;
        Place(accent.rectTransform, new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(4, 28));
        attachText = NewText("Text", bar.transform, "", paperFont, 18, Ui.Ink, TextAlignmentOptions.MidlineLeft);
        Stretch(attachText.rectTransform, new Vector2(22, 0), new Vector2(-56, 0));
        NewButton("Clear", bar.transform, "X", new Vector2(1, 0.5f), new Vector2(-6, 0), new Vector2(40, 34), ClearAttachments, Ui.ButtonStyle.Ghost);
        RefreshAttachBar();
    }

    Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // same as the chat canvas, so sizes line up
        return canvas;
    }

    static Button NewButton(string name, Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, UnityAction onClick,
                            Ui.ButtonStyle style = Ui.ButtonStyle.Secondary)
    {
        var button = Ui.NewButton(name, parent, label, style, onClick, 18f);
        Place((RectTransform)button.transform, anchor, pos, size);
        return button;
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var img = NewRect(name, parent).gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static TMP_Text NewText(string name, Transform parent, string text, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment)
    {
        var t = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = alignment;
        t.raycastTarget = false;
        t.text = text;
        return t;
    }

    // Anchor and pivot on the same point, so pos is measured from that corner or edge.
    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    static void AddShadow(GameObject go)
    {
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(7f, -7f);
    }
}
