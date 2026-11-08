using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing is a transmission, as Claude Bomb's: the lockdown corridor in the ceiling
// camera's feed on the left, this level's door lit amber; on the right the night shift's
// three short lines typed in, and one button. No chips, no cards, no ladder. Everything the
// course says beyond that is in the file, one icon away.
public class LessonScreen
{
    public readonly RectTransform root;
    const float PanelX = 1240f, PanelW = 640f;
    bool hurry;

    public LessonScreen(Transform parent, App app)
    {
        var l = app.room;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 40, 60, 1100, 960);
        app.world.Build();
        int here = Facility.RoomIndex(l.room);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == here ? Building.DoorState.Current : i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowDoor(frame, here);
        app.atmosphere.MenuBlur(false);
        Sfx.Ambient(true);
        CameraFeed.Build(frame, "CAM 00  /  CORRIDOR", "LOCKDOWN");

        // The transmission, right, on the ink.
        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, PanelX - 80, -40, 1000, 1200);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip; fade.color = Theme.Ink; fade.raycastTarget = false;
        fade.rectTransform.localScale = new Vector3(-1, 1, 1);
        Ui.Place(fade.rectTransform, PanelX - 80, -40, 200, 1200);

        var (header, sub, lines, button) = Script.Transmission(l);
        var dot = Ui.Dot(root, Theme.Teal, 12);
        Ui.Place(dot.rectTransform, PanelX, 318, 12, 12);
        var head = Ui.Text(root, header, 30, Color.white, true);
        Ui.Place(head.rectTransform, PanelX + 24, 300, PanelW - 140, 44);
        var subText = Ui.Eyebrow(root, sub.ToUpperInvariant(), Theme.Faint);
        Ui.Place(subText.rectTransform, PanelX + 24, 348, PanelW - 140, 20);
        var skip = Ui.Button(root, "Skip", () => hurry = true, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(skip.GetComponent<RectTransform>(), PanelX + PanelW - 100, 300, 100, 40);
        GameMenu.Icon(root);

        var texts = new List<TextMeshProUGUI>();
        float y = 400;
        for (int i = 0; i < lines.Count; i++)
        {
            bool last = i == lines.Count - 1;
            var t = Ui.Text(root, "", 24, last ? Theme.Teal : Color.white, last);
            float h = t.GetPreferredValues(lines[i], PanelW, 0).y + 8;
            Ui.Place(t.rectTransform, PanelX, y, PanelW, h);
            texts.Add(t);
            y += h + 14;
        }
        var go = Ui.Button(root, button, app.ShowRoom, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), PanelX, y + 24, Mathf.Max(300f, 40f + 13f * button.Length), 64);
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

    class Runner : MonoBehaviour { }
}
