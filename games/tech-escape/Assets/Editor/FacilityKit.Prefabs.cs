using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

// The prefabs Building places: the hero models normalised (scale in metres, pivot at the
// base centre, facing +Z), a door, a corridor bay with its two door openings, the
// corridor's two ends, the Suspicious Inbox room (the pilot) and the attacker.
//
// Architecture is boxes with UVs in metres (one texture repeat every 2.1 m, so a 4.2 m bay
// tiles seamlessly), the way Claude Bomb's lab is quads and cubes wearing textured Lit
// materials. Everything is lit by URP Lit; nothing here uses the Flat shader or MeshKit.
public static partial class FacilityKit
{
    public const float H = 2.8f;              // floor to ceiling
    public const float Pitch = 4.2f, Width = 5.5f, Wall = 0.2f;
    public const float DoorW = 1.2f, DoorH = 2.2f;
    const float Repeat = 2.1f;

    class Mats
    {
        public Material floor, roomFloor, labFloor, wall, roomWall, labWall, ceiling, metal, trim, frame, plate, lens, screen, ground, poster, beacon, panel;
        public Material[] boards;
    }

    [MenuItem("Tech Escape/Build facility kit")]
    public static void Build()
    {
        foreach (var d in new[] { Generated, Generated + "/Meshes", Out, Out + "/Hero" }) Directory.CreateDirectory(d);
        AssetDatabase.Refresh();
        SetUpModels();
        var m = new Mats
        {
            floor = Surface("concrete_floor_worn_001", new Color(0.46f, 0.49f, 0.56f), Vector2.one, name: "floor_corridor"),
            roomFloor = Surface("concrete_floor_worn_001", new Color(0.56f, 0.56f, 0.6f), Vector2.one, name: "floor_room"),
            wall = Surface("plastered_wall_04", new Color(0.42f, 0.47f, 0.56f), Vector2.one, name: "wall_corridor"),
            roomWall = Surface("plastered_wall_04", new Color(0.55f, 0.6f, 0.68f), Vector2.one, name: "wall_room"),
            labWall = Surface("plastered_wall_04", new Color(0.46f, 0.55f, 0.58f), Vector2.one, name: "wall_lab"),
            labFloor = Surface("concrete_floor_worn_001", new Color(0.5f, 0.54f, 0.6f), Vector2.one, name: "floor_lab"),
            ceiling = Surface("ceiling_interior", new Color(0.24f, 0.26f, 0.3f), Vector2.one, bump: 0.15f, name: "ceiling"),
            metal = Surface("blue_metal_plate", new Color(0.34f, 0.37f, 0.43f), Vector2.one, name: "metal"),
            trim = Surface("blue_metal_plate", new Color(0.13f, 0.14f, 0.18f), Vector2.one, name: "trim"),
            frame = Surface("blue_metal_plate", new Color(0.2f, 0.42f, 0.44f), Vector2.one, name: "frame"),
            plate = Surface("blue_metal_plate", new Color(0.86f, 0.89f, 0.92f), Vector2.one, bump: 0.3f, name: "sign_plate"),
            ground = Surface("concrete_floor_worn_001", new Color(0.16f, 0.18f, 0.24f), Vector2.one, name: "ground"),
        };
        // The lens of a door's status lamp: brushed metal behind a coloured glow, coloured per door at run time.
        m.lens = Lit("lamp_lens", Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_diff_1k.jpg", 256, true),
                     Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_nor_gl_1k.jpg", 256, false, true), null, null,
                     new Color(0.9f, 0.3f, 0.25f), new Color(0.84f, 0.27f, 0.24f) * 2.2f, null, 0.8f, 0.2f);
        m.screen = ScreenMaterial();
        m.boards = BoardMaterials();
        m.beacon = Lit("beacon_amber", Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_diff_1k.jpg", 256, true),
                       Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_nor_gl_1k.jpg", 256, false, true), null, null,
                       new Color(1f, 0.55f, 0.2f), new Color(1f, 0.42f, 0.08f) * 3f, null, 0.9f, 0.1f);
        // A recessed ceiling panel's diffuser, off in lockdown but catching the light.
        m.panel = Lit("ceiling_panel", Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_diff_1k.jpg", 256, true),
                      Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_nor_gl_1k.jpg", 256, false, true), null, null,
                      new Color(0.78f, 0.82f, 0.88f), new Color(0.6f, 0.68f, 0.82f) * 0.7f, null, 0.7f, 0.05f);
        // The corridor's concrete is polished: one smoothness for the floor, so the door lamps shine in it.
        m.floor.DisableKeyword("_METALLICSPECGLOSSMAP"); m.floor.SetTexture("_MetallicGlossMap", null);
        m.floor.SetFloat("_Metallic", 0f); m.floor.SetFloat("_Smoothness", 0.62f);
        EditorUtility.SetDirty(m.floor);
        // The board's lamps, lit: teal when a row is decided, amber while one is still open.
        foreach (var (name, c) in new[] { ("LampDecided", new Color(0.086f, 0.76f, 0.69f)), ("LampUndecided", new Color(0.88f, 0.61f, 0.1f)) })
        {
            var lit = Lit("lamp_" + name.ToLowerInvariant(), Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_diff_1k.jpg", 256, true),
                          Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_nor_gl_1k.jpg", 256, false, true), null, null,
                          Color.Lerp(c, Color.white, 0.2f), c * 2.6f, null, 0.85f, 0.1f);
            var copy = new Material(lit);
            copy.EnableKeyword("_EMISSION");
            copy.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            Save(copy, $"{Out}/{name}.mat");
        }
        m.poster = PosterMaterial();

        var heroes = BuildHeroes();
        BuildLamp(m);
        BuildDoor(m, heroes["Door"]);
        BuildBay(m, "CorridorBay", 0);
        BuildBay(m, "CorridorBayB", 1);
        BuildBay(m, "CorridorBayC", 2);
        BuildEntrance(m);
        BuildEnd(m, heroes["Door"]);
        BuildGround(m);
        BuildRooms(m, heroes);
        BuildAttacker();
        AssetDatabase.SaveAssets();
        Debug.Log("[FacilityKit] built " + string.Join(", ", Directory.GetFiles(Out, "*.prefab", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension)));
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    // ---- Heroes: one prefab each, scale fixed, pivot at the base centre, facing +Z ----

    static Dictionary<string, GameObject> BuildHeroes()
    {
        var h = new Dictionary<string, GameObject>();
        // name, folder, file, texture size, yaw that turns its front to +Z, the dimension to fit (0 x, 1 y, 2 z after turning) and its size
        h["MailSorter"] = Hero("MailSorter", "mail_sorter", 2048, 90f, 0, 1.22f, detail: 1024);
        h["ServerRack"] = Hero("ServerRack", "server_rack", 1024, 90f, 1, 1.95f, emission: 3.5f, detail: 512);
        h["Monitor"] = Hero("Monitor", "monitor", 1024, 90f, 0, 0.56f, detail: 512);
        h["Chair"] = Hero("Chair", "chair", 1024, 90f, 1, 1.02f, detail: 512);
        h["Door"] = Hero("Door", "door", 1024, 90f, 1, DoorH - 0.06f, clipThickness: 0.06f, detail: 512);
        // The rooms' props (each room's hero) and the dressing every room shares.
        h["RiskBoard"] = Hero("RiskBoard", "risk_board", 1024, 90f, 0, 1.5f, detail: 512);
        h["Safe"] = Hero("Safe", "safe", 1024, 90f, 1, 1.0f, emission: 2.5f, detail: 512);
        h["Containment"] = Hero("Containment", "containment", 1024, 90f, 0, 1.0f, emission: 3f, detail: 512);
        h["Turnstile"] = Hero("Turnstile", "turnstile", 1024, 90f, 1, 1.05f, emission: 2.5f, detail: 512);
        h["PatchBench"] = Hero("PatchBench", "patch_bench", 1024, 90f, 0, 1.35f, emission: 3f, detail: 512);
        h["NetworkCabinet"] = Hero("NetworkCabinet", "network_cabinet", 1024, 90f, 0, 0.7f, emission: 3f, detail: 512);
        h["SwitchConsole"] = Hero("SwitchConsole", "switch_console", 1024, 90f, 0, 1.25f, emission: 3f, detail: 512);
        h["Lockbox"] = Hero("Lockbox", "lockbox", 1024, 90f, 0, 0.62f, detail: 512);
        h["CloudLamp"] = Hero("CloudLamp", "cloud_lamp", 1024, 90f, 1, 1.45f, emission: 4f, detail: 512);
        h["TapeLibrary"] = Hero("TapeLibrary", "tape_library", 1024, 90f, 1, 1.95f, emission: 2.5f, detail: 512);
        h["FilingCabinet"] = Hero("FilingCabinet", "filing_cabinet", 1024, 90f, 1, 1.32f, detail: 512);
        h["NoticeBoard"] = Hero("NoticeBoard", "notice_board", 1024, 90f, 0, 0.9f, detail: 512);
        return h;
    }

    static GameObject Hero(string folder, string file, int texSize, float yaw, int axis, float size, float emission = 0f, float clipThickness = 0f, int detail = 0)
    {
        var glb = Directory.GetFiles($"{HeroDir}/{folder}", "*.glb")[0].Replace('\\', '/');
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(glb);
        var model = Object.Instantiate(source);
        model.name = "Model";
        var mat = HeroMaterial(folder, file, texSize, emission, detailSize: detail);
        foreach (var r in model.GetComponentsInChildren<Renderer>()) { r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray(); r.shadowCastingMode = ShadowCastingMode.On; }
        if (clipThickness > 0f) ClipSlab(model, file, clipThickness);
        var root = new GameObject(folder);
        model.transform.SetParent(root.transform, false);
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        var b = Bounds(model);
        float k = size / b.size[axis];
        model.transform.localScale *= k;
        b = Bounds(model);
        model.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
        b = Bounds(model);
        string path = $"{Out}/Hero/{folder}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[FacilityKit] hero {folder}: {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2} m");
        return prefab;
    }

