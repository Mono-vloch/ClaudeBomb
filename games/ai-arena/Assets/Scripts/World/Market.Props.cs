using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Pieces built at run time from the kit's own materials (the game-feel pass): boxes, discs,
// rings, stars, and the one dark plate every label in the world stands on (Robot Racing's
// rule: no floating white text, every world label on the same dark slab with a teal halo
// when it is lit). Nothing here is a model: the garage table, the crate, the coins, the
// lenses, the bulbs, the battery, the clock and the podium are made from these.
public partial class Market
{
    static readonly Dictionary<string, Material> kitMaterials = new Dictionary<string, Material>();

    // A material from one of the kit's prefabs, by the piece's name and a word of the
    // material's name; the first one found when no name matches; a plain URP Lit without the kit.
    public static Material KitMaterial(string piece, string contains)
    {
        string key = piece + ":" + contains;
        if (kitMaterials.TryGetValue(key, out var m) && m != null) return m;
        var prefab = Kit(piece);
        if (prefab != null)
            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
                foreach (var mat in r.sharedMaterials)
                    if (mat != null && mat.name.ToLowerInvariant().Contains(contains)) { kitMaterials[key] = mat; return mat; }
        var any = prefab != null ? prefab.GetComponentInChildren<Renderer>(true) : null;
        if (any != null && any.sharedMaterial != null) m = any.sharedMaterial;
        else
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            m = shader != null ? new Material(shader) { name = "lit " + key } : Look.Mat(Color.white);
        }
        kitMaterials[key] = m;
        return m;
    }

    public static Material Stone => KitMaterial("HomeStone", "home");
    public static Material Metal => KitMaterial("Agent", "mast");
    public static Material LampMat => KitMaterial("Agent", "bulb");
    public static Material CrateMat => KitMaterial("Unknown", "crate");
    public static Material PaperMat => KitMaterial("Agent", "notebook");

    public static void Tint(Renderer r, Color c)
    {
        var block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetColor("_BaseColor", c);
        block.SetColor("_Color", c);
        r.SetPropertyBlock(block);
    }

    public static void Glow(Renderer r, Color c)
    {
        var block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetColor("_EmissionColor", c);
        r.SetPropertyBlock(block);
    }

    static readonly Color Ink3D = new Color(0.06f, 0.09f, 0.18f);

    public static GameObject Box(Transform parent, Vector3 size, Vector3 at, Material mat, Color? tint, string name = "Box")
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = BoxMesh(size);
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (tint.HasValue) Tint(r, tint.Value);
        return go;
    }

    static Mesh unitBox;
    static readonly Dictionary<Vector3, Mesh> boxes = new Dictionary<Vector3, Mesh>();
    public static Mesh BoxMesh(Vector3 s)
    {
        if (s == Vector3.one && unitBox != null) return unitBox;
        if (boxes.TryGetValue(s, out var cached) && cached != null) return cached;
        var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        void Face(Vector3 normal, Vector3 right, Vector3 up, float w, float h)
        {
            int i = v.Count;
            var c = normal * 0.5f;
            v.Add(Vector3.Scale(c - right * 0.5f - up * 0.5f, s)); v.Add(Vector3.Scale(c - right * 0.5f + up * 0.5f, s));
            v.Add(Vector3.Scale(c + right * 0.5f + up * 0.5f, s)); v.Add(Vector3.Scale(c + right * 0.5f - up * 0.5f, s));
            for (int k = 0; k < 4; k++) n.Add(normal);
            uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(0, h)); uv.Add(new Vector2(w, h)); uv.Add(new Vector2(w, 0));
            tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
        Face(Vector3.forward, Vector3.left, Vector3.up, s.x, s.y); Face(Vector3.back, Vector3.right, Vector3.up, s.x, s.y);
        Face(Vector3.right, Vector3.forward, Vector3.up, s.z, s.y); Face(Vector3.left, Vector3.back, Vector3.up, s.z, s.y);
        Face(Vector3.up, Vector3.right, Vector3.forward, s.x, s.z); Face(Vector3.down, Vector3.right, Vector3.back, s.x, s.z);
        var mesh = new Mesh { name = "slab" };
        mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        if (s == Vector3.one) unitBox = mesh;
        boxes[s] = mesh;
        return mesh;
    }

    // A flat disc facing -Z, both faces.
    public static GameObject Disc(Transform parent, float radius, Vector3 at, Material mat, Color? tint, string name = "Disc")
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        const int n = 24;
        v.Add(Vector3.zero); uv.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i <= n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            v.Add(new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
            uv.Add(new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
            if (i > 0) tris.AddRange(new[] { 0, i + 1, i });
        }
        int baseCount = v.Count;
        for (int i = 0; i < baseCount; i++) { v.Add(v[i]); uv.Add(uv[i]); }
        for (int i = 1; i <= n; i++) tris.AddRange(new[] { baseCount, baseCount + i, baseCount + i + 1 });
        var mesh = new Mesh { name = "round" };
        mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        var normals = new List<Vector3>(); for (int i = 0; i < baseCount; i++) normals.Add(Vector3.back); for (int i = 0; i < baseCount; i++) normals.Add(Vector3.forward);
        mesh.SetNormals(normals);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (tint.HasValue) Tint(r, tint.Value);
        return go;
    }

    // A ring facing -Z, both faces: a lens.
    public static GameObject Ring(Transform parent, float radius, float width, Vector3 at, Material mat, Color? tint, string name = "Ring")
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>(); var nrm = new List<Vector3>();
        const int n = 24;
        float inner = radius - width;
        for (int i = 0; i <= n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            var d = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
            v.Add(d * inner); v.Add(d * radius); nrm.Add(Vector3.back); nrm.Add(Vector3.back);
            uv.Add(new Vector2(i / (float)n, 0)); uv.Add(new Vector2(i / (float)n, 1));
            if (i > 0) { int b = (i - 1) * 2; tris.AddRange(new[] { b, b + 3, b + 1, b, b + 2, b + 3 }); }
        }
        int baseCount = v.Count;
        for (int i = 0; i < baseCount; i++) { v.Add(v[i]); uv.Add(uv[i]); nrm.Add(Vector3.forward); }
        for (int i = 1; i <= n; i++) { int b = baseCount + (i - 1) * 2; tris.AddRange(new[] { b, b + 1, b + 3, b, b + 3, b + 2 }); }
        var mesh = new Mesh { name = "ring" };
        mesh.SetVertices(v); mesh.SetNormals(nrm); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (tint.HasValue) Tint(r, tint.Value);
        return go;
    }

    // A short cylinder standing on its base: a coin, a knob, a bin.
    public static GameObject Cylinder(Transform parent, float radius, float height, Vector3 at, Material mat, Color? tint, string name = "Cylinder")
    {
        var v = new List<Vector3>(); var nrm = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        const int n = 20;
        float h = height / 2f;
        for (int i = 0; i <= n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            v.Add(dir * radius - Vector3.up * h); v.Add(dir * radius + Vector3.up * h);
            nrm.Add(dir); nrm.Add(dir);
            uv.Add(new Vector2(i / (float)n, 0)); uv.Add(new Vector2(i / (float)n, 1));
            if (i > 0) { int b = (i - 1) * 2; tris.AddRange(new[] { b, b + 1, b + 2, b + 1, b + 3, b + 2 }); }
        }
        foreach (float sign in new[] { 1f, -1f })
        {
            int c = v.Count;
            v.Add(Vector3.up * h * sign); nrm.Add(Vector3.up * sign); uv.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f;
                v.Add(new Vector3(Mathf.Cos(a) * radius, h * sign, Mathf.Sin(a) * radius)); nrm.Add(Vector3.up * sign); uv.Add(new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
                if (i > 0) { if (sign > 0) tris.AddRange(new[] { c, c + i + 1, c + i }); else tris.AddRange(new[] { c, c + i, c + i + 1 }); }
            }
        }
        var mesh = new Mesh { name = "drum" };
        mesh.SetVertices(v); mesh.SetNormals(nrm); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (tint.HasValue) Tint(r, tint.Value);
        return go;
    }

    public static GameObject Star(Transform parent, float r, Vector3 at, Color tint)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        v.Add(Vector3.zero); uv.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i < 10; i++)
        {
            float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float rr = i % 2 == 0 ? r : r * 0.45f;
            v.Add(new Vector3(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
        }
        for (int i = 1; i <= 10; i++) { int j = i % 10 + 1; tris.AddRange(new[] { 0, j, i }); }
        int baseCount = v.Count;
        for (int i = 0; i < baseCount; i++) { v.Add(v[i]); uv.Add(uv[i]); }
        for (int i = 1; i <= 10; i++) { int j = i % 10 + 1; tris.AddRange(new[] { baseCount, baseCount + i, baseCount + j }); }
        var mesh = new Mesh { name = "star" };
        mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        var normals = new List<Vector3>(); for (int i = 0; i < baseCount; i++) normals.Add(Vector3.back); for (int i = 0; i < baseCount; i++) normals.Add(Vector3.forward);
        mesh.SetNormals(normals);
        mesh.RecalculateBounds();
        var go = new GameObject("Star");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = LampMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Tint(mr, tint);
        Glow(mr, tint * 0.6f);
        return go;
    }

    // ---- the dark plate: every label in the world ----

    // A slab of the kit's stone tinted ink, white text, and a teal halo behind its edge when it
    // is lit. The slab fits the text; a wrapped plate wraps at the given width.
    public static Transform DarkPlate(Transform parent, string text, float fontSize, out TextMeshPro tmp, float minW = 0.26f, float wrapWidth = 0f)
    {
        var root = new GameObject("Plate").transform;
        root.SetParent(parent, false);
        var halo = Box(root, Vector3.one, new Vector3(0f, 0f, 0.012f), Stone, Theme.Teal, "Halo");
        halo.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; halo.SetActive(false);
        var slab = Box(root, Vector3.one, Vector3.zero, Stone, Ink3D, "Slab");
        slab.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var go = new GameObject("Text");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(0f, 0f, -0.016f);
        tmp = go.AddComponent<TextMeshPro>();
        tmp.font = Theme.Bold; tmp.fontSize = fontSize; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(wrapWidth > 0f ? wrapWidth : 4f, 1f);
        tmp.enableWordWrapping = wrapWidth > 0f;
        tmp.text = text;
        var fit = root.gameObject.AddComponent<PlateFit>();
        fit.minW = minW; fit.wrapWidth = wrapWidth;
        FitPlate(root);
        return root;
    }

    // The slab and the halo sized to the text (called after the text changes).
    public static void FitPlate(Transform plateRoot)
    {
        var tmp = plateRoot.GetComponentInChildren<TextMeshPro>(true);
        var fit = plateRoot.GetComponent<PlateFit>();
        if (tmp == null) return;
        tmp.ForceMeshUpdate();
        var size = tmp.textBounds.size;
        float minW = fit != null ? fit.minW : 0.26f, pad = fit != null ? fit.pad : 0.1f;
        float w = Mathf.Max(minW, size.x + pad), h = Mathf.Max(0.08f, size.y + pad / 2f);
        var slab = plateRoot.Find("Slab"); if (slab != null) slab.localScale = new Vector3(w, h, 0.02f);
        var halo = plateRoot.Find("Halo"); if (halo != null) halo.localScale = new Vector3(w + 0.04f, h + 0.04f, 0.012f);
    }

    public static void SetPlateText(Transform plateRoot, string text)
    {
        var tmp = plateRoot.GetComponentInChildren<TextMeshPro>(true);
        if (tmp == null) return;
        tmp.text = text;
        FitPlate(plateRoot);
    }

    public static void LitPlate(Transform plateRoot, bool on)
    {
        var halo = plateRoot.Find("Halo");
        if (halo != null) halo.gameObject.SetActive(on);
    }

    public class PlateFit : MonoBehaviour { public float minW = 0.26f, pad = 0.1f, wrapWidth = 0f; }

    // A label standing in the world, turned to the camera.
    public Transform WorldPlate(string text, Vector3 at, float size, Color colour, Transform parent = null, float wrapWidth = 0f)
    {
        var plate = DarkPlate(parent != null ? parent : transform, text, size, out var tmp, 0.26f, wrapWidth);
        plate.name = "Label " + text;
        plate.position = at;
        tmp.color = colour;
        plate.gameObject.AddComponent<Billboard>().target = cam.transform;
        return plate;
    }

    // A pop: the thing swells for a moment.
    public void Swell(Transform t) { if (t != null) StartCoroutine(DoPunch(t)); }

    // A small teal lamp over a thing that wants attention; null stops it.
    Coroutine beckon; Light beckonLight; Transform beckoning;
    public void Beckon(Transform t)
    {
        if (beckon != null) { StopCoroutine(beckon); beckon = null; }
        if (beckoning != null) beckoning.localScale = Vector3.one;
        if (beckonLight != null) Destroy(beckonLight.gameObject);
        beckoning = null;
        if (t != null) { beckoning = t; beckon = StartCoroutine(DoBeckon(t)); }
    }

    System.Collections.IEnumerator DoBeckon(Transform t)
    {
        beckonLight = new GameObject("Beckon").AddComponent<Light>();
        beckonLight.transform.SetParent(t, false); beckonLight.transform.localPosition = Vector3.up * 0.35f;
        beckonLight.type = LightType.Point; beckonLight.color = Theme.Teal; beckonLight.range = 1.4f; beckonLight.shadows = LightShadows.None;
        while (t != null)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
            t.localScale = Vector3.one * (1f + 0.06f * k);
            beckonLight.intensity = 1.2f + 2.2f * k;
            yield return null;
        }
    }

    // A world point on the screen, in pixels (z below zero: behind the camera).
    public Vector3 ScreenOf(Vector3 world) => cam.ToScreen(world);

    // Where the pointer's ray meets a level plane at the given height.
    public bool PlaneHit(Vector2 screen, float y, out Vector3 hit)
    {
        var ray = cam.GetComponent<Camera>().ScreenPointToRay(screen);
        hit = Vector3.zero;
        if (Mathf.Abs(ray.direction.y) < 1e-4f) return false;
        float t = (y - ray.origin.y) / ray.direction.y;
        if (t < 0) return false;
        hit = ray.origin + ray.direction * t;
        return true;
    }

    // The direction from a home cell into the square, along an axis, and the camera's right
    // when it looks from the square at that house.
    public static Vector3 Inward(Vector2Int home)
    {
        var c = Cell(home);
        var d = -c; d.y = 0f;
        if (Mathf.Abs(d.x) > Mathf.Abs(d.z)) return new Vector3(Mathf.Sign(d.x), 0f, 0f);
        return new Vector3(0f, 0f, Mathf.Sign(d.z));
    }

    // The robot of an agent, for the screens that hang things on it.
    public RobotModel BotOf(AgentState a) => a != null && bots.TryGetValue(a, out var r) ? r : null;

    // The one house wall behind a home cell, for the lit window: the kit's houses ring the square.
    public Vector3 DoorOf(Vector2Int home) => Cell(home) - Inward(home) * (Pitch * 0.9f) + Vector3.up * 0.1f;
}
