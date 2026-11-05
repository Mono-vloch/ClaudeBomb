using System.Collections.Generic;
using UnityEngine;

// One household robot in the market: a chunky body in its team colour on two
// wheels, a white head with a visor, an antenna that glows while it is asking
// Claude, and a basket on its back that fills with what it buys. The root sits
// on the robot's cell and turns the way it walks; the body bobs on top.
public class RobotModel
{
    public readonly AgentState agent;
    public readonly Transform root, body;
    readonly Transform basket;
    readonly List<GameObject> goods = new List<GameObject>();
    readonly Renderer[] parts;
    readonly Material[] worn;        // what each part wears when nothing is the matter
    readonly Material antenna;       // its own, so it can pulse
    readonly Renderer[] eyes;
    readonly Transform antennaBall, leftHand;
    readonly List<GameObject> carried = new List<GameObject>(); // what its memory looks like
    readonly Color colour;
    float glow;                      // the antenna at rest: how hard it thinks
    bool ghost, dim, outed;

    public const float Scale = 1.25f; // a cell is 1; the robot is built 1 tall and shown this big

    public RobotModel(Transform parent, AgentState a, bool dusk)
    {
        agent = a;
        colour = Theme.Team[a.config.colour];
        root = new GameObject("Robot " + a.Name).transform;
        root.SetParent(parent, false);
        root.localScale = Vector3.one * Scale;
        body = new GameObject("Body").transform;
        body.SetParent(root, false);

        var team = Look.Mat(colour);
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
        MeshKit.Part(body, "head", MeshKit.Box, Look.Mat(Look.Shell), new Vector3(0, 0.74f, 0), new Vector3(0.38f, 0.28f, 0.34f));
        MeshKit.Part(body, "visor", MeshKit.Box, dark, new Vector3(0, 0.76f, 0.175f), new Vector3(0.3f, 0.12f, 0.02f));
        var eyeMat = Look.Mat(Look.Eye, dusk ? Look.Eye * 0.6f : Look.Eye * 0.25f);
        eyes = new[]
        {
            MeshKit.Part(body, "eye", MeshKit.Sphere, eyeMat, new Vector3(-0.07f, 0.76f, 0.19f), 0.065f).GetComponent<Renderer>(),
            MeshKit.Part(body, "eye", MeshKit.Sphere, eyeMat, new Vector3(0.07f, 0.76f, 0.19f), 0.065f).GetComponent<Renderer>(),
        };
        MeshKit.Part(body, "mast", MeshKit.Cylinder, dark, new Vector3(0, 0.93f, 0), new Vector3(0.03f, 0.1f, 0.03f));
        antenna = Look.Own(colour);
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

    public Vector3 Position { get => root.position; set => root.position = value; }
    public Vector3 Head => root.position + Vector3.up * 1.12f * Scale;
    public Vector3 Feet => root.position;
    public Vector3 BasketPoint => basket.position + Vector3.up * 0.15f;

    public static Quaternion Facing(int facing) => Quaternion.Euler(0, facing * 90f, 0); // 0 north (+Z), 1 east, 2 south, 3 west

    // Mid-step: k from 0 to 1. A little hop and a lean forward.
    public void Bob(float k)
    {
        body.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2)) * 0.045f, 0);
        body.localRotation = Quaternion.Euler(Mathf.Sin(k * Mathf.PI) * 7f, 0, 0);
    }

    public void Rest() { body.localPosition = Vector3.zero; body.localRotation = Quaternion.identity; }

    // The antenna, while the robot is asking Claude: pulse 0 off, 1 full.
    public void Think(float pulse)
    {
        if (outed) return;
        antenna.SetColor("_Emission", Color.Lerp(Look.Lamp * glow, Look.Lamp, pulse * 0.9f));
    }

    // Out of another robot's sight: see-through and grey.
    public void SetGhost(bool on)
    {
        if (ghost == on) return;
        ghost = on;
        Dress();
    }

    // Out of tokens: parked, in grey.
    public void SetDim(bool on)
    {
        if (dim == on) return;
        dim = on;
        Dress();
    }

    // Sent home: grey, eyes off.
    public void SetOut()
    {
        if (outed) return;
        outed = true;
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

    // One more thing in the basket, as a little ball of its colour.
    public void AddGoods(Color c)
    {
        int n = goods.Count;
        var pos = new Vector3((n % 2 == 0 ? -0.22f : 0.22f), 0.45f + 0.3f * (n / 4), (n / 2 % 2 == 0 ? -0.2f : 0.2f));
        goods.Add(MeshKit.Part(basket, "goods", MeshKit.Sphere, Look.Mat(c), pos, new Vector3(0.45f, 0.65f, 0.65f)));
    }

    public void Destroy() { if (root != null) Object.Destroy(root.gameObject); }
}
