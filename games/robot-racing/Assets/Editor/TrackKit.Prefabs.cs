using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// The prefabs Circuit places, in metres (a cell of the track is 2 m; Circuit places the kit at
// half scale): the robot car (the Higgsfield body with four Higgsfield wheels as their own
// objects, the front pair on steering pivots, a number roundel each side), the start gantry, the
// pit wall, the grandstand, a 2 m jersey barrier with its hazard band, a 2 m tyre wall with its
// belt, a corner stack of tyres, a 2 m kerb, a cone, three trees, grass clumps, a floodlight
// mast, the pit box's dressing, the finish line and its flags, the grass, the sky, and the
// holders of the road's and the paint's materials (Circuit makes those meshes from the grid).
//
// Bevelled shapes are made here as meshes with UVs in metres (extrusions, lathes, boxes), the
// way Claude Bomb's lab is primitives wearing textured Lit materials. Lights inside a prefab
// carry ranges in the world's units (cells), since the prefab is placed at half scale.
public static partial class TrackKit
{
    public const float CellM = 2f;

    class Mats
    {
        public Material road, pitlane, paint, kerb, barrier, belt, grass, bark, leaf, leafDark, grassCard, chequer, number, cone, steel, darkSteel, lamp, sky, concrete;
    }

    [MenuItem("Robot Racing/Build track kit")]
    public static void Build()
    {
        foreach (var d in new[] { Generated, Generated + "/Meshes", Out }) Directory.CreateDirectory(d);
        AssetDatabase.Refresh();
        SetUpModels();
        var m = new Mats();
        var asphaltN = Texture($"{PolyHaven}/asphalt_track/asphalt_track_nor_gl_1k.jpg", 1024, false, true);
        var concreteN = Texture($"{PolyHaven}/concrete/concrete_nor_gl_1k.jpg", 1024, false, true);
        m.road = Surface("asphalt_track", new Color(0.62f, 0.64f, 0.70f), Vector2.one, 2048, 1.15f, name: "road");
        m.pitlane = Surface("asphalt_02", new Color(0.56f, 0.58f, 0.62f), Vector2.one, 1024, name: "pit_lane");
        m.concrete = Surface("concrete", new Color(0.66f, 0.66f, 0.68f), Vector2.one, 512, name: "concrete");
        m.paint = Lit("paint", Texture($"{Generated}/paint_base.png", 512, true), asphaltN, null, null, new Color(0.64f, 0.64f, 0.63f), Color.black, null, 0.3f, 0.5f);
        m.kerb = Lit("kerb", Texture($"{Generated}/kerb_base.png", 1024, true), concreteN, null, null, Color.white, Color.black, null, 0.45f, 0.6f);
        m.barrier = Lit("barrier", Texture($"{Generated}/barrier_base.png", 1024, true), concreteN, null, null, Color.white, Color.black, null, 0.22f, 1f);
        m.belt = Lit("belt", Texture($"{Generated}/belt_base.png", 512, true), concreteN, null, null, Color.white, Color.black, null, 0.3f, 0.5f);
        m.chequer = Lit("chequer", Texture($"{Generated}/chequer_base.png", 512, true), asphaltN, null, null, Color.white, Color.black, null, 0.35f, 0.5f);
        m.cone = Lit("cone", Texture($"{Generated}/cone_base.png", 256, true), concreteN, null, null, Color.white, Color.black, null, 0.55f, 0.25f);
        m.number = Cutout(Lit("number", Texture($"{Generated}/number_base.png", 512, true, alpha: true), concreteN, null, null, Color.white, Color.black, null, 0.55f, 0.15f), false);
        m.steel = Surface("blue_metal_plate", new Color(0.55f, 0.58f, 0.62f), Vector2.one, 512, name: "steel");
        m.darkSteel = Surface("blue_metal_plate", new Color(0.16f, 0.17f, 0.2f), Vector2.one, 512, name: "dark_steel");
        m.bark = Surface("pine_bark", new Color(0.62f, 0.56f, 0.52f), Vector2.one, 512, name: "bark");
        // The grass: Poly Haven's sparse grass for the close detail, and a large-scale map of
        // greener and drier patches over it (URP Lit's detail albedo), so the field does not tile.
        m.grass = Surface("sparse_grass", new Color(0.62f, 0.78f, 0.46f), Vector2.one, 1024, 0.6f, name: "grass");
        m.grass.SetTexture("_DetailAlbedoMap", GroundVariation());
        m.grass.SetTextureScale("_DetailAlbedoMap", Vector2.one / 7f); // the mesh's UVs are in units of 4 m
        m.grass.SetFloat("_DetailAlbedoMapScale", 1f);
        m.grass.EnableKeyword("_DETAIL_MULX2");
        EditorUtility.SetDirty(m.grass);
        // The trees' leaves: a cluster of ambientCG leaves on a card, cut out, both faces lit.
        var leafBase = Texture($"{Generated}/leaf_cluster_base.png", 512, true, alpha: true);
        var leafN = Texture($"{Generated}/leaf_cluster_normal.png", 512, false, true);
        m.leaf = Cutout(Lit("leaf", leafBase, leafN, null, null, new Color(0.62f, 0.74f, 0.5f), Color.black, null, 0.25f, 0.8f));
        m.leafDark = Cutout(Lit("leaf_dark", leafBase, leafN, null, null, new Color(0.42f, 0.56f, 0.38f), Color.black, null, 0.2f, 0.8f));
        // Grass clumps: the clumps in Poly Haven's grass atlas, on crossed cards.
        var gdir = $"{PolyHaven}/grass_medium_01";
        m.grassCard = Cutout(Lit("grass_card", FoliageBase($"{gdir}/grass_medium_01_diff_1k.jpg", $"{gdir}/grass_medium_01_alpha_1k.jpg", "grass_medium_01", 512),
                                 Texture($"{gdir}/grass_medium_01_nor_gl_1k.jpg", 512, false, true), null, null, new Color(0.7f, 0.82f, 0.58f), Color.black, null, 0.2f, 0.6f));
        // A floodlight's lens: glowing warm white.
        m.lamp = Lit("flood_lens", Texture($"{Generated}/paint_base.png", 512, true), asphaltN, null, null, new Color(1f, 0.95f, 0.85f), new Color(1f, 0.86f, 0.62f) * 6f, null, 0.8f, 0.05f);
        m.sky = SkyMaterial();

        var heroes = BuildHeroes(m);
        BuildCar(m);
        BuildBarrier(m);
        BuildKerb(m);
        BuildTyres(m);
        BuildCone(m);
        BuildTrees(m);
        BuildGrass(m);
        BuildFlood(m);
        BuildPitBox(m);
        BuildFinish(m);
        BuildHolders(m);
        BuildSky(m);
        AssetDatabase.SaveAssets();
        Debug.Log("[TrackKit] built " + string.Join(", ", Directory.GetFiles(Out, "*.prefab", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension)));
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // ---- Heroes: one prefab each, scale fixed, pivot at the base centre, facing +Z ----

    static Dictionary<string, GameObject> BuildHeroes(Mats m)
    {
        var h = new Dictionary<string, GameObject>();
        // name, folder, file, texture size, yaw that turns its front to +Z, the dimension to fit (0 x, 1 y, 2 z after turning) and its size in metres
        h["Gantry"] = Hero("Gantry", "Gantry", "gantry", 1024, 90f, 0, 4.4f, emission: 4f, detail: 512);
        h["Grandstand"] = Hero("Grandstand", "Grandstand", "grandstand", 1024, 90f, 0, 9f, detail: 512);
        h["PitWall"] = PitWall(m);
        return h;
    }

    static GameObject Hero(string prefab, string folder, string file, int texSize, float yaw, int axis, float size, float emission = 0f, int detail = 0, bool save = true)
    {
        var glb = Directory.GetFiles($"{HeroDir}/{folder}", "*.glb")[0].Replace('\\', '/');
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(glb);
        var model = Object.Instantiate(source);
        model.name = "Model";
        var mat = HeroMaterial(folder, file, texSize, emission, detailSize: detail);
        foreach (var r in model.GetComponentsInChildren<Renderer>()) { r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray(); r.shadowCastingMode = ShadowCastingMode.On; }
        var root = new GameObject(prefab);
        model.transform.SetParent(root.transform, false);
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        var b = Bounds(model);
        float k = size / b.size[axis];
        model.transform.localScale *= k;
        b = Bounds(model);
        model.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
        b = Bounds(model);
        Debug.Log($"[TrackKit] hero {prefab}: {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2} m");
        if (!save) return root;
        var asset = PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/{prefab}.prefab");
        Object.DestroyImmediate(root);
        return asset;
    }

    // ---- The pit wall: the Higgsfield wall, its fence (which came out as mush) clipped off and
    // replaced by steel posts, a top rail and chain-link panels; the red band faces +Z ----

    static GameObject PitWall(Mats m)
    {
        var root = Hero("PitWall", "PitWall", "pit_wall", 1024, 90f, 0, 4f, detail: 512, save: false);
        const float top = 1.08f;
        foreach (var f in root.GetComponentsInChildren<MeshFilter>())
        {
            var src = f.sharedMesh; var v = src.vertices; var tris = src.triangles;
            var toRoot = root.transform.worldToLocalMatrix * f.transform.localToWorldMatrix;
            var keep = new List<int>();
            for (int i = 0; i < tris.Length; i += 3)
                if (toRoot.MultiplyPoint3x4(v[tris[i]]).y <= top && toRoot.MultiplyPoint3x4(v[tris[i + 1]]).y <= top && toRoot.MultiplyPoint3x4(v[tris[i + 2]]).y <= top)
                    keep.AddRange(new[] { tris[i], tris[i + 1], tris[i + 2] });
            var map = new Dictionary<int, int>(); var order = new List<int>();
            foreach (int i in keep) if (!map.ContainsKey(i)) { map[i] = order.Count; order.Add(i); }
            var nrm = src.normals; var tan = src.tangents; var uv0 = new List<Vector2>(); src.GetUVs(0, uv0);
            var mesh = new Mesh { name = "pit_wall_clean" };
            mesh.SetVertices(order.Select(i => v[i]).ToList());
            if (nrm.Length == v.Length) mesh.SetNormals(order.Select(i => nrm[i]).ToList());
            if (tan.Length == v.Length) mesh.SetTangents(order.Select(i => tan[i]).ToList());
            if (uv0.Count == v.Length) mesh.SetUVs(0, order.Select(i => uv0[i]).ToList());
            mesh.SetTriangles(keep.Select(i => map[i]).ToList(), 0);
            if (tan.Length != v.Length) mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            f.sharedMesh = SaveMesh(mesh, "pit_wall_clean");
            Debug.Log($"[TrackKit] pit wall: kept {keep.Count / 3} of {tris.Length / 3} triangles under the fence line");
        }
        var b = Bounds(root);
        // The fence: posts every metre, a top rail, chain-link between them.
        var cdir = $"{PolyHaven}/modular_chainlink_fence";
        var wire = Cutout(Lit("chainlink", FoliageBase($"{cdir}/modular_chainlink_fence_wire_diff_1k.jpg", $"{cdir}/modular_chainlink_fence_wire_opacity_1k.jpg", "chainlink", 512),
                              Texture($"{cdir}/modular_chainlink_fence_wire_nor_gl_1k.jpg", 512, false, true), null, null, new Color(0.75f, 0.77f, 0.8f), Color.black, null, 0.6f, 1f, 0.7f));
        float z = b.center.z - 0.1f, y0 = b.max.y - 0.02f, fh = 1.45f;
        for (int i = 0; i <= 4; i++)
            MeshObject(root.transform, "Post", Lathe("fence_post", new[] { new Vector2(0.03f, 0f), new Vector2(0.03f, fh), new Vector2(0f, fh + 0.02f) }, 8, 1f), m.steel).transform.localPosition = new Vector3(-1.95f + i * 0.975f, y0, z);
        var rail = MeshObject(root.transform, "Rail", BoxMesh("fence_rail", new Vector3(3.95f, 0.05f, 0.05f), 1f), m.steel);
        rail.transform.localPosition = new Vector3(0f, y0 + fh, z);
        var panel = MeshObject(root.transform, "Mesh", BoxMesh("fence_mesh", new Vector3(3.9f, fh - 0.04f, 0.004f), 1f), wire, shadows: false);
        panel.transform.localPosition = new Vector3(0f, y0 + fh / 2f, z);
        var asset = PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/PitWall.prefab");
        Object.DestroyImmediate(root);
        return asset;
    }

    // ---- The robot car: the body, four wheels that turn, the number, the rig ----

    public const float CarLength = 1.55f;         // metres, nose to tail
    const float WheelD = 0.17f * CarLength;       // the wheel's diameter: the arches are 0.28 m high
    const float FrontAxle = 0.5f - 0.25f;         // from the centre, in car lengths (+ toward the nose), measured on the arches
    const float RearAxle = -(0.5f - 0.155f);

    static void BuildCar(Mats m)
    {
        var root = new GameObject("Car");
        var body = new GameObject("Body").transform;
        body.SetParent(root.transform, false);
        var shell = Hero("CarBody", "Car", "robot_car", 2048, 90f, 2, CarLength, emission: 3.2f, detail: 1024, save: false);
        // Its paint keeps one smoothness (no metal in it): a satin finish, so the white wing does not flare under the lamps.
        var paint = shell.GetComponentInChildren<Renderer>().sharedMaterial;
        paint.SetFloat("_Smoothness", Mathf.Min(paint.GetFloat("_Smoothness"), 0.52f));
        EditorUtility.SetDirty(paint);
        var model = shell.transform.GetChild(0);
        // The image-to-3D closed the right front arch (its wheel would hide inside): the shell is
        // mirrored so the side with both arches open is the car's right, where the pit lane's camera stands.
        var mirror = new GameObject("Shell").transform;
        mirror.SetParent(body, false);
        mirror.localScale = new Vector3(-1f, 1f, 1f);
        model.SetParent(mirror, false);
        Object.DestroyImmediate(shell);
        model.localPosition += Vector3.up * 0.02f; // a little ground clearance
        var bb = Bounds(body.gameObject);
        float halfW = bb.extents.x;

        // The number roundel on each side, on the white panel: found by casting onto the body.
        var mf = model.GetComponentInChildren<MeshFilter>();
        var col = mf.gameObject.AddComponent<MeshCollider>();
        col.sharedMesh = mf.sharedMesh;
        Physics.SyncTransforms();
        foreach (int side in new[] { -1, 1 })
        {
            var from = new Vector3(side * (halfW + 0.5f), bb.min.y + bb.size.y * 0.3f, -0.07f * CarLength);
            if (Physics.Raycast(from, new Vector3(-side, 0f, 0f), out var hit, 2f))
            {
                var q = Quad(body, "Number", m.number, 0.2f, 0.2f);
                q.transform.position = hit.point + hit.normal * 0.004f;
                q.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            }
            else Debug.LogWarning("[TrackKit] the number's panel was not found on side " + side);
        }
        Object.DestroyImmediate(col);

        // The wheels: one model, four objects. Each sits on a steering pivot (the front pair
        // turn), and spins on its own transform about X.
        var wheelSrc = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Directory.GetFiles($"{HeroDir}/Wheel", "*.glb")[0].Replace('\\', '/')));
        var wheelMat = HeroMaterial("Wheel", "wheel", 1024, 0f, detailSize: 512);
        foreach (var r in wheelSrc.GetComponentsInChildren<Renderer>()) { r.sharedMaterials = r.sharedMaterials.Select(_ => wheelMat).ToArray(); r.shadowCastingMode = ShadowCastingMode.On; }
        var wb = Bounds(wheelSrc);
        float wk = WheelD / Mathf.Max(wb.size.y, wb.size.z);
        float wheelW = wb.size.x * wk;
        var wheels = new GameObject("Wheels").transform;
        wheels.SetParent(root.transform, false);
        var steer = new List<Transform>(); var spin = new List<Transform>();
        foreach (var (name, x, z) in new[] { ("FL", -1, FrontAxle), ("FR", 1, FrontAxle), ("RL", -1, RearAxle), ("RR", 1, RearAxle) })
        {
            var pivot = new GameObject("Wheel" + name).transform;
            pivot.SetParent(wheels, false);
            pivot.localPosition = new Vector3(x * (halfW - wheelW * 0.5f - 0.055f), WheelD / 2f, z * CarLength);
            var s = new GameObject("Spin").transform;
            s.SetParent(pivot, false);
            var w = Object.Instantiate(wheelSrc, s);
            w.name = "Model";
            w.transform.localScale = Vector3.one * wk;
            // The rim faces -X as made: the right-hand wheels turn round to show theirs.
            w.transform.localRotation = Quaternion.Euler(0f, x > 0 ? 180f : 0f, 0f);
            var c = Bounds(w);
            w.transform.position -= c.center - s.position;
            if (name[0] == 'F') steer.Add(pivot);
            spin.Add(s);
        }
        Object.DestroyImmediate(wheelSrc);

