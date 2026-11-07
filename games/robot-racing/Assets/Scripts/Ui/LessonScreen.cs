using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing is a transmission, as Claude Bomb's: the car on its grid in the trackside
// camera's feed on the left, and on the right, from race control, three or four short lines
// typed in and one button. No chips, no cards, no ladder. Everything the course says beyond
// that is in the file, one icon away.
public class LessonScreen
{
    public readonly RectTransform root;
    const float PanelX = 1240f, PanelW = 640f;
    bool hurry;

    public LessonScreen(Transform parent, App app)
    {
        var l = app.lesson;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The world: this level's track, the car on the grid, in the camera feed on the left.
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 40, 60, 1100, 960);
        var track = Track.Get(l.training[0]);
        app.world.Build(track, false);
        if (Circuit.HasKit) { app.world.ShowGrid(frame); CameraFeed.Build(frame, "CAM 3  /  THE GRID", "MAALOOMATIIA RACEWAY  /  EVENING PRACTICE"); }
        else app.world.ShowOverview(frame);
        app.atmosphere.MenuBlur(false);

        // The transmission, right, on the ink.
        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, PanelX - 80, -40, 1000, 1200);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip; fade.color = Theme.Ink; fade.raycastTarget = false;
        fade.rectTransform.localScale = new Vector3(-1, 1, 1);
        Ui.Place(fade.rectTransform, PanelX - 80, -40, 200, 1200);

        var (header, sub, lines) = Transmission(l, track);
        var dot = Ui.Dot(root, Theme.Teal, 12);
        Ui.Place(dot.rectTransform, PanelX, 318, 12, 12);
        var head = Ui.Text(root, header, 30, Color.white, true);
        Ui.Place(head.rectTransform, PanelX + 24, 300, PanelW - 140, 44);
        var subText = Ui.Eyebrow(root, sub, Theme.Faint);
        Ui.Place(subText.rectTransform, PanelX + 24, 348, PanelW - 140, 20);
        var skip = Ui.Button(root, "Skip", () => hurry = true, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(skip.GetComponent<RectTransform>(), PanelX + PanelW - 100, 300, 100, 40);
        GameMenu.Icon(root);

        var texts = new List<TextMeshProUGUI>();
        float y = 400;
        foreach (var line in lines)
        {
            bool last = line == lines[lines.Count - 1];
            var t = Ui.Text(root, "", 24, last ? Theme.Teal : Color.white, last);
            float h = t.GetPreferredValues(line, PanelW, 0).y + 8;
            Ui.Place(t.rectTransform, PanelX, y, PanelW, h);
            texts.Add(t);
            y += h + 14;
        }
        var go = Ui.Button(root, "To the pit lane", app.ShowPit, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), PanelX, y + 24, 300, 64);
        var runner = root.gameObject.AddComponent<Runner>();
        runner.StartCoroutine(TypeAll(texts, lines));
    }

    IEnumerator TypeAll(List<TextMeshProUGUI> texts, List<string> lines)
    {
        yield return new WaitForSecondsRealtime(0.4f);
        for (int i = 0; i < texts.Count; i++)
        {
            if (texts[i] == null) yield break;
            yield return Ui.Type(texts[i], lines[i], 48f, () => hurry);
            if (!hurry) yield return new WaitForSecondsRealtime(0.35f);
        }
    }

    // What race control says before each level: three or four short lines of story. The
    // middle lines are the level's own words (its kicker and LUMA's intro), cut to sentences.
    public static (string header, string sub, List<string> lines) Transmission(Lesson l, Track track)
    {
        var lines = new List<string>();
        string sub = "evening practice";
        switch (l.number)
        {
            case 1:
                lines.Add("The robot on the grid has never driven.");
                lines.Add("Nobody steers it. It senses, moves, gets points, and goes again.");
                lines.Add("Watch it crash, then crash later, then finish.");
                break;
            case 6:
                sub = "the hairpin";
                lines.Add("A long way round, with a tight turn back.");
                lines.Add("Points only at the end leave the robot in the dark.");
                lines.Add("Small points on the way light the path.");
                break;
            default:
                sub = track.name.ToLowerInvariant();
                lines.Add(track.note);
                foreach (var s in Sentences(l.kicker)) if (lines.Count < 3) lines.Add(s);
                var cue = Luma.FirstSentence(l.luma);
                if (cue != null && lines.Count < 4 && !lines.Contains(cue)) lines.Add(cue);
                break;
        }
        return ("Race control", sub, lines);
    }

    static IEnumerable<string> Sentences(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        foreach (var p in System.Text.RegularExpressions.Regex.Split(text.Trim(), @"(?<=[.!?])\s+"))
            if (p.Trim().Length > 0) yield return p.Trim();
    }

    // The track as a small map: road light, walls dark, start and finish marked (the file uses it).
    public static void Map(Transform parent, Track t, float x, float y, float w, float h)
    {
        float cell = Mathf.Min(w / t.W, h / t.H);
        float ox = x + (w - cell * t.W) / 2;
        for (int cy = 0; cy < t.H; cy++)
            for (int cx = 0; cx < t.W; cx++)
            {
                var p = new Vector2Int(cx, cy);
                char c = t.At(p);
                if (c == '#') continue;
                var tile = Ui.Box(parent, "cell", c == 'S' ? Theme.Aqua : c == 'F' ? Theme.Ink : c == 'O' ? Theme.Hex("#3A4A66") : Theme.Hex("#5C6270"), false);
                Ui.Place(tile.rectTransform, ox + cx * cell + 1, y + cy * cell + 1, cell - 2, cell - 2);
                tile.raycastTarget = false;
            }
    }

    class Runner : MonoBehaviour { }
}
