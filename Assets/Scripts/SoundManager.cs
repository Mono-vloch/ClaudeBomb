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
        musicSource.volume = 1f;
        musicSource.Play();
        StartCoroutine(FadeMusicOut());
    }

    IEnumerator FadeMusicOut()
    {
        int steps = Mathf.RoundToInt(musicFadeDuration / musicFadeStepInterval); // 10 steps of 3s
        for (int i = 1; i <= steps; i++)
        {
            yield return new WaitForSeconds(musicFadeStepInterval);
            musicSource.volume = Mathf.Max(0f, 1f - i * 0.1f);
        }
        musicSource.Stop();
    }
}
