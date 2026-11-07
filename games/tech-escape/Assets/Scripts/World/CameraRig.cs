using System.Collections;
using Cinemachine;
using UnityEngine;

// The camera moves of the attack, on Cinemachine 2.10.5 (the version Claude Bomb uses). The
// scene's own camera (SceneCamera) keeps the overview, the room and the cover's orbit,
// because it fits the facility into a part of the screen by shifting the projection, which
// no rig does. This rig takes over for the moments: the chase behind the attacker down the
// corridor with damping, the opening's fly-in from high over the entrance, the shake of a
// door giving way through an impulse, and the slow turn around a door (or the exit) at the
// end. When it lets go, SceneCamera glides on from wherever the rig left the camera.
[RequireComponent(typeof(Camera), typeof(SceneCamera))]
public class CameraRig : MonoBehaviour
{
    public float chaseFov = 50f;

    SceneCamera scene;
    CinemachineBrain brain;
    CinemachineVirtualCamera chase, start, finish;
    CinemachineTransposer chaseBody, startBody, finishBody;
    CinemachineComposer chaseAim, startAim, finishAim;
    CinemachineImpulseSource impulse;
    Transform target, aimPoint, followPoint; // the attacker, the point ahead of it the chase looks at, and the point it follows
    float orbitAngle, finishYaw;
    Coroutine moment;

    public bool Live => chase.enabled || start.enabled || finish.enabled;

    void Awake()
    {
        scene = GetComponent<SceneCamera>();
        brain = gameObject.AddComponent<CinemachineBrain>();
        brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 1.1f);
        brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
        brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
        brain.m_IgnoreTimeScale = true; // the camera keeps gliding through the slow-motion moment
        brain.m_ShowDebugText = false;

        chase = Make("Chase", 10, chaseFov, out chaseBody, out chaseAim);
        // World space, not the attacker's: the corridor runs along +X, so "behind" is always -X,
        // and the camera stays in the corridor when the attacker turns to face a door instead of
        // swinging behind it into the opposite room.
        chaseBody.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        chaseBody.m_FollowOffset = new Vector3(-3.6f, 2.05f, 0.5f); // behind the attacker down the corridor, a little to one side, under the ceiling
        chaseBody.m_XDamping = 0.6f; chaseBody.m_YDamping = 0.7f; chaseBody.m_ZDamping = 0.9f;
        chaseAim.m_TrackedObjectOffset = Vector3.zero; // the chase looks at aimPoint: ahead down the corridor, leaning to what the attacker faces
        chaseAim.m_LookaheadTime = 0.15f; chaseAim.m_LookaheadSmoothing = 8f;
        var listener = chase.gameObject.AddComponent<CinemachineImpulseListener>();
        listener.m_Gain = 1f; listener.m_UseCameraSpace = true;

        start = Make("Start", 0, 36f, out startBody, out startAim);
        startBody.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        startBody.m_FollowOffset = new Vector3(-7.5f, 1.75f, -1.3f); // outside the entrance at eye height, looking in down the corridor
        startBody.m_XDamping = startBody.m_YDamping = startBody.m_ZDamping = 0f;
        startAim.m_TrackedObjectOffset = Vector3.zero; // the opening looks at aimPoint too

        finish = Make("Finish", 0, 40f, out finishBody, out finishAim);
        finishBody.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        finishBody.m_FollowOffset = new Vector3(2.4f, 2.0f, -3.2f);
        finishBody.m_XDamping = finishBody.m_YDamping = finishBody.m_ZDamping = 0.3f;
        finishAim.m_TrackedObjectOffset = new Vector3(0f, 0.9f, 0f);
        finish.gameObject.AddComponent<CinemachineImpulseListener>().m_Gain = 0.6f;

        aimPoint = new GameObject("Aim").transform;
        aimPoint.SetParent(transform.parent, false);
        followPoint = new GameObject("Follow").transform;
        followPoint.SetParent(transform.parent, false);

