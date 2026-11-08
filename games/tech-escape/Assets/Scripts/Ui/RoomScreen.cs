using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Inside a room: the room itself fills the screen, seen from inside at shoulder height like a
// security camera's feed, and the fix is done on the room's prop (RoomRig): cards on the board,
// levers in the rack, badges, tickets, envelopes. One line at the top says what to do; the
// corridor monitor on the wall shows the attacker coming; Camera steps up to the prop and back;
// the menu holds the rest. Then: lift the lockdown, and the facility runs the attack.
public class RoomScreen
{
    public static RoomScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Room room;
    readonly RoomState st;
    readonly RectTransform frame;
    readonly int index;
    readonly TextMeshProUGUI objective;
    public RoomRig Rig { get; private set; }
    bool close, watched;
    UnityEngine.UI.Image[] rowLamps;

    public RoomScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        room = app.room;
        st = app.state;
        root = Ui.Node("Room", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        app.world.Build();
        index = Facility.RoomIndex(room.room);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == index ? Building.DoorState.Open : i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowRoomFeed(frame, index);
        app.atmosphere.MenuBlur(false);
        Sfx.Ambient(true);
        if (app.world.HasInterior(index)) CameraFeed.Build(frame, $"CAM {index + 1:00}", "", 96, bottom: false); // the buttons sit where the lower brackets would

        Rig = RoomRig.Build(app.world, index, room, st, Changed);
        Rig.Right = k => room.RowRight(st, app.session, k);
        Rig.Selected += t =>
        {
            if (t != null && Rig.FocusView(out var eye, out var look, out float fov)) app.world.ShowFocus(frame, eye, look, fov);
            else if (close) app.world.ShowRoomClose(frame, index, Rig.Bounds);
            else app.world.BackToRoom(frame, index);
        };
        Monitor();
        app.world.Lurk(index);

        objective = Hud.Objective(root, app.rehearsed ? Script.Rehearsed : Script.Objective(room));
        int missing = room.items.Count(i => i.hidden && !i.decoy && !st.added.Contains(i.key));
        int lampCount = Rig.RowKeys().Count + missing + (room.fields.Any(f => f.once) || room.signable ? 1 : 0);
        rowLamps = Hud.DoorLamps(root, (Ui.W - (Mathf.Min(12, lampCount) * 28 + 28)) / 2f, 24, Mathf.Min(12, lampCount));
        GameMenu.Icon(root);
        Hud.Small(root, "Camera", 40, 120, ToggleCamera);
        Hud.Main(root, Script.Lift, app.RunAttack);
        Changed();

        var runner = root.gameObject.AddComponent<Runner>();
        runner.StartCoroutine(Open());
        if (Rig.LookAround) Turnable();
    }

    // The corridor monitor on the back wall, on the far side of the prop from the board.
    void Monitor()
    {
        var roomRoot = app.world.RoomRoot(index);
        var heroT = app.world.HeroOf(index);
        var wallScreens = heroT != null ? heroT.GetComponentsInChildren<Renderer>().Where(r => r.name == "Screen").ToList() : new List<Renderer>();
        if (wallScreens.Count > 0)
        {
            // The Operations Center's wall of screens: the feed goes on the one the cards cover least.
            Renderer best = null; float bestCover = float.MaxValue;
            foreach (var r in wallScreens)
            {
                var b = r.bounds;
                var lo = roomRoot.InverseTransformPoint(b.min); var hi = roomRoot.InverseTransformPoint(b.max);
                var view = Rig.ViewRect(Mathf.Min(lo.x, hi.x), Mathf.Min(lo.y, hi.y), Mathf.Max(lo.x, hi.x), Mathf.Max(lo.y, hi.y), Mathf.Max(lo.z, hi.z));
                float cover = Covered(view, Rig.GridView) + (Rig.ConsoleView.HasValue ? Covered(view, Rig.ConsoleView.Value) : 0f);
                foreach (var o in Rig.Occluders) cover += Covered(view, o);
                if (cover < bestCover) { bestCover = cover; best = r; }
            }
            // A screen the cards hide in part loses the corridor: then a monitor of its own on the
            // wall beside the screens, when there is a clear spot for one.
            if (bestCover <= 0.15f || !PlaceStanding(true))
            {
                app.world.FeedOnScreen(index, best);
                if (DemoDriver.Wanted) Debug.Log($"[Monitor] {room.id}: on the wall screen {best.transform.parent.name}, {bestCover:P0} covered");
            }
            else if (DemoDriver.Wanted) Debug.Log($"[Monitor] {room.id}: the best wall screen was {bestCover:P0} covered; a monitor of its own instead");
            return;
        }
        PlaceStanding(false);
    }

