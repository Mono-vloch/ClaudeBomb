using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// AI Arena > Bake Fonts: static TextMesh Pro atlases for Inter Regular and Bold in
// Resources/Fonts, every Latin glyph the game can show already drawn (sampling 90,
// padding 9: the 10 % ratio on both, so the two weights match on one line). The game
// loads these first and only draws a font at runtime when they are missing, so the
// web build never builds an atlas while a screen waits. Run again after adding a
// character the game did not have.
public static class FontBaker
{
    const string Folder = "Assets/Resources/Fonts";
    static readonly string[] Names = { "Inter-Regular", "Inter-Bold" };

    [MenuItem("AI Arena/Bake Fonts")]
    public static void Bake() => Bake(true);

    public static void Bake(bool force)
    {
        var chars = new StringBuilder();
        for (int c = 32; c < 127; c++) chars.Append((char)c);
        for (int c = 160; c < 256; c++) chars.Append((char)c);
        chars.Append("–—‘’“”…•·→←↑↓×−≈≤≥≠™€★☆");
        foreach (var name in Names)
        {
            string path = $"{Folder}/{name} SDF.asset";
            if (!force && File.Exists(path)) continue;
            var font = AssetDatabase.LoadAssetAtPath<Font>($"{Folder}/{name}.ttf");
            if (font == null) { Debug.LogWarning($"[FontBaker] {Folder}/{name}.ttf missing"); continue; }
            var old = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (old != null) AssetDatabase.DeleteAsset(path);
            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
            asset.name = name + " SDF";
            if (!asset.TryAddCharacters(chars.ToString(), out string missing) && !string.IsNullOrEmpty(missing))
                Debug.LogWarning($"[FontBaker] {name}: not in the font: {missing}");
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(asset, path);
            var atlas = asset.atlasTextures[0];
            atlas.name = name + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, asset);
            asset.material.name = name + " Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            EditorUtility.SetDirty(asset);
            Debug.Log($"[FontBaker] {path}: {asset.characterTable.Count} characters, atlas {atlas.width}x{atlas.height}");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
