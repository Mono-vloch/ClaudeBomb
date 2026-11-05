using System.Collections.Generic;
using UnityEngine;

// The track's colours and materials: asphalt, kerbs, grass and barriers in the
// day; the same track at dusk on the cover. Materials are made once per colour
// and shared, so the parts batch.
public static class Look
{
    public static readonly Color SkyTop = Theme.Hex("#6FB3EA"), SkyHorizon = Theme.Hex("#E6F2FA"), SkyBottom = Theme.Hex("#B6D7BC"), Haze = Theme.Hex("#C4DAE8");
    public static readonly Color DuskTop = Theme.Ink, DuskHorizon = Theme.Hex("#F0915A"), DuskBottom = Theme.Hex("#2B3E52");
    public static readonly Color SunDay = Theme.Hex("#FFF1DC"), SunDusk = Theme.Hex("#FFA868"), Lamp = Theme.Hex("#FFD27A");

    public static readonly Color Grass = Theme.Hex("#86BE76"), Asphalt = Theme.Hex("#4A4F5A"), AsphaltLine = Theme.Hex("#D9D6CC"), Oil = Theme.Hex("#23262E");
    public static readonly Color KerbRed = Theme.Hex("#D6453D"), KerbWhite = Theme.Hex("#F4F6F7"), Barrier = Theme.Hex("#B9C2CC"), BarrierDark = Theme.Hex("#97A3AF");
    public static readonly Color Trunk = Theme.Hex("#7F5236"), Leaf = Theme.Hex("#4FA058"), LeafDark = Theme.Hex("#3F8C4A");
    public static readonly Color Stand = Theme.Hex("#D8D2C4"), StandDark = Theme.Hex("#B6AE9D");

    public static readonly Color Car = Theme.Aqua, CarDark = Theme.Hex("#0E7A6E"), Cabin = Theme.Hex("#2B2440"), Wheel = Theme.Hex("#2A2E36"), Shell = Theme.Hex("#F1F4F6");
    public static readonly Color RayClear = Theme.Aqua, RayNear = Theme.Amber, RayWall = Theme.Danger, RayOff = new Color(0.6f, 0.65f, 0.7f, 0.25f);
    public static readonly Color Grey = Theme.Hex("#9AA5B1");

    static Shader flat, glass, sky;
    static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

    static Shader Load(string name, ref Shader cache)
    {
        if (cache != null) return cache;
        cache = Resources.Load<Shader>("Shaders/" + name);
        if (cache == null) { Debug.LogWarning($"Shader {name} missing from Resources/Shaders; using Diffuse."); cache = Shader.Find("Diffuse"); }
        return cache;
    }

    public static Material Mat(Color c) => Mat(c, Color.black);

    public static Material Mat(Color c, Color glow)
    {
        string key = "f" + ColorUtility.ToHtmlStringRGBA(c) + ColorUtility.ToHtmlStringRGBA(glow);
        if (materials.TryGetValue(key, out var m) && m != null) return m;
        m = new Material(Load("Flat", ref flat)) { name = key, color = c, enableInstancing = true };
        m.SetColor("_Emission", glow);
        materials[key] = m;
        return m;
    }

    public static Material Own(Color c) => new Material(Load("Flat", ref flat)) { color = c };

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
