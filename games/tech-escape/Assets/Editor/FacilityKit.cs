using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Tech Escape > Build facility kit: the realism pass's world, made the way Claude Bomb
// makes Bay 3 (Docs/claude-bomb-recipe.md). Every world material is URP Lit with a base
// map and a normal map, made here from the CC0 texture sets in Environment/PolyHaven and
// from the hero models' maps in Models/Hero; the results are prefabs in
// Resources/Facility that Building places from the level data.
//
// This file: the materials and the hero models (FacilityKit.Materials.cs), the
// prefabs (FacilityKit.Prefabs.cs), and a preview that renders every hero from four
// sides into Builds/look-audit/realism/models, to check scale, pivot and facing.
public static partial class FacilityKit
{
    public const string PolyHaven = "Assets/Environment/PolyHaven";
    public const string HeroDir = "Assets/Models/Hero";
    public const string Generated = "Assets/Environment/Generated";
    public const string Out = "Assets/Resources/Facility";

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

    static Texture2D Texture(string path, int maxSize, bool srgb, bool normalMap = false, TextureWrapMode wrap = TextureWrapMode.Repeat)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) return null;
        bool dirty = importer.textureType != (normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default)
                     || importer.sRGBTexture != srgb || importer.maxTextureSize != maxSize || importer.wrapMode != wrap
                     || importer.alphaIsTransparency || importer.mipmapEnabled == false || !importer.crunchedCompression;
        if (dirty)
        {
            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = maxSize;
            importer.wrapMode = wrap;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true; // crunched DXT: the web build's texture budget
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
        if (tiling.HasValue)
        {
            m.SetTextureScale("_BaseMap", tiling.Value);
        }
        m.enableInstancing = true;
        return Save(m, $"{Generated}/{name}.mat");
    }

    // A hero's material from its maps: <dir>/<file>_base.jpg, _normal.png, _mask.png, _emission.png.
    // detailSize: the normal and mask maps' size, smaller for dressing seen at a distance (the web budget).
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
    // the ARM split into URP's metallic-smoothness (R metal, A smoothness) and occlusion maps.
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
            var (ms, ao) = SplitArm($"{dir}/{id}_arm_1k.jpg", id, size);
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
            string name = Path.GetFileNameWithoutExtension(file);
            yield return name.Substring(asset.Length + 1, name.Length - asset.Length - 1 - "diff_1k".Length).TrimEnd('_');
        }
    }

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
        string em = maps.ContainsKey("emission") ? maps["emission"] : maps.ContainsKey("emissive") ? maps["emissive"] : null;
        var emission = em != null ? Texture(em, size, true) : null;
        string name = $"{asset}_{(glass ? "glass" : set == "" ? "main" : set)}";
        var m = Lit(name, diff, nor, ms, emission, Color.white, emission != null ? Color.white * 2.2f : Color.black, null, ms != null ? 1f : smooth);
        // Alpha from an opacity map (the security camera's grille), as a cut-out.
        if (maps.TryGetValue("opacity", out var op) && diff != null)
        {
            m.SetFloat("_AlphaClip", 1f); m.SetFloat("_Cutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON");
            m.renderQueue = (int)RenderQueue.AlphaTest;
        }
        // A lamp's glass without a colour map of its own (the hanging lamp): its alpha map as the base, glowing warm.
        if (glass && diff == null && maps.TryGetValue("alpha", out var alpha))
        {
            m.SetTexture("_BaseMap", Texture(alpha, size, true));
            m.SetColor("_BaseColor", new Color(1f, 0.92f, 0.78f));
            m.SetColor("_EmissionColor", new Color(1f, 0.82f, 0.6f) * 3f);
            m.EnableKeyword("_EMISSION");
            if (nor == null) { m.SetTexture("_BumpMap", Texture($"{folder}/{asset}_glass_nor_gl_1k.jpg", size, false, normalMap: true)); m.EnableKeyword("_NORMALMAP"); }
        }
        // The fluorescent tubes: the glass glows cool white.
        if (asset.Contains("fluorescent") && glass)
        {
            m.SetColor("_BaseColor", new Color(0.95f, 0.97f, 1f));
            m.SetColor("_EmissionColor", new Color(0.85f, 0.92f, 1f) * 2.4f);
            m.EnableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---- Preview: every hero from four sides ----

    [MenuItem("Tech Escape/Preview hero models")]
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

        foreach (var dir in Directory.GetDirectories(HeroDir))
        {
            var glb = Directory.GetFiles(dir, "*.glb").FirstOrDefault();
            if (glb == null) continue;
            string folder = Path.GetFileName(dir), file = Path.GetFileNameWithoutExtension(Directory.GetFiles(dir, "*_base.jpg")[0]).Replace("_base", "");
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(glb.Replace('\\', '/'));
            if (asset == null) { Debug.LogWarning($"[FacilityKit] {glb} did not import as a GameObject"); continue; }
            var go = Object.Instantiate(asset);
            var mat = HeroMaterial(folder, file, 1024, 2f);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray();
            var b = Bounds(go);
            Debug.Log($"[FacilityKit] {folder}: size {b.size.x:F3} x {b.size.y:F3} x {b.size.z:F3}, min {b.min}, centre {b.center}, " +
                      $"tris {go.GetComponentsInChildren<MeshFilter>().Sum(f => f.sharedMesh.triangles.Length / 3) + go.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(s => s.sharedMesh.triangles.Length / 3)}");
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
            File.WriteAllBytes(Path.Combine(outDir, folder + ".png"), sheet.EncodeToPNG());
            Object.DestroyImmediate(go);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[FacilityKit] previews: +Z, +X, -Z, -X views in " + outDir);
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // ---- Preview: the pilot room and the corridor, from the game's own cameras ----

    [MenuItem("Tech Escape/Preview pilot room")]
    public static void PreviewRoom()
    {
        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/look-audit/realism/kit"));
        Directory.CreateDirectory(outDir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bay = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/CorridorBay.prefab"));
        var bay2 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/CorridorBay.prefab"));
        bay2.transform.position = new Vector3(4.2f, 0, 0);
        var roomRoot = new GameObject("RoomRoot").transform;
        roomRoot.position = new Vector3(0, 0, -2.75f);
        roomRoot.rotation = Quaternion.LookRotation(Vector3.forward);
        var door = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Door.prefab"));
        door.transform.SetParent(roomRoot, false);
        door.transform.Find("Hinge").localRotation = Quaternion.Euler(0, 100, 0);
        var room = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Room_SuspiciousInbox.prefab"));
        room.transform.SetParent(roomRoot, false);
        var door2 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Door.prefab"));
        door2.transform.position = new Vector3(4.2f, 0, 2.75f); door2.transform.rotation = Quaternion.LookRotation(Vector3.back);
        var attacker = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Attacker.prefab"));
        attacker.transform.position = new Vector3(2.4f, 0, 0.3f); attacker.transform.rotation = Quaternion.LookRotation(Vector3.right);

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.16f, 0.2f, 0.3f);
        RenderSettings.ambientEquatorColor = new Color(0.11f, 0.13f, 0.19f);
        RenderSettings.ambientGroundColor = new Color(0.05f, 0.06f, 0.09f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = 0.06f;
        RenderSettings.fogColor = new Color(0.035f, 0.06f, 0.13f);
        var key = new GameObject("Key").AddComponent<Light>();
        key.type = LightType.Directional; key.shadows = LightShadows.Soft; key.shadowStrength = 0.85f;
        key.color = new Color(0.86f, 0.9f, 1f); key.intensity = 0.9f;
        key.transform.rotation = Quaternion.Euler(62f, 155f, 0f);

        var cam = new GameObject("Cam").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = RenderSettings.fogColor;
        cam.nearClipPlane = 0.05f; cam.farClipPlane = 200f; cam.allowHDR = true;
        var data = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(cam);
        data.renderPostProcessing = true; data.renderShadows = true;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var tone = profile.Add<UnityEngine.Rendering.Universal.Tonemapping>(true); tone.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.Neutral;
        var bloom = profile.Add<UnityEngine.Rendering.Universal.Bloom>(true); bloom.threshold.value = 1f; bloom.intensity.value = 0.25f; bloom.scatter.value = 0.5f; bloom.highQualityFiltering.value = true;
        var vig = profile.Add<UnityEngine.Rendering.Universal.Vignette>(true); vig.intensity.value = 0.2f;
        var vol = new GameObject("Volume").AddComponent<Volume>(); vol.isGlobal = true; vol.sharedProfile = profile;
        var rt = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        var camPoint = room.transform.Find("CamPoint");
        var shots = new List<(string name, Vector3 pos, Quaternion rot, float fov)>
        {
            ("room", camPoint.position, camPoint.rotation, 50f),
            ("corridor", new Vector3(-1.8f, 2.0f, 0.6f), Quaternion.LookRotation(new Vector3(4.6f, 1.0f, 0.3f) - new Vector3(-1.8f, 2.0f, 0.6f)), 52f),
            ("door", new Vector3(2.0f, 1.55f, 1.6f), Quaternion.LookRotation(new Vector3(0f, 1.3f, -2.75f) - new Vector3(2.0f, 1.55f, 1.6f)), 50f),
            ("attacker", new Vector3(4.6f, 1.45f, 1.3f), Quaternion.LookRotation(new Vector3(2.4f, 1.0f, 0.3f) - new Vector3(4.6f, 1.45f, 1.3f)), 45f),
        };
        foreach (var (name, pos, rot, fov) in shots)
        {
            cam.transform.SetPositionAndRotation(pos, rot);
            cam.fieldOfView = fov;
            cam.Render(); cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(outDir, name + ".png"), tex.EncodeToPNG());
        }
        // The hero's share of the room shot: its bounds projected through the room camera.
        cam.transform.SetPositionAndRotation(camPoint.position, camPoint.rotation); cam.fieldOfView = 50f;
        var hb = Bounds(room.transform.Find("Hero").gameObject);
        float minX = 1, maxX = 0, minY = 1, maxY = 0;
        for (int i = 0; i < 8; i++)
        {
            var c = hb.center + Vector3.Scale(hb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            var v = cam.WorldToViewportPoint(c);
            minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x); minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y);
        }
        Debug.Log($"[FacilityKit] room shot: hero box {Mathf.Clamp01(maxX) - Mathf.Clamp01(minX):P0} x {Mathf.Clamp01(maxY) - Mathf.Clamp01(minY):P0} of the frame = {(Mathf.Clamp01(maxX) - Mathf.Clamp01(minX)) * (Mathf.Clamp01(maxY) - Mathf.Clamp01(minY)):P0}, camera {Vector3.Distance(camPoint.position, hb.center):F2} m from it");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // Every hero prefab from the front, and every room from its own camera point, lit as the
    // game lights a room: a cheap check of facing and framing before a game build.
    [MenuItem("Tech Escape/Preview kit rooms")]
    public static void PreviewKit()
    {
        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/look-audit/realism/kit"));
        Directory.CreateDirectory(outDir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.118f, 0.173f, 0.275f);
        RenderSettings.ambientEquatorColor = new Color(0.086f, 0.122f, 0.2f);
        RenderSettings.ambientGroundColor = new Color(0.043f, 0.063f, 0.125f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = 0.05f;
        RenderSettings.fogColor = new Color(0.035f, 0.063f, 0.13f);
        var key = new GameObject("Key").AddComponent<Light>();
        key.type = LightType.Directional; key.shadows = LightShadows.Soft; key.shadowStrength = 0.9f;
        key.color = new Color(0.82f, 0.88f, 1f); key.intensity = 0.75f; key.transform.rotation = Quaternion.Euler(58f, 151f, 0f);
        var cam = new GameObject("Cam").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = RenderSettings.fogColor; cam.nearClipPlane = 0.1f; cam.allowHDR = true;
        var data = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(cam);
        data.renderPostProcessing = true;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.Add<UnityEngine.Rendering.Universal.Tonemapping>(true).mode.value = UnityEngine.Rendering.Universal.TonemappingMode.Neutral;
        var bloom = profile.Add<UnityEngine.Rendering.Universal.Bloom>(true); bloom.threshold.value = 1f; bloom.intensity.value = 0.25f;
        profile.Add<UnityEngine.Rendering.Universal.Vignette>(true).intensity.value = 0.2f;
        var vol = new GameObject("Volume").AddComponent<Volume>(); vol.isGlobal = true; vol.sharedProfile = profile;
        var rt = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        Texture2D Grab()
        {
            cam.Render(); cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); tex.Apply();
            RenderTexture.active = null;
            return tex;
        }
        // The rooms, each from its camera point.
        var sheet = new Texture2D(960 * 3, 540 * 4, TextureFormat.RGB24, false);
        int n = 0;
        foreach (var path in Directory.GetFiles(Out, "Room_*.prefab"))
        {
            var room = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/')));
            var cp = room.transform.Find("CamPoint");
            cam.transform.SetPositionAndRotation(cp.position, cp.rotation);
            var view = room.GetComponent<RoomView>();
            cam.fieldOfView = view != null ? view.fov : 48f;
            var tex = Grab();
            File.WriteAllBytes(Path.Combine(outDir, Path.GetFileNameWithoutExtension(path) + ".png"), tex.EncodeToPNG());
            if (n < 12) sheet.SetPixels((n % 3) * 960, (3 - n / 3) * 540, 960, 540, tex.GetPixels());
            n++;
            Object.DestroyImmediate(room);
        }
        sheet.Apply();
        File.WriteAllBytes(Path.Combine(outDir, "rooms.png"), sheet.EncodeToPNG());
        Debug.Log($"[FacilityKit] previewed {n} rooms into {outDir}");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // The realism pass's machine check on the kit: every material on every renderer of every
    // prefab in Resources/Facility is URP Lit with a base map and a normal map; none is Flat.
    [MenuItem("Tech Escape/Check facility kit materials")]
    public static void CheckMaterials()
    {
        int renderers = 0, bad = 0;
        var seen = new HashSet<Material>();
        foreach (var path in Directory.GetFiles(Out, "*.prefab", SearchOption.AllDirectories))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer || r.GetComponent<TMPro.TextMeshPro>() != null) continue; // effects and world text
                renderers++;
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) { bad++; Debug.LogWarning($"[Check] {path} {r.name}: no material"); continue; }
                    if (!seen.Add(m)) continue;
                    bool lit = m.shader != null && m.shader.name == "Universal Render Pipeline/Lit";
                    bool maps = m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null && m.HasProperty("_BumpMap") && m.GetTexture("_BumpMap") != null;
                    if (!lit || !maps) { bad++; Debug.LogWarning($"[Check] {m.name} on {Path.GetFileNameWithoutExtension(path)}/{r.name}: shader {m.shader?.name}, base {(m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap")?.name : "-")}, normal {(m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap")?.name : "-")}"); }
                }
            }
        }
        Debug.Log($"[Check] {renderers} renderers, {seen.Count} materials, {bad} failing (URP Lit with a base map and a normal map)");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    public static Bounds Bounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer)).ToArray();
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
