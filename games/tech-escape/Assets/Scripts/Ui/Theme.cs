using TMPro;
using UnityEngine;

// The look, from the Maaloomatiia brand guidelines: white pages, deep-blue ink,
// a cool teal-tinted neutral ramp for cards and hairlines, and teal as a signal
// rather than a decoration. Dark (Ink) only on the cover screens, the title and
// the story. Inter for everything.
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

    // The neutral ramp
    public static readonly Color Page = Color.white;
    public static readonly Color Panel = Color.white;
    public static readonly Color Surface1 = Hex("#F5F7F8");
    public static readonly Color Surface2 = Hex("#ECF8F6");   // the standard card fill
    public static readonly Color Surface3 = Hex("#E9EDF0");
    public static readonly Color Border1 = Hex("#D9F2EC");    // the standard card stroke
    public static readonly Color Border = Hex("#D1D8E0");     // neutral stroke
    public static readonly Color BorderTeal = Hex("#AAE2D5"); // active stroke
    public static readonly Color Text2 = Hex("#5B6B78");
    public static readonly Color Muted = Hex("#8297AC");
    public static readonly Color Faint = Hex("#9EAEBB");      // on dark only
    public static readonly Color Shadow = new Color(0.51f, 0.59f, 0.67f, 0.2f); // #8297AC, barely there

    // Signals outside the palette, as in Claude Bomb: the red for a ruined dinner.
    public static readonly Color Danger = Hex("#D6453D");
    public static readonly Color DangerSoft = Hex("#FBE9E7");
    public static readonly Color Amber = Hex("#E09B1A");
    public static readonly Color AmberSoft = Hex("#FFF4DB");

    // The market square
    public static readonly Color Cobbles = Hex("#E9EDF0");
    public static readonly Color Tile = Hex("#F5F7F8");

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
