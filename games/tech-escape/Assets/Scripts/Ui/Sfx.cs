using UnityEngine;

// Every sound the game makes, drawn from waves at start so there is no audio file to
// keep: a tick on buttons, a chime for a thing done, a buzz for a thing wrong, the clack
// of a door that held, the crash of one that gave way, a siren when the attack begins,
// a fanfare when the exit opens, and the low hum of a facility in lockdown so the world
// is never silent. One static gain layer (no mixer asset: those are made in the Editor's
// window) with a sound switch remembered on the machine; everything ducks under LUMA's
// voice. Plain waves only: a layer of noise in a loop is the one thing never shown safe
// aloud in the Editor on the Mac this was made on.
public static class Sfx
{
    const int Rate = 22050;
    const string PrefSound = "tech_escape_sound";
    public static bool Enabled { get => PlayerPrefs.GetInt(PrefSound, 1) == 1; set { PlayerPrefs.SetInt(PrefSound, value ? 1 : 0); PlayerPrefs.Save(); Apply(); } }
    public static float Level = 1f;            // the one gain every source multiplies in
    public static Luma luma;                   // whose voice ducks the rest

    static AudioSource src, ambient;
    static AudioClip tick, buy, ruin, whoosh, crash, chime, clack, siren, fanfare, alarm, beep, go;
    static float ambientWant;

    public enum Mood { Beep, Go, Fanfare, Alarm, Siren }

    public static void Init(GameObject host)
    {
        src = host.AddComponent<AudioSource>(); src.playOnAwake = false; src.spatialBlend = 0f;
        ambient = host.AddComponent<AudioSource>(); ambient.playOnAwake = false; ambient.spatialBlend = 0f; ambient.loop = true; ambient.volume = 0f;
        ambient.clip = Hum();
        host.AddComponent<Runner>();
        Apply();
    }

    static void Apply() { AudioListener.volume = Enabled ? 1f : 0f; }

    public static void Tick() => Play(tick != null ? tick : (tick = Tone("tick", new[] { 1400f }, 0.045f, 0.18f)));
    public static void Buy() => Play(buy != null ? buy : (buy = Tone("buy", new[] { 784f, 1175f }, 0.11f, 0.3f)));
    public static void Ruin() => Play(ruin != null ? ruin : (ruin = Tone("ruin", new[] { 150f, 110f }, 0.28f, 0.35f, true)));
    public static void Whoosh() => Play(whoosh != null ? whoosh : (whoosh = Tone("whoosh", new[] { 220f, 330f }, 0.12f, 0.1f)));
    public static void Crash(float strength = 1f) => Play(crash != null ? crash : (crash = CrashClip()), strength);
    public static void Chime() => Play(chime != null ? chime : (chime = Tone("chime", new[] { 880f, 1320f }, 0.09f, 0.28f)));
    public static void Clack() => Play(clack != null ? clack : (clack = Tone("clack", new[] { 523f, 392f }, 0.06f, 0.3f, true)));

    public static void Sting(Mood mood)
    {
        switch (mood)
        {
            case Mood.Beep: Play(beep != null ? beep : (beep = Tone("beep", new[] { 880f }, 0.1f, 0.3f))); break;
            case Mood.Go: Play(go != null ? go : (go = Tone("go", new[] { 1320f, 1320f }, 0.14f, 0.34f))); break;
            case Mood.Fanfare: Play(fanfare != null ? fanfare : (fanfare = Tone("fanfare", new[] { 523f, 659f, 784f, 1046f }, 0.13f, 0.3f))); break;
            case Mood.Alarm: Play(alarm != null ? alarm : (alarm = Tone("alarm", new[] { 660f, 440f, 660f, 440f }, 0.11f, 0.26f, true))); break;
            case Mood.Siren: Play(siren != null ? siren : (siren = SirenClip())); break;
        }
    }

    public static void Ambient(bool on) => ambientWant = on ? 1f : 0f;
    public static bool AmbientPlaying => ambient != null && ambient.isPlaying && ambient.volume > 0.001f;

    static void Play(AudioClip clip, float gain = 1f) { if (src != null && clip != null) src.PlayOneShot(clip, gain * Level); }

    class Runner : MonoBehaviour
    {
        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float duck = luma != null && luma.Speaking ? 0.3f : 1f;
            float av = ambientWant * 0.07f * Level * duck;
            ambient.volume = Mathf.Lerp(ambient.volume, av, 1f - Mathf.Exp(-2f * dt));
            if (ambient.volume > 0.001f && !ambient.isPlaying) ambient.Play();
            if (ambient.volume <= 0.001f && ambient.isPlaying && av <= 0f) ambient.Stop();
        }
    }

    // ---- the waves ----------------------------------------------------------------------

    // Notes one after another, each with a short attack and a long decay.
    static AudioClip Tone(string name, float[] freqs, float each, float gain, bool square = false)
    {
        int per = (int)(each * Rate);
        var data = new float[per * freqs.Length];
        for (int n = 0; n < freqs.Length; n++)
            for (int i = 0; i < per; i++)
            {
                float t = (float)i / Rate;
                float env = Mathf.Min(1f, t / 0.005f) * Mathf.Exp(-t * (square ? 6f : 14f));
                float s = Mathf.Sin(2f * Mathf.PI * freqs[n] * t);
                if (square) s = Mathf.Sign(s) * 0.6f;
                data[n * per + i] = s * env * gain;
            }
        return Clip(name, data);
    }

    // A thud (a sine falling from 120 to 35 Hz) under a short square burst that dies fast.
    static AudioClip CrashClip()
    {
        int count = (int)(0.32f * Rate);
        var data = new float[count];
        float phase = 0f, phase2 = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / Rate, k = (float)i / count;
            float f = Mathf.Lerp(120f, 35f, k);
            phase += 2f * Mathf.PI * f / Rate;
            phase2 += 2f * Mathf.PI * 97f / Rate;
            float thud = Mathf.Sin(phase) * Mathf.Exp(-t * 9f);
            float burst = Mathf.Sign(Mathf.Sin(phase2)) * 0.5f * Mathf.Exp(-t * 22f);
            data[i] = (thud * 0.6f + burst * 0.5f) * Mathf.Min(1f, t / 0.003f) * 0.55f;
        }
        return Clip("crash", data);
    }

    // The siren: a sine sweeping up and down twice.
    static AudioClip SirenClip()
    {
        int count = (int)(1.2f * Rate);
        var data = new float[count];
        float phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / Rate, k = (float)i / count;
            float f = Mathf.Lerp(520f, 780f, 0.5f - 0.5f * Mathf.Cos(k * Mathf.PI * 4f));
            phase += 2f * Mathf.PI * f / Rate;
            float env = Mathf.Min(1f, t / 0.02f) * Mathf.Min(1f, (1.2f - t) / 0.3f);
            data[i] = Mathf.Sin(phase) * env * 0.22f;
        }
        return Clip("siren", data);
    }

    // The hum of a facility in lockdown: two low sines beating, four seconds, the ends
    // crossfaded into a loop.
    static AudioClip Hum()
    {
        int count = Rate * 4, fade = Rate / 5;
        var data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / Rate;
            float a = Mathf.Sin(2f * Mathf.PI * 55f * t), b = Mathf.Sin(2f * Mathf.PI * 57.5f * t) * 0.6f, c = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.25f;
            data[i] = (a + b + c) * 0.4f;
        }
        for (int i = 0; i < fade; i++) { float k = (float)i / fade; data[i] = data[i] * k + data[count - fade + i] * (1f - k); }
        return Clip("hum", data);
    }

    static AudioClip Clip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
