using System;
using System.Linq;
using TMPro;
using UnityEngine;

public enum ModuleId { Wires, Glyphs, Sequence, Grid, Switches }

// Owns all device/run truth: the generated DeviceState, timer, strikes and solved flags.
// Modules read input and call Rules themselves, then report outcomes here — this class
// never computes a module's answer.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum RunState { Playing, Won, Lost }

    [SerializeField] float startingSeconds = 600f;
    [SerializeField] int maxStrikes = 2;

    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text strikesText;
    [SerializeField] Color timerSafeColor = Color.green;    // first 33% of time remaining
    [SerializeField] Color timerWarningColor = new Color(1f, 0.55f, 0f); // middle 33%
    [SerializeField] Color timerDangerColor = Color.red;    // final 33%

    public DeviceState Device { get; private set; }
    public RunState State { get; private set; } = RunState.Playing;
    public int Strikes { get; private set; }
    public int MaxStrikes => maxStrikes;
    public float TimeRemaining { get; private set; }

    public event Action<RunState> OnGameEnded;
    public event Action<ModuleId> OnModuleSolved;
    public event Action<ModuleId> OnStrike;

    readonly bool[] solved = new bool[5];

    void Awake()
    {
        Instance = this;
        Device = DeviceState.Generate();
        TimeRemaining = startingSeconds;

        // Dev-only cheat sheet: lets you test each module in isolation without the
        // manual/chat UI built yet. Fine to leave in - it's Debug.Log, not gameplay.
        Debug.Log("[GameManager] SOLUTION -- " +
            $"casing: stamp={Device.casing.stamp} cells={Device.casing.cells} vent={Device.casing.vent} sync={Device.casing.sync} hold={Device.casing.hold} | " +
            $"wire: cut {Rules.WireAnswer(Device)} | " +
            $"glyphs: press positions {string.Join(",", Rules.GlyphAnswer(Device))} | " +
            $"sequence flashed [{string.Join(",", Device.sequence)}] -> press {string.Join(",", Rules.SequenceAnswer(Device))} | " +
            $"grid: press cell {Rules.GridAnswer(Device)} | " +
            $"switches target: {string.Join(",", Rules.SwitchTarget(Device).Select(b => b ? "up" : "down"))}");
    }

    void Update()
    {
        if (State != RunState.Playing) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            UpdateHud();
            Lose();
            return;
        }

        UpdateHud();
    }

    public bool IsSolved(ModuleId module) => solved[(int)module];

    public void ReportSolved(ModuleId module)
    {
        if (State != RunState.Playing || solved[(int)module]) return;

        solved[(int)module] = true;
        Debug.Log($"[GameManager] SOLVED: {module}");
        OnModuleSolved?.Invoke(module);

        bool allSolved = true;
        foreach (var s in solved) allSolved &= s;
        if (allSolved) Win();
    }

    public void ReportStrike(ModuleId module)
    {
        if (State != RunState.Playing) return;

        Strikes++;
        Debug.Log($"[GameManager] STRIKE ({Strikes}/{maxStrikes}) from: {module}");
        OnStrike?.Invoke(module);
        UpdateHud();

        if (Strikes >= maxStrikes) Lose();
    }

    void Win()
    {
        State = RunState.Won;
        Debug.Log("[GameManager] WIN");
        OnGameEnded?.Invoke(State);
    }

    void Lose()
    {
        State = RunState.Lost;
        Debug.Log("[GameManager] LOSE");
        OnGameEnded?.Invoke(State);
    }

    void UpdateHud()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(TimeRemaining);
            timerText.color = GetTimerColor();
        }

        if (strikesText != null)
            strikesText.text = $"strikes  {Strikes}/{maxStrikes}";
    }

    Color GetTimerColor()
    {
        float fraction = TimeRemaining / startingSeconds;
        if (fraction > 2f / 3f) return timerSafeColor;
        if (fraction > 1f / 3f) return timerWarningColor;
        return timerDangerColor;
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }
}
