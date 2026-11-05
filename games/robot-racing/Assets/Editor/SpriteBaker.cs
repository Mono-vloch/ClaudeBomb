using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Draws every sprite the game uses, from shapes, into Assets/Resources/Sprites.
// Flat sticker look: fills with a dark outline, no gradients. Re-run after
// changing a shape; the PNGs are checked in so a clone works without this.
public static class SpriteBaker
{
    const string Folder = "Assets/Resources/Sprites";
    const int Size = 128;
    static readonly Color Outline = Theme.Hex("#2B2440");

    [MenuItem("Robot Racing/Bake Sprites")]
    public static void Bake()
    {
        Directory.CreateDirectory(Folder);
        foreach (var (name, shapes) in Catalogue()) Write(name, shapes, Size);
        WriteUi();
        AssetDatabase.Refresh();
        Debug.Log("Robot Racing: sprites baked into " + Folder);
    }

    public static bool Missing => !File.Exists(Path.Combine(Folder, "apple.png")) || !File.Exists(Path.Combine(Folder, "ui_round.png"));

    // ---- shapes --------------------------------------------------------------

    abstract class Shape
    {
        public Color fill;
        public float outline = 3.2f;
        public abstract float Dist(Vector2 p);
    }
    class Circle : Shape { public Vector2 c; public float r; public override float Dist(Vector2 p) => (p - c).magnitude - r; }
    class Ellipse : Shape
    {
        public Vector2 c; public float rx, ry; public float angle;
        public override float Dist(Vector2 p)
        {
            var q = Rot(p - c, -angle);
            float k = Mathf.Min(rx, ry);
            return (new Vector2(q.x / rx, q.y / ry).magnitude - 1f) * k;
        }
    }
    class RoundRect : Shape
    {
        public Vector2 c, half; public float radius, angle;
        public override float Dist(Vector2 p)
        {
            var q = Rot(p - c, -angle);
            var d = new Vector2(Mathf.Abs(q.x), Mathf.Abs(q.y)) - half + Vector2.one * radius;
            return Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f) - radius;
        }
    }
    class Capsule : Shape
    {
        public Vector2 a, b; public float r;
        public override float Dist(Vector2 p)
        {
            var pa = p - a; var ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude - r;
        }
    }
    class Triangle : Shape
    {
        public Vector2 p0, p1, p2; public float round;
        public override float Dist(Vector2 p)
        {
            Vector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
            Vector2 v0 = p - p0, v1 = p - p1, v2 = p - p2;
            Vector2 pq0 = v0 - e0 * Mathf.Clamp01(Vector2.Dot(v0, e0) / Vector2.Dot(e0, e0));
            Vector2 pq1 = v1 - e1 * Mathf.Clamp01(Vector2.Dot(v1, e1) / Vector2.Dot(e1, e1));
            Vector2 pq2 = v2 - e2 * Mathf.Clamp01(Vector2.Dot(v2, e2) / Vector2.Dot(e2, e2));
            float s = Mathf.Sign(e0.x * e2.y - e0.y * e2.x);
            float dx = Mathf.Min(Mathf.Min(Vector2.Dot(pq0, pq0), Vector2.Dot(pq1, pq1)), Vector2.Dot(pq2, pq2));
            float dy = Mathf.Min(Mathf.Min(s * (v0.x * e0.y - v0.y * e0.x), s * (v1.x * e1.y - v1.y * e1.x)), s * (v2.x * e2.y - v2.y * e2.x));
            return -Mathf.Sqrt(dx) * Mathf.Sign(dy) - round;
        }
    }
    class Union : Shape
    {
        public List<Shape> parts;
        public override float Dist(Vector2 p) { float d = float.MaxValue; foreach (var s in parts) d = Mathf.Min(d, s.Dist(p)); return d; }
    }
    class Crescent : Shape
    {
        public Vector2 c, cut; public float r, rcut;
        public override float Dist(Vector2 p) => Mathf.Max((p - c).magnitude - r, -((p - cut).magnitude - rcut));
    }

    static Vector2 Rot(Vector2 v, float degrees)
    {
        float a = degrees * Mathf.Deg2Rad, c = Mathf.Cos(a), s = Mathf.Sin(a);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    // ---- the catalogue -------------------------------------------------------
    // Coordinates in a 128 unit square, origin top-left, y down (like the screen).

    static Color C(string hex) => Theme.Hex(hex);
    static Circle Ci(float x, float y, float r, string hex) => new Circle { c = new Vector2(x, y), r = r, fill = C(hex) };
    static Ellipse El(float x, float y, float rx, float ry, string hex, float angle = 0) => new Ellipse { c = new Vector2(x, y), rx = rx, ry = ry, fill = C(hex), angle = angle };
    static RoundRect Rr(float x, float y, float w, float h, float radius, string hex, float angle = 0) => new RoundRect { c = new Vector2(x, y), half = new Vector2(w / 2, h / 2), radius = radius, fill = C(hex), angle = angle };
    static Capsule Ca(float x0, float y0, float x1, float y1, float r, string hex) => new Capsule { a = new Vector2(x0, y0), b = new Vector2(x1, y1), r = r, fill = C(hex) };
    static Triangle Tr(float x0, float y0, float x1, float y1, float x2, float y2, float round, string hex) => new Triangle { p0 = new Vector2(x0, y0), p1 = new Vector2(x1, y1), p2 = new Vector2(x2, y2), round = round, fill = C(hex) };
    static Union Un(string hex, params Shape[] parts) => new Union { parts = new List<Shape>(parts), fill = C(hex) };
    static Crescent Cr(float x, float y, float r, float cx, float cy, float rcut, string hex) => new Crescent { c = new Vector2(x, y), r = r, cut = new Vector2(cx, cy), rcut = rcut, fill = C(hex) };

    static IEnumerable<(string, List<Shape>)> Catalogue()
    {
        const string leaf = "#5DBB46", stem = "#7A4E2D", red = "#E6413A", yellow = "#FFD23F", orange = "#FF8C2B", green = "#8CCB4E", purple = "#8D5BD9", brown = "#B8865B";

        yield return ("apple", new List<Shape> {
            Ca(64, 40, 70, 22, 4, stem),
            Un(red, Ci(50, 70, 32, red), Ci(78, 70, 32, red)),
            El(80, 34, 16, 8, leaf, -30),
            El(50, 58, 7, 11, "#FF8C85", -20) });
        yield return ("banana", new List<Shape> {
            Un(yellow, Cr(64, 40, 48, 64, 18, 44, yellow), El(22, 60, 7, 5, yellow, 60), El(106, 60, 7, 5, yellow, -60)),
            Ci(21, 62, 4, stem), Ci(107, 62, 4, stem) });
        yield return ("orange", new List<Shape> {
            Ci(64, 70, 38, orange),
            El(72, 34, 15, 7, leaf, -25),
            Ci(54, 60, 4, "#FFB26B"), Ci(44, 76, 3, "#FFB26B") });
        yield return ("strawberry", new List<Shape> {
            Tr(30, 50, 98, 50, 64, 112, 14, red),
            El(50, 42, 14, 8, leaf, 20), El(78, 42, 14, 8, leaf, -20), El(64, 36, 9, 12, leaf),
            Ci(54, 66, 3, yellow), Ci(74, 70, 3, yellow), Ci(62, 86, 3, yellow), Ci(50, 82, 3, yellow), Ci(78, 86, 3, yellow) });
        yield return ("grapes", new List<Shape> {
            Ca(64, 30, 64, 16, 4, stem),
            Ci(46, 44, 14, purple), Ci(82, 44, 14, purple), Ci(64, 48, 14, purple),
            Ci(50, 68, 14, purple), Ci(78, 68, 14, purple), Ci(64, 70, 14, purple),
            Ci(56, 90, 14, purple), Ci(72, 90, 14, purple), Ci(64, 106, 13, purple) });
        yield return ("lemon", new List<Shape> {
            El(64, 66, 44, 30, yellow, -20),
            Ci(20, 80, 8, yellow), Ci(108, 52, 8, yellow),
            El(62, 58, 8, 14, "#FFF0A3", -20) });
        yield return ("cherry", new List<Shape> {
            Ca(44, 72, 70, 22, 4, stem), Ca(84, 72, 70, 22, 4, stem),
            El(78, 26, 15, 7, leaf, 30),
            Ci(42, 86, 20, red), Ci(86, 86, 20, red),
            Ci(36, 78, 4, "#FF8C85"), Ci(80, 78, 4, "#FF8C85") });
        yield return ("pear", new List<Shape> {
            Ca(64, 32, 68, 16, 4, stem),
            Un(green, Ci(64, 52, 20, green), Ci(64, 82, 32, green), Rr(64, 66, 40, 30, 10, green)),
            El(74, 24, 13, 7, leaf, -35) });
        yield return ("carrot", new List<Shape> {
            Ca(52, 30, 36, 8, 6, leaf), Ca(64, 30, 64, 6, 6, leaf), Ca(76, 30, 92, 8, 6, leaf),
            Tr(38, 34, 90, 34, 64, 118, 10, orange),
            Ca(50, 56, 74, 56, 2, "#E07420"), Ca(52, 76, 72, 76, 2, "#E07420") });
        yield return ("potato", new List<Shape> {
            El(64, 66, 42, 30, brown, 15),
            Ci(48, 58, 4, "#8E6239"), Ci(74, 72, 4, "#8E6239"), Ci(64, 50, 3, "#8E6239") });
        yield return ("broccoli", new List<Shape> {
            Rr(64, 92, 24, 40, 8, "#B9D98B"),
            Un("#4F9E3A", Ci(40, 56, 20, "#4F9E3A"), Ci(88, 56, 20, "#4F9E3A"), Ci(64, 42, 22, "#4F9E3A"), Ci(50, 70, 18, "#4F9E3A"), Ci(78, 70, 18, "#4F9E3A")),
            Ci(56, 44, 5, "#6DBF4E"), Ci(80, 60, 5, "#6DBF4E") });
        yield return ("mushroom", new List<Shape> {
            Rr(64, 90, 30, 40, 10, "#F3E6CF"),
            Un("#D94C3C", Cr(64, 68, 44, 64, 112, 50, "#D94C3C"), Rr(64, 70, 84, 12, 6, "#D94C3C")),
            Ci(48, 46, 6, "#FFF4E8"), Ci(74, 36, 6, "#FFF4E8"), Ci(84, 58, 5, "#FFF4E8") });
        yield return ("chili", new List<Shape> {
            Ca(70, 28, 86, 14, 5, leaf),
            Cr(64, 68, 44, 30, 44, 36, red) });
        yield return ("tomato", new List<Shape> {
            Ci(64, 72, 38, "#F2473D"),
            El(48, 36, 12, 6, leaf, 30), El(80, 36, 12, 6, leaf, -30), El(64, 32, 6, 10, leaf), El(56, 40, 10, 5, leaf, 60), El(72, 40, 10, 5, leaf, -60),
            Ci(50, 66, 4, "#FF8F85") });
        yield return ("corn", new List<Shape> {
            El(40, 76, 14, 44, leaf, 18), El(88, 76, 14, 44, leaf, -18),
            El(64, 64, 20, 46, yellow),
            Ci(56, 36, 3, "#E0A800"), Ci(72, 36, 3, "#E0A800"), Ci(56, 56, 3, "#E0A800"), Ci(72, 56, 3, "#E0A800"), Ci(56, 76, 3, "#E0A800"), Ci(72, 76, 3, "#E0A800"), Ci(64, 46, 3, "#E0A800"), Ci(64, 66, 3, "#E0A800"), Ci(64, 86, 3, "#E0A800") });

        // The robot: body and antenna are white so the team colour tints them; the
        // face is a separate sprite laid on top, untinted.
        yield return ("bot_body", new List<Shape> {
            Ca(64, 30, 64, 14, 3, "#FFFFFF"), Ci(64, 12, 7, "#FFFFFF"),
            Rr(64, 70, 76, 68, 18, "#FFFFFF"),
            Rr(38, 76, 12, 30, 5, "#FFFFFF"), Rr(90, 76, 12, 30, 5, "#FFFFFF") });
        yield return ("bot_face", new List<Shape> {
            new RoundRect { c = new Vector2(64, 62), half = new Vector2(26, 12), radius = 10, fill = C("#2B2440"), outline = 0 },
            new Circle { c = new Vector2(54, 62), r = 5, fill = C("#9BE8FF"), outline = 0 },
            new Circle { c = new Vector2(74, 62), r = 5, fill = C("#9BE8FF"), outline = 0 },
            new Capsule { a = new Vector2(54, 86), b = new Vector2(74, 86), r = 2.5f, fill = C("#2B2440"), outline = 0 } });
        yield return ("bot_out", new List<Shape> {
            new Capsule { a = new Vector2(50, 54), b = new Vector2(78, 72), r = 3.5f, fill = C("#2B2440"), outline = 0 },
            new Capsule { a = new Vector2(78, 54), b = new Vector2(50, 72), r = 3.5f, fill = C("#2B2440"), outline = 0 } });
        yield return ("chip", new List<Shape> {
            new Circle { c = new Vector2(64, 64), r = 60, fill = Color.white, outline = 2.5f } });

        // The market: a stall under every item (teal and white awning, wooden
        // counter), the house each robot starts from (white, tinted by its team
        // colour), and the basket on the debrief.
        yield return ("stall", new List<Shape> {
            Rr(64, 86, 104, 56, 10, "#E3C89C"),
            Rr(64, 106, 104, 16, 6, "#B8905E"),
            Rr(64, 30, 116, 26, 7, "#24E0C0"),
            Plain(Rr(22, 30, 13, 20, 3, "#FFFFFF")), Plain(Rr(50, 30, 13, 20, 3, "#FFFFFF")), Plain(Rr(78, 30, 13, 20, 3, "#FFFFFF")), Plain(Rr(106, 30, 13, 20, 3, "#FFFFFF")) });
        yield return ("house", new List<Shape> {
            Tr(14, 62, 114, 62, 64, 14, 4, "#FFFFFF"),
            Rr(64, 88, 78, 56, 8, "#FFFFFF"),
            Plain(Rr(64, 100, 18, 30, 3, "#2B2440")),
            Plain(Ci(42, 84, 7, "#9BE8FF")) });
        yield return ("basket", new List<Shape> {
            Ca(40, 62, 52, 34, 4, "#B8905E"), Ca(52, 34, 76, 34, 4, "#B8905E"), Ca(76, 34, 88, 62, 4, "#B8905E"),
            Rr(64, 86, 84, 50, 12, "#E3C89C"),
            Ca(26, 66, 102, 66, 3, "#B8905E") });
    }

    static Shape Plain(Shape s) { s.outline = 0; return s; }

    // ---- rasteriser ----------------------------------------------------------

    static void Write(string name, List<Shape> shapes, int size)
    {
        var px = new Color[size * size];
        float scale = size / 128f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / scale, (y + 0.5f) / scale);
                Color acc = new Color(0, 0, 0, 0);
                foreach (var s in shapes)
                {
                    float d = s.Dist(p);
                    if (s.outline > 0) acc = Over(acc, Outline, Mathf.Clamp01(0.5f - (d - s.outline)));
                    acc = Over(acc, s.fill, Mathf.Clamp01(0.5f - d));
                }
                px[(size - 1 - y) * size + x] = acc;
            }
        Save(name, px, size, size, Vector4.zero);
    }

    static Color Over(Color under, Color over, float a)
    {
        if (a <= 0) return under;
        float outA = a + under.a * (1 - a);
        if (outA <= 0) return new Color(0, 0, 0, 0);
        var rgb = (new Color(over.r, over.g, over.b) * a + new Color(under.r, under.g, under.b) * under.a * (1 - a)) / outA;
        return new Color(rgb.r, rgb.g, rgb.b, outA);
    }

    // The two sliced sprites the chrome is built from.
    static void WriteUi()
    {
        const int n = 64;
        var round = new Color[n * n];
        var circle = new Color[n * n];
        var rr = new RoundRect { c = new Vector2(32, 32), half = new Vector2(30, 30), radius = 14 };
        var ci = new Circle { c = new Vector2(32, 32), r = 30 };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                round[(n - 1 - y) * n + x] = new Color(1, 1, 1, Mathf.Clamp01(0.5f - rr.Dist(p)));
                circle[(n - 1 - y) * n + x] = new Color(1, 1, 1, Mathf.Clamp01(0.5f - ci.Dist(p)));
            }
        Save("ui_round", round, n, n, new Vector4(20, 20, 20, 20));
        Save("ui_circle", circle, n, n, Vector4.zero);

        // A speech bubble's tail, pointing down.
        const int m = 32;
        var tail = new Color[m * m];
        var tri = new Triangle { p0 = new Vector2(3, 3), p1 = new Vector2(29, 3), p2 = new Vector2(16, 29), round = 1.5f };
        for (int y = 0; y < m; y++)
            for (int x = 0; x < m; x++)
                tail[(m - 1 - y) * m + x] = new Color(1, 1, 1, Mathf.Clamp01(0.5f - tri.Dist(new Vector2(x + 0.5f, y + 0.5f))));
        Save("ui_tail", tail, m, m, Vector4.zero);

        // A soft shadow: a rounded rectangle whose edge fades out over 22 units.
        const int s = 96;
        var shadow = new Color[s * s];
        var srr = new RoundRect { c = new Vector2(48, 48), half = new Vector2(26, 26), radius = 12 };
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = srr.Dist(new Vector2(x + 0.5f, y + 0.5f));
                float a = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d + 2f) / 22f));
                shadow[(s - 1 - y) * s + x] = new Color(1, 1, 1, a);
            }
        Save("ui_shadow", shadow, s, s, new Vector4(40, 40, 40, 40));
    }

    static void Save(string name, Color[] px, int w, int h, Vector4 border)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels(px);
        tex.Apply();
        string path = Path.Combine(Folder, name + ".png");
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.spriteBorder = border;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
}
