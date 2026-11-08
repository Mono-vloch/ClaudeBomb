using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// The game-feel pass's additions to the facility: the dark plates every world label sits on, the
// pieces the room rigs and the podium are built from (the kit's own materials on plain boxes),
// the corridor monitor in each room (a second camera on the corridor, the attacker walking to
// this room's door while the player works), the debrief's door with its podium and medal, and
// the close-up camera inside a room.
public partial class Building
{
    public const int CorridorLayer = 8; // the corridor, the doors and the attacker: what the monitor camera renders

    static GameObject Corridor(GameObject go) { foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = CorridorLayer; return go; }

    // ---- The kit's materials, borrowed from its prefabs so every piece matches the world ----

    static readonly Dictionary<string, Material> kitMaterials = new Dictionary<string, Material>();
    public static Material KitMaterial(string prefab, string child)
    {
        string key = prefab + "/" + child;
        if (kitMaterials.TryGetValue(key, out var m) && m != null) return m;
        var go = Kit(prefab);
        if (go == null) return Look.Mat(Look.Rack);
        var t = go.transform.Find(child) ?? go.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == child);
        var r = t != null ? t.GetComponent<Renderer>() : null;
        m = r != null ? r.sharedMaterial : Look.Mat(Look.Rack);
        kitMaterials[key] = m;
        return m;
    }
    public static Material Metal => KitMaterial("Door", "JambL");          // painted steel
    public static Material Trim => KitMaterial("Door", "StripHousing");    // dark trim
    public static Material Lens => KitMaterial("Lamp", "Lens");            // a lamp's lens, glows with SetGlow
    public static Material Enamel => KitMaterial("Door", "SignPlate");     // white enamel: paper, badges
    static Mesh cube, quad;
    public static Mesh Cube => cube != null ? cube : (cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx"));
    public static Mesh Quad => quad != null ? quad : (quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx"));

    // A box of the kit's material, tinted (the tint multiplies the base map).
    public static GameObject Box(Transform parent, string name, Vector3 size, Vector3 pos, Material m, Color? tint = null, bool shadows = true)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        go.AddComponent<MeshFilter>().sharedMesh = Cube;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = m;
        r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        if (tint.HasValue) Tint(r, tint.Value);
        return go;
    }

    public static void Tint(Renderer r, Color c)
    {
        var b = new MaterialPropertyBlock();
        r.GetPropertyBlock(b);
        b.SetColor(BaseColor, c);
        r.SetPropertyBlock(b);
    }

    public static void Glow(Renderer r, Color c, float strength = 1.6f)
    {
        if (r == null) return;
        var b = new MaterialPropertyBlock();
        r.GetPropertyBlock(b);
        b.SetColor(BaseColor, Color.Lerp(c, Color.white, 0.15f));
        b.SetColor(EmissionColor, c * strength);
        r.SetPropertyBlock(b);
    }

    // A small lamp: the kit's lens on a trim housing, lit in a colour.
    public static Renderer Lamp(Transform parent, Vector3 pos, float size, Color colour)
    {
        var housing = Box(parent, "Lamp", new Vector3(size * 1.4f, size * 1.4f, 0.014f), pos, Trim, null, false);
        var lens = Box(housing.transform, "Lens", new Vector3(0.7f, 0.7f, 0.9f), new Vector3(0f, 0f, -0.6f), Lens, null, false);
        var r = lens.GetComponent<Renderer>();
        Glow(r, colour);
        return r;
    }

    // ---- Every label in the world is the same dark plate: a slab of the kit's steel tinted
    // ink, white text, and a teal halo behind it when it is lit. The slab fits the text. The
    // plate reads from its -Z side. ----

    public static readonly Color PlateInk = new Color(0.035f, 0.05f, 0.1f);
    static readonly int Smoothness = Shader.PropertyToID("_Smoothness"), Metallic = Shader.PropertyToID("_Metallic");

    // A plate's slab: no sheen, so a lamp over it never washes its text out.
    public static void Matte(Renderer r)
    {
        var b = new MaterialPropertyBlock();
        r.GetPropertyBlock(b);
        b.SetFloat(Smoothness, 0f);
        b.SetFloat(Metallic, 0f);
        r.SetPropertyBlock(b);
    }
    public static Transform DarkPlate(Transform parent, string text, float fontSize, out TextMeshPro tmp, float minW = 0.26f, float wrapW = 0f)
    {
        var root = new GameObject("Plate").transform;
        root.SetParent(parent, false);
        var halo = Box(root, "Halo", Vector3.one, new Vector3(0f, 0f, 0.012f), Lens, null, false);
        Glow(halo.GetComponent<Renderer>(), Theme.Teal, 1.2f);
        halo.SetActive(false);
        Matte(Box(root, "Slab", Vector3.one, Vector3.zero, Metal, PlateInk, false).GetComponent<Renderer>());
        var go = new GameObject("Text");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(0f, 0f, -0.016f);
        tmp = go.AddComponent<TextMeshPro>();
        tmp.font = Theme.Bold; tmp.fontSize = fontSize; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(wrapW > 0f ? wrapW : 6f, 1f);
        tmp.textWrappingMode = wrapW > 0f ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        tmp.text = text;
        var fit = root.gameObject.AddComponent<PlateFit>();
        fit.minW = minW;
        FitPlate(root);
        return root;
    }

    public static void FitPlate(Transform plateRoot)
    {
        var tmp = plateRoot.GetComponentInChildren<TextMeshPro>(true);
        var fit = plateRoot.GetComponent<PlateFit>();
        if (tmp == null) return;
        tmp.ForceMeshUpdate();
        var size = tmp.textBounds.size;
        float minW = fit != null ? fit.minW : 0.26f, pad = fit != null ? fit.pad : 0.08f;
        float w = Mathf.Max(minW, size.x + pad), h = Mathf.Max(0.07f, size.y + pad / 2f);
        var slab = plateRoot.Find("Slab"); if (slab != null) slab.localScale = new Vector3(w, h, 0.02f);
        var halo = plateRoot.Find("Halo"); if (halo != null) halo.localScale = new Vector3(w + 0.035f, h + 0.035f, 0.012f);
    }

    public static void SetPlate(Transform plateRoot, string text, Color? colour = null)
    {
        var tmp = plateRoot.GetComponentInChildren<TextMeshPro>(true);
        if (tmp == null) return;
        tmp.text = text;
        if (colour.HasValue) tmp.color = colour.Value;
        FitPlate(plateRoot);
    }

    public static void Halo(Transform plateRoot, bool on) { var h = plateRoot != null ? plateRoot.Find("Halo") : null; if (h != null) h.gameObject.SetActive(on); }

    public class PlateFit : MonoBehaviour { public float minW = 0.26f, pad = 0.08f; }

    // A plate that turns to face the camera (its -Z toward the eye).
    public class Billboard : MonoBehaviour
    {
        public Transform cam;
        void LateUpdate() { if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.position); }
    }

    // ---- The corridor monitor: a second camera at the corridor's entrance, rendered small and
    // seldom onto a screen in the room, where the attacker can be seen coming. ----

    Camera monitorCam;
    RenderTexture monitorTex;
    public RenderTexture MonitorTexture => monitorTex;
    public Camera MonitorCamera => monitorCam;
    Coroutine lurking;
    static Material screenMat;

    public Transform Monitor(int room, Vector3 localPos, Quaternion localRot)
    {
        var door = doors[room];
        var mon = Kit("Hero/Monitor");
        Transform shell;
        if (mon != null)
        {
            shell = Instantiate(mon, door, false).transform;
            shell.name = "CorridorMonitor";
            shell.localPosition = localPos; shell.localRotation = localRot;
        }
        else
        {
            shell = Box(door, "CorridorMonitor", new Vector3(0.5f, 0.32f, 0.05f), localPos, Trim).transform;
            shell.localRotation = localRot;
        }
        // The screen: a panel just proud of the monitor's face, showing the corridor camera.
        var b = new Bounds(shell.position, Vector3.zero);
        bool any = false;
        foreach (var r in shell.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        if (monitorTex == null) { monitorTex = new RenderTexture(320, 180, 16, RenderTextureFormat.Default) { name = "CorridorFeed", filterMode = FilterMode.Bilinear }; }
        if (screenMat == null)
        {
            var shader = Resources.Load<Shader>("Shaders/Spark");
            screenMat = new Material(shader) { name = "corridor-screen" };
            screenMat.SetFloat("_Dst", (float)BlendMode.OneMinusSrcAlpha);
        }
        screenMat.mainTexture = monitorTex;
        screenMat.color = new Color(1.25f, 1.55f, 1.5f, 1f); // a security feed's cold cast, brightened
        var screen = new GameObject("Feed");
        screen.transform.SetParent(shell, false);
        screen.AddComponent<MeshFilter>().sharedMesh = Quad;
        var sr = screen.AddComponent<MeshRenderer>();
        sr.sharedMaterial = screenMat; sr.shadowCastingMode = ShadowCastingMode.Off; sr.receiveShadows = false;
        var fwd = shell.forward;
        float depth = Mathf.Abs(Vector3.Dot(b.size, new Vector3(Mathf.Abs(fwd.x), 0f, Mathf.Abs(fwd.z))));
        float width = Mathf.Abs(Vector3.Dot(b.size, new Vector3(Mathf.Abs(fwd.z), 0f, Mathf.Abs(fwd.x))));
        screen.transform.rotation = Quaternion.LookRotation(-fwd, Vector3.up); // a quad faces -Z: turned to face where the monitor faces
        screen.transform.position = b.center + fwd * (depth / 2f + 0.004f) + Vector3.up * b.size.y * 0.13f;
        screen.transform.localScale = new Vector3(width * 0.93f, b.size.y * 0.52f, 1f);
        var rec = Box(screen.transform, "Rec", new Vector3(0.04f, 0.07f, 0.001f), new Vector3(0.44f, 0.4f, -0.002f), Lens, null, false);
        Glow(rec.GetComponent<Renderer>(), Theme.Danger, 2f);
        rec.AddComponent<Blink>();
        // The camera: at the corridor's entrance under the ceiling, looking down it toward this room.
        if (monitorCam == null)
        {
            var go = new GameObject("MonitorCamera");
            go.transform.SetParent(transform, false);
            monitorCam = go.AddComponent<Camera>();
            monitorCam.cullingMask = 1 << CorridorLayer;
            monitorCam.fieldOfView = 58f; monitorCam.nearClipPlane = 0.2f; monitorCam.farClipPlane = 60f;
            monitorCam.clearFlags = CameraClearFlags.SolidColor; monitorCam.backgroundColor = Theme.Hex("#101A2E");
            monitorCam.allowHDR = false; monitorCam.allowMSAA = false;
            monitorCam.depth = -5;
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = false; data.renderShadows = false; data.antialiasing = AntialiasingMode.None;
            data.requiresColorOption = CameraOverrideOption.Off; data.requiresDepthOption = CameraOverrideOption.Off;
            go.AddComponent<Throttle>().cam = monitorCam;
        }
        monitorCam.targetTexture = monitorTex;
        monitorCam.transform.position = new Vector3(-Back + 0.3f, 2.25f, 0f);
        monitorCam.transform.LookAt(new Vector3(door.position.x, 0.9f, door.position.z * 0.35f));
        monitorCam.enabled = true;
        return shell;
    }

    // The monitor camera renders one frame in four: a security feed's frame rate, a quarter of the cost.
    class Throttle : MonoBehaviour { public Camera cam; int n; void Update() { if (cam != null) cam.enabled = (n++ % 4) == 0; } }
    class Blink : MonoBehaviour { Renderer r; void Update() { if (r == null) r = GetComponent<Renderer>(); if (r != null) r.enabled = Mathf.Repeat(Time.unscaledTime, 1.2f) < 0.7f; } }

    // While the player works, the attacker walks the corridor toward this room, door by door,
    // and waits outside it, trying the handle now and then. It never attacks before "Run the attack".
    public void Lurk(int room)
    {
        if (lurking != null) StopCoroutine(lurking);
        lurking = StartCoroutine(DoLurk(room));
    }

    IEnumerator DoLurk(int room)
    {
        attacker.gameObject.SetActive(true);
        attacker.position = Entrance;
        attacker.rotation = Quaternion.LookRotation(Vector3.right);
        var door = doors[room];
        var target = door.position + door.forward * 1.1f;
        float along = target.x - Entrance.x;
        yield return new WaitForSeconds(2.5f);
        // A pause at every door on the way, looking in.
        int stops = Mathf.Max(0, Mathf.FloorToInt(along / Pitch));
        for (int i = 0; i < stops; i++)
        {
            var at = new Vector3(Entrance.x + (i + 0.5f) * Pitch, 0f, 0f);
            yield return WalkTo(at, 5.5f);
            var side = doors[Mathf.Min(11, i * 2 + (room % 2))];
            var face = side.position - attacker.position; face.y = 0f;
            attacker.rotation = Quaternion.LookRotation(face);
            yield return new WaitForSeconds(2.2f);
        }
        yield return WalkTo(target, 5f);
        while (true)
        {
            var face = door.position - attacker.position; face.y = 0f;
            attacker.rotation = Quaternion.LookRotation(face);
            yield return new WaitForSeconds(Random.Range(4f, 7f));
            if (body != null) body.Force(0.7f);
            Sfx.Clack();
            yield return new WaitForSeconds(1.2f);
        }
    }

    // ---- The debrief: the corridor outside this room's door, the attacker turning away or
    // standing in the breach, a podium with the medal rising on it. ----

    Transform podium;
    public void ShowDebrief(RectTransform frame, int room, bool through, int medal, Color colour)
    {
        this.frame = frame; Mood(View.Corridor); CorridorReflections(); Canvas.ForceUpdateCanvases();
        var door = doors[room];
        attacker.gameObject.SetActive(true);
        if (trail != null) { trail.positionCount = 0; trailPoints.Clear(); }
        trail = null; // no red line on the floor here: it read as blood
        if (through) { attacker.position = door.position + door.forward * 0.35f; attacker.rotation = Quaternion.LookRotation(door.forward); }
        else
        {
            // Turning away down the corridor, toward the entrance, already a few steps off.
            var away = new Vector3(Entrance.x + 1f, 0f, 0f) - door.position; away.y = 0f;
            attacker.position = door.position + door.forward * 1.6f + away.normalized * 2.6f;
            attacker.rotation = Quaternion.LookRotation(away.normalized);
            StartCoroutine(Leave(door));
        }
        Podium(door, medal, colour);
        var centre = door.position + door.forward * 1.1f + Vector3.up * 1.15f;
        cam.orbitDistance = 5.0f; cam.orbitPitch = 10f; cam.orbitSpeed = 3f; cam.orbitFov = 40f;
        float yaw = Mathf.Atan2(door.forward.x, door.forward.z) * Mathf.Rad2Deg; // the camera stands in the corridor, in front of the door
        cam.Orbit(frame, centre);
        cam.orbitArc = 7f; cam.orbitBase = yaw + 180f + 16f; cam.orbitFov = 40f;
        cam.Snap();
    }

    IEnumerator Leave(Transform door)
    {
        yield return new WaitForSeconds(0.2f);
        yield return WalkTo(new Vector3(Entrance.x + 1f, 0f, Mathf.Clamp(attacker.position.z, -1f, 1f)), 12f);
        attacker.gameObject.SetActive(false);
    }

    // Three steps numbered 1, 2, 3 in the corridor before the door; the medal hangs on a ribbon
    // from the step it earned (gold on 1, silver on 2, bronze on 3) with its three stars on the
    // ribbon, lit as earned. Built from the kit's steel and lens, no model.
    public Transform Podium(Transform door, int medal, Color colour)
    {
        if (podium != null) Destroy(podium.gameObject);
        podium = new GameObject("Podium").transform;
        podium.SetParent(transform, false);
        podium.position = door.position + door.forward * 1.25f - door.right * 0.3f;
        podium.rotation = Quaternion.LookRotation(-door.forward); // its front toward the camera in the corridor
        float[] heights = { 0.44f, 0.3f, 0.18f };
        float[] widths = { 0.8f, 0.52f, 0.52f };
        float[] xs = { 0f, -0.7f, 0.7f };
        var face = new Color(0.12f, 0.16f, 0.28f);
        for (int i = 0; i < 3; i++)
        {
            float w = widths[i], h = heights[i];
            Box(podium, "Step", new Vector3(w, h, 0.7f), new Vector3(xs[i], h / 2f, 0f), Metal, face);
            Box(podium, "Edge", new Vector3(w, 0.015f, 0.7f), new Vector3(xs[i], h + 0.008f, 0f), Metal, new Color(0.75f, 0.78f, 0.84f), false);
            var num = new GameObject("Step").AddComponent<TextMeshPro>();
            num.transform.SetParent(podium, false);
            num.transform.localPosition = new Vector3(xs[i] + (i == 0 ? 0.26f : 0f), h * 0.5f, -0.36f);
            num.font = Theme.Bold; num.fontSize = i == 0 ? 1.6f : 1.2f; num.color = new Color(0.8f, 0.84f, 0.9f); num.alignment = TextAlignmentOptions.Center;
            num.rectTransform.sizeDelta = new Vector2(0.4f, 0.3f);
            num.text = (i + 1).ToString();
        }
        if (medal > 0)
        {
            int step = 3 - medal;
            var m = new GameObject("Medal").transform;
            m.SetParent(podium, false);
            m.localPosition = new Vector3(xs[step] - (step == 0 ? 0.06f : 0f), heights[step] + 0.02f, -0.36f);
            var ribbon = new Color(0.16f, 0.2f, 0.42f);
            float drop = 0.2f;
            foreach (float sx in new[] { -1f, 1f })
            {
                var strip = Box(m, "Ribbon", new Vector3(0.04f, drop * 1.15f, 0.01f), new Vector3(sx * 0.04f, -drop / 2f, -0.01f), Metal, ribbon, false);
                strip.transform.localRotation = Quaternion.Euler(0f, 0f, sx * 10f);
            }
            float discY = -drop - 0.085f;
            Disc(m, 0.09f, new Vector3(0f, discY, -0.016f), colour * 0.6f);
            Disc(m, 0.075f, new Vector3(0f, discY, -0.024f), colour);
            Star(m, 0.035f, new Vector3(0f, discY, -0.032f), colour * 1.3f);
            // The three stars in an arc over the disc, on the ribbon, big enough to read from the corridor.
            for (int i = 0; i < 3; i++)
            {
                var star = Star(m, 0.042f, new Vector3((i - 1) * 0.095f, -0.095f + (i == 1 ? 0.02f : 0f), -0.04f), i < medal ? Theme.Amber : new Color(0.3f, 0.33f, 0.4f));
                star.transform.localScale = Vector3.zero;
                star.name = "Star" + i;
            }
            // No lamp of its own: the disc glows, and a light here threw a yellow pool on the floor.
            m.localScale = new Vector3(1f, 0f, 1f); // it drops into place
        }
        return podium;
    }

    public IEnumerator RaisePodium(int medal)
    {
        if (podium == null) yield break;
        var m = podium.Find("Medal");
        if (m == null) yield break;
        float t = 0;
        while (t < 0.6f && m != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / 0.6f); k = 1f - (1f - k) * (1f - k); m.localScale = new Vector3(1f, k, 1f); yield return null; }
        for (int i = 0; i < 3 && m != null; i++)
        {
            var star = m.Find("Star" + i);
            if (star == null) continue;
            if (i < medal) Sfx.Chime();
            float u = 0;
            while (u < 0.22f && star != null) { u += Time.unscaledDeltaTime; star.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, Ui.EaseOutBack(Mathf.Clamp01(u / 0.22f))); yield return null; }
            if (star != null) star.localScale = Vector3.one;
            if (i < medal && medal >= 2) effects?.Confetti(m.position + Vector3.up * 1.2f, 24);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    GameObject Disc(Transform parent, float r, Vector3 at, Color tint)
    {
        var go = new GameObject("Disc");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(r * 2f, 0.004f, r * 2f);
        go.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Lens; mr.shadowCastingMode = ShadowCastingMode.Off;
        Glow(mr, tint, 0.7f);
        return go;
    }

    GameObject Star(Transform parent, float r, Vector3 at, Color tint)
    {
        var v = new List<Vector3>(); var tris = new List<int>();
        v.Add(Vector3.zero);
        for (int i = 0; i < 10; i++)
        {
            float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float rr = i % 2 == 0 ? r : r * 0.45f;
            v.Add(new Vector3(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr, 0f));
        }
        for (int i = 1; i <= 10; i++) { tris.Add(0); tris.Add(i % 10 + 1); tris.Add(i); } // clockwise from -Z: the face the camera sees
        var mesh = new Mesh();
        mesh.SetVertices(v); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        var go = new GameObject("Star");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Lens; mr.shadowCastingMode = ShadowCastingMode.Off;
        Glow(mr, tint, 1.4f);
        return go;
    }

    // ---- Inside a room, closer: the camera steps up to what the player sets and back. ----

    // Closer to the prop along the room camera's own line of sight, so nothing the feed does not
    // already show (the chair, the bin) can come between.
    public void ShowRoomClose(RectTransform frame, int room, Bounds what)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        Mood(View.Room);
        foreach (var l in stripLights) if (l != null) l.enabled = false;
        if (camPoints[room] == null) return;
        float fov = roomFov[room] > 0f ? roomFov[room] : 48f;
        var eye0 = camPoints[room].position;
        var dir = (eye0 - what.center).normalized;
        float half = Mathf.Max(what.extents.y, what.extents.x / 1.6f);
        float d = Mathf.Min(Vector3.Distance(eye0, what.center), half / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * 1.45f + what.extents.z);
        cam.Inside(frame, what.center + dir * d, what.center, fov);
        cam.Snap();
    }

    public void ShowRoomFeed(RectTransform frame, int room)
    {
        cam.minDistance = 4f; cam.fov = 30f;
        ShowRoom(frame, room);
    }

    public Transform RoomRoot(int room) => doors[room];

    // The room's own camera point (the kit measured it from the hero), so a rig can tell what
    // the feed will show before the camera has moved there.
    public bool RoomCamera(int room, out Vector3 eye, out Vector3 look, out float fov)
    {
        eye = look = Vector3.zero; fov = 48f;
        if (camPoints[room] == null) return false;
        eye = camPoints[room].position; look = lookAts[room].position; fov = roomFov[room] > 0f ? roomFov[room] : 48f;
        return true;
    }
    public Bounds HeroBounds(int room)
    {
        var hero = heroes[room];
        if (hero == null) return new Bounds(doors[room].TransformPoint(new Vector3(-0.4f, 0.8f, -3.2f)), new Vector3(1.4f, 1.6f, 0.6f));
        var rs = hero.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer)).ToArray();
        if (rs.Length == 0) return new Bounds(hero.position, Vector3.one);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
    public Vector3 ScreenOf(Vector3 world) => cam.ToScreen(world);
    public Camera Camera => cam.GetComponent<Camera>();
}
