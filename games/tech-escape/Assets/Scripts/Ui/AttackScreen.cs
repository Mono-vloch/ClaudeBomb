using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The facility runs the attack: the corridor from above on the left, the attacker
// walking to each door the attack tries; on the right, line by line, what it
// tried, and whether the door held or gave. What got through goes to the record.
public class AttackScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Attack attack;
    readonly RectTransform linesLayer, bannerLayer;
    readonly TextMeshProUGUI summary;
    readonly Button results;
    float y;

    public AttackScreen(Transform parent, App app)
    {
        this.app = app;
        var room = app.room;
        root = Ui.Node("Attack", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        attack = room.attack(app.state, app.session);
        app.Record(attack);

        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 40, 104, 940, 920);
        app.world.Build(false);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.SetDoor(Facility.RoomIndex(room.room), Building.DoorState.Current);
        app.world.ShowOverview(frame);

        var column = Ui.Box(root, "Column", new Color(1, 1, 1, 0.86f), false);
        Ui.Place(column.rectTransform, 1000, -300, 1600, 1700);
        column.raycastTarget = false;
        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.8f));
        Ui.Place(top.rectTransform, 40, 16, 950, 82);
        top.raycastTarget = false;
        var turnText = Ui.Text(root, "The facility runs the attack", 28, Theme.Ink, true);
        Ui.Place(turnText.rectTransform, 60, 24, 700, 40);
        var phaseText = Ui.Text(root, $"Level {room.id}  ·  {room.room}  ·  {app.StepName}", 15, Theme.Text2);
        Ui.Place(phaseText.rectTransform, 60, 62, 700, 26);

        var head = Ui.Eyebrow(root, "WHAT IT TRIED, AND WHAT GOT THROUGH", Theme.TealDeep);
        head.font = Theme.Bold;
        Ui.Place(head.rectTransform, 1020, 32, 800, 20);
        linesLayer = Ui.Node("Lines", root);
        Ui.Place(linesLayer, 1020, 60, 840, 760);
        bannerLayer = Ui.Node("Banners", root);
        Ui.Stretch(bannerLayer);
        summary = Ui.Text(root, "", 18, Theme.Ink, true);
        Ui.Place(summary.rectTransform, 1020, 830, 840, 60);
        Luma.Card(root, 1020, 900, 840, 120, app.luma.Say("live-" + room.number), 15);
        results = Ui.Button(root, "See the debrief", app.ShowResults, Ui.ButtonStyle.Primary, 18);
        Ui.Place(results.GetComponent<RectTransform>(), 1620, 1026, 240, 46);
        results.gameObject.SetActive(false);

        var runner = root.gameObject.AddComponent<Runner>();
        runner.StartCoroutine(Run());
        Ui.Emblem(root);
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(0.4f);
        yield return Banner("THE ATTACK BEGINS", Theme.Danger, 1.0f);
        yield return app.world.WalkTo(app.world.Entrance + Vector3.right * 1.5f, 0.8f);
        foreach (var line in attack.lines)
        {
            int index = Facility.RoomIndex(line.room);
            if (index >= 0) yield return app.world.Visit(index, !line.held, 0.9f);
            Line(line.room, line.held ? "HELD" : "THROUGH", line.held ? line.reached : line.gap + ": " + line.reached, line.held);
            if (!line.held) Sfx.Ruin(); else Sfx.Buy();
            yield return Banner(line.held ? "HELD  ·  " + line.room.ToUpperInvariant() : "THROUGH  ·  " + line.room.ToUpperInvariant(), line.held ? Theme.TealDeep : Theme.Danger, 0.8f);
            yield return new WaitForSeconds(0.2f);
        }
        if (app.room.Exit && !attack.AnyThrough) { app.world.SetExit(true); yield return app.world.WalkTo(app.world.ExitPoint, 1.2f); yield return Banner("THE EXIT OPENS", Theme.TealDeep, 1.4f); }
        summary.text = attack.summary;
        results.gameObject.SetActive(true);
    }

    void Line(string room, string verdict, string text, bool held)
    {
        var card = Ui.Card(linesLayer, "Line", held ? Theme.Panel : Theme.DangerSoft, held ? Theme.BorderTeal : Theme.Danger);
        Ui.Place(card.rectTransform, 0, y, 840, 64);
        var v = Ui.Text(card.transform, verdict, 11, held ? Theme.TealDeep : Theme.Danger, true);
        v.characterSpacing = 3;
        Ui.Place(v.rectTransform, 14, 8, 100, 16);
        var r = Ui.Text(card.transform, room, 11, Theme.Muted, true);
        Ui.Place(r.rectTransform, 100, 8, 400, 16);
        var t = Ui.Text(card.transform, text, 13, Theme.Ink);
        Ui.Place(t.rectTransform, 14, 26, 812, 36);
        var g = Ui.Group(card); g.alpha = 0;
        root.GetComponent<Runner>().StartCoroutine(Ui.Fade(g, 1f, 0.3f));
        y += 70;
    }

    IEnumerator Banner(string text, Color colour, float seconds)
    {
        var card = Ui.Box(bannerLayer, "Banner", new Color(1, 1, 1, 0.92f));
        Ui.Place(card.rectTransform, 110, 480, 820, 100);
        card.raycastTarget = false;
        var t = Ui.Text(card.transform, text, 40, colour, true, TextAlignmentOptions.Center);
        t.characterSpacing = 4;
        Ui.Stretch(t.rectTransform);
        var group = Ui.Group(card);
        card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = new Vector2(110 + 410, -(480 + 50));
        yield return Ui.Animate(0.18f, k => { group.alpha = k; card.rectTransform.localScale = Vector3.one * (1.15f - 0.15f * k); }, Ui.EaseOut);
        yield return new WaitForSecondsRealtime(seconds);
        yield return Ui.Animate(0.25f, k => group.alpha = 1 - k);
        Object.Destroy(card.gameObject);
    }

    class Runner : MonoBehaviour { }
}