    // The door's closer arm came out of the image-to-3D as a rod through the slab: keep only
    // the triangles within the slab's thickness (in the model's own units, X is the thickness).
    static void ClipSlab(GameObject model, string file, float half)
    {
        foreach (var f in model.GetComponentsInChildren<MeshFilter>())
        {
            var src = f.sharedMesh;
            var v = src.vertices;
            var tris = src.triangles;
            var keep = new List<int>();
            for (int i = 0; i < tris.Length; i += 3)
                if (Mathf.Abs(v[tris[i]].x) <= half && Mathf.Abs(v[tris[i + 1]].x) <= half && Mathf.Abs(v[tris[i + 2]].x) <= half)
                    keep.AddRange(new[] { tris[i], tris[i + 1], tris[i + 2] });
            // Keep only the vertices the kept triangles use, so the bounds are the slab's.
            var map = new Dictionary<int, int>();
            var order = new List<int>();
            foreach (int i in keep) if (!map.ContainsKey(i)) { map[i] = order.Count; order.Add(i); }
            var nrm = src.normals; var tan = src.tangents; var uv0 = new List<Vector2>(); src.GetUVs(0, uv0);
            var mesh = new Mesh { name = file + "_clean" };
            mesh.SetVertices(order.Select(i => v[i]).ToList());
            if (nrm.Length == v.Length) mesh.SetNormals(order.Select(i => nrm[i]).ToList());
            if (tan.Length == v.Length) mesh.SetTangents(order.Select(i => tan[i]).ToList());
            if (uv0.Count == v.Length) mesh.SetUVs(0, order.Select(i => uv0[i]).ToList());
            mesh.SetTriangles(keep.Select(i => map[i]).ToList(), 0);
            if (tan.Length != v.Length) mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            string path = $"{Generated}/Meshes/{file}_clean.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            f.sharedMesh = mesh;
            Debug.Log($"[FacilityKit] {file}: kept {keep.Count / 3} of {tris.Length / 3} triangles inside the slab");
        }
    }