        var rig = root.AddComponent<CarRig>();
        rig.body = body;
        rig.steer = steer.ToArray();
        rig.spin = spin.ToArray();
        rig.wheelRadius = WheelD / 2f * 0.5f; // in the world's units: the car is placed at half scale

        // Its own small light on the nose: the eyes' glow on the asphalt (world units).
        var glow = new GameObject("EyeGlow").AddComponent<Light>();
        glow.transform.SetParent(body, false);
        glow.transform.localPosition = new Vector3(0f, 0.25f, CarLength * 0.55f);
        glow.type = LightType.Point; glow.color = new Color(0.45f, 0.85f, 1f); glow.intensity = 0.18f; glow.range = 0.8f; glow.shadows = LightShadows.None;

        var b = Bounds(root);
        Debug.Log($"[TrackKit] car {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2} m, wheels {WheelD:F2} m x {wheelW:F2} m, half width {halfW:F2}");
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Car.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- The barrier: a 2 m jersey barrier, its track face toward +Z ----

    // The profile, from the track-side foot over the top to the back foot (z across, y up),
    // with small bevels on the top corners; 1.828 m round, as the barrier texture is painted.
    static readonly Vector2[] Jersey =
    {
        new Vector2(0.30f, 0f), new Vector2(0.30f, 0.08f), new Vector2(0.26f, 0.25f), new Vector2(0.085f, 0.78f), new Vector2(0.07f, 0.80f),
        new Vector2(-0.07f, 0.80f), new Vector2(-0.085f, 0.78f), new Vector2(-0.26f, 0.25f), new Vector2(-0.30f, 0.08f), new Vector2(-0.30f, 0f),
    };

