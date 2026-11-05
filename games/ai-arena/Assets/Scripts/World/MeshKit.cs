using System;
using System.Collections.Generic;
using UnityEngine;

// Every shape in the market, made in code the way the 2D sprites were (SpriteBaker):
// boxes, surfaces of revolution (spheres, cylinders, cones, pears, domes), a pyramid
// for the roofs and a bent tube for bananas and chilis. No model files to keep.
// Meshes are shared; Part() puts one in the scene.
public static class MeshKit
{
    static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

    public static Mesh Box => Get("box", BoxMesh);
    public static Mesh Sphere => Get("sphere", () => Lathe(Arc(0.5f, 0f, 12), 20));
    public static Mesh Cylinder => Get("cylinder", () => Lathe(new[] { (0f, -0.5f), (0.5f, -0.5f), (0.5f, -0.5f), (0.5f, 0.5f), (0.5f, 0.5f), (0f, 0.5f) }, 20));
    public static Mesh Cone => Get("cone", () => Lathe(new[] { (0f, -0.5f), (0.5f, -0.5f), (0.5f, -0.5f), (0f, 0.5f) }, 20));
    public static Mesh Capsule => Get("capsule", () => Lathe(CapsuleProfile(), 16)); // radius 0.25, 1 tall
    public static Mesh Dome => Get("dome", () => Lathe(DomeProfile(), 20));          // half a sphere, flat underneath, 0.5 tall
    public static Mesh Pear => Get("pear", () => Lathe(new[] { (0f, 0f), (0.26f, 0.02f), (0.36f, 0.14f), (0.33f, 0.32f), (0.22f, 0.52f), (0.14f, 0.7f), (0.11f, 0.88f), (0f, 1f) }, 16));
    public static Mesh Pyramid => Get("pyramid", PyramidMesh);                      // base 1 by 1 at y 0, apex at y 1
    public static Mesh Tube => Get("tube", () => BentTube(0.5f, 120f, 0.12f));       // a banana, about 1 long, lying on its back
    public static Mesh ThinTube => Get("thintube", () => BentTube(0.6f, 80f, 0.07f)); // a chili

    static Mesh Get(string name, Func<Mesh> make)
    {
        if (!meshes.TryGetValue(name, out var m) || m == null) { m = make(); m.name = name; meshes[name] = m; }
        return m;
    }