    // ---- The attacker: the three rigged copies (idle, walk, force the door), one Animator each ----

    static void BuildAttacker()
    {
        var mat = HeroMaterial("Attacker", "attacker", 2048, 3f);
        var root = new GameObject("Attacker");
        foreach (var (state, file) in new[] { ("Idle", "attacker_idle"), ("Walk", "attacker_walk"), ("Force", "attacker_push") })
        {
            string glb = $"{HeroDir}/Attacker/{file}.glb";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(glb);
            var clip = AssetDatabase.LoadAllAssetsAtPath(glb).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview"));
            var model = Object.Instantiate(source);
            model.name = state;
            model.transform.SetParent(root.transform, false);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) { r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray(); r.shadowCastingMode = ShadowCastingMode.On; }
            foreach (var s in model.GetComponentsInChildren<SkinnedMeshRenderer>()) s.updateWhenOffscreen = false;
            foreach (var a in model.GetComponentsInChildren<Animation>()) Object.DestroyImmediate(a);
            var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            string controllerPath = $"{Generated}/attacker_{state.ToLowerInvariant()}.controller";
            AssetDatabase.DeleteAsset(controllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerPath, clip);
            animator.runtimeAnimatorController = controller;
            model.SetActive(state == "Idle");
            Debug.Log($"[FacilityKit] attacker {state}: clip {(clip != null ? clip.name + " " + clip.length.ToString("F2") + " s" : "MISSING")}");
        }
        root.AddComponent<AttackerRig>();
        // The visor's scarlet light, at the face.
        var visor = new GameObject("VisorLight");
        visor.transform.SetParent(root.transform, false);
        visor.transform.localPosition = new Vector3(0f, 1.5f, 0.55f);
        var light = visor.AddComponent<Light>();
        light.type = LightType.Point; light.range = 2.6f; light.intensity = 0.6f; light.color = new Color(1f, 0.29f, 0.24f); light.shadows = LightShadows.None;
        var b = Bounds(root);
        Debug.Log($"[FacilityKit] attacker bounds {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2}, min y {b.min.y:F2}");
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Attacker.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- A lamp for a room's board: housing and a lens the board colours ----

