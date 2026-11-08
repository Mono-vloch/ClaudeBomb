using UnityEngine;

// The shopping agent's body: four rigged copies of the same robot (made with Higgsfield,
// see Docs/credits.md), one per clip, because separate rigging jobs do not share bone
// orientations: a walk in place (Market moves it), a breathing look around (slow, it is the
// idle; quicker from its start, the LOOK of the loop), picking something off a stall (a buy)
// and holding its head (sent home). Meshy's own idle clip turns the hips through 90 degrees
// and was dropped. Only one copy shows at a time and they share one material, tinted per team: the robot's
// coral panels are a mask (URP Lit's detail mask), and the detail albedo carries the team
// colour. The antenna (its bulb is the brain, its glow the effort), the basket on its back
// and what it remembers follow the bones of whichever copy is showing.
public class AgentRig : MonoBehaviour
{
    public enum Pose { Idle, Walk, Look, PickUp, SentHome }
    static readonly string[] Names = { "Idle", "Walk", "Look", "PickUp", "SentHome" };

    public Transform antenna, bulb, basket, goods, memory; // made by the kit, followed here
    public Transform noteMount, hipMount, hipLeftMount, faceMount, headMount, handMount; // mounts made here: the note on its chest, the battery on a hip, the lens on its eye, the brain's chip on its head, the notebook in its right hand
    public Renderer bulbRenderer;

    readonly Animator[] copies = new Animator[5];
    // Each copy sits on a pivot: played in the game, the Meshy clips can turn the whole body a
    // quarter round from the robot's root (the Editor's preview does not show it), so each copy
    // measures its torso's facing on its first animated frames and its pivot turns it back.
    readonly Transform[] pivot = new Transform[5], hips = new Transform[5];
    readonly Quaternion[] hipsBind = new Quaternion[5]; // the hips in the robot's space, in the bind pose (facing +Z)
    readonly Vector3[] facingSum = new Vector3[5];
    readonly int[] framesOn = new int[5];
    readonly bool[] calibrated = new bool[5];
    readonly Transform[] head = new Transform[5], headTop = new Transform[5], eyes = new Transform[5], spine = new Transform[5], chest = new Transform[5], hand = new Transform[5], rightHand = new Transform[5];
    Pose current = Pose.Idle;
    float onceUntil = -1f, freezeAt = -1f;
    Pose then;
    Material body, ghostBody, bulbMaterial;
    Renderer[] bodies;
    Texture2D teamTexture;
    Color team = Color.white, glow = Color.black;
    bool ghost, dim, outed;

    void Awake()
    {
        for (int i = 0; i < Names.Length; i++)
        {
            var t = transform.Find(Names[i]);
            if (t == null) continue;
            copies[i] = t.GetComponent<Animator>();
            pivot[i] = new GameObject("Pivot " + Names[i]).transform;
            pivot[i].SetParent(transform, false);
            t.SetParent(pivot[i], false);
            hips[i] = Deep(t, "Hips");
            if (hips[i] != null) hipsBind[i] = Quaternion.Inverse(transform.rotation) * hips[i].rotation; // before any clip has played
            head[i] = Deep(t, "Head"); headTop[i] = Deep(t, "head_end"); eyes[i] = Deep(t, "headfront");
            spine[i] = Deep(t, "Spine01"); chest[i] = Deep(t, "Spine02"); hand[i] = Deep(t, "LeftHand"); rightHand[i] = Deep(t, "RightHand") ?? hand[i];
        }
        noteMount = new GameObject("NoteMount").transform; noteMount.SetParent(transform, false);
        hipMount = new GameObject("HipMount").transform; hipMount.SetParent(transform, false);
        hipLeftMount = new GameObject("HipLeftMount").transform; hipLeftMount.SetParent(transform, false);
        faceMount = new GameObject("FaceMount").transform; faceMount.SetParent(transform, false);
        headMount = new GameObject("HeadMount").transform; headMount.SetParent(transform, false);
        handMount = new GameObject("HandMount").transform; handMount.SetParent(transform, false);
        bodies = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (bodies.Length > 0)
        {
            body = new Material(bodies[0].sharedMaterial) { name = "agent (team)" };
            body.EnableKeyword("_EMISSION");
            body.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            foreach (var r in bodies) r.sharedMaterial = body;
        }
        if (bulbRenderer != null)
        {
            bulbMaterial = new Material(bulbRenderer.sharedMaterial);
            bulbMaterial.EnableKeyword("_EMISSION");
            bulbMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            bulbRenderer.sharedMaterial = bulbMaterial;
        }
        active = CopyOf(Pose.Idle);
        current = Pose.Walk; // so the first Show switches the copies over
        Show(Pose.Idle);
    }