    // The monitor on the back wall, where the feed shows it and nothing else stands. Clear only:
    // with no clear spot it places nothing and says so; otherwise the least-covered spot.
    bool PlaceStanding(bool clearOnly)
    {
        var roomRoot = app.world.RoomRoot(index);
        var hb = app.world.HeroBounds(index);
        var min = roomRoot.InverseTransformPoint(hb.min); var max = roomRoot.InverseTransformPoint(hb.max);
        float hx0 = Mathf.Min(min.x, max.x), hx1 = Mathf.Max(min.x, max.x), hz = Mathf.Min(min.z, max.z);
        var board = roomRoot.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Board");
        float bx = board != null ? roomRoot.InverseTransformPoint(board.position).x : hx1 + 0.4f;
        float mid = (hx0 + hx1) / 2f;
        // The monitor goes where the feed shows it and nothing else stands: first the side away
        // from the console (or from the board of lamps), then beyond the console on its own side,
        // then over the console, else over the grid.
        bool boardLeft = bx > mid;
        bool consoleLeft = Rig.ConsoleX.HasValue ? Rig.ConsoleX.Value > mid : boardLeft;
        float awayX = Mathf.Clamp(!consoleLeft ? Mathf.Max(hx1, Rig.GridRight) + 0.42f : Mathf.Min(hx0, Rig.GridLeft) - 0.42f, -1.5f, 1.5f);
        bool awayOverBoard = !consoleLeft == boardLeft;
        var tries = new List<(float x, float y)>();
        foreach (var cy in awayOverBoard ? new[] { 1.45f, 1.3f, 0.55f } : new[] { 1.0f, 0.8f, 0.6f, 1.3f }) tries.Add((awayX, cy));
        if (!Rig.ConsoleX.HasValue)
        {
            float other = Mathf.Clamp(consoleLeft ? Mathf.Max(hx1, Rig.GridRight) + 0.42f : Mathf.Min(hx0, Rig.GridLeft) - 0.42f, -1.5f, 1.5f);
            foreach (var cy in new[] { 1.0f, 0.8f, 0.6f, 1.3f }) tries.Add((other, cy));
        }
        if (Rig.ConsoleX.HasValue)
        {
            float beyond = Mathf.Clamp(consoleLeft ? Rig.ConsoleX.Value + 0.52f : Rig.ConsoleX.Value - 0.52f, -1.5f, 1.5f);
            foreach (var cy in new[] { 1.0f, 0.8f, 1.3f }) tries.Add((beyond, cy));
            tries.Add((Rig.ConsoleX.Value, Rig.ConsoleTop + 0.08f));
        }
        foreach (float sx in new[] { -1.45f, -1.2f, -0.95f, 0.95f, 1.2f, 1.45f })
            foreach (var cy in new[] { 1.5f, 1.3f, 1.1f, 0.9f, 0.7f, 0.55f, 0.4f }) tries.Add((sx, cy));
        float x = awayX, y = Mathf.Min(Rig.GridTop + 0.1f, 1.3f);
        // The lab rooms' server rack stands between the camera and the back wall's corner.
        var rack = roomRoot.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Rack");
        float? rackX = rack != null ? roomRoot.InverseTransformPoint(rack.position).x : (float?)null;
        // Free: on screen, the monitor covers neither the console nor the grid (they stand nearer the camera).
        // The board of lamps on the wall must stay in sight too.
        Rect? boardView = null;
        if (board != null)
        {
            var bl = roomRoot.InverseTransformPoint(board.position);
            if (Mathf.Abs(bl.z + 3.588f) < 0.1f) boardView = Rig.ViewRect(bl.x - 0.38f, bl.y - 0.1f, bl.x + 0.38f, bl.y + 0.1f, bl.z);
        }
        const float K = Building.MonitorScale;
        bool Free(float tx, float ty)
        {
            var mon = Rig.ViewRect(tx - 0.32f * K, ty - 0.35f * K, tx + 0.32f * K, ty + 0.58f * K, -3.44f); // the screen, and its bracket under it
            mon = Rect.MinMaxRect(mon.xMin - 1f, mon.yMin - 1f, mon.xMax + 1f, mon.yMax + 1f);
            if (Rig.ConsoleView.HasValue && Rig.ConsoleView.Value.Overlaps(mon)) return false;
            if (boardView.HasValue && boardView.Value.Overlaps(mon)) return false;
            if (Rig.GridView.Overlaps(mon)) return false;
            if (Rig.HiddenOnWall(mon)) return false;
            return !rackX.HasValue || Mathf.Abs(tx - rackX.Value) > 0.8f;
        }
        bool InFrame(float tx, float ty) => Rig.PointInFrame(new Vector3(tx - 0.3f * K, ty + 0.58f * K, -3.44f), 2f) && Rig.PointInFrame(new Vector3(tx + 0.3f * K, ty + 0.58f * K, -3.44f), 2f)
                                            && Rig.PointInFrame(new Vector3(tx, ty - 0.3f * K, -3.44f), 2f);
        // How far a spot's top sticks out of the feed, in degrees (0 when it is inside).
        float Overflow(float tx, float ty)
        {
            float worst = 0f;
            foreach (var p in new[] { new Vector3(tx - 0.3f * K, ty + 0.58f * K, -3.44f), new Vector3(tx + 0.3f * K, ty + 0.58f * K, -3.44f), new Vector3(tx, ty - 0.3f * K, -3.44f) })
                for (float m = 0f; m < 30f && !Rig.PointInFrame(p, -m); m += 1f) worst = Mathf.Max(worst, m + 1f);
            return worst;
        }
        bool placed = false;
        foreach (var (tx, ty) in tries)
            if (InFrame(tx, ty) && Free(tx, ty)) { x = tx; y = ty; placed = true; break; }
        if (!placed && clearOnly) return false;
        if (!placed)
        {
            // No spot fully clear: the one in the feed that the least stands in front of.
            float Overlap(float tx, float ty)
            {
                var mon = Rig.ViewRect(tx - 0.32f * K, ty - 0.35f * K, tx + 0.32f * K, ty + 0.58f * K, -3.44f);
                float area = 0f;
                void Add(Rect o) { float w = Mathf.Min(o.xMax, mon.xMax) - Mathf.Max(o.xMin, mon.xMin), h = Mathf.Min(o.yMax, mon.yMax) - Mathf.Max(o.yMin, mon.yMin); if (w > 0 && h > 0) area += w * h; }
                if (Rig.ConsoleView.HasValue) Add(Rig.ConsoleView.Value);
                Add(Rig.GridView);
                if (boardView.HasValue) Add(boardView.Value);
                foreach (var o in Rig.WallOccluders) Add(o);
                return area / Mathf.Max(1f, mon.width * mon.height);
            }
            float best = float.MaxValue;
            foreach (var (tx, ty) in tries) { if (!InFrame(tx, ty)) continue; float o = Overlap(tx, ty); if (o < best) { best = o; x = tx; y = ty; } }
            if (best == float.MaxValue)
            {
                // Nothing wholly in the feed: the clear spot that sticks out least.
                float least = float.MaxValue;
                foreach (var (tx, ty) in tries) { if (!Free(tx, ty)) continue; float o = Overflow(tx, ty); if (o < least) { least = o; x = tx; y = ty; } }
            }
        }
        var shell = app.world.Monitor(index, new Vector3(x, y, -3.44f), Quaternion.identity);
        // A bracket under it, so it hangs on the wall rather than in the air.
        Building.Box(shell, "Bracket", new Vector3(0.12f, 0.5f, 0.06f), new Vector3(0f, -0.1f, -0.1f), Building.Trim); // in the shell's own (scaled) space
        if (DemoDriver.Wanted)
        {
            var cam = app.world.Camera;
            void Log(string what, Vector3 p) { var v = cam.WorldToViewportPoint(p); Debug.Log($"[Place] {room.id} {what}: local {roomRoot.InverseTransformPoint(p)} viewport {v.x:0.00},{v.y:0.00} z {v.z:0.0}"); }
            Log("monitor", shell.position);
            Log("hero min", hb.min); Log("hero max", hb.max);
            foreach (var t in Rig.Tokens) Log("token " + t.key, t.root.position);
        }
        return true;
    }

