using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The curve: points per attempt, one line per run, drawn from thin boxes. The
// one picture the student must always see. Marks the first finish and where the
// curve settled on the last run.
public class Chart
{
    public readonly RectTransform root;
    readonly RectTransform plot;
    readonly TextMeshProUGUI title, legend, yMax, yMin, xMax;
    readonly float w, h;

    public Chart(Transform parent, float x, float y, float width, float height, string heading)
    {
        root = Ui.Node("Chart", parent);
        Ui.Place(root, x, y, width, height);
        var card = Ui.Box(root, "Card", new Color(1, 1, 1, 0.9f));
        Ui.Stretch(card.rectTransform);
        card.raycastTarget = false;
        title = Ui.Eyebrow(root, heading, Theme.TealDeep);
        Ui.Place(title.rectTransform, 16, 10, 400, 20);
        legend = Ui.Text(root, "", 12, Theme.Text2, false, TextAlignmentOptions.TopRight);
        Ui.Place(legend.rectTransform, width - 616, 10, 600, 20);
        plot = Ui.Node("Plot", root);
        w = width - 72; h = height - 60;
        Ui.Place(plot, 56, 36, w, h);
        yMax = Ui.Text(root, "", 11, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(yMax.rectTransform, 4, 32, 48, 16);
        yMin = Ui.Text(root, "", 11, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(yMin.rectTransform, 4, 36 + h - 14, 48, 16);
        xMax = Ui.Text(root, "", 11, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(xMax.rectTransform, 56 + w - 120, 36 + h + 4, 120, 16);
    }

    public void Draw(IList<Training> runs, int highlight = -1)
    {
        Ui.Clear(plot);
        var axis = Ui.Box(plot, "Axis", Theme.Border, false);
        Ui.Place(axis.rectTransform, 0, 0, 1, h);
        var baseLine = Ui.Box(plot, "Axis", Theme.Border, false);
        Ui.Place(baseLine.rectTransform, 0, h - 1, w, 1);
        if (runs == null || runs.Count == 0) { yMax.text = yMin.text = xMax.text = ""; legend.text = ""; return; }

        float lo = float.MaxValue, hi = float.MinValue; int longest = 1;
        foreach (var r in runs) { foreach (var a in r.curve) { lo = Mathf.Min(lo, a.points); hi = Mathf.Max(hi, a.points); } longest = Mathf.Max(longest, r.s.attempts); }
        if (lo == float.MaxValue) { lo = -100; hi = 100; }
        if (hi - lo < 10) { hi = lo + 10; }
        lo = Mathf.Min(lo, 0); hi = Mathf.Max(hi, 0);
        float zeroY = h - (0 - lo) / (hi - lo) * h;
        var zero = Ui.Box(plot, "Zero", new Color(0.8f, 0.84f, 0.88f, 0.6f), false);
        Ui.Place(zero.rectTransform, 0, zeroY, w, 1);
        yMax.text = hi.ToString("0"); yMin.text = lo.ToString("0"); xMax.text = longest + " attempts";

        var colours = new[] { Theme.Aqua, Theme.Hex("#4F8EF7"), Theme.Amber, Theme.Hex("#9B6BFF"), Theme.Danger };
        var names = new List<string>();
        for (int i = 0; i < runs.Count; i++)
        {
            var r = runs[i];
            var colour = colours[i % colours.Length];
            bool last = i == runs.Count - 1;
            names.Add($"<color=#{ColorUtility.ToHtmlStringRGB(colour)}>■</color> {(string.IsNullOrEmpty(r.label) ? "Run " + (i + 1) : r.label)}");
            // Smoothed a little, so the line reads as a trend, and thinned to what the width can show.
            var pts = Smooth(r.curve.Select(a => a.points).ToList(), 5);
            int step = Mathf.Max(1, pts.Count / 160);
            Vector2? prev = null;
            for (int k = 0; k < pts.Count; k += step)
            {
                var p = new Vector2(k / (float)longest * w, h - (pts[k] - lo) / (hi - lo) * h);
                if (prev != null) Segment(prev.Value, p, colour, last ? 2.5f : 1.5f, last ? 1f : 0.55f);
                prev = p;
            }
            if (last && r.firstFinish > 0) Mark(r.firstFinish / (float)longest * w, Theme.Amber, "first finish");
            if (last && r.settled > 0) Mark(r.settled / (float)longest * w, Theme.TealDeep, "settled");
        }
        legend.text = string.Join("   ", names);
    }

    static List<float> Smooth(List<float> v, int window)
    {
        var o = new List<float>(v.Count);
        for (int i = 0; i < v.Count; i++)
        {
            int a = Mathf.Max(0, i - window), b = Mathf.Min(v.Count - 1, i + window);
            float s = 0; for (int j = a; j <= b; j++) s += v[j];
            o.Add(s / (b - a + 1));
        }
        return o;
    }

    void Segment(Vector2 a, Vector2 b, Color colour, float thick, float alpha)
    {
        var seg = Ui.Box(plot, "Seg", new Color(colour.r, colour.g, colour.b, alpha), false);
        seg.raycastTarget = false;
        var rt = seg.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 0.5f);
        var d = b - a;
        float len = d.magnitude;
        rt.sizeDelta = new Vector2(len + thick, thick);
        rt.anchoredPosition = new Vector2(a.x, -a.y);
        rt.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    void Mark(float x, Color colour, string label)
    {
        var line = Ui.Box(plot, "Mark", new Color(colour.r, colour.g, colour.b, 0.7f), false);
        line.raycastTarget = false;
        Ui.Place(line.rectTransform, x, 0, 1.5f, h);
        var t = Ui.Text(plot, label, 10, colour, true);
        Ui.Place(t.rectTransform, x + 4, 2, 90, 14);
        t.enableWordWrapping = false;
    }
}
