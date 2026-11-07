using TMPro;
using UnityEngine;

// The look, from the Maaloomatiia brand guidelines: deep-blue ink, teal as a signal
// rather than a decoration, Inter for everything. Since the realism pass every screen is
// dark brand glass over the world, the way Claude Bomb's HUD sits on its lab: the panels,
// cards and fields below are ink glass with teal or chalk hairlines, and text is chalk.
public static class Theme
{
    public static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

    // Brand colours
    public static readonly Color Ink = Hex("#0B1536");        // Deep Knowledge Blue: headings, dark covers
    public static readonly Color Ink2 = Hex("#0E3556");       // Midnight Intellect: deep panels on dark
    public static readonly Color Teal = Hex("#24E0C0");       // the accent: keylines, dots, text on dark
    public static readonly Color Aqua = Hex("#16C2B0");       // the working teal for fills with big text
    public static readonly Color TealDeep = Hex("#0E7A6E");   // the one teal that carries small white text
    public static readonly Color Cyan = Hex("#63FFF3");       // highlight on dark only

    // The glass ramp (it was a white page ramp before the realism pass)
    public static readonly Color Chalk = Hex("#EEF2F6");      // text on the glass
    public static readonly Color Page = Hex("#0B1536");
    public static readonly Color Panel = new Color(0.043f, 0.071f, 0.165f, 0.93f);   // dark brand glass, ink at 93 %
    public static readonly Color Surface1 = Hex("#16213F");
    public static readonly Color Surface2 = new Color(0.063f, 0.18f, 0.24f, 0.95f);  // the standard card fill: teal-tinted ink
    public static readonly Color Surface3 = Hex("#1F2B4A");
    public static readonly Color Border1 = new Color(0.14f, 0.88f, 0.75f, 0.26f);    // the standard card stroke
    public static readonly Color Border = new Color(1f, 1f, 1f, 0.2f);              // neutral stroke
    public static readonly Color BorderTeal = new Color(0.14f, 0.88f, 0.75f, 0.85f); // active stroke
    public static readonly Color Text2 = Hex("#C3CFDA");
    public static readonly Color Muted = Hex("#9DB0C2");
    public static readonly Color Faint = Hex("#9EAEBB");
    public static readonly Color Shadow = new Color(0f, 0f, 0f, 0.35f);

    // Signals outside the palette, as in Claude Bomb: the red for a ruined dinner.
    public static readonly Color Danger = Hex("#D6453D");
    public static readonly Color DangerText = Hex("#FF8F86");  // the red as text on the glass
    public static readonly Color DangerSoft = new Color(0.33f, 0.08f, 0.1f, 0.94f);
    public static readonly Color Amber = Hex("#E09B1A");
    public static readonly Color AmberSoft = new Color(0.31f, 0.2f, 0.05f, 0.94f);

    // The market square
    public static readonly Color Cobbles = Hex("#16213F"); // the story's board: ink, like the glass
    public static readonly Color Tile = Hex("#1F2C4E");

    public static readonly Color[] Team =
    {
        Hex("#FF6B5E"), Hex("#4F8EF7"), Hex("#6CC24A"), Hex("#FFB020"), Hex("#9B6BFF"),
        Hex("#16C2B0"), Hex("#FF6FB5"), Hex("#4A52C9"), Hex("#B5774A"), Hex("#6E7B8B"),
    };
    public static readonly string[] TeamNames = { "Coral", "Sky", "Lime", "Amber", "Violet", "Teal", "Pink", "Indigo", "Cocoa", "Slate" };

    static TMP_FontAsset body, bold;
    public static TMP_FontAsset Body => body != null ? body : (body = Make("Fonts/Inter-Regular"));
    public static TMP_FontAsset Bold => bold != null ? bold : (bold = Make("Fonts/Inter-Bold"));

    static TMP_FontAsset Make(string resource)
    {
        var baked = Resources.Load<TMP_FontAsset>(resource + " SDF"); // AI Arena > Bake Fonts
        if (baked != null) return baked;
        var font = Resources.Load<Font>(resource);
        if (font == null) { Debug.LogWarning($"Font {resource} missing, using the TextMesh Pro default."); return TMP_Settings.defaultFontAsset; }
        var asset = TMP_FontAsset.CreateFontAsset(font);
        asset.name = font.name;
        return asset;
    }

    public static Sprite Sprite(string name)
    {
        var s = Resources.Load<Sprite>("Sprites/" + name);
        if (s == null) Debug.LogWarning($"Sprite {name} missing. Run AI Arena > Bake Sprites in the Editor.");
        return s;
    }
    public static Sprite Round => Sprite("ui_round");
    public static Sprite Circle => Sprite("ui_circle");
    public static Sprite Tail => Sprite("ui_tail");
    public static Sprite SoftShadow => Sprite("ui_shadow");
}