    static void BuildBarrier(Mats m)
    {
        var root = new GameObject("Barrier");
        var go = MeshObject(root.transform, "Jersey", Extrude("jersey_2m", Jersey, 2f - 0.02f, 2f, 1.828f, true), m.barrier);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Barrier.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- The kerb: 2 m of red and white, 0.35 m wide, raised toward the barrier ----

    static void BuildKerb(Mats m)
    {
        var root = new GameObject("Kerb");
        // Across the kerb from the road (z = +0.175, flush) to the barrier side (z = -0.175, 4 cm up).
        var profile = new[] { new Vector2(0.175f, 0.004f), new Vector2(0.12f, 0.022f), new Vector2(-0.15f, 0.04f), new Vector2(-0.175f, 0.03f), new Vector2(-0.18f, 0.0f) };
        MeshObject(root.transform, "Kerb", Extrude("kerb_2m", profile, 2f, 2f, 0.4f, false), m.kerb, shadows: false);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Kerb.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- Tyres: Poly Haven's old tyre, lying flat; a 2 m wall with its belt, a corner stack ----

    const float TyreH = 0.16f; // a tyre lying flat, 0.62 m across

    static GameObject Tyre(Transform parent, Vector3 pos, float yaw)
    {
        var path = Directory.GetFiles($"{PolyHaven}/old_tyre", "*.fbx")[0].Replace('\\', '/');
        var t = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        t.transform.SetParent(parent, false);
        var b = Bounds(t);
        // Lay it flat: its axle (the smallest side) up.
        int axle = b.size.x < b.size.y && b.size.x < b.size.z ? 0 : b.size.y < b.size.z ? 1 : 2;
        if (axle == 0) t.transform.localRotation = Quaternion.Euler(0f, 0f, 90f) * t.transform.localRotation;
        if (axle == 2) t.transform.localRotation = Quaternion.Euler(90f, 0f, 0f) * t.transform.localRotation;
        t.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * t.transform.localRotation;
        b = Bounds(t);
        float k = 0.62f / Mathf.Max(b.size.x, b.size.z);
        t.transform.localScale *= k;
        b = Bounds(t);
        t.transform.position += parent.TransformPoint(pos) - new Vector3(b.center.x, b.min.y, b.center.z);
        return t;
    }

    static void BuildTyres(Mats m)
    {
        var root = new GameObject("TyreWall");
        for (int i = 0; i < 3; i++)
            for (int row = 0; row < 2; row++)
                Tyre(root.transform, new Vector3(-0.64f + i * 0.64f + (row == 1 ? 0.02f : 0f), row * TyreH, -0.04f), i * 70f + row * 33f);
        // The belt across the stacks' track face, red and white.
        MeshObject(root.transform, "Belt", BoxMesh("belt", new Vector3(1.98f, 0.3f, 0.025f), 2f, 2f), m.belt).transform.localPosition = new Vector3(0f, TyreH + 0.02f, 0.29f);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/TyreWall.prefab");
        Object.DestroyImmediate(root);

        var stack = new GameObject("TyreStack");
        for (int row = 0; row < 4; row++) Tyre(stack.transform, new Vector3(0f, row * TyreH, 0f), row * 47f);
        PrefabUtility.SaveAsPrefabAsset(stack, $"{Out}/TyreStack.prefab");
        Object.DestroyImmediate(stack);
    }

    // ---- A cone: 0.5 m, orange with its white band, on a square black foot ----

    static void BuildCone(Mats m)
    {
        var root = new GameObject("Cone");
        var profile = new[] { new Vector2(0.15f, 0.03f), new Vector2(0.135f, 0.06f), new Vector2(0.03f, 0.48f), new Vector2(0.018f, 0.5f), new Vector2(0f, 0.505f) };
        MeshObject(root.transform, "Cone", Lathe("cone", profile, 20, 0.5f), m.cone);
        MeshObject(root.transform, "Foot", BoxMesh("cone_foot", new Vector3(0.34f, 0.03f, 0.34f), 0.5f, 0.5f), m.cone).transform.localPosition = new Vector3(0f, 0.015f, 0f);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Cone.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- Trees: a bark trunk with branches, a crown of leaf cards lit as one volume ----

    static void BuildTrees(Mats m)
    {
        BuildTree("TreeA", m, 11, 7.5f, 2.6f, 26);
        BuildTree("TreeB", m, 23, 6f, 2.1f, 22);
        BuildTree("TreeC", m, 37, 9f, 2.9f, 30);
    }

    static void BuildTree(string name, Mats m, int seed, float height, float crown, int cards)
    {
        var rnd = new System.Random(seed);
        float R() => (float)rnd.NextDouble();
        var root = new GameObject(name);
        float trunkH = height * 0.45f;
        var trunk = new[] { new Vector2(0.24f, 0f), new Vector2(0.17f, 0.3f), new Vector2(0.13f, trunkH * 0.6f), new Vector2(0.09f, trunkH), new Vector2(0.05f, height * 0.75f), new Vector2(0f, height * 0.78f) };
        MeshObject(root.transform, "Trunk", Lathe(name + "_trunk", trunk, 9, 1.2f), m.bark);
        // Branches: thin lathes leaning out from the trunk's top.
        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f + R() * 40f, len = crown * (0.6f + R() * 0.3f);
            var br = MeshObject(root.transform, "Branch", Lathe("branch", new[] { new Vector2(0.06f, 0f), new Vector2(0.025f, 1f), new Vector2(0f, 1.02f) }, 6, 1f), m.bark);
            br.transform.localPosition = new Vector3(0f, trunkH * (0.85f + R() * 0.25f), 0f);
            br.transform.localRotation = Quaternion.Euler(0f, a, 0f) * Quaternion.Euler(50f + R() * 15f, 0f, 0f);
            br.transform.localScale = new Vector3(1f, len, 1f);
        }
        // The crown: cards on an ellipsoid, their normals pointing out from its centre.
        var centre = new Vector3(0f, trunkH + crown * 0.85f, 0f);
        var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        for (int i = 0; i < cards; i++)
        {
            var dir = Random3(rnd);
            dir.y = dir.y * 0.8f + 0.1f;
            var at = centre + Vector3.Scale(dir.normalized * (0.45f + R() * 0.55f), new Vector3(crown, crown * 0.9f, crown));
            float size = crown * (0.75f + R() * 0.45f);
            var facing = (Random3(rnd) * 0.6f + (at - centre).normalized).normalized;
            var rot = Quaternion.LookRotation(facing) * Quaternion.Euler(0f, 0f, R() * 360f);
            int b = verts.Count;
            foreach (var (u, v) in new[] { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) })
            {
                var p = at + rot * new Vector3((u - 0.5f) * size, (v - 0.5f) * size, 0f);
                verts.Add(p);
                norms.Add(((p - centre).normalized * 0.8f + Vector3.up * 0.2f).normalized);
                uvs.Add(new Vector2(u, v));
            }
            tris.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
        }
        var mesh = new Mesh { name = name + "_crown" };
        mesh.SetVertices(verts); mesh.SetNormals(norms); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        MeshObject(root.transform, "Crown", SaveMesh(mesh, name + "_crown"), seed % 2 == 0 ? m.leafDark : m.leaf);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/{name}.prefab");
        Object.DestroyImmediate(root);
    }

    static Vector3 Random3(System.Random r)
    {
        Vector3 v;
        do v = new Vector3((float)r.NextDouble() * 2 - 1, (float)r.NextDouble() * 2 - 1, (float)r.NextDouble() * 2 - 1); while (v.sqrMagnitude > 1f || v.sqrMagnitude < 0.01f);
        return v.normalized;
    }

    // ---- Grass clumps: three crossed cards with one of the atlas's clumps ----

    // The clumps in grass_medium_01's atlas: u0, u1, v0, v1 (Unity's v, from the bottom).
    static readonly Vector4[] Clumps =
    {
        new Vector4(0.195f, 0.49f, 0.116f, 0.258f), new Vector4(0.586f, 0.79f, 0.145f, 0.243f),
        new Vector4(0.03f, 0.195f, 0.005f, 0.129f), new Vector4(0.25f, 0.49f, 0.004f, 0.111f), new Vector4(0.576f, 0.786f, 0.004f, 0.106f),
    };

    static void BuildGrass(Mats m)
    {
        for (int c = 0; c < 3; c++)
        {
            var rect = Clumps[c * 2 % Clumps.Length];
            var root = new GameObject("Grass" + (char)('A' + c));
            var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
            float aspect = (rect.y - rect.x) / (rect.w - rect.z);
            float h = 0.42f + c * 0.08f, w = h * aspect;
            for (int k = 0; k < 3; k++)
            {
                var rot = Quaternion.Euler(0f, k * 60f + c * 17f, 0f);
                int b = verts.Count;
                foreach (var (u, v) in new[] { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) })
                {
                    verts.Add(rot * new Vector3((u - 0.5f) * w, v * h, 0f));
                    norms.Add((Vector3.up * 0.8f + rot * Vector3.forward * 0.2f).normalized);
                    uvs.Add(new Vector2(Mathf.Lerp(rect.x, rect.y, u), Mathf.Lerp(rect.z, rect.w, v)));
                }
                tris.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
            }
            var mesh = new Mesh { name = root.name };
            mesh.SetVertices(verts); mesh.SetNormals(norms); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
            mesh.RecalculateTangents(); mesh.RecalculateBounds();
            MeshObject(root.transform, "Cards", SaveMesh(mesh, "grass_" + c), m.grassCard, shadows: false);
            PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/{root.name}.prefab");
            Object.DestroyImmediate(root);
        }
    }

