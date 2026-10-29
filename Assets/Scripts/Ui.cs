using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// The game's look in one place, following the Maaloomatiia brand guidelines: the brand
// palette, the fonts (from UiTheme), the logo and emblem (Resources/Brand), shapes drawn
// in code (rounded panels, soft shadows, glows, icons) and the builders every screen
// uses to make its UI at runtime. Shapes are drawn into textures once, on first use.
// Motion runs on unscaled time: many screens are up while the game is paused.
//
// The brand's theme rule, as the game applies it: the title, the briefing and the
// debrief are cover and closing screens, on Deep Knowledge Blue with the full wordmark.
// Everything used during play - the chat, the HUD, LUMA's cards, the help - is light,
// with the OO emblem bottom-right. The 3D device stays as dark as the room it's in.
public static class Ui
{
    // ---- Palette: the Maaloomatiia brand colours and neutral ramp ----

    public static readonly Color Ink = Hex(0x0B1536);      // Deep Knowledge Blue: headings, text on light, dark screens
    public static readonly Color Ink2 = Hex(0x0E3556);     // Midnight Intellect: secondary dark
    public static readonly Color Teal = Hex(0x24E0C0);     // Maaloomatiia Teal: the accent; as text, on dark only
    public static readonly Color Aqua = Hex(0x16C2B0);     // Aqua Bridge: filled shapes
    public static readonly Color Cyan = Hex(0x63FFF3);     // Illuminated Cyan: highlights on dark only
    public static readonly Color TealDeep = Hex(0x0E7A6E); // fills that carry small white text, teal text on white
    public static readonly Color Text2 = Hex(0x5B6B78);    // body text on light
    public static readonly Color Muted = Hex(0x8297AC);    // placeholders and large captions on light
    public static readonly Color Faint = Hex(0x9EAEBB);    // secondary text on dark
    public static readonly Color Surface1 = Hex(0xF5F7F8); // palest tint: fields
    public static readonly Color Surface2 = Hex(0xECF8F6); // teal-tinted cards: LUMA
    public static readonly Color Surface3 = Hex(0xE9EDF0); // neutral cards: Claude's messages, tiles
    public static readonly Color Border2 = Hex(0xD1D8E0);  // hairlines and outlines
    public static readonly Color Border3 = Hex(0xAAE2D5);  // accent outlines
    // Not a brand colour: strikes and the explosion need a warning red, and only they use it.
    public static readonly Color Danger = Hex(0xE5484D);

