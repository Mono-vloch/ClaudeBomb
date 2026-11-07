using System;
using UnityEngine;

// The camera on the market. Three ways of looking: Overview, the whole square
// from above and a little to the side, fitted into a given part of the screen
// (the frame) so the cards can sit next to it; Follow, from just behind one
// robot, the way the robot sees it; and Orbit, a slow turn around the square
// for the cover. Whatever the mode, the subject is kept at the frame's centre
// by shifting the projection, not by turning the camera, so the perspective
// stays straight. Changes glide.
public class MarketCamera : MonoBehaviour
{
    public enum Mode { Off, Overview, Follow, Orbit, Portrait, Rig } // Rig: CameraRig drives the camera

    public Mode mode = Mode.Off;
    public RectTransform frame;   // the part of the screen (an overlay-canvas rect) the subject sits in
    public Vector3 subject;       // Overview and Orbit: the point to centre
    public Bounds bounds;         // Overview: what must fit in the frame
    public Transform follow;      // Follow: the robot
    public float pitch = 55f, yaw = 20f, fov = 30f;
    public float orbitPitch = 26f, orbitDistance = 15f, orbitSpeed = 3.5f;
    public event Action Framed;   // after every frame's placing: labels follow
    public Func<Vector3, bool> Blocked; // the real square: is a camera point inside a stall?

    Camera cam;
    Vector3 pos, velocity;
    Quaternion rot = Quaternion.identity;
    float currentFov = 30f, distance = 18f, orbitYaw = 200f;
    bool snap = true;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = false;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 400f;
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    public void Sleep()
    {
        mode = Mode.Off;
        follow = null;
        if (cam != null) cam.ResetProjectionMatrix();
    }

    public void Overview(RectTransform frame, Bounds what)
    {
        mode = Mode.Overview;
        this.frame = frame;
        bounds = what;
        subject = what.center;
    }

    public void Follow(Transform robot, RectTransform frame)
    {
        mode = Mode.Follow;
        follow = robot;
        this.frame = frame;
    }

    public void Orbit(RectTransform frame, Vector3 centre)
    {
        mode = Mode.Orbit;
        this.frame = frame;
        subject = centre;
    }

    // One robot up close, from the front and a little to its side.
    public void Portrait(Transform robot, RectTransform frame)
    {
        mode = Mode.Portrait;
        follow = robot;
        this.frame = frame;
    }

    // The next frame jumps into place instead of gliding (a new screen).
    public void Snap() => snap = true;

    // Taking the camera back from the rig: glide on from where it is now.
    public void Resume()
    {
        pos = transform.position; rot = transform.rotation; currentFov = cam.fieldOfView;
        velocity = Vector3.zero; snap = false;
        if (mode == Mode.Rig) mode = follow != null ? Mode.Follow : Mode.Overview;
    }

    // A small shake of the overview when an agent is sent home: an offset that dies down.
    float kick;
    public void Kick(float strength) => kick = Mathf.Max(kick, strength);

