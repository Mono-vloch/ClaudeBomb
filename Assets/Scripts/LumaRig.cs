using UnityEngine;

// Gives LUMA a skeleton and simple moods. The model (from Meshy) is one solid
// mesh with no bones, so on Awake this builds four - body, head, left and right
// arm - from landmarks in the model's own proportions, attaches every vertex to
// the nearest bone with smooth blending at the shoulders and neck, and drives
// them with procedural motion. It runs on unscaled time: the game is paused
// while LUMA talks. Works on any re-export of the same character (set the axes).
[RequireComponent(typeof(MeshFilter))]
public class LumaRig : MonoBehaviour
{
    public enum Axis { PosX, NegX, PosY, NegY, PosZ, NegZ }
    public enum Mood { Idle, Talk, Wave, Cheer, Concern }

    [SerializeField] Axis meshUp = Axis.PosZ;     // LUMA.fbx from Meshy: Z is up...
    [SerializeField] Axis meshFace = Axis.NegY;   // ...and the face looks down -Y
    [SerializeField] Transform floatRoot;         // bobbed up and down; defaults to the parent
    [SerializeField] float floatHeight = 0.012f;  // world units

    Vector3 up, face, right;                       // mesh-local axes of the upright frame
    Transform head, armL, armR;
    Vector3 floatRest;
    Mood mood = Mood.Idle;
    float moodStart;
    // Current pose, eased toward each mood's target so changes never snap.
    float downL = 98f, downR = 98f, tilt, nod, hop;

    public void SetMood(Mood m)
    {
        if (m == mood) return;
        mood = m;
        moodStart = Time.unscaledTime;
    }

    void Awake()
    {
        if (floatRoot == null) floatRoot = transform.parent;
        if (floatRoot != null) floatRest = floatRoot.localPosition;
        Build();
    }

    static Vector3 Dir(Axis a) => a switch
    {
        Axis.PosX => Vector3.right, Axis.NegX => Vector3.left,
        Axis.PosY => Vector3.up, Axis.NegY => Vector3.down,
        Axis.PosZ => Vector3.forward, _ => Vector3.back,
    };

    void Build()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();
        Mesh mesh = meshFilter.sharedMesh;
        up = Dir(meshUp);
        face = Dir(meshFace);
        right = Vector3.Cross(up, face);

        // Vertices in the upright frame: x right, y up, z toward the face.
        Vector3[] verts = mesh.vertices;
        var u = new Vector3[verts.Length];
        Vector3 min = Vector3.one * float.MaxValue, max = Vector3.one * float.MinValue;
        for (int i = 0; i < verts.Length; i++)
        {
            var v = verts[i];
            u[i] = new Vector3(Vector3.Dot(v, right), Vector3.Dot(v, up), Vector3.Dot(v, face));
            min = Vector3.Min(min, u[i]);
            max = Vector3.Max(max, u[i]);
        }
        Vector3 size = max - min, c = (min + max) / 2f;
        float W = size.x, H = size.y, D = size.z;

        // LUMA's proportions: a round head on an egg-shaped body, arms rising from
        // the shoulders. Hands are found as the left- and right-most vertices.
        var headC = new Vector3(c.x, max.y - 0.29f * H, c.z);
        float headR = 0.29f * H;
        var bodyC = new Vector3(c.x, min.y + 0.21f * H, c.z);
        var bodyR = new Vector3(0.18f * W, 0.226f * H, 0.18f * W);
        var neck = new Vector3(c.x, min.y + 0.437f * H, c.z);
        float armRadius = 0.065f * H;
        Vector3 shoulderL = new Vector3(c.x - 0.294f * W, min.y + 0.432f * H, c.z + 0.093f * D);
        Vector3 shoulderR = new Vector3(c.x + 0.294f * W, shoulderL.y, shoulderL.z);
        // Each arm's line runs from the shoulder to a bit past the hand's centre, so
        // the mitten's tip belongs to the arm too.
        Vector3 handL = Centroid(u, p => p.x - c.x < -0.41f * W);
        Vector3 handR = Centroid(u, p => p.x - c.x > 0.41f * W);
        handL = shoulderL + (handL - shoulderL) * 1.35f;
        handR = shoulderR + (handR - shoulderR) * 1.35f;

        // Bones live on a child that carries the skinned copy; the original mesh
        // renderer is only switched off, so the scene object stays untouched.
        var skin = new GameObject("LumaSkin").transform;
        skin.SetParent(transform, false);
        skin.gameObject.layer = gameObject.layer;
        var body = NewBone("Body", skin, bodyC);
        head = NewBone("Head", skin, neck);
        armL = NewBone("ArmL", skin, shoulderL);
        armR = NewBone("ArmR", skin, shoulderR);
        var bones = new[] { body, head, armL, armR };

