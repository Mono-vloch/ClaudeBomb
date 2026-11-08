using System.Collections.Generic;
using UnityEngine;

// One household robot in the market. In the real square (the market kit) it is the
// rigged robot from Higgsfield (AgentRig): its coral panels in the team colour, an antenna
// whose bulb is its brain and whose glow is its effort, a wicker basket on its back that
// fills with what it buys, and its clips (idle, walk, look, pick up, sent home). Without
// the kit it is the robot built in code: a chunky body in its team colour on two wheels, a
// white head with a visor, the antenna and a basket. The root sits on the robot's cell and
// turns the way it walks.
public class RobotModel
{
    public readonly AgentState agent;
    public readonly Transform root;
    public Transform body;
    readonly AgentRig rig;           // the kit's robot, or null for the one built in code
    Transform basket;
    readonly List<GameObject> goods = new List<GameObject>();
    Renderer[] parts;
    Material[] worn;                 // what each part wears when nothing is the matter
    Material antenna;                // its own, so it can pulse
    Renderer[] eyes;
    Transform antennaBall, leftHand;
    readonly List<GameObject> carried = new List<GameObject>(); // what its memory looks like
    readonly Color colour;
    float glow;                      // the antenna at rest: how hard it thinks
    bool ghost, dim, outed;

    public static float Scale => Market.Real ? 1f : 1.25f; // the code-built robot is 1 tall in a 1-unit cell, shown this big
    public AgentRig Rig => rig;                       // the kit's robot, for what the garage hangs on it (null without the kit)
    public Transform Goods => basket;                 // where what it buys goes
    public bool Outed => outed;
    // The basket on its back: off while it lies on the garage table, on when the agent is sent.
    public void ShowBasket(bool on) { if (rig != null && rig.basket != null) rig.basket.gameObject.SetActive(on && !ghost); }
    public void ClearGoods() { foreach (var g in goods) Object.Destroy(g); goods.Clear(); }
    public Vector3 NotePoint => rig != null && rig.noteMount != null ? rig.noteMount.position : body.TransformPoint(new Vector3(0f, 0.45f, 0.2f));
    public Vector3 HipPoint => rig != null && rig.hipMount != null ? rig.hipMount.position : body.TransformPoint(new Vector3(0.25f, 0.4f, 0f));

