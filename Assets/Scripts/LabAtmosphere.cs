using System.Collections;
using UnityEngine;

// Bay 3 at night, around the device: the room itself is Resources/Lab.prefab (made by
// Claude Bomb > Build Lab, see Editor/LabBuilder.cs) and added to every run by GameUi.
// This keeps it alive: the fog, the rotating beacon, a flickering tube, the fire
// alarm's light and the room's hum, and how the room reacts to the run - the beacon
// turns red and speeds up in the last minute, the lights stutter on a strike, the
// alarm stops when the device is defused, and the power cuts when it goes off.
//
// The room has its own lights on rendering layer 2 ("Light Layer 1"); the device keeps
// the scene's directional light on the default layer, so it reads as it always has.
public class LabAtmosphere : MonoBehaviour
{
    [SerializeField] float floorY;
    [SerializeField] Light keyLight;      // the lamp over the table
    [SerializeField] Light glow;          // the inspection table's light, from below
    [SerializeField] Light beacon;        // one of the beacon's two beams; both turn with beaconHead
    [SerializeField] Transform beaconHead;
    [SerializeField] Renderer beaconDome;
    [SerializeField] Light[] tubes;       // the ceiling's fluorescent lights
    [SerializeField] Renderer[] tubeGlass;
    [SerializeField] Renderer alarmLed;
    [SerializeField] ReflectionProbe probe;
    [SerializeField] Color fog = new Color(0.035f, 0.06f, 0.13f);
    [SerializeField] float fogDensity = 0.085f;

    static readonly Color Amber = new Color(1f, 0.42f, 0.08f), Red = new Color(1f, 0.08f, 0.05f);
    static readonly int Emission = Shader.PropertyToID("_EmissionColor");

    float keyBase, glowBase, beaconBase;
    Light[] beams;
    float[] tubeBase;
    Color[] tubeGlow;
    Color beaconGlow;
    float strikeFlash, power = 1f, flicker = 1f, nextFlicker, nextBurst = 2f;
    int burstLeft;
    bool won, lost;
    MaterialPropertyBlock block;
    AudioSource hum;

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fog;
        RenderSettings.fogDensity = fogDensity;
        var cam = FindFirstObjectByType<BombCameraController>();
        if (cam != null)
        {
            cam.MinHeight = floorY + 0.12f; // never under the floor
            var view = cam.GetComponent<Camera>();
            if (view != null) view.backgroundColor = fog;
        }

