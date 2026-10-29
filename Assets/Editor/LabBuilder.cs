using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Claude Bomb > Build Lab: makes Bay 3, the room around the device, as
// Resources/Lab.prefab (GameUi adds it to every run; LabAtmosphere keeps it alive).
//
// 1. Materials for the Poly Haven models and textures in Environment/PolyHaven: URP Lit
//    materials from each asset's maps, the packed AO/rough/metal map split into URP's
//    metallic-smoothness and occlusion maps (Environment/Generated).
// 2. The room: floor, walls and ceiling in tiles (forward rendering lights each object
//    with at most four lights), the glass inspection table under the device, the props,
//    the lights, signs, dust in the lamp's beam, and a reflection probe.
//
// The device is measured, not assumed: the table top goes where the device's bottom is.
// Run it again after changing the layout here; open the prefab to adjust by hand.
public static class LabBuilder
{
    const string Source = "Assets/Environment/PolyHaven";
    const string Generated = "Assets/Environment/Generated";
    const string PrefabPath = "Assets/Resources/Lab.prefab";

    // Rendering layers: the device is on Default (1) and lit by the scene's directional
    // light; the room is on "Light Layer 1" (2) and lit only by its own lights.
    const uint Device = 1, Room = 2, Both = 3;

    const float HalfRoom = 4.2f, TableHeight = 0.76f, RoomHeight = 3.8f;
    static float floorY, ceilingY;