    public RobotModel(Transform parent, AgentState a, bool dusk)
    {
        agent = a;
        colour = Theme.Team[a.config.colour];
        root = new GameObject("Robot " + a.Name).transform;
        root.SetParent(parent, false);
        var kit = Market.Real ? Market.Kit("Agent") : null;
        if (kit != null)
        {
            var go = Object.Instantiate(kit, root, false);
            go.name = "Agent";
            rig = go.GetComponent<AgentRig>();
            body = go.transform;
            basket = rig.goods;
            rig.SetTeam(colour);
            Wear(a.config);
            return;
        }
        root.localScale = Vector3.one * Scale;
        body = new GameObject("Body").transform;
        body.SetParent(root, false);

        var team = Look.Mat(colour, Color.black, 0.45f);
        var dark = Look.Mat(Look.Dark);
        MeshKit.Part(body, "wheel", MeshKit.Sphere, Look.Mat(Look.Wheel), new Vector3(-0.15f, 0.1f, 0), 0.2f);
        MeshKit.Part(body, "wheel", MeshKit.Sphere, Look.Mat(Look.Wheel), new Vector3(0.15f, 0.1f, 0), 0.2f);
        MeshKit.Part(body, "chassis", MeshKit.Box, dark, new Vector3(0, 0.17f, 0), new Vector3(0.44f, 0.08f, 0.36f));
        MeshKit.Part(body, "shell", MeshKit.Box, team, new Vector3(0, 0.4f, 0), new Vector3(0.46f, 0.34f, 0.36f));
        MeshKit.Part(body, "belly", MeshKit.Box, Look.Mat(Look.Shell), new Vector3(0, 0.4f, 0.185f), new Vector3(0.28f, 0.18f, 0.02f));
        MeshKit.Part(body, "arm", MeshKit.Capsule, team, new Vector3(-0.29f, 0.4f, 0), new Vector3(0, 0, -8), new Vector3(0.1f, 0.3f, 0.1f));
        MeshKit.Part(body, "arm", MeshKit.Capsule, team, new Vector3(0.29f, 0.4f, 0), new Vector3(0, 0, 8), new Vector3(0.1f, 0.3f, 0.1f));
        leftHand = MeshKit.Part(body, "hand", MeshKit.Sphere, Look.Mat(Look.Shell), new Vector3(-0.31f, 0.25f, 0.03f), 0.1f).transform;
        MeshKit.Part(body, "hand", MeshKit.Sphere, Look.Mat(Look.Shell), new Vector3(0.31f, 0.25f, 0.03f), 0.1f);
        MeshKit.Part(body, "head", MeshKit.Box, Look.Mat(Look.Shell, Color.black, 0.5f), new Vector3(0, 0.74f, 0), new Vector3(0.38f, 0.28f, 0.34f));
        MeshKit.Part(body, "visor", MeshKit.Box, Look.Mat(Look.Dark, Color.black, 0.6f), new Vector3(0, 0.76f, 0.175f), new Vector3(0.3f, 0.12f, 0.02f));
        var eyeMat = Look.Mat(Look.Eye, dusk ? Look.Eye * 0.9f : Look.Eye * 0.45f, 0.8f);
        eyes = new[]
        {
            MeshKit.Part(body, "eye", MeshKit.Sphere, eyeMat, new Vector3(-0.07f, 0.76f, 0.19f), 0.065f).GetComponent<Renderer>(),
            MeshKit.Part(body, "eye", MeshKit.Sphere, eyeMat, new Vector3(0.07f, 0.76f, 0.19f), 0.065f).GetComponent<Renderer>(),
        };
        MeshKit.Part(body, "mast", MeshKit.Cylinder, dark, new Vector3(0, 0.93f, 0), new Vector3(0.03f, 0.1f, 0.03f));
        antenna = Look.Own(colour, 0.5f);
        antennaBall = MeshKit.Part(body, "antenna", MeshKit.Sphere, antenna, new Vector3(0, 1.0f, 0), 0.09f).transform;
        basket = MeshKit.Part(body, "basket", MeshKit.Box, Look.Mat(Look.Straw), new Vector3(0, 0.5f, -0.27f), new Vector3(0.3f, 0.2f, 0.2f)).transform;
        MeshKit.Part(basket, "rim", MeshKit.Box, Look.Mat(Look.WoodDark), new Vector3(0, 0.5f, 0), new Vector3(1.08f, 0.12f, 1.08f));

        parts = body.GetComponentsInChildren<Renderer>();
        worn = new Material[parts.Length];
        for (int i = 0; i < parts.Length; i++) worn[i] = parts[i].sharedMaterial;
        Wear(a.config);
    }

    // The dials, on the robot: the brain is the size of its antenna, memory is what
    // it carries (the turns it drags along, or a notebook in its hand), effort is
    // how brightly the antenna glows at rest.
    public void Wear(AgentConfig c)
    {
        if (rig != null) { WearKit(c); return; }
        antennaBall.localScale = Vector3.one * (c.model == 0 ? 0.08f : c.model == 1 ? 0.11f : 0.15f);
        antennaBall.localPosition = new Vector3(0, 0.96f + antennaBall.localScale.y / 2, 0);
        foreach (var g in carried) Object.Destroy(g);
        carried.Clear();
        if (c.memory == MemoryModes.Notebook)
        {
            var book = MeshKit.Part(leftHand, "notebook", MeshKit.Box, Look.Mat(Look.Shell), new Vector3(0, 0.6f, 0.9f), new Vector3(0, -20, 0), new Vector3(1.6f, 2.0f, 0.3f));
            MeshKit.Part(book.transform, "cover", MeshKit.Box, Look.Mat(Look.Dark), new Vector3(0, 0, 0.55f), new Vector3(1.02f, 1.02f, 0.2f));
            carried.Add(book);
        }
        else
        {
            int turns = MemoryModes.Turns(c.memory);
            var card = Look.Glass(new Color(colour.r, colour.g, colour.b, 0.5f));
            for (int i = 0; i < turns; i++)
            {
                var g = MeshKit.Part(body, "turn", MeshKit.Box, card, new Vector3((i % 2 == 0 ? -1 : 1) * 0.04f, 0.62f + i * 0.05f, -0.5f - i * 0.065f), new Vector3(18, 0, 0), new Vector3(0.2f, 0.14f, 0.01f));
                g.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                carried.Add(g);
            }
        }
        glow = c.effort == 0 ? 0f : c.effort == 1 ? 0.3f : 0.7f;
        Think(0);
    }

