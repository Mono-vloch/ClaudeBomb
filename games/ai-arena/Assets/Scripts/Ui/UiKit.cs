using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Builders for the handful of widgets every screen is made of. Everything is laid
// out on a 1920 by 1080 canvas that scales to the window; positions are given from
// the top-left corner in those units.
public static class Ui
{
    public const float W = 1920f, H = 1080f;

    public static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    // Top-left anchored box.
    public static RectTransform Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    public static RectTransform Stretch(RectTransform rt, float left = 0, float top = 0, float right = 0, float bottom = 0)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom); rt.offsetMax = new Vector2(-right, -top);
        return rt;
    }

    public static Image Box(Transform parent, string name, Color fill, bool rounded = true)
    {
        var rt = Node(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = fill;
        if (rounded) { img.sprite = Corner; img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = 1f; } // 12 px corners, everywhere
        return img;
    }

    // The buttons' shape: a box with corners rounded by 12, whatever its size. Drawn
    // once, in code, like the baked shapes. (A pill turned every short button into
    // an oval.)
    static Sprite corner;
    public static Sprite Corner
    {
        get
        {
            if (corner != null) return corner;
            const int r = 12, size = 2 * r + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float qx = Mathf.Max(Mathf.Abs(x + 0.5f - size / 2f) - (size / 2f - r), 0f);
                    float qy = Mathf.Max(Mathf.Abs(y + 0.5f - size / 2f) - (size / 2f - r), 0f);
                    float d = Mathf.Sqrt(qx * qx + qy * qy) - r; // signed distance to the edge
                    px[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(0.5f - d));
                }
            tex.SetPixels(px); tex.Apply();
            // 100 pixels per unit, like the baked sprites and the canvas, so the border is 12 units and not 1200.
            corner = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            return corner;
        }
    }

    // A strip that fades from solid on the left to nothing on the right: the edge of
    // the cover's ink over the market.
    static Sprite fadeStrip;
    public static Sprite FadeStrip
    {
        get
        {
            if (fadeStrip != null) return fadeStrip;
            const int w = 256, h = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float k = x / (float)(w - 1);
                    px[y * w + x] = new Color(1, 1, 1, 1f - k * k * (3f - 2f * k));
                }
            tex.SetPixels(px); tex.Apply();
            fadeStrip = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
            return fadeStrip;
        }
    }

    // Dark glass: the HUD's material over the world, ink at 78 % with a keyline on the left.
    public static Image Glass(Transform parent, string name, float x, float y, float w, float h, bool keyline = true, Color? keylineColour = null)
    {
        var box = Box(parent, name, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.78f));
        Place(box.rectTransform, x, y, w, h);
        box.raycastTarget = false;
        if (keyline)
        {
            var line = Box(box.transform, "Keyline", keylineColour ?? Theme.Teal, false);
            Place(line.rectTransform, 0, 10, 4, h - 20);
            line.raycastTarget = false;
        }
        return box;
    }

    // A box with a hairline: an outer image in the border colour, an inner one in the fill.
    public static Image Card(Transform parent, string name, Color fill, Color border, float borderWidth = 2f)
    {
        var outer = Box(parent, name, border);
        var inner = Box(outer.transform, "Fill", fill);
        Stretch(inner.rectTransform, borderWidth, borderWidth, borderWidth, borderWidth);
        return outer;
    }

    // The standard card: teal-tinted fill, hairline, no shadow.
    public static Image Soft(Transform parent, string name) => Card(parent, name, Theme.Surface2, Theme.Border1);

    // A floating card: white, no stroke, a shadow so soft it is barely there. For
    // the one thing on the screen that matters.
    public static Image Float(Transform parent, string name, float x, float y, float w, float h)
    {
        var shadow = Node("Shadow", parent).gameObject.AddComponent<Image>();
        shadow.sprite = Theme.SoftShadow; shadow.type = Image.Type.Sliced; shadow.color = Theme.Shadow; shadow.raycastTarget = false;
        Place(shadow.rectTransform, x - 22, y - 18, w + 44, h + 44);
        var card = Box(parent, name, Theme.Panel);
        Place(card.rectTransform, x, y, w, h);
        return card;
    }

    public static TextMeshProUGUI Text(Transform parent, string text, float size, Color color, bool bold = false,
                                       TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var rt = Node("Text", parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = bold ? Theme.Bold : Theme.Body;
        t.fontSize = size;
        t.color = color;
        t.text = text;
        t.alignment = align;
        t.enableWordWrapping = true;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // The small caps label above a block: the one place the brand tracks its type.
    public static TextMeshProUGUI Eyebrow(Transform parent, string text, Color color)
    {
        var t = Text(parent, text, 13, color);
        t.characterSpacing = 5;
        return t;
    }

    public enum ButtonStyle { Primary, Secondary, Quiet, Danger, Accent, GhostOnDark }

    public static Button Button(Transform parent, string label, Action onClick, ButtonStyle style = ButtonStyle.Primary, float fontSize = 22)
    {
        Color fill = style switch
        {
            ButtonStyle.Primary => Theme.TealDeep,
            ButtonStyle.Danger => Theme.DangerSoft,
            ButtonStyle.Accent => Theme.Teal,
            ButtonStyle.GhostOnDark => Theme.Ink, // the dark it sits on: a keyline, no fill
            _ => Theme.Panel,
        };
        Color border = style switch
        {
            ButtonStyle.Primary => Theme.TealDeep,
            ButtonStyle.Danger => Theme.Danger,
            ButtonStyle.Accent => Theme.Teal,
            ButtonStyle.GhostOnDark => new Color(1f, 1f, 1f, 0.3f),
            ButtonStyle.Quiet => Color.clear,
            _ => Theme.Border,
        };
        Color ink = style switch
        {
            ButtonStyle.Primary => Color.white,
            ButtonStyle.Danger => Theme.Danger,
            ButtonStyle.GhostOnDark => Color.white,
            _ => Theme.Ink,
        };
        var outer = Card(parent, "Button " + label, fill, border);
        outer.sprite = Corner;
        outer.transform.GetChild(0).GetComponent<Image>().sprite = Corner;
        var btn = outer.gameObject.AddComponent<Button>();
        btn.targetGraphic = outer;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.93f, 0.93f, 0.93f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colors.disabledColor = new Color(1, 1, 1, 0.45f);
        btn.colors = colors;
        var t = Text(outer.transform, label, fontSize, ink, true, TextAlignmentOptions.Center);
        Stretch(t.rectTransform, 12, 0, 12, 0);
        btn.onClick.AddListener(() => { Sfx.Tick(); onClick?.Invoke(); });
        return btn;
    }

    public static void SetLabel(Button b, string label) => b.GetComponentInChildren<TextMeshProUGUI>().text = label;

    public static TMP_InputField Input(Transform parent, string placeholder, bool multiline, float fontSize = 20)
    {
        var outer = Card(parent, "Input", Theme.Panel, Theme.Border);
        var field = outer.gameObject.AddComponent<TMP_InputField>();
        field.targetGraphic = outer;

        var area = Node("Text Area", outer.transform);
        Stretch(area, 14, 10, 14, 10);
        area.gameObject.AddComponent<RectMask2D>();

        var ph = Text(area, placeholder, fontSize, Theme.Muted);
        Stretch(ph.rectTransform);
        ph.fontStyle = FontStyles.Italic;
        var text = Text(area, "", fontSize, Theme.Ink);
        Stretch(text.rectTransform);
        text.raycastTarget = true;

        field.textViewport = area;
        field.textComponent = text;
        field.placeholder = ph;
        field.fontAsset = Theme.Body;
        field.pointSize = fontSize;
        field.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
        field.selectionColor = new Color(0.086f, 0.76f, 0.69f, 0.35f);
        field.customCaretColor = true;
        field.caretColor = Color.clear; // the field draws its own caret (CaretView)
        field.caretWidth = 1;
        field.onSelect.AddListener(_ => outer.color = Theme.Aqua);
        field.onDeselect.AddListener(_ => outer.color = Theme.Border);
        outer.gameObject.AddComponent<CaretView>().Bind(field, text);
        return field;
    }

    // A row of choices where exactly one is lit.
    public class Segmented
    {
        public readonly RectTransform root;
        readonly List<Button> buttons = new List<Button>();
        readonly List<Image> fills = new List<Image>();
        readonly List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
        public int Selected { get; private set; }
        public Action<int> OnSelect;

        public Segmented(Transform parent, string[] options, int selected, float width, float height, float fontSize = 19)
        {
            root = Node("Segmented", parent);
            root.sizeDelta = new Vector2(width, height);
            float gap = 8f, w = (width - gap * (options.Length - 1)) / options.Length;
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                var b = Button(root, options[i], () => Set(index), ButtonStyle.Secondary, fontSize);
                Place(b.GetComponent<RectTransform>(), i * (w + gap), 0, w, height);
                buttons.Add(b);
                fills.Add(b.transform.Find("Fill").GetComponent<Image>());
                labels.Add(b.GetComponentInChildren<TextMeshProUGUI>());
            }
            Set(selected, false);
        }

        public void Set(int index, bool notify = true)
        {
            Selected = Mathf.Clamp(index, 0, buttons.Count - 1);
            for (int i = 0; i < buttons.Count; i++)
            {
                bool on = i == Selected;
                buttons[i].GetComponent<Image>().color = on ? Theme.BorderTeal : Theme.Border;
                fills[i].color = on ? Theme.Surface2 : Theme.Panel;
                labels[i].color = on ? Theme.TealDeep : Theme.Ink;
            }
            if (notify) OnSelect?.Invoke(Selected);
        }
    }

    public static Image Dot(Transform parent, Color color, float size)
    {
        var img = Box(parent, "Dot", color, false);
        img.sprite = Theme.Circle;
        img.rectTransform.sizeDelta = new Vector2(size, size);
        return img;
    }

    public static Image Icon(Transform parent, string spriteName, float size)
    {
        var rt = Node("Icon " + spriteName, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Theme.Sprite(spriteName);
        img.preserveAspect = true;
        img.raycastTarget = false;
        rt.sizeDelta = new Vector2(size, size);
        return img;
    }

    // An image from Resources (the brand marks, LUMA's portrait).
    public static Image Picture(Transform parent, string resource, float x, float y, float w, float h)
    {
        var img = Node("Picture", parent).gameObject.AddComponent<Image>();
        img.sprite = Resources.Load<Sprite>(resource);
        img.preserveAspect = true;
        img.raycastTarget = false;
        Place(img.rectTransform, x, y, w, h);
        return img;
    }

    // The OO emblem, bottom right, on every light screen.
    public static Image Emblem(Transform parent)
    {
        var img = Picture(parent, "Brand/emblem-light", 0, 0, 64, 44);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-48, 32);
        return img;
    }

    // The full wordmark, top left, on the dark cover screens.
    public static Image Wordmark(Transform parent, float x, float y, float w) =>
        Picture(parent, "Brand/logo-dark", x, y, w, w * 200f / 1400f);

    // A speech bubble with a tail at the bottom left: what a robot texted home.
    public static TextMeshProUGUI Bubble(Transform parent, string text, float fontSize, float width, out RectTransform bubble)
    {
        bubble = Node("Bubble", parent);
        var card = Card(bubble, "Card", Theme.Panel, Theme.Border);
        Stretch(card.rectTransform, 0, 0, 0, 10);
        var tail = Node("Tail", bubble).gameObject.AddComponent<Image>();
        tail.sprite = Theme.Tail; tail.color = Theme.Border; tail.raycastTarget = false;
        tail.rectTransform.anchorMin = tail.rectTransform.anchorMax = new Vector2(0, 0);
        tail.rectTransform.pivot = new Vector2(0, 1);
        tail.rectTransform.anchoredPosition = new Vector2(22, 12);
        tail.rectTransform.sizeDelta = new Vector2(18, 14);
        var t = Text(card.transform, text, fontSize, Theme.Ink);
        Stretch(t.rectTransform, 12, 8, 12, 8);
        float h = t.GetPreferredValues(text, width - 24, 0).y + 16 + 10;
        bubble.sizeDelta = new Vector2(width, h);
        return t;
    }

    // The market's catalogue: every item with its name, fruit on one row and
    // vegetables on the next, so a list can name things instead of describing
    // them. Returns the height used.
    public static float Catalogue(Transform parent, float x, float y, float width, float icon, Color label)
    {
        float rowH = icon + 22;
        var groups = new[] { ("Fruit", System.Linq.Enumerable.ToList(Items.Fruit)), ("Vegetables", System.Linq.Enumerable.ToList(Items.Vegetables)) };
        float cy = y;
        foreach (var (name, items) in groups)
        {
            var l = Text(parent, name, 13, label, true);
            Place(l.rectTransform, x, cy + icon / 2 - 9, 100, 20);
            float cx = x + 100;
            float step = Mathf.Min(icon + 58, (width - 100) / items.Count);
            foreach (var k in items)
            {
                var img = Icon(parent, k.id, icon);
                Place(img.rectTransform, cx + (step - icon) / 2, cy, icon, icon);
                var n = Text(parent, k.id, 13, Theme.Text2, false, TextAlignmentOptions.Center); // readable from the back of a class
                Place(n.rectTransform, cx, cy + icon + 2, step, 18);
                n.enableWordWrapping = false;
                cx += step;
            }
            cy += rowH;
        }
        return cy - y;
    }

    // A filled bar: outer track, inner fill scaled by 0..1.
    public static (Image track, Image fill) Bar(Transform parent, Color trackColor, Color fillColor, float w, float h)
    {
        var track = Box(parent, "Bar", trackColor);
        track.rectTransform.sizeDelta = new Vector2(w, h);
        var fill = Box(track.transform, "Fill", fillColor);
        fill.rectTransform.anchorMin = new Vector2(0, 0); fill.rectTransform.anchorMax = new Vector2(0, 1);
        fill.rectTransform.pivot = new Vector2(0, 0.5f);
        fill.rectTransform.offsetMin = Vector2.zero; fill.rectTransform.offsetMax = Vector2.zero;
        fill.rectTransform.sizeDelta = new Vector2(w, 0);
        return (track, fill);
    }

    public static void SetBar((Image track, Image fill) bar, float fraction)
    {
        float w = bar.track.rectTransform.sizeDelta.x;
        bar.fill.rectTransform.sizeDelta = new Vector2(Mathf.Clamp01(fraction) * w, 0);
    }

    public static void Clear(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
    }

    public static ScrollRect Scroll(Transform parent, string name, out RectTransform content)
    {
        var rt = Node(name, parent);
        var scroll = rt.gameObject.AddComponent<ScrollRect>();
        var viewport = Node("Viewport", rt);
        Stretch(viewport);
        viewport.gameObject.AddComponent<RectMask2D>();
        var img = viewport.gameObject.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.001f);
        content = Node("Content", viewport);
        content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
        content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;
        return scroll;
    }

    // ---- motion ----------------------------------------------------------------

    // TryGetComponent, not GetComponent with ??: in the Editor a missing component is a
    // stand-in that ?? accepts, and the next access throws (a build returns null).
    public static CanvasGroup Group(Component c) => c.TryGetComponent<CanvasGroup>(out var group) ? group : c.gameObject.AddComponent<CanvasGroup>();

    // Runs step(k) with k from 0 to 1 over the given seconds, unscaled.
    public static IEnumerator Animate(float seconds, Action<float> step, Func<float, float> ease = null)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            step(ease != null ? ease(k) : k);
            yield return null;
        }
        step(ease != null ? ease(1f) : 1f);
    }

    public static float EaseOut(float k) => 1f - (1f - k) * (1f - k);
    public static float EaseOutBack(float k) { const float c1 = 1.70158f, c3 = c1 + 1f; return 1f + c3 * Mathf.Pow(k - 1f, 3) + c1 * Mathf.Pow(k - 1f, 2); }

    public static IEnumerator Fade(CanvasGroup group, float to, float seconds)
    {
        if (group == null) yield break;
        float from = group.alpha;
        yield return Animate(seconds, k => { if (group != null) group.alpha = Mathf.Lerp(from, to, k); });
    }

    // Reveals text letter by letter. hurry() true shows the rest at once.
    public static IEnumerator Type(TMP_Text text, string content, float lettersPerSecond, Func<bool> hurry = null)
    {
        text.text = content;
        text.maxVisibleCharacters = 0;
        text.ForceMeshUpdate();
        int total = text.textInfo.characterCount;
        float shown = 0f;
        while (shown < total)
        {
            if (hurry != null && hurry()) break;
            shown += lettersPerSecond * Time.unscaledDeltaTime;
            text.maxVisibleCharacters = Mathf.Min(total, (int)shown);
            yield return null;
        }
        text.maxVisibleCharacters = 99999;
    }
}
