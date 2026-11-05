using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// The facility in three dimensions, in lockdown: a corridor with a door to each of
// the twelve rooms, six a side, the exit at the far end. Each door has a strip of
// light (locked, current, held, breached) that lights the corridor, and a sign;
// behind it a small room with a rack, a desk, a screen, the one prop that says what
// the room is, and a board of lamps that says what has been decided in it. The
// attacker walks the corridor when the facility runs the attack, leaving a trail;
// a door that gives way is sparks, a shake and the alarm.
public class Building : MonoBehaviour
{
    public enum DoorState { Locked, Current, Held, Breached, Open }

    public SceneCamera cam;
    public CameraRig rig;
    public Effects effects;
    public RectTransform frame;
    const float Pitch = 4.2f, Width = 5.5f, Back = 5f; // Back: the corridor before the first door, room for the chase camera
    readonly Transform[] doors = new Transform[12];
    readonly Renderer[] strips = new Renderer[12];
    readonly Light[] stripLights = new Light[12];
    readonly Transform[] panels = new Transform[12];
    readonly Light[] roomLamps = new Light[12], deskLamps = new Light[12];
    readonly Transform[] boards = new Transform[12];
    readonly DoorState[] states = new DoorState[12];
    Transform attacker, exitLight, exitRoot;
    Light attackerLight;
    LineRenderer trail;
    readonly List<Vector3> trailPoints = new List<Vector3>();
    float alarmUntil, dustClock;
    readonly System.Random dice = new System.Random(5);

    public float Length => 6 * Pitch + 4f;
    public Bounds Bounds => new Bounds(new Vector3((Length - 2f - Back) / 2f, 1.2f, 0), new Vector3(Length + Back, 2.4f, Width + 4f));
    public Vector3 DoorPosition(int room) => doors[room].position;
    public Transform DoorTransform(int room) => doors[room];
    public Transform ExitTransform => exitRoot;
    public Transform AttackerTransform => attacker;
    public DoorState StateOf(int room) => states[room];

    public void Build()
    {
        Clear();
        gameObject.SetActive(true);
        BuildLight();
        BuildCorridor();
        for (int i = 0; i < 12; i++) BuildRoom(i);
        BuildExit();
        BuildAttacker();
        BuildTrail();
        attacker.gameObject.SetActive(false);
        alarmUntil = 0f;
    }

    public void Clear()
    {
        StopAllCoroutines();
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        trailPoints.Clear();
        trail = null;
    }

    public void Hide() { Clear(); gameObject.SetActive(false); if (cam != null) cam.Sleep(); }

    public void ShowOverview(RectTransform frame) { this.frame = frame; Canvas.ForceUpdateCanvases(); cam.Overview(frame, Bounds); cam.pitch = 48; cam.yaw = 28; cam.Snap(); }
    public void ShowOrbit(RectTransform frame) { this.frame = frame; Canvas.ForceUpdateCanvases(); cam.Orbit(frame, Bounds.center); cam.orbitDistance = 24f; cam.orbitPitch = 28f; cam.Snap(); }