    static void BuildLamp(Mats m)
    {
        var root = new GameObject("Lamp");
        Disc(root.transform, "Housing", m.metal, new Vector3(0f, 0f, 0.012f), 0.07f, 0.024f);
        Disc(root.transform, "Lens", m.lens, new Vector3(0f, 0f, 0.028f), 0.045f, 0.012f);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Lamp.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- A door: the generated leaf on a hinge, a painted steel frame, the status lamp, the sign and a badge reader ----
    // Origin: the opening's corridor face at floor level; +Z points into the corridor.

    static void BuildDoor(Mats m, GameObject leaf)
    {
        var root = new GameObject("Door");
        var t = root.transform;
        float zc = -Wall / 2f;
        var hinge = new GameObject("Hinge").transform;
        hinge.SetParent(t, false);
        hinge.localPosition = new Vector3(-DoorW / 2f + 0.06f, 0f, zc);
        var leafGo = (GameObject)PrefabUtility.InstantiatePrefab(leaf);
        leafGo.name = "Leaf";
        leafGo.transform.SetParent(hinge, false);
        leafGo.transform.localPosition = new Vector3(DoorW / 2f - 0.06f, 0.01f, 0f);
        // Frame: two jambs and a head, proud of the wall on the corridor side.
        Part(t, "JambL", m.frame, new Vector3(-DoorW / 2f - 0.03f, DoorH / 2f, zc + 0.02f), new Vector3(0.07f, DoorH + 0.06f, Wall + 0.06f));
        Part(t, "JambR", m.frame, new Vector3(DoorW / 2f + 0.03f, DoorH / 2f, zc + 0.02f), new Vector3(0.07f, DoorH + 0.06f, Wall + 0.06f));
        Part(t, "Head", m.frame, new Vector3(0f, DoorH + 0.03f, zc + 0.02f), new Vector3(DoorW + 0.13f, 0.07f, Wall + 0.06f));
        // The status lamp over the door: a dark housing, the lens Building colours, its light.
        Part(t, "StripHousing", m.trim, new Vector3(0f, DoorH + 0.17f, 0.03f), new Vector3(0.92f, 0.08f, 0.06f));
        var lens = Part(t, "Strip", m.lens, new Vector3(0f, DoorH + 0.17f, 0.063f), new Vector3(0.84f, 0.04f, 0.012f), shadows: false);
        var lightGo = new GameObject("StripLight");
        lightGo.transform.SetParent(lens.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, -0.1f, 0.25f);
        var l = lightGo.AddComponent<Light>();
        l.type = LightType.Point; l.range = 3.2f; l.intensity = 1.2f; l.color = new Color(0.84f, 0.27f, 0.24f); l.shadows = LightShadows.None;
        // The sign: a white enamelled plate; Building writes the room's name on it.
        Part(t, "SignPlate", m.plate, new Vector3(0f, DoorH + 0.42f, 0.012f), new Vector3(1.1f, 0.22f, 0.024f), shadows: false);
        var text = new GameObject("SignText");
        text.transform.SetParent(t, false);
        text.transform.localPosition = new Vector3(0f, DoorH + 0.42f, 0.026f);
        text.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var tmp = text.AddComponent<TextMeshPro>();
        tmp.text = "ROOM"; tmp.fontSize = 1.15f; tmp.alignment = TextAlignmentOptions.Center; tmp.color = new Color(0.06f, 0.08f, 0.16f);
        tmp.rectTransform.sizeDelta = new Vector2(1.04f, 0.2f); tmp.characterSpacing = 3f; tmp.textWrappingMode = TextWrappingModes.NoWrap;
        // A badge reader beside the opening, its LED red while locked.
        Part(t, "Reader", m.trim, new Vector3(DoorW / 2f + 0.22f, 1.25f, 0.02f), new Vector3(0.09f, 0.14f, 0.04f));
        Part(t, "ReaderLed", m.lens, new Vector3(DoorW / 2f + 0.22f, 1.3f, 0.042f), new Vector3(0.025f, 0.012f, 0.006f), shadows: false);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Door.prefab");
        Object.DestroyImmediate(root);
    }

    // ---- The corridor: one bay is 4.2 m along X with a door opening on each side at x = 0 ----

    static void BuildBay(Mats m, string name, int variant)
    {
        var root = new GameObject(name);
        var t = root.transform;
        float half = Pitch / 2f;
        Part(t, "Floor", m.floor, new Vector3(0f, -0.05f, 0f), new Vector3(Pitch, 0.1f, Width), shadows: false);
        var ceiling = new GameObject("Ceiling").transform; ceiling.SetParent(t, false);
        Part(ceiling, "Ceiling", m.ceiling, new Vector3(0f, H + 0.05f, 0f), new Vector3(Pitch, 0.1f, Width + 2f * Wall), shadows: false);
        foreach (int side in new[] { -1, 1 })
        {
            float z = side * (Width / 2f + Wall / 2f);
            float segW = half - DoorW / 2f;
            foreach (int dir in new[] { -1, 1 })
            {
                float cx = dir * (DoorW / 2f + segW / 2f);
                Part(t, "Wall", m.wall, new Vector3(cx, H / 2f, z), new Vector3(segW, H, Wall));
                Part(t, "Skirting", m.trim, new Vector3(cx, 0.06f, side * (Width / 2f - 0.01f)), new Vector3(segW, 0.12f, 0.02f), shadows: false);
                Part(t, "Rail", m.trim, new Vector3(cx, 1.05f, side * (Width / 2f - 0.008f)), new Vector3(segW, 0.05f, 0.016f), shadows: false);
            }
            Part(t, "Lintel", m.wall, new Vector3(0f, DoorH + (H - DoorH) / 2f, z), new Vector3(DoorW, H - DoorH, Wall));
        }
        // Two recessed ceiling panels per bay, off in lockdown (part of the ceiling: not seen from above).
        foreach (float px in new[] { -1.05f, 1.05f })
        {
            Part(ceiling, "PanelFrame", m.trim, new Vector3(px, H - 0.012f, 0f), new Vector3(0.66f, 0.024f, 1.26f), shadows: false);
            Part(ceiling, "Panel", m.panel, new Vector3(px, H - 0.026f, 0f), new Vector3(0.58f, 0.008f, 1.18f), shadows: false);
        }
        // What a facility's corridor has along its walls, different in each of the three bays.
        // The left wall's inner face is at z = -2.75 (props face +Z), the right's at +2.75 (they face -Z);
        // the openings are x in [-0.6, 0.6] and each door's reader sits at x = 0.82.
        float L = -Width / 2f, R = Width / 2f;
        switch (variant)
        {
            case 0:
                Prop("korean_fire_extinguisher_01", t, new Vector3(-1.55f, 0f, L + 0.14f), 0f);
                Prop("fire_alarm", t, new Vector3(-1.55f, 1.45f, L + 0.02f), 0f, onFloor: false);
                Prop("cardboard_box_01", t, new Vector3(1.55f, 0f, R - 0.3f), 172f, scale: 0.7f);
                Prop("cardboard_box_01", t, new Vector3(1.62f, 0f, R - 0.32f), 160f, onFloor: true, floorY: 0.36f, scale: 0.6f);
                Prop("security_camera_01", t, new Vector3(-2.0f, H - 0.1f, L + 0.15f), 40f, onFloor: false);
                break;
            case 1:
                Prop("WetFloorSign_01", t, new Vector3(1.35f, 0f, 1.1f), -35f);
                Prop("metal_trash_can", t, new Vector3(-1.6f, 0f, R - 0.3f), 180f);
                Prop("wall_clock", t, new Vector3(1.6f, 2.2f, L + 0.03f), 0f, onFloor: false);
                Prop("power_box_01", t, new Vector3(-1.45f, 1.35f, L + 0.04f), 0f, onFloor: false);
                break;
            default:
                Prop("power_box_01", t, new Vector3(-1.5f, 1.35f, R - 0.04f), 180f, onFloor: false);
                Prop("korean_fire_extinguisher_01", t, new Vector3(1.6f, 0f, R - 0.14f), 180f);
                Prop("fire_alarm", t, new Vector3(1.6f, 1.45f, R - 0.02f), 180f, onFloor: false);
                Prop("cardboard_box_01", t, new Vector3(1.5f, 0f, L + 0.32f), 8f, scale: 0.75f);
                Prop("security_camera_01", t, new Vector3(2.0f, H - 0.1f, R - 0.15f), -140f, onFloor: false);
                break;
        }
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/{name}.prefab");
        Object.DestroyImmediate(root);
    }

    // The way in: 2.9 m of corridor before the first door, open at its far end, a threshold strip.
    static void BuildEntrance(Mats m)
    {
        var root = new GameObject("CorridorEntrance");
        var t = root.transform;
        const float len = 2.9f; // from x = -5 to x = -2.1, origin at its middle
        Part(t, "Floor", m.floor, new Vector3(0f, -0.05f, 0f), new Vector3(len, 0.1f, Width), shadows: false);
        var ceiling = new GameObject("Ceiling").transform; ceiling.SetParent(t, false);
        Part(ceiling, "Ceiling", m.ceiling, new Vector3(0f, H + 0.05f, 0f), new Vector3(len, 0.1f, Width + 2f * Wall), shadows: false);
        foreach (int side in new[] { -1, 1 })
        {
            Part(t, "Wall", m.wall, new Vector3(0f, H / 2f, side * (Width / 2f + Wall / 2f)), new Vector3(len, H, Wall));
            Part(t, "Skirting", m.trim, new Vector3(0f, 0.06f, side * (Width / 2f - 0.01f)), new Vector3(len, 0.12f, 0.02f), shadows: false);
            Part(t, "Rail", m.trim, new Vector3(0f, 1.05f, side * (Width / 2f - 0.008f)), new Vector3(len, 0.05f, 0.016f), shadows: false);
            // The returns of the entrance: the corridor's mouth.
            Part(t, "Return", m.wall, new Vector3(-len / 2f, H / 2f, side * (Width / 2f - 0.45f)), new Vector3(Wall, H, 0.9f));
        }
        Part(t, "Head", m.wall, new Vector3(-len / 2f, H - 0.2f, 0f), new Vector3(Wall, 0.4f, Width));
        Part(t, "Threshold", m.frame, new Vector3(-len / 2f, 0.005f, 0f), new Vector3(0.12f, 0.01f, Width - 1.8f), shadows: false);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/CorridorEntrance.prefab");
        Object.DestroyImmediate(root);
    }

    // The far end: 0.9 m of corridor and the exit wall with a pair of doors and the EXIT sign.
    // Origin: the exit wall's corridor face at floor level, the corridor towards -X.
    static void BuildEnd(Mats m, GameObject leaf)
    {
        var root = new GameObject("CorridorEnd");
        var t = root.transform;
        const float len = 0.9f;
        Part(t, "Floor", m.floor, new Vector3(-len / 2f, -0.05f, 0f), new Vector3(len, 0.1f, Width), shadows: false);
        var ceiling = new GameObject("Ceiling").transform; ceiling.SetParent(t, false);
        Part(ceiling, "Ceiling", m.ceiling, new Vector3(-len / 2f, H + 0.05f, 0f), new Vector3(len, 0.1f, Width + 2f * Wall), shadows: false);
        foreach (int side in new[] { -1, 1 })
        {
            Part(t, "Wall", m.wall, new Vector3(-len / 2f, H / 2f, side * (Width / 2f + Wall / 2f)), new Vector3(len, H, Wall));
            Part(t, "Skirting", m.trim, new Vector3(-len / 2f, 0.06f, side * (Width / 2f - 0.01f)), new Vector3(len, 0.12f, 0.02f), shadows: false);
        }
        // The exit wall, with a 2.4 m opening for the pair of doors.
        const float ow = 2.4f;
        float seg = (Width + 2f * Wall - ow) / 2f;
        foreach (int side in new[] { -1, 1 })
            Part(t, "ExitWall", m.wall, new Vector3(Wall / 2f, H / 2f, side * (ow / 2f + seg / 2f)), new Vector3(Wall, H, seg));
        Part(t, "ExitLintel", m.wall, new Vector3(Wall / 2f, DoorH + (H - DoorH) / 2f, 0f), new Vector3(Wall, H - DoorH, ow));
        var exit = new GameObject("Exit").transform;
        exit.SetParent(t, false);
        exit.localRotation = Quaternion.LookRotation(Vector3.left); // its corridor side faces -X
        foreach (int side in new[] { -1, 1 })
        {
            var hinge = new GameObject(side < 0 ? "HingeL" : "HingeR").transform;
            hinge.SetParent(exit, false);
            hinge.localPosition = new Vector3(side * (ow / 2f - 0.04f), 0f, -Wall / 2f);
            var leafGo = (GameObject)PrefabUtility.InstantiatePrefab(leaf);
            leafGo.transform.SetParent(hinge, false);
            leafGo.transform.localPosition = new Vector3(-side * (ow / 4f - 0.02f), 0.01f, 0f);
            leafGo.transform.localScale = new Vector3(side < 0 ? 1f : -1f, 1f, 1f) * 1.03f;
        }
        Part(exit, "JambL", m.frame, new Vector3(-ow / 2f - 0.03f, DoorH / 2f, -Wall / 2f + 0.02f), new Vector3(0.07f, DoorH + 0.06f, Wall + 0.06f));
        Part(exit, "JambR", m.frame, new Vector3(ow / 2f + 0.03f, DoorH / 2f, -Wall / 2f + 0.02f), new Vector3(0.07f, DoorH + 0.06f, Wall + 0.06f));
        Part(exit, "Head", m.frame, new Vector3(0f, DoorH + 0.03f, -Wall / 2f + 0.02f), new Vector3(ow + 0.13f, 0.07f, Wall + 0.06f));
        Part(exit, "StripHousing", m.trim, new Vector3(0f, DoorH + 0.17f, 0.03f), new Vector3(2.2f, 0.08f, 0.06f));
        var lens = Part(exit, "Strip", m.lens, new Vector3(0f, DoorH + 0.17f, 0.063f), new Vector3(2.1f, 0.04f, 0.012f), shadows: false);
        var lightGo = new GameObject("StripLight");
        lightGo.transform.SetParent(lens.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, -0.1f, 0.4f);
        var l = lightGo.AddComponent<Light>();
        l.type = LightType.Point; l.range = 5f; l.intensity = 1.8f; l.color = new Color(0.84f, 0.27f, 0.24f); l.shadows = LightShadows.None;
        // The EXIT sign: a green lightbox with the word on it.
        var sign = Lit("exit_sign", Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_diff_1k.jpg", 256, true),
                       Texture($"{PolyHaven}/blue_metal_plate/blue_metal_plate_nor_gl_1k.jpg", 256, false, true), null, null,
                       new Color(0.1f, 0.75f, 0.35f), new Color(0.1f, 0.75f, 0.35f) * 2.4f, null, 0.6f, 0.15f);
        Part(exit, "ExitSign", sign, new Vector3(0f, DoorH + 0.42f, 0.04f), new Vector3(0.62f, 0.2f, 0.07f), shadows: false);
        var text = new GameObject("ExitText");
        text.transform.SetParent(exit, false);
        text.transform.localPosition = new Vector3(0f, DoorH + 0.42f, 0.077f);
        text.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var tmp = text.AddComponent<TextMeshPro>();
        tmp.text = "EXIT"; tmp.fontSize = 1.5f; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(0.6f, 0.2f); tmp.characterSpacing = 8f;
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/CorridorEnd.prefab");
        Object.DestroyImmediate(root);
    }

    static void BuildGround(Mats m)
    {
        var root = new GameObject("Ground");
        Part(root.transform, "Ground", m.ground, new Vector3(0f, -0.3f, 0f), new Vector3(120f, 0.2f, 120f), shadows: false, repeat: 6f);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Ground.prefab");
        Object.DestroyImmediate(root);
    }

    static void Stencil(Transform parent, string text, Vector3 pos, float size, Vector2 box)
    {
        var go = new GameObject("Stencil");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // TMP faces -Z: turned to read from the room
        var tmp = go.AddComponent<TextMeshPro>();
        var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/Inter-Bold SDF.asset");
        if (bold != null) tmp.font = bold;
        tmp.text = text; tmp.fontSize = size; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.62f, 0.68f, 0.76f, 0.26f); tmp.characterSpacing = 6f; tmp.lineSpacing = -22f;
        tmp.rectTransform.sizeDelta = box; tmp.textWrappingMode = TextWrappingModes.NoWrap;
    }

