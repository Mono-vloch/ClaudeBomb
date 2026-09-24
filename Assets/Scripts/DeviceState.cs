using System;
using UnityEngine;

[Serializable]
public class Casing
{
    public string stamp;          // e.g. "R4-TQ9"; last char may be a letter or a digit
    public int cells;             // 0..3
    public bool vent, sync, hold; // lamp states
}

public class DeviceState
{
    public Casing casing = new Casing();
    public string[] wires = new string[4];

    // The four glyph ids (from GlyphSets) in the order they must be pressed.
    public int[] glyphPressOrder;
    // glyphAtPosition[i] = the glyph id shown at pad position i (0-based, position i+1).
    public int[] glyphAtPosition = new int[4];

    public string[] sequence = new string[4];
    public string[] grid = new string[9];
    public bool[] switches = new bool[5];

    static readonly string[] WireColors = { "red", "blue", "yellow", "white", "black" };
    static readonly string[] GridColors = { "red", "blue", "yellow" };
    static readonly string[] SequenceColors = { "red", "blue", "green", "yellow" };

    static readonly int[] SetA = { 1, 3, 5, 2 };
    static readonly int[] SetB = { 4, 2, 6, 7 };
    static readonly int[] SetC = { 3, 7, 1, 6 };
    static readonly int[][] GlyphSets = { SetA, SetB, SetC };

    public static DeviceState Generate()
    {
        var d = new DeviceState();
        d.GenerateCasing();     // must run first: switches/wires answers depend on casing
        d.GenerateWires();
        d.GenerateGlyphs();
        d.GenerateSequence();
        d.GenerateGrid();
        d.GenerateSwitches();
        return d;
    }

    void GenerateCasing()
    {
        char tail = UnityEngine.Random.value < 0.5f ? RandDigit() : RandLetter();
        casing.stamp = $"{RandLetter()}{RandDigit()}-{RandLetter()}{RandLetter()}{tail}";
        casing.cells = UnityEngine.Random.Range(0, 4);
        casing.vent = UnityEngine.Random.value < 0.5f;
        casing.sync = UnityEngine.Random.value < 0.5f;
        casing.hold = UnityEngine.Random.value < 0.5f;
    }

    static char RandLetter() => (char)('A' + UnityEngine.Random.Range(0, 26));
    static char RandDigit() => (char)('0' + UnityEngine.Random.Range(0, 10));

    void GenerateWires()
    {
        for (int i = 0; i < wires.Length; i++)
            wires[i] = WireColors[UnityEngine.Random.Range(0, WireColors.Length)];
    }

    void GenerateGlyphs()
    {
        glyphPressOrder = GlyphSets[UnityEngine.Random.Range(0, GlyphSets.Length)];
        var positions = new[] { 0, 1, 2, 3 };
        Shuffle(positions);
        for (int i = 0; i < 4; i++)
            glyphAtPosition[positions[i]] = glyphPressOrder[i];
    }

    void GenerateSequence()
    {
        for (int i = 0; i < sequence.Length; i++)
            sequence[i] = SequenceColors[UnityEngine.Random.Range(0, SequenceColors.Length)];
    }

    void GenerateGrid()
    {
        for (int i = 0; i < grid.Length; i++)
            grid[i] = GridColors[UnityEngine.Random.Range(0, GridColors.Length)];
    }

    void GenerateSwitches()
    {
        // Reject a start that already matches the target or already violates either
        // interlock pair (2&3, 4&5), so a safe solve path always exists from frame one.
        bool[] target = Rules.SwitchTarget(this);
        bool[] candidate;
        int guard = 0;
        do
        {
            candidate = new bool[5];
            for (int i = 0; i < 5; i++)
                candidate[i] = UnityEngine.Random.value < 0.5f;
            guard++;
        } while (guard < 1000 && (Matches(candidate, target)
                                   || (candidate[1] && candidate[2])
                                   || (candidate[3] && candidate[4])));

        switches = candidate;
    }

    static bool Matches(bool[] a, bool[] b)
    {
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    static void Shuffle(int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}
