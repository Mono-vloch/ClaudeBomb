using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectBootstrap
{
    // Dev-only: draws simple placeholder shapes for the 7 manual glyphs so Module B can
    // be tested before real art exists. Safe to delete once real glyph sprites replace
    // these during the art pass.
    public static class GlyphSpriteGenerator
    {
        const int Size = 128;
        const string OutputFolder = "Assets/Art/Glyphs";
        static readonly Color White = Color.white;
        static readonly Color Clear = new Color(0, 0, 0, 0);

        [MenuItem("Claude Crisis Protocol/Generate Placeholder Glyph Sprites")]
        public static void Generate()
        {
            Directory.CreateDirectory(OutputFolder);

            SaveSprite("g1", DrawG1()); // circle with a bar across its middle
            SaveSprite("g2", DrawG2()); // triangle with a dot inside
            SaveSprite("g3", DrawG3()); // square with a V-notch in its top edge
            SaveSprite("g4", DrawG4()); // crescent
            SaveSprite("g5", DrawG5()); // five-pointed star
            SaveSprite("g6", DrawG6()); // hourglass
            SaveSprite("g7", DrawG7()); // zigzag / lightning bolt

            AssetDatabase.Refresh();
            Debug.Log($"[GlyphSpriteGenerator] Wrote 7 placeholder sprites to {OutputFolder}. " +
                      "Assign them into ModuleB_Glyphs' Glyph Sprites array in order g1..g7.");
        }

        static void SaveSprite(string name, Texture2D tex)
        {
            string path = $"{OutputFolder}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        static Texture2D Blank()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var pixels = new Color32[Size * Size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(0, 0, 0, 0);
            tex.SetPixels32(pixels);
            return tex;
        }

        static Texture2D DrawG1()
        {
            var tex = Blank();
            DrawRing(tex, new Vector2(64, 64), 40, 7, White);
            DrawThickLine(tex, new Vector2(20, 64), new Vector2(108, 64), 7, White);
            tex.Apply();
            return tex;
        }

        static Texture2D DrawG2()
        {
            var tex = Blank();
            Vector2 top = new Vector2(64, 104), bl = new Vector2(20, 28), br = new Vector2(108, 28);
            DrawThickLine(tex, top, bl, 7, White);
            DrawThickLine(tex, bl, br, 7, White);
            DrawThickLine(tex, br, top, 7, White);
            DrawFilledCircle(tex, new Vector2(64, 52), 8, White);
            tex.Apply();
            return tex;
        }

        static Texture2D DrawG3()
        {
            var tex = Blank();
            Vector2 tl = new Vector2(24, 104), tr = new Vector2(104, 104);
            Vector2 bl = new Vector2(24, 24), br = new Vector2(104, 24);
            Vector2 notchL = new Vector2(54, 104), notchR = new Vector2(74, 104), notchTip = new Vector2(64, 86);

            DrawThickLine(tex, tl, notchL, 7, White);
            DrawThickLine(tex, notchL, notchTip, 7, White);
            DrawThickLine(tex, notchTip, notchR, 7, White);
            DrawThickLine(tex, notchR, tr, 7, White);
            DrawThickLine(tex, tr, br, 7, White);
            DrawThickLine(tex, br, bl, 7, White);
            DrawThickLine(tex, bl, tl, 7, White);
            tex.Apply();
            return tex;
        }

        static Texture2D DrawG4()
        {
            var tex = Blank();
            DrawFilledCircle(tex, new Vector2(60, 64), 38, White);
            DrawFilledCircle(tex, new Vector2(78, 64), 34, Clear);
            tex.Apply();
            return tex;
        }

        static Texture2D DrawG5()
        {
            var tex = Blank();
            var points = new Vector2[10];
            const float outerR = 42, innerR = 17;
            var center = new Vector2(64, 62);
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI / 2 + i * Mathf.PI / 5;
                float r = i % 2 == 0 ? outerR : innerR;
                points[i] = center + new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
            }
            FillPolygon(tex, points, White);
            tex.Apply();
            return tex;
        }

        static Texture2D DrawG6()
        {
            var tex = Blank();
            FillTriangle(tex, new Vector2(24, 104), new Vector2(104, 104), new Vector2(64, 64), White);
            FillTriangle(tex, new Vector2(24, 24), new Vector2(104, 24), new Vector2(64, 64), White);
            tex.Apply();
            return tex;
        }

        static Texture2D DrawG7()
        {
            var tex = Blank();
            var points = new[] { new Vector2(78, 112), new Vector2(46, 66), new Vector2(68, 66), new Vector2(40, 16) };
            for (int i = 0; i < points.Length - 1; i++)
                DrawThickLine(tex, points[i], points[i + 1], 9, White);
            tex.Apply();
            return tex;
        }

        static void SetPixelSafe(Texture2D tex, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) return;
            tex.SetPixel(x, y, c);
        }

        static void DrawFilledCircle(Texture2D tex, Vector2 center, float radius, Color c)
        {
            int minX = Mathf.FloorToInt(center.x - radius), maxX = Mathf.CeilToInt(center.x + radius);
            int minY = Mathf.FloorToInt(center.y - radius), maxY = Mathf.CeilToInt(center.y + radius);
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    if (Vector2.Distance(new Vector2(x, y), center) <= radius)
                        SetPixelSafe(tex, x, y, c);
        }

        static void DrawRing(Texture2D tex, Vector2 center, float radius, float thickness, Color c)
        {
            int minX = Mathf.FloorToInt(center.x - radius - thickness), maxX = Mathf.CeilToInt(center.x + radius + thickness);
            int minY = Mathf.FloorToInt(center.y - radius - thickness), maxY = Mathf.CeilToInt(center.y + radius + thickness);
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (Mathf.Abs(d - radius) <= thickness * 0.5f) SetPixelSafe(tex, x, y, c);
                }
        }

        static void DrawThickLine(Texture2D tex, Vector2 a, Vector2 b, float thickness, Color c)
        {
            float len = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(len * 2));
            for (int i = 0; i <= steps; i++)
                DrawFilledCircle(tex, Vector2.Lerp(a, b, i / (float)steps), thickness * 0.5f, c);
        }

        static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b), d2 = Sign(p, b, c), d3 = Sign(p, c, a);
            bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }

        static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) =>
            (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);

        static void FillTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 c, Color col)
        {
            int minX = Mathf.FloorToInt(Mathf.Min(a.x, b.x, c.x)), maxX = Mathf.CeilToInt(Mathf.Max(a.x, b.x, c.x));
            int minY = Mathf.FloorToInt(Mathf.Min(a.y, b.y, c.y)), maxY = Mathf.CeilToInt(Mathf.Max(a.y, b.y, c.y));
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    if (PointInTriangle(new Vector2(x, y), a, b, c)) SetPixelSafe(tex, x, y, col);
        }

        static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (poly[i].y > p.y != poly[j].y > p.y &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            }
            return inside;
        }

        static void FillPolygon(Texture2D tex, Vector2[] poly, Color col)
        {
            float minX = poly.Min(p => p.x), maxX = poly.Max(p => p.x);
            float minY = poly.Min(p => p.y), maxY = poly.Max(p => p.y);
            for (int x = Mathf.FloorToInt(minX); x <= Mathf.CeilToInt(maxX); x++)
                for (int y = Mathf.FloorToInt(minY); y <= Mathf.CeilToInt(maxY); y++)
                    if (PointInPolygon(new Vector2(x, y), poly)) SetPixelSafe(tex, x, y, col);
        }
    }
}
