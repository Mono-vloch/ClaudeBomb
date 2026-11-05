using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// The facility in three dimensions: a corridor with a door to each of the twelve
// rooms, six a side, the exit at the far end. Each door has a light (locked,
// current, held, breached) and a sign; behind it a small room with a rack, a desk
// and a screen. The attacker walks the corridor when the facility runs the attack.
public class Building : MonoBehaviour
{
    public enum DoorState { Locked, Current, Held, Breached, Open }

    public SceneCamera cam;
    public RectTransform frame;
    const float Pitch = 4.2f, Width = 5.5f;
    readonly Transform[] doors = new Transform[12];
    readonly Transform[] anchors = new Transform[12];
    readonly Renderer[] lights = new Renderer[12];
    readonly Transform[] panels = new Transform[12];
    Transform attacker, exitLight;
    bool night;
    readonly System.Random dice = new System.Random(5);

    public static void Setup()
    {
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.shadowProjection = ShadowProjection.StableFit;
        QualitySettings.shadowDistance = 60f;
        QualitySettings.shadowCascades = 2;
        QualitySettings.pixelLightCount = 4;
        QualitySettings.antiAliasing = 4;
    }

    public float Length => 6 * Pitch + 4f;
    public Bounds Bounds => new Bounds(new Vector3(Length / 2 - 2f, 1.2f, 0), new Vector3(Length + 2f, 2.4f, Width + 4f));
    public Vector3 DoorPosition(int room) => doors[room].position;

    public void Build(bool night)
    {
        Clear();
        gameObject.SetActive(true);
        this.night = night;
        BuildLight();
        BuildCorridor();
        for (int i = 0; i < 12; i++) BuildRoom(i);
        BuildExit();
        BuildAttacker();
        attacker.gameObject.SetActive(false);
    }

    public void Clear()
    {
        StopAllCoroutines();
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
    }

    public void Hide() { Clear(); gameObject.SetActive(false); if (cam != null) cam.Sleep(); }

