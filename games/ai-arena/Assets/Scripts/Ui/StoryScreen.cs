using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The story, before the first errand: six short beats, each a small animated
// scene with LUMA telling it. Next hurries, Skip ends it, and it moves on by
// itself once LUMA has finished the line.
public class StoryScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly RectTransform scene;
    readonly TextMeshProUGUI caption, lumaText;
    readonly List<Image> dots = new List<Image>();
    bool next, skip;
    Coroutine main, playing, typing;
    public int Current { get; private set; } // the beat on screen, 1 to 6

    // Stops everything: called by the app when another screen takes over.
    public void Stop()
    {
        if (main != null) app.StopCoroutine(main);
        if (playing != null) app.StopCoroutine(playing);
        if (typing != null) app.StopCoroutine(typing);
        main = playing = typing = null;
    }

    static readonly string[] Captions =
    {
        "Meet your agent.", "The family texts you.", "You write the list.",
        "It shops alone.", "Some things ruin dinner.", "Six errands.",
    };

    public StoryScreen(Transform parent, App app, Action done)
    {
        this.app = app;
        root = Ui.Node("Story", parent);
        Ui.Stretch(root);
        app.Paper(Theme.Ink);

        Ui.Wordmark(root, 120, 64, 200);
        var eyebrow = Ui.Eyebrow(root, "THE STORY", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 152, 400, 22);
        caption = Ui.Text(root, "", 46, Color.white, true);
        Ui.Place(caption.rectTransform, 118, 178, 1100, 62);
        scene = Ui.Node("Scene", root);
        Ui.Place(scene, 120, 262, 1100, 620);

        lumaText = Luma.Card(root, 1300, 262, 500, 330, "", 20, null, null, true, 170);
        for (int i = 0; i < Captions.Length; i++)
        {
            var d = Ui.Dot(root, Theme.Faint, 10);
            Ui.Place(d.rectTransform, 1300 + i * 22, 616, 10, 10);
            dots.Add(d);
        }
        var skipBtn = Ui.Button(root, "Skip the story", () => skip = true, Ui.ButtonStyle.GhostOnDark, 17);
        Ui.Place(skipBtn.GetComponent<RectTransform>(), 1300, 650, 190, 54);
        var nextBtn = Ui.Button(root, "Next", () => next = true, Ui.ButtonStyle.Accent, 20);
        Ui.Place(nextBtn.GetComponent<RectTransform>(), 1510, 650, 160, 54);
        var hint = Ui.Text(root, "Best with sound on.", 15, Theme.Faint);
        Ui.Place(hint.rectTransform, 1300, 720, 400, 24);

        main = app.StartCoroutine(Run(done));
        app.onLeave = Stop;
    }

    IEnumerator Run(Action done)
    {
        for (int i = 1; i <= Captions.Length && !skip; i++)
        {
            Current = i;
            for (int d = 0; d < dots.Count; d++) dots[d].color = d < i ? Theme.Teal : Theme.Faint;
            Ui.Clear(scene);
            next = false;
            caption.text = Captions[i - 1];
            playing = app.StartCoroutine(Beat(i));
            bool typed = false;
            string line = app.luma.Say("story-" + i);
            typing = app.StartCoroutine(Then(Ui.Type(lumaText, line, 42f, () => next || skip), () => typed = true));
            float after = 0f;
            while (!next && !skip)
            {
                if (typed && !app.luma.Speaking)
                {
                    after += Time.unscaledDeltaTime;
                    if (after > 1.8f) break;
                }
                yield return null;
            }
            app.luma.Hush();
            app.StopCoroutine(playing);
            app.StopCoroutine(typing);
            playing = typing = null;
            lumaText.maxVisibleCharacters = 99999;
        }
        Sfx.Whoosh();
        app.onLeave = null;
        main = null;
        done?.Invoke();
    }

    static IEnumerator Then(IEnumerator routine, Action after) { yield return routine; after(); }

    IEnumerator Beat(int n)
    {
        switch (n)
        {
            case 1: yield return Beat1(); break;
            case 2: yield return Beat2(); break;
            case 3: yield return Beat3(); break;
            case 4: yield return Beat4(); break;
            case 5: yield return Beat5(); break;
            default: yield return Beat6(); break;
        }
    }

    // ---- the pieces ----

    static RectTransform Robot(Transform parent, int colour, float x, float y, float s)
    {
        var bot = Ui.Node("Agent", parent);
        Ui.Place(bot, x, y, s, s);
        var body = Ui.Icon(bot, "bot_body", s); Ui.Stretch(body.rectTransform); body.color = Theme.Team[colour];
        var face = Ui.Icon(bot, "bot_face", s); Ui.Stretch(face.rectTransform);
        return bot;
    }

    static Image House(Transform parent, int colour, float x, float y, float s)
    {
        var house = Ui.Icon(parent, "house", s);
        Ui.Place(house.rectTransform, x, y, s, s);
        house.color = Theme.Team[colour];
        return house;
    }

    static RectTransform Stall(Transform parent, string item, float x, float y, float s)
    {
        var stall = Ui.Icon(parent, "stall", s);
        Ui.Place(stall.rectTransform, x, y, s, s);
        var icon = Ui.Icon(stall.transform, item, s * 0.56f);
        icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        icon.rectTransform.anchoredPosition = new Vector2(0, -s * 0.06f);
        return stall.rectTransform;
    }

    // A small market: cobbles with tiles, cell positions by column and row.
    RectTransform Market(int cols, int rows, float cell, float x0, float y0)
    {
        var mat = Ui.Card(scene, "Market", Theme.Cobbles, Theme.Border, 3);
        Ui.Place(mat.rectTransform, x0, y0, cols * cell + 24, rows * cell + 24);
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
            {
                var tile = Ui.Box(mat.transform, "Tile", Theme.Tile);
                Ui.Place(tile.rectTransform, 12 + c * cell + 3, 12 + r * cell + 3, cell - 6, cell - 6);
            }
        return mat.rectTransform;
    }

    static Vector2 At(int c, int r, float cell, float x0, float y0, float size) => new Vector2(x0 + 12 + c * cell + (cell - size) / 2, y0 + 12 + r * cell + (cell - size) / 2);

    IEnumerator Slide(RectTransform rt, Vector2 to, float seconds)
    {
        var from = rt.anchoredPosition;
        var target = new Vector2(to.x, -to.y);
        yield return Ui.Animate(seconds, k => rt.anchoredPosition = Vector2.Lerp(from, target, k), Ui.EaseOut);
    }

    IEnumerator Say(RectTransform bot, string text, float width = 330)
    {
        var t = Ui.Bubble(scene, "", 19, width, out var bubble);
        var pos = bot.anchoredPosition;
        Ui.Place(bubble, pos.x + 20, -pos.y - bubble.sizeDelta.y - 6, width, bubble.sizeDelta.y);
        var g = Ui.Group(bubble); g.alpha = 0;
        app.StartCoroutine(Ui.Fade(g, 1f, 0.2f));
        yield return Ui.Type(t, text, 55f);
    }

    // 1. The house and the robot, bobbing.
    IEnumerator Beat1()
    {
        var house = House(scene, 0, 150, 180, 300);
        var bot = Robot(scene, 0, 500, 240, 200);
        var g1 = Ui.Group(house); var g2 = Ui.Group(bot); g1.alpha = g2.alpha = 0;
        yield return Ui.Animate(0.5f, k => { g1.alpha = k; house.rectTransform.anchoredPosition = new Vector2(150, -180 - 20 * (1 - k)); });
        app.StartCoroutine(Ui.Fade(g2, 1f, 0.4f));
        var label = Ui.Text(scene, "Walks. Looks. Buys. Does not plan dinner.", 22, Theme.Faint);
        Ui.Place(label.rectTransform, 150, 520, 700, 40);
        float t = 0;
        while (true) { t += Time.unscaledDeltaTime; bot.anchoredPosition = new Vector2(500, -240 + Mathf.Sin(t * 2.5f) * 8f); yield return null; }
    }

    // 2. The family's message slides in.
    IEnumerator Beat2()
    {
        House(scene, 0, 60, 200, 220);
        Robot(scene, 0, 300, 250, 150);
        var holder = Ui.Node("Holder", scene);
        Ui.Place(holder, 500, 140, 560, 230);
        var card = Ui.Float(holder, "Message", 0, 0, 560, 230);
        var from = Ui.Eyebrow(card.transform, "A MESSAGE FROM DAD", Theme.TealDeep);
        Ui.Place(from.rectTransform, 28, 24, 400, 22);
        var msg = Ui.Text(card.transform, "Fruit salad tonight. Bring back fruit, no vegetables please.", 28, Theme.Ink);
        Ui.Place(msg.rectTransform, 28, 60, 504, 120);
        var time = Ui.Text(card.transform, "7:42 pm", 15, Theme.Muted, false, TextAlignmentOptions.TopRight);
        Ui.Place(time.rectTransform, 400, 24, 132, 22);
        var g = Ui.Group(holder); g.alpha = 0;
        yield return Ui.Animate(0.5f, k => { g.alpha = k; holder.anchoredPosition = new Vector2(500 + 60 * (1 - k), -140); }, Ui.EaseOut);
        var note = Ui.Text(scene, "You read it. The agent never does.", 22, Theme.Faint);
        Ui.Place(note.rectTransform, 500, 400, 560, 40);
    }

    // 3. The list, typed out.
    IEnumerator Beat3()
    {
        Robot(scene, 0, 120, 200, 200);
        var card = Ui.Float(scene, "List", 420, 60, 600, 360);
        var eyebrow = Ui.Eyebrow(card.transform, "YOUR LIST FOR THE AGENT", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 28, 24, 400, 22);
        var list = Ui.Text(card.transform, "", 24, Theme.Ink);
        Ui.Place(list.rectTransform, 28, 60, 544, 220);
        yield return new WaitForSecondsRealtime(0.3f);
        yield return Ui.Type(list, "Apples, bananas, pears, grapes.\nNo tomatoes, no mushrooms, no vegetables at all.\nIf nothing on this list is in view, keep walking.", 40f);
        var note = Ui.Text(card.transform, "This is its job description. Nothing else goes with it.", 17, Theme.TealDeep, true);
        Ui.Place(note.rectTransform, 28, 300, 544, 30);
        var g = Ui.Group(note); g.alpha = 0;
        yield return Ui.Fade(g, 1f, 0.4f);
    }

    // 4. The market: three steps of the loop, each named.
    IEnumerator Beat4()
    {
        const float cell = 96; const int cols = 7, rows = 4;
        float x0 = 20, y0 = 10, size = cell * 0.84f;
        Market(cols, rows, cell, x0, y0);
        var apple = Stall(scene, "apple", At(3, 0, cell, x0, y0, size).x, At(3, 0, cell, x0, y0, size).y, size);
        Stall(scene, "carrot", At(1, 2, cell, x0, y0, size).x, At(1, 2, cell, x0, y0, size).y, size);
        Stall(scene, "banana", At(5, 1, cell, x0, y0, size).x, At(5, 1, cell, x0, y0, size).y, size);
        Stall(scene, "tomato", At(4, 3, cell, x0, y0, size).x, At(4, 3, cell, x0, y0, size).y, size);
        var bot = Robot(scene, 0, At(0, 1, cell, x0, y0, size).x, At(0, 1, cell, x0, y0, size).y, size);

        // The loop's four words, lit one after another under the market.
        var words = new[] { "See", "Decide", "Act", "Text home" };
        var chips = new List<(Image card, TextMeshProUGUI label)>();
        for (int i = 0; i < words.Length; i++)
        {
            var c = Ui.Card(scene, words[i], Theme.Ink2, Theme.TealDeep);
            Ui.Place(c.rectTransform, 20 + i * 180, 436, 168, 52);
            var l = Ui.Text(c.transform, words[i], 20, Theme.Faint, true, TextAlignmentOptions.Center);
            Ui.Stretch(l.rectTransform);
            chips.Add((c, l));
        }
        void Light(int i)
        {
            for (int k = 0; k < chips.Count; k++)
            {
                chips[k].card.color = k == i ? Theme.Teal : Theme.TealDeep;
                chips[k].card.transform.Find("Fill").GetComponent<Image>().color = k == i ? Theme.Teal : Theme.Ink2;
                chips[k].label.color = k == i ? Theme.Ink : Theme.Faint;
            }
        }
        var caption = Ui.Text(scene, "No phone. At every stall: look, decide, move, text home. Then again.", 20, Theme.Faint);
        Ui.Place(caption.rectTransform, 20, 500, 900, 40);

        var steps = new (int c, int r, string text)[]
        {
            (1, 1, "Nothing on the list in view. Keep walking."),
            (2, 1, "I see an apple, north-east. Apples are on the list. Heading there."),
            (3, 1, "Apple is just north. Going north."),
        };
        foreach (var (c, r, text) in steps)
        {
            Light(0); yield return new WaitForSecondsRealtime(0.5f);
            Light(1); yield return Say(bot, text);
            Light(2); yield return Slide(bot, At(c, r, cell, x0, y0, size), 0.45f);
            Light(3); yield return new WaitForSecondsRealtime(0.7f);
            foreach (Transform child in scene) if (child.name == "Bubble") UnityEngine.Object.Destroy(child.gameObject);
        }
        Light(2);
        yield return Slide(bot, At(3, 0, cell, x0, y0, size), 0.45f);
        UnityEngine.Object.Destroy(apple.gameObject);
        Sfx.Buy();
        Light(3);
        yield return Say(bot, "Got the apple. Texting home.");
        while (true) yield return null;
    }

    // 5. The tomato: a literal robot and a vague list.
    IEnumerator Beat5()
    {
        const float cell = 96; const int cols = 7, rows = 4;
        float x0 = 20, y0 = 10, size = cell * 0.84f;
        Market(cols, rows, cell, x0, y0);
        var tomato = Stall(scene, "tomato", At(4, 2, cell, x0, y0, size).x, At(4, 2, cell, x0, y0, size).y, size);
        Stall(scene, "pear", At(6, 0, cell, x0, y0, size).x, At(6, 0, cell, x0, y0, size).y, size);
        var bot = Robot(scene, 0, At(3, 2, cell, x0, y0, size).x, At(3, 2, cell, x0, y0, size).y, size);
        var listCard = Ui.Soft(scene, "List");
        Ui.Place(listCard.rectTransform, 20, 436, 420, 70);
        var listEyebrow = Ui.Eyebrow(listCard.transform, "THE LIST SAID", Theme.TealDeep);
        Ui.Place(listEyebrow.rectTransform, 20, 12, 300, 20);
        var listText = Ui.Text(listCard.transform, "\"Get fruit. No vegetables.\"", 20, Theme.Ink, true);
        Ui.Place(listText.rectTransform, 20, 34, 380, 30);

        yield return new WaitForSecondsRealtime(0.6f);
        yield return Say(bot, "Fruit salad needs fruit. A tomato is a fruit. Buying.", 360);
        yield return new WaitForSecondsRealtime(0.4f);
        yield return Slide(bot, At(4, 2, cell, x0, y0, size), 0.45f);
        UnityEngine.Object.Destroy(tomato.gameObject);
        Sfx.Ruin();
        var flash = Ui.Box(scene, "Flash", Theme.Danger, false);
        Ui.Stretch(flash.rectTransform);
        flash.raycastTarget = false;
        yield return Ui.Animate(0.7f, k => flash.color = new Color(Theme.Danger.r, Theme.Danger.g, Theme.Danger.b, 0.45f * Mathf.Sin(k * Mathf.PI)));
        UnityEngine.Object.Destroy(flash.gameObject);
        var verdict = Ui.Card(scene, "Verdict", Theme.DangerSoft, Theme.Danger);
        Ui.Place(verdict.rectTransform, 470, 436, 560, 70);
        var v = Ui.Text(verdict.transform, "Dinner ruined. The agent is sent home.", 22, Theme.Danger, true, TextAlignmentOptions.MidlineLeft);
        Ui.Place(v.rectTransform, 20, 0, 520, 70);
        var g = Ui.Group(verdict); g.alpha = 0;
        yield return Ui.Fade(g, 1f, 0.3f);
        var note = Ui.Text(scene, "The agent was not wrong. The list was vague. Say exactly what you mean.", 20, Theme.Faint);
        Ui.Place(note.rectTransform, 20, 530, 1000, 40);
    }

    // 6. The ladder.
    IEnumerator Beat6()
    {
        for (int i = 0; i < Ladder.Count; i++)
        {
            var l = Ladder.All[i];
            var card = Ui.Card(scene, "Step", Theme.Ink2, Theme.TealDeep);
            Ui.Place(card.rectTransform, 20 + (i % 3) * 360, 40 + (i / 3) * 210, 340, 180);
            var n = Ui.Text(card.transform, (i + 1).ToString(), 56, Theme.Teal, true);
            Ui.Place(n.rectTransform, 24, 14, 100, 70);
            var t = Ui.Text(card.transform, l.title, 24, Color.white, true);
            Ui.Place(t.rectTransform, 24, 86, 300, 34);
            var k = Ui.Text(card.transform, l.kicker, 15, Theme.Faint);
            Ui.Place(k.rectTransform, 24, 120, 300, 50);
            var g = Ui.Group(card); g.alpha = 0;
            app.StartCoroutine(Ui.Animate(0.45f, q => { if (g == null) return; g.alpha = q; card.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, q); }, Ui.EaseOutBack));
            Sfx.Tick();
            yield return new WaitForSecondsRealtime(0.25f);
        }
        var note = Ui.Text(scene, "Each errand opens one more part of the agent. Free play, with everything open, waits at the end.", 20, Theme.Faint);
        Ui.Place(note.rectTransform, 20, 470, 1040, 60);
    }
}
