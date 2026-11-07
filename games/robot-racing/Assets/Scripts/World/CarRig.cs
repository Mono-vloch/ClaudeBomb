using UnityEngine;

// The robot car's moving parts (the car is made by Robot Racing > Build track kit): its four
// wheels spin with the distance it covers, the front pair steer with the rate it turns, and
// the body leans out of a bend and squats as it speeds up. Circuit only moves the car; this
// reads the move after it, every frame, so it works at any training speed. A jump of more
// than a cell (a new attempt back on the grid) moves nothing.
public class CarRig : MonoBehaviour
{
    public Transform body;            // leans and pitches
    public Transform[] steer;         // the front wheels' steering pivots
    public Transform[] spin;          // every wheel's spinning part, turning about its X
    public float wheelRadius = 0.08f; // in the world's units
    public float maxSteer = 26f, leanPerDegree = 0.22f, maxLean = 5f;
    public float pitch;               // set by Circuit: the squat at speed, in degrees

    Vector3 lastPos;
    float lastYaw, steerAngle, lean, spinAngle;
    bool started;

    public float Steer => steerAngle;

    void LateUpdate()
    {
        float dt = Mathf.Max(Time.unscaledDeltaTime, 1e-4f);
        var pos = transform.position;
        float yaw = transform.eulerAngles.y;
        if (!started) { lastPos = pos; lastYaw = yaw; started = true; }
        var delta = pos - lastPos;
        bool jumped = delta.magnitude > 1.1f;
        float forward = jumped ? 0f : Vector3.Dot(delta, transform.forward);
        float yawRate = jumped ? 0f : Mathf.DeltaAngle(lastYaw, yaw) / dt;
        float speed = jumped ? 0f : delta.magnitude / dt;

        // Steer toward the turn being made, a little ahead of the body; settle back when straight.
        float want = Mathf.Clamp(yawRate * 0.22f, -maxSteer, maxSteer);
        steerAngle = Mathf.Lerp(steerAngle, want, 1f - Mathf.Exp(-14f * dt));
        spinAngle = Mathf.Repeat(spinAngle + forward / Mathf.Max(0.01f, wheelRadius) * Mathf.Rad2Deg, 360f);
        // Out of the bend: a right turn rolls the body to the left, more at speed.
        float wantLean = Mathf.Clamp(steerAngle * leanPerDegree * Mathf.Clamp01(speed / 1.5f), -maxLean, maxLean);
        lean = Mathf.Lerp(lean, wantLean, 1f - Mathf.Exp(-9f * dt));

        if (body != null) body.localRotation = Quaternion.Euler(pitch, 0f, lean);
        if (steer != null) foreach (var s in steer) if (s != null) s.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
        if (spin != null) foreach (var w in spin) if (w != null) w.localRotation = Quaternion.Euler(spinAngle, 0f, 0f);
        lastPos = pos; lastYaw = yaw;
    }

    // A new car on the grid: no turn carried over.
    public void Settle() { started = false; steerAngle = lean = 0f; }
}
