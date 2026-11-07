using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The few things a play screen wears over the world: the objective line (one short line,
// top left), the fuel gauge (the attempt budget as a needle, no digits), and the small
// buttons along the bottom. Everything else the world shows, or the menu holds.
public static class Hud
{
    // One line, at most eight words, in dark glass at the top left.
    public static TextMeshProUGUI Objective(Transform parent, string text)
    {
        var glass = Ui.Glass(parent, "Objective", 40, 24, 24 + Mathf.Max(260f, 13f * text.Length), 56, true);
        var t = Ui.Text(glass.transform, text, 24, Color.white, true, TextAlignmentOptions.MidlineLeft);
        Ui.Place(t.rectTransform, 22, 0, glass.rectTransform.sizeDelta.x - 30, 56);
        t.enableWordWrapping = false;
        return t;
    }

    // A small button along the bottom of the world.
    public static Button Small(Transform parent, string label, float x, float w, System.Action act, bool accent = false)
    {
        var b = Ui.Button(parent, label, act, accent ? Ui.ButtonStyle.Accent : Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(b.GetComponent<RectTransform>(), x, 1012, w, 48);
        return b;
    }

    // The main action: big, teal, bottom right of the world.
    public static Button Main(Transform parent, string label, System.Action act, float w = 240)
    {
        var b = Ui.Button(parent, label, act, Ui.ButtonStyle.Accent, 22);
        Ui.Place(b.GetComponent<RectTransform>(), Ui.W - 40 - w, 996, w, 64);
        return b;
    }

    // The fuel gauge: the attempts left, as a needle on a dial. Full at the first attempt,
    // empty at the last; a teal mark at full, a red one at empty, and no digits.
    public class Gauge
    {
        public readonly RectTransform root;
        readonly RectTransform needle;
        readonly Image face;
        float shown = 1f;

        public Gauge(Transform parent, float x, float y, float size = 92)
        {
            root = Ui.Node("Fuel", parent);
            Ui.Place(root, x, y, size, size);
            face = Ui.Dot(root, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.82f), size);
            Ui.Place(face.rectTransform, 0, 0, size, size);
            var rim = Ui.Dot(root, new Color(1f, 1f, 1f, 0.22f), size);
            Ui.Place(rim.rectTransform, 0, 0, size, size);
            var inner = Ui.Dot(root, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.95f), size - 4);
            Ui.Place(inner.rectTransform, 2, 2, size - 4, size - 4);
            // Ticks round the dial: teal where it is full, red where it is empty.
            for (int i = 0; i <= 8; i++)
            {
                float a = Mathf.Lerp(-125f, 125f, i / 8f);
                var tick = Ui.Box(root, "Tick", i == 0 ? Theme.Teal : i == 8 ? Theme.Danger : new Color(1f, 1f, 1f, 0.35f), false);
                tick.raycastTarget = false;
                var rt = tick.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(i % 4 == 0 ? 4 : 2, i % 4 == 0 ? 11 : 7);
                rt.anchoredPosition = Vector2.zero;
                rt.localEulerAngles = new Vector3(0, 0, -a);
                rt.anchoredPosition = Quaternion.Euler(0, 0, -a) * new Vector2(0, size / 2 - 14);
            }
            var n = Ui.Box(root, "Needle", Theme.Teal, false);
            n.raycastTarget = false;
            needle = n.rectTransform;
            needle.anchorMin = needle.anchorMax = new Vector2(0.5f, 0.5f);
            needle.pivot = new Vector2(0.5f, 0.1f);
            needle.sizeDelta = new Vector2(4, size * 0.4f);
            needle.anchoredPosition = Vector2.zero;
            var hub = Ui.Dot(root, Theme.Teal, 10);
            Ui.Place(hub.rectTransform, size / 2 - 5, size / 2 - 5, 10, 10);
            Set(1f, true);
        }

        // fraction: 1 full, 0 empty. Glides unless told to jump.
        public void Set(float fraction, bool jump = false)
        {
            fraction = Mathf.Clamp01(fraction);
            shown = jump ? fraction : Mathf.Lerp(shown, fraction, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
            float a = Mathf.Lerp(-125f, 125f, 1f - shown);
            needle.localEulerAngles = new Vector3(0, 0, -a);
            needle.GetComponent<Image>().color = shown > 0.25f ? Theme.Teal : Color.Lerp(Theme.Danger, Theme.Teal, shown / 0.25f);
        }
    }
}
