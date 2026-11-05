using UnityEngine;

// Every sound the game makes, drawn from waves at start so there is no audio file to
// keep: a tick on buttons, a chime for a finish, a thud and sparks for a crash, beeps for
// the countdown, a fanfare for the race, an alarm for the wrong reward, the engine whose
// pitch follows the car's speed, and a quiet wind over the circuit so the world is never
// silent. One static gain layer (no mixer asset: those are made in the Editor's window)
// with a sound switch remembered on the machine; everything ducks under LUMA's voice.
public static class Sfx
{
    const int Rate = 22050;
    const string PrefSound = "robot_racing_sound";
    public static bool Enabled { get => PlayerPrefs.GetInt(PrefSound, 1) == 1; set { PlayerPrefs.SetInt(PrefSound, value ? 1 : 0); PlayerPrefs.Save(); Apply(); } }
    public static float Level = 1f;            // the one gain every source multiplies in
    public static bool EngineAudible = true;   // off at the fast training speeds
    public static Luma luma;                   // whose voice ducks the rest

    static AudioSource src, engine, ambient;
    static AudioClip tick, buy, ruin, whoosh, crash, chime, beep, go, fanfare, alarm;
    static float engineSpeed, engineWant, ambientWant;

    public enum Mood { Beep, Go, Fanfare, Alarm }

    public static void Init(GameObject host)
    {
        src = host.AddComponent<AudioSource>(); src.playOnAwake = false; src.spatialBlend = 0f;
        engine = host.AddComponent<AudioSource>(); engine.playOnAwake = false; engine.spatialBlend = 0f; engine.loop = true; engine.volume = 0f;
        engine.clip = EngineLoop();
        ambient = host.AddComponent<AudioSource>(); ambient.playOnAwake = false; ambient.spatialBlend = 0f; ambient.loop = true; ambient.volume = 0f;
        ambient.clip = Wind();
        host.AddComponent<Runner>();
        Apply();
    }

    static void Apply() { AudioListener.volume = Enabled ? 1f : 0f; }

    public static void Tick() => Play(tick != null ? tick : (tick = Tone("tick", new[] { 1400f }, 0.045f, 0.18f)));
    public static void Buy() => Play(buy != null ? buy : (buy = Tone("buy", new[] { 784f, 1175f }, 0.11f, 0.3f)));
    public static void Ruin() => Play(ruin != null ? ruin : (ruin = Tone("ruin", new[] { 150f, 110f }, 0.28f, 0.35f, true)));
    public static void Whoosh() => Play(whoosh != null ? whoosh : (whoosh = Noise("whoosh", 0.25f, 0.12f)));
    public static void Crash(float strength = 1f) => Play(crash != null ? crash : (crash = CrashClip()), strength);
    public static void Chime() => Play(chime != null ? chime : (chime = Tone("chime", new[] { 880f, 1320f }, 0.09f, 0.28f)));

    public static void Sting(Mood mood)
    {
        switch (mood)
        {
            case Mood.Beep: Play(beep != null ? beep : (beep = Tone("beep", new[] { 880f }, 0.1f, 0.3f))); break;
            case Mood.Go: Play(go != null ? go : (go = Tone("go", new[] { 1320f, 1320f }, 0.14f, 0.34f))); break;
            case Mood.Fanfare: Play(fanfare != null ? fanfare : (fanfare = Tone("fanfare", new[] { 523f, 659f, 784f, 1046f }, 0.13f, 0.3f))); break;
            case Mood.Alarm: Play(alarm != null ? alarm : (alarm = Tone("alarm", new[] { 660f, 440f, 660f, 440f }, 0.11f, 0.26f, true))); break;
        }
    }

    // The engine: speed 0 to 1. It is heard only when the training runs at a watchable speed.
    public static void Engine(float speed01) => engineWant = Mathf.Clamp01(speed01);
    public static void Ambient(bool on) => ambientWant = on ? 1f : 0f;
    public static bool EnginePlaying => engine != null && engine.isPlaying && engine.volume > 0.01f;
    public static float EnginePitch => engine != null ? engine.pitch : 0f;
    public static bool AmbientPlaying => ambient != null && ambient.isPlaying && ambient.volume > 0.001f;

