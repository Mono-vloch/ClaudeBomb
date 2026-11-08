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

    public void BuildBattery(RobotModel bot, int notchCount)
    {
        if (bot == null) return;
        if (batteries.TryGetValue(bot, out var old)) { if (old.root != null) Destroy(old.root.gameObject); batteries.Remove(bot); }
        var rig = bot.Rig;
        Transform parent = rig != null && rig.hipMount != null ? rig.hipMount : bot.body;
        var b = new Battery();
        b.root = new GameObject("Battery").transform;
        b.root.SetParent(parent, false);
        if (rig == null) { b.root.localPosition = new Vector3(0.26f, 0.42f, 0f); b.root.localRotation = Quaternion.Euler(0f, 90f, 0f); }
        var dark = new Color(0.12f, 0.13f, 0.17f);
        Box(b.root, new Vector3(0.15f, 0.17f, 0.045f), new Vector3(0f, 0f, 0.03f), Metal, dark, "Pack");
        Box(b.root, new Vector3(0.1f, 0.012f, 0.03f), new Vector3(0f, 0.1f, 0.03f), Metal, new Color(0.05f, 0.05f, 0.07f), "Slot");
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

    // ---- the clock on the square: dinner time ----

    Transform clockRoot, hourHand, minuteHand;
    Renderer clockFaceR;

    void BuildClock()
    {
        clockRoot = new GameObject("Clock").transform;
        clockRoot.SetParent(transform, false);
        clockRoot.position = new Vector3(-Size * Pitch / 2f - 1.2f, 0f, Size * Pitch / 2f + 1.2f);
        Box(clockRoot, new Vector3(0.14f, 4.4f, 0.14f), new Vector3(0f, 2.2f, 0f), Metal, new Color(0.25f, 0.26f, 0.3f), "Post");
        var head = new GameObject("Head").transform; head.SetParent(clockRoot, false);
        head.localPosition = new Vector3(0f, 4.6f, 0f);
        head.gameObject.AddComponent<Billboard>().target = cam.transform;
        var rim = Disc(head, 0.72f, new Vector3(0f, 0f, 0.02f), Metal, new Color(0.3f, 0.32f, 0.36f), "Rim");
        var face = Disc(head, 0.64f, Vector3.zero, Stone, Ink3D, "Face");
        clockFaceR = face.GetComponent<Renderer>();
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f * Mathf.Deg2Rad;
            var tick = Box(head, new Vector3(i % 3 == 0 ? 0.05f : 0.03f, i % 3 == 0 ? 0.12f : 0.07f, 0.01f), new Vector3(Mathf.Sin(a) * 0.54f, Mathf.Cos(a) * 0.54f, -0.012f), LampMat, Color.white, "Tick");
            tick.transform.localRotation = Quaternion.Euler(0f, 0f, -i * 30f);
        }
        var hour = Box(head, new Vector3(0.06f, 0.36f, 0.015f), Vector3.zero, LampMat, Color.white, "Hour");
        hour.GetComponent<MeshFilter>().sharedMesh = BoxMesh(new Vector3(0.06f, 0.36f, 0.015f));
        hourHand = new GameObject("HourPivot").transform; hourHand.SetParent(head, false); hourHand.localPosition = new Vector3(0f, 0f, -0.02f);
        hour.transform.SetParent(hourHand, false); hour.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        var minute = Box(head, new Vector3(0.04f, 0.52f, 0.015f), Vector3.zero, LampMat, Theme.Teal, "Minute");
        minuteHand = new GameObject("MinutePivot").transform; minuteHand.SetParent(head, false); minuteHand.localPosition = new Vector3(0f, 0f, -0.03f);
        minute.transform.SetParent(minuteHand, false); minute.transform.localPosition = new Vector3(0f, 0.26f, 0f);
        Glow(minute.GetComponent<Renderer>(), Theme.Teal * 0.8f);
        var lamp = new GameObject("Lamp").AddComponent<Light>();
        lamp.transform.SetParent(head, false); lamp.transform.localPosition = new Vector3(0f, 0f, -0.6f);
        lamp.type = LightType.Point; lamp.color = new Color(1f, 0.95f, 0.85f); lamp.intensity = dusk ? 2.5f : 0f; lamp.range = 2.5f; lamp.shadows = LightShadows.None;
        SetClock(0f, false);
    }

    // fraction: 0 at six o'clock, 1 at seven; amber in the last turns.
    public void SetClock(float fraction, bool amber)
    {
        if (clockRoot == null) return;
        fraction = Mathf.Clamp01(fraction);
        if (minuteHand != null) minuteHand.localRotation = Quaternion.Euler(0f, 0f, -fraction * 360f);
        if (hourHand != null) hourHand.localRotation = Quaternion.Euler(0f, 0f, -(180f + fraction * 30f));
        if (clockFaceR != null) Tint(clockFaceR, amber ? new Color(0.32f, 0.2f, 0.05f) : Ink3D);
        var minute = minuteHand != null ? minuteHand.GetComponentInChildren<Renderer>() : null;
        if (minute != null) { Tint(minute, amber ? Theme.Amber : Theme.Teal); Glow(minute, (amber ? Theme.Amber : Theme.Teal) * 0.8f); }
    }

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

    public Transform Podium(int medal, Color colour, AgentState hero)
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
        var stone = Stone; var metal = Metal;
        float[] heights = { 0.36f, 0.24f, 0.15f };
        float[] widths = { 1.0f, 0.64f, 0.64f };
        float[] xs = { 0f, -0.84f, 0.84f };
        var faceDark = new Color(0.1f, 0.14f, 0.26f);
        for (int i = 0; i < 3; i++)
        {
            float w = widths[i], h = heights[i];
            Box(podium, new Vector3(w, h, 1.0f), new Vector3(xs[i], h / 2f, 0f), stone, faceDark);
            Box(podium, new Vector3(w, 0.02f, 1.0f), new Vector3(xs[i], h + 0.01f, 0f), metal, new Color(0.7f, 0.74f, 0.8f));
            var num = new GameObject("Step").AddComponent<TextMeshPro>();
            num.transform.SetParent(podium, false);
            num.transform.localPosition = new Vector3(xs[i], h * 0.5f, -0.51f);
            num.font = Theme.Bold; num.fontSize = i == 0 ? 2.4f : 1.8f; num.color = new Color(0.8f, 0.84f, 0.9f); num.alignment = TextAlignmentOptions.Center;
            num.rectTransform.sizeDelta = new Vector2(0.4f, 0.3f);
            num.text = (i + 1).ToString();
        }
        int step = medal > 0 ? 3 - medal : 0;
        // The agent on its step, turned a little so the basket on its back shows; beside the
        // podium when there is no medal.
        if (medal > 0) r.Position = podium.position + podium.TransformDirection(new Vector3(xs[step], heights[step] + 0.005f, -0.12f));
        else r.Position = podium.position + podium.TransformDirection(new Vector3(-1.5f, 0f, -0.3f));
        r.root.rotation = Quaternion.LookRotation(-podiumFront) * Quaternion.Euler(0f, 145f, 0f);
        r.Rest();
        if (medal > 0)
        {
            // The medal: a ribbon post rising from the step behind the agent, the disc at its top, the stars in an arc over it.
            var m = new GameObject("Medal").transform;
            m.SetParent(podium, false);
            m.localPosition = new Vector3(xs[step] + (step == 0 ? 0.47f : 0.3f), heights[step] + 0.02f, 0.25f);
            var ribbon = new Color(0.16f, 0.2f, 0.42f);
            float rise = 0.85f;
            foreach (float sx in new[] { -1f, 1f })
            {
                var strip = Box(m, new Vector3(0.05f, rise * 1.05f, 0.012f), new Vector3(sx * 0.05f, rise / 2f, 0f), stone, ribbon);
                strip.transform.localRotation = Quaternion.Euler(0f, 0f, -sx * 6f);
            }
            float discY = rise + 0.12f;
            var back = Disc(m, 0.15f, new Vector3(0f, discY, 0.01f), LampMat, colour * 0.72f);
            var front = Disc(m, 0.125f, new Vector3(0f, discY, -0.005f), LampMat, colour);
            Glow(front.GetComponent<Renderer>(), colour * 0.7f);
            Star(m, 0.05f, new Vector3(0f, discY, -0.02f), colour * 1.25f);
            for (int i = 0; i < 3; i++)
            {
                var star = Star(m, 0.045f, new Vector3((i - 1) * 0.14f, discY + 0.24f - Mathf.Abs(i - 1) * 0.04f, -0.02f), i < medal ? Theme.Amber : new Color(0.3f, 0.33f, 0.4f));
                star.transform.localScale = Vector3.zero;
                star.name = "Star" + i;
            }
            var lamp = new GameObject("Medal lamp").AddComponent<Light>();
            lamp.transform.SetParent(m, false); lamp.transform.localPosition = new Vector3(0f, discY, -0.5f);
            lamp.type = LightType.Point; lamp.color = colour; lamp.intensity = 2.2f; lamp.range = 1.6f; lamp.shadows = LightShadows.None;
            m.localScale = new Vector3(1f, 0f, 1f); // it rises
        }
        // The debrief's camera: the door behind, the agent on the podium with its basket, from a little to the left.
        var centre = podium.position + Vector3.up * 0.7f + right * 0.1f;
        doorCamAt = centre + inward * 3.4f - right * 1.1f + Vector3.up * 0.75f;
        doorCamLook = centre;
        return podium;
    }

    // What the agent brought home, tipped out on the step in front of it: the wrong thing glows red.
    public void TipOut(AgentState hero, System.Func<string, bool> wanted)
    {
        if (podium == null || !bots.TryGetValue(hero, out var r)) return;
        var items = hero.eaten.Take(8).ToList();
        if (items.Count == 0) return;
        var front = r.Position + podiumFront * 0.32f;
        var across = Vector3.Cross(Vector3.up, podiumFront);
        for (int i = 0; i < items.Count; i++)
        {
            var piece = Piece(items[i], Vector3.zero);
            piece.SetParent(podium, true);
            piece.localScale = Vector3.one * 1.5f;
            piece.position = front + across * ((i - (items.Count - 1) / 2f) * 0.13f) + Vector3.up * 0.002f;
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

    // A number that is the lesson stays in the world: the receipt by the basket.
    public Transform Receipt(string text, AgentState hero)
    {
        if (!bots.TryGetValue(hero, out var r)) return null;
        var right = Vector3.Cross(Vector3.up, -Inward(hero.home));
        var plate = WorldPlate(text, r.Position - right * 0.85f + Vector3.up * 0.75f, 1.0f, Theme.Amber);
        plate.name = "Receipt";
        return plate;
    }

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
