using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The few things a play screen wears over the world: the objective line (one short line,
// top left), the fuel gauge (the attempt budget as a needle, no digits, with the player's
// guess for the first finish as a notch on the rim), and the small buttons along the bottom.
// Everything else the world shows, or the menu holds.
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

    public static Button Small(Transform parent, string label, float x, float w, System.Action act, bool accent = false)
    {
        var b = Ui.Button(parent, label, act, accent ? Ui.ButtonStyle.Accent : Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(b.GetComponent<RectTransform>(), x, 1012, w, 48);
        return b;
    }

    public static Button Main(Transform parent, string label, System.Action act, float w = 240)
    {
        var b = Ui.Button(parent, label, act, Ui.ButtonStyle.Accent, 22);
        Ui.Place(b.GetComponent<RectTransform>(), Ui.W - 40 - w, 996, w, 64);
        return b;
    }

    // The fuel gauge: the attempts left, as a needle on a dial. Full at the first attempt,
    // empty at the last; a teal mark at full, a red one at empty, and no digits. The notch
    // on the rim is the player's guess for the first finish: dragged on the pit lane, read
    // on the run.
    public class Gauge
    {
        public readonly RectTransform root;
        readonly RectTransform needle, notch;
        readonly float size;
        float shown = 1f, guess = 0.4f;
        public bool Draggable;
        public System.Action<float> OnGuess;
        public float Guess { get => guess; set { guess = Mathf.Clamp01(value); PlaceNotch(); } }

        const float Sweep = 125f; // degrees either side of straight up

        public Gauge(Transform parent, float x, float y, float size = 92)
        {
            this.size = size;
            root = Ui.Node("Fuel", parent);
            Ui.Place(root, x, y, size, size);
            var face = Ui.Dot(root, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.82f), size);
            Ui.Place(face.rectTransform, 0, 0, size, size);
            face.raycastTarget = true;
            var rim = Ui.Dot(root, new Color(1f, 1f, 1f, 0.22f), size);
            Ui.Place(rim.rectTransform, 0, 0, size, size);
            rim.raycastTarget = false;
            var inner = Ui.Dot(root, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.95f), size - 4);
            Ui.Place(inner.rectTransform, 2, 2, size - 4, size - 4);
            inner.raycastTarget = false;
            for (int i = 0; i <= 8; i++)
            {
                float a = Mathf.Lerp(-Sweep, Sweep, i / 8f);
                var tick = Ui.Box(root, "Tick", i == 0 ? Theme.Teal : i == 8 ? Theme.Danger : new Color(1f, 1f, 1f, 0.35f), false);
                tick.raycastTarget = false;
                var rt = tick.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(i % 4 == 0 ? 4 : 2, i % 4 == 0 ? 11 : 7);
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
            hub.raycastTarget = false;
            // The guess: a small amber notch on the rim, outside the ticks.
            var g = Ui.Box(root, "Guess", Theme.Amber, false);
            g.raycastTarget = false;
            notch = g.rectTransform;
            notch.anchorMin = notch.anchorMax = new Vector2(0.5f, 0.5f);
            notch.pivot = new Vector2(0.5f, 0f);
            notch.sizeDelta = new Vector2(8, 12);
            PlaceNotch();
            var drag = root.gameObject.AddComponent<Drag>();
            drag.gauge = this;
            Set(1f, true);
        }

        void PlaceNotch()
        {
            float a = Mathf.Lerp(-Sweep, Sweep, guess);
            notch.localEulerAngles = new Vector3(0, 0, -a);
            notch.anchoredPosition = Quaternion.Euler(0, 0, -a) * new Vector2(0, size / 2 - 4);
        }

        public void Set(float fraction, bool jump = false)
        {
            fraction = Mathf.Clamp01(fraction);
            shown = jump ? fraction : Mathf.Lerp(shown, fraction, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
            float a = Mathf.Lerp(-Sweep, Sweep, 1f - shown);
            needle.localEulerAngles = new Vector3(0, 0, -a);
            needle.GetComponent<Image>().color = shown > 0.25f ? Theme.Teal : Color.Lerp(Theme.Danger, Theme.Teal, shown / 0.25f);
        }

        // A drag on the dial moves the notch to the angle under the pointer.
        class Drag : MonoBehaviour, IPointerDownHandler, IDragHandler
        {
            public Gauge gauge;
            public void OnPointerDown(PointerEventData e) => Move(e);
            public void OnDrag(PointerEventData e) => Move(e);
            void Move(PointerEventData e)
            {
                if (gauge == null || !gauge.Draggable) return;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(gauge.root, e.position, e.pressEventCamera, out var local);
                var centre = new Vector2(gauge.size / 2f, -gauge.size / 2f);
                var d = local - centre;
                if (d.magnitude < 6f) return;
                float a = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg; // 0 up, clockwise positive
                gauge.Guess = Mathf.InverseLerp(-Sweep, Sweep, Mathf.Clamp(a, -Sweep, Sweep));
                gauge.OnGuess?.Invoke(gauge.Guess);
            }
        }
    }
}
