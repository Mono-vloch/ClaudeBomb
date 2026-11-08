using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The facility runs the attack: the corridor in lockdown, the camera flying in over the
// entrance as the siren goes, then chasing the attacker from door to door; a door that gives
// way is sparks, a shake, the alarm and THROUGH in scarlet, one that holds a teal flash and
// HELD. The HUD is the row of twelve door lamps, nothing else; LUMA says one line, at the end:
// what was taken, or the door that held. The first attack on a room as it was handed
// over is a rehearsal: it shows the door break and counts for nothing; the next run counts.
public class AttackScreen
{
    public static bool Running { get; private set; }
    public static bool Rehearsal { get; private set; }

    public readonly RectTransform root;
    readonly App app;
    readonly Attack attack;
    readonly RectTransform frame, bannerLayer;
    readonly Image[] lamps;
    readonly Button next;
    readonly bool rehearsal;
    int through, held;

    public AttackScreen(Transform parent, App app)
    {
        this.app = app;
        var room = app.room;
        root = Ui.Node("Attack", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        attack = room.attack(app.state, app.session);
        rehearsal = app.Untouched && attack.AnyThrough;
        Rehearsal = rehearsal;
        if (rehearsal) app.rehearsed = true; else app.Record(attack);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        app.world.Build();
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.SetDoor(Facility.RoomIndex(room.room), Building.DoorState.Current);
        app.world.ShowOverview(frame);
        app.atmosphere.MenuBlur(false);
        Sfx.Ambient(true);

        lamps = Hud.DoorLamps(root, (Ui.W - (12 * 28 + 28)) / 2f);
        if (rehearsal)
        {
            var tag = Ui.Glass(root, "Rehearsal", (Ui.W - 360) / 2f, 88, 360, 36, true, Theme.Amber);
            var t = Ui.Eyebrow(tag.transform, "REHEARSAL  ·  NOT COUNTED", Theme.Amber);
            t.font = Theme.Bold; t.enableWordWrapping = false;
            Ui.Place(t.rectTransform, 20, 9, 330, 20);
        }
        GameMenu.Icon(root);
        Lamps();

        next = Ui.Button(root, rehearsal ? "Back to the room" : "Debrief", rehearsal ? app.ShowRoom : (System.Action)app.ShowResults, Ui.ButtonStyle.Accent, 22);
        Ui.Place(next.GetComponent<RectTransform>(), Ui.W - 40 - 260, 996, 260, 64);
        next.gameObject.SetActive(false);
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);

        var runner = root.gameObject.AddComponent<Runner>();
        runner.StartCoroutine(Run());
        app.onLeave += () => { Running = false; Rehearsal = false; };
    }

    void Lamps()
    {
        for (int i = 0; i < 12; i++)
        {
            var s = app.world.StateOf(i);
            lamps[i].color = s == Building.DoorState.Breached ? Look.Breached : s == Building.DoorState.Held || s == Building.DoorState.Open ? Theme.Teal : s == Building.DoorState.Current ? Theme.Amber : new Color(1f, 1f, 1f, 0.18f);
        }
    }

