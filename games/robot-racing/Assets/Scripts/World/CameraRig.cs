using System.Collections;
using Cinemachine;
using UnityEngine;

// The camera moves of a race, on Cinemachine 2.10.5 (the version Claude Bomb uses). The
// scene's own camera (SceneCamera) keeps the overview, the portrait and the title's orbit,
// because it fits the circuit into a part of the screen by shifting the projection, which
// no rig does. This rig takes over for the moments: the chase behind the agent's car with
// damping and a field-of-view kick at speed, the countdown's fly-in from high and wide,
// the crash shake through an impulse, and the slow-motion orbit of the finish. When it
// lets go, SceneCamera glides on from wherever the rig left the camera.
[RequireComponent(typeof(Camera), typeof(SceneCamera))]
public class CameraRig : MonoBehaviour
{
    public float chaseFov = 46f, speedFovKick = 10f;

    SceneCamera scene;
    CinemachineBrain brain;
    CinemachineVirtualCamera chase, start, finish;
    CinemachineTransposer chaseBody, startBody, finishBody;
    CinemachineComposer chaseAim, startAim, finishAim;
    CinemachineImpulseSource impulse;
    RectTransform frame;
    Transform target;
    float speed01, orbitAngle;
    Coroutine moment;

    public bool Live => chase.enabled || start.enabled || finish.enabled;

    void Awake()
    {
        scene = GetComponent<SceneCamera>();
        brain = gameObject.AddComponent<CinemachineBrain>();
        brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 1.1f);
        brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
        brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
        brain.m_IgnoreTimeScale = true; // the camera keeps gliding through the slow-motion finish
        brain.m_ShowDebugText = false;

        if (Circuit.HasKit) { chaseFov = 42f; speedFovKick = 5f; }
        chase = Make("Chase", 10, chaseFov, out chaseBody, out chaseAim);
        chaseBody.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
        // Low and close behind the car (the realism pass): it fills a third of the picture.
        bool kit = Circuit.HasKit;
        chaseBody.m_FollowOffset = kit ? new Vector3(0.2f, 0.46f, -1.0f) : new Vector3(0f, 2.0f, -3.4f); // the whole car in the lower third, the track ahead in view (the game-feel pass)
        chaseBody.m_XDamping = 0.5f; chaseBody.m_YDamping = 0.6f; chaseBody.m_ZDamping = 0.8f; chaseBody.m_YawDamping = 0.9f;
        if (kit) { chaseBody.m_XDamping = 0.12f; chaseBody.m_YDamping = 0.2f; chaseBody.m_ZDamping = 0.12f; chaseBody.m_YawDamping = 0.35f; } // close behind: it keeps up
        chaseAim.m_TrackedObjectOffset = kit ? new Vector3(0f, 0.08f, 0.7f) : new Vector3(0f, 0.4f, 0.9f);
        chaseAim.m_LookaheadTime = 0.15f; chaseAim.m_LookaheadSmoothing = 8f;
        var listener = chase.gameObject.AddComponent<CinemachineImpulseListener>();
        listener.m_Gain = 1f; listener.m_UseCameraSpace = true;

        start = Make("Start", 0, 34f, out startBody, out startAim);
        startBody.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
        startBody.m_FollowOffset = kit ? new Vector3(-1.4f, 2.4f, -3.6f) : new Vector3(-2.2f, 5.5f, -7.5f); // high and wide behind the grid, looking down the first straight
        startBody.m_XDamping = startBody.m_YDamping = startBody.m_ZDamping = 0f;
        startAim.m_TrackedObjectOffset = kit ? new Vector3(0f, 0.15f, 0.6f) : new Vector3(0f, 0.3f, 0f);

