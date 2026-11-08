using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The security camera's frame over a room, as Claude Bomb frames Bay 3 (CAM 02): four
// corner brackets, a blinking red REC dot, the camera and the room, a running clock,
// and the site underneath. Text at 16 px or more, the way the realism pass reads it.
public class CameraFeed : MonoBehaviour
{
    Image rec;
    TextMeshProUGUI clock;
    static float origin = -1f; // one clock for the whole night, whatever the screen
    const int Start = 23 * 3600 + 47 * 60; // the clock starts at 23:47:00, as in Bay 3

    public static CameraFeed Build(RectTransform area, string camera, string site, float top = 48f, bool bottom = true)
    {
        var root = Ui.Node("CameraFeed", area);
        Ui.Stretch(root, 48, top, 48, 48);
        var feed = root.gameObject.AddComponent<CameraFeed>();
        var line = new Color(Theme.Faint.r, Theme.Faint.g, Theme.Faint.b, 0.55f);
        foreach (var corner in bottom ? new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one } : new[] { Vector2.up, Vector2.one })
            foreach (var size in new[] { new Vector2(48f, 2f), new Vector2(2f, 48f) })
            {
                var stroke = Ui.Box(root, "Corner", line, false);
                stroke.raycastTarget = false;
                var rt = stroke.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = corner;
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = size;
            }
        feed.rec = Ui.Dot(root, Theme.Danger, 16);
        var dot = feed.rec.rectTransform;
        dot.anchorMin = dot.anchorMax = dot.pivot = new Vector2(0f, 1f);
        dot.anchoredPosition = new Vector2(22f, -24f);
        var label = Ui.Text(root, "REC    " + camera, 16, Color.white, true);
        label.characterSpacing = 4;
        Anchor(label.rectTransform, new Vector2(0f, 1f), new Vector2(50f, -20f), new Vector2(700f, 24f));
        feed.clock = Ui.Text(root, "", 22, Color.white, true, TextAlignmentOptions.TopRight);
        Anchor(feed.clock.rectTransform, new Vector2(1f, 1f), new Vector2(-52f, -18f), new Vector2(240f, 28f));
        if (!string.IsNullOrEmpty(site))
        {
            var where = Ui.Text(root, site, 16, Theme.Faint, true, TextAlignmentOptions.BottomRight);
            where.characterSpacing = 4;
            Anchor(where.rectTransform, new Vector2(1f, 0f), new Vector2(-22f, 20f), new Vector2(700f, 24f));
        }
        return feed;
    }

    static void Anchor(RectTransform rt, Vector2 corner, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = corner;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    void Update()
    {
        if (origin < 0f) origin = Time.unscaledTime;
        float since = Time.unscaledTime - origin;
        int s = Start + Mathf.FloorToInt(since);
        if (clock != null) clock.text = $"<mspace=0.62em>{s / 3600 % 24:00}:{s / 60 % 60:00}:{s % 60:00}</mspace>";
        if (rec != null) rec.color = new Color(Theme.Danger.r, Theme.Danger.g, Theme.Danger.b, Mathf.Repeat(since, 1.2f) < 0.7f ? 1f : 0.15f);
    }
}
