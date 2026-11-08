using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// The finished, painted LUMA, as drawn in the brand file Looma-Final.ai (navy body and
// head, teal glasses rings, shoulder collars and wrist cuffs, mint eyes and smile). The
// model in Assets/Models/LUMA was made with Higgsfield (GPT Image 2.6 turned the
// drawing's arms-up pose into a 3D render, Meshy 7 turned that into a textured mesh);
// see Assets/Models/LUMA/README.md.
//
// Claude Bomb > Install LUMA Model puts it on the scene's LUMA. The mesh is baked into
// the frame LumaRig was set up for (Z up, face -Y, the first model's size), so the
// scene's transform, LUMA's camera and the rig's moods all stay as they were; the
// material is URP Lit with the model's maps plus a glow on the eyes and smile; the
// rig's shoulders move to her collars, and her glasses are marked in the vertex
// colours so the rig keeps them on the head while the cuffs beside them follow the arms.
// Claude Bomb > Render LUMA Portrait renders the 2D LUMA the other three games show
// (their Resources/Luma/luma.png) from the same model.
public static class LumaModelTools
{
    const string Dir = "Assets/Models/LUMA";
    const string Glb = Dir + "/LUMA.glb";
    const string MeshPath = Dir + "/LUMA.asset";
    const string MatPath = Dir + "/LUMA.mat";
    const string Tex = Dir + "/Textures/";
    const string ReferenceMesh = "Assets/Models/LUMA_25k.asset"; // the first model: its size and frame
    // Her teal shoulder collars, measured on LUMA.glb (fractions, as LumaRig reads them).
    static readonly Vector3 Shoulder = new Vector3(0.17f, 0.37f, 0.057f);
    // How bright the glowing parts are: the map holds the colour (mint eyes and smile
    // in full, the teal at about half), this scales it. LUMA's camera does no tone
    // mapping, so above about 1 the eyes clip to white instead of mint.
    const float Glow = 0.75f;