    static void Play(AudioClip clip, float gain = 1f) { if (src != null && clip != null) src.PlayOneShot(clip, gain * Level); }

    class Runner : MonoBehaviour
    {
        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float duck = luma != null && luma.Speaking ? 0.3f : 1f;
            engineSpeed = Mathf.Lerp(engineSpeed, EngineAudible ? engineWant : 0f, 1f - Mathf.Exp(-6f * dt));
            float ev = (engineSpeed > 0.02f ? 0.1f + 0.2f * engineSpeed : 0f) * Level * duck;
            engine.volume = Mathf.Lerp(engine.volume, ev, 1f - Mathf.Exp(-8f * dt));
            engine.pitch = 0.75f + 1.05f * engineSpeed;
            if (engine.volume > 0.005f && !engine.isPlaying) engine.Play();
            if (engine.volume <= 0.005f && engine.isPlaying && ev <= 0f) engine.Stop();
            float av = ambientWant * 0.08f * Level * duck;
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

    static AudioClip Noise(string name, float seconds, float gain)
    {
        int count = (int)(seconds * Rate);
        var data = new float[count];
        var rng = new System.Random(7);
        float last = 0f;
        for (int i = 0; i < count; i++)
        {
            float k = (float)i / count;
            float env = Mathf.Sin(k * Mathf.PI);
            last = 0.85f * last + 0.15f * ((float)rng.NextDouble() * 2f - 1f); // softened noise
            data[i] = last * env * gain;
        }
        return Clip(name, data);
    }

    // A thud (a sine falling from 120 to 35 Hz) under a burst of noise that dies fast.
    static AudioClip CrashClip()
    {
        int count = (int)(0.32f * Rate);
        var data = new float[count];
        var rng = new System.Random(11);
        float phase = 0f, last = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / Rate, k = (float)i / count;
            float f = Mathf.Lerp(120f, 35f, k);
            phase += 2f * Mathf.PI * f / Rate;
            float thud = Mathf.Sin(phase) * Mathf.Exp(-t * 9f);
            last = 0.6f * last + 0.4f * ((float)rng.NextDouble() * 2f - 1f);
            float burst = last * Mathf.Exp(-t * 22f);
            data[i] = (thud * 0.6f + burst * 0.5f) * Mathf.Min(1f, t / 0.003f) * 0.55f;
        }
        return Clip("crash", data);
    }

    // The engine: a saw and its octave below, four seconds that loop without a seam
    // (368 and 184 whole cycles), the same length as the wind. Plain waves only: on the
    // Mac this was made on, the same loop with a noise layer under it made the Editor
    // quit while it played aloud (never while muted), and this wave never did.
    static AudioClip EngineLoop()
    {
        int count = Rate * 4;
        var data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / Rate;
            float saw = 2f * (t * 92f - Mathf.Floor(t * 92f + 0.5f));
            float sub = Mathf.Sin(2f * Mathf.PI * 46f * t);
            data[i] = (saw * 0.35f + sub * 0.45f) * 0.5f;
        }
        return Clip("engine", data);
    }

    // Wind over the circuit: brown noise, four seconds, the ends crossfaded into a loop.
    static AudioClip Wind()
    {
        int count = Rate * 4, fade = Rate / 5;
        var data = new float[count];
        var rng = new System.Random(5);
        float v = 0f;
        for (int i = 0; i < count; i++)
        {
            v = Mathf.Clamp(v + ((float)rng.NextDouble() * 2f - 1f) * 0.02f, -1f, 1f);
            v *= 0.999f;
            float swell = 0.7f + 0.3f * Mathf.Sin((float)i / count * Mathf.PI * 2f);
            data[i] = v * swell;
        }
        for (int i = 0; i < fade; i++) { float k = (float)i / fade; data[i] = data[i] * k + data[count - fade + i] * (1f - k); }
        return Clip("wind", data);
    }

    static AudioClip Clip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
