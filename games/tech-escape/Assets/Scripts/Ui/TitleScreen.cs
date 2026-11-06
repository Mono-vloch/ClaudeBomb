using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The cover: the facility at night, the door lights on, the attacker pacing the
// corridor; over it the wordmark, the pitch, LUMA, the campaign (three tiers of
// rooms with the stars each earned) and the way in.
public class TitleScreen
{
    public readonly RectTransform root;

    public TitleScreen(Transform parent, App app)
    {
        root = Ui.Node("Title", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 1000, 40, 920, 960);
        app.world.Build();
        app.world.ShowOrbit(frame);
        app.world.Demo(true);
        app.atmosphere.MenuBlur(false);
        Sfx.Ambient(true);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);

        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, -800, -300, 1720, 1700);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip; fade.color = Theme.Ink; fade.raycastTarget = false;
        Ui.Place(fade.rectTransform, 920, -300, 440, 1700);

        Ui.Wordmark(root, 120, 64, 250);
        if (Platform.IsWeb)
        {
            var menu = Ui.Button(root, "Games menu", Platform.GoToMenu, Ui.ButtonStyle.GhostOnDark, 18);
            Ui.Place(menu.GetComponent<RectTransform>(), 1600, 58, 200, 52);
        }
        Button sound = null;
        sound = Ui.Button(root, Sfx.Enabled ? "Sound on" : "Sound off", () => { Sfx.Enabled = !Sfx.Enabled; Ui.SetLabel(sound, Sfx.Enabled ? "Sound on" : "Sound off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(sound.GetComponent<RectTransform>(), 1460, 58, 124, 52);
        Button voice = null;
        voice = Ui.Button(root, Luma.Voice ? "Voice on" : "Voice off", () => { Luma.Voice = !Luma.Voice; if (!Luma.Voice) app.luma.Hush(); Ui.SetLabel(voice, Luma.Voice ? "Voice on" : "Voice off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(voice.GetComponent<RectTransform>(), 1316, 58, 128, 52);

        var eyebrow = Ui.Eyebrow(root, "TECH ESCAPE  ·  GAME 04", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 170, 600, 22);
        var title = Ui.Text(root, "Nothing Gets Through", 92, Color.white, true);
        Ui.Place(title.rectTransform, 114, 196, 1300, 130);
        title.enableWordWrapping = false;
        var tag = Ui.Text(root, "Fix what is misconfigured. Then the facility runs the attack.", 36, Theme.Teal, true);
        Ui.Place(tag.rectTransform, 120, 336, 900, 90);
        int done = app.LevelsDone, next = Mathf.Min(done + 1, Rooms.Count);
        app.luma.Line(root, 120, 436, 820,
            done == 0 ? "I'm LUMA, your coach. Nineteen levels in twelve rooms. Press Start: the first door is the Risk Room."
                      : $"Welcome back. {done} of {Rooms.Count} levels done, {app.TotalStars} stars: {app.Rank}. Next: {Rooms.Get(next).title.ToLowerInvariant()}",
            true, true, 17);

        float y = 560;
        for (int tier = 1; tier <= 3; tier++)
        {
            var label = Ui.Eyebrow(root, $"TIER {tier}  ·  {Rooms.TierNames[tier - 1].ToUpperInvariant()}", Theme.Faint);
            Ui.Place(label.rectTransform, 120, y, 700, 18);
            var levels = System.Array.FindAll(Rooms.All, l => l.tier == tier);
            float x0 = 150, step = levels.Length > 6 ? 76 : 112, ty = y + 42;
            var track = Ui.Box(root, "Track", new Color(1, 1, 1, 0.22f), false);
            Ui.Place(track.rectTransform, x0, ty - 1, step * (levels.Length - 1), 2);
            track.raycastTarget = false;
            for (int i = 0; i < levels.Length; i++)
            {
                var l = levels[i];
                bool isNext = l.number == next, isDone = l.number <= done;
                float x = x0 + i * step;
                if (isNext) { var halo = Ui.Box(root, "Halo", new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.3f)); Ui.Place(halo.rectTransform, x - 23, ty - 23, 46, 46); halo.raycastTarget = false; }
                var rim = Ui.Box(root, "Rim", isDone || isNext ? Theme.Teal : new Color(1, 1, 1, 0.45f));
                Ui.Place(rim.rectTransform, x - 17, ty - 17, 34, 34);
                var stop = Ui.Box(root, "Stop", isDone || isNext ? Theme.Teal : Theme.Ink);
                Ui.Place(stop.rectTransform, x - 15, ty - 15, 30, 30);
                var num = Ui.Text(stop.transform, l.id, 10, isDone || isNext ? Theme.Ink : Theme.Faint, true, TextAlignmentOptions.Center);
                Ui.Stretch(num.rectTransform);
                int stars = app.StarsOf(l.number);
                for (int s = 0; s < 3; s++) { var star = Ui.Box(root, "Star", s < stars ? Theme.Teal : new Color(1, 1, 1, 0.18f), false); Ui.Place(star.rectTransform, x - 11 + s * 9, ty + 22, 6, 6); star.raycastTarget = false; }
                var hit = Ui.Box(root, "Level " + l.id, Color.clear, false);
                Ui.Place(hit.rectTransform, x - step / 2, ty - 26, step, 60);
                var button = hit.gameObject.AddComponent<Button>();
                button.targetGraphic = stop;
                int number = l.number;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
            y += 90;
        }

        var start = Ui.Button(root, done == 0 ? "Start" : $"Continue: level {Rooms.Get(next).id}", () => app.StartLevel(next), Ui.ButtonStyle.Accent, 22);
        Ui.Place(start.GetComponent<RectTransform>(), 120, 860, 300, 56);
    }
}