    public static Color Hex(int rgb, float alpha = 1f) =>
        new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, alpha);

    public static Color WithAlpha(this Color c, float alpha) { c.a = alpha; return c; }

    // ---- Fonts ----

    static UiTheme theme;
    static UiTheme Theme => theme != null ? theme : theme = Resources.Load<UiTheme>("UiTheme");
    public static TMP_FontAsset Display => Theme.display;
    public static TMP_FontAsset Body => Theme.body;

    // ---- Shapes ----

    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    static Sprite Cached(string key, Func<Sprite> make)
    {
        if (!sprites.TryGetValue(key, out var sprite) || sprite == null) sprites[key] = sprite = make();
        return sprite;
    }

    // A rounded rectangle, 9-sliced so it keeps its corners at any size. Drawn at twice
    // the size (see Round) so the edges stay smooth on large screens.
    public static Sprite Rounded(int radius) => Cached("rounded" + radius, () => Box(radius * 2, 0, BoxFill.Solid));
    // Only the outline of one, for ghost buttons and highlight rings.
    public static Sprite RoundedLine(int radius) => Cached("line" + radius, () => Box(radius * 2, 0, BoxFill.Line));
    // A rounded rectangle with a blurred edge: shadows and glows.
    public static Sprite Soft(int radius, int blur) => Cached($"soft{radius}/{blur}", () => Box(radius * 2, blur * 2, BoxFill.Solid));
    // Inside out - clear in the middle, solid around it, with a soft edge: the tour's spotlight.
    public static Sprite Hole(int radius, int blur) => Cached($"hole{radius}/{blur}", () => Box(radius * 2, blur * 2, BoxFill.Hole));

    public static Sprite Circle => Cached("circle", () => Icon(64, p => p.magnitude - 0.47f));
    public static Sprite Ring => Cached("ring", () => Icon(64, p => Mathf.Abs(p.magnitude - 0.42f) - 0.05f));
    public static Sprite Check => Cached("check", () => Icon(64, p =>
        Mathf.Min(Segment(p, new Vector2(-0.30f, 0.02f), new Vector2(-0.08f, -0.22f)),
                  Segment(p, new Vector2(-0.08f, -0.22f), new Vector2(0.32f, 0.24f))) - 0.075f));
    public static Sprite Cross => Cached("cross", () => Icon(64, p =>
        Mathf.Min(Segment(p, new Vector2(-0.24f, -0.24f), new Vector2(0.24f, 0.24f)),
                  Segment(p, new Vector2(-0.24f, 0.24f), new Vector2(0.24f, -0.24f))) - 0.075f));
    public static Sprite Star => Cached("star", () => Icon(96, p => Polygon(p, StarPoints())));
    // Points right; rotate the image for the other directions.
    public static Sprite Triangle => Cached("triangle", () => Icon(64, p =>
        Polygon(p, new[] { new Vector2(-0.22f, -0.30f), new Vector2(0.30f, 0f), new Vector2(-0.22f, 0.30f) }) - 0.02f));
    // Clear on the left, solid on the right (rotate the image for other directions).
    public static Sprite Gradient => Cached("gradient", () =>
    {
        var tex = new Texture2D(256, 4, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color32[256 * 4];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, (byte)(Mathf.SmoothStep(0f, 1f, (i % 256) / 255f) * 255));
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 256, 4), new Vector2(0.5f, 0.5f), 100f);
    });
    // Two upright bars: pause.
    public static Sprite PauseIcon => Cached("pause", () => Icon(64, p =>
        Mathf.Min(BoxDistance(p, new Vector2(-0.12f, 0f), new Vector2(0.055f, 0.2f)),
                  BoxDistance(p, new Vector2(0.12f, 0f), new Vector2(0.055f, 0.2f))) - 0.02f));
    // A soft round glow, brightest in the middle.
    public static Sprite Glow => Cached("glow", () => Field(128, d => Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f)));
    // Clear in the middle, darkening toward the edges of the screen.
    public static Sprite Vignette => Cached("vignette", () => Field(256, d => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1.4f, d))));

    enum BoxFill { Solid, Line, Hole }

    // radius and blur in texture pixels. The texture is the smallest that 9-slices:
    // the corners, plus a 2-pixel middle that stretches.
    static Sprite Box(int radius, int blur, BoxFill fill)
    {
        int border = radius + blur, size = 2 * border + 2;
        float half = size / 2f - blur, line = 3f;
        var tex = NewTexture(size);
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // Signed distance to the rounded rectangle (negative inside).
                var q = new Vector2(Mathf.Abs(x + 0.5f - size / 2f), Mathf.Abs(y + 0.5f - size / 2f)) - new Vector2(half - radius, half - radius);
                float d = Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
                float a = blur > 0 ? 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-blur * 0.35f, blur, d)) : Mathf.Clamp01(0.5f - d);
                if (fill == BoxFill.Line) a = Mathf.Clamp01(0.5f - Mathf.Abs(d + line / 2f) + line / 2f);
                if (fill == BoxFill.Hole) a = 1f - a;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                             new Vector4(border, border, border, border));
    }

    // An icon from a signed distance function over [-0.5, 0.5]², in units of the icon's size.
    static Sprite Icon(int size, Func<Vector2, float> distance)
    {
        var tex = NewTexture(size);
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / size - 0.5f, (y + 0.5f) / size - 0.5f);
                float a = Mathf.Clamp01(0.5f - distance(p) * size);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // A radial field: alpha from the distance to the centre (0 there, 1 at the edge's middle).
    static Sprite Field(int size, Func<float, float> alpha)
    {
        var tex = NewTexture(size);
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f).magnitude;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha(d)) * 255));
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    static Texture2D NewTexture(int size) =>
        new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };

    static float BoxDistance(Vector2 p, Vector2 centre, Vector2 half)
    {
        var q = new Vector2(Mathf.Abs(p.x - centre.x), Mathf.Abs(p.y - centre.y)) - half;
        return Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f);
    }

    static float Segment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return (p - (a + t * ab)).magnitude;
    }

    // Signed distance to a closed polygon: negative inside.
    static float Polygon(Vector2 p, Vector2[] points)
    {
        float d = float.MaxValue;
        bool inside = false;
        for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
        {
            d = Mathf.Min(d, Segment(p, points[j], points[i]));
            if ((points[i].y > p.y) != (points[j].y > p.y) &&
                p.x < (points[j].x - points[i].x) * (p.y - points[i].y) / (points[j].y - points[i].y) + points[i].x)
                inside = !inside;
        }
        return inside ? -d : d;
    }

    static Vector2[] StarPoints()
    {
        var points = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float angle = Mathf.PI / 2f + i * Mathf.PI / 5f, r = i % 2 == 0 ? 0.47f : 0.2f;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r + new Vector2(0f, -0.03f);
        }
        return points;
    }

    // ---- Builders ----

    public static Canvas NewCanvas(string name, Transform parent, int sortingOrder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(parent, false);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // as on every canvas in the game, so sizes line up
        return canvas;
    }

    public static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static Image NewImage(string name, Transform parent, Color color, Sprite sprite = null)
    {
        var img = NewRect(name, parent).gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        return img;
    }

    // A panel with rounded corners.
    public static Image NewPanel(string name, Transform parent, Color color, int radius = 14)
    {
        var img = NewImage(name, parent, color);
        Round(img, radius);
        return img;
    }

    // Gives an existing image (also one made in the scene) rounded corners.
    public static void Round(Image img, int radius, bool outlineOnly = false)
    {
        img.sprite = outlineOnly ? RoundedLine(radius) : Rounded(radius);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 2f; // the sprites are drawn at twice the size
    }

    // A floating card: a soft shadow, the rounded background and whatever goes on it.
    // Move and fade the root; shadow and content come along.
    public static RectTransform NewCard(string name, Transform parent, Color color, out Image background, int radius = 18)
    {
        var root = NewRect(name, parent);
        var shadow = NewImage("Shadow", root, new Color(0f, 0f, 0f, 0.35f), Soft(radius, 28)); // soft: a card has a shadow or a stroke, never both
        shadow.type = Image.Type.Sliced;
        shadow.pixelsPerUnitMultiplier = 2f;
        shadow.raycastTarget = false;
        Stretch(shadow.rectTransform, new Vector2(-28, -40), new Vector2(28, 16));
        background = NewPanel("Background", root, color, radius);
        Stretch(background.rectTransform);
        return root;
    }

    // A soft coloured glow behind something; size is the glow's own size.
    public static Image NewGlow(string name, Transform parent, Color color, Vector2 size)
    {
        var glow = NewImage(name, parent, color, Glow);
        glow.raycastTarget = false;
        glow.rectTransform.sizeDelta = size;
        return glow;
    }

    public static Image NewIcon(string name, Transform parent, Sprite sprite, Color color, float size)
    {
        var icon = NewImage(name, parent, color, sprite);
        icon.raycastTarget = false;
        icon.rectTransform.sizeDelta = new Vector2(size, size);
        return icon;
    }

    public static TMP_Text NewText(string name, Transform parent, string text, float size, Color color,
                                   TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft, TMP_FontAsset font = null)
    {
        var t = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font != null ? font : Body;
        t.fontSize = size;
        t.color = color;
        t.alignment = alignment;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        t.text = text;
        return t;
    }

    // The brand's eyebrow: small capitals with 0.12em tracking, the one place tracking is
    // allowed. Text2 on light, Faint on dark.
    public static TMP_Text NewLabel(string name, Transform parent, string text, float size, Color color,
                                    TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
    {
        var t = NewText(name, parent, text.ToUpperInvariant(), size, color, alignment, Body);
        t.characterSpacing = 12f;
        t.fontStyle = FontStyles.Bold;
        return t;
    }

    // A heading: the display font, sentence case, normal tracking.
    public static TMP_Text NewHeading(string name, Transform parent, string text, float size, Color color,
                                      TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
    {
        var t = NewText(name, parent, text, size, color, alignment, Display);
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    // The one decorative rule the brand allows: a short teal underline under a section heading.
    public static Image NewUnderline(Transform parent, Vector2 topLeft)
    {
        var line = NewImage("Underline", parent, Teal);
        line.raycastTarget = false;
        Place(line.rectTransform, new Vector2(0f, 1f), topLeft, new Vector2(62f, 4f));
        return line;
    }

    // ---- The Maaloomatiia logo and emblem (the official artwork, never rebuilt) ----

    const float LogoRatio = 7.0352f, EmblemRatio = 1.4528f;

    // The full wordmark, for cover and closing screens. dark: the version for dark backgrounds.
    public static RawImage NewLogo(Transform parent, bool dark, float width)
    {
        var logo = NewRect("Maaloomatiia", parent).gameObject.AddComponent<RawImage>();
        logo.texture = Resources.Load<Texture2D>(dark ? "Brand/logo-dark" : "Brand/logo-light");
        logo.raycastTarget = false;
        logo.rectTransform.sizeDelta = new Vector2(width, width / LogoRatio);
        return logo;
    }

    // The OO emblem, bottom-right on screens used during play.
    public static RawImage NewEmblem(Transform parent, bool dark, float height)
    {
        var emblem = NewRect("MaaloomatiiaEmblem", parent).gameObject.AddComponent<RawImage>();
        emblem.texture = Resources.Load<Texture2D>(dark ? "Brand/emblem-dark" : "Brand/emblem-light");
        emblem.raycastTarget = false;
        emblem.rectTransform.sizeDelta = new Vector2(height * EmblemRatio, height);
        return emblem;
    }

    // On light screens: Primary (deep teal, white text), Secondary (neutral), Ghost (outline).
    // On the dark cover and closing screens: Accent (teal, navy text) and GhostOnDark.
    public enum ButtonStyle { Primary, Secondary, Ghost, Accent, GhostOnDark }

    public static Button NewButton(string name, Transform parent, string label, ButtonStyle style, UnityAction onClick, float fontSize = 22f)
    {
        var img = NewPanel(name, parent, Color.white, 6);
        var button = img.gameObject.AddComponent<Button>();
        var text = NewText("Label", img.transform, label, fontSize, Ink, TextAlignmentOptions.Center, Display);
        Stretch(text.rectTransform, new Vector2(12, 0), new Vector2(-12, 0));
        Style(button, style);
        button.onClick.AddListener(onClick);
        return button;
    }

    // Restyles a button, also one made in the scene: corners, colours, label, and the
    // hover-and-press feel (ButtonFeel).
    public static void Style(Button button, ButtonStyle style)
    {
        var img = button.targetGraphic as Image ?? button.GetComponent<Image>();
        (Color fill, Color ink) = style switch
        {
            ButtonStyle.Primary => (TealDeep, Color.white),
            ButtonStyle.Secondary => (Surface3, Ink),
            ButtonStyle.Ghost => (Border2, Text2),
            ButtonStyle.Accent => (Teal, Ink),
            _ => (Faint.WithAlpha(0.6f), Color.white),
        };
        if (img != null)
        {
            // Corners at the brand's 7% of the shorter side: crisp, not a pill.
            Round(img, 6, outlineOnly: style == ButtonStyle.Ghost || style == ButtonStyle.GhostOnDark);
            img.color = fill;
        }

        // Tints multiply the fill: a touch darker at rest, full colour under the pointer.
        var colors = ColorBlock.defaultColorBlock;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.5f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        // No keyboard navigation: Enter and the arrow keys belong to the chat and the camera.
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        // Sentence case, normal tracking: size and weight carry the emphasis.
        foreach (var t in button.GetComponentsInChildren<TMP_Text>(true))
        {
            t.font = Display;
            t.color = ink;
            t.fontStyle = FontStyles.Normal;
            t.characterSpacing = 0f;
        }
        if (button.GetComponent<ButtonFeel>() == null) button.gameObject.AddComponent<ButtonFeel>();
    }

    // An on/off switch: a track that turns deep teal when on, and a knob that slides.
    public static Button NewSwitch(string name, Transform parent, bool on, Action<bool> changed)
    {
        var track = NewImage(name, parent, on ? TealDeep : Border2, Rounded(15));
        track.type = Image.Type.Sliced;
        track.pixelsPerUnitMultiplier = 2f;
        track.rectTransform.sizeDelta = new Vector2(56f, 30f);
        var knob = NewIcon("Knob", track.transform, Circle, Color.white, 24f);
        knob.rectTransform.anchoredPosition = new Vector2(on ? 13f : -13f, 0f);
        var button = track.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.gameObject.AddComponent<ButtonFeel>();
        button.onClick.AddListener(() =>
        {
            on = !on;
            track.color = on ? TealDeep : Border2;
            knob.rectTransform.anchoredPosition = new Vector2(on ? 13f : -13f, 0f);
            changed(on);
        });
        return button;
    }

    // Restyles a text field, also one made in the scene.
    public static void Style(TMP_InputField field, string placeholder, float fontSize = 20f)
    {
        if (field.targetGraphic is Image img)
        {
            Round(img, 6);
            img.color = Surface1;
        }
        field.textComponent.font = Body;
        field.textComponent.fontSize = fontSize;
        field.textComponent.color = Ink;
        if (field.placeholder is TMP_Text hint)
        {
            hint.text = placeholder;
            hint.font = Body;
            hint.fontSize = fontSize;
            hint.fontStyle = FontStyles.Normal;
            hint.color = Muted;
        }
        field.customCaretColor = true;
        field.caretColor = TealDeep;
        field.caretWidth = 2;
        field.selectionColor = Teal.WithAlpha(0.35f);
        var colors = ColorBlock.defaultColorBlock;
        colors.normalColor = new Color(0.92f, 0.92f, 0.92f);
        colors.highlightedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        field.colors = colors;
    }

    // Anchor and pivot on the same point, so pos is measured from that corner or edge.
    public static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    public static void Stretch(RectTransform rt) => Stretch(rt, Vector2.zero, Vector2.zero);

    public static void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    public static CanvasGroup Group(Component c)
    {
        var group = c.GetComponent<CanvasGroup>();
        return group != null ? group : c.gameObject.AddComponent<CanvasGroup>();
    }

    // The screen rectangle of a UI element, in pixels (any canvas, overlay mode).
    public static Rect ScreenRect(RectTransform rt)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    }

    // A screen rectangle in the local space of an overlay canvas (centre origin).
    public static Rect ToCanvas(RectTransform canvas, Rect screen)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen.min, null, out var min);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen.max, null, out var max);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // ---- Motion ----

    public static float EaseOut(float t) => 1f - Mathf.Pow(1f - t, 3f);
    public static float EaseInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    public static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // Calls step with the eased progress (0 to 1) every frame for the given time.
    public static IEnumerator Animate(float seconds, Action<float> step, Func<float, float> ease = null)
    {
        ease = ease ?? EaseOut;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            step(ease(t / seconds));
            yield return null;
        }
        step(1f);
    }

    public static IEnumerator Fade(CanvasGroup group, float to, float seconds)
    {
        float from = group.alpha;
        yield return Animate(seconds, k => group.alpha = Mathf.Lerp(from, to, k));
    }

    // Reveals text letter by letter. hurry() true shows the rest at once; onLetter
    // runs for every few letters (a typing sound).
    public static IEnumerator Type(TMP_Text text, string content, float lettersPerSecond, Func<bool> hurry = null, Action onLetter = null)
    {
        text.text = content;
        text.ForceMeshUpdate();
        int total = text.textInfo.characterCount;
        float shown = 0f;
        int last = 0;
        while (shown < total && (hurry == null || !hurry()))
        {
            shown += lettersPerSecond * Time.unscaledDeltaTime;
            int now = Mathf.Min(total, Mathf.FloorToInt(shown));
            if (onLetter != null && now / 3 > last / 3) onLetter();
            last = now;
            text.maxVisibleCharacters = now;
            yield return null;
        }
        text.maxVisibleCharacters = 99999;
    }
}
