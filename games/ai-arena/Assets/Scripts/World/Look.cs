using System.Collections.Generic;
using UnityEngine;

// The market's colours and materials: a sunny square in the day, the same square
// at dusk on the cover. Materials are made once per colour and shared, so the
// hundreds of parts batch. The brand's teal is on the awnings; the robots wear
// the team colours of the 2D screens (Theme.Team).
public static class Look
{
    // The day
    public static readonly Color SkyTop = Theme.Hex("#6FB3EA"), SkyHorizon = Theme.Hex("#E6F2FA"), SkyBottom = Theme.Hex("#B6D7BC");
    public static readonly Color Haze = Theme.Hex("#C4DAE8"); // the fog, a little deeper than the horizon
    public static readonly Color Grass = Theme.Hex("#8FBF6E"), GrassDark = Theme.Hex("#6FA35A"), Plaza = Theme.Hex("#CDC1A8"), Kerb = Theme.Hex("#A89C86");
    public static readonly Color TileA = Theme.Hex("#DED5C3"), TileB = Theme.Hex("#CEC4AF");
    public static readonly Color SunDay = Theme.Hex("#FFE3C0");

    // The dusk, on the cover
    public static readonly Color DuskTop = Theme.Ink, DuskHorizon = Theme.Hex("#F0915A"), DuskBottom = Theme.Hex("#2B3E52");
    public static readonly Color SunDusk = Theme.Hex("#FFA868"), Lamp = Theme.Hex("#FFD27A");

    // The stalls and the rest
    public static readonly Color Wood = Theme.Hex("#C08F57"), WoodDark = Theme.Hex("#8F6239"), Cloth = Theme.Hex("#E6DAC4");
    public static readonly Color Awning = Theme.Aqua, AwningWhite = Theme.Hex("#F4F6F7");
    public static readonly Color Trunk = Theme.Hex("#7F5236"), Leaf = Theme.Hex("#4FA058"), LeafDark = Theme.Hex("#3F8C4A"), Bush = Theme.Hex("#63AE5A");
    public static readonly Color Unknown = Theme.Hex("#B9C2CC"), UnknownDark = Theme.Hex("#97A3AF");

    // The robots
    public static readonly Color Shell = Theme.Hex("#F1F4F6"), Dark = Theme.Hex("#2B2440"), Eye = Theme.Hex("#9BE8FF"), Wheel = Theme.Hex("#3B3550");
    public static readonly Color Straw = Theme.Hex("#E3C89C"), Grey = Theme.Hex("#9AA5B1"), GreyDark = Theme.Hex("#6F7A86");
    public static readonly Color Ghost = new Color(0.55f, 0.6f, 0.66f, 0.3f);
    public static readonly Color Chalk = Theme.Hex("#E8E2D3"); // text on the dark glass

    public static Color Lighter(Color c, float k = 0.45f) => Color.Lerp(c, Color.white, k);
    public static Color Darker(Color c, float k = 0.35f) => Color.Lerp(c, Theme.Ink, k);

    static Shader flat, glass, sky;
    static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

    static Shader Load(string name, ref Shader cache)
    {
        if (cache != null) return cache;
        cache = Resources.Load<Shader>("Shaders/" + name);
        if (cache == null) { Debug.LogWarning($"Shader {name} missing from Resources/Shaders; using Diffuse."); cache = Shader.Find("Diffuse"); }
        return cache;
    }

    public static Material Mat(Color c) => Mat(c, Color.black, 0f);
    public static Material Mat(Color c, Color glow) => Mat(c, glow, 0f);

    // gloss: the sheen, 0 for matte parts, up to 1 for a robot's shell and its eyes.
    public static Material Mat(Color c, Color glow, float gloss)
    {
        string key = "f" + ColorUtility.ToHtmlStringRGBA(c) + ColorUtility.ToHtmlStringRGBA(glow) + gloss.ToString("0.00");
        if (materials.TryGetValue(key, out var m) && m != null) return m;
        m = new Material(Load("Flat", ref flat)) { name = key, color = c, enableInstancing = true };
        m.SetColor("_Emission", glow);
        m.SetFloat("_Gloss", gloss);
        materials[key] = m;
        return m;
    }

    // One of its own, for a part whose colour changes (the antenna while thinking).
    public static Material Own(Color c, float gloss = 0f)
    {
        var m = new Material(Load("Flat", ref flat)) { color = c };
        m.SetFloat("_Gloss", gloss);
        return m;
    }

    // The sky the sheen reflects: the Flat shader reads these globals.
    public static void SetSky(Color top, Color horizon)
    {
        Shader.SetGlobalColor("_SkyTopColor", top);
        Shader.SetGlobalColor("_SkyHorizonColor", horizon);
    }

    public static Material Glass(Color c)
    {
        string key = "g" + ColorUtility.ToHtmlStringRGBA(c);
        if (materials.TryGetValue(key, out var m) && m != null) return m;
        m = new Material(Load("Glass", ref glass)) { name = key, color = c };
        materials[key] = m;
        return m;
    }

    public static Material Sky(Color top, Color horizon, Color bottom)
    {
        var m = new Material(Load("Sky", ref sky));
        m.SetColor("_Top", top); m.SetColor("_Horizon", horizon); m.SetColor("_Bottom", bottom);
        return m;
    }
}
