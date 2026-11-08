using System.Collections;
using UnityEngine;

// Mom at the family's door (Higgsfield, Meshy: rigged, one clip, a one-hand wave). At rest she
// holds the clip's first frame; when the agent comes home she waves once, then rests again.
public class MomRig : MonoBehaviour
{
    public Animator animator;
    public float waveSeconds = 4.13f;
    Coroutine waving;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        Rest();
    }

    public void Rest()
    {
        if (animator == null) return;
        if (waving != null) { StopCoroutine(waving); waving = null; }
        animator.speed = 0f;
        animator.Play(0, 0, 0f);
        animator.Update(0f);
    }

    public void Wave(float delay = 0f)
    {
        if (animator == null) return;
        if (waving != null) StopCoroutine(waving);
        waving = StartCoroutine(DoWave(delay));
    }

    IEnumerator DoWave(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        animator.speed = 1f;
        animator.Play(0, 0, 0f);
        yield return new WaitForSeconds(waveSeconds);
        waving = null;
        Rest();
    }
}