        block = new MaterialPropertyBlock();
        keyBase = keyLight != null ? keyLight.intensity : 0f;
        glowBase = glow != null ? glow.intensity : 0f;
        beaconBase = beacon != null ? beacon.intensity : 0f;
        beams = beaconHead != null ? beaconHead.GetComponentsInChildren<Light>() : new Light[0];
        tubeBase = new float[tubes.Length];
        for (int i = 0; i < tubes.Length; i++) tubeBase[i] = tubes[i].intensity;
        tubeGlow = new Color[tubeGlass.Length];
        for (int i = 0; i < tubeGlass.Length; i++) tubeGlow[i] = tubeGlass[i].sharedMaterial.GetColor(Emission);
        beaconGlow = beaconDome != null ? beaconDome.sharedMaterial.GetColor(Emission) : Amber;

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnStrike += HandleStrike;
            gm.OnGameEnded += HandleEnded;
        }
        StartHum();
        if (probe != null) StartCoroutine(CaptureReflections());
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnStrike -= HandleStrike;
        GameManager.Instance.OnGameEnded -= HandleEnded;
    }

    // Once the room's lights are on: the device's metal reflects the room, not an empty sky.
    IEnumerator CaptureReflections()
    {
        yield return null;
        probe.RenderProbe();
    }

    void Update()
    {
        float t = Time.unscaledTime, dt = Time.unscaledDeltaTime;
        var gm = GameManager.Instance;
        bool lastMinute = gm != null && gm.State == GameManager.RunState.Playing && gm.ClockStarted && !gm.NoClock && gm.TimeRemaining <= 60f;
        strikeFlash = Mathf.MoveTowards(strikeFlash, 0f, dt);

        // The beacon: amber, red in the last minute and on a strike, off once the device is safe.
        if (beacon != null && beaconHead != null)
        {
            float speed = won ? 0f : lastMinute || lost ? 300f : 110f;
            beaconHead.Rotate(0f, speed * dt, 0f, Space.World);
            Color c = lastMinute || lost || strikeFlash > 0f ? Red : Amber;
            float on = won ? 0f : 1f;
            foreach (var beam in beams)
            {
                beam.color = c;
                beam.intensity = beaconBase * on * (lost ? 1.4f : 1f);
            }
            SetGlow(beaconDome, (lastMinute || lost || strikeFlash > 0f ? Red * 3f : beaconGlow) * on);
        }

        // One tube with a tired starter stutters now and then; on a strike, all of them do.
        if (burstLeft == 0 && t > nextBurst) burstLeft = Random.Range(3, 8);
        if (burstLeft > 0 && t > nextFlicker)
        {
            flicker = flicker < 1f ? 1f : Random.Range(0.1f, 0.5f);
            nextFlicker = t + Random.Range(0.03f, 0.1f);
            if (--burstLeft == 0)
            {
                flicker = 1f;
                nextBurst = t + Random.Range(3f, 9f);
            }
        }
        float stutter = strikeFlash > 0f && Mathf.Repeat(t * 14f, 1f) < 0.5f ? 0.2f : 1f;
        for (int i = 0; i < tubes.Length; i++)
        {
            float k = power * stutter * (i == 0 ? flicker : 1f);
            tubes[i].intensity = tubeBase[i] * k;
            if (i < tubeGlass.Length) SetGlow(tubeGlass[i], tubeGlow[i] * k);
        }
        if (keyLight != null) keyLight.intensity = keyBase * (lost ? power : 1f) * (won ? 1.15f : 1f);
        if (glow != null) glow.intensity = glowBase * (lost ? power : 1f) * (won ? 1f + 0.25f * Mathf.Sin(t * 3f) : 1f);

        // The fire alarm's light: a slow blink, fast in the last minute.
        if (alarmLed != null)
        {
            bool lit = Mathf.Repeat(t * (lastMinute || lost ? 4f : 0.8f), 1f) < 0.35f && !won;
            SetGlow(alarmLed, lit ? Red * 4f : Color.black);
        }

        if (hum != null) hum.volume = 0.16f * (lost ? power : 1f);
    }

    void SetGlow(Renderer r, Color c)
    {
        if (r == null) return;
        r.GetPropertyBlock(block);
        block.SetColor(Emission, c);
        r.SetPropertyBlock(block);
    }

    void HandleStrike(ModuleId module) => strikeFlash = 1.2f;

    void HandleEnded(GameManager.RunState state)
    {
        won = state == GameManager.RunState.Won;
        lost = state == GameManager.RunState.Lost;
        if (lost) StartCoroutine(PowerCut());
    }

    // The blast takes out the power: the lights stutter and die; the beacon runs on battery.
    IEnumerator PowerCut()
    {
        yield return new WaitForSeconds(0.6f);
        for (int i = 0; i < 6; i++)
        {
            power = i % 2 == 0 ? 0.1f : 0.7f;
            yield return new WaitForSeconds(0.07f);
        }
        power = 0.04f;
    }

    // The hum of the room: mains hum and the tubes' buzz, made once and looped.
    void StartHum()
    {
        const int rate = 22050;
        var data = new float[rate * 4];
        var noise = new System.Random(3);
        float low = 0f;
        for (int i = 0; i < data.Length; i++)
        {
            float s = i / (float)rate;
            low += ((float)noise.NextDouble() * 2f - 1f - low) * 0.02f;
            float edge = Mathf.Clamp01(Mathf.Min(i, data.Length - i) / (rate * 0.05f)); // no click at the loop point
            data[i] = (0.5f * Mathf.Sin(s * 100f * Mathf.PI * 2f) + 0.18f * Mathf.Sin(s * 200f * Mathf.PI * 2f) + 0.9f * low) * edge * 0.5f;
        }
        var clip = AudioClip.Create("RoomHum", data.Length, 1, rate, false);
        clip.SetData(data, 0);
        hum = gameObject.AddComponent<AudioSource>();
        hum.clip = clip;
        hum.loop = true;
        hum.spatialBlend = 0f;
        hum.volume = 0.16f;
        hum.Play();
    }
}