    static Transform Deep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t) { var f = Deep(c, name); if (f != null) return f; }
        return null;
    }

    int active = (int)Pose.Idle;                 // the copy that shows
    int I => active;
    // The copy a pose plays on: the idle is the look-around clip, slowly, when there is no idle copy.
    int CopyOf(Pose p) => copies[(int)p] != null ? (int)p : p == Pose.Idle && copies[(int)Pose.Look] != null ? (int)Pose.Look : (int)Pose.Walk;

    public Pose Current => current;
    public bool Busy => Time.time < onceUntil;

    // Show a pose from its start; the same pose already showing just carries on.
    public void Show(Pose pose, float speed = 1f)
    {
        onceUntil = -1f; freezeAt = -1f;
        int c = CopyOf(pose);
        if (copies[c] == null) return;
        bool idleOnLook = pose == Pose.Idle && c != (int)Pose.Idle;
        if (idleOnLook) speed *= 0.55f; // the look-around, slow, is the robot breathing at rest
        bool same = pose == current && c == active && copies[c].gameObject.activeSelf;
        bool sameCopy = c == active && copies[c].gameObject.activeSelf;
        current = pose;
        active = c;
        for (int i = 0; i < copies.Length; i++)
        {
            if (copies[i] == null) continue;
            bool on = i == c;
            if (copies[i].gameObject.activeSelf != on) copies[i].gameObject.SetActive(on);
            if (on)
            {
                copies[i].speed = speed;
                // Back to rest on the same clip carries on where it is; a new pose starts its clip.
                if (!same && !(idleOnLook && sameCopy)) copies[i].Play(0, 0, 0f);
            }
        }
        Follow();
    }

    // A pose played for a moment, then another (a buy, then idle); hold freezes on its last frame.
    public void Once(Pose pose, float seconds, Pose after, bool hold = false, float speed = 1f)
    {
        Show(pose, speed);
        then = after;
        onceUntil = Time.time + seconds;
        freezeAt = hold ? onceUntil : -1f;
    }

    void Update()
    {
        if (onceUntil > 0f && Time.time >= onceUntil)
        {
            onceUntil = -1f;
            if (freezeAt > 0f) { if (copies[I] != null) copies[I].speed = 0f; freezeAt = -1f; }
            else Show(then);
        }
    }

    void LateUpdate()
    {
        Calibrate();
        Follow();
    }

    // How far the clip has turned the body: the hips' rotation now against their bind pose, in
    // the robot's own space, carried onto +Z (the bind pose's facing), summed over a few
    // animated frames; then the pivot turns the copy back by that much.
    void Calibrate()
    {
        int i = I;
        if (copies[i] == null || calibrated[i] || hips[i] == null) return;
        if (++framesOn[i] < 3) return;
        var now = Quaternion.Inverse(transform.rotation) * Quaternion.Inverse(pivot[i].localRotation) * hips[i].rotation;
        var f = now * Quaternion.Inverse(hipsBind[i]) * Vector3.forward;
        f.y = 0f;
        if (f.sqrMagnitude > 1e-6f) facingSum[i] += f.normalized;
        if (framesOn[i] < 9) return;
        calibrated[i] = true;
        float yaw = Vector3.SignedAngle(facingSum[i], Vector3.forward, Vector3.up);
        if (Mathf.Abs(yaw) > 20f) pivot[i].localRotation = Quaternion.Euler(0f, yaw, 0f);
        Debug.Log($"[AgentRig] {Names[i]}: the clip faced {-yaw:F0} degrees off the robot{(Mathf.Abs(yaw) > 20f ? ", turned back" : "")}");
    }

    // The attachments ride the copy that shows: positions from its bones, the turn of the
    // robot's root, the lean of its spine.
    void Follow()
    {
        int i = I;
        if (copies[i] == null) return;
        var up = chest[i] != null && spine[i] != null ? (chest[i].position - spine[i].position).normalized : transform.up;
        if (up.sqrMagnitude < 0.5f) up = transform.up;
        if (Vector3.Dot(up, Vector3.up) < 0f) up = -up; // these rigs name their spine bones top to bottom
        if (antenna != null && headTop[i] != null)
        {
            var hu = head[i] != null ? (headTop[i].position - head[i].position).normalized : transform.up;
            if (Vector3.Dot(hu, Vector3.up) < 0.3f) hu = transform.up;
            antenna.SetPositionAndRotation(headTop[i].position - hu * 0.03f, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hu), hu));
        }
        if (basket != null && chest[i] != null)
            basket.SetPositionAndRotation(chest[i].position - transform.forward * 0.15f * transform.lossyScale.z - up * 0.04f, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, up), up));
        if (memory != null && headTop[i] != null)
            memory.SetPositionAndRotation(headTop[i].position, transform.rotation); // what it remembers rides beside its head
        var fwdFlat = Vector3.ProjectOnPlane(transform.forward, up).normalized;
        if (fwdFlat.sqrMagnitude < 0.5f) fwdFlat = transform.forward;
        if (noteMount != null && chest[i] != null)
            noteMount.SetPositionAndRotation(chest[i].position + fwdFlat * 0.13f * transform.lossyScale.z - up * 0.02f, Quaternion.LookRotation(fwdFlat, up));
        if (hipMount != null && spine[i] != null)
            hipMount.SetPositionAndRotation(spine[i].position + transform.right * 0.17f * transform.lossyScale.x + fwdFlat * 0.02f, Quaternion.LookRotation(transform.right, up));
        if (hipLeftMount != null && spine[i] != null)
            hipLeftMount.SetPositionAndRotation(spine[i].position - transform.right * 0.19f * transform.lossyScale.x + fwdFlat * 0.14f - up * 0.14f, Quaternion.LookRotation((-transform.right + fwdFlat * 1.3f).normalized, up));
        if (headMount != null && headTop[i] != null)
            headMount.SetPositionAndRotation(headTop[i].position, Quaternion.LookRotation(fwdFlat, up));
        if (handMount != null && rightHand[i] != null)
            handMount.SetPositionAndRotation(rightHand[i].position, Quaternion.LookRotation(fwdFlat, Vector3.up));
        if (faceMount != null && eyes[i] != null)
            faceMount.SetPositionAndRotation(eyes[i].position + fwdFlat * 0.03f, Quaternion.LookRotation(fwdFlat, up));
    }

    public Vector3 AntennaPoint => bulb != null ? bulb.position : transform.position + Vector3.up * 1.3f;
    public Vector3 EyePoint => eyes[I] != null ? eyes[I].position : transform.position + Vector3.up * 1.0f;
    public Vector3 BackPoint => basket != null ? basket.position + Vector3.up * 0.12f : transform.position + Vector3.up * 0.8f;
    public Vector3 HeadTop => headTop[I] != null ? headTop[I].position : transform.position + Vector3.up * 1.2f;
    public Transform Hand => hand[I];

    // The team colour, on the coral panels only (the detail mask); white leaves them grey.
    public void SetTeam(Color c)
    {
        team = c;
        Dress();
    }

    // The bulb's glow: how hard it thinks (the effort at rest, a pulse while it asks Claude).
    public void SetGlow(Color c)
    {
        glow = c;
        if (bulbMaterial != null && !outed) bulbMaterial.SetColor("_EmissionColor", c);
    }

    public void SetGhost(bool on) { if (ghost == on) return; ghost = on; Dress(); }
    public void SetDim(bool on) { if (dim == on) return; dim = on; Dress(); }
    public void SetOut() { if (outed) return; outed = true; if (bulbMaterial != null) bulbMaterial.SetColor("_EmissionColor", Color.black); Dress(); }

    void Dress()
    {
        if (body == null) return;
        // Out or out of tokens: the panels go grey and the eyes dim (red when sent home).
        var c = outed || dim ? Color.Lerp(team, new Color(0.55f, 0.58f, 0.62f), 0.75f) : team;
        if (teamTexture == null) teamTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false, true) { name = "team", wrapMode = TextureWrapMode.Repeat };
        var lin = c.linear * 0.5f; // URP doubles the detail albedo: half of the colour multiplies to the colour
        var px = new Color[16];
        for (int k = 0; k < 16; k++) px[k] = new Color(lin.r, lin.g, lin.b, 1f);
        teamTexture.SetPixels(px); teamTexture.Apply(false);
        body.SetTexture("_DetailAlbedoMap", teamTexture);
        body.SetColor("_BaseColor", outed || dim ? new Color(0.72f, 0.74f, 0.78f) : Color.white);
        body.SetColor("_EmissionColor", outed ? new Color(0.84f, 0.27f, 0.24f) * 1.6f : dim ? Color.white * 0.5f : Color.white * 2.2f);
        if (ghost)
        {
            if (ghostBody == null)
            {
                ghostBody = new Material(body) { name = "agent (unseen)" };
                ghostBody.SetFloat("_Surface", 1f); ghostBody.SetFloat("_Blend", 0f);
                ghostBody.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                ghostBody.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                ghostBody.SetFloat("_ZWrite", 0f);
                ghostBody.SetOverrideTag("RenderType", "Transparent");
                ghostBody.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                ghostBody.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                ghostBody.SetColor("_BaseColor", new Color(0.6f, 0.65f, 0.72f, 0.3f));
            }
            foreach (var r in bodies) { r.sharedMaterial = ghostBody; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; }
        }
        else foreach (var r in bodies) { r.sharedMaterial = body; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
        if (antenna != null) antenna.gameObject.SetActive(!ghost);
        if (basket != null) basket.gameObject.SetActive(!ghost);
    }

    void OnDestroy()
    {
        if (body != null) Destroy(body);
        if (ghostBody != null) Destroy(ghostBody);
        if (bulbMaterial != null) Destroy(bulbMaterial);
        if (teamTexture != null) Destroy(teamTexture);
    }
}
