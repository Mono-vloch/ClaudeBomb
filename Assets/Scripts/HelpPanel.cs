using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "How to play", opened from the gear button: the situation, the controls and the
// rules. It deliberately says nothing about sharing manual pages with Claude -
// finding that out is part of the game, and Claude tells the player when asked.
// The clock is frozen while the panel is open. Builds its own UI at runtime.
public class HelpPanel : MonoBehaviour
{
    [SerializeField] TMP_FontAsset titleFont;
    [SerializeField] TMP_FontAsset bodyFont;

    const string HelpText =
        "<size=28><b>THE SITUATION</b></size>\n" +
        "A live bomb with five modules is in front of you. You can see it, but you don't know how to defuse it. " +
        "Claude knows how, but it can't see the bomb. Talk to Claude in the chat on the right.\n" +
        "You have 10 minutes, and the clock only starts after your first module. Three mistakes and the bomb explodes. Solve all five modules to win.\n\n" +
        "<size=28><b>LOOK AT THE BOMB</b></size>\n" +
        "Rotate: the arrow buttons at the bottom, the arrow keys, or hold the right mouse button and drag\n" +
        "Zoom: mouse wheel, or W and S\n" +
        "Move left and right: A and D\n" +
        "Look at every side of the bomb: some information is on the back and underneath.\n\n" +
        "<size=28><b>USE THE MODULES</b></size>\n" +
        "Cut a wire: hold the left mouse button and drag across the wire\n" +
        "Buttons, glyphs, coloured cells and switches: left click\n" +
        "Sequence strip: press PLAY to watch the lamps flash\n" +
        "A module's light turns from red to green when it is solved.\n\n" +
        "<size=28><b>THE MANUALS</b></size>\n" +
        "MANUALS (bottom left) opens the table with the manual pages.\n" +
        "Click a page to look at it up close. Click or drag over any text to black it out.";

    GameObject panel;

    void Start()
    {
        Build();
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
        GamePause.Hold(this);
    }

    public void Hide()
    {
        panel.SetActive(false);
        GamePause.Release(this);
    }

    void OnDestroy() => GamePause.Release(this);

    void Build()
    {
        var canvasGo = new GameObject("HelpCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20; // over the chat and the manual table
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Full-screen dim: also blocks clicks from reaching the bomb behind it.
        panel = NewImage("HelpPanel", canvasGo.transform, new Color(0f, 0f, 0f, 0.85f)).gameObject;
        var dimRt = (RectTransform)panel.transform;
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;

        var box = NewImage("Box", panel.transform, new Color(0.12f, 0.12f, 0.14f, 0.98f));
        Place(box.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240, 980));

        var title = NewText("Title", box.transform, "HOW TO PLAY", titleFont, 48, TextAlignmentOptions.Center);
        Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -34), new Vector2(1100, 64));

        var body = NewText("Body", box.transform, HelpText, bodyFont, 24, TextAlignmentOptions.TopLeft);
        Place(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(1120, 740));
        body.enableAutoSizing = true;
        body.fontSizeMin = 16;
        body.fontSizeMax = 24;
        body.color = new Color(0.9f, 0.9f, 0.88f);

        var button = NewImage("Close", box.transform, new Color(0.95f, 0.94f, 0.9f));
        Place(button.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(380, 66));
        button.gameObject.AddComponent<Button>().onClick.AddListener(Hide);
        var label = NewText("Label", button.transform, "BACK TO THE GAME", titleFont, 26, TextAlignmentOptions.Center);
        label.color = new Color(0.13f, 0.13f, 0.15f);
        var lr = label.rectTransform;
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = lr.offsetMax = Vector2.zero;
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
        t.color = Color.white;
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
}
