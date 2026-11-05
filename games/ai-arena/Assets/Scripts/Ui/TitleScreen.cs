using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The cover: the market at dusk with four robots strolling about it, the
// camera turning slowly; over it, on the left, the wordmark, the pitch in one
// line, LUMA, and the way in. The story plays the first time, then the ladder
// continues from wherever this machine left off. Every errand can also be
// started straight from here: for a class on one idea, or to test one.
public class TitleScreen
{
    public readonly RectTransform root;

    public TitleScreen(Transform parent, App app)
    {
        root = Ui.Node("Title", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear); // the market shows through

        // The market behind, on the right.
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 1020, 40, 900, 960);
        app.world.ShowCover(frame);

        // Ink over the left, fading into the picture.
        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, -800, -300, 1720, 1700);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip;
        fade.color = Theme.Ink;
        fade.raycastTarget = false;
        Ui.Place(fade.rectTransform, 920, -300, 440, 1700);

        Ui.Wordmark(root, 120, 64, 250);
        if (Platform.IsWeb)
        {
            // On the platform's site: out of this game, back to the menu of games.
            var menu = Ui.Button(root, "Games menu", Platform.GoToMenu, Ui.ButtonStyle.GhostOnDark, 18);
            Ui.Place(menu.GetComponent<RectTransform>(), 1600, 58, 200, 52);
        }

        var eyebrow = Ui.Eyebrow(root, "AI ARENA  ·  GAME 02", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 170, 600, 22);
        var title = Ui.Text(root, "Send the Agent", 116, Color.white, true);
        Ui.Place(title.rectTransform, 114, 196, 1100, 140);
        var tag = Ui.Text(root, "You write the list. It shops alone.", 40, Theme.Teal, true);
        Ui.Place(tag.rectTransform, 120, 350, 1000, 56);
        var blurb = Ui.Text(root,
            "Every evening the family texts what dinner needs. You turn it into a shopping list for your agent. " +
            "Then it leaves, and nobody can help it. Can it come home with the right dinner, and money to spare?",
            22, Theme.Faint);
        Ui.Place(blurb.rectTransform, 120, 424, 820, 120);

        int done = app.LevelsDone, next = Mathf.Min(done + 1, Ladder.Count);
        Luma.Card(root, 120, 582, 820, 92,
            done == 0 ? "I'm LUMA, your coach. Seven errands, one idea each. Press Start and I will tell you the story first."
                      : $"Welcome back. {done} of {Ladder.Count} errands done, {app.TotalStars} stars: {app.Rank}. Next: {Ladder.Get(next).title.ToLowerInvariant()}",
            16, null, null, true);

        // Connection
        var status = Ui.Text(root, "", 18, Theme.Faint);
        Ui.Place(status.rectTransform, 150, 700, 900, 30);
        var dot = Ui.Dot(root, Theme.Teal, 12);
        Ui.Place(dot.rectTransform, 124, 708, 12, 12);
        var hint = Ui.Text(root, "", 15, Theme.Faint);
        Ui.Place(hint.rectTransform, 150, 732, 820, 50);
        void ShowStatus()
        {
            if (status == null) return; // the screen may be gone by the time the probe answers
            bool offline = ClaudeAccess.Mode == ClaudeMode.Offline;
            status.text = ClaudeAccess.Describe();
            dot.color = offline ? Theme.Amber : Theme.Teal;
            hint.text = offline ? ClaudeAccess.Hint : "";
        }
        ShowStatus();
        app.StartCoroutine(ClaudeAccess.Probe(ShowStatus));

        // The six errands as stops on a track: the ones done are filled, the next one
        // is lit, and any of them starts from a click.
        var jump = Ui.Eyebrow(root, "THE SEVEN ERRANDS  ·  START FROM ANY", Theme.Faint);
        Ui.Place(jump.rectTransform, 120, 786, 500, 22);
        const float first = 150, step = 132, trackY = 840;
        var track = Ui.Box(root, "Track", new Color(1, 1, 1, 0.22f), false);
        Ui.Place(track.rectTransform, first, trackY - 1, step * (Ladder.Count - 1), 2);
        track.raycastTarget = false;
        for (int i = 1; i <= Ladder.Count; i++)
        {
            var l = Ladder.Get(i);
            bool isNext = i == next, isDone = i <= done;
            float x = first + (i - 1) * step;
            if (isNext)
            {
                var halo = Ui.Dot(root, new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.3f), 64);
                Ui.Place(halo.rectTransform, x - 32, trackY - 32, 64, 64);
                halo.raycastTarget = false;
            }
            var rim = Ui.Dot(root, isDone || isNext ? Theme.Teal : new Color(1, 1, 1, 0.45f), 44);
            Ui.Place(rim.rectTransform, x - 22, trackY - 22, 44, 44);
            var stop = Ui.Dot(root, isDone || isNext ? Theme.Teal : Theme.Ink, 40);
            Ui.Place(stop.rectTransform, x - 20, trackY - 20, 40, 40);
            var num = Ui.Text(stop.transform, i.ToString(), 18, isDone || isNext ? Theme.Ink : Theme.Faint, true, TextAlignmentOptions.Center);
            Ui.Stretch(num.rectTransform);
            var name = Ui.Text(root, l.title, 12, isNext ? Color.white : isDone ? Theme.Teal : Theme.Faint, isNext, TextAlignmentOptions.Top);
            Ui.Place(name.rectTransform, x - 64, trackY + 30, 128, 44);
            // The stars this errand earned, under its name.
            int stars = app.StarsOf(i);
            for (int s = 0; s < 3; s++)
            {
                var star = Ui.Dot(root, s < stars ? Theme.Teal : new Color(1, 1, 1, 0.18f), 8);
                Ui.Place(star.rectTransform, x - 16 + s * 12, trackY + 78, 8, 8);
                star.raycastTarget = false;
            }
            // The whole stop, number and name, takes the click.
            var hit = Ui.Box(root, "Errand " + i, Color.clear, false);
            Ui.Place(hit.rectTransform, x - 64, trackY - 34, 128, 124);
            var button = hit.gameObject.AddComponent<Button>();
            button.targetGraphic = stop;
            int number = i;
            button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
        }

        var start = Ui.Button(root, done == 0 ? "Start" : $"Continue: errand {Ladder.Get(next).id}", () => app.Begin(next), Ui.ButtonStyle.Accent, 22);
        Ui.Place(start.GetComponent<RectTransform>(), 120, 926, 300, 60);
        var free = Ui.Button(root, "Free play", app.StartFree, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(free.GetComponent<RectTransform>(), 440, 926, 170, 60);
        var story = Ui.Button(root, "The story", () => app.ShowStory(() => app.StartLevel(next)), Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(story.GetComponent<RectTransform>(), 630, 926, 170, 60);

        var foot = Ui.Text(root, "A Maaloomatiia Academy game about working with AI agents.", 15, Theme.Faint);
        Ui.Place(foot.rectTransform, 120, 1016, 1000, 30);
    }
}
