using System.Collections;
using UnityEngine;

// Central sound hub. Two jobs:
// 1) Modules call the Play*() methods directly at the moment of interaction
//    (button clicks, switch flips, wire cuts, chat messages).
// 2) This script itself listens to GameManager's existing events (strikes,
//    module-solved, win/lose) and reacts automatically - modules never need to
//    know about strike/solve/win sounds themselves.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources (2 on this object - see comments)")]
    [SerializeField] AudioSource musicSource; // dedicated: only the startup music, needs its own fading volume
    [SerializeField] AudioSource sfxSource;   // shared: every one-shot effect, via PlayOneShot (overlaps fine)

    [Header("Clicks - sequence, glyphs, grid")]
    [SerializeField] AudioClip metalButtonClip;

    [Header("Switches")]
    [SerializeField] AudioClip switchClip;

    [Header("Wires")]
    [SerializeField] AudioClip electricalShotClip; // any cut, correct or not

    [Header("Chat")]
    [SerializeField] AudioClip messageClip; // both sent and received

    [Header("Startup music")]
    [SerializeField] AudioClip startupMusic;
    [SerializeField] float startupMusicVolume = 0.5f; // half volume, so LUMA's voice stays clear over it
    [SerializeField] float musicFadeDuration = 30f;
    [SerializeField] float musicFadeStepInterval = 3f; // drop 10% every 3s -> 0% at 30s

    [Header("Strikes / outcome")]
    [SerializeField] AudioClip firstStrikeClip;   // strike 1
    [SerializeField] AudioClip finalStrikeClip;   // the fatal strike, plays just before the explosion
    [SerializeField] AudioClip explosionClip;
    [SerializeField] AudioClip fullDefuseClip;    // all 5 modules solved
    [SerializeField] AudioClip moduleSolvedClip;  // each individual module solve
    [SerializeField] float finalStrikeToExplosionDelay = 0.6f;

    [Header("Timer")]
    [SerializeField] AudioClip heartbeatClip;
    [SerializeField] AudioClip clockTickClip;

    // Remaining-time marks (seconds) where heartbeat + tick fire together, for a
    // 10-minute timer: 9:10, 8:10, 7:10 ... 0:10 remaining.
    static readonly int[] TimerMarks = { 550, 490, 430, 370, 310, 250, 190, 130, 70, 10 };
    int nextMarkIndex;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Settings.Apply(); // the menu's sound switch
        GameManager.Instance.OnStrike += HandleStrike;
        GameManager.Instance.OnModuleSolved += HandleModuleSolved;
        GameManager.Instance.OnGameEnded += HandleGameEnded;

        PlayStartupMusic();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStrike -= HandleStrike;
            GameManager.Instance.OnModuleSolved -= HandleModuleSolved;
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
        }
    }

    void Update()
    {
        CheckTimerMarks();
    }

    void CheckTimerMarks()
    {
        if (nextMarkIndex >= TimerMarks.Length) return;
        if (GameManager.Instance.State != GameManager.RunState.Playing) return;

        if (GameManager.Instance.TimeRemaining <= TimerMarks[nextMarkIndex])
        {
            sfxSource.PlayOneShot(heartbeatClip);
            sfxSource.PlayOneShot(clockTickClip);
            nextMarkIndex++;
        }
    }

    // ---- Called directly by modules at the moment of interaction ----
    public void PlayButtonClick() => sfxSource.PlayOneShot(metalButtonClip);
    public void PlaySwitchFlip() => sfxSource.PlayOneShot(switchClip);
    public void PlayWireCut() => sfxSource.PlayOneShot(electricalShotClip);
    public void PlayMessageSound() => sfxSource.PlayOneShot(messageClip);

    // ---- Interface sounds (buttons, the opening, the tour, the HUD) ----
    // Made here from sine waves and noise rather than recorded, once per session.
    public void PlayUiTick() => sfxSource.PlayOneShot(Synth("tick", 0.04f, t => Mathf.Sin(t * 2400f * Tau) * Decay(t, 160f) * 0.25f));
    public void PlayBlip() => sfxSource.PlayOneShot(Synth("blip", 0.03f, t => Mathf.Sign(Mathf.Sin(t * 1150f * Tau)) * Decay(t, 90f) * 0.05f));
    public void PlayWhoosh() => sfxSource.PlayOneShot(Synth("whoosh", 0.7f, Whoosh));
    // note: steps up a pentatonic scale, for chimes that rise one after another.
    public void PlayChime(int note = 0) => sfxSource.PlayOneShot(Synth("chime" + note, 1.4f, t => Bell(t, 660f * Pentatonic(note))));
    public void PlayBeep(bool last = false) => sfxSource.PlayOneShot(Synth(last ? "go" : "beep", last ? 0.7f : 0.16f, t =>
        (Mathf.Sin(t * (last ? 1320f : 880f) * Tau) + (last ? 0.5f * Mathf.Sin(t * 1980f * Tau) : 0f)) * Decay(t, last ? 6f : 30f) * 0.3f));

    const float Tau = Mathf.PI * 2f;
    static readonly System.Collections.Generic.Dictionary<string, AudioClip> synthesized = new System.Collections.Generic.Dictionary<string, AudioClip>();
    static readonly System.Random noise = new System.Random(7);

    static float Decay(float t, float rate) => Mathf.Exp(-t * rate) * Mathf.Clamp01(t * 2000f); // with a tiny attack, no click
    static float Pentatonic(int note) => Mathf.Pow(2f, new[] { 0, 2, 4, 7, 9 }[note % 5] / 12f + note / 5);

    static float Bell(float t, float f) =>
        (Mathf.Sin(t * f * Tau) + 0.45f * Mathf.Sin(t * f * 2.01f * Tau) + 0.2f * Mathf.Sin(t * f * 3.02f * Tau)) * Decay(t, 3.2f) * 0.16f;

    // Noise through a filter that opens and closes: air rushing past.
    static float whooshLow;
    static float Whoosh(float t)
    {
        float k = Mathf.Sin(Mathf.PI * t / 0.7f);
        whooshLow += (float)(noise.NextDouble() * 2 - 1 - whooshLow) * (0.02f + 0.25f * k);
        return whooshLow * k * k * 0.5f;
    }

    static AudioClip Synth(string name, float seconds, System.Func<float, float> wave)
    {
        if (synthesized.TryGetValue(name, out var clip) && clip != null) return clip;
        const int rate = 44100;
        var data = new float[Mathf.CeilToInt(seconds * rate)];
        for (int i = 0; i < data.Length; i++) data[i] = wave(i / (float)rate);
        clip = AudioClip.Create(name, data.Length, 1, rate, false);
        clip.SetData(data, 0);
        return synthesized[name] = clip;
    }

    // ---- Automatic reactions to GameManager's own events ----
    void HandleStrike(ModuleId module)
    {
        bool isFatal = GameManager.Instance.Strikes >= GameManager.Instance.MaxStrikes;
        sfxSource.PlayOneShot(isFatal ? finalStrikeClip : firstStrikeClip);
    }

    void HandleModuleSolved(ModuleId module)
    {
        sfxSource.PlayOneShot(moduleSolvedClip);
    }

    void HandleGameEnded(GameManager.RunState state)
    {
        if (state == GameManager.RunState.Won)
            sfxSource.PlayOneShot(fullDefuseClip);
        else if (state == GameManager.RunState.Lost)
            StartCoroutine(PlayExplosionAfterDelay());
    }

    IEnumerator PlayExplosionAfterDelay()
    {
        yield return new WaitForSeconds(finalStrikeToExplosionDelay);
        sfxSource.PlayOneShot(explosionClip);
    }

    void PlayStartupMusic()
    {
        if (startupMusic == null) return;
        musicSource.clip = startupMusic;
        musicSource.volume = startupMusicVolume;
        musicSource.Play();
        StartCoroutine(FadeMusicOut());
    }

    IEnumerator FadeMusicOut()
    {
        int steps = Mathf.RoundToInt(musicFadeDuration / musicFadeStepInterval); // 10 steps of 3s
        for (int i = 1; i <= steps; i++)
        {
            yield return new WaitForSeconds(musicFadeStepInterval);
            musicSource.volume = startupMusicVolume * Mathf.Max(0f, 1f - i / (float)steps);
        }
        musicSource.Stop();
    }
}