        impulse = gameObject.AddComponent<CinemachineImpulseSource>();
        impulse.m_ImpulseDefinition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        impulse.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulse.m_ImpulseDefinition.m_ImpulseDuration = 0.35f;
    }

    CinemachineVirtualCamera Make(string name, int priority, float fov, out CinemachineTransposer body, out CinemachineComposer aim)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform.parent, false);
        var vc = go.AddComponent<CinemachineVirtualCamera>();
        vc.Priority = priority;
        vc.m_Lens.FieldOfView = fov;
        vc.m_Lens.NearClipPlane = 0.2f;
        vc.m_Lens.FarClipPlane = 400f;
        body = vc.AddCinemachineComponent<CinemachineTransposer>();
        aim = vc.AddCinemachineComponent<CinemachineComposer>();
        aim.m_ScreenX = 0.5f; aim.m_ScreenY = 0.5f;
        aim.m_DeadZoneWidth = 0.04f; aim.m_DeadZoneHeight = 0.04f;
        aim.m_SoftZoneWidth = name == "Chase" ? 0.5f : 0.2f; aim.m_SoftZoneHeight = name == "Chase" ? 0.5f : 0.2f;
        aim.m_HorizontalDamping = 0.5f; aim.m_VerticalDamping = 0.5f;
        vc.enabled = false;
        return vc;
    }

    void Aim(RectTransform frame)
    {
        if (frame == null) return;
        var corners = new Vector3[4];
        frame.GetWorldCorners(corners);
        float cx = (corners[0].x + corners[2].x) / 2f / Screen.width, cy = (corners[0].y + corners[2].y) / 2f / Screen.height;
        // Cinemachine 2's composer counts ScreenY from the bottom; the subject sits a little below the frame's centre.
        foreach (var a in new[] { startAim, finishAim }) { a.m_ScreenX = cx; a.m_ScreenY = cy - 0.06f; }
        chaseAim.m_ScreenX = cx; chaseAim.m_ScreenY = cy + 0.18f; // the chase looks well up: the corridor ahead, the attacker mid-frame, the moments under it
    }

    void Take()
    {
        scene.mode = SceneCamera.Mode.Rig;
        GetComponent<Camera>().ResetProjectionMatrix();
    }

    // Where the chase and the opening look: a little ahead of the attacker down the corridor,
    // leaning toward whatever it faces (the door it is at), in world space.
    // The follow point is the attacker kept to the middle band of the corridor, so the camera
    // behind it never leaves the corridor for a room when the attacker steps into a doorway.
    void PlaceAim()
    {
        if (target == null) return;
        var p = target.position;
        aimPoint.position = p + Vector3.up * 1.0f + Vector3.right * 2.2f + target.forward * 1.0f;
        followPoint.position = new Vector3(p.x, 0f, Mathf.Clamp(p.z, -1.4f, 1.4f));
    }

    // The chase: just behind the attacker, settling, never teleporting.
    public void Chase(Transform attacker, RectTransform frame)
    {
        if (moment != null) { StopCoroutine(moment); moment = null; Time.timeScale = 1f; }
        target = attacker;
        PlaceAim();
        Aim(frame);
        chase.Follow = followPoint; chase.LookAt = aimPoint;
        Take();
        chase.enabled = true;
        start.enabled = finish.enabled = false;
    }

    // The opening: from high over the entrance, the camera flies down behind the attacker.
    public void Opening(Transform attacker, RectTransform frame, float seconds)
    {
        if (moment != null) StopCoroutine(moment);
        target = attacker;
        PlaceAim();
        Aim(frame);
        foreach (var vc in new[] { chase, start }) { vc.Follow = followPoint; vc.LookAt = aimPoint; }
        Take();
        start.Priority = 20;
        start.enabled = chase.enabled = true;
        finish.enabled = false;
        moment = StartCoroutine(DoOpening(seconds));
    }

    IEnumerator DoOpening(float seconds)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.2f, seconds - 1.1f));
        start.Priority = 0; // the brain blends into the chase over its default 1.1 s
        yield return new WaitForSecondsRealtime(1.2f);
        start.enabled = false;
        moment = null;
    }

    // The end: a slow turn around a point (the breached door, the exit) in slow motion, then back to whoever asks.
    public void FinishMoment(Transform subject, float seconds, System.Action done, bool slow = true)
    {
        if (moment != null) StopCoroutine(moment);
        finish.Follow = finish.LookAt = subject;
        Take();
        orbitAngle = -38f;
        var fwd = subject.forward; fwd.y = 0f;
        finishYaw = fwd.sqrMagnitude > 0.01f ? Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg : 0f;
        finish.Priority = 30;
        finish.enabled = true;
        moment = StartCoroutine(DoFinish(seconds, done, slow));
    }

    IEnumerator DoFinish(float seconds, System.Action done, bool slow)
    {
        Time.timeScale = slow ? 0.25f : 1f;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            // An arc in front of it, on the corridor side: the walls and ceiling are real now.
            orbitAngle += Time.unscaledDeltaTime * 28f;
            finishBody.m_FollowOffset = Quaternion.Euler(0f, finishYaw + orbitAngle, 0f) * new Vector3(0f, 1.55f, 3.1f);
            yield return null;
        }
        Time.timeScale = 1f;
        finish.Priority = 0;
        moment = null;
        done?.Invoke();
    }

    // A door giving way: the camera takes a hit.
    public void Shake(float strength)
    {
        if (Live) impulse.GenerateImpulseWithVelocity(new Vector3(0.12f, 0.22f, 0.05f) * strength);
        else scene.Kick(0.18f * strength);
    }

    // Back to the scene's own camera, gliding from here.
    public void Release()
    {
        if (moment != null) { StopCoroutine(moment); moment = null; Time.timeScale = 1f; }
        if (!Live) return;
        chase.enabled = start.enabled = finish.enabled = false;
        scene.Resume();
    }

    void LateUpdate() { if (chase.enabled || start.enabled) PlaceAim(); }
}
