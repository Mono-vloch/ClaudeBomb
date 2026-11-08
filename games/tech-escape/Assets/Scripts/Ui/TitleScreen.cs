using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The cover: the facility at night from the corridor camera, the door lights on, the attacker
// pacing; beside it the wordmark, the pitch, LUMA's one line, one big Start and a small Levels.
// Levels opens the facility map: the twelve rooms as cards, each with its levels and the stars
// they earned. The one ladder in the game.
public class TitleScreen
{
    public readonly RectTransform root;
    RectTransform map, content;

    public TitleScreen(Transform parent, App app)
    {
        root = Ui.Node("Title", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        content = Ui.Node("Content", root);
        Ui.Stretch(content);
        Ui.Group(content);

        var frame = Ui.Node("Frame", content);
        Ui.Place(frame, 1000, 40, 920, 960);
        app.world.Build();
        app.world.ShowOrbit(frame);
        app.world.Demo(true);
        app.atmosphere.MenuBlur(false);
        Sfx.Ambient(true);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);

        var shade = Ui.Box(content, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, -800, -300, 1720, 1700);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", content).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip; fade.color = Theme.Ink; fade.raycastTarget = false;
        Ui.Place(fade.rectTransform, 920, -300, 440, 1700);
        var feedArea = Ui.Node("Feed", content);
        Ui.Place(feedArea, 1000, 0, 920, 1080);
        CameraFeed.Build(feedArea, "CAM 00  /  THE FACILITY", "LOCKDOWN", 96);

        Ui.Wordmark(content, 120, 64, 250);
        GameMenu.Icon(content);

        var eyebrow = Ui.Eyebrow(content, "TECH ESCAPE  ·  GAME 04", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 170, 600, 22);
        var title = Ui.Text(content, "Nothing Gets Through", 80, Color.white, true);
        Ui.Place(title.rectTransform, 114, 196, 1300, 130);
        title.enableWordWrapping = false;
        var tag = Ui.Text(content, "Fix the room. Then the facility runs the attack.", 36, Theme.Teal, true);
        Ui.Place(tag.rectTransform, 120, 336, 900, 90);
        int done = app.LevelsDone, next = Mathf.Min(done + 1, Rooms.Count);
        if (done == 0) app.luma.Cue(content, "title", 120, 436, 820, stay: true);
        else app.luma.Text(content, $"Welcome back. Next: {Rooms.Get(next).title.Replace("Capstone: ", "").ToLowerInvariant()}", 120, 436, 820, stay: true);

        var start = Ui.Button(content, done == 0 ? "Start" : $"Continue: level {Rooms.Get(next).id}", () => app.StartLevel(next), Ui.ButtonStyle.Accent, 24);
        Ui.Place(start.GetComponent<RectTransform>(), 120, 580, 360, 72);
        var levels = Ui.Button(content, "Levels", () => Map(app), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(levels.GetComponent<RectTransform>(), 500, 592, 120, 48);
    }

    void CloseMap()
    {
        if (map != null) Object.Destroy(map.gameObject);
        map = null;
        if (content != null) Ui.Group(content).alpha = 1f;
    }

    // The facility map: a card per room in the corridor's order, its levels as steps with their
    // stars; tap a step to go there.
    void Map(App app)
    {
        if (map != null) { CloseMap(); return; }
        Ui.Group(content).alpha = 0f;
        map = Ui.Node("Map", root);
        Ui.Stretch(map);
        var dim = Ui.Box(map, "Dim", Theme.Ink, false); // opaque: the title must not show through the cards
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(CloseMap);
        var head = Ui.Eyebrow(map, "THE FACILITY  ·  TWELVE ROOMS", Theme.Teal); head.font = Theme.Bold;
        Ui.Place(head.rectTransform, 120, 118, 600, 22);
        var close = Ui.Button(map, "Close", CloseMap, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), Ui.W - 120 - 100, 108, 100, 40);
        int done = app.LevelsDone, next = Mathf.Min(done + 1, Rooms.Count);
        const float cw = 268f, ch = 300f, gap = 16f;
        for (int r = 0; r < Facility.Rooms.Length; r++)
        {
            string name = Facility.Rooms[r];
            var levels = Rooms.All.Where(l => l.room == name).ToList();
            int col = r % 6, row = r / 6;
            float x = 120 + col * (cw + gap), y = 160 + row * (ch + gap);
            bool here = levels.Any(l => l.number == next);
            var record = app.session.record.FindAll(l => l.room == name);
            bool held = record.Count > 0 && record.TrueForAll(l => l.gap == "held"), breached = record.Count > 0 && !held;
            var card = Ui.Card(map, name, new Color(0.08f, 0.14f, 0.27f, 1f), here ? Theme.Teal : breached ? Look.Breached : held ? Theme.TealDeep : Ui.Hairline, here ? 2f : 1f);
            Ui.Place(card.rectTransform, x, y, cw, ch);
            var t = card.transform;
            var lamp = Ui.Dot(t, held ? Theme.Aqua : breached ? Look.Breached : here ? Theme.Amber : new Color(1f, 1f, 1f, 0.2f), 12);
            Ui.Place(lamp.rectTransform, 18, 22, 12, 12); lamp.raycastTarget = false;
            var n = Ui.Text(t, name, 18, Color.white, true);
            Ui.Place(n.rectTransform, 40, 14, cw - 56, 52);
            float ly = 76;
            foreach (var l in levels)
            {
                bool isNext = l.number == next, isDone = l.number <= done;
                var step = Ui.Button(t, l.id, () => app.StartLevel(l.number), Ui.ButtonStyle.GhostOnDark, 16);
                Ui.Place(step.GetComponent<RectTransform>(), 18, ly, 72, 40);
                if (isNext || isDone) Ui.Chip(step, true);
                int stars = app.StarsOf(l.number);
                for (int s = 0; s < 3; s++)
                {
                    var star = Ui.Dot(t, s < stars ? Theme.Amber : new Color(1, 1, 1, 0.18f), 12);
                    Ui.Place(star.rectTransform, 104 + s * 18, ly + 14, 12, 12);
                    star.raycastTarget = false;
                }
                ly += 50;
            }
        }
    }
}
