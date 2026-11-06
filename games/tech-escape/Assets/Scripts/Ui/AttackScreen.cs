using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The facility runs the attack: the corridor in lockdown, the camera flying in over
// the entrance as the siren goes, then chasing the attacker from door to door; a door
// that gives way is sparks, a shake, the alarm and THROUGH in scarlet, one that holds
// a teal flash and HELD; the lines of what it tried fill the column on the right. The
// exit opening, or what gave way, is the last moment. What got through goes to the record.
public class AttackScreen
{
    public static bool Running { get; private set; }

    public readonly RectTransform root;
    readonly App app;
    readonly Attack attack;
    readonly RectTransform frame, linesLayer, bannerLayer;
    readonly TextMeshProUGUI summary, counts;
    readonly Image[] lamps = new Image[12];
    readonly Button results;
    float y;
    int through, held;

    public AttackScreen(Transform parent, App app)
    {
        this.app = app;
        var room = app.room;
        root = Ui.Node("Attack", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        attack = room.attack(app.state, app.session);
        app.Record(attack);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 40, 110, 1180, 930);
        app.world.Build();
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.SetDoor(Facility.RoomIndex(room.room), Building.DoorState.Current);
        app.world.ShowOverview(frame);
        Sfx.Ambient(true);

        // The HUD: the level and the room, the twelve doors as lamps, the counts.
        var hud = Ui.Glass(root, "Hud", 40, 16, 1180, 84, true, Theme.Danger);
        var eyebrow = Ui.Eyebrow(hud.transform, "THE FACILITY RUNS THE ATTACK", Theme.Danger);
        eyebrow.font = Theme.Bold;
        Ui.Place(eyebrow.rectTransform, 24, 12, 600, 18);
        var phaseText = Ui.Text(hud.transform, $"Level {room.id}  ·  {room.room}  ·  {app.StepName}", 24, Color.white, true);
        Ui.Place(phaseText.rectTransform, 24, 34, 720, 40);
        phaseText.enableWordWrapping = false; phaseText.overflowMode = TextOverflowModes.Ellipsis;
        var doorsLabel = Ui.Eyebrow(hud.transform, "THE DOORS", Theme.Teal);
        Ui.Place(doorsLabel.rectTransform, 800, 12, 200, 18);
        for (int i = 0; i < 12; i++)
        {
            lamps[i] = Ui.Box(hud.transform, "Lamp", Color.white, false);
            Ui.Place(lamps[i].rectTransform, 800 + i * 26, 34, 18, 18);
            lamps[i].raycastTarget = false;
        }
        counts = Ui.Text(hud.transform, "THROUGH 0  ·  HELD 0", 13, Look.Chalk, true);
        counts.characterSpacing = 3;
        Ui.Place(counts.rectTransform, 800, 58, 360, 20);
        Lamps();

        // The lines, in glass on the right.
        var column = Ui.Glass(root, "Lines", 1240, 110, 640, 700);
        var head = Ui.Eyebrow(column.transform, "WHAT IT TRIED", Theme.Teal);
        head.font = Theme.Bold;
        Ui.Place(head.rectTransform, 20, 14, 600, 18);
        linesLayer = Ui.Node("Lines", column.transform);
        Ui.Place(linesLayer, 20, 44, 600, 640);
        var sumBox = Ui.Glass(root, "Summary", 1240, 820, 640, 60);
        summary = Ui.Text(sumBox.transform, "", 16, Look.Chalk, true, TextAlignmentOptions.MidlineLeft);
        Ui.Place(summary.rectTransform, 20, 0, 600, 60);
        app.luma.Line(root, 1240, 890, 640, app.luma.Say("live-" + room.number), true, true, 14);
        results = Ui.Button(root, "See the debrief", app.ShowResults, Ui.ButtonStyle.Primary, 18);
        Ui.Place(results.GetComponent<RectTransform>(), 1640, 1012, 240, 52);
        results.gameObject.SetActive(false);
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);

