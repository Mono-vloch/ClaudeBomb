using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectBootstrap
{
    // Step 1 verification harness: generate random devices and print every module's
    // correct answer next to the raw state that produced it, so it can be checked by
    // hand against the manual before any scene/UI is built on top of Rules.cs.
    public static class RulesCheck
    {
        [MenuItem("Claude Crisis Protocol/Verify Rules (log 20 devices)")]
        public static void LogTwentyDevices()
        {
            for (int i = 0; i < 20; i++)
                Debug.Log(Dump(DeviceState.Generate(), i + 1));
        }

        static string Dump(DeviceState d, int index)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"--- device {index} ---");
            sb.AppendLine($"casing: stamp={d.casing.stamp} cells={d.casing.cells} vent={d.casing.vent} sync={d.casing.sync} hold={d.casing.hold}");

            sb.AppendLine($"wires: [{string.Join(", ", d.wires)}]  -> cut wire {Rules.WireAnswer(d)}");

            string glyphLine = "";
            for (int p = 0; p < 4; p++)
                glyphLine += $"pos{p + 1}=g{d.glyphAtPosition[p]} ";
            sb.AppendLine($"glyphs: {glyphLine}(set order: {string.Join(",", d.glyphPressOrder)}) -> press positions [{string.Join(", ", Rules.GlyphAnswer(d))}]");

            sb.AppendLine($"sequence flashed: [{string.Join(", ", d.sequence)}] -> press [{string.Join(", ", Rules.SequenceAnswer(d))}]");

            sb.AppendLine($"grid: [{string.Join(", ", d.grid)}] -> press cell {Rules.GridAnswer(d)}");

            var target = Rules.SwitchTarget(d);
            bool noInterlockViolation = !(d.switches[1] && d.switches[2]) && !(d.switches[3] && d.switches[4]);
            sb.AppendLine($"switches start: [{BoolRow(d.switches)}] target: [{BoolRow(target)}] " +
                           $"(start valid: {noInterlockViolation && !Same(d.switches, target)})");

            return sb.ToString();
        }

        static string BoolRow(bool[] b)
        {
            var parts = new string[b.Length];
            for (int i = 0; i < b.Length; i++) parts[i] = b[i] ? "up" : "down";
            return string.Join(", ", parts);
        }

        static bool Same(bool[] a, bool[] b)
        {
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }
    }
}
