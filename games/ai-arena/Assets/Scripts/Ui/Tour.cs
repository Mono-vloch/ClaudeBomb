using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// LUMA shows the setup screen around on the first errand: the rest of the
// screen goes dark, one part stays lit, and she says what it is for. Next moves
// on, Skip ends it, and each step moves on by itself once she has said the line.
public static class Tour
{
    const string Pref = "tour_seen";
    public static bool Seen
    {
        get => PlayerPrefs.GetInt(Pref, 0) == 1;
        set { PlayerPrefs.SetInt(Pref, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static IEnumerator Run(App app, RectTransform screen, (RectTransform target, string line)[] steps)
    {
        Seen = true;
        var overlay = Ui.Node("Tour", screen);
        Ui.Stretch(overlay);
        var dim = new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.72f);
        var dims = new Image[4];
        for (int i = 0; i < 4; i++) { dims[i] = Ui.Box(overlay, "Dim", dim, false); dims[i].raycastTarget = true; }
        var edges = new Image[4];
        for (int i = 0; i < 4; i++) { edges[i] = Ui.Box(overlay, "Edge", Theme.Teal, false); edges[i].raycastTarget = false; }
        bool next = false, skip = false;
        var (card, text) = Luma.Make(overlay, 0, 0, 600, 170, "", 18);
        var skipBtn = Ui.Button(card.transform, "Skip", () => skip = true, Ui.ButtonStyle.Quiet, 15);
        Ui.Place(skipBtn.GetComponent<RectTransform>(), 600 - 232, 170 - 50, 90, 38);
        var nextBtn = Ui.Button(card.transform, "Next", () => next = true, Ui.ButtonStyle.Primary, 16);
        Ui.Place(nextBtn.GetComponent<RectTransform>(), 600 - 132, 170 - 50, 116, 38);
        var count = Ui.Text(card.transform, "", 13, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(count.rectTransform, 600 - 100, 14, 86, 20);

        float width = overlay.rect.width, height = overlay.rect.height;
        var corners = new Vector3[4];
        for (int s = 0; s < steps.Length && !skip; s++)
        {
            if (overlay == null || steps[s].target == null) yield break; // the screen is gone
            next = false;
            count.text = $"{s + 1} / {steps.Length}";
            // The target's rectangle, in the overlay's top-left coordinates.
            steps[s].target.GetWorldCorners(corners);
            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;
            foreach (var c in corners)
            {
                var l = overlay.InverseTransformPoint(c);
                xMin = Mathf.Min(xMin, l.x); xMax = Mathf.Max(xMax, l.x); yMin = Mathf.Min(yMin, l.y); yMax = Mathf.Max(yMax, l.y);
            }
            float x = xMin - overlay.rect.xMin - 8, y = overlay.rect.yMax - yMax - 8, w = xMax - xMin + 16, h = yMax - yMin + 16;
            Ui.Place(dims[0].rectTransform, 0, 0, width, y);
            Ui.Place(dims[1].rectTransform, 0, y + h, width, height - y - h);
            Ui.Place(dims[2].rectTransform, 0, y, x, h);
            Ui.Place(dims[3].rectTransform, x + w, y, width - x - w, h);
            Ui.Place(edges[0].rectTransform, x, y, w, 3);
            Ui.Place(edges[1].rectTransform, x, y + h - 3, w, 3);
            Ui.Place(edges[2].rectTransform, x, y, 3, h);
            Ui.Place(edges[3].rectTransform, x + w - 3, y, 3, h);
            // LUMA's card below the lit part when there is room, else above it.
            float cx = Mathf.Clamp(x, 40, width - 640);
            float cy = y + h + 18 + 170 <= height - 20 ? y + h + 18 : y - 18 - 170;
            Ui.Place(card.rectTransform, cx, cy, 600, 170);

            bool typed = false;
            string line = app.luma.Say(steps[s].line);
            var typing = app.StartCoroutine(Then(Ui.Type(text, line, 45f, () => next || skip), () => typed = true));
            float after = 0f;
            while (!next && !skip)
            {
                if (overlay == null) { app.StopCoroutine(typing); yield break; }
                if (typed && !app.luma.Speaking) { after += Time.unscaledDeltaTime; if (after > 1.4f) break; }
                yield return null;
            }
            app.luma.Hush();
            app.StopCoroutine(typing);
        }
        if (overlay != null) UnityEngine.Object.Destroy(overlay.gameObject);
    }

    static IEnumerator Then(IEnumerator routine, Action after) { yield return routine; after(); }
}