        // Each arm owns everything along its line from shoulder to hand - the tube,
        // both rings and the mitten - fading into the body over the first third,
        // so the shoulder bends like skin instead of tearing. The head owns what is
        // above the neck, with a short fade there too; the body keeps the rest.
        var weights = new BoneWeight[u.Length];
        float neckLo = neck.y - 0.03f * H, neckHi = neck.y + 0.06f * H;
        for (int i = 0; i < u.Length; i++)
        {
            var p = u[i];
            float tL, tR;
            float dL = SegmentDistance(p, shoulderL, handL, out tL) / armRadius;
            float dR = SegmentDistance(p, shoulderR, handR, out tR) / armRadius;
            bool left = dL <= dR;
            float dArm = left ? dL : dR, tArm = left ? tL : tR;
            float dHead = (p - headC).magnitude / headR;
            float dBody = Vector3.Scale(p - bodyC, new Vector3(1f / bodyR.x, 1f / bodyR.y, 1f / bodyR.z)).magnitude;

            float arm = 0f;
            // On the arm's line, clear of the head (with a margin: the head isn't a
            // perfect sphere, and its lower edge sits close to the shoulders), and -
            // near the shoulder - outside the body.
            // The glasses ring sticks out past the head beside the hands: anything that
            // high up and that close to the middle is head, not arm.
            bool glasses = Mathf.Abs(p.x - c.x) < 0.325f * W && p.y > c.y + 0.07f * H;
            if (dArm < 1.9f && dHead > 1.12f && !glasses && (tArm > 0.3f || dBody > 0.9f))
                arm = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.02f, 0.32f, tArm))
                    * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1.5f, 1.9f, dArm)));
            float headW = (1f - arm) * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(neckLo, neckHi, p.y));
            float bodyW = 1f - arm - headW;

            weights[i] = new BoneWeight
            {
                boneIndex0 = 0, weight0 = bodyW,
                boneIndex1 = 1, weight1 = headW,
                boneIndex2 = left ? 2 : 3, weight2 = arm,
                boneIndex3 = left ? 3 : 2, weight3 = 0f,
            };
        }

        var skinned = Instantiate(mesh);
        skinned.name = mesh.name + " (rigged)";
        skinned.boneWeights = weights;
        var bindposes = new Matrix4x4[bones.Length];
        for (int k = 0; k < bones.Length; k++) bindposes[k] = bones[k].worldToLocalMatrix * skin.localToWorldMatrix;
        skinned.bindposes = bindposes;

        var smr = skin.gameObject.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = skinned;
        smr.bones = bones;
        smr.rootBone = body;
        smr.sharedMaterials = meshRenderer != null ? meshRenderer.sharedMaterials : new Material[0];
        smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        smr.updateWhenOffscreen = true;
        if (meshRenderer != null) meshRenderer.enabled = false;
    }

    // Upright-frame point -> bone at that spot in the skin's (mesh-local) space.
    Transform NewBone(string name, Transform parent, Vector3 upright)
    {
        var bone = new GameObject(name).transform;
        bone.SetParent(parent, false);
        bone.localPosition = right * upright.x + up * upright.y + face * upright.z;
        return bone;
    }

    static Vector3 Centroid(Vector3[] points, System.Func<Vector3, bool> include)
    {
        Vector3 sum = Vector3.zero;
        int n = 0;
        foreach (var p in points) if (include(p)) { sum += p; n++; }
        return n > 0 ? sum / n : Vector3.zero;
    }

    // Distance from p to the segment a-b, and how far along it (0 at a, 1 at b) the nearest point lies.
    static float SegmentDistance(Vector3 p, Vector3 a, Vector3 b, out float t)
    {
        Vector3 ab = b - a;
        t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
        return (p - (a + t * ab)).magnitude;
    }

    void LateUpdate()
    {
        if (head == null) return;
        float t = Time.unscaledTime, since = t - moodStart;

        // Target pose per mood. "Down" angles lower each arm from the modelled
        // arms-up pose; tilt and nod turn the head.
        float tL, tR, tTilt, tNod, tHop = 0f;
        switch (mood)
        {
            case Mood.Talk:
                tL = 84f + 12f * Mathf.Sin(t * 4.5f);
                tR = 84f + 12f * Mathf.Sin(t * 4.5f + 2f);
                tTilt = 5f * Mathf.Sin(t * 1.3f);
                tNod = 5f * Mathf.Sin(t * 6f);
                break;
            case Mood.Wave:
                tL = 105f;
                tR = -8f + 24f * Mathf.Sin(t * 9f);
                tTilt = 8f;
                tNod = 0f;
                break;
            case Mood.Cheer:
                tL = -8f;
                tR = -8f;
                tTilt = 4f * Mathf.Sin(t * 3f);
                tNod = -4f;
                tHop = since < 1.5f ? Mathf.Abs(Mathf.Sin(t * 7f)) : 0.3f * Mathf.Abs(Mathf.Sin(t * 3f));
                break;
            case Mood.Concern:
                tL = 112f + 3f * Mathf.Sin(t * 1.5f);
                tR = 112f + 3f * Mathf.Sin(t * 1.5f + 1f);
                tTilt = 12f + 3f * Mathf.Sin(t * 1.2f);
                tNod = 8f;
                break;
            default: // Idle
                tL = 98f + 4f * Mathf.Sin(t * 1.5f);
                tR = 98f + 4f * Mathf.Sin(t * 1.5f + 0.7f);
                tTilt = 4f * Mathf.Sin(t * 0.8f);
                tNod = 0f;
                break;
        }

        float ease = 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime);
        downL = Mathf.Lerp(downL, tL, ease);
        downR = Mathf.Lerp(downR, tR, ease);
        tilt = Mathf.Lerp(tilt, tTilt, ease);
        nod = Mathf.Lerp(nod, tNod, ease);
        hop = Mathf.Lerp(hop, tHop, ease);

        // Lowering the left arm turns it one way around the facing axis, the right arm the other.
        armL.localRotation = Quaternion.AngleAxis(downL, face);
        armR.localRotation = Quaternion.AngleAxis(-downR, face);
        head.localRotation = Quaternion.AngleAxis(tilt, face) * Quaternion.AngleAxis(nod, right);

        if (floatRoot != null)
            floatRoot.localPosition = floatRest + Vector3.up * (floatHeight * Mathf.Sin(t * 2f) + 4f * floatHeight * hop);
    }
}
