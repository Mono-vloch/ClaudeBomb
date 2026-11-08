using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The few things a play screen wears over the world (the game-feel pass): the objective
// line (one short line, top left), the market clock (dinner time running out, no digits),
// the home window (one face per agent, lit when it is home, crossed when it was sent home),
// the small buttons along the bottom and the one main action. Everything else the world
// shows, or the menu holds.
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

    public static void SetObjective(TextMeshProUGUI t, string text)
    {
        if (t == null) return;
        t.text = text;
        var glass = t.transform.parent.GetComponent<RectTransform>();
        float w = 24 + Mathf.Max(260f, 13f * text.Length);
        glass.sizeDelta = new Vector2(w, 56);
        t.rectTransform.sizeDelta = new Vector2(w - 30, 56);
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

    // ---- the clock: dinner time, from six to seven, no digits ----

    public class Clock
    {
        public readonly RectTransform root;
        readonly RectTransform hour, minute;
        readonly Image face, hourImg, minuteImg;
        readonly float size;
        float shown;

        public Clock(Transform parent, float x, float y, float size = 92)
        {
            this.size = size;
            root = Ui.Node("Clock", parent);
            Ui.Place(root, x, y, size, size);
            face = Ui.Dot(root, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.86f), size);
            Ui.Place(face.rectTransform, 0, 0, size, size);
            face.raycastTarget = false;
            var rim = Ui.Dot(root, new Color(1f, 1f, 1f, 0.22f), size);
            Ui.Place(rim.rectTransform, 0, 0, size, size);
            rim.raycastTarget = false;
            var inner = Ui.Dot(root, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.96f), size - 4);
            Ui.Place(inner.rectTransform, 2, 2, size - 4, size - 4);
            inner.raycastTarget = false;
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f;
                var tick = Ui.Box(root, "Tick", i % 3 == 0 ? new Color(1f, 1f, 1f, 0.8f) : new Color(1f, 1f, 1f, 0.35f), false);
                tick.raycastTarget = false;
                var rt = tick.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(i % 3 == 0 ? 4 : 2, i % 3 == 0 ? 10 : 6);
                rt.localEulerAngles = new Vector3(0, 0, -a);
                rt.anchoredPosition = Quaternion.Euler(0, 0, -a) * new Vector2(0, size / 2 - 13);
            }
            hourImg = Ui.Box(root, "Hour", Color.white, false); hourImg.raycastTarget = false;
            hour = hourImg.rectTransform;
            hour.anchorMin = hour.anchorMax = new Vector2(0.5f, 0.5f); hour.pivot = new Vector2(0.5f, 0.1f);
            hour.sizeDelta = new Vector2(5, size * 0.26f); hour.anchoredPosition = Vector2.zero;
            minuteImg = Ui.Box(root, "Minute", Theme.Teal, false); minuteImg.raycastTarget = false;
            minute = minuteImg.rectTransform;
            minute.anchorMin = minute.anchorMax = new Vector2(0.5f, 0.5f); minute.pivot = new Vector2(0.5f, 0.1f);
            minute.sizeDelta = new Vector2(3, size * 0.38f); minute.anchoredPosition = Vector2.zero;
            var hub = Ui.Dot(root, Theme.Teal, 9);
            Ui.Place(hub.rectTransform, size / 2 - 4.5f, size / 2 - 4.5f, 9, 9);
            hub.raycastTarget = false;
            Set(0f, false, true);
        }

        // fraction: 0 at six o'clock, 1 at seven; amber in the last turns.
        public void Set(float fraction, bool amber, bool jump = false)
        {
            fraction = Mathf.Clamp01(fraction);
            shown = jump ? fraction : Mathf.Lerp(shown, fraction, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
            minute.localEulerAngles = new Vector3(0, 0, -shown * 360f);
            hour.localEulerAngles = new Vector3(0, 0, -(180f + shown * 30f));
            var c = amber ? Theme.Amber : Theme.Teal;
            minuteImg.color = c;
            hourImg.color = amber ? Theme.Amber : Color.white;
            face.color = amber ? new Color(0.3f, 0.2f, 0.05f, 0.9f) : new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.86f);
        }
    }

    // ---- the home window: one face per agent ----

    public enum Face { Out, Home, SentHome, Chosen }

    public class Window
    {
        public readonly RectTransform root;
        readonly Dictionary<AgentState, (Image frame, Image face, Image dot)> faces = new Dictionary<AgentState, (Image, Image, Image)>();
        public System.Action<AgentState> OnPick;

        public Window(Transform parent, IList<AgentState> agents, float right, float y, bool lit)
        {
            int n = agents.Count;
            const float cell = 52f, gap = 8f;
            float w = 24 + n * cell + (n - 1) * gap;
            root = Ui.Node("Window", parent);
            Ui.Place(root, right - w, y, w, 64);
            var glass = Ui.Glass(root, "Glass", 0, 0, w, 64, false);
            glass.color = lit ? new Color(0.32f, 0.26f, 0.08f, 0.9f) : new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.86f);
            var frameLine = Ui.Box(root, "Sill", lit ? Theme.Amber : new Color(1f, 1f, 1f, 0.22f), false);
            Ui.Place(frameLine.rectTransform, 0, 60, w, 4);
            frameLine.raycastTarget = false;
            for (int i = 0; i < n; i++)
            {
                var a = agents[i];
                var c = Theme.Team[a.config.colour];
                var frame = Ui.Box(root, "Face " + a.Name, new Color(1f, 1f, 1f, 0.18f));
                Ui.Place(frame.rectTransform, 12 + i * (cell + gap), 8, cell, cell - 8);
                var face = Ui.Box(frame.transform, "Fill", c);
                Ui.Stretch(face.rectTransform, 3, 3, 3, 3);
                face.raycastTarget = false;
                var eye1 = Ui.Dot(face.transform, Theme.Ink, 7); Ui.Place(eye1.rectTransform, 13, 14, 7, 7); eye1.raycastTarget = false;
                var eye2 = Ui.Dot(face.transform, Theme.Ink, 7); Ui.Place(eye2.rectTransform, 26, 14, 7, 7); eye2.raycastTarget = false;
                var mouth = Ui.Box(face.transform, "Mouth", Theme.Ink, false); Ui.Place(mouth.rectTransform, 16, 28, 14, 3); mouth.raycastTarget = false;
                var cross = Ui.Text(frame.transform, "×", 30, Theme.Danger, true, TextAlignmentOptions.Center);
                Ui.Stretch(cross.rectTransform);
                cross.gameObject.SetActive(false);
                var dot = Ui.Dot(root, Theme.Teal, 8);
                Ui.Place(dot.rectTransform, 12 + i * (cell + gap) + cell / 2 - 4, 56 - 2, 8, 8);
                dot.raycastTarget = false;
                dot.gameObject.SetActive(false);
                var agent = a;
                frame.raycastTarget = true;
                frame.gameObject.AddComponent<Tap>().act = () => OnPick?.Invoke(agent);
                faces[a] = (frame, face, dot);
                crosses[a] = cross;
            }
        }

        readonly Dictionary<AgentState, TextMeshProUGUI> crosses = new Dictionary<AgentState, TextMeshProUGUI>();

        public void Set(AgentState a, Face state, bool chosen)
        {
            if (!faces.TryGetValue(a, out var f)) return;
            var c = Theme.Team[a.config.colour];
            bool home = state == Face.Home, gone = state == Face.SentHome;
            f.face.color = gone ? new Color(0.35f, 0.3f, 0.3f) : home ? c : Color.Lerp(c, new Color(0.25f, 0.28f, 0.34f), 0.6f);
            f.frame.color = home ? Theme.Teal : chosen ? new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.6f) : new Color(1f, 1f, 1f, 0.18f);
            crosses[a].gameObject.SetActive(gone);
            f.dot.gameObject.SetActive(chosen);
        }
    }

    // ---- the loop on the first errand: an eye, a bulb, a foot light in turn, no words ----

    public class LoopIcons
    {
        public readonly RectTransform root;
        readonly Image[] rings = new Image[3];
        readonly Image[] glyphs = new Image[3];

        public LoopIcons(Transform parent, float x, float y)
        {
            root = Ui.Node("Loop", parent);
            Ui.Place(root, x, y, 3 * 44 + 2 * 8, 44);
            for (int i = 0; i < 3; i++)
            {
                rings[i] = Ui.Dot(root, new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.86f), 44);
                Ui.Place(rings[i].rectTransform, i * 52, 0, 44, 44);
                rings[i].raycastTarget = false;
                var t = rings[i].transform;
                if (i == 0)
                {
                    // An eye: a white almond with a dark pupil.
                    var white = Ui.Dot(t, Color.white, 22); Ui.Place(white.rectTransform, 11, 13, 22, 18); white.raycastTarget = false;
                    var pupil = Ui.Dot(white.transform, Theme.Ink, 9); Ui.Place(pupil.rectTransform, 6.5f, 4.5f, 9, 9); pupil.raycastTarget = false;
                    glyphs[i] = white;
                }
                else if (i == 1)
                {
                    // A bulb: a round head on a short base.
                    var bulb = Ui.Dot(t, Color.white, 18); Ui.Place(bulb.rectTransform, 13, 9, 18, 18); bulb.raycastTarget = false;
                    var neck = Ui.Box(t, "Base", Color.white, false); Ui.Place(neck.rectTransform, 18, 27, 8, 6); neck.raycastTarget = false;
                    glyphs[i] = bulb;
                }
                else
                {
                    // A foot: a rounded sole with a toe.
                    var sole = Ui.Box(t, "Sole", Color.white); Ui.Place(sole.rectTransform, 12, 20, 22, 11); sole.raycastTarget = false;
                    var toe = Ui.Dot(t, Color.white, 10); Ui.Place(toe.rectTransform, 25, 12, 10, 10); toe.raycastTarget = false;
                    glyphs[i] = sole;
                }
            }
            Light(-1);
        }

        public void Light(int which)
        {
            for (int i = 0; i < 3; i++)
            {
                bool on = i == which;
                rings[i].color = on ? Theme.Teal : new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.86f);
                rings[i].transform.localScale = Vector3.one * (on ? 1.1f : 1f);
                foreach (var g in rings[i].GetComponentsInChildren<Image>()) if (g != rings[i]) g.color = on ? Theme.Ink : (g.name == "Dot" && g.transform.parent != rings[i].transform ? Theme.Ink : Color.white);
            }
        }
    }

    // ---- a moment over the world ----

    static float OutBack(float k) { const float c = 1.70158f; k -= 1f; return 1f + (c + 1f) * k * k * k + c * k * k; }

    // Dark glass, the words in the moment's colour, in with an overshoot; a big one flashes
    // the frame first and may carry one short line underneath.
    public static IEnumerator Moment(Transform layer, string text, string sub, Color colour, float seconds, bool big, float width = Ui.W)
    {
        Image flash = null;
        if (big)
        {
            flash = Ui.Box(layer, "Flash", new Color(colour.r, colour.g, colour.b, 0f), false);
            Ui.Place(flash.rectTransform, 0, 0, width, Ui.H);
            flash.raycastTarget = false;
        }
        var card = Ui.Box(layer, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        float h = big ? (string.IsNullOrEmpty(sub) ? 150 : 180) : 92;
        float w = big ? 780 : 620;
        Ui.Place(card.rectTransform, (width - w) / 2, 330 - h / 2, w, h);
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", colour, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, h);
        var t = Ui.Text(card.transform, text, big ? (text.Length <= 2 ? 96 : 56) : 36, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = big ? 6 : 4;
        Ui.Place(t.rectTransform, 20, big ? 14 : 20, w - 40, big ? 100 : 52);
        t.enableWordWrapping = false;
        if (!string.IsNullOrEmpty(sub))
        {
            var st = Ui.Text(card.transform, sub, big ? 19 : 16, Look.Chalk, false, TextAlignmentOptions.Center);
            Ui.Place(st.rectTransform, 20, h - (big ? 52 : 32), w - 40, 26);
            st.enableWordWrapping = false; st.overflowMode = TextOverflowModes.Ellipsis;
        }
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(width / 2, -330);
        if (big) Sfx.Sting(colour == Theme.Danger ? Sfx.Mood.Alarm : text == "AGENTS OUT" ? Sfx.Mood.Go : text.Length <= 1 ? Sfx.Mood.Beep : colour == Theme.Amber ? Sfx.Mood.Alarm : Sfx.Mood.Fanfare); else Sfx.Tick();
        yield return Ui.Animate(big ? 0.34f : 0.2f, k =>
        {
            if (card == null) return;
            group.alpha = Mathf.Min(1f, k * 3f);
            float sc = big ? Mathf.LerpUnclamped(0.6f, 1f, OutBack(k)) : Mathf.Lerp(1.12f, 1f, k);
            card.rectTransform.localScale = Vector3.one * sc;
            if (flash != null) flash.color = new Color(colour.r, colour.g, colour.b, 0.45f * Mathf.Max(0f, 1f - k * 2.5f));
        });
        if (flash != null) Object.Destroy(flash.gameObject);
        yield return new WaitForSecondsRealtime(seconds);
        if (card == null) yield break;
        yield return Ui.Animate(0.22f, k => { if (card == null) return; group.alpha = 1 - k; card.rectTransform.localScale = Vector3.one * (1f - 0.08f * k); });
        if (card != null) Object.Destroy(card.gameObject);
    }

    // A two-word arrival over the world: "NEW: BRAIN".
    public static IEnumerator Arrival(Transform layer, string text)
    {
        var card = Ui.Box(layer, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        Ui.Place(card.rectTransform, (Ui.W - 640) / 2, 380, 640, 120);
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", Theme.Teal, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, 120);
        var t = Ui.Text(card.transform, text, 48, Theme.Teal, true, TextAlignmentOptions.Center);
        t.characterSpacing = 6;
        Ui.Stretch(t.rectTransform, 20, 0, 20, 0);
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(Ui.W / 2, -440);
        Sfx.Sting(Sfx.Mood.Fanfare);
        yield return Ui.Animate(0.34f, k => { if (card == null) return; group.alpha = Mathf.Min(1f, k * 3f); card.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, Ui.EaseOutBack(k)); });
        yield return new WaitForSecondsRealtime(1.4f);
        if (card == null) yield break;
        yield return Ui.Animate(0.22f, k => { if (card != null) group.alpha = 1 - k; });
        if (card != null) Object.Destroy(card.gameObject);
    }

    // A tap on a thing that shows state (a face, a bubble): not a form control.
    public class Tap : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public System.Action act;
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData e) => act?.Invoke();
    }

    // A full-screen invisible button: a tap anywhere on the world.
    public static Button TapAnywhere(Transform parent, System.Action act)
    {
        var hit = Ui.Box(parent, "Tap", Color.clear, false);
        Ui.Stretch(hit.rectTransform);
        hit.raycastTarget = true;
        var b = hit.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.None;
        b.onClick.AddListener(() => act?.Invoke());
        return b;
    }
}