    // One mesh in the scene: a filter and a renderer, casting and receiving shadows.
    public static GameObject Part(Transform parent, string name, Mesh mesh, Material material, Vector3 position, Vector3 euler, Vector3 scale)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = material;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        r.receiveShadows = true;
        return go;
    }

    public static GameObject Part(Transform parent, string name, Mesh mesh, Material material, Vector3 position, Vector3 scale)
        => Part(parent, name, mesh, material, position, Vector3.zero, scale);

    public static GameObject Part(Transform parent, string name, Mesh mesh, Material material, Vector3 position, float scale)
        => Part(parent, name, mesh, material, position, Vector3.zero, Vector3.one * scale);

    // ---- profiles for the lathe: (radius, height), from the bottom up ----

    // The right half of a circle, bottom to top, centred at height c.
    static (float, float)[] Arc(float r, float c, int steps)
    {
        var p = new (float, float)[steps + 1];
        for (int i = 0; i <= steps; i++) { float a = Mathf.PI * i / steps; p[i] = (r * Mathf.Sin(a), c - r * Mathf.Cos(a)); }
        return p;
    }

    static (float, float)[] CapsuleProfile()
    {
        var list = new List<(float, float)>();
        const float r = 0.25f;
        for (int i = 0; i <= 6; i++) { float a = Mathf.PI / 2 * i / 6; list.Add((r * Mathf.Sin(a), -0.25f - r * Mathf.Cos(a))); }
        for (int i = 0; i <= 6; i++) { float a = Mathf.PI / 2 * i / 6; list.Add((r * Mathf.Cos(a), 0.25f + r * Mathf.Sin(a))); }
        return list.ToArray();
    }

    static (float, float)[] DomeProfile()
    {
        var list = new List<(float, float)> { (0f, 0f), (0.5f, 0f) };
        for (int i = 0; i <= 8; i++) { float a = Mathf.PI / 2 * i / 8; list.Add((0.5f * Mathf.Cos(a), 0.5f * Mathf.Sin(a))); }
        return list.ToArray();
    }

    // ---- the generators ----

    // A surface of revolution around the Y axis. A point repeated in the profile
    // makes a hard edge (the rim of a cylinder); the rings in between are skipped.
    public static Mesh Lathe((float r, float y)[] profile, int segments)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        int n = profile.Length;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < segments; j++)
            {
                float t = Mathf.PI * 2 * j / segments;
                verts.Add(new Vector3(profile[i].r * Mathf.Cos(t), profile[i].y, profile[i].r * Mathf.Sin(t)));
            }
        bool? flip = null;
        for (int i = 0; i < n - 1; i++)
        {
            if (profile[i] == profile[i + 1]) continue;
            for (int j = 0; j < segments; j++)
            {
                int a = i * segments + j, b = i * segments + (j + 1) % segments, c = (i + 1) * segments + j, d = (i + 1) * segments + (j + 1) % segments;
                if (flip == null && (profile[i].r > 0 || profile[i + 1].r > 0))
                {
                    var outward = new Vector3(verts[a].x + verts[c].x, 0, verts[a].z + verts[c].z);
                    flip = Vector3.Dot(Vector3.Cross(verts[c] - verts[a], verts[d] - verts[a]), outward) < 0;
                }
                Quad(tris, a, c, d, b, flip ?? false);
            }
        }
        return Finish(verts, tris);
    }

    // A tube along an arc in the XY plane, the middle at the origin and both ends
    // curving up, fatter in the middle and closed at the ends.
    static Mesh BentTube(float bend, float degrees, float radius)
    {
        const int rings = 16, segments = 12;
        var verts = new List<Vector3>();
        var tris = new List<int>();
        float half = degrees / 2 * Mathf.Deg2Rad;
        for (int i = 0; i <= rings; i++)
        {
            float k = (float)i / rings, th = -half + k * 2 * half;
            var centre = new Vector3(bend * Mathf.Sin(th), bend * (1 - Mathf.Cos(th)), 0);
            var normal = new Vector3(-Mathf.Sin(th), Mathf.Cos(th), 0);
            float rho = i == 0 || i == rings ? 0f : radius * (0.3f + 0.7f * Mathf.Pow(Mathf.Sin(Mathf.PI * k), 0.6f));
            for (int j = 0; j < segments; j++)
            {
                float p = Mathf.PI * 2 * j / segments;
                verts.Add(centre + rho * (Mathf.Cos(p) * normal + Mathf.Sin(p) * Vector3.forward));
            }
        }
        bool? flip = null;
        for (int i = 0; i < rings; i++)
            for (int j = 0; j < segments; j++)
            {
                int a = i * segments + j, b = i * segments + (j + 1) % segments, c = (i + 1) * segments + j, d = (i + 1) * segments + (j + 1) % segments;
                if (flip == null && i > 0)
                {
                    float th = -half + (float)i / rings * 2 * half;
                    var centre = new Vector3(bend * Mathf.Sin(th), bend * (1 - Mathf.Cos(th)), 0);
                    flip = Vector3.Dot(Vector3.Cross(verts[c] - verts[a], verts[d] - verts[a]), verts[a] - centre) < 0;
                }
                Quad(tris, a, c, d, b, flip ?? false);
            }
        return Finish(verts, tris);
    }

    static Mesh BoxMesh()
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward)
        {
            int i = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
            Quad(tris, i, i + 1, i + 2, i + 3, Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0);
        }
        const float h = 0.5f;
        Face(new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(h, h, h), new Vector3(h, -h, h), Vector3.right);
        Face(new Vector3(-h, -h, -h), new Vector3(-h, h, -h), new Vector3(-h, h, h), new Vector3(-h, -h, h), Vector3.left);
        Face(new Vector3(-h, h, -h), new Vector3(h, h, -h), new Vector3(h, h, h), new Vector3(-h, h, h), Vector3.up);
        Face(new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, -h, h), new Vector3(-h, -h, h), Vector3.down);
        Face(new Vector3(-h, -h, h), new Vector3(h, -h, h), new Vector3(h, h, h), new Vector3(-h, h, h), Vector3.forward);
        Face(new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(-h, h, -h), Vector3.back);
        return Finish(verts, tris);
    }

    static Mesh PyramidMesh()
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var apex = new Vector3(0, 1, 0);
        var corners = new[] { new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f) };
        for (int i = 0; i < 4; i++)
        {
            Vector3 a = corners[i], b = corners[(i + 1) % 4];
            var outward = (a + b) / 2 - new Vector3(0, 0.3f, 0);
            int k = verts.Count;
            verts.Add(apex); verts.Add(a); verts.Add(b);
            bool flip = Vector3.Dot(Vector3.Cross(a - apex, b - apex), outward) < 0;
            if (flip) { tris.Add(k); tris.Add(k + 2); tris.Add(k + 1); } else { tris.Add(k); tris.Add(k + 1); tris.Add(k + 2); }
        }
        int q = verts.Count;
        verts.AddRange(corners);
        Quad(tris, q, q + 1, q + 2, q + 3, Vector3.Dot(Vector3.Cross(corners[1] - corners[0], corners[2] - corners[0]), Vector3.down) < 0);
        return Finish(verts, tris);
    }

    static void Quad(List<int> tris, int a, int b, int c, int d, bool flip)
    {
        if (flip) { tris.Add(a); tris.Add(c); tris.Add(b); tris.Add(a); tris.Add(d); tris.Add(c); }
        else { tris.Add(a); tris.Add(b); tris.Add(c); tris.Add(a); tris.Add(c); tris.Add(d); }
    }

    static Mesh Finish(List<Vector3> verts, List<int> tris)
    {
        var m = new Mesh();
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