    public void ShowOverview(RectTransform frame) { this.frame = frame; Canvas.ForceUpdateCanvases(); cam.Overview(frame, Bounds); cam.pitch = 48; cam.yaw = 28; cam.Snap(); }
    public void ShowOrbit(RectTransform frame) { this.frame = frame; Canvas.ForceUpdateCanvases(); cam.Orbit(frame, Bounds.center); cam.orbitDistance = 24f; cam.orbitPitch = 28f; cam.Snap(); }
    // Looks down into the room from the corridor side: the walls are low enough to see
    // the rack, the desk and the screen over them.
    public void ShowRoom(RectTransform frame, int room)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        var centre = doors[room].TransformPoint(new Vector3(0, 0.9f, -1.9f));
        cam.Overview(frame, new Bounds(centre, new Vector3(4.4f, 2.6f, 4.2f)));
        cam.pitch = 50; cam.yaw = room % 2 == 0 ? 200 : 20; cam.Snap();
    }

    void BuildLight()
    {
        var sunGo = new GameObject("Sun"); sunGo.transform.SetParent(transform, false);
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional; sun.shadows = LightShadows.Soft; sun.shadowStrength = 0.5f; sun.shadowBias = 0.05f; sun.shadowNormalBias = 0.4f;
        var dome = MeshKit.Part(transform, "sky", MeshKit.Sphere, null, Vector3.zero, Vector3.one * 300f);
        var dr = dome.GetComponent<Renderer>(); dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; dr.receiveShadows = false;
        var camera = cam.GetComponent<Camera>();
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        if (night)
        {
            sun.transform.rotation = Quaternion.Euler(40, 30, 0); sun.color = Look.SunNight; sun.intensity = 0.35f;
            RenderSettings.ambientSkyColor = Theme.Hex("#22365C"); RenderSettings.ambientEquatorColor = Theme.Hex("#2B3350"); RenderSettings.ambientGroundColor = Theme.Hex("#101728");
            RenderSettings.fogColor = Look.NightHorizon; dr.sharedMaterial = Look.Sky(Look.NightTop, Look.NightHorizon, Look.NightBottom); camera.backgroundColor = Look.NightHorizon;
        }
        else
        {
            sun.transform.rotation = Quaternion.Euler(55, -25, 0); sun.color = Look.SunDay; sun.intensity = 0.65f;
            RenderSettings.ambientSkyColor = Theme.Hex("#7E8EA0"); RenderSettings.ambientEquatorColor = Theme.Hex("#8A8F96"); RenderSettings.ambientGroundColor = Theme.Hex("#4F555D");
            RenderSettings.fogColor = Look.Haze; dr.sharedMaterial = Look.Sky(Look.SkyTop, Look.SkyHorizon, Look.SkyBottom); camera.backgroundColor = Look.SkyHorizon;
        }
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 40f; RenderSettings.fogEndDistance = 140f;
    }

    static GameObject NoShadow(GameObject go) { go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; return go; }

    void BuildCorridor()
    {
        float len = Length;
        NoShadow(MeshKit.Part(transform, "ground", MeshKit.Box, Look.Mat(Theme.Hex("#5F6771")), new Vector3(len / 2 - 2, -0.3f, 0), new Vector3(120, 0.2f, 120)));
        NoShadow(MeshKit.Part(transform, "floor", MeshKit.Box, Look.Mat(Look.Floor), new Vector3(len / 2 - 2, -0.05f, 0), new Vector3(len, 0.1f, Width)));
        for (int i = 0; i < 7; i++) NoShadow(MeshKit.Part(transform, "line", MeshKit.Box, Look.Mat(Look.FloorLine), new Vector3(i * Pitch, 0.005f, 0), new Vector3(0.05f, 0.01f, Width)));
        // The walls, in segments between doors, both sides; a skirting along the floor.
        for (int side = -1; side <= 1; side += 2)
        {
            float z = side * Width / 2;
            for (int i = 0; i <= 6; i++)
            {
                float x0 = i * Pitch - Pitch / 2 + (i == 0 ? -1.2f : 0.9f), x1 = i * Pitch + Pitch / 2 - (i == 6 ? -1.2f : 0.9f);
                if (i == 6) x1 = len - 2;
                if (i == 0) x0 = -2;
                float mid = (x0 + x1) / 2, w = x1 - x0;
                if (w <= 0) continue;
                MeshKit.Part(transform, "wall", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(mid, 1.2f, z), new Vector3(w, 2.4f, 0.3f));
                MeshKit.Part(transform, "skirt", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(mid, 0.08f, z - side * 0.16f), new Vector3(w, 0.16f, 0.04f));
            }
            // The lintels over the doors
            for (int i = 0; i < 6; i++) MeshKit.Part(transform, "lintel", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(i * Pitch, 2.1f, z), new Vector3(1.8f, 0.6f, 0.3f));
        }
        // The near end: the entrance, open.
        MeshKit.Part(transform, "endwall", MeshKit.Box, Look.Mat(Look.WallDark), new Vector3(-2, 1.2f, 0), new Vector3(0.3f, 2.4f, Width + 0.3f));
    }

    // One room: its door in the corridor wall, the light above it, the sign, and the
    // room behind: a rack with LEDs, a desk with a glowing screen, a chair.
    void BuildRoom(int i)
    {
        int side = i % 2 == 0 ? -1 : 1;          // even rooms on the left wall, odd on the right
        float x = (i / 2) * Pitch, z = side * Width / 2;
        var root = new GameObject("Room " + Facility.Rooms[i]).transform;
        root.SetParent(transform, false);
        root.localPosition = new Vector3(x, 0, z);
        root.localRotation = Quaternion.LookRotation(new Vector3(0, 0, -side)); // forward points into the corridor
        // The door panel, in the wall, sliding open when the room is visited.
        var panel = MeshKit.Part(root, "door", MeshKit.Box, Look.Mat(Look.Door), new Vector3(0, 0.9f, 0), new Vector3(1.6f, 1.8f, 0.12f));
        panels[i] = panel.transform;
        MeshKit.Part(root, "frameL", MeshKit.Box, Look.Mat(Look.DoorFrame), new Vector3(-0.86f, 0.95f, 0), new Vector3(0.1f, 1.9f, 0.34f));
        MeshKit.Part(root, "frameR", MeshKit.Box, Look.Mat(Look.DoorFrame), new Vector3(0.86f, 0.95f, 0), new Vector3(0.1f, 1.9f, 0.34f));
        var light = MeshKit.Part(root, "light", MeshKit.Box, Look.Own(Look.Locked, Look.Locked * 0.8f), new Vector3(0, 1.98f, 0.17f), new Vector3(1.2f, 0.08f, 0.06f));
        lights[i] = light.GetComponent<Renderer>();
        var point = light.AddComponent<Light>(); point.type = LightType.Point; point.range = 3f; point.intensity = night ? 1.2f : 0.5f; point.color = Look.Locked;
        // The sign
        var sign = MeshKit.Part(root, "sign", MeshKit.Box, Look.Mat(Look.Sign), new Vector3(0, 2.25f, 0.17f), new Vector3(1.7f, 0.3f, 0.04f));
        var text = new GameObject("name"); text.transform.SetParent(root, false); text.transform.localPosition = new Vector3(0, 2.25f, 0.2f); text.transform.localRotation = Quaternion.Euler(0, 180, 0);
        var tmp = text.AddComponent<TextMeshPro>(); tmp.font = Theme.Bold; tmp.text = Facility.Rooms[i].ToUpperInvariant(); tmp.fontSize = 1.6f; tmp.color = Theme.Ink; tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(1.8f, 0.3f); tmp.characterSpacing = 4;
        // The room behind the wall
        var inside = new GameObject("inside").transform; inside.SetParent(root, false); inside.localPosition = new Vector3(0, 0, -1.9f);
        NoShadow(MeshKit.Part(inside, "floor", MeshKit.Box, Look.Mat(Look.WallDark), new Vector3(0, -0.04f, 0), new Vector3(3.6f, 0.08f, 3.4f)));
        MeshKit.Part(inside, "back", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(0, 1.2f, -1.75f), new Vector3(3.6f, 2.4f, 0.1f));
        MeshKit.Part(inside, "sideL", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(-1.8f, 1.2f, 0), new Vector3(0.1f, 2.4f, 3.5f));
        MeshKit.Part(inside, "sideR", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(1.8f, 1.2f, 0), new Vector3(0.1f, 2.4f, 3.5f));
        var rack = MeshKit.Part(inside, "rack", MeshKit.Box, Look.Mat(Look.Rack), new Vector3(-1.1f, 0.9f, -1.1f), new Vector3(0.7f, 1.8f, 0.6f));
        for (int k = 0; k < 6; k++) NoShadow(MeshKit.Part(inside, "led", MeshKit.Box, Look.Mat(Look.RackLed, Look.RackLed * 0.8f), new Vector3(-1.3f + (k % 2) * 0.4f, 0.3f + (k / 2) * 0.5f, -0.79f), new Vector3(0.06f, 0.04f, 0.02f)));
        MeshKit.Part(inside, "desk", MeshKit.Box, Look.Mat(Look.Desk), new Vector3(0.8f, 0.72f, -0.9f), new Vector3(1.6f, 0.06f, 0.8f));
        MeshKit.Part(inside, "legs", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(0.8f, 0.35f, -0.9f), new Vector3(1.4f, 0.7f, 0.1f));
        NoShadow(MeshKit.Part(inside, "screen", MeshKit.Box, Look.Mat(Look.Screen, Look.Screen * 0.6f), new Vector3(0.8f, 1.05f, -1.15f), new Vector3(0.8f, 0.5f, 0.04f)));
        MeshKit.Part(inside, "chair", MeshKit.Box, Look.Mat(Look.Chair), new Vector3(0.8f, 0.3f, -0.2f), new Vector3(0.5f, 0.6f, 0.5f));
        // Where the camera stands to look into the room: in the corridor, facing the door.
        var anchor = new GameObject("anchor").transform; anchor.SetParent(root, false); anchor.localPosition = new Vector3(0.3f, 0, -1.6f); anchor.localRotation = Quaternion.identity;
        anchors[i] = anchor;
        doors[i] = root;
    }

    void BuildExit()
    {
        float x = Length - 2;
        var root = new GameObject("Exit").transform; root.SetParent(transform, false); root.localPosition = new Vector3(x, 0, 0); root.localRotation = Quaternion.LookRotation(Vector3.left);
        MeshKit.Part(root, "wall", MeshKit.Box, Look.Mat(Look.WallDark), new Vector3(0, 1.2f, 0), new Vector3(Width + 0.3f, 2.4f, 0.3f));
        MeshKit.Part(root, "door", MeshKit.Box, Look.Mat(Look.Exit), new Vector3(0, 1.0f, 0.18f), new Vector3(2.4f, 2.0f, 0.1f));
        MeshKit.Part(root, "frame", MeshKit.Box, Look.Mat(Look.DoorFrame), new Vector3(0, 2.05f, 0.18f), new Vector3(2.6f, 0.1f, 0.3f));
        var light = MeshKit.Part(root, "light", MeshKit.Box, Look.Own(Look.Locked, Look.Locked * 0.8f), new Vector3(0, 2.15f, 0.3f), new Vector3(2.2f, 0.1f, 0.08f));
        exitLight = light.transform;
        var point = light.AddComponent<Light>(); point.type = LightType.Point; point.range = 5f; point.intensity = 1.5f; point.color = Look.Locked;
        var text = new GameObject("exit"); text.transform.SetParent(root, false); text.transform.localPosition = new Vector3(0, 2.4f, 0.3f); text.transform.localRotation = Quaternion.Euler(0, 180, 0);
        var tmp = text.AddComponent<TextMeshPro>(); tmp.font = Theme.Bold; tmp.text = "EXIT"; tmp.fontSize = 3f; tmp.color = Look.Sign; tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(3f, 0.6f); tmp.characterSpacing = 8;
    }

    void BuildAttacker()
    {
        attacker = new GameObject("Attacker").transform;
        attacker.SetParent(transform, false);
        attacker.localPosition = new Vector3(-1.5f, 0, 0);
        MeshKit.Part(attacker, "body", MeshKit.Capsule, Look.Mat(Look.Attacker), new Vector3(0, 0.75f, 0), new Vector3(0.6f, 1.5f, 0.6f));
        MeshKit.Part(attacker, "hood", MeshKit.Sphere, Look.Mat(Look.Attacker), new Vector3(0, 1.45f, 0), 0.5f);
        NoShadow(MeshKit.Part(attacker, "visor", MeshKit.Box, Look.Mat(Look.Visor, Look.Visor), new Vector3(0, 1.45f, 0.2f), new Vector3(0.3f, 0.06f, 0.1f)));
        MeshKit.Part(attacker, "bag", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(0, 0.9f, -0.3f), new Vector3(0.4f, 0.5f, 0.2f));
    }

    public void SetDoor(int room, DoorState state)
    {
        if (lights[room] == null) return;
        var c = state == DoorState.Held || state == DoorState.Open ? Look.Held : state == DoorState.Current ? Look.Current : state == DoorState.Breached ? Look.Breached : Look.Locked;
        lights[room].sharedMaterial = Look.Own(c, c * 0.8f);
        var point = lights[room].GetComponent<Light>(); if (point != null) point.color = c;
        // An open door slides aside; a locked one is shut.
        if (panels[room] != null) panels[room].localPosition = new Vector3(state == DoorState.Open || state == DoorState.Breached || state == DoorState.Current ? -1.4f : 0, 0.9f, 0);
    }

    public void SetExit(bool open)
    {
        if (exitLight == null) return;
        var c = open ? Look.Held : Look.Locked;
        exitLight.GetComponent<Renderer>().sharedMaterial = Look.Own(c, c * 0.8f);
        var point = exitLight.GetComponent<Light>(); if (point != null) point.color = c;
    }

    // The attacker walks to a room's door; true when the door gives.
    public IEnumerator Visit(int room, bool through, float seconds)
    {
        attacker.gameObject.SetActive(true);
        var door = doors[room];
        var target = door.position + door.forward * 1.2f;
        var from = attacker.position;
        float t = 0;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0, 1, t / seconds);
            attacker.position = Vector3.Lerp(from, target, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 20f)) * 0.03f;
            var dir = (target - from); dir.y = 0;
            if (dir.sqrMagnitude > 0.01f) attacker.rotation = Quaternion.Slerp(attacker.rotation, Quaternion.LookRotation(dir), 0.2f);
            yield return null;
        }
        var face = door.position - attacker.position; face.y = 0;
        attacker.rotation = Quaternion.LookRotation(face);
        SetDoor(room, through ? DoorState.Breached : DoorState.Held);
        if (through) { StartCoroutine(Puff(door.position + Vector3.up * 1f, Look.Breached)); float s = 0; var p0 = attacker.position; while (s < 0.5f) { s += Time.deltaTime; attacker.position = Vector3.Lerp(p0, door.position + door.forward * 0.3f, s / 0.5f); yield return null; } }
        else { float s = 0; var p0 = attacker.position; while (s < 0.35f) { s += Time.deltaTime; attacker.position = p0 + door.forward * Mathf.Sin(s / 0.35f * Mathf.PI) * -0.3f; yield return null; } attacker.position = p0; }
    }

    public IEnumerator WalkTo(Vector3 target, float seconds)
    {
        attacker.gameObject.SetActive(true);
        var from = attacker.position; float t = 0;
        while (t < seconds) { t += Time.deltaTime; attacker.position = Vector3.Lerp(from, target, Mathf.SmoothStep(0, 1, t / seconds)); yield return null; }
    }

    public Vector3 Entrance => new Vector3(-1.5f, 0, 0);
    public Vector3 ExitPoint => new Vector3(Length - 3.5f, 0, 0);

    IEnumerator Puff(Vector3 at, Color colour)
    {
        var bits = new List<(Transform t, Vector3 v)>();
        for (int i = 0; i < 12; i++)
        {
            var bit = MeshKit.Part(transform, "puff", MeshKit.Sphere, Look.Mat(i % 3 == 0 ? colour : Look.Grey), at, 0.1f).transform;
            bits.Add((bit, new Vector3((float)dice.NextDouble() - 0.5f, (float)dice.NextDouble() * 0.8f + 0.4f, (float)dice.NextDouble() - 0.5f) * 3f));
        }
        float t = 0, d = 0.6f;
        while (t < d)
        {
            t += Time.deltaTime;
            foreach (var (b, v) in bits) { if (b == null) continue; b.position += (v + Vector3.down * 5f * t) * Time.deltaTime; b.localScale = Vector3.one * 0.1f * (1 - t / d); }
            yield return null;
        }
        foreach (var (b, _) in bits) if (b != null) Destroy(b.gameObject);
    }

    // The cover: the attacker paces the corridor and the door lights breathe.
    public void Demo(bool on) { if (on) StartCoroutine(Pace()); }
    IEnumerator Pace()
    {
        attacker.gameObject.SetActive(true);
        while (true)
        {
            yield return WalkTo(ExitPoint, 9f);
            yield return new WaitForSeconds(0.6f);
            attacker.rotation = Quaternion.LookRotation(Vector3.left);
            yield return WalkTo(Entrance, 9f);
            attacker.rotation = Quaternion.LookRotation(Vector3.right);
            yield return new WaitForSeconds(0.6f);
        }
    }
}
