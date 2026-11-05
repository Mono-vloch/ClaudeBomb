using System.Collections.Generic;
using UnityEngine;

// The facility's colours and materials: a concrete corridor with a door to every
// room, door lights that say locked, held or breached, server racks with their
// LEDs, screens that glow. Night on the cover, working light inside.
public static class Look
{
    public static readonly Color SkyTop = Theme.Hex("#5C6B80"), SkyHorizon = Theme.Hex("#C9D3DC"), SkyBottom = Theme.Hex("#7A8694"), Haze = Theme.Hex("#B5C0CA");
    public static readonly Color NightTop = Theme.Ink, NightHorizon = Theme.Hex("#1E3A5F"), NightBottom = Theme.Hex("#0B1536");
    public static readonly Color SunDay = Theme.Hex("#F3F1EA"), SunNight = Theme.Hex("#5E78A8");

    public static readonly Color Floor = Theme.Hex("#8E949C"), FloorLine = Theme.Hex("#B9C0C8"), Wall = Theme.Hex("#D9DCE0"), WallDark = Theme.Hex("#C4C9CF"), Skirting = Theme.Hex("#2B2440");
    public static readonly Color Door = Theme.Hex("#2B2440"), DoorFrame = Theme.Hex("#16C2B0"), Sign = Theme.Hex("#F1F4F6"), Exit = Theme.Hex("#0E3556");
    public static readonly Color Locked = Theme.Hex("#D6453D"), Current = Theme.Hex("#E09B1A"), Held = Theme.Hex("#16C2B0"), Breached = Theme.Hex("#FF4B3E");
    public static readonly Color Rack = Theme.Hex("#1F2430"), RackLed = Theme.Hex("#6CC24A"), Desk = Theme.Hex("#C08F57"), Screen = Theme.Hex("#9BE8FF"), Chair = Theme.Hex("#3B3550");
    public static readonly Color Attacker = Theme.Hex("#1A1726"), Visor = Theme.Hex("#FF4B3E"), Grey = Theme.Hex("#9AA5B1"), Ghost = new Color(0.55f, 0.6f, 0.66f, 0.3f);

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
    public static Material Own(Color c, Color glow) { var m = new Material(Load("Flat", ref flat)) { color = c }; m.SetColor("_Emission", glow); return m; }
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