    static float Covered(Rect a, Rect b)
    {
        float w = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin), h = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
        return w > 0f && h > 0f ? w * h / Mathf.Max(1e-4f, a.width * a.height) : 0f;
    }

    IEnumerator Open()
    {
        yield return new WaitForSecondsRealtime(app.rehearsed ? 0.8f : 3.2f);
        if (root == null) yield break;
        if (app.rehearsed)
        {
            // Back from the rehearsal: what the attack took, and the thing to fix pulsing.
            app.luma.Cue(root, "live-" + room.number, 40, 924);
            Rig.Pulse(Script.Mistake(room), true);
        }
        else app.luma.Cue(root, "enter-" + room.number, 40, 924);
    }

    void ToggleCamera()
    {
        close = !close;
        if (close) app.world.ShowRoomClose(frame, index, Rig.Bounds);
        else app.world.ShowRoomFeed(frame, index);
    }

    // Something was set: the board of lamps follows, the objective moves on, LUMA reacts once.
    void Changed()
    {
        if (Rig == null) return;
        // Each row's lamp: teal when it is set right (the room's own checks say), amber when not.
        var right = Rig.RowKeys().Select(k => room.RowRight(st, app.session, k)).ToList();
        app.world.ShowConfig(index, right);
        Rig.ShowRight();
        // The progress at the top: a lamp per row on the prop, and one for the console and the pad.
        if (rowLamps != null)
        {
            bool roomDone = (!room.fields.Any(f => f.once) || room.RowRight(st, app.session, "room")) && (!room.signable || st.signed);
            var all = new List<bool>(right);
            // What should be on the board and is not yet: an amber lamp each, so the row never reads as done too early.
            foreach (var i in room.items) if (i.hidden && !i.decoy && !st.added.Contains(i.key)) all.Add(false);
            if (room.fields.Any(f => f.once) || room.signable) all.Add(roomDone);
            for (int i = 0; i < rowLamps.Length; i++) rowLamps[i].color = i < all.Count && all[i] ? Theme.Teal : Theme.Amber;
        }
        if (room.number == 1 && st.added.Contains("shared") && root != null)
        {
            Rig.Pulse(Script.Mistake(room), false);
            bool owned = st.Pick("shared", "owner") > 0, found = room.RowRight(st, app.session, "shared");
            Hud.SetObjective(objective, found ? "Lift the lockdown." : owned ? "Rate it. Then lift the lockdown." : "Set its owner. Then lift the lockdown.");
            if (found && !watched) { watched = true; app.luma.Cue(root, "watch-1", 40, 924); }
        }
        else if (app.rehearsed && Rig.Decided().All(d => d) && Script.Mistake(room).All(k => st.Pick(k, room.fields.FirstOrDefault(f => !f.once)?.key ?? "") >= 0)) Rig.Pulse(Script.Mistake(room), false);
    }

    // Where the camera can be turned: two faint chevrons at the edges of the feed, breathing,
    // until the player first turns it.
    void Turnable()
    {
        var hints = new List<UnityEngine.UI.Image>();
        foreach (int side in new[] { -1, 1 })
        {
            var node = Ui.Node("Turn hint", root);
            Ui.Place(node, side < 0 ? 30 : Ui.W - 30 - 40, Ui.H / 2f - 40, 40, 80);
            foreach (int arm in new[] { -1, 1 })
            {
                var bar = Ui.Box(node, "Arm", new Color(1f, 1f, 1f, 0.5f), false);
                bar.raycastTarget = false;
                var rt = bar.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(6f, 46f);
                rt.anchoredPosition = new Vector2(0f, arm * 15f);
                rt.localRotation = Quaternion.Euler(0f, 0f, side * arm * 40f); // pointing out, the way the camera turns
                hints.Add(bar);
            }
        }
        root.GetComponent<Runner>().StartCoroutine(Breathe(hints));
    }

    IEnumerator Breathe(List<UnityEngine.UI.Image> hints)
    {
        float gone = -1f;
        while (root != null)
        {
            bool turned = Mathf.Abs(app.world.cam.lookYaw) > 3f || Mathf.Abs(app.world.cam.lookPitch) > 3f;
            if (turned && gone < 0f) gone = Time.unscaledTime;
            float a = gone < 0f ? 0.28f + 0.22f * Mathf.Sin(Time.unscaledTime * 2.4f) : Mathf.Max(0f, 0.5f - (Time.unscaledTime - gone) * 0.8f);
            foreach (var h in hints) if (h != null) h.color = new Color(1f, 1f, 1f, a);
            if (gone >= 0f && a <= 0f) { foreach (var h in hints) if (h != null) Object.Destroy(h.transform.parent.gameObject); yield break; }
            yield return null;
        }
    }

    // The walk's fix: the room set right through the rig, as a player would leave it.
    public void AutoFix()
    {
        if (room.ready.Count == 0) return;
        Rig.Apply(room.ready, room.readyOrder, true);
        Changed();
    }

    class Runner : MonoBehaviour { }
}
