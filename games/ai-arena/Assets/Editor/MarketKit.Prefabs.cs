using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

// The prefabs Market places: the Higgsfield models normalised (metres, pivot at the base
// centre, facing +Z), a stall per style with its counter measured, the goods of every item
// (a crate heaped with the fruit or vegetable and a pile on the counter, the pieces merged
// into one mesh), the crate a stall becomes when the agent cannot see it, the stone that
// marks an agent's home, the square itself (cobbles in metre UVs, granite bands between
// the cells, a kerb, the street, a ring of houses, the fountain, lamps, planters, benches),
// the agent with its five clips and the two skies.
//
// One cell of the arena is Pitch metres, so a 2 m stall stands in a cell with room to
// walk round it and the 1.2 m agent is to scale with both. Nothing here uses the Flat
// shader or MeshKit.
public static partial class MarketKit
{
    public const float Pitch = 2.4f;   // one cell of the arena, in metres (Market.KitPitch)
    const int Size = 10;               // Arena.Size
    const float Half = Size * Pitch / 2f;

    class Mats { public Material plaza, band, kerb, street, home, unknown, metal, bulb, flame, notebook; }

    [MenuItem("AI Arena/Build market kit")]
    public static void Build()
    {
        foreach (var d in new[] { Generated, Generated + "/Meshes", Generated + "/Clips", Out, Out + "/Hero", Out + "/Goods", Out + "/Pieces" }) Directory.CreateDirectory(d);
        AssetDatabase.Refresh();
        SetUpModels();
        var m = new Mats
        {
            plaza = Surface("cobblestone_floor_08", new Color(0.96f, 0.9f, 0.82f), Vector2.one, name: "plaza"),
            band = Surface("large_sandstone_blocks_01", new Color(1.12f, 1.06f, 0.96f), Vector2.one, name: "band"),
            kerb = Surface("large_sandstone_blocks_01", new Color(0.92f, 0.88f, 0.8f), Vector2.one, name: "kerb"),
            street = Surface("cobblestone_floor_04", new Color(0.82f, 0.8f, 0.76f), Vector2.one, name: "street"),
            home = Surface("large_sandstone_blocks_01", Color.white, Vector2.one, name: "home_stone"),
        };
        m.metal = Lit("antenna_mast", Swatch("graphite", new Color32(58, 60, 66, 255)), FlatNormal(), null, null, Color.white, Color.black, null, 0.6f);
        m.bulb = Lit("antenna_bulb", Swatch("lamp", new Color32(255, 214, 140, 255)), FlatNormal(), null, null, Color.white, new Color(1f, 0.82f, 0.48f) * 0.3f, null, 0.85f);
        m.flame = Lit("lantern_flame", Swatch("flame", new Color32(255, 200, 120, 255)), FlatNormal(), null, null, Color.white, new Color(1f, 0.72f, 0.36f) * 4f, null, 0.5f);
        m.notebook = Lit("notebook", Swatch("notebook", new Color32(22, 37, 70, 255)), FlatNormal(), null, null, Color.white, Color.black, null, 0.3f);

        var heroes = BuildHeroes();
        m.unknown = Lit("crate_unseen", AssetDatabase.LoadAssetAtPath<Texture2D>($"{HeroDir}/Crate/crate_base.jpg"), AssetDatabase.LoadAssetAtPath<Texture2D>($"{HeroDir}/Crate/crate_normal.png"),
                        null, null, new Color(0.58f, 0.63f, 0.7f), Color.black, null, 0.3f);
        var counters = new Dictionary<string, Vector3>();
        foreach (var style in new[] { "StallA", "StallB" }) counters[style] = BuildStall(style, heroes[style], m);
        foreach (var id in Specs.Keys) { BuildGoods(id, heroes["Crate"]); BuildPiece(id); }
        BuildUnknown(heroes["Crate"], m);
        BuildHomeStone(m);
        BuildSquare(heroes, m);
        BuildAgent(m);
        BuildSkies();
        AssetDatabase.SaveAssets();
        Debug.Log("[MarketKit] built " + string.Join(", ", Directory.GetFiles(Out, "*.prefab", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension)));
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // ---- Heroes: one prefab each, scale fixed, pivot at the base centre, facing +Z ----

    static Dictionary<string, GameObject> BuildHeroes()
    {
        var h = new Dictionary<string, GameObject>();
        // folder, file, texture size, yaw that turns its front to +Z, the dimension to fit (0 x, 1 y, 2 z after turning) and its size
        h["StallA"] = Hero("StallA", "stall_a", 1024, 90f, 0, 2.0f, detail: 512);
        h["StallB"] = Hero("StallB", "stall_b", 1024, 90f, 0, 2.0f, detail: 512);
        h["HouseA"] = Hero("HouseA", "house_a", 1024, 90f, 1, 7.2f, detail: 512);
        h["HouseB"] = Hero("HouseB", "house_b", 1024, 90f, 1, 6.8f, detail: 512);
        h["Fountain"] = Hero("Fountain", "fountain", 1024, 90f, 0, 3.4f, detail: 512);
        h["Crate"] = Hero("Crate", "crate", 1024, 90f, 0, 0.6f, detail: 512);
        return h;
    }

    static GameObject Hero(string folder, string file, int texSize, float yaw, int axis, float size, float emission = 0f, int detail = 0)
    {
        var glb = Directory.GetFiles($"{HeroDir}/{folder}", "*.glb")[0].Replace('\\', '/');
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(glb);
        var model = Object.Instantiate(source);
        model.name = "Model";
        var mat = HeroMaterial(folder, file, texSize, emission, detailSize: detail);
        foreach (var r in model.GetComponentsInChildren<Renderer>()) { r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray(); r.shadowCastingMode = ShadowCastingMode.On; }
        var root = new GameObject(folder);
        model.transform.SetParent(root.transform, false);
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        var b = Bounds(model);
        float k = size / b.size[axis];
        model.transform.localScale *= k;
        b = Bounds(model);
        model.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
        b = Bounds(model);
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Hero/{folder}.prefab");
        Object.DestroyImmediate(root);
        Debug.Log($"[MarketKit] hero {folder}: {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2} m");
        return prefab;
    }

    // ---- A stall: the model, its counter measured, a lantern under the awning (lit at dusk) ----

    static Vector3 BuildStall(string style, GameObject hero, Mats m)
    {
        var root = new GameObject("Stall" + style.Substring(5));
        var model = (GameObject)PrefabUtility.InstantiatePrefab(hero);
        model.transform.SetParent(root.transform, false);
        // The counter: the highest surface in the middle of the stall below the awning.
        var b = Bounds(model);
        float top = 0f, zs = 0f; int nz = 0;
        foreach (var f in model.GetComponentsInChildren<MeshFilter>())
            foreach (var v in f.sharedMesh.vertices)
            {
                var w = f.transform.TransformPoint(v);
                if (Mathf.Abs(w.x) > 0.45f || w.y < 0.5f || w.y > 1.35f) continue;
                if (Mathf.Abs(w.z - b.center.z) > 0.3f) continue;
                if (w.y > top + 0.002f) { top = w.y; zs = w.z; nz = 1; }
                else if (Mathf.Abs(w.y - top) < 0.03f) { zs += w.z; nz++; }
            }
        var counter = new GameObject("Counter").transform;
        counter.SetParent(root.transform, false);
        counter.localPosition = new Vector3(0f, top, nz > 0 ? zs / nz : b.center.z);
        // A lantern hangs from the awning's front edge, its flame and light on at dusk only.
        var lantern = new GameObject("Lantern").transform;
        lantern.SetParent(root.transform, false);
        lantern.localPosition = new Vector3(0.55f, Mathf.Min(b.max.y - 0.55f, 1.75f), b.max.z - 0.35f);
        var lamp = Prop("wooden_lantern_01", lantern, Vector3.zero, 0f, onFloor: false, height: 0.3f);
        var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.DestroyImmediate(flame.GetComponent<Collider>());
        flame.name = "Flame";
        flame.transform.SetParent(lantern, false);
        flame.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        flame.transform.localScale = Vector3.one * 0.06f;
        flame.GetComponent<MeshRenderer>().sharedMaterial = m.flame;
        flame.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        var light = AddLight(lantern, "LanternLight", LightType.Point, new Vector3(0f, 0.05f, 0.1f), new Color(1f, 0.78f, 0.5f), 2.2f, 4.5f);
        light.enabled = false;
        lantern.gameObject.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/{root.name}.prefab");
        Object.DestroyImmediate(root);
        Debug.Log($"[MarketKit] {style}: counter top {top:F2} m at z {(nz > 0 ? zs / nz : 0):F2}, stall {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2}");
        return new Vector3(0f, top, nz > 0 ? zs / nz : 0f);
    }

    // ---- The goods: what each item looks like on a counter ----

    // id -> where the model comes from, its size in metres (its longest side), laid on its side or not
    class Spec { public string kenney, polyHaven; public float size; public bool lay; public Vector3 stretch = Vector3.one; }
    static readonly Dictionary<string, Spec> Specs = new Dictionary<string, Spec>
    {
        ["apple"] = new Spec { kenney = "apple", size = 0.1f },
        ["banana"] = new Spec { kenney = "banana", size = 0.23f },
        ["orange"] = new Spec { kenney = "orange", size = 0.1f },
        ["strawberry"] = new Spec { kenney = "strawberry", size = 0.065f },
        ["grapes"] = new Spec { kenney = "grapes", size = 0.17f, lay = true },
        ["lemon"] = new Spec { kenney = "lemon", size = 0.095f },
        ["cherry"] = new Spec { kenney = "cherries", size = 0.08f },
        ["pear"] = new Spec { kenney = "pear", size = 0.12f },
        ["carrot"] = new Spec { kenney = "carrot", size = 0.24f, lay = true },
        ["potato"] = new Spec { polyHaven = "sweet_potato", size = 0.16f },
        ["broccoli"] = new Spec { kenney = "broccoli", size = 0.17f },
        ["mushroom"] = new Spec { kenney = "mushroom", size = 0.08f },
        ["chili"] = new Spec { kenney = "paprika", size = 0.15f, lay = true, stretch = new Vector3(1f, 2.3f, 0.75f) },
        ["tomato"] = new Spec { kenney = "tomato", size = 0.095f },
        ["corn"] = new Spec { kenney = "corn", size = 0.25f, lay = true },
    };

    // One piece of an item, standing on its origin, its longest side the spec's size.
    static GameObject Piece(string id, Transform parent, System.Random r)
    {
        var s = Specs[id];
        var holder = new GameObject(id).transform;
        holder.SetParent(parent, false);
        GameObject model;
        if (s.polyHaven != null)
        {
            var fbx = Directory.GetFiles($"{PolyHaven}/{s.polyHaven}", "*.fbx")[0].Replace('\\', '/');
            model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fbx));
        }
        else
        {
            model = new GameObject(s.kenney);
            model.AddComponent<MeshFilter>().sharedMesh = SmoothFood(s.kenney);
            model.AddComponent<MeshRenderer>().sharedMaterial = FoodMaterial();
        }
        model.transform.SetParent(holder, false);
        model.transform.localScale = Vector3.Scale(model.transform.localScale, s.stretch);
        if (s.lay) model.transform.localRotation = Quaternion.Euler(0f, 0f, 90f) * model.transform.localRotation;
        var b = Bounds(model);
        float k = s.size / Mathf.Max(b.size.x, b.size.y, b.size.z);
        holder.localScale = Vector3.one * k;
        b = Bounds(model);
        model.transform.position += new Vector3(holder.position.x - b.center.x, holder.position.y - b.min.y, holder.position.z - b.center.z);
        holder.localRotation = Quaternion.Euler((float)(r.NextDouble() * 10 - 5), (float)(r.NextDouble() * 360), (float)(r.NextDouble() * 10 - 5));
        return holder.gameObject;
    }

    // A crate heaped with the item, tipped a little toward the buyer, and a small pile on the
    // counter beside it. Origin: the middle of the counter top; the buyer stands at +Z.
    static void BuildGoods(string id, GameObject crate)
    {
        var s = Specs[id];
        var root = new GameObject("Goods_" + id);
        var rng = new System.Random(id.GetHashCode() & 0x7fff);
        var box = (GameObject)PrefabUtility.InstantiatePrefab(crate);
        box.transform.SetParent(root.transform, false);
        box.transform.localPosition = new Vector3(-0.3f, 0f, -0.04f);
        box.transform.localRotation = Quaternion.Euler(-7f, (float)(rng.NextDouble() * 6 - 3), 0f);
        var cb = Bounds(box);
        var pile = new GameObject("Pile").transform;
        pile.SetParent(box.transform, false);
        // The heap: rows across the crate, three layers, the top one mounding over the rim.
        float w = 0.5f, d = 0.3f, step = s.size * (s.lay ? 0.55f : 0.92f), stepX = s.lay ? s.size * 1.02f : step;
        float floor = cb.size.y * 0.5f;
        for (int layer = 0; layer < 3; layer++)
        {
            float inset = layer * step * 0.45f;
            int nx = Mathf.Max(1, Mathf.FloorToInt((w - 2 * inset) / stepX)), nzz = Mathf.Max(1, Mathf.FloorToInt((d - 2 * inset) / step));
            if (layer == 2) { nx = Mathf.Max(1, nx - 1); nzz = Mathf.Max(1, nzz - 1); }
            for (int ix = 0; ix < nx; ix++)
                for (int iz = 0; iz < nzz; iz++)
                {
                    var p = Piece(id, pile, rng);
                    float jx = (float)(rng.NextDouble() - 0.5) * step * 0.18f, jz = (float)(rng.NextDouble() - 0.5) * step * 0.18f;
                    p.transform.localPosition = new Vector3((ix - (nx - 1) / 2f) * stepX + jx, floor + layer * s.size * 0.42f, (iz - (nzz - 1) / 2f) * step + jz);
                }
        }
        // The pile on the counter: three, then two, then one.
        var heap = new GameObject("Heap").transform;
        heap.SetParent(root.transform, false);
        heap.localPosition = new Vector3(0.42f, 0f, 0.04f);
        int[] rows = s.lay ? new[] { 3, 2 } : new[] { 3, 2, 1 };
        for (int row = 0; row < rows.Length; row++)
            for (int i = 0; i < rows[row]; i++)
            {
                var p = Piece(id, heap, rng);
                float sp = s.lay ? s.size * 0.5f : s.size * 0.95f;
                p.transform.localPosition = new Vector3((i - (rows[row] - 1) / 2f) * (s.lay ? s.size * 0.25f : sp), row * s.size * (s.lay ? 0.35f : 0.62f), (i - (rows[row] - 1) / 2f) * (s.lay ? sp : 0f) * 0.6f + row * 0.01f);
            }
        Merge(pile, $"goods_{id}_crate");
        Merge(heap, $"goods_{id}_heap");
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Goods/{id}.prefab");
        Object.DestroyImmediate(root);
    }

    // The pieces under a holder merged into one mesh per material (a crate of apples is one
    // renderer, not twenty), saved as an asset.
    static void Merge(Transform holder, string name)
    {
        var byMat = new Dictionary<Material, List<CombineInstance>>();
        foreach (var f in holder.GetComponentsInChildren<MeshFilter>())
        {
            var mr = f.GetComponent<MeshRenderer>();
            for (int sub = 0; sub < f.sharedMesh.subMeshCount; sub++)
            {
                var mat = mr.sharedMaterials[Mathf.Min(sub, mr.sharedMaterials.Length - 1)];
                if (!byMat.TryGetValue(mat, out var list)) byMat[mat] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = f.sharedMesh, subMeshIndex = sub, transform = holder.worldToLocalMatrix * f.transform.localToWorldMatrix });
            }
        }
        for (int i = holder.childCount - 1; i >= 0; i--) Object.DestroyImmediate(holder.GetChild(i).gameObject);
        int n = 0;
        foreach (var kv in byMat)
        {
            // The index format is chosen before combining: switching it afterwards drops the
            // triangles (found 8 Nov 2026: the goods meshes were saved with no indices).
            int total = kv.Value.Sum(c => c.mesh.vertexCount);
            var mesh = new Mesh { name = name + "_" + n, indexFormat = total < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32 };
            mesh.CombineMeshes(kv.Value.ToArray(), true, true);
            mesh.RecalculateBounds();
            MeshUtility.SetMeshCompression(mesh, ModelImporterMeshCompression.Medium); // the web budget
            // No MeshUtility.Optimize here: on Unity 6000.3 it empties a combined mesh (found 8 Nov
            // 2026: every goods mesh was saved with 0 vertices, so the stalls showed bare crates).
            string path = $"{Generated}/Meshes/{name}_{n}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            var go = new GameObject("Merged");
            go.transform.SetParent(holder, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = kv.Key;
            r.shadowCastingMode = ShadowCastingMode.On;
            n++;
        }
    }

    // One piece, for the throw into the agent's basket and the basket itself.
    static void BuildPiece(string id)
    {
        var root = new GameObject("Piece_" + id);
        var p = Piece(id, root.transform, new System.Random(1));
        p.transform.localRotation = Quaternion.identity;
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Pieces/{id}.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- What the agent cannot see: a pair of covered crates, grey ----

    static void BuildUnknown(GameObject crate, Mats m)
    {
        var root = new GameObject("Unknown");
        for (int i = 0; i < 2; i++)
        {
            var c = (GameObject)PrefabUtility.InstantiatePrefab(crate);
            c.transform.SetParent(root.transform, false);
            c.transform.localScale = Vector3.one * (i == 0 ? 1.6f : 1.35f);
            c.transform.localRotation = Quaternion.Euler(0f, i == 0 ? 4f : -9f, 0f);
            c.transform.localPosition = new Vector3(i == 0 ? 0f : 0.05f, i == 0 ? 0f : 0.54f, 0f);
            foreach (var r in c.GetComponentsInChildren<Renderer>()) r.sharedMaterial = m.unknown;
        }
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Unknown.prefab");
        Object.DestroyImmediate(root);
    }

    // The agent's home: a paving slab in the team's colour (Market tints it) on its cell.
    static void BuildHomeStone(Mats m)
    {
        var root = new GameObject("HomeStone");
        Part(root.transform, "Slab", m.home, new Vector3(0f, 0.006f, 0f), new Vector3(1.9f, 0.012f, 1.9f), shadows: false, repeat: 0.95f);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/HomeStone.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- The square ----

    static void BuildSquare(Dictionary<string, GameObject> heroes, Mats m)
    {
        var root = new GameObject("Square");
        var t = root.transform;
        float edge = Half + 0.5f; // the plaza's half width: the grid and a margin
        Part(t, "Plaza", m.plaza, new Vector3(0f, -0.06f, 0f), new Vector3(2 * edge, 0.12f, 2 * edge), shadows: false, repeat: 1.7f);
        // Granite bands between the cells: the arena's grid, laid in stone.
        for (int i = 0; i <= Size; i++)
        {
            float c = -Half + i * Pitch;
            Part(t, "Band", m.band, new Vector3(c, 0.003f, 0f), new Vector3(0.22f, 0.012f, 2 * Half + 0.22f), shadows: false, repeat: 1.1f);
            Part(t, "Band", m.band, new Vector3(0f, 0.003f, c), new Vector3(2 * Half + 0.22f, 0.012f, 0.22f), shadows: false, repeat: 1.1f);
        }
        // The kerb round the plaza, a step above the street.
        foreach (int side in new[] { -1, 1 })
        {
            Part(t, "Kerb", m.kerb, new Vector3(side * (edge + 0.2f), -0.02f, 0f), new Vector3(0.4f, 0.16f, 2 * edge + 0.8f), repeat: 0.8f);
            Part(t, "Kerb", m.kerb, new Vector3(0f, -0.02f, side * (edge + 0.2f)), new Vector3(2 * edge + 0.8f, 0.16f, 0.4f), repeat: 0.8f);
        }
        Part(t, "Street", m.street, new Vector3(0f, -0.2f, 0f), new Vector3(160f, 0.2f, 160f), shadows: false, repeat: 2.2f);

        // The houses: a ring facing in, the corners left open as streets.
        var rng = new System.Random(7);
        float front = edge + 5.6f;
        var houses = new[] { heroes["HouseA"], heroes["HouseB"] };
        int k = 0;
        for (int side = 0; side < 4; side++)
        {
            var side0 = Quaternion.Euler(0f, side * 90f, 0f); // side 0 north, then east, south, west
            float x = -14.6f;
            while (x < 14.6f)
            {
                var hero = houses[k % 2];
                var h = (GameObject)PrefabUtility.InstantiatePrefab(hero);
                h.transform.SetParent(t, false);
                float s = 0.92f + (float)rng.NextDouble() * 0.16f;
                bool mirror = (k / 2) % 2 == 1;
                h.transform.localScale = new Vector3(mirror ? -s : s, s, s);
                var hb = Bounds(h); // still unturned: width along X, depth along Z
                float width = hb.size.x, depth = hb.size.z;
                h.transform.localPosition = side0 * new Vector3(x + width / 2f, 0f, front + depth / 2f);
                h.transform.localRotation = side0 * Quaternion.Euler(0f, 180f, 0f); // its front toward the square
                x += width + 0.25f;
                k++;
            }
        }
        // The fountain in the open north-west corner, the landmark.
        var fountain = (GameObject)PrefabUtility.InstantiatePrefab(heroes["Fountain"]);
        fountain.transform.SetParent(t, false);
        fountain.transform.localPosition = new Vector3(-(edge + 3.2f), 0f, edge + 3.2f);
        fountain.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
        // Street lamps round the kerb, lit at dusk.
        int n = 0;
        foreach (var (x, z) in new[] { (-1f, -1f), (1f, -1f), (-1f, 1f), (1f, 1f), (-1f, -0.4f), (1f, 0.4f), (0.4f, -1f), (-0.4f, 1f) })
        {
            var p = new Vector3(x * (edge + 0.75f), 0f, z * (edge + 0.75f));
            if (Mathf.Abs(x) < 1f) p.x = x * edge; if (Mathf.Abs(z) < 1f) p.z = z * edge;
            var lamp = Prop("street_lamp_02", t, p, Mathf.Atan2(-p.x, -p.z) * Mathf.Rad2Deg, height: 3.6f);
            lamp.name = "StreetLamp";
            var lb = Bounds(lamp);
            var l = AddLight(lamp.transform, "LampLight", LightType.Point, Vector3.zero, new Color(1f, 0.78f, 0.5f), 3f, 9f);
            l.transform.position = new Vector3(lb.center.x, lb.max.y - 0.35f, lb.center.z);
            l.enabled = false;
            n++;
        }
        // Planters, benches and stacked crates along the house fronts.
        foreach (var (x, z, yaw) in new[] { (-8f, 1f, 180f), (8.5f, 1f, 180f), (-9f, -1f, 0f), (7.5f, -1f, 0f) })
        {
            var p = Prop("planter_box_01", t, new Vector3(x, 0f, z * (front - 0.6f)), yaw, height: 0.75f);
            Prop("painted_wooden_bench", t, new Vector3(x + 2.2f, 0f, z * (front - 0.7f)), yaw, height: 0.85f);
        }
        foreach (var (x, z, yaw) in new[] { (1f, 1f, 90f), (-1f, -1f, 270f) })
        {
            Prop("planter_box_01", t, new Vector3(x * (front - 0.6f), 0f, 8.5f), yaw, height: 0.75f);
            Prop("potted_plant_04", t, new Vector3(x * (front - 0.5f), 0f, -6f), yaw, height: 0.9f);
        }
        foreach (var (x, z, yaw, stack) in new[] { (11.5f, 1f, 12f, 2), (-11.2f, -1f, -20f, 3), (1f, -1f, 6f, 2) })
            for (int i = 0; i < stack; i++)
                Prop("wooden_crate_02", t, new Vector3(x + (i == 2 ? 0.7f : 0f), i == 1 ? 0.5f : 0f, z * (front - 0.7f)), yaw + i * 9f, onFloor: i != 1, height: 0.5f);
        Prop("wooden_bucket_01", t, new Vector3(-edge - 1.5f, 0f, edge + 1.2f), 20f, height: 0.4f);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Square.prefab");
        Object.DestroyImmediate(root);
        Debug.Log($"[MarketKit] square: {k} houses, {n} lamps");
    }

    // ---- The agent: five rigged copies, one Animator each, the antenna and the basket ----

    const float AgentHeight = 1.2f;

    static readonly (string state, string file)[] Clips =
    {
        ("Walk", "agent_walk"), ("Look", "agent_look"), ("PickUp", "agent_pickup"), ("SentHome", "agent_senthome"),
    };

    static void BuildAgent(Mats m)
    {
        var mat = HeroMaterial("Agent", "agent", 2048, 2.2f, detailSize: 1024);
        // The team colour: the coral panels are the detail mask, the detail albedo is the colour (AgentRig).
        var team = Texture($"{HeroDir}/Agent/agent_team.png", 1024, false);
        mat.SetTexture("_DetailMask", team);
        mat.SetTexture("_DetailAlbedoMap", Swatch("team_neutral", new Color32(188, 188, 188, 255))); // 0.5 linear: no tint
        mat.SetFloat("_DetailAlbedoMapScale", 1f);
        mat.EnableKeyword("_DETAIL_MULX2");
        EditorUtility.SetDirty(mat);
        var root = new GameObject("Agent");
        foreach (var (state, file) in Clips)
        {
            string glb = $"{HeroDir}/Agent/{file}.glb";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(glb);
            if (source == null) { Debug.LogWarning($"[MarketKit] agent {state}: {glb} missing"); continue; }
            var clip = AssetDatabase.LoadAllAssetsAtPath(glb).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview"));
            var model = Object.Instantiate(source);
            model.name = state;
            model.transform.SetParent(root.transform, false);
            // The clip jobs re-rig the robot at 1.7 m; every copy is brought to the idle's 1.2 m.
            var mb = Bounds(model);
            if (mb.size.y > 0.01f) model.transform.localScale *= AgentHeight / mb.size.y;
            foreach (var r in model.GetComponentsInChildren<Renderer>()) { r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray(); r.shadowCastingMode = ShadowCastingMode.On; }
            foreach (var s in model.GetComponentsInChildren<SkinnedMeshRenderer>()) s.updateWhenOffscreen = false;
            foreach (var a in model.GetComponentsInChildren<Animation>()) Object.DestroyImmediate(a);
            var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (clip != null)
            {
                // Idle, walk and look loop; the buy and the dismay play once (AgentRig holds the last).
                var copy = Object.Instantiate(clip);
                copy.name = "agent_" + state.ToLowerInvariant();
                var settings = AnimationUtility.GetAnimationClipSettings(copy);
                settings.loopTime = state == "Idle" || state == "Walk" || state == "Look";
                AnimationUtility.SetAnimationClipSettings(copy, settings);
                string clipPath = $"{Generated}/Clips/agent_{state.ToLowerInvariant()}.anim";
                AssetDatabase.DeleteAsset(clipPath);
                AssetDatabase.CreateAsset(copy, clipPath);
                string controllerPath = $"{Generated}/agent_{state.ToLowerInvariant()}.controller";
                AssetDatabase.DeleteAsset(controllerPath);
                animator.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerPath, copy);
            }
            model.SetActive(state == "Look"); // the look-around, slowed, is the idle (AgentRig)
            Debug.Log($"[MarketKit] agent {state}: clip {(clip != null ? clip.name + " " + clip.length.ToString("F2") + " s, loop " + clip.isLooping : "MISSING")}");
        }
        var rig = root.AddComponent<AgentRig>();
        // The antenna: a graphite mast and the bulb (its size is the brain, its glow the effort).
        var antenna = new GameObject("Antenna").transform;
        antenna.SetParent(root.transform, false);
        var mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.DestroyImmediate(mast.GetComponent<Collider>());
        mast.name = "Mast"; mast.transform.SetParent(antenna, false);
        mast.transform.localPosition = new Vector3(0f, 0.075f, 0f); mast.transform.localScale = new Vector3(0.022f, 0.075f, 0.022f);
        mast.GetComponent<MeshRenderer>().sharedMaterial = m.metal;
        var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.DestroyImmediate(bulb.GetComponent<Collider>());
        bulb.name = "Bulb"; bulb.transform.SetParent(antenna, false);
        bulb.transform.localPosition = new Vector3(0f, 0.17f, 0f); bulb.transform.localScale = Vector3.one * 0.07f;
        bulb.GetComponent<MeshRenderer>().sharedMaterial = m.bulb;
        rig.antenna = antenna; rig.bulb = bulb.transform; rig.bulbRenderer = bulb.GetComponent<MeshRenderer>();
        // The basket on its back, what it buys inside.
        var basket = new GameObject("Basket").transform;
        basket.SetParent(root.transform, false);
        var wicker = Prop("wicker_basket_02", basket, Vector3.zero, 90f, onFloor: false, height: 0.22f);
        var wb = Bounds(wicker);
        wicker.transform.position += new Vector3(-wb.center.x, -wb.center.y, -wb.center.z) + basket.position;
        var goods = new GameObject("Goods").transform;
        goods.SetParent(basket, false);
        goods.localPosition = new Vector3(0f, -wb.size.y * 0.15f, 0f);
        rig.basket = basket; rig.goods = goods;
        var memory = new GameObject("Memory").transform;
        memory.SetParent(root.transform, false);
        rig.memory = memory;
        // The notebook it carries when its memory is a notebook (shown by RobotModel).
        var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(book.GetComponent<Collider>());
        book.name = "Notebook"; book.transform.SetParent(memory, false);
        book.transform.localPosition = new Vector3(0.27f, -0.62f, 0.04f); book.transform.localScale = new Vector3(0.03f, 0.2f, 0.15f); // at its side, under the arm (Memory rides the head)
        book.GetComponent<MeshRenderer>().sharedMaterial = m.notebook;
        book.SetActive(false);
        var b = Bounds(root);
        Debug.Log($"[MarketKit] agent bounds {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2}, min y {b.min.y:F2}");
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Agent.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- The skies: Unity's procedural sky, late morning and dusk ----

    static void BuildSkies()
    {
        var day = new Material(Shader.Find("Skybox/Procedural")) { name = "SkyDay" };
        day.SetFloat("_SunSize", 0.035f); day.SetFloat("_AtmosphereThickness", 0.8f);
        day.SetColor("_SkyTint", new Color(0.46f, 0.6f, 0.78f)); day.SetColor("_GroundColor", new Color(0.55f, 0.52f, 0.48f));
        day.SetFloat("_Exposure", 1.15f);
        Save(day, $"{Out}/SkyDay.mat");
        var dusk = new Material(Shader.Find("Skybox/Procedural")) { name = "SkyDusk" };
        dusk.SetFloat("_SunSize", 0.05f); dusk.SetFloat("_AtmosphereThickness", 1.6f);
        dusk.SetColor("_SkyTint", new Color(0.32f, 0.36f, 0.6f)); dusk.SetColor("_GroundColor", new Color(0.2f, 0.18f, 0.22f));
        dusk.SetFloat("_Exposure", 0.85f);
        Save(dusk, $"{Out}/SkyDusk.mat");
    }

    // ---- Small builders ----

    // A box with UVs in metres: one texture repeat every <repeat> m on every face.
    public static GameObject Part(Transform parent, string name, Material m, Vector3 centre, Vector3 size, bool shadows = true, float repeat = 2f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = centre;
        go.AddComponent<MeshFilter>().sharedMesh = BoxMesh(size, repeat);
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = m;
        r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        r.receiveShadows = true;
        return go;
    }

    static Mesh BoxMesh(Vector3 s, float repeat)
    {
        string key = $"box_{s.x:F3}_{s.y:F3}_{s.z:F3}_{repeat:F2}".Replace('.', 'p').Replace('-', 'm');
        string path = $"{Generated}/Meshes/{key}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        var h = s / 2f;
        var faces = new[]
        {
            (Vector3.forward, Vector3.left, Vector3.up), (Vector3.back, Vector3.right, Vector3.up),
            (Vector3.right, Vector3.forward, Vector3.up), (Vector3.left, Vector3.back, Vector3.up),
            (Vector3.up, Vector3.right, Vector3.forward), (Vector3.down, Vector3.right, Vector3.back),
        };
        foreach (var (n, u, v) in faces)
        {
            int b = verts.Count;
            float du = Mathf.Abs(Vector3.Dot(u, s)) / 2f, dv = Mathf.Abs(Vector3.Dot(v, s)) / 2f, dn = Mathf.Abs(Vector3.Dot(n, h));
            foreach (var (a, c) in new[] { (-1, -1), (1, -1), (1, 1), (-1, 1) })
            {
                var p = n * dn + u * (a * du) + v * (c * dv);
                verts.Add(p); norms.Add(n);
                uvs.Add(new Vector2((a * du + du) / repeat, (c * dv + dv) / repeat));
            }
            tris.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
        }
        var mesh = new Mesh { name = key };
        mesh.SetVertices(verts); mesh.SetNormals(norms); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    // A Poly Haven model, standing on the floor (or hanging at pos), turned by yaw, scaled to a height.
    static GameObject Prop(string asset, Transform parent, Vector3 pos, float yaw, bool onFloor = true, float height = 0f, float scale = 1f)
    {
        var path = Directory.GetFiles($"{PolyHaven}/{asset}", "*.fbx")[0].Replace('\\', '/');
        var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        model.transform.SetParent(parent, false);
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * model.transform.localRotation;
        model.transform.localScale *= scale;
        if (height > 0f)
        {
            var hb = Bounds(model);
            if (hb.size.y > 0.001f) model.transform.localScale *= height / hb.size.y;
        }
        model.transform.localPosition = pos;
        if (onFloor)
        {
            var b = Bounds(model);
            float floorWorld = parent.TransformPoint(new Vector3(0f, pos.y, 0f)).y;
            model.transform.position += Vector3.up * (floorWorld - b.min.y);
        }
        foreach (var c in model.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        return model;
    }

    static Light AddLight(Transform parent, string name, LightType type, Vector3 pos, Color c, float intensity, float range)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var l = go.AddComponent<Light>();
        l.type = type; l.color = c; l.intensity = intensity; l.range = range; l.shadows = LightShadows.None;
        l.renderMode = LightRenderMode.ForcePixel;
        return l;
    }
}
