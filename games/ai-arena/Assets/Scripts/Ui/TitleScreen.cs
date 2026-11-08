using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The cover: the player's agent close in the evening square, the others strolling behind it;
// on the left the wordmark, the pitch, LUMA's one line, the errand map (the seven stops and
// their stars: the only ladder in the game) and Start. The agent's name and colour are set
// here, once: tap the agent to paint it, tap its name tag to rename it. Sound, the voice, free
// play, the story and the games menu live in the menu. A small amber lamp by the menu shows
// only when Claude is not connected, and opens the reason.
public class TitleScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly RectTransform frame;
    RectTransform tag;
    TMP_InputField rename;
    Image offline;
    RectTransform reason;

    public TitleScreen(Transform parent, App app)
    {
        this.app = app;
        root = Ui.Node("Title", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear); // the market shows through

        // The market behind, on the right: the player's agent in front.
        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 1020, 40, 900, 960);
        app.world.ShowCover(frame, hero: true, heroConfig: app.configs[0]);

        // Ink over the left, fading into the picture.
        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, -800, -300, 1720, 1700);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip;
        fade.color = Theme.Ink;
        fade.raycastTarget = false;
        Ui.Place(fade.rectTransform, 920, -300, 440, 1700);

        // A tap on the agent paints it the next colour.
        var hit = Ui.Box(root, "Paint", Color.clear, false);
        Ui.Place(hit.rectTransform, 1020, 40, 900, 960);
        hit.raycastTarget = true;
        var paint = hit.gameObject.AddComponent<Button>();
        paint.transition = Selectable.Transition.None;
        paint.onClick.AddListener(Paint);

        Ui.Wordmark(root, 120, 64, 250);
        GameMenu.Icon(root);
        var eyebrow = Ui.Eyebrow(root, "AI ARENA  ·  GAME 02", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 170, 600, 22);
        var title = Ui.Text(root, "Send the Agent", 116, Color.white, true);
        Ui.Place(title.rectTransform, 114, 196, 1100, 140);
        var tagline = Ui.Text(root, "You write the list. It shops alone.", 40, Theme.Teal, true);
        Ui.Place(tagline.rectTransform, 120, 350, 1000, 56);
        int done = app.LevelsDone, next = Mathf.Min(done + 1, Ladder.Count);
        app.luma.Cue(root, "title", 120, 436, 820, stay: true);

        // The errand map: the seven stops, their stars; any of them starts from a tap.
        const float first = 150, step = 112, trackY = 620;
        var track = Ui.Box(root, "Track", new Color(1, 1, 1, 0.22f), false);
        Ui.Place(track.rectTransform, first, trackY - 1, step * (Ladder.Count - 1), 2);
        track.raycastTarget = false;
        for (int i = 1; i <= Ladder.Count; i++)
        {
            bool isNext = i == next, isDone = i <= done;
            float x = first + (i - 1) * step;
            if (isNext)
            {
                var halo = Ui.Box(root, "Halo", new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.3f));
                Ui.Place(halo.rectTransform, x - 32, trackY - 32, 64, 64);
                halo.raycastTarget = false;
            }
            var rim = Ui.Box(root, "Rim", isDone || isNext ? Theme.Teal : new Color(1, 1, 1, 0.45f));
            Ui.Place(rim.rectTransform, x - 24, trackY - 24, 48, 48);
            var stop = Ui.Box(root, "Stop", isDone || isNext ? Theme.Teal : Theme.Ink);
            Ui.Place(stop.rectTransform, x - 22, trackY - 22, 44, 44);
            var num = Ui.Text(stop.transform, i.ToString(), 18, isDone || isNext ? Theme.Ink : Theme.Faint, true, TextAlignmentOptions.Center);
            Ui.Stretch(num.rectTransform);
            int stars = app.StarsOf(i);
            for (int s = 0; s < 3; s++)
            {
                var star = Ui.Box(root, "Star", s < stars ? Theme.Amber : new Color(1, 1, 1, 0.18f));
                Ui.Place(star.rectTransform, x - 16 + s * 12, trackY + 34, 8, 8);
                star.raycastTarget = false;
            }
            var hitStop = Ui.Box(root, "Errand " + i, Color.clear, false);
            Ui.Place(hitStop.rectTransform, x - 56, trackY - 34, 112, 90);
            var button = hitStop.gameObject.AddComponent<Button>();
            button.targetGraphic = stop;
            int number = i;
            button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
        }

        var start = Ui.Button(root, done == 0 ? "Start" : "Continue", () => app.Begin(next), Ui.ButtonStyle.Accent, 24);
        Ui.Place(start.GetComponent<RectTransform>(), 120, 740, 360, 72);

        // Claude: an amber lamp by the menu only when it is not connected; tap for the reason.
        offline = Ui.Dot(root, Theme.Amber, 18);
        Ui.Place(offline.rectTransform, Ui.W - 40 - 52 - 22 - 18, 41, 18, 18);
        offline.gameObject.SetActive(false);
        var ob = offline.gameObject.AddComponent<Button>();
        ob.targetGraphic = offline;
        ob.onClick.AddListener(ShowReason);
        void ShowStatus()
        {
            if (offline == null) return; // the screen may be gone by the time the probe answers
            offline.gameObject.SetActive(ClaudeAccess.Mode == ClaudeMode.Offline);
        }
        ShowStatus();
        app.StartCoroutine(ClaudeAccess.Probe(ShowStatus));

        // The agent's name tag, at its feet in the world; tap to rename.
        tag = Ui.Node("Tag", root);
        BuildTag();
        FramedFollower.On(app, root, PlaceTag);
    }

    void BuildTag()
    {
        Ui.Clear(tag);
        var c = app.configs[0];
        var glass = Ui.Glass(tag, "Name", 0, 0, 24 + Mathf.Max(90f, 12f * c.name.Length), 40, false);
        glass.raycastTarget = true;
        var t = Ui.Text(glass.transform, c.name, 18, Color.white, true, TextAlignmentOptions.Center);
        Ui.Stretch(t.rectTransform, 8, 0, 8, 0);
        t.enableWordWrapping = false;
        var b = glass.gameObject.AddComponent<Button>();
        b.targetGraphic = glass;
        b.onClick.AddListener(Rename);
        tag.sizeDelta = glass.rectTransform.sizeDelta;
    }

    void PlaceTag()
    {
        var hero = app.world.CoverHero;
        if (tag == null || hero == null) return;
        bool on = app.world.OnScreen(hero, out var feet, out _);
        tag.gameObject.SetActive(on && reason == null);
        if (!on) return;
        var p = app.StagePoint(feet);
        float w = tag.sizeDelta.x;
        Ui.Place(tag, Mathf.Clamp(p.x - w / 2, 1030, Ui.W - w - 40), Mathf.Clamp(p.y + 10, 60, Ui.H - 60), w, tag.sizeDelta.y);
    }

    void Rename()
    {
        if (rename != null) return;
        var c = app.configs[0];
        Ui.Clear(tag);
        rename = Ui.DarkInput(tag, "Its name", false, 18);
        Ui.Place(rename.GetComponent<RectTransform>(), 0, 0, 240, 44);
        tag.sizeDelta = new Vector2(240, 44);
        rename.text = c.name;
        rename.characterLimit = 16;
        rename.onEndEdit.AddListener(v =>
        {
            if (!string.IsNullOrWhiteSpace(v)) c.name = v.Trim();
            app.SaveConfigs();
            var hero = app.world.CoverHero;
            if (hero != null) { hero.config.name = c.name; }
            rename = null;
            BuildTag();
            PlaceTag();
        });
        rename.ActivateInputField();
        rename.Select();
    }

    // The next colour of the ten: the agent is built again in it.
    void Paint()
    {
        var hero = app.world.CoverHero;
        if (hero == null) return;
        if (!app.world.OnScreen(hero, out var feet, out var head)) return;
        var mouse = Input.mousePosition;
        float cx = (feet.x + head.x) / 2f, cy = (feet.y + head.y) / 2f;
        if (Mathf.Abs(mouse.x - cx) > Mathf.Abs(head.y - feet.y) * 0.6f + 40f || mouse.y < feet.y - 30f || mouse.y > head.y + 30f) return;
        var c = app.configs[0];
        c.colour = (c.colour + 1) % Theme.Team.Length;
        hero.config.colour = c.colour;
        app.SaveConfigs();
        app.world.RefreshRobot(hero);
        app.world.Punch(hero);
        Sfx.Tick();
    }

    void ShowReason()
    {
        if (reason != null) { Object.Destroy(reason.gameObject); reason = null; return; }
        reason = Ui.Node("Reason", root);
        Ui.Stretch(reason);
        var dim = Ui.Box(reason, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.5f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(() => { Object.Destroy(reason.gameObject); reason = null; });
        var card = Ui.Box(reason, "Card", Ui.DarkFill);
        Ui.Place(card.rectTransform, Ui.W - 40 - 620, 90, 620, 190);
        var head = Ui.Text(card.transform, ClaudeAccess.Describe(), 20, Theme.Amber, true);
        Ui.Place(head.rectTransform, 28, 24, 564, 56);
        var hint = Ui.Text(card.transform, ClaudeAccess.Hint, 16, Color.white);
        Ui.Place(hint.rectTransform, 28, 84, 564, 90);
    }
}
