using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// What the world shows during the errand and after it (the game-feel pass): the battery on
// every agent's hip draining notch by notch, the market clock running toward dinner, the
// house with its lit window for the briefing and the debrief, the podium with the medal, and
// the props of the four story shots.
public partial class Market
{
    // ---- the battery: the pocket money, as a ring of notches on a pack at the hip ----

    class Battery { public Transform root; public readonly List<Renderer> notches = new List<Renderer>(); public float shown = 1f; }
    readonly Dictionary<RobotModel, Battery> batteries = new Dictionary<RobotModel, Battery>();

    // left: on the left hip (the garage's camera sees that side); plate: its name on the pack.
    public void BuildBattery(RobotModel bot, int notchCount, bool left = false, bool plate = false)
    {
        if (bot == null) return;
        if (batteries.TryGetValue(bot, out var old)) { if (old.root != null) Destroy(old.root.gameObject); batteries.Remove(bot); }
        var rig = bot.Rig;
        Transform parent = rig != null && (left ? rig.hipLeftMount : rig.hipMount) != null ? (left ? rig.hipLeftMount : rig.hipMount) : bot.body;
        var b = new Battery();
        b.root = new GameObject("Battery").transform;
        b.root.SetParent(parent, false);
        if (rig == null) { b.root.localPosition = new Vector3(left ? -0.26f : 0.26f, 0.42f, 0f); b.root.localRotation = Quaternion.Euler(0f, left ? -90f : 90f, 0f); }
        var dark = new Color(0.12f, 0.13f, 0.17f);
        float lower = plate ? 0.05f : 0f; // the pack runs longer under the ring for its plate
        Box(b.root, new Vector3(0.2f, 0.17f + lower * 2f, 0.045f), new Vector3(0f, -lower, 0.03f), Metal, dark, "Pack");
        Box(b.root, new Vector3(0.1f, 0.012f, 0.03f), new Vector3(0f, 0.1f, 0.03f), Metal, new Color(0.05f, 0.05f, 0.07f), "Slot");
        if (plate) PlateOn(b.root, "Battery", 0.34f, new Vector3(0f, -0.11f, 0.056f), Vector3.forward);
        var face = Disc(b.root, 0.07f, new Vector3(0f, -0.005f, 0.057f), Stone, Ink3D, "Face");
        face.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        int n = Mathf.Clamp(notchCount, 6, 60);
        for (int i = 0; i < n; i++)
        {
            float a = -Mathf.PI / 2f + (i + 0.5f) / n * Mathf.PI * 2f; // from the top, clockwise as seen from outside
            var at = new Vector3(Mathf.Cos(a) * 0.058f, -0.005f + Mathf.Sin(a) * 0.058f, 0.06f);
            var notch = Box(b.root, new Vector3(0.006f, 0.016f, 0.004f), at, LampMat, Theme.Teal, "Notch");
            notch.transform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f);
            notch.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            b.notches.Add(notch.GetComponent<Renderer>());
        }
        batteries[bot] = b;
        SetBattery(bot, 1f);
    }

    // How far the money goes, 0 to 1: the notches light clockwise from the top.
    public void SetBattery(RobotModel bot, float fraction)
    {
        if (bot == null || !batteries.TryGetValue(bot, out var b) || b.root == null) return;
        fraction = Mathf.Clamp01(fraction);
        int lit = Mathf.RoundToInt(fraction * b.notches.Count);
        var low = fraction < 0.2f;
        for (int i = 0; i < b.notches.Count; i++)
        {
            bool on = i < lit;
            var c = on ? (low ? Theme.Amber : Theme.Teal) : new Color(0.2f, 0.22f, 0.27f);
            Tint(b.notches[i], c);
            Glow(b.notches[i], on ? c * 1.3f : Color.black);
        }
    }

    public void SetBattery(AgentState a, float fraction) { if (bots.TryGetValue(a, out var r)) SetBattery(r, fraction); }
    public void BatteriesFor(int turns) { foreach (var kv in bots) BuildBattery(kv.Value, turns); }
    public Transform BatteryOf(AgentState a) => a != null && bots.TryGetValue(a, out var r) && batteries.TryGetValue(r, out var b) ? b.root : null;

    // ---- the house: the briefing's feed, the debrief's door ----

    // Every agent at its door; the hero a step in front, facing the house or the square.
    public void PlaceAtDoor(AgentState a, bool facingHouse, float step = 0.5f)
    {
        if (!bots.TryGetValue(a, out var r)) return;
        var inward = Inward(a.home);
        r.Position = Cell(a.home) + inward * step;
        r.root.rotation = Quaternion.LookRotation(facingHouse ? -inward : inward);
        r.Rest();
    }

    // A warm lamp at the door: the family waits at the lit window.
    public Light DoorLamp(AgentState a, float intensity = 3f)
    {
        var go = new GameObject("Door lamp");
        go.transform.SetParent(transform, false);
        go.transform.position = DoorOf(a.home) + Vector3.up * 1.7f + Inward(a.home) * 0.4f;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.8f, 0.5f); l.intensity = intensity; l.range = 5.5f; l.shadows = LightShadows.None;
        return l;
    }

    // The house from the square, the agent at its door, the door centred in the frame.
    public void ShowHouse(RectTransform frame, AgentState a, float distance = 5.2f, float fov = 38f, bool onAgent = false)
    {
        this.frame = frame;
        view = View.Market;
        Canvas.ForceUpdateCanvases();
        var H = Cell(a.home); var inward = Inward(a.home);
        var right = Vector3.Cross(Vector3.up, -inward);
        var at = H + inward * distance + right * (onAgent ? 0.7f : 1.1f) + Vector3.up * (onAgent ? 1.25f : 1.55f);
        var look = onAgent && bots.TryGetValue(a, out var bot) ? bot.Position + Vector3.up * 0.72f : DoorOf(a.home) + Vector3.up * 1.05f;
        cam.Fixed(frame, at, look, fov, true, 0.8f);
        cam.Snap();
        ApplyFog();
    }

    // ---- the podium beside the door, with the medal and its stars ----

    Transform podium; Vector3 podiumFront; Vector3 doorCamAt, doorCamLook;

    // Robot Racing's podium and ribbon medal (Circuit.Podium), built from this kit's materials:
    // three steps numbered 1, 2, 3 when several agents ran (the next two on steps 2 and 3), one
    // step when one did. The medal hangs on its ribbon over the front of the hero's step, its three
    // stars along the ribbon. A number that is the lesson (1.5's cost per item) stands on the
    // step's face beside it.
    public Transform Podium(int medal, Color colour, AgentState hero, IList<AgentState> runnersUp = null, string onStep = null)
    {
        if (podium != null) Destroy(podium.gameObject);
        if (!bots.TryGetValue(hero, out var r)) return null;
        var inward = Inward(hero.home);
        var right = Vector3.Cross(Vector3.up, -inward); // the camera's right, looking at the house
        podiumFront = inward;
        podium = new GameObject("Podium").transform;
        podium.SetParent(transform, false);
        podium.position = Cell(hero.home) + inward * 0.9f + right * 0.3f;
        podium.rotation = Quaternion.LookRotation(-podiumFront);
        bool three = runnersUp != null && runnersUp.Count > 0;
        float[] heights = { 0.36f, 0.24f, 0.15f };
        float[] widths = { 1.1f, 0.7f, 0.7f };
        float[] xs = { 0f, -0.92f, 0.92f };
        var faceDark = new Color(0.1f, 0.14f, 0.26f);
        for (int i = 0; i < (three ? 3 : 1); i++)
        {
            float w = widths[i], h = heights[i];
            Box(podium, new Vector3(w, h, 1.1f), new Vector3(xs[i], h / 2f, 0f), Stone, faceDark);
            Box(podium, new Vector3(w, 0.02f, 1.1f), new Vector3(xs[i], h + 0.01f, 0f), Metal, new Color(0.7f, 0.74f, 0.8f));
            if (!three || (i == 0 && onStep != null)) continue;
            var num = new GameObject("Step").AddComponent<TextMeshPro>();
            num.transform.SetParent(podium, false);
            num.transform.localPosition = new Vector3(xs[i] + (i == 0 ? 0.36f : 0f), h * 0.5f, -0.56f);
            num.font = Theme.Bold; num.fontSize = i == 0 ? 2.4f : 1.8f; num.color = new Color(0.8f, 0.84f, 0.9f); num.alignment = TextAlignmentOptions.Center;
            num.rectTransform.sizeDelta = new Vector2(0.4f, 0.3f);
            num.text = (i + 1).ToString();
        }
        // The hero on step 1, turned a little so the basket on its back shows; the runners-up on 2 and 3.
        void Stand(RobotModel bot, int step)
        {
            bot.Position = podium.position + podium.TransformDirection(new Vector3(xs[step], heights[step] + 0.005f, 0.05f));
            bot.root.rotation = Quaternion.LookRotation(-podiumFront) * Quaternion.Euler(0f, 145f, 0f);
            bot.Rest();
        }
        Stand(r, 0);
        if (three)
            for (int i = 0; i < runnersUp.Count && i < 2; i++)
                if (bots.TryGetValue(runnersUp[i], out var other)) Stand(other, i + 1);
        if (onStep != null)
        {
            var plate = PlateOn(podium, onStep, 0.7f, new Vector3(0.3f, heights[0] * 0.5f, -0.556f), Vector3.back);
            plate.name = "Receipt";
            plate.GetComponentInChildren<TextMeshPro>().color = Theme.Amber;
        }
        if (medal > 0)
        {
            // The medal: a ribbon from the step's edge, the disc at its end, the stars on the ribbon.
            int step = 0;
            var m = new GameObject("Medal").transform;
            m.SetParent(podium, false);
            m.localPosition = new Vector3(xs[step] - 0.08f, heights[step] + 0.02f, -0.56f);
            var ribbon = new Color(0.16f, 0.2f, 0.42f);
            float drop = heights[step] * 0.55f;
            foreach (float sx in new[] { -1f, 1f })
            {
                var strip = Box(m, new Vector3(0.05f, drop * 1.08f, 0.012f), new Vector3(sx * 0.06f, -drop / 2f, -0.012f), Stone, ribbon);
                strip.transform.localRotation = Quaternion.Euler(0f, 0f, sx * 12f);
            }
            float discY = -drop - 0.09f;
            Disc(m, 0.115f, new Vector3(0f, discY, -0.02f), LampMat, colour * 0.72f);
            var disc = Disc(m, 0.095f, new Vector3(0f, discY, -0.026f), LampMat, colour);
            Glow(disc.GetComponent<Renderer>(), colour * 0.25f);
            Star(m, 0.04f, new Vector3(0f, discY, -0.032f), colour * 1.25f);
            for (int i = 0; i < 3; i++)
            {
                var star = Star(m, 0.03f, new Vector3((i - 1) * 0.055f, -drop * 0.45f - Mathf.Abs(i - 1) * 0.02f, -0.03f), i < medal ? Theme.Amber : new Color(0.3f, 0.33f, 0.4f));
                star.transform.localScale = Vector3.zero;
                star.name = "Star" + i;
            }
            var lamp = new GameObject("Medal lamp").AddComponent<Light>();
            lamp.transform.SetParent(m, false); lamp.transform.localPosition = new Vector3(0f, discY, -0.5f);
            lamp.type = LightType.Point; lamp.color = colour; lamp.intensity = 2f; lamp.range = 1.4f; lamp.shadows = LightShadows.None;
            m.localScale = new Vector3(1f, 0f, 1f); // it drops into place
        }
        // The debrief's camera: the door behind, the agent on the podium with its basket, from a little to the left.
        var centre = podium.position + Vector3.up * 0.7f + right * 0.1f;
        doorCamAt = centre + inward * 3.6f - right * 1.1f + Vector3.up * 0.65f;
        doorCamLook = centre;
        return podium;
    }

    // What the agent brought home, tipped out on the step in front of it: the wrong thing glows red.
    public void TipOut(AgentState hero, System.Func<string, bool> wanted)
    {
        if (podium == null || !bots.TryGetValue(hero, out var r)) return;
        var items = hero.eaten.Take(8).ToList();
        if (items.Count == 0) return;
        var across = -Vector3.Cross(Vector3.up, podiumFront); // the camera's right
        var front = r.Position + podiumFront * 0.36f + across * 0.18f;
        for (int i = 0; i < items.Count; i++)
        {
            var piece = Piece(items[i], Vector3.zero);
            piece.SetParent(podium, true);
            piece.localScale = Vector3.one * 1.5f;
            piece.position = front + across * ((i - (items.Count - 1) / 2f) * 0.11f) + Vector3.up * 0.002f;
            piece.rotation = Quaternion.Euler(0f, i * 53f, 0f);
            if (!wanted(items[i]))
            {
                var l = new GameObject("Wrong").AddComponent<Light>();
                l.transform.SetParent(piece, false); l.transform.localPosition = Vector3.up * 0.12f;
                l.type = LightType.Point; l.color = Theme.Danger; l.intensity = 3f; l.range = 0.6f; l.shadows = LightShadows.None;
            }
        }
    }

    // The medal rises on its ribbon, then the stars pop one by one.
    public IEnumerator RaisePodium(int medal)
    {
        if (podium == null) yield break;
        var m = podium.Find("Medal");
        if (m == null) yield break;
        float t = 0;
        while (t < 0.6f && m != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / 0.6f); k = 1f - (1f - k) * (1f - k); m.localScale = new Vector3(1f, k, 1f); yield return null; }
        for (int i = 0; i < 3 && m != null; i++)
        {
            var star = m.Find("Star" + i);
            if (star == null) continue;
            if (i < medal) Sfx.Chime();
            float u = 0;
            while (u < 0.22f && star != null) { u += Time.unscaledDeltaTime; star.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, Ui.EaseOutBack(Mathf.Clamp01(u / 0.22f))); yield return null; }
            if (star != null) star.localScale = Vector3.one;
            if (i < medal && medal >= 2 && effects != null) effects.Confetti(podium.position + Vector3.up * 1.2f, 18);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void ShowDoorView(RectTransform frame)
    {
        this.frame = frame;
        view = View.Market;
        Canvas.ForceUpdateCanvases();
        if (rig != null) rig.Release();
        cam.Fixed(frame, doorCamAt, doorCamLook, 40f, false, 0.7f);
        cam.Snap();
        ApplyFog();
    }

    // ---- the family's spot at the door ----

    // Where the family stands at the door of the house behind an agent's home: in front of the
    // nearest house of the square's ring, a doormat on the cobbles. A model at
    // Resources/Market/Family (origin at its feet, facing +Z) is stood there when it exists.
    Transform familySpot;
    public Transform FamilySpot(AgentState a)
    {
        if (familySpot != null) return familySpot;
        if (!Real || ground == null) return null;
        var inward = Inward(a.home);
        var behind = Cell(a.home) - inward * 7.3f;
        Transform best = null; float bestD = float.MaxValue;
        var square = ground.Find("Square");
        if (square == null) return null;
        foreach (Transform h in square)
        {
            if (!h.name.StartsWith("House")) continue;
            float d = (Flat(h.position) - Flat(behind)).sqrMagnitude;
            if (d < bestD) { bestD = d; best = h; }
        }
        if (best == null) return null;
        var bounds = new Bounds(best.position, Vector3.zero);
        foreach (var r in best.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
        float half = Mathf.Abs(Vector3.Dot(bounds.extents, new Vector3(Mathf.Abs(inward.x), 0f, Mathf.Abs(inward.z))));
        var at = Flat(bounds.center) + inward * (half + 0.45f);
        familySpot = new GameObject("Family spot").transform;
        familySpot.SetParent(transform, false);
        familySpot.position = at;
        familySpot.rotation = Quaternion.LookRotation(inward);
        var mat = Box(familySpot, new Vector3(0.9f, 0.012f, 0.55f), new Vector3(0f, 0.006f, 0f), Stone, new Color(0.42f, 0.16f, 0.12f), "Doormat");
        mat.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var model = Kit("Family");
        if (model != null)
        {
            // Mom (Higgsfield, Meshy 7), at rest on the doormat; a porch lamp over the door lights her at dusk.
            var f = Instantiate(model, familySpot, false); f.name = "Family";
            var porch = new GameObject("Porch lamp").AddComponent<Light>();
            porch.transform.SetParent(familySpot, false);
            porch.transform.localPosition = new Vector3(0.3f, 2.4f, 0.5f);
            porch.type = LightType.Point; porch.color = new Color(1f, 0.82f, 0.56f); porch.intensity = dusk ? 1.8f : 0.6f; porch.range = 3.6f; porch.shadows = LightShadows.None;
        }
        return familySpot;
    }

    // The agent is home: Mom waves once, then rests.
    public void FamilyWave(float delay = 0f)
    {
        var mom = familySpot != null ? familySpot.GetComponentInChildren<MomRig>() : null;
        if (mom != null) mom.Wave(delay);
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // ---- the story's props ----

    // A phone lying by the door, its screen dark until the message arrives.
    public Transform StoryPhone(AgentState a)
    {
        var inward = Inward(a.home);
        var root = new GameObject("Story phone").transform;
        root.SetParent(transform, false);
        root.position = Cell(a.home) + inward * 0.75f + Vector3.Cross(Vector3.up, -inward) * 0.4f + Vector3.up * 0.16f;
        root.rotation = Quaternion.LookRotation(-inward) * Quaternion.Euler(0f, 25f, 0f);
        Box(root, new Vector3(0.1f, 0.012f, 0.2f), new Vector3(0f, 0.006f, 0f), Metal, new Color(0.08f, 0.08f, 0.11f), "Body");
        var screen = Box(root, new Vector3(0.088f, 0.002f, 0.18f), new Vector3(0f, 0.013f, 0f), LampMat, new Color(0.1f, 0.12f, 0.16f), "Screen");
        Glow(screen.GetComponent<Renderer>(), Color.black);
        var l = new GameObject("Glow").AddComponent<Light>();
        l.transform.SetParent(root, false); l.transform.localPosition = Vector3.up * 0.12f;
        l.type = LightType.Point; l.color = new Color(0.6f, 0.75f, 1f); l.range = 0.9f; l.intensity = 0f; l.shadows = LightShadows.None;
        return root;
    }

    public void PhoneScreen(Transform phone, bool on)
    {
        if (phone == null) return;
        var screen = phone.Find("Screen");
        if (screen != null) { var r = screen.GetComponent<Renderer>(); Tint(r, on ? new Color(0.75f, 0.88f, 1f) : new Color(0.1f, 0.12f, 0.16f)); Glow(r, on ? new Color(0.45f, 0.65f, 1f) * 1.6f : Color.black); }
        var l = phone.GetComponentInChildren<Light>(true);
        if (l != null) l.intensity = on ? 2.4f : 0f;
    }

    // A side view of the agent and the cell in front of it: the story's tomato.
    public void SideView(RectTransform frame, AgentState a, Vector2Int cell)
    {
        if (!bots.TryGetValue(a, out var r)) return;
        this.frame = frame; view = View.Market;
        if (rig != null) rig.Release();
        var mid = (r.Position + Cell(cell)) / 2f;
        var along = (Cell(cell) - r.Position).normalized;
        var side = Vector3.Cross(Vector3.up, along);
        cam.Fixed(frame, mid + side * 3.4f + Vector3.up * 1.4f - along * 0.4f, mid + Vector3.up * 0.75f, 40f, false, 0.3f);
        cam.Snap();
        ApplyFog();
    }

    // The agent walks into the square from its door, the camera at its shoulder, and looks around.
    public IEnumerator WalkInto(AgentState a, int cells, RectTransform frame)
    {
        if (!bots.TryGetValue(a, out var r)) yield break;
        var inward = Inward(a.home);
        r.root.rotation = Quaternion.LookRotation(inward);
        this.frame = frame; view = View.Street;
        Highlight(a);
        cam.Follow(r.root, frame); cam.Snap();
        var from = r.Position; var to = from + inward * Pitch * cells;
        r.Walk(1.0f);
        float t = 0f, d = 2.2f * cells;
        while (t < d) { t += Time.deltaTime; r.Position = Vector3.Lerp(from, to, t / d); yield return null; }
        r.Position = to;
        a.pos += new Vector2Int(Mathf.RoundToInt(inward.x), -Mathf.RoundToInt(inward.z)) * cells;
        r.Rest();
        yield return new WaitForSeconds(0.3f);
        r.LookAround();
    }
}