    IEnumerator Run()
    {
        Running = true;
        var room = app.room;
        yield return new WaitForSeconds(0.3f);
        app.world.OpeningShot(frame, 2.4f);
        app.world.Alarm(2.2f);
        app.atmosphere.Pulse(Theme.Danger, 0.4f, 0.9f);
        yield return Moment("THE ATTACK BEGINS", "", Theme.Danger, 1.0f, true, Sfx.Mood.Siren);
        yield return app.world.WalkTo(app.world.Entrance + Vector3.right * 1.5f, 0.8f);
        float waited = 0f;
        Transform last = app.world.AttackerTransform;
        int lastDoor = -1, lastBroken = -1;
        Building.Payoff = "";
        foreach (var line in attack.lines)
        {
            int index = Facility.RoomIndex(line.room);
            if (index >= 0) { yield return app.world.Visit(index, !line.held, 1.0f); last = app.world.DoorTransform(index); lastDoor = index; if (!line.held) lastBroken = index; }
            else if (!line.held) { Sfx.Crash(); app.atmosphere.Pulse(Theme.Danger, 0.5f, 0.6f); app.world.Alarm(2f); }
            else Sfx.Clack();
            if (line.held) held++; else through++;
            Lamps();
            yield return Moment(line.held ? "HELD" : "THROUGH", Labels.Of(line.held ? line.reached : line.gap), line.held ? Theme.Teal : Look.Breached, line.held ? 0.7f : 1.1f, !line.held, line.held ? Sfx.Mood.Beep : Sfx.Mood.Alarm);
            yield return new WaitForSeconds(0.15f);
        }
        // The ending at the door: through it, the attacker comes back out with the drive and runs
        // off with it; held, he tries the handle again, gives up and walks away.
        if (lastBroken >= 0) { yield return app.world.Grab(lastBroken); last = app.world.DoorTransform(lastBroken); }
        else if (lastDoor >= 0) yield return app.world.GiveUp(lastDoor, walkAway: !room.Exit);
        bool finished = false;
        if (room.Exit && !attack.AnyThrough)
        {
            app.world.SetExit(true);
            yield return app.world.WalkTo(app.world.ExitPoint, 1.4f);
            app.effects.Confetti(app.world.ExitPoint + Vector3.up * 1.6f, 60);
            app.world.MomentAround(app.world.ExitTransform, 2.6f, () => finished = true, hold: true);
            yield return Moment("THE EXIT OPENS", "", Theme.Teal, 1.8f, true, Sfx.Mood.Fanfare);
        }
        else if (attack.AnyThrough)
        {
            app.world.MomentAround(last, 2.4f, () => finished = true, hold: true);
            yield return Moment(through == 1 ? "ONE DOOR GAVE WAY" : $"{through} DOORS GAVE WAY", "", Look.Breached, 1.8f, true, Sfx.Mood.Alarm);
        }
        else
        {
            app.world.MomentAround(last, 2.4f, () => finished = true, hold: true);
            yield return Moment("NOTHING GOT THROUGH", "", Theme.Teal, 1.8f, true, Sfx.Mood.Fanfare);
        }
        waited = 0f;
        while (!finished && waited < 4f) { waited += Time.unscaledDeltaTime; yield return null; }
        // The end stays on the door (the camera's last turn holds): shut and teal when it held,
        // hanging open and scarlet when not.
        // The summary as LUMA's line, whole: the first sentence when the whole runs long.
        string summary = Luma.Words(attack.summary) <= Luma.CueWords ? attack.summary : Luma.FirstSentence(attack.summary) ?? attack.summary;
        app.luma.Text(root, summary, 40, 944, 820, stay: true);
        next.gameObject.SetActive(true);
        Running = false;
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
            Ui.Stretch(flash.rectTransform);
            flash.raycastTarget = false;
        }
        var card = Ui.Box(bannerLayer, "Moment", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f));
        float h = big ? (string.IsNullOrEmpty(sub) ? 150 : 180) : string.IsNullOrEmpty(sub) ? 92 : 118;
        float w = big ? 820 : 640;
        Ui.Place(card.rectTransform, (Ui.W - w) / 2, 800 - h / 2, w, h); // low in the frame, over LUMA's strip: the attacker and the door stay in view above it
        card.raycastTarget = false;
        var keyline = Ui.Box(card.transform, "Keyline", colour, false);
        Ui.Place(keyline.rectTransform, 0, 0, 6, h);
        var t = Ui.Text(card.transform, text, big ? (text.Length > 16 ? 48 : 56) : 36, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = big ? 6 : 4;
        Ui.Place(t.rectTransform, 20, big ? 14 : 20, w - 40, big ? 100 : 52);
        t.enableWordWrapping = false;
        if (!string.IsNullOrEmpty(sub))
        {
            var st = Ui.Text(card.transform, sub, big ? 20 : 16, Look.Chalk, false, TextAlignmentOptions.Center);
            Ui.Place(st.rectTransform, 20, h - (big ? 52 : 36), w - 40, 26);
            st.enableWordWrapping = false; st.overflowMode = TextOverflowModes.Ellipsis;
        }
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(Ui.W / 2, -800);
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