    void LateUpdate()
    {
        if (mode == Mode.Off || mode == Mode.Rig || frame == null) return;
        if ((mode == Mode.Follow || mode == Mode.Portrait) && follow == null) mode = Mode.Overview;
        float dt = Time.unscaledDeltaTime;

        Vector3 targetPos; Quaternion targetRot; float targetFov; Vector3 anchor;
        switch (mode)
        {
            case Mode.Portrait when Market.Real:
            {
                // The real agent in the garage: from the front and a little to its side, at its height.
                var turn = Quaternion.Euler(0, Mathf.Sin(Time.unscaledTime * 0.3f) * 6f, 0);
                var offset = turn * (follow.forward * 2.2f - follow.right * 0.8f + Vector3.up * 0.98f);
                targetPos = follow.position + offset;
                anchor = follow.position + Vector3.up * 0.64f;
                targetRot = Quaternion.LookRotation(anchor - targetPos);
                targetFov = 36f;
                break;
            }
            case Mode.Follow when Market.Real:
            {
                // The street: behind and to the right of the agent at its shoulder, close enough
                // that it is the hero of the frame, the market ahead of it; pulled in where a
                // stall stands behind it.
                var fwd = Vector3.ProjectOnPlane(follow.forward, Vector3.up).normalized;
                var right = Vector3.Cross(Vector3.up, fwd);
                float back = 1.12f;
                Vector3 At(float b) => follow.position - fwd * b + right * (0.74f * b / 1.12f) + Vector3.up * (1.05f + (1.12f - b) * 0.45f);
                targetPos = At(back);
                while (Blocked != null && back > 0.8f && Blocked(targetPos)) { back -= 0.1f; targetPos = At(back); }
                // Composed by aiming, not by shifting the picture: the agent's chest a little left of
                // the middle of the screen (inside the part the drawer leaves), the market beyond it.
                targetRot = Quaternion.LookRotation(follow.position + Vector3.up * 0.62f + fwd * 0.55f + right * 0.32f - targetPos);
                targetFov = 52f;
                anchor = Vector3.zero; // no shift
                break;
            }
            case Mode.Portrait:
            {
                var turn = Quaternion.Euler(0, Mathf.Sin(Time.unscaledTime * 0.3f) * 5f, 0); // a slow look around it
                var offset = turn * (follow.forward * 4.2f - follow.right * 1.5f + Vector3.up * 1.5f);
                targetPos = follow.position + offset;
                anchor = follow.position + Vector3.up * 0.78f;
                targetRot = Quaternion.LookRotation(anchor - targetPos);
                targetFov = 30f;
                break;
            }
            case Mode.Follow:
            {
                var fwd = follow.forward;
                targetPos = follow.position - fwd * 2.7f + Vector3.up * 2.0f;
                targetRot = Quaternion.LookRotation(follow.position + fwd * 2.2f + Vector3.up * 0.2f - targetPos);
                targetFov = 52f;
                anchor = follow.position + fwd * 0.8f + Vector3.up * 0.5f;
                break;
            }
            case Mode.Orbit:
            {
                orbitYaw += orbitSpeed * dt;
                var dir = Quaternion.Euler(Market.Real ? 21f : orbitPitch, orbitYaw, 0) * Vector3.back;
                targetPos = subject + dir * (Market.Real ? 15.5f : orbitDistance);
                targetRot = Quaternion.LookRotation(subject - targetPos);
                targetFov = 36f;
                anchor = subject;
                break;
            }
            default:
            {
                // A slow sway, so the square is not a still picture.
                float sway = Mathf.Sin(Time.unscaledTime * 0.21f) * 1.6f, sway2 = Mathf.Sin(Time.unscaledTime * 0.13f) * 0.7f;
                var dir = Quaternion.Euler(pitch + sway2, yaw + sway, 0) * Vector3.back;
                targetFov = fov;
                distance = Fit(dir, targetFov, distance);
                targetPos = subject + dir * distance;
                targetRot = Quaternion.LookRotation(subject - targetPos);
                anchor = subject;
                break;
            }
        }

        if (snap) { pos = targetPos; rot = targetRot; currentFov = targetFov; velocity = Vector3.zero; snap = false; }
        else
        {
            pos = Vector3.SmoothDamp(pos, targetPos, ref velocity, 0.4f, float.PositiveInfinity, dt);
            rot = Quaternion.Slerp(rot, targetRot, 1f - Mathf.Exp(-5f * dt));
            currentFov = Mathf.Lerp(currentFov, targetFov, 1f - Mathf.Exp(-4f * dt));
        }
        if (kick > 0.001f)
        {
            float k = kick;
            kick = Mathf.MoveTowards(kick, 0f, dt * 0.8f);
            pos += new Vector3(Mathf.Sin(Time.unscaledTime * 61f), Mathf.Cos(Time.unscaledTime * 47f), 0f) * (k * k * 0.4f);
        }
        transform.SetPositionAndRotation(pos, rot);
        cam.fieldOfView = currentFov;
        cam.ResetProjectionMatrix();

        // Shift the picture so the subject sits at the frame's centre. In the
        // overview the subject is the middle of the square's picture, not of the
        // square: the far side is drawn smaller.
        if (mode == Mode.Follow && Market.Real) { Framed?.Invoke(); return; } // the street view is composed by aiming
        Vector2 at = mode == Mode.Overview ? ProjectedCentre() : (Vector2)cam.WorldToViewportPoint(anchor);
        var centre = FrameRect().center;
        float dx = (centre.x - at.x) * 2f, dy = (centre.y - at.y) * 2f;
        var m = cam.projectionMatrix;
        m[0, 2] = -dx;
        m[1, 2] = -dy;
        cam.projectionMatrix = m;
        Framed?.Invoke();
    }

    // The frame in viewport units (0..1), with a margin.
    Rect FrameRect()
    {
        var corners = new Vector3[4];
        frame.GetWorldCorners(corners); // screen pixels: the canvas is a screen-space overlay
        float x0 = corners[0].x / Screen.width, y0 = corners[0].y / Screen.height, x1 = corners[2].x / Screen.width, y1 = corners[2].y / Screen.height;
        float mx = (x1 - x0) * 0.02f, my = (y1 - y0) * 0.02f;
        return Rect.MinMaxRect(x0 + mx, y0 + my, x1 - mx, y1 - my);
    }

    Vector3[] Corners()
    {
        var b = bounds;
        var c = new Vector3[8];
        int i = 0;
        for (int x = 0; x < 2; x++) for (int y = 0; y < 2; y++) for (int z = 0; z < 2; z++)
            c[i++] = new Vector3(x == 0 ? b.min.x : b.max.x, y == 0 ? b.min.y : b.max.y, z == 0 ? b.min.z : b.max.z);
        return c;
    }

    // The distance at which the bounds fill the frame, from the given direction.
    float Fit(Vector3 dir, float fieldOfView, float d)
    {
        var fr = FrameRect();
        var corners = Corners();
        for (int pass = 0; pass < 4; pass++)
        {
            var p = subject + dir * d;
            var view = Matrix4x4.Scale(new Vector3(1, 1, -1)) * Matrix4x4.TRS(p, Quaternion.LookRotation(subject - p), Vector3.one).inverse;
            var proj = Matrix4x4.Perspective(fieldOfView, cam.aspect, 0.3f, 400f) * view;
            float minX = 1e9f, maxX = -1e9f, minY = 1e9f, maxY = -1e9f;
            foreach (var c in corners)
            {
                var clip = proj * new Vector4(c.x, c.y, c.z, 1);
                float x = (clip.x / clip.w + 1) / 2, y = (clip.y / clip.w + 1) / 2;
                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
            }
            float scale = Mathf.Max((maxX - minX) / fr.width, (maxY - minY) / fr.height);
            d = Mathf.Clamp(d * Mathf.Clamp(scale, 0.5f, 2f), 4f, 80f);
        }
        return d;
    }

    // Where the middle of the square's picture lands now, in viewport units.
    Vector2 ProjectedCentre()
    {
        float minX = 1e9f, maxX = -1e9f, minY = 1e9f, maxY = -1e9f;
        foreach (var c in Corners())
        {
            var v = cam.WorldToViewportPoint(c);
            minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x); minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y);
        }
        return new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
    }

    // A world point on the screen, in pixels; z below zero means behind the camera.
    public Vector3 ToScreen(Vector3 world) => cam.WorldToScreenPoint(world);
}
