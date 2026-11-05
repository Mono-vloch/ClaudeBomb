using UnityEngine;

// The few sounds the game makes, drawn from sine waves at start so there is no
// audio file to keep: a tick on buttons, a chime for a thing bought, a low buzz
// for a dinner ruined, a soft whoosh between screens.
public static class Sfx
{
    const int Rate = 22050;
    static AudioSource src;
    static AudioClip tick, buy, ruin, whoosh;

    public static void Init(GameObject host)
    {
        src = host.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
    }

    public static void Tick() => Play(tick != null ? tick : (tick = Tone("tick", new[] { 1400f }, 0.045f, 0.18f)));
    public static void Buy() => Play(buy != null ? buy : (buy = Tone("buy", new[] { 784f, 1175f }, 0.11f, 0.3f)));
    public static void Ruin() => Play(ruin != null ? ruin : (ruin = Tone("ruin", new[] { 150f, 110f }, 0.28f, 0.35f, true)));
    public static void Whoosh() => Play(whoosh != null ? whoosh : (whoosh = Noise("whoosh", 0.25f, 0.12f)));

    static void Play(AudioClip clip) { if (src != null && clip != null) src.PlayOneShot(clip); }

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
        var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
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
        var clip = AudioClip.Create(name, count, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