    // The kit robot's parts (the game-feel pass): what the screen shows of it (Market.Parts) is
    // fitted on it as a thing: the lens on its eye, the brain's chip on its head, the notebook
    // in its right hand; on the garage each carries its plate. The bulb on the antenna is its
    // thinking light: its glow at rest is the effort.
    void WearKit(AgentConfig c)
    {
        rig.bulb.localScale = Vector3.one * 0.06f;
        rig.bulb.localPosition = new Vector3(0f, 0.15f + 0.03f, 0f);
        foreach (var g in carried) Object.Destroy(g);
        carried.Clear();
        FittedBook = null;
        var kitBook = rig.memory != null ? rig.memory.Find("Notebook") : null;
        if (kitBook != null) kitBook.gameObject.SetActive(false);
        var parts = Market.Parts.sight || Market.Parts.memory || Market.Parts.brain;
        if (Market.Parts.sight && rig.faceMount != null)
        {
            var lens = new GameObject("Lens").transform;
            lens.SetParent(rig.faceMount, false);
            lens.localPosition = new Vector3(-0.05f, 0.15f, 0.06f); // over its left eye, on the visor
            lens.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Market.LensShape(lens, c.vision, new Vector3(0f, 0f, -0.012f));
            carried.Add(lens.gameObject);
        }
        if (Market.Parts.brain && rig.headMount != null)
        {
            var chip = Market.ChipShape(rig.headMount, Mathf.Clamp(c.model, 0, 2), Market.Parts.plates);
            chip.localPosition = new Vector3(0f, -0.01f, 0.03f);
            chip.localRotation = Quaternion.Euler(0f, 180f, 0f);
            chip.localScale = Vector3.one * 0.8f; // worn: Opus as wide as the head, Haiku and Sonnet smaller
            carried.Add(chip.gameObject);
        }
        if (Market.Parts.memory && c.memory != MemoryModes.None && rig.handMount != null)
        {
            var book = Market.BookShape(rig.handMount, c.memory, Market.Parts.plates);
            book.localPosition = new Vector3(0.07f, -0.19f, 0.07f);
            book.localRotation = Quaternion.Euler(0f, 180f, 0f);
            FittedBook = book;
            carried.Add(book.gameObject);
        }
        foreach (var g in carried) foreach (var r in g.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        glow = c.effort == 0 ? 0f : c.effort == 1 ? 0.3f : 0.7f;
        Think(0);
    }

    public Transform FittedBook { get; private set; }

    public Vector3 Position { get => root.position; set => root.position = value; }
    public Vector3 Head => rig != null ? rig.HeadTop + Vector3.up * 0.3f : root.position + Vector3.up * 1.12f * Scale;
    public Vector3 Feet => root.position;
    public Vector3 BasketPoint => rig != null ? basket.position + Vector3.up * 0.08f : basket.position + Vector3.up * 0.15f;
    // Where the dials show on it: the brain and the effort on the antenna, the sight at the
    // eyes, the memory on its back.
    public Vector3 AntennaPoint => rig != null ? rig.AntennaPoint : antennaBall.position;
    public Vector3 EyePoint => rig != null ? rig.EyePoint : body.TransformPoint(new Vector3(0.1f, 0.76f, 0.19f));
    public Vector3 BackPoint => rig != null ? rig.BackPoint : basket.position + Vector3.up * 0.12f;

    // The kit robot's clips; the code-built robot bobs instead.
    public void Walk(float speed) { if (rig != null && !outed) rig.Show(AgentRig.Pose.Walk, Mathf.Clamp(speed, 0.6f, 2.2f)); }
    public void PickUp() { if (rig != null) rig.Once(AgentRig.Pose.PickUp, 1.6f, AgentRig.Pose.Idle, false, 1.5f); }
    public void SentHome() { if (rig != null) rig.Once(AgentRig.Pose.SentHome, 2.0f, AgentRig.Pose.SentHome, true, 1.1f); }
    public void LookAround() { if (rig != null && !outed && rig.Current == AgentRig.Pose.Idle && !rig.Busy) rig.Once(AgentRig.Pose.Look, 1.8f, AgentRig.Pose.Idle, false, 1.3f); }

    public static Quaternion Facing(int facing) => Quaternion.Euler(0, facing * 90f, 0); // 0 north (+Z), 1 east, 2 south, 3 west

    // Mid-step: k from 0 to 1. A little hop and a lean forward.
    public void Bob(float k)
    {
        if (rig != null) return;
        body.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2)) * 0.045f, 0);
        body.localRotation = Quaternion.Euler(Mathf.Sin(k * Mathf.PI) * 7f, 0, 0);
    }

    public void Rest()
    {
        if (rig != null) { if (!outed && !rig.Busy) rig.Show(AgentRig.Pose.Idle); return; }
        body.localPosition = Vector3.zero; body.localRotation = Quaternion.identity;
    }

    // The antenna, while the robot is asking Claude: pulse 0 off, 1 full.
    public void Think(float pulse)
    {
        if (outed) return;
        if (rig != null) { rig.SetGlow(Color.Lerp(Look.Lamp * (0.25f + glow * 1.6f), Look.Lamp * 3.5f, pulse * 0.9f)); return; }
        antenna.SetColor("_Emission", Color.Lerp(Look.Lamp * glow, Look.Lamp, pulse * 0.9f));
    }

    // Out of another robot's sight: see-through and grey.
    public void SetGhost(bool on)
    {
        if (ghost == on) return;
        ghost = on;
        if (rig != null) { rig.SetGhost(on); foreach (var g in carried) g.SetActive(!on); return; }
        Dress();
    }

    // Out of tokens: parked, in grey.
    public void SetDim(bool on)
    {
        if (dim == on) return;
        dim = on;
        if (rig != null) { rig.SetDim(on); return; }
        Dress();
    }

    // Sent home: grey, eyes off.
    public void SetOut()
    {
        if (outed) return;
        outed = true;
        if (rig != null) { rig.SetOut(); return; }
        antenna.SetColor("_Emission", Color.black);
        Dress();
        foreach (var e in eyes) e.sharedMaterial = Look.Mat(Theme.Danger);
    }

    void Dress()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (ghost) parts[i].sharedMaterial = Look.Glass(Look.Ghost);
            else if (outed || dim) parts[i].sharedMaterial = worn[i] == antenna ? antenna : Look.Mat(Grey(worn[i].color));
            else parts[i].sharedMaterial = worn[i];
        }
        foreach (var p in parts) p.shadowCastingMode = ghost ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
    }

    static Color Grey(Color c)
    {
        float l = 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
        return Color.Lerp(new Color(l, l, l), Look.Grey, 0.5f);
    }

    // One more thing in the basket: the piece itself in the kit robot's wicker basket.
    public void AddGoods(string id, Color c)
    {
        var prefab = rig != null ? Market.Kit("Pieces/" + id) : null;
        if (prefab == null) { AddGoods(c); return; }
        int n = goods.Count;
        var g = Object.Instantiate(prefab, basket, false);
        g.transform.localPosition = new Vector3((n % 3 - 1) * 0.07f, 0.02f + 0.06f * (n / 6), (n / 3 % 2 == 0 ? -0.04f : 0.04f));
        g.transform.localRotation = Quaternion.Euler(0f, n * 47f, 0f);
        goods.Add(g);
    }

    // One more thing in the basket, as a little ball of its colour.
    public void AddGoods(Color c)
    {
        int n = goods.Count;
        var pos = new Vector3((n % 2 == 0 ? -0.22f : 0.22f), 0.45f + 0.3f * (n / 4), (n / 2 % 2 == 0 ? -0.2f : 0.2f));
        goods.Add(MeshKit.Part(basket, "goods", MeshKit.Sphere, Look.Mat(c), pos, new Vector3(0.45f, 0.65f, 0.65f)));
    }

    public void Destroy() { if (root != null) Object.Destroy(root.gameObject); }
}
