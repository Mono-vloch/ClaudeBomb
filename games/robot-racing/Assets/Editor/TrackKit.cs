using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Robot Racing > Build track kit: the realism pass's circuit, made the way Claude Bomb makes
// Bay 3 and Tech Escape makes its facility (Docs/claude-bomb-recipe.md). Every world material
// is URP Lit with a base map and a normal map, made here from the CC0 texture sets in
// Environment/PolyHaven, the painted textures in Environment/Generated (made from the same
// CC0 detail) and the hero models' maps in Models/Hero; the results are prefabs in
// Resources/Track that Circuit places from the track's grid.
//
// The kit is in metres: one cell of the track is 2 m, so Circuit places it at half scale
// (the world's unit is the cell). This file: the materials, the models, the previews and the
// material check; the prefabs are in TrackKit.Prefabs.cs.
public static partial class TrackKit
{
    public const string PolyHaven = "Assets/Environment/PolyHaven";
    public const string HeroDir = "Assets/Models/Hero";
    public const string Generated = "Assets/Environment/Generated";
    public const string Out = "Assets/Resources/Track";

    // ---- Materials ----

    static Material Save(Material m, string path)
    {
        // A glowing material must not carry Unity's default "emissive is black" flag, or URP's
        // material validation strips its _EMISSION keyword (copies start with that flag).
        if (m.IsKeywordEnabled("_EMISSION")) m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing == null) { AssetDatabase.CreateAsset(m, path); return m; }
        existing.shader = m.shader;
        existing.CopyPropertiesFromMaterial(m);
        existing.shaderKeywords = m.shaderKeywords;
        foreach (var k in m.shaderKeywords) existing.EnableKeyword(k);
        existing.renderQueue = m.renderQueue;
        existing.globalIlluminationFlags = m.globalIlluminationFlags;
        EditorUtility.SetDirty(existing);
        return existing;
    }

    public static Texture2D Texture(string path, int maxSize, bool srgb, bool normalMap = false, TextureWrapMode wrap = TextureWrapMode.Repeat, bool alpha = false)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) { Debug.LogWarning("[TrackKit] no texture at " + path); return null; }
        bool dirty = importer.textureType != (normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default)
                     || importer.sRGBTexture != srgb || importer.maxTextureSize != maxSize || importer.wrapMode != wrap
                     || importer.alphaIsTransparency != alpha || importer.mipmapEnabled == false || !importer.crunchedCompression
                     || importer.alphaSource != (alpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None);
        if (dirty)
        {
            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = maxSize;
            importer.wrapMode = wrap;
            importer.alphaSource = alpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = alpha;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true; // crunched DXT: the web build's texture budget
            importer.compressionQuality = 50;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    public static Material Lit(string name, Texture2D baseMap, Texture2D normal, Texture2D mask, Texture2D emission, Color tint, Color emissionColor, Vector2? tiling = null, float smoothness = 1f, float bump = 1f, float metallic = 0f)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        m.SetColor("_BaseColor", tint);
        if (baseMap != null) m.SetTexture("_BaseMap", baseMap);
        if (normal != null) { m.SetTexture("_BumpMap", normal); m.SetFloat("_BumpScale", bump); m.EnableKeyword("_NORMALMAP"); }
        if (mask != null)
        {
            m.SetTexture("_MetallicGlossMap", mask);
            m.EnableKeyword("_METALLICSPECGLOSSMAP");
            m.SetFloat("_Smoothness", smoothness);
        }
        else { m.SetFloat("_Metallic", metallic); m.SetFloat("_Smoothness", smoothness); }
        if (emission != null || emissionColor.maxColorComponent > 0f)
        {
            if (emission != null) m.SetTexture("_EmissionMap", emission);
            m.SetColor("_EmissionColor", emissionColor);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }
        if (tiling.HasValue) m.SetTextureScale("_BaseMap", tiling.Value);
        m.enableInstancing = true;
        return Save(m, $"{Generated}/{name}.mat");
    }

    // Cut-out foliage and decals: alpha clip at 0.5, both faces drawn.
    static Material Cutout(Material m, bool twoSided = true)
    {
        m.SetFloat("_AlphaClip", 1f); m.SetFloat("_Cutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON");
        m.renderQueue = (int)RenderQueue.AlphaTest;
        m.SetOverrideTag("RenderType", "TransparentCutout");
        if (twoSided) { m.SetFloat("_Cull", 0f); m.doubleSidedGI = true; }
        EditorUtility.SetDirty(m);
        return m;
    }

    // A hero's material from its maps: <dir>/<file>_base.jpg, _normal.png, _mask.png, _emission.png.
    // detailSize: the normal and mask maps' size, smaller for props seen at a distance (the web budget).
    public static Material HeroMaterial(string folder, string file, int size, float emissionStrength = 0f, Color? tint = null, int detailSize = 0)
    {
        string p = $"{HeroDir}/{folder}/{file}";
        int ds = detailSize > 0 ? detailSize : size;
        var baseMap = Texture(p + "_base.jpg", size, true);
        var normal = Texture(p + "_normal.png", ds, false, normalMap: true);
        // A model with almost no metal in it keeps one smoothness, not a whole map (the web budget).
        var (metal, smooth) = MaskMeans(p + "_mask.png");
        var mask = metal >= 0.04f ? Texture(p + "_mask.png", Mathf.Min(ds, 1024), false) : null;
        var emission = File.Exists(p + "_emission.png") ? Texture(p + "_emission.png", Mathf.Min(size, 1024), true) : null;
        return Lit("hero_" + file, baseMap, normal, mask, emission, tint ?? Color.white,
                   emission != null ? Color.white * emissionStrength : Color.black, null, mask != null ? 1f : smooth);
    }

    // The mean metal (R) and smoothness (A) of a mask map, read from the file.
    static (float metal, float smooth) MaskMeans(string path)
    {
        if (!File.Exists(path)) return (0f, 0.4f);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(path));
        var px = tex.GetPixels32();
        double m = 0, a = 0;
        for (int i = 0; i < px.Length; i += 7) { m += px[i].r; a += px[i].a; }
        int n = (px.Length + 6) / 7;
        Object.DestroyImmediate(tex);
        return ((float)(m / n / 255.0), (float)(a / n / 255.0));
    }

    // A Poly Haven texture set ({id}_diff_1k.jpg, _nor_gl_1k.jpg, _arm_1k.jpg) as a tiling surface,
    // the ARM split into URP's metallic-smoothness when it has metal in it, one smoothness when not.
    public static Material Surface(string id, Color tint, Vector2 tiling, int size = 1024, float smoothnessScale = 1f, float bump = 1f, string name = null)
    {
        string dir = $"{PolyHaven}/{id}";
        var diff = Texture($"{dir}/{id}_diff_1k.jpg", size, true);
        var nor = Texture($"{dir}/{id}_nor_gl_1k.jpg", size, false, normalMap: true);
        var (metal, rough) = ArmMeans($"{dir}/{id}_arm_1k.jpg");
        Material m;
        if (metal < 0.04f)
            m = Lit(name ?? "surface_" + id, diff, nor, null, null, tint, Color.black, tiling, Mathf.Clamp01((1f - rough) * smoothnessScale), bump);
        else
        {
            var ms = SplitArm($"{dir}/{id}_arm_1k.jpg", id, size);
            m = Lit(name ?? "surface_" + id, diff, nor, ms, null, tint, Color.black, tiling, smoothnessScale, bump);
        }
        foreach (var t in new[] { "_BumpMap", "_MetallicGlossMap" }) m.SetTextureScale(t, tiling);
        EditorUtility.SetDirty(m);
        return m;
    }

    // The mean metal and roughness of a Poly Haven ARM map (B and G).
    static (float metal, float rough) ArmMeans(string armPath)
    {
        if (!File.Exists(armPath)) return (0f, 0.6f);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(armPath));
        var px = tex.GetPixels32();
        double m = 0, r = 0;
        for (int i = 0; i < px.Length; i += 7) { m += px[i].b; r += px[i].g; }
        int n = (px.Length + 6) / 7;
        Object.DestroyImmediate(tex);
        return ((float)(m / n / 255.0), (float)(r / n / 255.0));
    }

    // Poly Haven packs AO, roughness and metal in R, G and B: URP wants metal in R, smoothness in A.
    public static Texture2D SplitArm(string armPath, string name, int size)
    {
        if (!File.Exists(armPath)) return null;
        string msPath = $"{Generated}/{name}_metal_smooth.png";
        if (!File.Exists(msPath))
        {
            var arm = new Texture2D(2, 2);
            arm.LoadImage(File.ReadAllBytes(armPath));
            var src = arm.GetPixels32();
            var ms = new Color32[src.Length];
            for (int i = 0; i < src.Length; i++) ms[i] = new Color32(src[i].b, src[i].b, src[i].b, (byte)(255 - src[i].g));
            WritePng(msPath, arm.width, arm.height, ms);
        }
        return Texture(msPath, size, false);
    }

    public static void WritePng(string path, int w, int h, Color32[] pixels)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
    }

    // ---- Poly Haven models: URP Lit materials remapped onto every slot ----

    public static void SetUpModels()
    {
        foreach (var fbx in Directory.GetFiles(PolyHaven, "*.fbx", SearchOption.AllDirectories).Select(f => f.Replace('\\', '/')))
            SetUpModel(fbx);
    }

    static void SetUpModel(string fbx)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(fbx);
        importer.importCameras = importer.importLights = importer.importAnimation = false;
        importer.animationType = ModelImporterAnimationType.None;
        importer.addCollider = false;
        importer.meshCompression = ModelImporterMeshCompression.Medium; // the web budget
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        string folder = Path.GetDirectoryName(fbx).Replace('\\', '/'), asset = Path.GetFileName(folder);
        var slots = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Material>().Select(m => m.name)
            .Concat(importer.GetExternalObjectMap().Keys.Where(k => k.type == typeof(Material)).Select(k => k.name)).Distinct().ToList();
        foreach (var slot in slots)
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), slot), PropMaterial(folder, asset, slot));
        importer.SaveAndReimport();
    }

    // The maps of one texture set: {asset}_{set}_{map}_1k.jpg, set "" for the main one.
    static Dictionary<string, string> Maps(string folder, string asset, string set)
    {
        var maps = new Dictionary<string, string>();
        string prefix = asset + "_" + (set == "" ? "" : set + "_");
        foreach (var file in Directory.GetFiles(folder, prefix + "*_1k.*"))
        {
            if (file.EndsWith(".meta")) continue;
            string map = Path.GetFileNameWithoutExtension(file).Substring(prefix.Length).Replace("_1k", "");
            if (map.Contains("_") && map != "nor_gl") continue;
            maps[map] = file.Replace('\\', '/');
        }
        return maps;
    }

    static IEnumerable<string> Sets(string folder, string asset)
    {
        foreach (var file in Directory.GetFiles(folder, asset + "_*diff_1k.*"))
        {
            if (file.EndsWith(".meta")) continue;
            string name = Path.GetFileNameWithoutExtension(file);
            yield return name.Substring(asset.Length + 1, name.Length - asset.Length - 1 - "diff_1k".Length).TrimEnd('_');
        }
    }

    static Material PropMaterial(string folder, string asset, string slot)
    {
        var sets = Sets(folder, asset).Where(s => s != "dry").ToList();
        string set = sets.FirstOrDefault(s => s != "" && slot.ToLowerInvariant().Contains(s.ToLowerInvariant()))
                     ?? (sets.Contains("") ? "" : sets.FirstOrDefault() ?? "");
        var maps = Maps(folder, asset, set);
        const int size = 512;
        // Foliage keeps its alpha in the colour map (cut-out); everything else is opaque.
        bool foliage = maps.ContainsKey("alpha");
        var diff = maps.TryGetValue("diff", out var d) ? (foliage ? FoliageBase(d, maps["alpha"], asset, size) : Texture(d, size, true)) : null;
        var nor = maps.TryGetValue("nor_gl", out var n) ? Texture(n, size, false, normalMap: true) : null;
        Texture2D ms = null; float smooth = 0.4f;
        if (maps.TryGetValue("arm", out var a))
        {
            var (metal, rough) = ArmMeans(a);
            if (metal >= 0.04f) ms = SplitArm(a, Path.GetFileNameWithoutExtension(a).Replace("_arm_1k", ""), size);
            else smooth = Mathf.Clamp01(1f - rough);
        }
        string name = $"{asset}_{(set == "" ? "main" : set)}";
        var m = Lit(name, diff, nor, ms, null, Color.white, Color.black, null, ms != null ? 1f : smooth);
        if (foliage) Cutout(m);
        EditorUtility.SetDirty(m);
        return m;
    }

    // A colour map with its alpha map packed into A, for cut-out foliage.
    static Texture2D FoliageBase(string diffPath, string alphaPath, string name, int size)
    {
        string path = $"{Generated}/{name}_foliage.png";
        if (!File.Exists(path))
        {
            var c = new Texture2D(2, 2); c.LoadImage(File.ReadAllBytes(diffPath));
            var a = new Texture2D(2, 2); a.LoadImage(File.ReadAllBytes(alphaPath));
            if (a.width != c.width) { var rt = RenderTexture.GetTemporary(c.width, c.height); Graphics.Blit(a, rt); var r = new Texture2D(c.width, c.height); RenderTexture.active = rt; r.ReadPixels(new Rect(0, 0, c.width, c.height), 0, 0); RenderTexture.active = null; RenderTexture.ReleaseTemporary(rt); a = r; }
            var cp = c.GetPixels32(); var ap = a.GetPixels32();
            for (int i = 0; i < cp.Length; i++) cp[i].a = ap[i].r;
            WritePng(path, c.width, c.height, cp);
        }
        return Texture(path, size, true, alpha: true);
    }

    // ---- Preview: every hero from four sides ----

    [MenuItem("Robot Racing/Preview hero models")]
    public static void PreviewHeroes()
    {
        Directory.CreateDirectory(Generated);
        AssetDatabase.Refresh();
        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/look-audit/realism/models"));
        Directory.CreateDirectory(outDir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.42f, 0.45f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
        var sun = new GameObject("Key").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.6f; sun.transform.rotation = Quaternion.Euler(40, 30, 0);
        var cam = new GameObject("Cam").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.3f, 0.32f, 0.36f);
        cam.fieldOfView = 30f; cam.nearClipPlane = 0.05f; cam.farClipPlane = 50f;
        var rt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        // The raw models, then the finished prefabs (the car with its wheels, every piece of the kit).
        var items = new List<(string name, GameObject go)>();
        foreach (var dir in Directory.GetDirectories(HeroDir))
        {
            var glb = Directory.GetFiles(dir, "*.glb").FirstOrDefault();
            if (glb == null) continue;
            string folder = Path.GetFileName(dir), file = Path.GetFileNameWithoutExtension(Directory.GetFiles(dir, "*_base.jpg")[0]).Replace("_base", "");
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(glb.Replace('\\', '/'));
            if (asset == null) { Debug.LogWarning($"[TrackKit] {glb} did not import as a GameObject"); continue; }
            var go = Object.Instantiate(asset);
            var mat = HeroMaterial(folder, file, 1024, 2f);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray();
            items.Add(("raw-" + folder, go));
        }
        if (Directory.Exists(Out))
            foreach (var path in Directory.GetFiles(Out, "*.prefab").Select(p => p.Replace('\\', '/')))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (name == "Sky" || name == "Ground" || name == "Road" || name == "Paint" || name == "PitLane") continue;
                items.Add((name, (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path))));
            }
        foreach (var (_, go) in items) go.SetActive(false);
        foreach (var (name, go) in items)
        {
            go.SetActive(true);
            go.transform.position = Vector3.zero;
            var b = Bounds(go);
            Debug.Log($"[TrackKit] {name}: size {b.size.x:F3} x {b.size.y:F3} x {b.size.z:F3}, min {b.min}, centre {b.center}, " +
                      $"tris {go.GetComponentsInChildren<MeshFilter>().Where(f => f.sharedMesh != null).Sum(f => f.sharedMesh.triangles.Length / 3)}");
            var sheet = new Texture2D(512 * 4, 512, TextureFormat.RGB24, false);
            float dist = b.extents.magnitude / Mathf.Tan(15f * Mathf.Deg2Rad) * 1.05f;
            var views = new[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            for (int v = 0; v < 4; v++)
            {
                cam.transform.position = b.center + views[v] * dist + Vector3.up * dist * 0.15f;
                cam.transform.LookAt(b.center);
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
                tex.Apply();
                sheet.SetPixels(v * 512, 0, 512, 512, tex.GetPixels());
                RenderTexture.active = null;
            }
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(outDir, name + ".png"), sheet.EncodeToPNG());
            Object.DestroyImmediate(go);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[TrackKit] previews: +Z, +X, -Z, -X views in " + outDir);
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // The realism pass's machine check on the kit: every material on every renderer of every
    // prefab in Resources/Track is URP Lit with a base map and a normal map; none is Flat.
    [MenuItem("Robot Racing/Check track kit materials")]
    public static void CheckMaterials()
    {
        int renderers = 0, bad = 0;
        var seen = new HashSet<Material>();
        foreach (var path in Directory.GetFiles(Out, "*.prefab", SearchOption.AllDirectories))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue; // effects
                renderers++;
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) { bad++; Debug.LogWarning($"[Check] {path} {r.name}: no material"); continue; }
                    if (!seen.Add(m)) continue;
                    bool sky = m.shader != null && m.shader.name.Contains("Sky");
                    bool lit = m.shader != null && m.shader.name == "Universal Render Pipeline/Lit";
                    bool maps = m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null && m.HasProperty("_BumpMap") && m.GetTexture("_BumpMap") != null;
                    bool flat = m.shader != null && m.shader.name.Contains("Flat");
                    if (sky) { Debug.Log($"[Check] {m.name}: the sky dome ({m.shader.name}), exempt like a skybox"); continue; }
                    if (!lit || !maps || flat) { bad++; Debug.LogWarning($"[Check] {m.name} on {Path.GetFileNameWithoutExtension(path)}/{r.name}: shader {m.shader?.name}, base {(m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap")?.name : "-")}, normal {(m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap")?.name : "-")}"); }
                }
            }
        }
        Debug.Log($"[Check] {renderers} renderers, {seen.Count} materials, {bad} failing (URP Lit with a base map and a normal map)");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    public static Bounds Bounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer) && !(r is TrailRenderer)).ToArray();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
