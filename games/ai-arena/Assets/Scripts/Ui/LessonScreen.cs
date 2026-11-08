using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing is a transmission, as Claude Bomb's: the family's house at dusk in the home
// camera's feed on the left, the agent at the door; on the right the phone message arriving,
// typed in, two short lines under it, and one button. No chips, no cards, no ladder.
// Everything the course says beyond that is in the file, one icon away.
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

        // The world: the house at dusk, the agent at the door, in the home camera's feed.
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 40, 60, 1100, 960);
        var arena = new Arena(app.brief, app.configs.ConvertAll(c => c.Clone()), app.budget, l.number * 17 + 3);
        var hero = arena.agents[0];
        var inward = Market.Inward(hero.home);
        var step = new Vector2Int(Mathf.RoundToInt(inward.x), -Mathf.RoundToInt(inward.z));
        var side = new Vector2Int(step.y, step.x);
        for (int d = 0; d <= 3; d++) for (int s = -2; s <= 2; s++) { var p = hero.home + step * d + side * s; if (Arena.InBounds(p)) arena.RemoveItem(p); }
        app.world.Build(arena, true);
        app.world.PlaceAtDoor(hero, false, 0.45f);
        app.world.DoorLamp(hero);
        app.world.ShowHouse(frame, hero, 5.0f, 38f);
        CameraFeed.Build(frame, "HOME CAM  /  THE DOOR", "THE FAMILY'S HOUSE  /  DINNER TIME");
        app.atmosphere.MenuBlur(false);

        // The transmission, right, on the ink.
        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, PanelX - 80, -40, 1000, 1200);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip; fade.color = Theme.Ink; fade.raycastTarget = false;
        fade.rectTransform.localScale = new Vector3(-1, 1, 1);
        Ui.Place(fade.rectTransform, PanelX - 80, -40, 200, 1200);

        var lines = Transmission(l, app.brief);
        var dot = Ui.Dot(root, Theme.Teal, 12);
        Ui.Place(dot.rectTransform, PanelX, 318, 12, 12);
        var head = Ui.Text(root, l.decide ? Decide.Tasks[0].from : app.brief.from, 30, Color.white, true);
        Ui.Place(head.rectTransform, PanelX + 24, 300, PanelW - 140, 44);
        var sub = Ui.Eyebrow(root, "6:00 PM", Theme.Faint);
        Ui.Place(sub.rectTransform, PanelX + 24, 348, PanelW - 140, 20);
        var skip = Ui.Button(root, "Skip", () => hurry = true, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(skip.GetComponent<RectTransform>(), PanelX + PanelW - 100, 300, 100, 40);
        GameMenu.Icon(root);

        var texts = new List<TextMeshProUGUI>();
        float y = 400;
        for (int i = 0; i < lines.Count; i++)
        {
            bool message = i == 0, last = i == lines.Count - 1;
            var t = Ui.Text(root, "", message ? 26 : 24, last ? Theme.Teal : Color.white, message || last);
            float h = t.GetPreferredValues(lines[i], PanelW, 0).y + 8;
            Ui.Place(t.rectTransform, PanelX, y, PanelW, h);
            texts.Add(t);
            y += h + (message ? 26 : 14);
        }
        var go = Ui.Button(root, l.decide ? "Decide" : "Pack the agent", app.ShowSetup, Ui.ButtonStyle.Accent, 22);
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

    // The message first, verbatim, then two short lines of story: the level's kicker cut to
    // sentences, or LUMA's first sentence where the kicker is one sentence.
    public static List<string> Transmission(Lesson l, Brief brief)
    {
        var lines = new List<string> { l.decide ? Decide.Tasks[0].message : brief.rule };
        switch (l.number)
        {
            case 1: lines.Add("Your agent shops alone. You write its list."); lines.Add("Once it leaves, nobody can help it."); break;
            case 2: lines.Add("A vague list does not come home with a bad basket."); lines.Add("It comes home with a tomato in the fruit salad."); break;
            case 3: lines.Add("The agent only knows the stalls in front of it."); lines.Add("Now you choose how far it sees."); break;
            case 4: lines.Add("Every turn starts from nothing, unless you give it a memory."); lines.Add("Now you choose what it remembers."); break;
            case 5: lines.Add("A bigger brain is not always a better buy."); lines.Add("It is always pricier."); break;
            case 6: lines.Add("Five messages tonight. Not every one needs an agent."); lines.Add("A fixed rule costs nothing. It cannot decide anything."); break;
            case 7: lines.Add("Everything is open, and there is no help this time."); lines.Add("Every choice costs tokens, every turn."); break;
            default:
                foreach (var s in Sentences(l.kicker)) if (lines.Count < 3) lines.Add(s);
                break;
        }
        return lines;
    }

    static IEnumerable<string> Sentences(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        foreach (var p in System.Text.RegularExpressions.Regex.Split(text.Trim(), @"(?<=[.!?])\s+"))
            if (p.Trim().Length > 0) yield return p.Trim();
    }

    class Runner : MonoBehaviour { }
}
