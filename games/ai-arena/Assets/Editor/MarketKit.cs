using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// AI Arena > Build market kit: the realism pass's market, made the way Claude Bomb makes
// Bay 3 and Tech Escape makes its facility (Docs/claude-bomb-recipe.md). Every world
// material is URP Lit with a base map and a normal map, made here from the CC0 texture
// sets in Environment/PolyHaven, the Kenney food kit in Environment/Kenney (its palette
// texture with a flat normal map) and the Higgsfield models' maps in Models/Hero; the
// results are prefabs in Resources/Market that Market places from the arena's state.
//
// This file: the materials and the model set-up, the previews (every hero from four
// sides, the square from the game's own cameras) and the machine check. The prefabs are in
// MarketKit.Prefabs.cs.
public static partial class MarketKit
{
    public const string PolyHaven = "Assets/Environment/PolyHaven";
    public const string Kenney = "Assets/Environment/Kenney/Food";
    public const string HeroDir = "Assets/Models/Hero";
    public const string Generated = "Assets/Environment/Generated";
    public const string Out = "Assets/Resources/Market";

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

    static Texture2D Texture(string path, int maxSize, bool srgb, bool normalMap = false, TextureWrapMode wrap = TextureWrapMode.Repeat, bool crunch = true)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) return null;
        bool dirty = importer.textureType != (normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default)
                     || importer.sRGBTexture != srgb || importer.maxTextureSize != maxSize || importer.wrapMode != wrap
                     || importer.alphaIsTransparency || importer.mipmapEnabled == false || importer.crunchedCompression != crunch;
        if (dirty)
        {
            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = maxSize;
            importer.wrapMode = wrap;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.textureCompression = crunch ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = crunch; // crunched DXT: the web build's texture budget
            importer.compressionQuality = 50;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    public static Material Lit(string name, Texture2D baseMap, Texture2D normal, Texture2D mask, Texture2D emission, Color tint, Color emissionColor, Vector2? tiling = null, float smoothness = 1f, float bump = 1f)
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
        else { m.SetFloat("_Metallic", 0f); m.SetFloat("_Smoothness", smoothness); }
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

    // A hero's material from its maps: <dir>/<file>_base.jpg, _normal.png, _mask.png, _emission.png.
    // detailSize: the normal and mask maps' size, smaller for dressing (the web budget).
    public static Material HeroMaterial(string folder, string file, int size, float emissionStrength = 0f, Color? tint = null, int detailSize = 0, string name = null)
    {
        string p = $"{HeroDir}/{folder}/{file}";
        int ds = detailSize > 0 ? detailSize : size;
        var baseMap = Texture(p + "_base.jpg", size, true);
        var normal = Texture(p + "_normal.png", ds, false, normalMap: true);
        // A model with almost no metal in it keeps one smoothness, not a whole map (the web budget).
        var (metal, smooth) = MaskMeans(p + "_mask.png");
        var mask = metal >= 0.04f ? Texture(p + "_mask.png", Mathf.Min(ds, 1024), false) : null;
        var emission = File.Exists(p + "_emission.png") ? Texture(p + "_emission.png", Mathf.Min(size, 1024), true) : null;
        return Lit(name ?? "hero_" + file, baseMap, normal, mask, emission, tint ?? Color.white,
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
    // the ARM split into URP's metallic-smoothness when the set has metal in it.
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
            var (ms, _) = SplitArm($"{dir}/{id}_arm_1k.jpg", id, size);
            m = Lit(name ?? "surface_" + id, diff, nor, ms, null, tint, Color.black, tiling, smoothnessScale, bump);
        }
        foreach (var t in new[] { "_BumpMap", "_MetallicGlossMap", "_OcclusionMap" }) m.SetTextureScale(t, tiling);
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

    // Poly Haven packs AO, roughness and metal in R, G and B.
    public static (Texture2D metal, Texture2D occlusion) SplitArm(string armPath, string name, int size)
    {
        if (!File.Exists(armPath)) return (null, null);
        string msPath = $"{Generated}/{name}_metal_smooth.png", aoPath = $"{Generated}/{name}_occlusion.png";
        if (!File.Exists(msPath) || !File.Exists(aoPath))
        {
            var arm = new Texture2D(2, 2);
            arm.LoadImage(File.ReadAllBytes(armPath));
            var src = arm.GetPixels32();
            var ms = new Color32[src.Length];
            var ao = new Color32[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                ms[i] = new Color32(src[i].b, src[i].b, src[i].b, (byte)(255 - src[i].g));
                ao[i] = new Color32(src[i].r, src[i].r, src[i].r, 255);
            }
            WritePng(msPath, arm.width, arm.height, ms);
            WritePng(aoPath, arm.width, arm.height, ao);
        }
        return (Texture(msPath, size, false), Texture(aoPath, size, false));
    }

    static void WritePng(string path, int w, int h, Color32[] pixels)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
    }

    public static Texture2D FlatNormal()
    {
        string path = $"{Generated}/flat_normal.png";
        if (!File.Exists(path)) WritePng(path, 4, 4, Enumerable.Repeat(new Color32(128, 128, 255, 255), 16).ToArray());
        return Texture(path, 4, false, normalMap: true, crunch: false);
    }

    // A plain colour as a tiny texture (the antenna's bulb, the notebook's cover), so even the
    // smallest part is a textured Lit material.
    static Texture2D Swatch(string name, Color32 c)
    {
        string path = $"{Generated}/swatch_{name}.png";
        if (!File.Exists(path)) WritePng(path, 4, 4, Enumerable.Repeat(c, 16).ToArray());
        return Texture(path, 4, true, crunch: false);
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

    // The food from Poly Haven is seen close on the counters: its colour at 512 like the rest of
    // the props, the street lamps' glass glowing at dusk only (Market lights it).
    static Material PropMaterial(string folder, string asset, string slot)
    {
        var sets = Sets(folder, asset).ToList();
        string set = sets.FirstOrDefault(s => s != "" && slot.ToLowerInvariant().Contains(s.ToLowerInvariant()))
                     ?? (sets.Contains("") ? "" : sets.FirstOrDefault() ?? "");
        bool glass = slot.ToLowerInvariant().Contains("glass");
        var maps = Maps(folder, asset, set);
        const int size = 512;
        var diff = maps.TryGetValue("diff", out var d) ? Texture(d, size, true) : null;
        var nor = maps.TryGetValue("nor_gl", out var n) ? Texture(n, size, false, normalMap: true) : null;
        Texture2D ms = null; float smooth = 0.4f;
        if (maps.TryGetValue("arm", out var a))
        {
            var (metal, rough) = ArmMeans(a);
            if (metal >= 0.04f) ms = SplitArm(a, Path.GetFileNameWithoutExtension(a).Replace("_arm_1k", ""), size).metal;
            else smooth = Mathf.Clamp01(1f - rough);
        }
        string name = $"{asset}_{(glass ? "glass" : set == "" ? "main" : set)}";
        var m = Lit(name, diff, nor ?? FlatNormal(), ms, null, Color.white, Color.black, null, ms != null ? 1f : smooth);
        // Alpha from an opacity map (the street lamp's glass panes), as a cut-out.
        if (maps.TryGetValue("opacity", out var op) && diff != null && !glass)
        {
            m.SetFloat("_AlphaClip", 1f); m.SetFloat("_Cutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON");
            m.renderQueue = (int)RenderQueue.AlphaTest;
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---- Kenney's food: one smooth-shaded mesh per piece, one palette material ----

    static Material foodMaterial;

    static Material FoodMaterial()
    {
        if (foodMaterial != null) return foodMaterial;
        // The palette is a handful of flat swatches: no crunch (it would smear them), no mipmap blur across swatches.
        var palette = Texture($"{Kenney}/Textures/colormap.png", 256, true, crunch: false, wrap: TextureWrapMode.Clamp);
        var importer = (TextureImporter)AssetImporter.GetAtPath($"{Kenney}/Textures/colormap.png");
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear; importer.SaveAndReimport(); }
        foodMaterial = Lit("food_kenney", palette, FlatNormal(), null, null, Color.white, Color.black, null, 0.55f);
        return foodMaterial;
    }

    // Kenney's pieces are faceted (one normal per face): the mesh is copied with its normals
    // smoothed across faces that meet at less than 60 degrees, so a fruit reads round.
    static Mesh SmoothFood(string id)
    {
        string path = $"{Generated}/Meshes/food_{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        var src = AssetDatabase.LoadAllAssetsAtPath($"{Kenney}/{id}.glb").OfType<Mesh>().FirstOrDefault();
        if (src == null) { Debug.LogWarning($"[MarketKit] no mesh in {Kenney}/{id}.glb"); return null; }
        var mesh = Object.Instantiate(src);
        mesh.name = "food_" + id;
        var v = mesh.vertices; var n = mesh.normals;
        var groups = new Dictionary<Vector3Int, List<int>>();
        for (int i = 0; i < v.Length; i++)
        {
            var key = Vector3Int.RoundToInt(v[i] * 10000f);
            if (!groups.TryGetValue(key, out var list)) groups[key] = list = new List<int>();
            list.Add(i);
        }
        var smooth = new Vector3[v.Length];
        float cos = Mathf.Cos(60f * Mathf.Deg2Rad);
        foreach (var list in groups.Values)
            foreach (int i in list)
            {
                var sum = Vector3.zero;
                foreach (int j in list) if (Vector3.Dot(n[i], n[j]) >= cos) sum += n[j];
                smooth[i] = sum.sqrMagnitude > 0 ? sum.normalized : n[i];
            }
        mesh.normals = smooth;
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    // ---- Preview: every hero from four sides ----

    [MenuItem("AI Arena/Preview hero models")]
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
        cam.fieldOfView = 30f; cam.nearClipPlane = 0.05f; cam.farClipPlane = 80f;
        var rt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        foreach (var path in Directory.GetFiles($"{Out}/Hero", "*.prefab").Concat(new[] { $"{Out}/Agent.prefab" }).Concat(Directory.GetFiles($"{Out}/Goods", "*.prefab").Take(15)))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
            if (asset == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            var b = Bounds(go);
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
            File.WriteAllBytes(Path.Combine(outDir, Path.GetFileNameWithoutExtension(path) + ".png"), sheet.EncodeToPNG());
            Debug.Log($"[MarketKit] preview {Path.GetFileNameWithoutExtension(path)}: {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2} m, min y {b.min.y:F2}, " +
                      $"tris {go.GetComponentsInChildren<MeshFilter>().Where(f => f.sharedMesh != null).Sum(f => f.sharedMesh.triangles.Length / 3) + go.GetComponentsInChildren<SkinnedMeshRenderer>().Where(s => s.gameObject.activeInHierarchy).Sum(s => s.sharedMesh.triangles.Length / 3)}");
            Object.DestroyImmediate(go);
        }
        Debug.Log("[MarketKit] previews: +Z, +X, -Z, -X views in " + outDir);
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // The square dressed as a round would dress it, lit as the game lights the day, from the
    // game's cameras: the street view behind the agent, the agent's eye, the overview, the
    // garage's portrait. A cheap check of scale and framing before a game build.
    [MenuItem("AI Arena/Preview market kit")]
    public static void PreviewKit()
    {
        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/look-audit/realism/kit"));
        Directory.CreateDirectory(outDir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject Put(string path, Vector3 pos, float yaw)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/{path}.prefab"));
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            return go;
        }
        Vector3 Cell(int x, int y) => new Vector3((x - (Size - 1) / 2f) * Pitch, 0f, ((Size - 1) / 2f - y) * Pitch);
        Put("Square", Vector3.zero, 0f);
        var ids = Specs.Keys.ToArray();
        var rng = new System.Random(3);
        int n = 0;
        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
            {
                if (rng.NextDouble() > 0.3 || (x >= 3 && x <= 5 && y >= 4 && y <= 7)) continue;
                var stall = Put(n % 3 == 1 ? "StallB" : "StallA", Cell(x, y), y < Size / 2 ? 180f : 0f);
                var counter = stall.transform.Find("Counter");
                var goods = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Goods/{ids[n % ids.Length]}.prefab"));
                goods.transform.SetParent(counter, false);
                n++;
            }
        foreach (var (x, y) in new[] { (3, 4), (5, 4), (4, 3) })
        {
            var stall = Put("StallA", Cell(x, y), 180f);
            var goods = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Goods/{(x == 3 ? "apple" : x == 5 ? "tomato" : "corn")}.prefab"));
            goods.transform.SetParent(stall.transform.Find("Counter"), false);
        }
        var agent = Put("Agent", Cell(4, 6), 0f);
        var idle = agent.transform.Find("Idle").gameObject;
        var clip = idle.GetComponent<Animator>().runtimeAnimatorController.animationClips[0];
        clip.SampleAnimation(idle, 1.2f);
        Put("HomeStone", Cell(4, 0), 0f);

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.86f);
        RenderSettings.ambientEquatorColor = new Color(0.66f, 0.62f, 0.56f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.33f, 0.38f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = 0.018f;
        RenderSettings.fogColor = new Color(0.78f, 0.82f, 0.86f);
        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>($"{Out}/SkyDay.mat");
        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional; sun.shadows = LightShadows.Soft; sun.shadowStrength = 0.85f;
        sun.color = new Color(1f, 0.89f, 0.75f); sun.intensity = 1.6f; sun.transform.rotation = Quaternion.Euler(42f, -55f, 0f);
        RenderSettings.sun = sun;
        var cam = new GameObject("Cam").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox; cam.nearClipPlane = 0.1f; cam.farClipPlane = 400f; cam.allowHDR = true;
        var data = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(cam);
        data.renderPostProcessing = true; data.renderShadows = true;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.Add<UnityEngine.Rendering.Universal.Tonemapping>(true).mode.value = UnityEngine.Rendering.Universal.TonemappingMode.Neutral;
        var bloom = profile.Add<UnityEngine.Rendering.Universal.Bloom>(true); bloom.threshold.value = 1f; bloom.intensity.value = 0.25f;
        profile.Add<UnityEngine.Rendering.Universal.Vignette>(true).intensity.value = 0.2f;
        var vol = new GameObject("Volume").AddComponent<Volume>(); vol.isGlobal = true; vol.sharedProfile = profile;
        var rt = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        var a = agent.transform.position;
        var shots = new List<(string name, Vector3 pos, Vector3 look, float fov)>
        {
            ("street", a + new Vector3(0.75f, 1.3f, -1.7f), a + new Vector3(-0.05f, 0.78f, 0.6f), 50f),
            ("agents-eye", a + new Vector3(0.3f, 1.45f, -1.2f), a + new Vector3(0f, 1.0f, 5f), 55f),
            ("portrait", a + new Vector3(-0.9f, 1.0f, 2.6f), a + new Vector3(0f, 0.72f, 0f), 34f),
            ("overview", new Vector3(10f, 30f, -36f), new Vector3(0f, 0f, 0f), 34f),
            ("square-low", new Vector3(-6f, 1.6f, -10f), new Vector3(0f, 1.6f, 14f), 55f),
        };
        foreach (var (name, pos, look, fov) in shots)
        {
            cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(look - pos));
            cam.fieldOfView = fov;
            cam.Render(); cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(outDir, name + ".png"), tex.EncodeToPNG());
            if (name == "street")
            {
                var b = Bounds(idle);
                float x0 = 1, x1 = 0, y0 = 1, y1 = 0;
                for (int i = 0; i < 8; i++)
                {
                    var c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var v = cam.WorldToViewportPoint(c);
                    x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
                }
                Debug.Log($"[MarketKit] street shot: agent box {Mathf.Clamp01(x1) - Mathf.Clamp01(x0):P0} x {Mathf.Clamp01(y1) - Mathf.Clamp01(y0):P0} = {(Mathf.Clamp01(x1) - Mathf.Clamp01(x0)) * (Mathf.Clamp01(y1) - Mathf.Clamp01(y0)):P0}");
            }
        }
        Debug.Log($"[MarketKit] previewed the square into {outDir}");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // The agent's five clips, each driven by its own Animator as the game drives it, tinted
    // in the first team's colour: front three-quarter and side, a moment into each clip.
    [MenuItem("AI Arena/Preview agent clips")]
    public static void PreviewAgent()
    {
        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/look-audit/realism/models"));
        Directory.CreateDirectory(outDir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.62f, 0.68f, 0.78f);
        RenderSettings.ambientEquatorColor = new Color(0.5f, 0.5f, 0.5f);
        RenderSettings.ambientGroundColor = new Color(0.25f, 0.25f, 0.25f);
        var sun = new GameObject("Key").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.5f; sun.transform.rotation = Quaternion.Euler(40, 30, 0);
        var cam = new GameObject("Cam").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.3f, 0.32f, 0.36f);
        cam.fieldOfView = 30f; cam.nearClipPlane = 0.05f;
        var rt = new RenderTexture(400, 520, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Agent.prefab");
        string[] states = { "Idle", "Walk", "Look", "PickUp", "SentHome" };
        var sheet = new Texture2D(400 * states.Length, 520 * 2, TextureFormat.RGB24, false);
        var team = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
        var lin = new Color(1f, 0.42f, 0.37f).linear * 0.5f;
        team.SetPixels(Enumerable.Repeat(new Color(lin.r, lin.g, lin.b, 1f), 16).ToArray()); team.Apply();
        for (int s = 0; s < states.Length; s++)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            foreach (Transform c in go.transform) if (System.Array.IndexOf(states, c.name) >= 0) c.gameObject.SetActive(c.name == states[s]);
            var copy = go.transform.Find(states[s]);
            if (copy == null) { Object.DestroyImmediate(go); continue; }
            var mat = new Material(copy.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial);
            mat.SetTexture("_DetailAlbedoMap", team);
            foreach (var r in copy.GetComponentsInChildren<SkinnedMeshRenderer>()) r.sharedMaterial = mat;
            var anim = copy.GetComponent<Animator>();
            anim.Rebind();
            float len = anim.runtimeAnimatorController.animationClips[0].length;
            anim.Update(0f);
            anim.Update(len * 0.35f);
            var b = Bounds(copy.gameObject);
            var hips = copy.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Hips");
            Debug.Log($"[MarketKit] clip {states[s]}: {len:F2} s, box {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2}, centre {b.center}, hips forward {(hips != null ? hips.forward.ToString("F2") : "-")}");
            var views = new[] { new Vector3(0.6f, 0.25f, 1f), new Vector3(1f, 0.15f, 0f) };
            for (int v = 0; v < 2; v++)
            {
                var dir = views[v].normalized;
                cam.transform.position = new Vector3(0f, 0.62f, 0f) + dir * 3.4f;
                cam.transform.LookAt(new Vector3(0f, 0.62f, 0f));
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(400, 520, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 400, 520), 0, 0); tex.Apply();
                sheet.SetPixels(s * 400, (1 - v) * 520, 400, 520, tex.GetPixels());
                RenderTexture.active = null;
            }
            Object.DestroyImmediate(go);
        }
        sheet.Apply();
        File.WriteAllBytes(Path.Combine(outDir, "agent-clips.png"), sheet.EncodeToPNG());
        Debug.Log("[MarketKit] agent clips previewed (top: from the front-right, bottom: from its right side)");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // The realism pass's machine check on the kit: every material on every renderer of every
    // prefab in Resources/Market is URP Lit with a base map and a normal map; none is Flat.
    [MenuItem("AI Arena/Check market kit materials")]
    public static void CheckMaterials()
    {
        int renderers = 0, bad = 0, exempt = 0;
        var seen = new HashSet<Material>();
        foreach (var path in Directory.GetFiles(Out, "*.prefab", SearchOption.AllDirectories))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer || r.GetComponent<TMPro.TextMeshPro>() != null) { exempt++; continue; } // effects and world text
                renderers++;
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) { bad++; Debug.LogWarning($"[Check] {path} {r.name}: no material"); continue; }
                    if (!seen.Add(m)) continue;
                    bool lit = m.shader != null && m.shader.name == "Universal Render Pipeline/Lit";
                    bool maps = m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null && m.HasProperty("_BumpMap") && m.GetTexture("_BumpMap") != null;
                    if (!lit || !maps) { bad++; Debug.LogWarning($"[Check] {m.name} on {Path.GetFileNameWithoutExtension(path)}/{r.name}: shader {m.shader?.name}, base {(m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap")?.name : "-")}, normal {(m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap")?.name : "-")}"); }
                    if (m.shader != null && m.shader.name.Contains("Flat")) Debug.LogWarning($"[Check] FLAT shader on {path}/{r.name}");
                }
                var f = r.GetComponent<MeshFilter>();
                if (f != null && f.sharedMesh != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(f.sharedMesh))) Debug.LogWarning($"[Check] {path}/{r.name}: a mesh made at run time");
            }
        }
        Debug.Log($"[Check] {renderers} renderers, {seen.Count} materials, {bad} failing (URP Lit with a base map and a normal map), {exempt} exempt (particles, world text)");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    public static Bounds Bounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer) && r.enabled && r.gameObject.activeInHierarchy).ToArray();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
