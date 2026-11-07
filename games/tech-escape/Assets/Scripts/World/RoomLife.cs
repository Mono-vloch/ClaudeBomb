using UnityEngine;

// A real room is never quite still (Claude Bomb's lab: a tube that stutters, a blinking
// LED): the rack's LEDs breathe and wink, the monitor's glow wavers as the inbox
// refreshes, the ceiling tube stutters now and then, the lockdown beacon turns.
public class RoomLife : MonoBehaviour
{
    Renderer rack, screen;
    Transform beacon; // the lockdown beacon's head: two amber beams turning
    Light fill, glow;
    float fillBase, glowBase, nextBurst = 3f, nextFlicker;
    int burst;
    MaterialPropertyBlock block;
    Color rackGlow, screenGlow;
    static readonly int Emission = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        block = new MaterialPropertyBlock();
        var r = transform.Find("Rack");
        if (r != null) rack = r.GetComponentInChildren<Renderer>();
        var s = transform.Find("Screen");
        if (s != null) screen = s.GetComponent<Renderer>();
        var lights = transform.Find("Lights");
        if (lights != null)
        {
            var f = lights.Find("Fill"); if (f != null) { fill = f.GetComponent<Light>(); fillBase = fill.intensity; }
            var g = lights.Find("ScreenGlow"); if (g != null) { glow = g.GetComponent<Light>(); glowBase = glow.intensity; }
        }
        beacon = transform.Find("Beacon/BeaconHead");
        if (rack != null) rackGlow = rack.sharedMaterial.GetColor(Emission);
        if (screen != null) screenGlow = screen.sharedMaterial.GetColor(Emission);
    }

    void Update()
    {
        float t = Time.unscaledTime;
        if (beacon != null) beacon.Rotate(0f, 110f * Time.unscaledDeltaTime, 0f, Space.World);
        if (rack != null)
        {
            float wink = 0.55f + 0.45f * Mathf.PerlinNoise(t * 3.1f, 0.7f) * (Mathf.Repeat(t * 1.7f, 1f) < 0.85f ? 1f : 0.2f);
            Set(rack, rackGlow * wink);
        }
        if (screen != null)
        {
            float waver = 0.92f + 0.08f * Mathf.PerlinNoise(t * 0.8f, 3.3f);
            Set(screen, screenGlow * waver);
            if (glow != null) glow.intensity = glowBase * waver;
        }
        if (fill != null)
        {
            if (burst == 0 && t > nextBurst) burst = Random.Range(3, 7);
            if (burst > 0 && t > nextFlicker)
            {
                fill.intensity = fill.intensity < fillBase ? fillBase : fillBase * Random.Range(0.35f, 0.7f);
                nextFlicker = t + Random.Range(0.03f, 0.09f);
                if (--burst == 0) { fill.intensity = fillBase; nextBurst = t + Random.Range(5f, 12f); }
            }
        }
    }

    void Set(Renderer r, Color c)
    {
        r.GetPropertyBlock(block);
        block.SetColor(Emission, c);
        r.SetPropertyBlock(block);
    }
}