    // A slow turn around one door, close: the debrief's window.
    public void ShowOrbitOf(RectTransform frame, int room)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        cam.Orbit(frame, doors[room].position + doors[room].forward * 0.6f + Vector3.up * 0.9f);
        cam.orbitDistance = 15f; cam.orbitPitch = 36f; cam.Snap();
    }

    // One door and its neighbours from the corridor, this level's lit amber: the briefing's window.
    public void ShowDoor(RectTransform frame, int room)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        var d = doors[room];
        cam.Overview(frame, new Bounds(d.position + d.forward * 1.2f + Vector3.up * 1.1f, new Vector3(7.5f, 2.6f, 4f)));
        cam.pitch = 22; cam.yaw = room % 2 == 0 ? 210 : 30; cam.Snap();
    }

    // Looks down into the room from the corridor side: the walls are low enough to see
    // the rack, the desk and the screen over them. The room's own lamps come on.
    public void ShowRoom(RectTransform frame, int room)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        var centre = doors[room].TransformPoint(new Vector3(0, 0.9f, -1.9f));
        cam.Overview(frame, new Bounds(centre, new Vector3(4.4f, 2.6f, 4.2f)));
        cam.pitch = 50; cam.yaw = room % 2 == 0 ? 200 : 20; cam.Snap();
        for (int i = 0; i < 12; i++) { if (roomLamps[i] != null) roomLamps[i].enabled = i == room; if (deskLamps[i] != null) deskLamps[i].enabled = i == room; }
    }

    // Every room's ceiling lamp on: the cover picture (more lights than the web allows per view).
    public void LightRooms(bool on) { for (int i = 0; i < 12; i++) if (roomLamps[i] != null) roomLamps[i].enabled = on; }

    // The rig's moves, for the attack.
    public void OpeningShot(RectTransform frame, float seconds) { this.frame = frame; attacker.gameObject.SetActive(true); rig.Opening(attacker, frame, seconds); }
    public void ChaseAttacker(RectTransform frame) { this.frame = frame; attacker.gameObject.SetActive(true); rig.Chase(attacker, frame); }
    public void MomentAround(Transform subject, float seconds, System.Action done, bool slow = true) => rig.FinishMoment(subject, seconds, done, slow);
    public void BackToOverview() { rig.Release(); cam.Overview(frame, Bounds); cam.pitch = 48; cam.yaw = 28; }

    void BuildLight()
    {
        var moonGo = new GameObject("Moon"); moonGo.transform.SetParent(transform, false);
        var moon = moonGo.AddComponent<Light>();
        moon.type = LightType.Directional; moon.shadows = LightShadows.Soft; moon.shadowStrength = 0.7f;
        moon.transform.rotation = Quaternion.Euler(50, -60, 0); moon.color = Look.Moon; moon.intensity = 1.2f;
        var dome = MeshKit.Part(transform, "sky", MeshKit.Sphere, null, Vector3.zero, Vector3.one * 300f);
        var dr = dome.GetComponent<Renderer>(); dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; dr.receiveShadows = false;
        dr.sharedMaterial = Look.Sky(Look.NightTop, Look.NightHorizon, Look.NightBottom);
        var camera = cam.GetComponent<Camera>();
        camera.backgroundColor = Look.NightHorizon;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Theme.Hex("#3C5A8C"); RenderSettings.ambientEquatorColor = Theme.Hex("#46527A"); RenderSettings.ambientGroundColor = Theme.Hex("#1A2238");
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogColor = Look.Haze; RenderSettings.fogStartDistance = 50f; RenderSettings.fogEndDistance = 160f;
        Look.SetSky(Look.NightTop, Look.NightHorizon);
    }

    static GameObject NoShadow(GameObject go) { go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; return go; }

    void BuildCorridor()
    {
        float len = Length;
        NoShadow(MeshKit.Part(transform, "ground", MeshKit.Box, Look.Mat(Look.Ground), new Vector3(len / 2 - 2, -0.3f, 0), new Vector3(120, 0.2f, 120)));
        // The floor in one piece per door, so every piece is lit by the strips nearest it (four lights an object).
        for (int i = 0; i <= 6; i++)
        {
            float x0 = Mathf.Max(-Back, i * Pitch - Pitch / 2), x1 = Mathf.Min(len - 2f, i * Pitch + Pitch / 2);
            NoShadow(MeshKit.Part(transform, "floor", MeshKit.Box, Look.Mat(Look.Floor), new Vector3((x0 + x1) / 2, -0.05f, 0), new Vector3(x1 - x0, 0.1f, Width)));
        }
        for (int i = 0; i < 7; i++) NoShadow(MeshKit.Part(transform, "line", MeshKit.Box, Look.Mat(Look.FloorLine, Look.FloorLine * 0.35f), new Vector3(i * Pitch, 0.005f, 0), new Vector3(0.05f, 0.01f, Width)));
        // The walls, in segments between doors, both sides; a skirting along the floor.
        for (int side = -1; side <= 1; side += 2)
        {
            float z = side * Width / 2;
            for (int i = 0; i <= 6; i++)
            {
                float x0 = i * Pitch - Pitch / 2 + (i == 0 ? -1.2f : 0.9f), x1 = i * Pitch + Pitch / 2 - (i == 6 ? -1.2f : 0.9f);
                if (i == 6) x1 = len - 2;
                if (i == 0) x0 = -Back;
                float mid = (x0 + x1) / 2, w = x1 - x0;
                if (w <= 0) continue;
                MeshKit.Part(transform, "wall", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(mid, 1.2f, z), new Vector3(w, 2.4f, 0.3f));
                MeshKit.Part(transform, "skirt", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(mid, 0.08f, z - side * 0.16f), new Vector3(w, 0.16f, 0.04f));
            }
            // The lintels over the doors
            for (int i = 0; i < 6; i++) MeshKit.Part(transform, "lintel", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(i * Pitch, 2.1f, z), new Vector3(1.8f, 0.6f, 0.3f));
        }
        // The near end: the entrance, open: two short returns of wall and a threshold strip, so the
        // chase camera behind the attacker looks in from outside.
        for (int side = -1; side <= 1; side += 2) MeshKit.Part(transform, "return", MeshKit.Box, Look.Mat(Look.WallDark), new Vector3(-Back, 1.2f, side * (Width / 2 - 0.5f)), new Vector3(0.3f, 2.4f, 1.3f));
        NoShadow(MeshKit.Part(transform, "threshold", MeshKit.Box, Look.Mat(Look.DoorFrame, Look.DoorFrame * 0.35f, 0.2f), new Vector3(-Back, 0.005f, 0), new Vector3(0.1f, 0.01f, Width - 2.6f)));
    }

    // One room: its door in the corridor wall, the strip above it, the sign, and the
    // room behind: a rack with LEDs, a desk with a glowing screen, a chair, a lamp, the
    // room's prop, and the board of lamps for the decisions.
    void BuildRoom(int i)
    {
        int side = i % 2 == 0 ? -1 : 1;          // even rooms on the left wall, odd on the right
        float x = (i / 2) * Pitch, z = side * Width / 2;
        var root = new GameObject("Room " + Facility.Rooms[i]).transform;
        root.SetParent(transform, false);
        root.localPosition = new Vector3(x, 0, z);
        root.localRotation = Quaternion.LookRotation(new Vector3(0, 0, -side)); // forward points into the corridor
        // The door panel, in the wall, sliding open when the room is visited.
        var panel = MeshKit.Part(root, "door", MeshKit.Box, Look.Mat(Look.Door, Color.black, 0.3f), new Vector3(0, 0.9f, 0), new Vector3(1.6f, 1.8f, 0.12f));
        panels[i] = panel.transform;
        var frameMat = Look.Mat(Look.DoorFrame, Look.DoorFrame * 0.35f, 0.2f);
        MeshKit.Part(root, "frameL", MeshKit.Box, frameMat, new Vector3(-0.86f, 0.95f, 0), new Vector3(0.1f, 1.9f, 0.34f));
        MeshKit.Part(root, "frameR", MeshKit.Box, frameMat, new Vector3(0.86f, 0.95f, 0), new Vector3(0.1f, 1.9f, 0.34f));
        var strip = MeshKit.Part(root, "strip", MeshKit.Box, Look.Own(Look.Locked, Look.Locked * 1.8f), new Vector3(0, 1.98f, 0.17f), new Vector3(1.2f, 0.08f, 0.06f));
        strips[i] = strip.GetComponent<Renderer>();
        NoShadow(strip);
        var point = strip.AddComponent<Light>(); point.type = LightType.Point; point.range = 3.6f; point.intensity = 1.6f; point.color = Look.Locked; point.shadows = LightShadows.None;
        stripLights[i] = point;
        states[i] = DoorState.Locked;
        // The sign
        MeshKit.Part(root, "sign", MeshKit.Box, Look.Mat(Look.Sign), new Vector3(0, 2.25f, 0.17f), new Vector3(1.7f, 0.3f, 0.04f));
        var text = new GameObject("name"); text.transform.SetParent(root, false); text.transform.localPosition = new Vector3(0, 2.25f, 0.2f); text.transform.localRotation = Quaternion.Euler(0, 180, 0);
        var tmp = text.AddComponent<TextMeshPro>(); tmp.font = Theme.Bold; tmp.text = Facility.Rooms[i].ToUpperInvariant(); tmp.fontSize = 1.6f; tmp.color = Theme.Ink; tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(1.8f, 0.3f); tmp.characterSpacing = 4;
        // The room behind the wall
        var inside = new GameObject("inside").transform; inside.SetParent(root, false); inside.localPosition = new Vector3(0, 0, -1.9f);
        NoShadow(MeshKit.Part(inside, "floor", MeshKit.Box, Look.Mat(Look.WallDark), new Vector3(0, -0.04f, 0), new Vector3(3.6f, 0.08f, 3.4f)));
        MeshKit.Part(inside, "back", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(0, 1.2f, -1.75f), new Vector3(3.6f, 2.4f, 0.1f));
        MeshKit.Part(inside, "sideL", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(-1.8f, 1.2f, 0), new Vector3(0.1f, 2.4f, 3.5f));
        MeshKit.Part(inside, "sideR", MeshKit.Box, Look.Mat(Look.Wall), new Vector3(1.8f, 1.2f, 0), new Vector3(0.1f, 2.4f, 3.5f));
        MeshKit.Part(inside, "rack", MeshKit.Box, Look.Mat(Look.Rack, Color.black, 0.35f), new Vector3(-1.1f, 0.9f, -1.1f), new Vector3(0.7f, 1.8f, 0.6f));
        for (int k = 0; k < 6; k++) NoShadow(MeshKit.Part(inside, "led", MeshKit.Box, Look.Mat(Look.RackLed, Look.RackLed * 1.5f), new Vector3(-1.3f + (k % 2) * 0.4f, 0.3f + (k / 2) * 0.5f, -0.79f), new Vector3(0.06f, 0.04f, 0.02f)));
        MeshKit.Part(inside, "desk", MeshKit.Box, Look.Mat(Look.Desk), new Vector3(0.8f, 0.72f, -0.9f), new Vector3(1.6f, 0.06f, 0.8f));
        MeshKit.Part(inside, "legs", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(0.8f, 0.35f, -0.9f), new Vector3(1.4f, 0.7f, 0.1f));
        NoShadow(MeshKit.Part(inside, "screen", MeshKit.Box, Look.Mat(Look.Screen, Look.Screen * 1.3f, 0.8f), new Vector3(0.8f, 1.05f, -1.15f), new Vector3(0.8f, 0.5f, 0.04f)));
        MeshKit.Part(inside, "chair", MeshKit.Box, Look.Mat(Look.Chair), new Vector3(0.8f, 0.3f, -0.2f), new Vector3(0.5f, 0.6f, 0.5f));
        // The lamps: a cool one on the ceiling, a warm one on the desk; lit only in the room on screen.
        var lampGo = NoShadow(MeshKit.Part(inside, "lamp", MeshKit.Box, Look.Mat(Look.Lamp, Look.Lamp * 0.9f), new Vector3(0, 2.3f, -0.6f), new Vector3(0.5f, 0.04f, 0.14f)));
        var lamp = lampGo.AddComponent<Light>(); lamp.type = LightType.Point; lamp.range = 4.8f; lamp.intensity = 1.5f; lamp.color = Look.Lamp; lamp.shadows = LightShadows.None; lamp.enabled = false;
        roomLamps[i] = lamp;
        var deskGo = NoShadow(MeshKit.Part(inside, "desklamp", MeshKit.Sphere, Look.Mat(Look.DeskLamp, Look.DeskLamp * 1.4f), new Vector3(1.45f, 0.9f, -1.0f), 0.12f));
        MeshKit.Part(inside, "stem", MeshKit.Cylinder, Look.Mat(Look.Skirting), new Vector3(1.45f, 0.8f, -1.0f), new Vector3(0.02f, 0.08f, 0.02f));
        var desk = deskGo.AddComponent<Light>(); desk.type = LightType.Point; desk.range = 2.6f; desk.intensity = 1.1f; desk.color = Look.DeskLamp; desk.shadows = LightShadows.None; desk.enabled = false;
        deskLamps[i] = desk;
        BuildProp(inside, i);
        // The board of lamps, on the back wall over the desk: one lamp per row of the system.
        var board = new GameObject("board").transform; board.SetParent(inside, false); board.localPosition = new Vector3(0.7f, 1.78f, -1.68f);
        boards[i] = board;
        doors[i] = root;
    }

    // The one prop that says what the room is, in the free corner by the door.
    void BuildProp(Transform inside, int i)
    {
        // Free-standing props stand mid-room, wall props on the left wall past the rack: anything
        // nearer the door than that is hidden behind the door wall from where the camera looks.
        var p = new GameObject("prop").transform; p.SetParent(inside, false); p.localPosition = new Vector3(-0.7f, 0, -0.35f);
        var steel = Look.Mat(Look.Steel, Color.black, 0.3f);
        var paper = Look.Mat(Look.Paper);
        switch (i)
        {
            case 0: // Risk Room: a filing cabinet and a register board with coloured cards
                MeshKit.Part(p, "cabinet", MeshKit.Box, steel, new Vector3(0, 0.55f, 0), new Vector3(0.5f, 1.1f, 0.5f));
                for (int k = 0; k < 3; k++) MeshKit.Part(p, "drawer", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(0, 0.2f + k * 0.35f, 0.26f), new Vector3(0.4f, 0.04f, 0.02f));
                for (int k = 0; k < 6; k++) NoShadow(MeshKit.Part(p, "card", MeshKit.Box, Look.Mat(k == 4 ? Look.Breached : k % 2 == 0 ? Look.Held : Look.Current), new Vector3(-0.78f, 1.0f + (k / 3) * 0.3f, -0.45f + (k % 3) * 0.3f), new Vector3(0.02f, 0.2f, 0.22f)));
                break;
            case 1: // Suspicious Inbox: a mail sorter, one red envelope sticking out
                MeshKit.Part(p, "sorter", MeshKit.Box, Look.Mat(Look.Desk), new Vector3(0, 0.6f, 0), new Vector3(0.9f, 1.0f, 0.35f));
                for (int k = 0; k < 6; k++) NoShadow(MeshKit.Part(p, "envelope", MeshKit.Box, k == 2 ? Look.Mat(Look.Breached, Look.Breached * 0.8f) : paper, new Vector3(-0.28f + (k % 3) * 0.28f, 0.42f + (k / 3) * 0.36f, k == 2 ? 0.26f : 0.19f), new Vector3(0.22f, 0.14f, 0.02f)));
                break;
            case 2: // Password Vault: a safe with a teal dial
                MeshKit.Part(p, "safe", MeshKit.Box, Look.Mat(Look.Skirting, Color.black, 0.6f), new Vector3(0, 0.45f, 0), new Vector3(0.8f, 0.9f, 0.7f));
                MeshKit.Part(p, "dial", MeshKit.Cylinder, Look.Mat(Look.DoorFrame, Look.DoorFrame * 0.6f, 0.7f), new Vector3(0.1f, 0.5f, 0.37f), new Vector3(90, 0, 0), new Vector3(0.18f, 0.03f, 0.18f));
                MeshKit.Part(p, "handle", MeshKit.Box, steel, new Vector3(-0.22f, 0.5f, 0.37f), new Vector3(0.06f, 0.3f, 0.04f));
                break;
            case 3: // Malware Containment: a glass cell with something red inside
                NoShadow(MeshKit.Part(p, "cell", MeshKit.Box, Look.Glass(new Color(0.6f, 0.9f, 0.85f, 0.35f)), new Vector3(0, 0.5f, 0), new Vector3(0.8f, 1.0f, 0.8f)));
                NoShadow(MeshKit.Part(p, "blob", MeshKit.Sphere, Look.Mat(Look.Breached, Look.Breached * 1.4f), new Vector3(0, 0.35f, 0), 0.4f));
                MeshKit.Part(p, "base", MeshKit.Box, steel, new Vector3(0, 0.03f, 0), new Vector3(0.9f, 0.06f, 0.9f));
                break;
            case 4: // Identity Chamber: a badge gate with a glowing reader
                MeshKit.Part(p, "postL", MeshKit.Box, steel, new Vector3(-0.4f, 0.5f, 0), new Vector3(0.12f, 1.0f, 0.12f));
                MeshKit.Part(p, "postR", MeshKit.Box, steel, new Vector3(0.4f, 0.5f, 0), new Vector3(0.12f, 1.0f, 0.12f));
                NoShadow(MeshKit.Part(p, "bar", MeshKit.Box, Look.Mat(Look.Held, Look.Held * 1.2f), new Vector3(0, 0.6f, 0), new Vector3(0.7f, 0.05f, 0.05f)));
                NoShadow(MeshKit.Part(p, "reader", MeshKit.Box, Look.Mat(Look.Screen, Look.Screen * 1.2f), new Vector3(0.4f, 0.85f, 0.08f), new Vector3(0.1f, 0.14f, 0.03f)));
                break;
            case 5: // Patch Laboratory: a workbench with three small servers, one amber
                MeshKit.Part(p, "bench", MeshKit.Box, Look.Mat(Look.Desk), new Vector3(0, 0.75f, 0), new Vector3(1.0f, 0.06f, 0.5f));
                MeshKit.Part(p, "benchlegs", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(0, 0.37f, 0), new Vector3(0.9f, 0.7f, 0.08f));
                for (int k = 0; k < 3; k++)
                {
                    MeshKit.Part(p, "server", MeshKit.Box, Look.Mat(Look.Rack), new Vector3(-0.3f + k * 0.3f, 0.88f, 0), new Vector3(0.24f, 0.2f, 0.36f));
                    NoShadow(MeshKit.Part(p, "serverled", MeshKit.Box, Look.Mat(k == 1 ? Look.Current : Look.RackLed, (k == 1 ? Look.Current : Look.RackLed) * 1.5f), new Vector3(-0.3f + k * 0.3f, 0.92f, 0.19f), new Vector3(0.05f, 0.03f, 0.01f)));
                }
                break;
            case 6: // Network Maze: two patch panels and the cables between them
                MeshKit.Part(p, "panelL", MeshKit.Box, Look.Mat(Look.Rack), new Vector3(-0.45f, 0.6f, 0), new Vector3(0.1f, 1.0f, 0.5f));
                MeshKit.Part(p, "panelR", MeshKit.Box, Look.Mat(Look.Rack), new Vector3(0.45f, 0.6f, 0), new Vector3(0.1f, 1.0f, 0.5f));
                for (int k = 0; k < 6; k++) NoShadow(MeshKit.Part(p, "cable", MeshKit.Cylinder, Look.Mat(k % 3 == 0 ? Look.Cable : k % 3 == 1 ? Look.Held : Look.Current), new Vector3(0, 0.3f + k * 0.12f, -0.15f + (k % 2) * 0.3f), new Vector3(0, 0, 90), new Vector3(0.03f, 0.4f, 0.03f)));
                break;
            case 7: // Firewall Control: a wall panel of switches, teal and red
                p.localPosition = new Vector3(-1.72f, 0, -0.2f);
                MeshKit.Part(p, "panel", MeshKit.Box, Look.Mat(Look.Rack, Color.black, 0.3f), new Vector3(0, 1.2f, 0), new Vector3(0.06f, 0.9f, 1.2f));
                for (int k = 0; k < 8; k++) NoShadow(MeshKit.Part(p, "switch", MeshKit.Box, Look.Mat(k == 6 ? Look.Breached : Look.Held, (k == 6 ? Look.Breached : Look.Held) * 1.3f), new Vector3(0.04f, 1.0f + (k / 4) * 0.35f, -0.45f + (k % 4) * 0.3f), new Vector3(0.03f, 0.12f, 0.08f)));
                break;
            case 8: // Encryption Chamber: a lockbox with a padlock on it and the key beside
                MeshKit.Part(p, "box", MeshKit.Box, Look.Mat(Look.Exit, Color.black, 0.5f), new Vector3(0, 0.25f, 0), new Vector3(0.7f, 0.5f, 0.5f));
                MeshKit.Part(p, "lockbody", MeshKit.Box, Look.Mat(Look.Sign, Color.black, 0.6f), new Vector3(0, 0.62f, 0), new Vector3(0.24f, 0.2f, 0.12f));
                MeshKit.Part(p, "shackle", MeshKit.Box, steel, new Vector3(0, 0.8f, 0), new Vector3(0.16f, 0.16f, 0.04f));
                MeshKit.Part(p, "shacklehole", MeshKit.Box, Look.Mat(Look.Exit), new Vector3(0, 0.8f, 0), new Vector3(0.08f, 0.1f, 0.05f));
                MeshKit.Part(p, "key", MeshKit.Box, Look.Mat(Look.DeskLamp, Look.DeskLamp * 0.5f, 0.6f), new Vector3(0.5f, 0.03f, 0.1f), new Vector3(0, 30, 0), new Vector3(0.3f, 0.03f, 0.06f));
                break;
            case 9: // Cloud Chamber: a cloud over a pedestal
                MeshKit.Part(p, "pedestal", MeshKit.Cylinder, steel, new Vector3(0, 0.3f, 0), new Vector3(0.3f, 0.3f, 0.3f));
                foreach (var (dx, dy, dz, r) in new[] { (0f, 0f, 0f, 0.5f), (-0.3f, -0.08f, 0f, 0.38f), (0.3f, -0.06f, 0f, 0.4f), (0f, 0.08f, -0.25f, 0.36f), (0.1f, 0.1f, 0.22f, 0.34f) })
                    NoShadow(MeshKit.Part(p, "cloud", MeshKit.Sphere, Look.Mat(Look.Sign, Look.Sign * 0.25f, 0.5f), new Vector3(dx, 1.25f + dy, dz), r));
                break;
            case 10: // Operations Center: a wall of screens
                p.localPosition = new Vector3(-1.72f, 0, -0.2f);
                for (int k = 0; k < 6; k++)
                {
                    var tint = k % 3 == 0 ? Look.Screen : k % 3 == 1 ? Look.Held : Look.Current;
                    NoShadow(MeshKit.Part(p, "screen", MeshKit.Box, Look.Mat(tint, tint * 1.1f, 0.8f), new Vector3(0, 0.95f + (k / 3) * 0.5f, -0.6f + (k % 3) * 0.55f), new Vector3(0.04f, 0.4f, 0.48f)));
                }
                break;
            default: // Backup Vault: shelves of tapes
                p.localPosition = new Vector3(-1.7f, 0, -0.2f);
                for (int s = 0; s < 3; s++)
                {
                    MeshKit.Part(p, "shelf", MeshKit.Box, Look.Mat(Look.Desk), new Vector3(0.15f, 0.4f + s * 0.45f, 0), new Vector3(0.3f, 0.04f, 1.4f));
                    for (int k = 0; k < 5; k++) MeshKit.Part(p, "tape", MeshKit.Box, Look.Mat(k == 2 && s == 1 ? Look.Sign : Look.Tape), new Vector3(0.15f, 0.55f + s * 0.45f, -0.55f + k * 0.27f), new Vector3(0.22f, 0.26f, 0.2f));
                }
                break;
        }
    }

    // The board of lamps in the room on screen: one per row, teal when every decision
    // in that row is made, amber while one is still "?".
    public void ShowConfig(int room, IList<bool> decided)
    {
        var board = boards[room];
        if (board == null) return;
        for (int i = board.childCount - 1; i >= 0; i--) Destroy(board.GetChild(i).gameObject);
        if (decided == null || decided.Count == 0) return;
        int n = Mathf.Min(decided.Count, 10);
        float step = 0.19f, w = n * step + 0.1f;
        MeshKit.Part(board, "plate", MeshKit.Box, Look.Mat(Look.Rack, Color.black, 0.3f), new Vector3(0, 0, 0), new Vector3(w, 0.3f, 0.04f));
        for (int i = 0; i < n; i++)
        {
            var c = decided[i] ? Look.Decided : Look.Undecided;
            NoShadow(MeshKit.Part(board, "lamp", MeshKit.Box, Look.Mat(c, c * 1.6f), new Vector3(-w / 2 + 0.05f + step / 2 + i * step, 0, 0.03f), new Vector3(0.12f, 0.12f, 0.02f)));
        }
    }

    void BuildExit()
    {
        float x = Length - 2;
        var root = new GameObject("Exit").transform; root.SetParent(transform, false); root.localPosition = new Vector3(x, 0, 0); root.localRotation = Quaternion.LookRotation(Vector3.left);
        exitRoot = root;
        MeshKit.Part(root, "wall", MeshKit.Box, Look.Mat(Look.WallDark), new Vector3(0, 1.2f, 0), new Vector3(Width + 0.3f, 2.4f, 0.3f));
        MeshKit.Part(root, "door", MeshKit.Box, Look.Mat(Look.Exit, Color.black, 0.3f), new Vector3(0, 1.0f, 0.18f), new Vector3(2.4f, 2.0f, 0.1f));
        MeshKit.Part(root, "frame", MeshKit.Box, Look.Mat(Look.DoorFrame, Look.DoorFrame * 0.35f, 0.2f), new Vector3(0, 2.05f, 0.18f), new Vector3(2.6f, 0.1f, 0.3f));
        var light = NoShadow(MeshKit.Part(root, "light", MeshKit.Box, Look.Own(Look.Locked, Look.Locked * 1.8f), new Vector3(0, 2.15f, 0.3f), new Vector3(2.2f, 0.1f, 0.08f)));
        exitLight = light.transform;
        var point = light.AddComponent<Light>(); point.type = LightType.Point; point.range = 5f; point.intensity = 1.8f; point.color = Look.Locked; point.shadows = LightShadows.None;
        var text = new GameObject("exit"); text.transform.SetParent(root, false); text.transform.localPosition = new Vector3(0, 2.4f, 0.3f); text.transform.localRotation = Quaternion.Euler(0, 180, 0);
        var tmp = text.AddComponent<TextMeshPro>(); tmp.font = Theme.Bold; tmp.text = "EXIT"; tmp.fontSize = 3f; tmp.color = Look.Sign; tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(3f, 0.6f); tmp.characterSpacing = 8;
    }

    // The attacker: a hooded figure in shadow, the visor the one thing lit on it, a bag
    // on its back, and a scarlet light it carries down the corridor.
    void BuildAttacker()
    {
        attacker = new GameObject("Attacker").transform;
        attacker.SetParent(transform, false);
        attacker.localPosition = Entrance;
        attacker.localRotation = Quaternion.LookRotation(Vector3.right); // facing down the corridor
        var dark = Look.Mat(Look.Attacker, Color.black, 0.55f); // glossy enough to catch the moon and the strips
        MeshKit.Part(attacker, "body", MeshKit.Capsule, dark, new Vector3(0, 0.78f, 0), new Vector3(0.56f, 1.5f, 0.5f));
        MeshKit.Part(attacker, "shoulders", MeshKit.Box, dark, new Vector3(0, 1.15f, 0), new Vector3(0.72f, 0.18f, 0.44f));
        NoShadow(MeshKit.Part(attacker, "band", MeshKit.Box, Look.Mat(Look.Visor, Look.Visor * 1.2f), new Vector3(0, 1.17f, 0), new Vector3(0.75f, 0.1f, 0.47f))); // a scarlet band on the shoulders
        NoShadow(MeshKit.Part(attacker, "bagband", MeshKit.Box, Look.Mat(Look.Visor, Look.Visor * 1.2f), new Vector3(0, 0.95f, -0.43f), new Vector3(0.3f, 0.08f, 0.02f))); // and on the bag, seen from behind
        var ring = NoShadow(MeshKit.Part(attacker, "ring", MeshKit.Cylinder, Look.Glass(Look.Trail), new Vector3(0, 0.015f, 0), new Vector3(1.1f, 0.01f, 1.1f))); // its mark on the floor, from any angle
        ring.GetComponent<Renderer>().receiveShadows = false;
        MeshKit.Part(attacker, "armL", MeshKit.Capsule, dark, new Vector3(-0.38f, 0.8f, 0.05f), new Vector3(0, 0, 10), new Vector3(0.16f, 0.6f, 0.16f));
        MeshKit.Part(attacker, "armR", MeshKit.Capsule, dark, new Vector3(0.38f, 0.8f, 0.05f), new Vector3(0, 0, -10), new Vector3(0.16f, 0.6f, 0.16f));
        MeshKit.Part(attacker, "hood", MeshKit.Sphere, dark, new Vector3(0, 1.48f, -0.02f), 0.52f);
        MeshKit.Part(attacker, "brim", MeshKit.Box, dark, new Vector3(0, 1.6f, 0.16f), new Vector3(0.44f, 0.08f, 0.22f));
        var visor = NoShadow(MeshKit.Part(attacker, "visor", MeshKit.Box, Look.Mat(Look.Visor, Look.Visor * 2.2f, 0.9f), new Vector3(0, 1.46f, 0.22f), new Vector3(0.3f, 0.06f, 0.1f)));
        attackerLight = visor.AddComponent<Light>(); attackerLight.type = LightType.Point; attackerLight.range = 3f; attackerLight.intensity = 1.1f; attackerLight.color = Look.Visor; attackerLight.shadows = LightShadows.None;
        MeshKit.Part(attacker, "bag", MeshKit.Box, Look.Mat(Look.Skirting), new Vector3(0, 0.95f, -0.32f), new Vector3(0.42f, 0.52f, 0.2f));
    }

    void BuildTrail()
    {
        var go = new GameObject("Trail");
        go.transform.SetParent(transform, false);
        trail = go.AddComponent<LineRenderer>();
        trail.sharedMaterial = Look.Glass(Look.Trail);
        trail.startWidth = trail.endWidth = 0.14f;
        trail.useWorldSpace = true;
        trail.alignment = LineAlignment.TransformZ;
        go.transform.rotation = Quaternion.Euler(90, 0, 0); // flat on the floor
        trail.numCornerVertices = 3; trail.numCapVertices = 3;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; trail.receiveShadows = false;
        trail.positionCount = 0;
    }

    void Mark()
    {
        if (trail == null) return;
        var p = attacker.position + Vector3.up * 0.02f;
        if (trailPoints.Count > 0 && (trailPoints[trailPoints.Count - 1] - p).sqrMagnitude < 0.09f) return;
        trailPoints.Add(p);
        trail.positionCount = trailPoints.Count;
        trail.SetPosition(trailPoints.Count - 1, p);
    }

    public void SetDoor(int room, DoorState state)
    {
        if (strips[room] == null) return;
        states[room] = state;
        var c = state == DoorState.Held || state == DoorState.Open ? Look.Held : state == DoorState.Current ? Look.Current : state == DoorState.Breached ? Look.Breached : Look.Locked;
        strips[room].sharedMaterial = Look.Own(c, c * 1.8f);
        if (stripLights[room] != null) { stripLights[room].color = c; stripLights[room].intensity = state == DoorState.Breached ? 2.4f : 1.6f; }
        // An open door slides aside; a locked one is shut.
        if (panels[room] != null) panels[room].localPosition = new Vector3(state == DoorState.Open || state == DoorState.Breached || state == DoorState.Current ? -1.4f : 0, 0.9f, 0);
    }

    public void SetExit(bool open)
    {
        if (exitLight == null) return;
        var c = open ? Look.Held : Look.Locked;
        exitLight.GetComponent<Renderer>().sharedMaterial = Look.Own(c, c * 1.8f);
        var point = exitLight.GetComponent<Light>(); if (point != null) { point.color = c; point.intensity = open ? 2.6f : 1.8f; }
    }

    // The alarm: for these seconds every locked door's strip strobes scarlet.
    public void Alarm(float seconds) => alarmUntil = Mathf.Max(alarmUntil, Time.unscaledTime + seconds);
    public bool AlarmOn => Time.unscaledTime < alarmUntil;

    void Update()
    {
        bool on = AlarmOn;
        float k = on ? 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 14f) : 0f;
        for (int i = 0; i < 12; i++)
        {
            if (stripLights[i] == null || states[i] != DoorState.Locked) continue;
            stripLights[i].intensity = Mathf.Lerp(1.6f, 3.2f, k);
            stripLights[i].color = Color.Lerp(Look.Locked, Look.Breached, k);
        }
        if (attackerLight != null) attackerLight.intensity = on ? Mathf.Lerp(1.1f, 2.2f, k) : 1.1f;
    }

    // The attacker walks to a room's door; true when the door gives.
    public IEnumerator Visit(int room, bool through, float seconds)
    {
        attacker.gameObject.SetActive(true);
        var door = doors[room];
        var target = door.position + door.forward * 1.2f;
        yield return Walk(target, seconds);
        var face = door.position - attacker.position; face.y = 0;
        attacker.rotation = Quaternion.LookRotation(face);
        if (through)
        {
            SetDoor(room, DoorState.Breached);
            var at = door.position + Vector3.up * 1f;
            effects?.Sparks(at, 1.2f);
            StartCoroutine(Puff(at, Look.Breached));
            rig?.Shake(1f);
            Atmosphere.Current?.Pulse(Theme.Danger, 0.5f, 0.6f);
            Atmosphere.Current?.HitStop(0.08f);
            Alarm(2.6f);
            Sfx.Crash();
            float s = 0; var p0 = attacker.position;
            while (s < 0.5f) { s += Time.deltaTime; attacker.position = Vector3.Lerp(p0, door.position + door.forward * 0.3f, s / 0.5f); Mark(); yield return null; }
        }
        else
        {
            SetDoor(room, DoorState.Held);
            effects?.Motes(door.position + Vector3.up * 1.2f);
            Sfx.Clack();
            float s = 0; var p0 = attacker.position;
            while (s < 0.35f) { s += Time.deltaTime; attacker.position = p0 + door.forward * Mathf.Sin(s / 0.35f * Mathf.PI) * -0.3f; yield return null; }
            attacker.position = p0;
        }
    }

    public IEnumerator WalkTo(Vector3 target, float seconds) { attacker.gameObject.SetActive(true); yield return Walk(target, seconds); }

    // A walk with a bob, the trail behind, dust under the feet.
    IEnumerator Walk(Vector3 target, float seconds)
    {
        var from = attacker.position; float t = 0;
        var flat = target - from; flat.y = 0;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0, 1, t / seconds);
            attacker.position = Vector3.Lerp(from, target, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * seconds * 11f)) * 0.03f;
            if (flat.sqrMagnitude > 0.01f) attacker.rotation = Quaternion.Slerp(attacker.rotation, Quaternion.LookRotation(flat), 0.2f);
            Mark();
            dustClock += Time.deltaTime;
            if (dustClock > 0.14f) { dustClock = 0f; effects?.Dust(attacker.position + Vector3.up * 0.05f); }
            yield return null;
        }
    }

    public Vector3 Entrance => new Vector3(-Back + 0.6f, 0, 0);
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
        trail = null; // no trail on the cover
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