    [MenuItem("Claude Bomb/Build Lab")]
    public static void Build()
    {
        Directory.CreateDirectory(Generated);
        AssetDatabase.Refresh();
        foreach (var fbx in Directory.GetFiles(Source, "*.fbx", SearchOption.AllDirectories)) SetUpModel(fbx.Replace('\\', '/'));

        // The device sits at the scene's origin; measure it in the scene.
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        var bomb = GameObject.Find("Bomb");
        var bounds = RendererBounds(bomb);
        floorY = bounds.min.y - TableHeight;
        ceilingY = floorY + RoomHeight;

        var root = new GameObject("Lab");
        var atmosphere = root.AddComponent<LabAtmosphere>();
        BuildRoom(root.transform);
        BuildTable(root.transform, bounds);
        var lights = BuildLights(root.transform);
        BuildProps(root.transform, lights);
        BuildSigns(root.transform);

        var so = new SerializedObject(atmosphere);
        so.FindProperty("floorY").floatValue = floorY;
        so.FindProperty("keyLight").objectReferenceValue = lights.key;
        so.FindProperty("glow").objectReferenceValue = lights.glow;
        so.FindProperty("beacon").objectReferenceValue = lights.beacon;
        so.FindProperty("beaconHead").objectReferenceValue = lights.beaconHead;
        so.FindProperty("beaconDome").objectReferenceValue = lights.beaconDome;
        so.FindProperty("alarmLed").objectReferenceValue = lights.alarmLed;
        so.FindProperty("probe").objectReferenceValue = lights.probe;
        SetArray(so.FindProperty("tubes"), lights.tubes.Cast<Object>().ToList());
        SetArray(so.FindProperty("tubeGlass"), lights.tubeGlass.Cast<Object>().ToList());
        so.ApplyModifiedPropertiesWithoutUndo();

        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            if (r.renderingLayerMask == Device) r.renderingLayerMask = Room; // whatever wasn't set on purpose belongs to the room
        foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c); // clicks go to the device only

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LabBuilder] Built {PrefabPath}: floor at {floorY:F2}, device bottom at {bounds.min.y:F2}");
    }

    // ---- Materials for the Poly Haven assets ----

    static void SetUpModel(string fbx)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(fbx);
        importer.importCameras = importer.importLights = importer.importAnimation = false;
        importer.animationType = ModelImporterAnimationType.None;
        importer.addCollider = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        string folder = Path.GetDirectoryName(fbx).Replace('\\', '/'), asset = Path.GetFileName(folder);
        // The model's material slots: embedded on the first run, remapped after that.
        var slots = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Material>().Select(m => m.name)
            .Concat(importer.GetExternalObjectMap().Keys.Where(k => k.type == typeof(Material)).Select(k => k.name)).Distinct().ToList();
        foreach (var embedded in slots)
        {
            var material = MakeMaterial(folder, asset, embedded);
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), embedded), material);
        }
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
            if (map.Contains("_") && map != "nor_gl") continue; // another set's map
            maps[map] = file.Replace('\\', '/');
        }
        return maps;
    }

    static IEnumerable<string> Sets(string folder, string asset)
    {
        foreach (var file in Directory.GetFiles(folder, asset + "_*diff_1k.*"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string middle = name.Substring(asset.Length + 1, name.Length - asset.Length - 1 - "diff_1k".Length).TrimEnd('_');
            yield return middle;
        }
    }

    static Material MakeMaterial(string folder, string asset, string slot)
    {
        var sets = Sets(folder, asset).ToList();
        bool hasGlassMaps = Directory.GetFiles(folder, asset + "_glass_*").Length > 0;
        string set = sets.FirstOrDefault(s => s != "" && slot.ToLowerInvariant().Contains(s.ToLowerInvariant()))
                     ?? (slot.Contains("glass") && hasGlassMaps ? "glass" : sets.Contains("") ? "" : sets.FirstOrDefault() ?? "");
        bool tubeGlass = slot.Contains("fluorescent") && slot.EndsWith("glass");
        var maps = tubeGlass ? new Dictionary<string, string>() : Maps(folder, asset, set);
        string path = $"{Generated}/{asset}_{(slot.Contains("glass") ? "glass" : set == "" ? "main" : set)}.mat";

        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        int size = asset.Contains("floor") || asset.Contains("wall") ? 1024 : 512;
        if (maps.TryGetValue("diff", out var diff))
        {
            var opacity = maps.ContainsKey("alpha") ? maps["alpha"] : maps.ContainsKey("opacity") ? maps["opacity"] : null;
            m.SetTexture("_BaseMap", opacity != null ? WithAlpha(diff, opacity, size) : Texture(diff, size, srgb: true));
            if (opacity != null && slot.Contains("glass")) Transparent(m);
            else if (opacity != null) AlphaClip(m);
        }
        else if (slot.Contains("glass") && !tubeGlass)
        {
            m.SetColor("_BaseColor", new Color(0.9f, 0.92f, 0.9f, 0.35f));
            Transparent(m);
        }
        if (maps.TryGetValue("nor_gl", out var normal))
        {
            m.SetTexture("_BumpMap", Texture(normal, size, srgb: false, normalMap: true));
            m.EnableKeyword("_NORMALMAP");
        }
        if (maps.TryGetValue("arm", out var arm))
        {
            var (metal, occlusion) = SplitArm(arm, size);
            m.SetTexture("_MetallicGlossMap", metal);
            m.EnableKeyword("_METALLICSPECGLOSSMAP");
            m.SetFloat("_Smoothness", 1f);
            m.SetTexture("_OcclusionMap", occlusion);
            m.EnableKeyword("_OCCLUSIONMAP");
        }
        string emission = maps.ContainsKey("emissive") ? maps["emissive"] : maps.ContainsKey("emission") ? maps["emission"] : null;
        if (emission != null) Glow(m, Color.white * 2.5f, Texture(emission, size, srgb: true));
        if (tubeGlass)
        {
            m.SetColor("_BaseColor", new Color(0.95f, 0.97f, 1f));
            Glow(m, new Color(0.85f, 0.92f, 1f) * 2.2f, null);
        }
        return Save(m, path);
    }

    static Material Save(Material m, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing == null) { AssetDatabase.CreateAsset(m, path); return m; }
        existing.CopyPropertiesFromMaterial(m);
        existing.shaderKeywords = m.shaderKeywords;
        existing.renderQueue = m.renderQueue;
        EditorUtility.SetDirty(existing);
        return existing;
    }

    static void Transparent(Material m)
    {
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent;
    }

    static void AlphaClip(Material m)
    {
        m.SetFloat("_AlphaClip", 1f);
        m.SetFloat("_Cutoff", 0.5f);
        m.EnableKeyword("_ALPHATEST_ON");
        m.renderQueue = (int)RenderQueue.AlphaTest;
    }

    static void Glow(Material m, Color color, Texture map)
    {
        if (map != null) m.SetTexture("_EmissionMap", map);
        m.SetColor("_EmissionColor", color);
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
    }

    static Texture2D Texture(string path, int maxSize, bool srgb, bool normalMap = false)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
        importer.sRGBTexture = srgb;
        importer.maxTextureSize = maxSize;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Texture2D Read(string path)
    {
        var tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(path));
        return tex;
    }

    // Poly Haven packs AO, roughness and metalness in R, G and B. URP wants metalness in R
    // with smoothness in alpha, and occlusion on its own.
    static (Texture2D metal, Texture2D occlusion) SplitArm(string armPath, int size)
    {
        var arm = Read(armPath);
        var src = arm.GetPixels32();
        var ms = new Color32[src.Length];
        var ao = new Color32[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            ms[i] = new Color32(src[i].b, src[i].b, src[i].b, (byte)(255 - src[i].g));
            ao[i] = new Color32(src[i].r, src[i].r, src[i].r, 255);
        }
        string name = Path.GetFileNameWithoutExtension(armPath).Replace("_arm_1k", "");
        return (WritePng($"{Generated}/{name}_metal_smooth.png", arm.width, arm.height, ms, size, srgb: false),
                WritePng($"{Generated}/{name}_occlusion.png", arm.width, arm.height, ao, size, srgb: false));
    }

    // The diffuse map with an opacity map in its alpha.
    static Texture2D WithAlpha(string diffPath, string alphaPath, int size)
    {
        var diff = Read(diffPath);
        var alpha = Read(alphaPath);
        var a = alpha.width == diff.width ? alpha.GetPixels32() : null;
        var px = diff.GetPixels32();
        for (int i = 0; i < px.Length; i++) px[i].a = a != null ? a[i].r : (byte)255;
        string name = Path.GetFileNameWithoutExtension(diffPath).Replace("_diff_1k", "");
        return WritePng($"{Generated}/{name}_base_alpha.png", diff.width, diff.height, px, size, srgb: true);
    }

    static Texture2D WritePng(string path, int w, int h, Color32[] pixels, int maxSize, bool srgb, TextureWrapMode wrap = TextureWrapMode.Repeat)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.sRGBTexture = srgb;
        importer.maxTextureSize = maxSize;
        importer.alphaIsTransparency = srgb;
        importer.wrapMode = wrap;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // ---- Plain materials ----

    static Material Lit(string name, Color color, float metallic = 0f, float smoothness = 0.4f, Texture2D map = null, Vector2? tiling = null)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smoothness);
        if (map != null) m.SetTexture("_BaseMap", map);
        if (tiling.HasValue) m.SetTextureScale("_BaseMap", tiling.Value);
        return Save(m, $"{Generated}/{name}.mat");
    }

    static Material Emissive(string name, Color color, float strength)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", color);
        Glow(m, color * strength, null);
        return Save(m, $"{Generated}/{name}.mat");
    }

    // A texture set from Poly Haven as a material for room surfaces, tinted.
    static Material Surface(string asset, Color tint)
    {
        var m = MakeMaterial($"{Source}/{asset}", asset, asset);
        m.SetColor("_BaseColor", tint);
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---- The room ----

    static void BuildRoom(Transform root)
    {
        var room = Child("Room", root);
        var floor = Surface("concrete_floor_worn_001", new Color(0.5f, 0.51f, 0.54f));
        var wall = Surface("plastered_wall_04", new Color(0.4f, 0.45f, 0.54f));
        var ceiling = Lit("Ceiling", new Color(0.16f, 0.17f, 0.2f), 0f, 0.1f);
        var trim = Lit("Trim", new Color(0.08f, 0.1f, 0.14f), 0.3f, 0.35f);
        var pipe = Lit("Pipe", new Color(0.42f, 0.45f, 0.5f), 0.85f, 0.55f);

        const int n = 4;
        float tile = 2f * HalfRoom / n, mid = (floorY + ceilingY) / 2f;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                float x = -HalfRoom + tile * (i + 0.5f), z = -HalfRoom + tile * (j + 0.5f);
                Quad("Floor", room, floor, new Vector3(x, floorY, z), new Vector3(90f, 0f, 0f), new Vector2(tile, tile));
                Quad("Ceiling", room, ceiling, new Vector3(x, ceilingY, z), new Vector3(-90f, 0f, 0f), new Vector2(tile, tile));
            }
        // Walls in tiles too, facing in; and a dark skirting along the bottom.
        var sides = new[] { (Vector3.forward, 0f), (Vector3.back, 180f), (Vector3.left, -90f), (Vector3.right, 90f) };
        foreach (var (side, yaw) in sides)
        {
            var along = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            for (int i = 0; i < n; i++)
                for (int k = 0; k < 2; k++)
                {
                    float a = -HalfRoom + tile * (i + 0.5f), h = RoomHeight / 2f;
                    var p = side * HalfRoom + along * a + Vector3.up * (floorY + h * (k + 0.5f));
                    Quad("Wall", room, wall, p, new Vector3(0f, yaw, 0f), new Vector2(tile, h));
                }
            Box("Skirting", room, trim, side * (HalfRoom - 0.02f) + Vector3.up * (floorY + 0.06f), new Vector3(0f, yaw, 0f), new Vector3(2f * HalfRoom, 0.12f, 0.04f));
        }
        // Pipes along the top of the back wall, and one down the corner.
        Cylinder("Pipe", room, pipe, new Vector3(0f, ceilingY - 0.35f, HalfRoom - 0.16f), new Vector3(0f, 0f, 90f), 0.07f, 2f * HalfRoom);
        Cylinder("Pipe", room, pipe, new Vector3(0f, ceilingY - 0.55f, HalfRoom - 0.12f), new Vector3(0f, 0f, 90f), 0.045f, 2f * HalfRoom);
        Cylinder("Pipe", room, pipe, new Vector3(-HalfRoom + 0.3f, mid, HalfRoom - 0.16f), Vector3.zero, 0.07f, RoomHeight);
        Cylinder("Pipe", room, pipe, new Vector3(HalfRoom - 0.16f, ceilingY - 0.4f, 0f), new Vector3(90f, 0f, 0f), 0.06f, 2f * HalfRoom);

        // The bomb squad's cordon: hazard tape on the floor around the table.
        var tape = Lit("HazardTape", Color.white, 0f, 0.3f, Stripes(), new Vector2(1f, 1f));
        const float r = 1.55f, w = 0.09f;
        foreach (var (pos, rot) in new[] { (new Vector3(0f, 0f, r), 0f), (new Vector3(0f, 0f, -r), 0f), (new Vector3(r, 0f, 0f), 90f), (new Vector3(-r, 0f, 0f), 90f) })
        {
            var q = Quad("Tape", room, tape, pos + Vector3.up * (floorY + 0.004f), new Vector3(90f, rot, 0f), new Vector2(2f * r + w, w));
            q.GetComponent<MeshRenderer>().sharedMaterial = TiledCopy(tape, "HazardTapeRun", new Vector2((2f * r + w) / 0.5f, 1f));
        }
    }

    static Material TiledCopy(Material m, string name, Vector2 tiling)
    {
        var copy = new Material(m);
        copy.SetTextureScale("_BaseMap", tiling);
        return Save(copy, $"{Generated}/{name}.mat");
    }

    // ---- The inspection table: steel frame, glass top, lit from below ----

    static void BuildTable(Transform root, Bounds device)
    {
        var table = Child("InspectionTable", root);
        var steel = Lit("BrushedSteel", new Color(0.62f, 0.65f, 0.7f), 0.9f, 0.62f);
        var glass = Lit("TableGlass", new Color(0.55f, 0.78f, 0.82f, 0.07f), 0f, 0.92f);
        Transparent(glass);
        EditorUtility.SetDirty(glass);
        var led = Emissive("TableLed", new Color(0.14f, 0.88f, 0.75f), 2f); // Maaloomatiia teal

        float top = device.min.y, w = 1.4f, d = 0.78f, frame = 0.035f;
        var glassTop = Box("Glass", table, glass, new Vector3(0f, top - 0.006f, 0f), Vector3.zero, new Vector3(w, 0.012f, d));
        glassTop.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        glassTop.GetComponent<MeshRenderer>().renderingLayerMask = Room; // lit by the device's light too, it turns milky
        foreach (var (pos, size) in new[]
        {
            (new Vector3(0f, 0f, d / 2f), new Vector3(w + frame, frame, frame)), (new Vector3(0f, 0f, -d / 2f), new Vector3(w + frame, frame, frame)),
            (new Vector3(w / 2f, 0f, 0f), new Vector3(frame, frame, d)), (new Vector3(-w / 2f, 0f, 0f), new Vector3(frame, frame, d)),
        })
        {
            Box("Frame", table, steel, pos + Vector3.up * (top - 0.03f), Vector3.zero, size).GetComponent<MeshRenderer>().renderingLayerMask = Both;
            Box("Led", table, led, pos * 0.985f + Vector3.up * (top - 0.052f), Vector3.zero, new Vector3(Mathf.Max(0.008f, size.x - 0.06f), 0.008f, Mathf.Max(0.008f, size.z - 0.06f)));
        }
        float legHeight = top - 0.045f - floorY;
        foreach (var (x, z) in new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) })
        {
            Box("Leg", table, steel, new Vector3(x * (w / 2f - 0.02f), floorY + legHeight / 2f, z * (d / 2f - 0.02f)), Vector3.zero, new Vector3(0.035f, legHeight, 0.035f));
            Box("Foot", table, steel, new Vector3(x * (w / 2f - 0.02f), floorY + 0.01f, z * (d / 2f - 0.02f)), Vector3.zero, new Vector3(0.07f, 0.02f, 0.07f));
        }
    }

    // ---- Lights ----

    class Lights
    {
        public Light key, glow, beacon;
        public Transform beaconHead;
        public Renderer beaconDome, alarmLed;
        public ReflectionProbe probe;
        public List<Light> tubes = new List<Light>();
        public List<Renderer> tubeGlass = new List<Renderer>();
    }

    static Lights BuildLights(Transform root)
    {
        var l = new Lights();
        var group = Child("Lights", root);

        // The hanging lamp over the table: the room's key light. It also lights the table,
        // and the device casts its shadow through the glass onto the floor.
        l.key = NewLight("LampLight", group, LightType.Spot, new Vector3(0f, ceilingY - 1.42f, 0f), new Vector3(90f, 0f, 0f),
                         new Color(1f, 0.9f, 0.76f), 12f, 6f, Room);
        l.key.spotAngle = 72f;
        l.key.innerSpotAngle = 30f;
        l.key.shadows = LightShadows.Soft;
        l.key.shadowStrength = 0.85f;
        var keyData = l.key.GetUniversalAdditionalLightData();
        keyData.customShadowLayers = true;
        keyData.shadowRenderingLayers = Both;

        // Under the glass, the table's own teal light: a soft glow on the device from below,
        // never so strong that the stamp underneath gets hard to read.
        l.glow = NewLight("TableGlow", group, LightType.Point, new Vector3(0f, floorY + 0.2f, 0f), Vector3.zero,
                          new Color(0.14f, 0.88f, 0.75f), 0.45f, 1.3f, Both);

        // A light cone and dust in the lamp's beam.
        var cone = Child("Beam", group);
        cone.localPosition = new Vector3(0f, ceilingY - 1.4f, 0f);
        cone.gameObject.AddComponent<MeshFilter>().sharedMesh = BeamMesh(0.2f, 1.7f, ceilingY - 1.4f - floorY);
        var beam = cone.gameObject.AddComponent<MeshRenderer>();
        beam.sharedMaterial = Additive("Beam", null, new Color(1f, 0.88f, 0.7f, 1f));
        beam.shadowCastingMode = ShadowCastingMode.Off;
        Dust(group, (ceilingY - 1.4f + floorY) / 2f);

        // The reflection probe: captured once the room is lit (LabAtmosphere).
        var probeGo = Child("Reflections", group);
        probeGo.localPosition = new Vector3(0f, 0.9f, 0f);
        l.probe = probeGo.gameObject.AddComponent<ReflectionProbe>();
        l.probe.mode = ReflectionProbeMode.Realtime;
        l.probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        l.probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
        l.probe.resolution = 128;
        l.probe.size = new Vector3(2f * HalfRoom, RoomHeight, 2f * HalfRoom);
        l.probe.center = new Vector3(0f, (floorY + ceilingY) / 2f - 0.9f, 0f);
        l.probe.intensity = 1.6f;
        l.probe.cullingMask = ~((1 << 31) | (1 << 5));
        return l;
    }

    static Light NewLight(string name, Transform parent, LightType type, Vector3 pos, Vector3 euler, Color color, float intensity, float range, uint layers)
    {
        var t = Child(name, parent);
        t.localPosition = pos;
        t.localEulerAngles = euler;
        var light = t.gameObject.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        light.renderMode = LightRenderMode.ForcePixel;
        light.GetUniversalAdditionalLightData().renderingLayers = layers;
        return light;
    }

    static Material Additive(string name, Texture2D map, Color color)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        m.SetColor("_BaseColor", color);
        if (map != null) m.SetTexture("_BaseMap", map);
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 2f); // additive
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.One);
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", (float)CullMode.Back); // from inside the beam, it disappears
        m.SetOverrideTag("RenderType", "Transparent");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent;
        return Save(m, $"{Generated}/{name}.mat");
    }

    // An open cone, brightest at the lamp: a faint beam of light in the dusty air.
    static Mesh BeamMesh(float topRadius, float bottomRadius, float height)
    {
        const int sides = 48;
        var verts = new List<Vector3>();
        var colors = new List<Color>();
        var tris = new List<int>();
        for (int i = 0; i <= sides; i++)
        {
            float a = i * Mathf.PI * 2f / sides;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            verts.Add(dir * topRadius);
            colors.Add(new Color(1f, 1f, 1f, 0.06f));
            verts.Add(dir * bottomRadius + Vector3.down * height);
            colors.Add(new Color(1f, 1f, 1f, 0f));
            if (i == sides) continue;
            int v = i * 2;
            tris.AddRange(new[] { v, v + 2, v + 1, v + 1, v + 2, v + 3 });
        }
        var mesh = new Mesh { name = "Beam" };
        mesh.SetVertices(verts);
        mesh.SetColors(colors);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        string path = $"{Generated}/Beam.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    static void Dust(Transform parent, float centreY)
    {
        var go = Child("Dust", parent);
        go.localPosition = new Vector3(0f, centreY, 0f);
        var ps = go.gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = 10f;
        main.startSpeed = 0.01f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.005f, 0.014f);
        main.startColor = new Color(1f, 0.94f, 0.82f, 0.55f);
        main.maxParticles = 160;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;
        var emission = ps.emission;
        emission.rateOverTime = 16f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(2f, ceilingY - 1.4f - floorY, 2f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.025f;
        noise.frequency = 0.25f;
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                         new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = Additive("Dust", Dot(), Color.white);
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    // ---- Props ----

    static void BuildProps(Transform root, Lights l)
    {
        var props = Child("Props", root);
        float f = floorY, c = ceilingY, back = HalfRoom - 0.02f;

        // Overhead: the lamp over the table, and three fluorescent fittings.
        Prop("hanging_industrial_lamp", props, new Vector3(0f, c, 0f), 0f, onFloor: false);
        foreach (var (x, z, flicker) in new[] { (-2.3f, 1.8f, true), (2.3f, 1.8f, false), (0f, -2.6f, false) })
        {
            var fitting = Prop("mounted_fluorescent_lights", props, new Vector3(x, c, z), 90f, onFloor: false);
            var tube = NewLight("TubeLight", l.key.transform.parent, LightType.Point, new Vector3(x, c - 0.35f, z), Vector3.zero,
                                new Color(0.78f, 0.87f, 1f), 1.0f, 5f, Room);
            var glass = fitting.GetComponentsInChildren<Renderer>().FirstOrDefault(r => r.sharedMaterials.Any(m => m != null && m.name.Contains("glass")));
            if (flicker) { l.tubes.Insert(0, tube); if (glass != null) l.tubeGlass.Insert(0, glass); }
            else { l.tubes.Add(tube); if (glass != null) l.tubeGlass.Add(glass); }
        }

        // The back wall: the tool chest, the extinguisher and the fire alarm.
        Prop("metal_tool_chest", props, new Vector3(-2.7f, f, back - 0.25f), 180f);
        Prop("korean_fire_extinguisher_01", props, new Vector3(1.35f, f, back - 0.2f), 180f);
        var alarm = Prop("fire_alarm", props, new Vector3(1.35f, 0.55f, back - 0.02f), 180f, onFloor: false);
        var led = Sphere("AlarmLed", props, Emissive("AlarmLed", new Color(1f, 0.1f, 0.06f), 4f), alarm.transform.position + new Vector3(0f, 0.11f, -0.02f), 0.025f);
        l.alarmLed = led.GetComponent<Renderer>();

        // The left wall: the lab bench with the microscope, the first aid box and a clipboard.
        Prop("metal_office_desk", props, new Vector3(-HalfRoom + 0.55f, f, 0.8f), 90f);
        float deskTop = f + 0.79f;
        Prop("industrial_microscope", props, new Vector3(-HalfRoom + 0.5f, deskTop, 0.15f), 120f);
        Prop("medical_box", props, new Vector3(-HalfRoom + 0.55f, deskTop, 1.4f), 80f);
        Prop("clipboard", props, new Vector3(-HalfRoom + 0.75f, deskTop, 0.75f), 75f);

        // The right wall: the tool cart, boxes, and an explosives barrel in the corner.
        Prop("industrial_storage_cart", props, new Vector3(HalfRoom - 0.6f, f, 0.7f), -90f);
        Prop("metal_toolbox", props, new Vector3(HalfRoom - 0.75f, f, -0.55f), -70f);
        Prop("cardboard_box_01", props, new Vector3(HalfRoom - 0.4f, f, -2.6f), 10f);
        Prop("cardboard_box_01", props, new Vector3(HalfRoom - 0.42f, f + 0.33f, -2.55f), 32f);
        Prop("cardboard_box_01", props, new Vector3(HalfRoom - 0.95f, f, -3.3f), -20f);
        Prop("Barrel_01", props, new Vector3(HalfRoom - 0.55f, f, HalfRoom - 0.6f), 30f);

        // The bomb squad's gear crate with their beacon on it; the cleaner's wet floor sign.
        var crate = Prop("old_military_crate", props, new Vector3(-1.95f, f, 2.15f), 18f);
        float crateTop = RendererBounds(crate).max.y;
        BuildBeacon(props, l, new Vector3(-1.75f, crateTop, 2.0f));
        Prop("WetFloorSign_01", props, new Vector3(-1.2f, f, -2.3f), 35f);
    }

    static void BuildBeacon(Transform parent, Lights l, Vector3 at)
    {
        var black = Lit("BeaconBase", new Color(0.04f, 0.04f, 0.05f), 0.2f, 0.4f);
        Cylinder("BeaconBase", parent, black, at + Vector3.up * 0.03f, Vector3.zero, 0.07f, 0.06f);
        var dome = Sphere("BeaconDome", parent, Emissive("BeaconDome", new Color(1f, 0.42f, 0.08f), 3f), at + Vector3.up * 0.1f, 0.12f);
        l.beaconDome = dome.GetComponent<Renderer>();
        var head = Child("BeaconHead", parent);
        head.position = at + Vector3.up * 0.1f;
        l.beaconHead = head;
        l.beacon = NewLight("BeaconLight", head, LightType.Spot, Vector3.zero, new Vector3(8f, 0f, 0f), new Color(1f, 0.42f, 0.08f), 7f, 9f, Room);
        l.beacon.spotAngle = 44f;
        l.beacon.innerSpotAngle = 10f;
        var back = NewLight("BeaconLight", head, LightType.Spot, Vector3.zero, new Vector3(8f, 180f, 0f), new Color(1f, 0.42f, 0.08f), 7f, 9f, Room);
        back.spotAngle = 44f;
        back.innerSpotAngle = 10f;
    }

    // A Poly Haven model, standing on the floor (or at pos, hanging) and turned by yaw.
    static GameObject Prop(string asset, Transform parent, Vector3 pos, float yaw, bool onFloor = true)
    {
        var path = Directory.GetFiles($"{Source}/{asset}", "*.fbx")[0].Replace('\\', '/');
        var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        model.transform.SetParent(parent, false);
        model.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * model.transform.rotation;
        model.transform.position = pos;
        if (onFloor)
        {
            var b = RendererBounds(model);
            model.transform.position += Vector3.up * (pos.y - b.min.y);
        }
        foreach (var r in model.GetComponentsInChildren<Renderer>()) r.renderingLayerMask = Room;
        return model;
    }

    // ---- Signs ----

    static void BuildSigns(Transform root)
    {
        var signs = Child("Signs", root);
        var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Inter/Inter Bold SDF.asset");
        var regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Inter/Inter SDF.asset");
        float back = HalfRoom - 0.015f;

        // Stencilled on the back wall, in worn paint.
        Text("Bay", signs, bold, "BAY 3", new Vector3(-0.9f, 1.35f, back), Quaternion.identity, 6.5f, new Color(0.5f, 0.56f, 0.63f, 0.35f), new Vector2(3f, 1f));
        Text("Use", signs, regular, "HAZARD CONTAINMENT  /  AUTHORISED STAFF ONLY", new Vector3(-0.9f, 0.78f, back), Quaternion.identity, 1f,
             new Color(0.5f, 0.56f, 0.63f, 0.35f), new Vector2(4f, 0.3f));

        // The door, and the exit sign above it.
        var door = Lit("Door", new Color(0.09f, 0.15f, 0.25f), 0.45f, 0.45f);
        var frame = Lit("DoorFrame", new Color(0.06f, 0.07f, 0.09f), 0.6f, 0.4f);
        float dx = 2.55f, f = floorY;
        Box("Door", signs, door, new Vector3(dx, f + 1.05f, back - 0.03f), Vector3.zero, new Vector3(1.0f, 2.1f, 0.05f));
        Box("DoorFrame", signs, frame, new Vector3(dx - 0.55f, f + 1.1f, back - 0.04f), Vector3.zero, new Vector3(0.1f, 2.2f, 0.08f));
        Box("DoorFrame", signs, frame, new Vector3(dx + 0.55f, f + 1.1f, back - 0.04f), Vector3.zero, new Vector3(0.1f, 2.2f, 0.08f));
        Box("DoorFrame", signs, frame, new Vector3(dx, f + 2.15f, back - 0.04f), Vector3.zero, new Vector3(1.2f, 0.1f, 0.08f));
        Box("PushBar", signs, frame, new Vector3(dx, f + 1.0f, back - 0.08f), Vector3.zero, new Vector3(0.8f, 0.05f, 0.04f));
        Box("DoorWindow", signs, Lit("DoorWindow", new Color(0.02f, 0.03f, 0.05f), 0f, 0.95f), new Vector3(dx, f + 1.6f, back - 0.06f), Vector3.zero, new Vector3(0.3f, 0.4f, 0.01f));
        var exit = Box("ExitSign", signs, Emissive("ExitSign", new Color(0.1f, 0.75f, 0.35f), 2.5f), new Vector3(dx, f + 2.42f, back - 0.05f), Vector3.zero, new Vector3(0.5f, 0.17f, 0.06f));
        Text("Exit", signs, bold, "EXIT", exit.transform.position + new Vector3(0f, 0f, -0.032f), Quaternion.identity, 1.1f, Color.white, new Vector2(0.5f, 0.17f));
        NewLight("ExitLight", signs, LightType.Point, exit.transform.position + new Vector3(0f, -0.1f, -0.25f), Vector3.zero, new Color(0.1f, 0.85f, 0.4f), 0.9f, 2f, Room);

        // A safety poster on the left wall, from the academy that runs the training.
        var posterAt = new Vector3(-HalfRoom + 0.015f, 0.55f, -1.6f);
        var facing = Quaternion.Euler(0f, -90f, 0f);
        var paper = Lit("Poster", new Color(0.93f, 0.94f, 0.92f), 0f, 0.15f);
        Quad("Poster", signs, paper, posterAt, facing.eulerAngles, new Vector2(0.62f, 0.86f));
        var logo = Lit("PosterLogo", Color.white, 0f, 0.15f, AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Brand/logo-light.png"));
        AlphaClip(logo);
        EditorUtility.SetDirty(logo);
        Quad("PosterLogo", signs, logo, posterAt + facing * new Vector3(0f, 0.27f, -0.003f), facing.eulerAngles, new Vector2(0.44f, 0.44f / 7.035f));
        Text("PosterTitle", signs, bold, "Stop. Look. Describe.", posterAt + facing * new Vector3(0f, 0.05f, -0.004f), facing, 0.55f, new Color(0.04f, 0.08f, 0.21f), new Vector2(0.5f, 0.25f));
        Text("PosterText", signs, regular, "Hazard awareness training\nMaaloomatiia Academy", posterAt + facing * new Vector3(0f, -0.22f, -0.004f), facing, 0.3f, new Color(0.36f, 0.42f, 0.47f), new Vector2(0.5f, 0.2f));
    }

    static TextMeshPro Text(string name, Transform parent, TMP_FontAsset font, string text, Vector3 pos, Quaternion rot, float size, Color color, Vector2 box)
    {
        var t = Child(name, parent);
        t.position = pos;
        t.rotation = rot;
        var tmp = t.gameObject.AddComponent<TextMeshPro>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.rectTransform.sizeDelta = box;
        return tmp;
    }

    // ---- Generated textures ----

    static Texture2D Stripes()
    {
        const int w = 128, h = 32;
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = Mathf.Repeat((x + y) / 32f, 1f) < 0.5f ? new Color32(236, 186, 24, 255) : new Color32(22, 22, 24, 255);
        return WritePng($"{Generated}/hazard_stripes.png", w, h, px, 128, srgb: true);
    }

    static Texture2D Dot()
    {
        const int s = 32;
        var px = new Color32[s * s];
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = new Vector2(x + 0.5f - s / 2f, y + 0.5f - s / 2f).magnitude / (s / 2f);
                px[y * s + x] = new Color32(255, 255, 255, (byte)(Mathf.Pow(Mathf.Clamp01(1f - d), 2f) * 255));
            }
        return WritePng($"{Generated}/dust.png", s, s, px, 32, srgb: true, TextureWrapMode.Clamp);
    }

    // ---- Small builders ----

    static Transform Child(string name, Transform parent)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material m)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.GetComponent<MeshRenderer>().sharedMaterial = m;
        go.GetComponent<MeshRenderer>().renderingLayerMask = Room;
        return go;
    }

    static GameObject Quad(string name, Transform parent, Material m, Vector3 pos, Vector3 euler, Vector2 size)
    {
        var go = Primitive(PrimitiveType.Quad, name, parent, m);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        return go;
    }

    static GameObject Box(string name, Transform parent, Material m, Vector3 pos, Vector3 euler, Vector3 size)
    {
        var go = Primitive(PrimitiveType.Cube, name, parent, m);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = size;
        return go;
    }

    static GameObject Cylinder(string name, Transform parent, Material m, Vector3 pos, Vector3 euler, float radius, float length)
    {
        var go = Primitive(PrimitiveType.Cylinder, name, parent, m);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = new Vector3(radius * 2f, length / 2f, radius * 2f);
        return go;
    }

    static GameObject Sphere(string name, Transform parent, Material m, Vector3 pos, float diameter)
    {
        var go = Primitive(PrimitiveType.Sphere, name, parent, m);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * diameter;
        return go;
    }

    static Bounds RendererBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer)).ToArray();
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    static void SetArray(SerializedProperty array, List<Object> items)
    {
        array.arraySize = items.Count;
        for (int i = 0; i < items.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
    }
}
