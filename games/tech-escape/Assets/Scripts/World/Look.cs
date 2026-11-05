using System.Collections.Generic;
using UnityEngine;

// The facility's colours and materials, from Docs/art-bible.md: a concrete corridor
// in lockdown blue, a door to every room with a strip that says locked, current, held
// or breached, racks with their LEDs, screens that glow, the attacker in shadow with
// a scarlet visor. Materials are made once per colour and shared, so the hundreds of
// parts batch.
public static class Look
{
    // The night
    public static readonly Color NightTop = Theme.Ink, NightHorizon = Theme.Hex("#1E3A5F"), NightBottom = Theme.Hex("#0B1536");
    public static readonly Color Haze = Theme.Hex("#1E3A5F"), Moon = Theme.Hex("#5E78A8");
    public static readonly Color Ground = Theme.Hex("#161D2E");

    // The corridor and the rooms
    public static readonly Color Floor = Theme.Hex("#3A4356"), FloorLine = Theme.Hex("#55607A"), Wall = Theme.Hex("#8C96A8"), WallDark = Theme.Hex("#6B7489"), Skirting = Theme.Hex("#2B2440");
    public static readonly Color Door = Theme.Hex("#2B2440"), DoorFrame = Theme.Hex("#16C2B0"), Sign = Theme.Hex("#F1F4F6"), Exit = Theme.Hex("#0E3556");
    public static readonly Color Locked = Theme.Hex("#D6453D"), Current = Theme.Hex("#E09B1A"), Held = Theme.Hex("#16C2B0"), Breached = Theme.Hex("#FF4B3E");
    public static readonly Color Rack = Theme.Hex("#1F2430"), RackLed = Theme.Hex("#6CC24A"), Desk = Theme.Hex("#C08F57"), Screen = Theme.Hex("#9BE8FF"), Chair = Theme.Hex("#3B3550");
    public static readonly Color Lamp = Theme.Hex("#DDE6FF"), DeskLamp = Theme.Hex("#FFD27A");
    public static readonly Color Decided = Theme.Hex("#16C2B0"), Undecided = Theme.Hex("#E09B1A");
    public static readonly Color Paper = Theme.Hex("#F1F4F6"), Steel = Theme.Hex("#9AA5B1"), Cable = Theme.Hex("#4F8EF7"), Tape = Theme.Hex("#3B3550");

    // The attacker and the rest
    public static readonly Color Attacker = Theme.Hex("#3A3556"), Visor = Theme.Hex("#FF4B3E"), Trail = new Color(1f, 0.29f, 0.24f, 0.6f);
    public static readonly Color Grey = Theme.Hex("#9AA5B1"), Ghost = new Color(0.55f, 0.6f, 0.66f, 0.3f);
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

    // gloss: the sheen, 0 for concrete, up to 1 for a screen, a visor, a safe's door.
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

    // One of its own, for a part whose colour changes (a door's strip, the visor).
    public static Material Own(Color c, Color glow, float gloss = 0f)
    {
        var m = new Material(Load("Flat", ref flat)) { color = c };
        m.SetColor("_Emission", glow);
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
