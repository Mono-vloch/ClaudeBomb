using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The story, before the first errand (the game-feel pass): four short shots of the world,
// each with one sentence from LUMA's story lines, played once; Skip ends it and a tap anywhere
// moves on. The house at dusk and the phone lighting up; the agent at the door with its note;
// the agent walking into the market; the tomato going into the basket. The flat pictures,
// captions and the ladder of errands went: the full story is in the file.
public class StoryScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly RectTransform frame;
    bool next, skip;
    Coroutine main;
    public int Current { get; private set; } // the shot on screen, 1 to 4

    public void Stop()
    {
        if (main != null) app.StopCoroutine(main);
        main = null;
    }

    public StoryScreen(Transform parent, App app, Action done)
    {
        this.app = app;
        root = Ui.Node("Story", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        Hud.TapAnywhere(root, () => next = true);
        var skipBtn = Ui.Button(root, "Skip", () => skip = true, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(skipBtn.GetComponent<RectTransform>(), Ui.W - 40 - 100, 30, 100, 40);
        main = app.StartCoroutine(Run(done));
        app.onLeave = Stop;
    }

    // Waits for a tap, Skip, or LUMA's line to end plus a breath.
    IEnumerator Hold(float least, float most = 7f)
    {
        next = false;
        float t = 0f;
        while (!next && !skip)
        {
            t += Time.unscaledDeltaTime;
            if (t > least && !app.luma.Busy) break;
            if (t > most) break;
            yield return null;
        }
    }

    IEnumerator Run(Action done)
    {
        var rng = new System.Random(5);
        var configs = new List<AgentConfig> { app.configs[0].Clone() };
        for (int i = 1; i < 4; i++) configs.Add(new AgentConfig { name = Theme.TeamNames[i], colour = (app.configs[0].colour + i * 3) % Theme.Team.Length });
        var arena = new Arena(Brief.Make(0, rng), configs, 30_000, 21);
        var hero = arena.agents[0];
        var world = app.world;
        {
            var inward0 = Market.Inward(hero.home);
            var step0 = new Vector2Int(Mathf.RoundToInt(inward0.x), -Mathf.RoundToInt(inward0.z));
            var side0 = new Vector2Int(step0.y, step0.x);
            for (int d = 0; d <= 3; d++) for (int s = -2; s <= 2; s++) { var p = hero.home + step0 * d + side0 * s; if (Arena.InBounds(p)) arena.RemoveItem(p); }
        }

        // 1. The house at dusk; the phone on the step lights up.
        Current = 1;
        world.Build(arena, true);
        world.PlaceAtDoor(hero, false, Market.Pitch * 3f); // out of the shot for now
        world.DoorLamp(hero, 2.2f);
        world.FamilySpot(hero);
        var phone = world.StoryPhone(hero);
        world.ShowHouse(frame, hero, 4.4f, 36f);
        yield return new WaitForSecondsRealtime(0.9f);
        if (!skip)
        {
            world.PhoneScreen(phone, true);
            Sfx.Chime();
            app.luma.Cue(root, "shot-1");
            yield return Hold(2.4f);
        }

        // 2. The agent at the door, its note pinned on.
        Transform note = null;
        if (!skip)
        {
            Current = 2;
            world.PhoneScreen(phone, false);
            world.PlaceAtDoor(hero, false, 0.5f);
            note = world.PinNote(hero, "Get fruit.");
            world.ShowHouse(frame, hero, 2.9f, 34f, onAgent: true);
            app.luma.Cue(root, "shot-2");
            yield return Hold(2.4f);
        }

        // 3. It walks into the market, and looks around: no phone.
        if (!skip)
        {
            Current = 3;
            if (note != null) UnityEngine.Object.Destroy(note.gameObject);
            if (phone != null) UnityEngine.Object.Destroy(phone.gameObject);
            app.luma.Cue(root, "shot-3");
            var walk = app.StartCoroutine(world.WalkInto(hero, 2, frame));
            float t = 0f; next = false;
            while (!skip && !next && t < 6.5f) { t += Time.unscaledDeltaTime; yield return null; }
            if (t < 6.5f) app.StopCoroutine(walk);
            yield return Hold(0.4f, 2f);
        }

        // 4. A tomato goes into the basket: dinner ruined.
        if (!skip)
        {
            Current = 4;
            var inward = Market.Inward(hero.home);
            var step = new Vector2Int(Mathf.RoundToInt(inward.x), -Mathf.RoundToInt(inward.z));
            var cell = hero.pos + step;
            app.luma.Cue(root, "shot-4", stay: true);
            if (Arena.InBounds(cell))
            {
                arena.RemoveItem(cell);
                arena.items[cell.x, cell.y] = Items.Get("tomato");
                world.RefreshItems();
                world.SideView(frame, hero, cell);
                yield return new WaitForSecondsRealtime(1.0f);
                hero.alive = false; hero.ateItem = "tomato"; hero.eliminatedTurn = 1;
                var move = new Simulation.Move { agent = hero, from = hero.pos, to = cell, ate = Items.Get("tomato") };
                hero.pos = cell;
                yield return world.Animate(new List<Simulation.Move> { move }, Market.StepSeconds(1f));
            }
            yield return Hold(1.2f);
        }

        Sfx.Whoosh();
        app.onLeave = null;
        main = null;
        done?.Invoke();
    }
}
