using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The few things a play screen wears over the world: the objective line (one short line, top
// left), the door lamps during the attack, and the small buttons along the bottom. Everything
// else the world shows, or the menu holds.
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

    public static Button Main(Transform parent, string label, System.Action act, float w = 260)
    {
        var b = Ui.Button(parent, label, act, Ui.ButtonStyle.Accent, 22);
        Ui.Place(b.GetComponent<RectTransform>(), Ui.W - 40 - w, 996, w, 64);
        return b;
    }

    // The twelve doors as lamps in a strip of glass at the top centre, no words: the attack's HUD.
    public static Image[] DoorLamps(Transform parent, float x = 760, float y = 24, int count = 12)
    {
        var glass = Ui.Glass(parent, "Doors", x, y, count * 28 + 28, 56, false);
        var lamps = new Image[count];
        for (int i = 0; i < count; i++)
        {
            lamps[i] = Ui.Dot(glass.transform, Color.white, 16);
            Ui.Place(lamps[i].rectTransform, 20 + i * 28, 20, 16, 16);
            lamps[i].raycastTarget = false;
        }
        return lamps;
    }
}