    [MenuItem("Claude Bomb/Install LUMA Model")]
    public static void Install()
    {
        ConfigureTextures();
        var mesh = BakeMesh();
        var mat = BuildMaterial();
        ApplyToScene(mesh, mat);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LUMA] Installed: {mesh.vertexCount} vertices, {mesh.triangles.Length / 3} triangles");
    }

    // Batch entry: install, then quit.
    public static void InstallBatch()
    {
        Install();
        EditorApplication.Exit(0);
    }

    static void ConfigureTextures()
    {
        Configure("LUMA_BaseColor.png", true, 2048, TextureImporterType.Default);
        Configure("LUMA_Emission.png", true, 512, TextureImporterType.Default);
        Configure("LUMA_Normal.png", false, 512, TextureImporterType.NormalMap);
        Configure("LUMA_MetalSmooth.png", false, 512, TextureImporterType.Default);
    }

    static void Configure(string file, bool srgb, int size, TextureImporterType type)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(Tex + file);
        imp.textureType = type;
        imp.sRGBTexture = srgb;
        imp.maxTextureSize = size;
        imp.mipmapEnabled = true;
        imp.alphaSource = file.Contains("MetalSmooth") ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
        imp.textureCompression = TextureImporterCompression.CompressedHQ;
        imp.SaveAndReimport();
    }

    static Mesh BakeMesh()
    {
        var src = AssetDatabase.LoadAllAssetsAtPath(Glb).OfType<Mesh>().First();
        var reference = AssetDatabase.LoadAssetAtPath<Mesh>(ReferenceMesh);

        var tris = src.triangles;

        // glTFast hands the model over Y up, facing +Z; LumaRig's LUMA is Z up, facing -Y.
        // (x, y, z) -> (x, -z, y) is a 90 degree turn about X, so the winding stays.
        Vector3 Turn(Vector3 p) => new Vector3(p.x, -p.z, p.y);
        var verts = src.vertices.Select(Turn).ToArray();
        var normals = src.normals.Select(Turn).ToArray();
        var b = GeometryUtility.CalculateBounds(verts, Matrix4x4.identity);
        // Same height and centre as the first model, so the camera framing holds.
        float scale = reference.bounds.size.z / b.size.z;
        for (int i = 0; i < verts.Length; i++)
            verts[i] = reference.bounds.center + (verts[i] - b.center) * scale;

        var mesh = new Mesh { name = "LUMA" };
        mesh.indexFormat = verts.Length > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.uv = src.uv;
        mesh.triangles = tris;
        mesh.colors32 = MarkGlasses(src.vertices, src.uv, tris);
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
        if (existing == null) { AssetDatabase.CreateAsset(mesh, MeshPath); return mesh; }
        EditorUtility.CopySerialized(mesh, existing); // keeps the asset's id, so the scene's link holds
        EditorUtility.SetDirty(existing);
        return existing;
    }

    // Her glasses, in red; everything else black. The teal parts (glasses rings, shoulder
    // collars, wrist cuffs) are found as connected regions of teal on the surface, read
    // from the colour map at each vertex; the glasses are the regions high up and in the
    // middle. URP Lit ignores vertex colours, so this only tells LumaRig what is what.
    static Color32[] MarkGlasses(Vector3[] up, Vector2[] uv, int[] tris)
    {
        var png = new Texture2D(2, 2);
        png.LoadImage(File.ReadAllBytes(Tex + "LUMA_BaseColor.png"));
        var px = png.GetPixels32();
        int w = png.width, h = png.height;
        Object.DestroyImmediate(png);
        bool IsTeal(Vector2 t)
        {
            int x = Mathf.Clamp((int)(Mathf.Repeat(t.x, 1f) * w), 0, w - 1);
            int y = Mathf.Clamp((int)(Mathf.Repeat(t.y, 1f) * h), 0, h - 1);
            var c = px[y * w + x];
            return c.g > 128 && c.r < 128;
        }

        // Weld the copies a vertex has along texture seams, so regions cross them.
        var ids = new int[up.Length];
        var byPos = new System.Collections.Generic.Dictionary<Vector3Int, int>();
        for (int i = 0; i < up.Length; i++)
        {
            var key = Vector3Int.RoundToInt(up[i] * 100000f);
            if (!byPos.TryGetValue(key, out int id)) { id = byPos.Count; byPos[key] = id; }
            ids[i] = id;
        }
        int n = byPos.Count;
        var teal = new bool[n];
        for (int i = 0; i < up.Length; i++) if (IsTeal(uv[i])) teal[ids[i]] = true;
        var parent = Enumerable.Range(0, n).ToArray();
        int Find(int a) { while (parent[a] != a) { parent[a] = parent[parent[a]]; a = parent[a]; } return a; }
        void Join(int a, int b) { a = Find(ids[a]); b = Find(ids[b]); if (a != b && teal[a] == teal[b]) parent[b] = a; }
        for (int t = 0; t < tris.Length; t += 3)
            for (int k = 0; k < 3; k++)
            {
                int a = tris[t + k], b = tris[t + (k + 1) % 3];
                if (teal[ids[a]] && teal[ids[b]]) Join(a, b);
            }

        var b0 = GeometryUtility.CalculateBounds(up, Matrix4x4.identity);
        var sum = new System.Collections.Generic.Dictionary<int, (Vector3 s, int count)>();
        for (int i = 0; i < up.Length; i++)
        {
            if (!teal[ids[i]]) continue;
            int r = Find(ids[i]);
            sum.TryGetValue(r, out var acc);
            sum[r] = (acc.s + up[i], acc.count + 1);
        }
        var glasses = new System.Collections.Generic.HashSet<int>();
        foreach (var kv in sum)
        {
            var centre = kv.Value.s / kv.Value.count;
            float fx = (centre.x - b0.center.x) / b0.size.x, fy = (centre.y - b0.min.y) / b0.size.y;
            bool isGlasses = Mathf.Abs(fx) < 0.25f && fy > 0.6f;
            if (isGlasses) glasses.Add(kv.Key);
            if (kv.Value.count > 50)
                Debug.Log($"[LUMA] teal region: {kv.Value.count} vertices at ({fx:F2} W, {fy:F2} H){(isGlasses ? " - glasses" : "")}");
        }
        var colours = new Color32[up.Length];
        int marked = 0;
        for (int i = 0; i < up.Length; i++)
        {
            bool g = teal[ids[i]] && glasses.Contains(Find(ids[i]));
            colours[i] = g ? new Color32(255, 0, 0, 255) : new Color32(0, 0, 0, 255);
            if (g) marked++;
        }
        Debug.Log($"[LUMA] Glasses: {marked} vertices marked");
        return colours;
    }

    static Material BuildMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNew = mat == null;
        if (isNew) mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "LUMA" };

        mat.SetTexture("_BaseMap", Load(Tex + "LUMA_BaseColor.png"));
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTexture("_BumpMap", Load(Tex + "LUMA_Normal.png"));
        mat.SetFloat("_BumpScale", 1f);
        mat.EnableKeyword("_NORMALMAP");
        // Metallic in R (none: she is painted vinyl), smoothness in A (glossy rings, satin body).
        mat.SetTexture("_MetallicGlossMap", Load(Tex + "LUMA_MetalSmooth.png"));
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 1f);
        mat.SetFloat("_SmoothnessTextureChannel", 0f);
        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        mat.SetTexture("_EmissionMap", Load(Tex + "LUMA_Emission.png"));
        mat.SetColor("_EmissionColor", Color.white * Glow);
        // As URP's material editor sets it, so its keyword pass keeps _EMISSION on.
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        mat.SetKeyword(new UnityEngine.Rendering.LocalKeyword(mat.shader, "_EMISSION"), true);

        if (isNew) AssetDatabase.CreateAsset(mat, MatPath);
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssetIfDirty(mat);
        if (!mat.IsKeywordEnabled("_EMISSION")) Debug.LogError("[LUMA] The material lost its glow (_EMISSION)");
        return mat;
    }

    static Texture2D Load(string path) => AssetDatabase.LoadAssetAtPath<Texture2D>(path);

    static void ApplyToScene(Mesh mesh, Material mat)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        var rig = FindRig();
        // Through SerializedObject, so the changes are recorded as overrides on the
        // LUMA prefab instance and survive a reload.
        var filter = new SerializedObject(rig.GetComponent<MeshFilter>());
        filter.FindProperty("m_Mesh").objectReferenceValue = mesh;
        filter.ApplyModifiedPropertiesWithoutUndo();
        var renderer = new SerializedObject(rig.GetComponent<MeshRenderer>());
        var mats = renderer.FindProperty("m_Materials");
        mats.arraySize = 1;
        mats.GetArrayElementAtIndex(0).objectReferenceValue = mat;
        renderer.ApplyModifiedPropertiesWithoutUndo();
        var so = new SerializedObject(rig);
        so.FindProperty("shoulder").vector3Value = Shoulder;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static LumaRig FindRig()
    {
        var coach = Object.FindFirstObjectByType<LoomaCoach>(FindObjectsInactive.Include);
        return (LumaRig)new SerializedObject(coach).FindProperty("luma").objectReferenceValue;
    }

    // ---- The portrait for the other games ----

    [MenuItem("Claude Bomb/Render LUMA Portrait")]
    public static void RenderPortrait()
    {
        string outPath = System.Environment.GetEnvironmentVariable("LUMA_PORTRAIT") ?? "Builds/luma.png";
        RenderPortraitTo(outPath, 512, 680);
        Debug.Log("[LUMA] Portrait written to " + outPath);
    }

    public static void RenderPortraitBatch()
    {
        RenderPortrait();
        EditorApplication.Exit(0);
    }

    // LUMA in her drawn pose (arms up), turned a little so the glasses read as rings,
    // lit by a warm key, a soft fill and a cool rim that keeps her navy outline visible on
    // the dark cards the games put her on. The background is cut out by rendering twice,
    // over black and over white, and taking the difference as coverage.
    public static void RenderPortraitTo(string path, int w, int h)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        var luma = new GameObject("LUMA");
        luma.AddComponent<MeshFilter>().sharedMesh = mesh;
        luma.AddComponent<MeshRenderer>().sharedMaterial = mat;
        // The mesh is Z up facing -Y: stand her up (Y up, facing +Z), then turn her round
        // to face the camera at -Z, and 14 degrees more.
        luma.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        luma.transform.Rotate(Vector3.up, 180f + 14f, Space.World);
        luma.transform.localScale = Vector3.one * (1f / mesh.bounds.size.z); // one unit tall

        Light Make(string name, Color c, float intensity, Vector3 euler)
        {
            var l = new GameObject(name).AddComponent<Light>();
            l.type = LightType.Directional; l.color = c; l.intensity = intensity;
            l.transform.rotation = Quaternion.Euler(euler);
            return l;
        }
        Make("Key", new Color(1f, 0.97f, 0.93f), 1.5f, new Vector3(25f, 160f, 0f));
        Make("Fill", new Color(0.8f, 0.88f, 1f), 0.5f, new Vector3(5f, 220f, 0f));
        Make("Rim", new Color(0.7f, 0.85f, 1f), 1.1f, new Vector3(15f, 20f, 0f));
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.32f, 0.36f, 0.42f);

        var bounds = luma.GetComponent<MeshRenderer>().bounds;
        var cam = new GameObject("Cam").AddComponent<Camera>();
        cam.fieldOfView = 20f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        float fit = Mathf.Max(bounds.extents.y, bounds.extents.x * h / w) * 1.08f;
        float dist = fit / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) + bounds.extents.z;
        cam.transform.position = bounds.center + new Vector3(0f, 0.04f, -dist);
        cam.transform.LookAt(bounds.center);

        var black = Grab(cam, Color.black, w, h);
        var white = Grab(cam, Color.white, w, h);
        var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++)
        {
            Color k = black[i], wh = white[i];
            float a = Mathf.Clamp01(1f - ((wh.r - k.r) + (wh.g - k.g) + (wh.b - k.b)) / 3f);
            px[i] = a > 0.002f ? new Color(Mathf.Clamp01(k.r / a), Mathf.Clamp01(k.g / a), Mathf.Clamp01(k.b / a), a) : Color.clear;
        }
        outTex.SetPixels(px);
        outTex.Apply();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        File.WriteAllBytes(path, outTex.EncodeToPNG());
    }

    static Color[] Grab(Camera cam, Color background, int w, int h)
    {
        cam.backgroundColor = background;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        rt.Release();
        return tex.GetPixels();
    }
}