        var runner = root.gameObject.AddComponent<Runner>();
        runner.StartCoroutine(Run());
        app.onLeave += () => Running = false;
    }

    void Lamps()
    {
        for (int i = 0; i < 12; i++)
        {
            var s = app.world.StateOf(i);
            lamps[i].color = s == Building.DoorState.Breached ? Look.Breached : s == Building.DoorState.Held || s == Building.DoorState.Open ? Theme.Teal : s == Building.DoorState.Current ? Theme.Amber : new Color(Look.Locked.r, Look.Locked.g, Look.Locked.b, 0.7f);
        }
        counts.text = $"THROUGH {through}  ·  HELD {held}";
    }

    IEnumerator Run()
    {
        Running = true;
        yield return new WaitForSeconds(0.3f);
        app.world.OpeningShot(frame, 2.4f);
        app.world.Alarm(2.2f);
        app.atmosphere.Pulse(Theme.Danger, 0.4f, 0.9f);
        yield return Moment("THE ATTACK BEGINS", "", Theme.Danger, 1.3f, true, Sfx.Mood.Siren);
        yield return app.world.WalkTo(app.world.Entrance + Vector3.right * 1.5f, 0.8f);
        Transform last = app.world.AttackerTransform;
        foreach (var line in attack.lines)
        {
            int index = Facility.RoomIndex(line.room);
            if (index >= 0) { yield return app.world.Visit(index, !line.held, 1.0f); last = app.world.DoorTransform(index); }
            else if (!line.held) { Sfx.Crash(); app.atmosphere.Pulse(Theme.Danger, 0.5f, 0.6f); app.world.Alarm(2f); }
            else Sfx.Clack();
            Line(line.room, line.held ? "HELD" : "THROUGH", line.held ? "" : line.gap, line.held);
            if (line.held) held++; else through++;
            Lamps();
            yield return Moment(line.held ? "HELD" : "THROUGH", line.room + "  ·  " + (line.held ? line.reached : line.gap), line.held ? Theme.Teal : Look.Breached, line.held ? 0.7f : 1.1f, !line.held, line.held ? Sfx.Mood.Beep : Sfx.Mood.Alarm);
            yield return new WaitForSeconds(0.15f);
        }
        bool finished = false;
        if (app.room.Exit && !attack.AnyThrough)
        {
            app.world.SetExit(true);
            yield return app.world.WalkTo(app.world.ExitPoint, 1.4f);
            app.effects.Confetti(app.world.ExitPoint + Vector3.up * 1.6f, 60);
            app.world.MomentAround(app.world.ExitTransform, 2.6f, () => finished = true);
            yield return Moment("THE EXIT OPENS", "the whole chain held", Theme.Teal, 1.8f, true, Sfx.Mood.Fanfare);
        }
        else if (attack.AnyThrough)
        {
            app.world.MomentAround(last, 2.4f, () => finished = true);
            yield return Moment(through == 1 ? "ONE DOOR GAVE WAY" : $"{through} DOORS GAVE WAY", attack.summary, Look.Breached, 1.8f, true, Sfx.Mood.Alarm);
        }
        else
        {
            app.world.MomentAround(last, 2.4f, () => finished = true);
            yield return Moment("NOTHING GOT THROUGH", attack.summary, Theme.Teal, 1.8f, true, Sfx.Mood.Fanfare);
        }
        float waited = 0f;
        while (!finished && waited < 4f) { waited += Time.unscaledDeltaTime; yield return null; }
        app.world.BackToOverview();
        summary.text = attack.summary;
        results.gameObject.SetActive(true);
        Running = false;
    }

    void Line(string room, string verdict, string text, bool held)
    {
        if (y > 600) return; // the column is full; the debrief lists every line
        var card = Ui.Card(linesLayer, "Line", new Color(1f, 1f, 1f, held ? 0.06f : 0.1f), held ? new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.6f) : Look.Breached);
        Ui.Place(card.rectTransform, 0, y, 600, 64);
        var v = Ui.Text(card.transform, verdict, 11, held ? Theme.Teal : Look.Breached, true);
        v.characterSpacing = 3;
        Ui.Place(v.rectTransform, 14, 8, 100, 16);
        var r = Ui.Text(card.transform, room, 11, Look.Chalk, true);
        Ui.Place(r.rectTransform, 100, 8, 400, 16);
        var t = Ui.Text(card.transform, text, 12, Look.Chalk);
        Ui.Place(t.rectTransform, 14, 26, 572, 36);
        t.overflowMode = TextOverflowModes.Ellipsis;
        var g = Ui.Group(card); g.alpha = 0;
        root.GetComponent<Runner>().StartCoroutine(Ui.Fade(g, 1f, 0.3f));
        y += 70;
    }

    // A moment over the corridor, low in the frame so the attacker stays in view, in dark glass
    // with a keyline in its colour: big ones come in with an overshoot and a flash of the colour
    // over the world.
    IEnumerator Moment(string text, string sub, Color colour, float seconds, bool big, Sfx.Mood mood)
    {
        Image flash = null;
        if (big)
        {
            flash = Ui.Box(bannerLayer, "Flash", new Color(colour.r, colour.g, colour.b, 0f), false);
            Ui.Place(flash.rectTransform, 40, 110, 1180, 930);
            flash.raycastTarget = false;
        }
        var card = Ui.Box(bannerLayer, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        float h = big ? (string.IsNullOrEmpty(sub) ? 150 : 180) : 92;
        float w = big ? 820 : 640;
        Ui.Place(card.rectTransform, 40 + (1180 - w) / 2, 935 - h / 2, w, h); // low in the frame: the attacker and the door stay in view above it
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", colour, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, h);
        var t = Ui.Text(card.transform, text, big ? (text.Length > 16 ? 48 : 56) : 36, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = big ? 6 : 4;
        Ui.Place(t.rectTransform, 20, big ? 14 : 20, w - 40, big ? 100 : 52);
        t.enableWordWrapping = false;
        if (!string.IsNullOrEmpty(sub))
        {
            var st = Ui.Text(card.transform, sub, big ? 18 : 14, Look.Chalk, false, TextAlignmentOptions.Center);
            Ui.Place(st.rectTransform, 20, h - (big ? 52 : 32), w - 40, 26);
            st.enableWordWrapping = false; st.overflowMode = TextOverflowModes.Ellipsis;
        }
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(630, -935);
        Sfx.Sting(mood);
        yield return Ui.Animate(big ? 0.34f : 0.2f, k =>
        {
            group.alpha = Mathf.Min(1f, k * 3f);
            float sc = big ? Mathf.LerpUnclamped(0.6f, 1f, Ui.EaseOutBack(k)) : Mathf.Lerp(1.12f, 1f, k);
            card.rectTransform.localScale = Vector3.one * sc;
            if (flash != null) flash.color = new Color(colour.r, colour.g, colour.b, 0.45f * Mathf.Max(0f, 1f - k * 2.5f));
        });
        if (flash != null) Object.Destroy(flash.gameObject);
        yield return new WaitForSecondsRealtime(seconds);
        yield return Ui.Animate(0.22f, k => { group.alpha = 1 - k; card.rectTransform.localScale = Vector3.one * (1f - 0.08f * k); });
        Object.Destroy(card.gameObject);
    }

    class Runner : MonoBehaviour { }
}