    // ---- A floodlight mast: a steel pole, a head of four lamps aimed down at the track ----

    static void BuildFlood(Mats m)
    {
        var root = new GameObject("Floodlight");
        const float H = 9f;
        MeshObject(root.transform, "Pole", Lathe("flood_pole", new[] { new Vector2(0.16f, 0f), new Vector2(0.12f, 0.4f), new Vector2(0.09f, H), new Vector2(0f, H + 0.02f) }, 10, 1f), m.steel);
        MeshObject(root.transform, "Base", BoxMesh("flood_base", new Vector3(0.7f, 0.25f, 0.7f), 1f, 1f), m.concrete).transform.localPosition = new Vector3(0f, 0.125f, 0f);
        var head = new GameObject("Head").transform;
        head.SetParent(root.transform, false);
        head.localPosition = new Vector3(0f, H, 0.15f);
        head.localRotation = Quaternion.Euler(35f, 0f, 0f); // aimed down toward +Z
        MeshObject(head, "Frame", BoxMesh("flood_frame", new Vector3(1.5f, 0.9f, 0.12f), 1f, 1f), m.darkSteel).transform.localPosition = new Vector3(0f, 0f, -0.06f);
        for (int i = 0; i < 4; i++)
        {
            var lamp = new GameObject("Lamp").transform;
            lamp.SetParent(head, false);
            lamp.localPosition = new Vector3(-0.36f + (i % 2) * 0.72f, -0.2f + (i / 2) * 0.4f, 0.02f);
            MeshObject(lamp, "Housing", BoxMesh("flood_housing", new Vector3(0.6f, 0.34f, 0.22f), 1f, 1f), m.darkSteel).transform.localPosition = new Vector3(0f, 0f, 0.11f);
            MeshObject(lamp, "Lens", BoxMesh("flood_lens", new Vector3(0.5f, 0.26f, 0.02f), 1f, 1f), m.lamp, shadows: false).transform.localPosition = new Vector3(0f, 0f, 0.225f);
        }
        // The light itself, in the world's units (the mast is placed at half scale).
        var l = new GameObject("Light").AddComponent<Light>();
        l.transform.SetParent(head, false);
        l.transform.localPosition = new Vector3(0f, 0f, 0.4f);
        l.type = LightType.Spot; l.color = new Color(1f, 0.88f, 0.7f); l.intensity = 4f; l.range = 26f; l.spotAngle = 72f; l.innerSpotAngle = 40f; l.shadows = LightShadows.None;
        l.renderMode = LightRenderMode.ForcePixel;
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Floodlight.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- The pit box: what stands behind the pit wall by the grid (4 m x 1.6 m, facing +Z) ----

    static void BuildPitBox(Mats m)
    {
        var root = new GameObject("PitBox");
        var t = root.transform;
        Prop("metal_tool_chest", t, new Vector3(-1.5f, 0f, -0.3f), 180f, 1f);
        Prop("industrial_storage_cart", t, new Vector3(-0.45f, 0f, -0.2f), 170f, 1f);
        Prop("tire_pump", t, new Vector3(0.35f, 0f, 0.1f), 30f, 1f);
        Prop("korean_fire_extinguisher_01", t, new Vector3(0.75f, 0f, -0.45f), 200f, 1f);
        Prop("Barrel_01", t, new Vector3(1.85f, 0f, -0.45f), 0f, 1f);
        for (int i = 0; i < 4; i++) Tyre(t, new Vector3(1.1f + (i / 2) * 0.68f, (i % 2) * TyreH, 0.35f), i * 40f);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/PitBox.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- The finish: the chequered line (2 m across, 0.5 m deep) and a flag on a pole ----

    static void BuildFinish(Mats m)
    {
        var line = new GameObject("FinishLine");
        var q = Quad(line.transform, "Chequer", m.chequer, 2f, 0.5f);
        q.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        q.transform.localPosition = new Vector3(0f, 0.006f, 0f);
        q.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        PrefabUtility.SaveAsPrefabAsset(line, $"{Out}/FinishLine.prefab");
        Object.DestroyImmediate(line);

        var flag = new GameObject("FinishFlag");
        MeshObject(flag.transform, "Pole", Lathe("flag_pole", new[] { new Vector2(0.04f, 0f), new Vector2(0.03f, 2.6f), new Vector2(0f, 2.62f) }, 8, 1f), m.steel);
        var f = Quad(flag.transform, "Flag", m.chequer, 0.9f, 0.6f);
        f.transform.localPosition = new Vector3(0.48f, 2.25f, 0f);
        var mr = f.GetComponent<MeshRenderer>();
        var two = new Material(m.chequer) { name = "chequer_flag" };
        two.SetFloat("_Cull", 0f);
        mr.sharedMaterial = Save(two, $"{Generated}/chequer_flag.mat");
        PrefabUtility.SaveAsPrefabAsset(flag, $"{Out}/FinishFlag.prefab");
        Object.DestroyImmediate(flag);
    }

    // ---- What Circuit fills in: the road, the paint, the pit lane and the grass carry their materials ----

    static void BuildHolders(Mats m)
    {
        foreach (var (name, mat, shadows) in new[] { ("Road", m.road, false), ("Paint", m.paint, false), ("PitLane", m.pitlane, false), ("Ground", m.grass, false) })
        {
            var go = new GameObject(name);
            go.AddComponent<MeshFilter>();
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat; r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off; r.receiveShadows = true;
            PrefabUtility.SaveAsPrefabAsset(go, $"{Out}/{name}.prefab");
            Object.DestroyImmediate(go);
        }
    }

    // ---- The sky: a dome with the game's gradient shader (coloured at run time) ----

    static Material SkyMaterial()
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Resources/Shaders/Sky.shader");
        var m = new Material(shader) { name = "sky" };
        return Save(m, $"{Generated}/sky.mat");
    }

    static void BuildSky(Mats m)
    {
        var root = new GameObject("Sky");
        const int seg = 32, rings = 16;
        var verts = new List<Vector3>(); var tris = new List<int>(); var uvs = new List<Vector2>();
        for (int r = 0; r <= rings; r++)
        {
            float lat = Mathf.Lerp(-90f, 90f, r / (float)rings) * Mathf.Deg2Rad;
            for (int s = 0; s <= seg; s++)
            {
                float lon = s / (float)seg * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(lat) * Mathf.Cos(lon), Mathf.Sin(lat), Mathf.Cos(lat) * Mathf.Sin(lon)) * 0.5f);
                uvs.Add(new Vector2(s / (float)seg, r / (float)rings));
            }
        }
        for (int r = 0; r < rings; r++)
            for (int s = 0; s < seg; s++)
            {
                int a = r * (seg + 1) + s, b = a + seg + 1;
                tris.AddRange(new[] { a, a + 1, b, a + 1, b + 1, b });
            }
        // Wound to face outward: the Sky shader culls front faces and draws the inside.
        var mesh = new Mesh { name = "sky_dome" };
        mesh.SetVertices(verts); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
        mesh.SetNormals(verts.Select(v => v.normalized).ToList());
        FixWinding(mesh);
        mesh.RecalculateBounds();
        var go = MeshObject(root.transform, "Dome", SaveMesh(mesh, "sky_dome"), m.sky, shadows: false);
        go.GetComponent<MeshRenderer>().receiveShadows = false;
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Sky.prefab");
        Object.DestroyImmediate(root);
    }

    // The ground's large-scale variation: greener and drier patches round a mid grey (URP's
    // detail albedo multiplies by twice the map), 256 px over 28 m.
    static Texture2D GroundVariation()
    {
        string path = $"{Generated}/ground_variation.png";
        if (!File.Exists(path))
        {
            const int S = 256;
            var px = new Color32[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.025f, y * 0.025f) * 0.65f + Mathf.PerlinNoise(x * 0.07f + 31f, y * 0.07f + 7f) * 0.35f;
                    float dry = Mathf.Clamp01((n - 0.45f) * 2.2f), lush = Mathf.Clamp01((0.42f - n) * 2.5f);
                    var c = new Color(0.5f, 0.5f, 0.5f);
                    c = Color.Lerp(c, new Color(0.62f, 0.55f, 0.4f), dry * 0.8f);
                    c = Color.Lerp(c, new Color(0.4f, 0.5f, 0.38f), lush * 0.8f);
                    px[y * S + x] = c;
                }
            WritePng(path, S, S, px);
        }
        return Texture(path, 256, true);
    }

    // ---- Small builders ----

    static Mesh SaveMesh(Mesh mesh, string key)
    {
        string path = $"{Generated}/Meshes/{key}.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    static GameObject MeshObject(Transform parent, string name, Mesh mesh, Material m, bool shadows = true)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = m;
        r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        return go;
    }

    // A flat card facing -Z (so it reads from +Z... turned by its caller), UV 0..1.
    static GameObject Quad(Transform parent, string name, Material m, float w, float h)
    {
        string key = $"quad_{w:F2}_{h:F2}".Replace('.', 'p');
        string path = $"{Generated}/Meshes/{key}.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null)
        {
            mesh = new Mesh { name = key };
            mesh.SetVertices(new List<Vector3> { new Vector3(-w / 2, -h / 2, 0), new Vector3(w / 2, -h / 2, 0), new Vector3(w / 2, h / 2, 0), new Vector3(-w / 2, h / 2, 0) });
            mesh.SetNormals(Enumerable.Repeat(Vector3.back, 4).ToList());
            mesh.SetUVs(0, new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            mesh.SetTriangles(new[] { 0, 2, 1, 0, 3, 2 }, 0);
            mesh.RecalculateTangents(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
        }
        return MeshObject(parent, name, mesh, m, shadows: false);
    }

    // A profile (z across, y up) extruded along X, hard edges between its segments, UVs in
    // metres: u along the length over uRepeat, v round the profile over vLength. Caps optional.
    static Mesh Extrude(string key, Vector2[] profile, float length, float uRepeat, float vLength, bool caps)
    {
        var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        float s = 0f, x0 = -length / 2f, x1 = length / 2f;
        for (int i = 0; i < profile.Length - 1; i++)
        {
            var a = profile[i]; var b = profile[i + 1];
            float seg = Vector2.Distance(a, b);
            var d = (b - a).normalized;
            var n = new Vector3(0f, -d.x, d.y); // outward for a profile running over the top from +z to -z
            if (Vector3.Dot(n, new Vector3(0f, (a.y + b.y) / 2f, (a.x + b.x) / 2f)) < 0f) n = -n;
            int v0 = verts.Count;
            verts.Add(new Vector3(x0, a.y, a.x)); verts.Add(new Vector3(x1, a.y, a.x)); verts.Add(new Vector3(x1, b.y, b.x)); verts.Add(new Vector3(x0, b.y, b.x));
            for (int k = 0; k < 4; k++) norms.Add(n);
            uvs.Add(new Vector2(x0 / uRepeat + 0.5f, s / vLength)); uvs.Add(new Vector2(x1 / uRepeat + 0.5f, s / vLength));
            uvs.Add(new Vector2(x1 / uRepeat + 0.5f, (s + seg) / vLength)); uvs.Add(new Vector2(x0 / uRepeat + 0.5f, (s + seg) / vLength));
            tris.AddRange(new[] { v0, v0 + 2, v0 + 1, v0, v0 + 3, v0 + 2 });
            s += seg;
        }
        if (caps)
            foreach (int side in new[] { -1, 1 })
            {
                int c0 = verts.Count;
                float x = side * length / 2f;
                var centre = new Vector2(profile.Average(p => p.x), profile.Average(p => p.y));
                verts.Add(new Vector3(x, centre.y, centre.x)); norms.Add(new Vector3(side, 0, 0)); uvs.Add(new Vector2(0.5f + centre.x / uRepeat, centre.y / vLength));
                foreach (var p in profile) { verts.Add(new Vector3(x, p.y, p.x)); norms.Add(new Vector3(side, 0, 0)); uvs.Add(new Vector2(0.5f + p.x / uRepeat, p.y / vLength)); }
                for (int i = 0; i < profile.Length - 1; i++)
                {
                    if (side > 0) tris.AddRange(new[] { c0, c0 + 1 + i, c0 + 2 + i });
                    else tris.AddRange(new[] { c0, c0 + 2 + i, c0 + 1 + i });
                }
                // close the bottom of the cap back to the first point
                if (side > 0) tris.AddRange(new[] { c0, c0 + profile.Length, c0 + 1 }); else tris.AddRange(new[] { c0, c0 + 1, c0 + profile.Length });
            }
        var mesh = new Mesh { name = key };
        mesh.SetVertices(verts); mesh.SetNormals(norms); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
        // Wind every face toward its normal.
        FixWinding(mesh);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        return SaveMesh(mesh, key);
    }

    // A profile (radius, height) turned round Y, smooth round, UVs in metres.
    static Mesh Lathe(string key, Vector2[] profile, int segments, float repeat)
    {
        var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        float s = 0f;
        float maxR = profile.Max(p => p.x);
        for (int i = 0; i < profile.Length; i++)
        {
            if (i > 0) s += Vector2.Distance(profile[i - 1], profile[i]);
            var tangent = (i < profile.Length - 1 ? profile[i + 1] - profile[i] : profile[i] - profile[i - 1]).normalized;
            var n2 = new Vector2(tangent.y, -tangent.x); // outward (radius grows outward)
            for (int k = 0; k <= segments; k++)
            {
                float a = k / (float)segments * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                verts.Add(dir * profile[i].x + Vector3.up * profile[i].y);
                norms.Add((dir * n2.x + Vector3.up * n2.y).normalized);
                uvs.Add(new Vector2(k / (float)segments * Mathf.Max(1f, Mathf.Round(2f * Mathf.PI * maxR / repeat)), s / repeat));
            }
        }
        for (int i = 0; i < profile.Length - 1; i++)
            for (int k = 0; k < segments; k++)
            {
                int a = i * (segments + 1) + k, b = a + segments + 1;
                tris.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
            }
        var mesh = new Mesh { name = key };
        mesh.SetVertices(verts); mesh.SetNormals(norms); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
        FixWinding(mesh);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        return SaveMesh(mesh, key);
    }

    // Each triangle wound so its face points along its vertices' normals.
    static void FixWinding(Mesh mesh)
    {
        var v = mesh.vertices; var n = mesh.normals; var t = mesh.triangles;
        for (int i = 0; i < t.Length; i += 3)
        {
            var face = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
            // Unity's front faces wind clockwise as seen from the front: Cross(b - a, c - a) points at the viewer.
            if (Vector3.Dot(face, n[t[i]] + n[t[i + 1]] + n[t[i + 2]]) < 0f) { int k = t[i + 1]; t[i + 1] = t[i + 2]; t[i + 2] = k; }
        }
        mesh.triangles = t;
    }

    // A box with UVs in metres: one texture repeat every <repeat> m on every face.
    static Mesh BoxMesh(string key, Vector3 s, float repeat, float _ = 0f)
    {
        string path = $"{Generated}/Meshes/box_{key}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) AssetDatabase.DeleteAsset(path);
        var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        var faces = new[]
        {
            (Vector3.forward, Vector3.left, Vector3.up), (Vector3.back, Vector3.right, Vector3.up),
            (Vector3.right, Vector3.forward, Vector3.up), (Vector3.left, Vector3.back, Vector3.up),
            (Vector3.up, Vector3.right, Vector3.forward), (Vector3.down, Vector3.right, Vector3.back),
        };
        var h = s / 2f;
        foreach (var (n, u, v) in faces)
        {
            int b = verts.Count;
            float du = Mathf.Abs(Vector3.Dot(u, s)) / 2f, dv = Mathf.Abs(Vector3.Dot(v, s)) / 2f, dn = Mathf.Abs(Vector3.Dot(n, h));
            foreach (var (a, c) in new[] { (-1, -1), (1, -1), (1, 1), (-1, 1) })
            {
                verts.Add(n * dn + u * (a * du) + v * (c * dv)); norms.Add(n);
                uvs.Add(new Vector2((a * du + du) / repeat, (c * dv + dv) / repeat));
            }
            tris.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
        }
        var mesh = new Mesh { name = "box_" + key };
        mesh.SetVertices(verts); mesh.SetNormals(norms); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
        FixWinding(mesh);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    // A Poly Haven model, standing on the floor (or at pos when not), turned by yaw, scaled.
    static GameObject Prop(string asset, Transform parent, Vector3 pos, float yaw, float scale, bool onFloor = true)
    {
        var path = Directory.GetFiles($"{PolyHaven}/{asset}", "*.fbx")[0].Replace('\\', '/');
        var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        model.transform.SetParent(parent, false);
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * model.transform.localRotation;
        model.transform.localScale *= scale;
        model.transform.localPosition = pos;
        if (onFloor)
        {
            var b = Bounds(model);
            float floorWorld = parent.TransformPoint(Vector3.zero).y;
            model.transform.position += Vector3.up * (floorWorld - b.min.y);
        }
        return model;
    }
}