    static Material TubesOff(Material lit)
    {
        var m = new Material(lit) { name = "fluorescent_glass_off" };
        m.SetColor("_BaseColor", new Color(0.55f, 0.58f, 0.62f));
        m.SetColor("_EmissionColor", new Color(0.5f, 0.56f, 0.66f) * 0.15f);
        return Save(m, $"{Generated}/fluorescent_glass_off.mat");
    }

    // A round part facing +Z: Unity's cylinder turned on its side.
    static GameObject Disc(Transform parent, string name, Material m, Vector3 pos, float diameter, float depth)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(diameter, depth / 2f, diameter);
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = m; r.shadowCastingMode = ShadowCastingMode.Off;
        return go;
    }

    // A run of conduit: Unity's cylinder between two points, painted metal.
    static void Conduit(Transform parent, Material m, Vector3 a, Vector3 b, float diameter)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.name = "Conduit";
        go.transform.SetParent(parent, false);
        go.transform.localPosition = (a + b) / 2f;
        go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        go.transform.localScale = new Vector3(diameter, (b - a).magnitude / 2f, diameter);
        go.GetComponent<MeshRenderer>().sharedMaterial = m;
    }

    // Claude Bomb's beam: an open cone, brightest at the lamp, in the dusty air; and the dust.
    static void Beam(Transform parent, Vector3 top, float topRadius, float bottomRadius, float height)
    {
        string meshPath = $"{Generated}/Meshes/beam_{height:F2}.asset".Replace(',', '.');
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null)
        {
            const int sides = 40;
            var verts = new List<Vector3>(); var colors = new List<Color>(); var tris = new List<int>();
            for (int i = 0; i <= sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                verts.Add(dir * topRadius); colors.Add(new Color(1f, 1f, 1f, 0.07f));
                verts.Add(dir * bottomRadius + Vector3.down * height); colors.Add(new Color(1f, 1f, 1f, 0f));
                if (i == sides) continue;
                int v = i * 2;
                tris.AddRange(new[] { v, v + 2, v + 1, v + 1, v + 2, v + 3 });
            }
            mesh = new Mesh { name = "Beam" };
            mesh.SetVertices(verts); mesh.SetColors(colors); mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, meshPath);
        }
        var cone = new GameObject("Beam");
        cone.transform.SetParent(parent, false);
        cone.transform.localPosition = top;
        cone.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = cone.AddComponent<MeshRenderer>();
        r.sharedMaterial = Additive("beam", null, new Color(1f, 0.88f, 0.7f, 1f));
        r.shadowCastingMode = ShadowCastingMode.Off;

        var dust = new GameObject("Dust");
        dust.transform.SetParent(parent, false);
        dust.transform.localPosition = top + Vector3.down * height / 2f;
        var ps = dust.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true; main.prewarm = true; main.startLifetime = 10f; main.startSpeed = 0.01f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.005f, 0.014f);
        main.startColor = new Color(1f, 0.94f, 0.82f, 0.55f);
        main.maxParticles = 120; main.simulationSpace = ParticleSystemSimulationSpace.World; main.useUnscaledTime = true;
        var emission = ps.emission; emission.rateOverTime = 12f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(bottomRadius * 1.6f, height, bottomRadius * 1.6f);
        var noise = ps.noise; noise.enabled = true; noise.strength = 0.025f; noise.frequency = 0.25f;
        var fade = ps.colorOverLifetime; fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        fade.color = g;
        var pr = dust.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = Additive("dust", Dot(), Color.white);
        pr.shadowCastingMode = ShadowCastingMode.Off;
    }

    static Material Additive(string name, Texture2D map, Color color)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        m.SetColor("_BaseColor", color);
        if (map != null) m.SetTexture("_BaseMap", map);
        m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 2f);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.One);
        m.SetFloat("_ZWrite", 0f); m.SetFloat("_Cull", (float)CullMode.Back);
        m.SetOverrideTag("RenderType", "Transparent");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent;
        return Save(m, $"{Generated}/{name}.mat");
    }

    static Texture2D Dot()
    {
        string path = $"{Generated}/dust.png";
        if (!File.Exists(path))
        {
            const int s = 32;
            var px = new Color32[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = new Vector2(x + 0.5f - s / 2f, y + 0.5f - s / 2f).magnitude / (s / 2f);
                    px[y * s + x] = new Color32(255, 255, 255, (byte)(Mathf.Pow(Mathf.Clamp01(1f - d), 2f) * 255));
                }
            WritePng(path, s, s, px);
        }
        return Texture(path, 32, true, wrap: TextureWrapMode.Clamp);
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

    // ---- The monitor's screen and the poster: their own textures ----

    static Material ScreenMaterial()
    {
        string path = $"{Generated}/inbox_screen.png";
        if (!File.Exists(path))
        {
            const int W = 512, Hh = 320;
            var px = new Color32[W * Hh];
            Color32 bg = new Color32(14, 22, 40, 255), bar = new Color32(28, 44, 74, 255), row = new Color32(38, 56, 90, 255), text = new Color32(120, 150, 190, 255), red = new Color32(214, 69, 61, 255), teal = new Color32(36, 224, 192, 255);
            for (int i = 0; i < px.Length; i++) px[i] = bg;
            void Rect(int x, int y, int w, int h, Color32 c) { for (int yy = y; yy < y + h; yy++) for (int xx = x; xx < x + w; xx++) if (xx >= 0 && xx < W && yy >= 0 && yy < Hh) px[(Hh - 1 - yy) * W + xx] = c; }
            Rect(0, 0, W, 34, bar); Rect(14, 12, 90, 10, teal); Rect(W - 120, 12, 100, 10, text);
            Rect(0, 34, 120, Hh - 34, bar);
            for (int k = 0; k < 6; k++) Rect(14, 52 + k * 26, 90, 9, k == 0 ? teal : text);
            for (int k = 0; k < 7; k++)
            {
                int y = 46 + k * 38;
                Rect(132, y, W - 146, 32, k == 2 ? new Color32(70, 30, 40, 255) : row);
                Rect(142, y + 7, 18, 18, k == 2 ? red : text);
                Rect(170, y + 7, 120 + (k * 37 % 60), 7, text);
                Rect(170, y + 19, 220 - (k * 23 % 70), 6, new Color32(80, 104, 140, 255));
                if (k == 2) Rect(W - 40, y + 9, 14, 14, red);
            }
            WritePng(path, W, Hh, px);
        }
        var tex = Texture(path, 512, true, wrap: TextureWrapMode.Clamp);
        var flat = FlatNormal();
        var m = Lit("monitor_screen", tex, flat, null, tex, Color.white, Color.white * 1.6f, null, 0.92f);
        return m;
    }

    static Texture2D FlatNormal()
    {
        string path = $"{Generated}/flat_normal.png";
        if (!File.Exists(path)) WritePng(path, 4, 4, Enumerable.Repeat(new Color32(128, 128, 255, 255), 16).ToArray());
        return Texture(path, 4, false, normalMap: true);
    }

    static Material PosterMaterial()
    {
        // The academy's poster for the post room: the logo and two words, printed on paper.
        string path = $"{Generated}/poster_post_room.png";
        if (!File.Exists(path))
        {
            const int W = 300, Hh = 420;
            var px = new Color32[W * Hh];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(232, 234, 228, 255);
            void Rect(int x, int y, int w, int h, Color32 c) { for (int yy = y; yy < y + h; yy++) for (int xx = x; xx < x + w; xx++) if (xx >= 0 && xx < W && yy >= 0 && yy < Hh) px[(Hh - 1 - yy) * W + xx] = c; }
            Rect(0, 0, W, 70, new Color32(11, 21, 54, 255));
            Rect(24, 26, 120, 18, new Color32(36, 224, 192, 255));
            // An envelope with a red mark, the poster's picture.
            Rect(60, 120, 180, 120, new Color32(250, 250, 248, 255));
            for (int k = 0; k < 90; k++) { Rect(60 + k, 120 + k * 60 / 90, 2, 2, new Color32(150, 160, 170, 255)); Rect(238 - k, 120 + k * 60 / 90, 2, 2, new Color32(150, 160, 170, 255)); }
            Rect(200, 205, 26, 26, new Color32(214, 69, 61, 255));
            for (int k = 0; k < 4; k++) Rect(40, 280 + k * 22, 220 - k * 30, 10, new Color32(60, 70, 90, 255));
            WritePng(path, W, Hh, px);
        }
        var tex = Texture(path, 512, true, wrap: TextureWrapMode.Clamp);
        return Lit("poster_post_room", tex, FlatNormal(), null, null, Color.white, Color.black, null, 0.15f);
    }

    // ---- Small builders ----

    // A box with UVs in metres: one texture repeat every <repeat> m on every face.
    public static GameObject Part(Transform parent, string name, Material m, Vector3 centre, Vector3 size, bool shadows = true, float repeat = Repeat)
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
        // normal, u axis, v axis
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

    // A Poly Haven model, standing on the floor (or at pos, hanging) and turned by yaw.
    static GameObject Prop(string asset, Transform parent, Vector3 pos, float yaw, bool onFloor = true, float floorY = 0f, float scale = 1f)
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
            float floorWorld = parent.TransformPoint(new Vector3(0f, floorY, 0f)).y;
            model.transform.position += Vector3.up * (floorWorld - b.min.y);
        }
        return model;
    }

    static GameObject Place(GameObject prefab, Transform parent, Vector3 pos, float yaw)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        return go;
    }
}
