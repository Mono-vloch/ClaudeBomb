using UnityEngine;

// The attacker's body: three rigged copies of the same hooded figure (made with
// Higgsfield, see Docs/credits.md), one per clip, because the three rigs do not share
// bone orientations: idle, a sneaking walk in place (Building moves it), and a step
// forward and push for a door that gives way. Only one is shown at a time; the one
// coming in starts its clip from the beginning.
public class AttackerRig : MonoBehaviour
{
    public enum Pose { Idle, Walk, Force }

    Animator idle, walk, force;
    Pose current = Pose.Idle;
    float forceUntil;

    void Awake()
    {
        idle = Find("Idle");
        walk = Find("Walk");
        force = Find("Force");
        Show(Pose.Idle);
    }

    Animator Find(string child)
    {
        var t = transform.Find(child);
        return t != null ? t.GetComponent<Animator>() : null;
    }

    public Pose Current => current;

    public void Show(Pose pose)
    {
        if (pose == current && Get(pose) != null && Get(pose).gameObject.activeSelf) return;
        current = pose;
        foreach (var (p, a) in new[] { (Pose.Idle, idle), (Pose.Walk, walk), (Pose.Force, force) })
        {
            if (a == null) continue;
            bool on = p == pose;
            a.gameObject.SetActive(on);
            if (on) a.Play(0, 0, 0f);
        }
    }

    // A door forced: the push plays for a moment, then the walk or the idle comes back.
    public void Force(float seconds = 1.6f)
    {
        Show(Pose.Force);
        forceUntil = Time.time + seconds;
    }

    public bool Forcing => current == Pose.Force && Time.time < forceUntil;

    void Update()
    {
        if (current == Pose.Force && Time.time >= forceUntil) Show(Pose.Idle);
    }

    Animator Get(Pose p) => p == Pose.Idle ? idle : p == Pose.Walk ? walk : force;
}
