using System;
using System.Linq;
using TMPro;
using UnityEngine;

public enum ModuleId { Wires, Glyphs, Sequence, Grid, Switches }

// Each module's number and name as printed on its plate on the device, in plate order.
public static class Plates
{
    public static readonly (ModuleId id, int number, string name)[] All =
    {
        (ModuleId.Wires, 1, "Wire bank"), (ModuleId.Grid, 2, "Colour grid"), (ModuleId.Sequence, 3, "Sequence strip"),
        (ModuleId.Glyphs, 4, "Glyph pad"), (ModuleId.Switches, 5, "Switch bank"),
    };

    public static (ModuleId id, int number, string name) Of(ModuleId id) => System.Array.Find(All, p => p.id == id);
}

// Owns all device/run truth: the generated DeviceState, timer, strikes and solved flags.
// Modules read input and call Rules themselves, then report outcomes here — this class
// never computes a module's answer.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum RunState { Playing, Won, Lost }

    [SerializeField] float startingSeconds = 600f; // replaced by the menu's clock setting (Settings.ClockMinutes)
    [SerializeField] int maxStrikes = 3; // a first mistake costs a third of the lives, not half

    [SerializeField] TMP_Text timerText;   // the display on the device itself
    [SerializeField] TMP_Text strikesText; // the old readout in the chat header - hidden, the HUD shows strikes
    [SerializeField] Color timerSafeColor = Color.green;    // first 33% of time remaining
    [SerializeField] Color timerWarningColor = new Color(1f, 0.55f, 0f); // middle 33%
    [SerializeField] Color timerDangerColor = Color.red;    // final 33%

    public DeviceState Device { get; private set; }
    public RunState State { get; private set; } = RunState.Playing;
    public int Strikes { get; private set; }
    public int MaxStrikes => maxStrikes;
    public float TimeRemaining { get; private set; }
    // The clock waits until the opening - title, briefing, LUMA's tour - is over
    // (Opening calls StartClock).
    public bool ClockStarted { get; private set; }
    public void StartClock() => ClockStarted = true;
    // Practice: the menu's clock setting is "no clock". Nothing counts down.
    public bool NoClock { get; private set; }

    // Every device comes from a seed, so "try this device again" can rebuild the same
    // one: set ReplaySeed before reloading the scene.
    public int Seed { get; private set; }
    public static int? ReplaySeed { get; set; }

    // Stops only the countdown - the bomb stays playable - e.g. while LUMA is talking,
    // so listening to the coach never costs the player time.
    readonly System.Collections.Generic.HashSet<object> clockHolds = new System.Collections.Generic.HashSet<object>();
    public void HoldClock(object owner) => clockHolds.Add(owner);
    public void ReleaseClock(object owner) => clockHolds.Remove(owner);

    public event Action<RunState> OnGameEnded;
    public event Action<ModuleId> OnModuleSolved;
    public event Action<ModuleId> OnStrike;

    readonly bool[] solved = new bool[5];

    void Awake()
    {
        Instance = this;
        GamePause.ResetAll();
        Seed = ReplaySeed ?? Environment.TickCount;
        ReplaySeed = null;
        UnityEngine.Random.InitState(Seed);
        Device = DeviceState.Generate();
        int minutes = Settings.ClockMinutes;
        NoClock = minutes <= 0;
        if (!NoClock) startingSeconds = minutes * 60f;
        TimeRemaining = startingSeconds;

#if UNITY_EDITOR
        // Dev-only cheat sheet for testing in the Editor. Never in a build: in the web
        // version Debug.Log lands in the browser console, where players can read it.
        Debug.Log("[GameManager] SOLUTION -- " +
            $"casing: stamp={Device.casing.stamp} cells={Device.casing.cells} vent={Device.casing.vent} sync={Device.casing.sync} hold={Device.casing.hold} | " +
            $"wire: cut {Rules.WireAnswer(Device)} | " +
            $"glyphs: press positions {string.Join(",", Rules.GlyphAnswer(Device))} | " +
            $"sequence flashed [{string.Join(",", Device.sequence)}] -> press {string.Join(",", Rules.SequenceAnswer(Device))} | " +
            $"grid: press cell {Rules.GridAnswer(Device)} | " +
            $"switches target: {string.Join(",", Rules.SwitchTarget(Device).Select(b => b ? "up" : "down"))}");
#endif
    }

    void Start()
    {
        if (strikesText != null) strikesText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (State != RunState.Playing) return;
        if (!ClockStarted || NoClock || clockHolds.Count > 0) { UpdateHud(); return; }

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
    public int SolvedCount => solved.Count(s => s);
    public float StartingSeconds => startingSeconds;

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
            timerText.text = NoClock ? "--:--" : FormatTime(TimeRemaining);
            timerText.color = GetTimerColor();
        }
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