        finish = Make("Finish", 0, 42f, out finishBody, out finishAim);
        finishBody.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        finishBody.m_FollowOffset = kit ? new Vector3(1.2f, 0.6f, -1.4f) : new Vector3(2.4f, 2.0f, -3.2f);
        finishBody.m_XDamping = finishBody.m_YDamping = finishBody.m_ZDamping = 0.3f;
        finishAim.m_TrackedObjectOffset = kit ? new Vector3(0f, 0.15f, 0f) : new Vector3(0f, 0.3f, 0f);
        finish.gameObject.AddComponent<CinemachineImpulseListener>().m_Gain = 0.6f;

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
        vc.m_Lens.NearClipPlane = 0.1f;
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
        this.frame = frame;
        if (frame == null) return;
        var corners = new Vector3[4];
        frame.GetWorldCorners(corners);
        float cx = (corners[0].x + corners[2].x) / 2f / Screen.width, cy = (corners[0].y + corners[2].y) / 2f / Screen.height;
        // Cinemachine 2's composer counts ScreenY from the bottom; the car sits a little below the frame's centre.
        foreach (var a in new[] { chaseAim, startAim, finishAim }) { a.m_ScreenX = cx; a.m_ScreenY = cy - 0.08f; }
    }

    void Take()
    {
        scene.mode = SceneCamera.Mode.Rig;
        GetComponent<Camera>().ResetProjectionMatrix();
    }

    // The chase: just behind the car, settling, never teleporting.
    public void Chase(Transform car, RectTransform frame)
    {
        if (moment != null) { StopCoroutine(moment); moment = null; Time.timeScale = 1f; }
        target = car;
        Aim(frame);
        chase.Follow = chase.LookAt = car;
        Take();
        chase.enabled = true;
        start.enabled = finish.enabled = false;
    }

    // The countdown: from high and wide, the camera flies down behind the car.
    public void Countdown(Transform car, RectTransform frame, float seconds)
    {
        if (moment != null) StopCoroutine(moment);
        target = car;
        Aim(frame);
        foreach (var vc in new[] { chase, start }) { vc.Follow = vc.LookAt = car; }
        Take();
        start.Priority = 20;
        start.enabled = chase.enabled = true;
        finish.enabled = false;
        moment = StartCoroutine(DoCountdown(seconds));
    }

    IEnumerator DoCountdown(float seconds)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.2f, seconds - 1.1f));
        start.Priority = 0; // the brain blends into the chase over its default 1.1 s
        yield return new WaitForSecondsRealtime(1.2f);
        start.enabled = false;
        moment = null;
    }

    // The finish: a slow orbit around the car in slow motion, then back to whoever asks.
    public void FinishMoment(Transform car, float seconds, System.Action done)
    {
        if (moment != null) StopCoroutine(moment);
        target = car;
        finish.Follow = finish.LookAt = car;
        Take();
        orbitAngle = 35f;
        finish.Priority = 30;
        finish.enabled = true;
        moment = StartCoroutine(DoFinish(seconds, done));
    }

    IEnumerator DoFinish(float seconds, System.Action done)
    {
        Time.timeScale = 0.25f;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            orbitAngle += Time.unscaledDeltaTime * 28f;
            if (Circuit.HasKit && target != null)
                // The kit's road is one cell wide between barriers: a slow swing behind the car,
                // over the way it came, instead of a full turn through the barriers and the flags.
                finishBody.m_FollowOffset = Quaternion.Euler(0f, Mathf.Sin(t * 1.4f) * 22f, 0f) * (-target.forward * 1.05f) + Vector3.up * 0.42f;
            else finishBody.m_FollowOffset = Quaternion.Euler(0f, orbitAngle, 0f) * new Vector3(0f, 2.0f, 4.4f);
            yield return null;
        }
        Time.timeScale = 1f;
        finish.Priority = 0;
        moment = null;
        done?.Invoke();
    }

    // The crash: the camera takes a hit.
    public void Shake(float strength)
    {
        if (Live) impulse.GenerateImpulseWithVelocity(new Vector3(0.12f, 0.22f, 0.05f) * strength);
        else scene.Kick(0.18f * strength);
    }

    // The car jumped (a new attempt, back on the grid): the chase jumps with it instead of flying there.
    public void Warped(Transform car, Vector3 delta)
    {
        if (chase.enabled && chase.Follow == car) chase.OnTargetObjectWarped(car, delta);
    }

    // How fast the car goes, 0 to 1: the field of view opens with it.
    public void Speed(float k) => speed01 = Mathf.Clamp01(k);

    // Back to the scene's own camera, gliding from here.
    public void Release()
    {
        if (moment != null) { StopCoroutine(moment); moment = null; Time.timeScale = 1f; }
        if (!Live) return;
        chase.enabled = start.enabled = finish.enabled = false;
        scene.Resume();
    }

    void Update()
    {
        if (!chase.enabled) return;
        float want = chaseFov + speedFovKick * speed01;
        chase.m_Lens.FieldOfView = Mathf.Lerp(chase.m_Lens.FieldOfView, want, 1f - Mathf.Exp(-5f * Time.unscaledDeltaTime));
    }
}
