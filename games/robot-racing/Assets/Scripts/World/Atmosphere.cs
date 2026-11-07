using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// The camera's post-processing, Claude Bomb's (Docs/claude-bomb-recipe.md): neutral
// tonemapping, bloom 0.25 from a threshold of 1 on the glowing parts only (the floodlights,
// the start lights, the eyes and the antenna, a moment), a black vignette at 0.2, no grading
// on top; SMAA for the thin edges (cheaper than MSAA on the web), and a depth of field that
// comes in behind menus and briefings.
// Built in code on the scene's camera, like everything else in the game; no motion blur.
[RequireComponent(typeof(Camera))]
public class Atmosphere : MonoBehaviour
{
    public static Atmosphere Current { get; private set; }

    public float bloomIntensity = 0.25f, bloomThreshold = 1.0f, vignetteStrength = 0.2f, saturation = 0f, contrast = 0f; // Claude Bomb's values
    public float blurRadius = 0.9f, blurStart = 9f, blurEnd = 30f;

    Volume volume;
    VolumeProfile profile;
    Bloom bloom;
    Vignette vignette;
    ColorAdjustments grading;
    DepthOfField depth;
    Coroutine blurring, pulsing;
    float blur; // 0 sharp, 1 behind a menu

    void Awake()
    {
        Current = this;
        var cam = GetComponent<Camera>();
        cam.allowHDR = true;
        var data = cam.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = true;
        data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        data.antialiasingQuality = AntialiasingQuality.Medium;
        data.renderShadows = true;
        data.dithering = true;
        data.volumeLayerMask = 1 << gameObject.layer;

        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Atmosphere";
        var tone = profile.Add<Tonemapping>(true);
        tone.mode.value = TonemappingMode.Neutral;
        bloom = profile.Add<Bloom>(true);
        bloom.intensity.value = bloomIntensity; bloom.threshold.value = bloomThreshold; bloom.scatter.value = 0.5f; bloom.highQualityFiltering.value = true;
        vignette = profile.Add<Vignette>(true);
        vignette.intensity.value = vignetteStrength; vignette.smoothness.value = 0.2f; vignette.color.value = Color.black;
        grading = profile.Add<ColorAdjustments>(true);
        grading.saturation.value = saturation; grading.contrast.value = contrast; grading.postExposure.value = 0f;
        depth = profile.Add<DepthOfField>(true);
        depth.mode.value = DepthOfFieldMode.Gaussian;
        depth.gaussianStart.value = blurStart; depth.gaussianEnd.value = blurEnd; depth.gaussianMaxRadius.value = 0f; depth.highQualitySampling.value = false;
        depth.active = false;

        var go = new GameObject("Atmosphere");
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);
        volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 0f;
        volume.sharedProfile = profile;
    }

    // The world goes soft behind a menu, a briefing, a debrief; sharp again for play.
    public void MenuBlur(bool on)
    {
        if (blurring != null) StopCoroutine(blurring);
        blurring = StartCoroutine(Blur(on ? 1f : 0f));
    }

    IEnumerator Blur(float to)
    {
        depth.active = true;
        float from = blur, t = 0f, d = 0.35f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            blur = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / d));
            depth.gaussianMaxRadius.value = blurRadius * blur;
            yield return null;
        }
        blur = to;
        depth.gaussianMaxRadius.value = blurRadius * to;
        depth.active = to > 0f;
    }

    // A short pulse of the vignette in a colour: red for a crash, teal for a finish.
    public void Pulse(Color colour, float strength = 0.45f, float seconds = 0.5f)
    {
        if (pulsing != null) StopCoroutine(pulsing);
        pulsing = StartCoroutine(DoPulse(colour, strength, seconds));
    }

    IEnumerator DoPulse(Color colour, float strength, float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - t / seconds;
            vignette.color.value = Color.Lerp(Color.black, colour, k);
            vignette.intensity.value = vignetteStrength + strength * k;
            yield return null;
        }
        vignette.color.value = Color.black;
        vignette.intensity.value = vignetteStrength;
    }

    // A short hold of the world on an impact, in unscaled time, never on top of a slow-motion moment.
    Coroutine stopping;
    public void HitStop(float seconds)
    {
        if (Time.timeScale < 0.9f) return;
        if (stopping != null) StopCoroutine(stopping);
        stopping = StartCoroutine(DoHitStop(seconds));
    }

    IEnumerator DoHitStop(float seconds)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(seconds);
        if (Time.timeScale < 0.9f && Time.timeScale > 0.03f) Time.timeScale = 1f;
        stopping = null;
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
        if (profile != null) Destroy(profile);
    }
}
