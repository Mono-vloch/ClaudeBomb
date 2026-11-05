using UnityEngine;

// The fruit and vegetables on the stalls, one small model each, in the colours
// of their 2D sprites (SpriteBaker). Make() builds one piece standing on its
// origin, about 0.3 tall; a stall shows a few.
public static class ItemModels
{
    static readonly Color red = Theme.Hex("#E6413A"), yellow = Theme.Hex("#FFD23F"), orange = Theme.Hex("#FF8C2B"), green = Theme.Hex("#8CCB4E"),
                          purple = Theme.Hex("#8D5BD9"), brown = Theme.Hex("#B8865B"), leaf = Theme.Hex("#5DBB46"), stem = Theme.Hex("#7A4E2D"),
                          cream = Theme.Hex("#F3E6CF"), capRed = Theme.Hex("#D94C3C"), broccoli = Theme.Hex("#4F9E3A"), broccoliStem = Theme.Hex("#B9D98B"),
                          seeds = Theme.Hex("#FFF4E8"), pale = Theme.Hex("#FFF0A3"), cornDark = Theme.Hex("#E0A800");

    // The colour the thing shows as once it is in a basket.
    public static Color Colour(string id) => id switch
    {
        "apple" => red, "banana" => yellow, "orange" => orange, "strawberry" => red, "grapes" => purple, "lemon" => yellow, "cherry" => red,
        "pear" => green, "carrot" => orange, "potato" => brown, "broccoli" => broccoli, "mushroom" => capRed, "chili" => red, "tomato" => red, "corn" => yellow,
        _ => Color.grey,
    };

    // How many pieces a stall shows.
    public static int Pieces(string id) => id switch { "grapes" => 2, "broccoli" => 2, "corn" => 2, "pear" => 2, "potato" => 4, "strawberry" => 4, _ => 3 };

    public static GameObject Make(string id, Transform parent)
    {
        var root = new GameObject(id);
        root.transform.SetParent(parent, false);
        var t = root.transform;
        switch (id)
        {
            case "apple":
                Ball(t, red, 0.26f, 0.23f, 0.26f);
                Stem(t, 0.23f);
                Leaf(t, 0.05f, 0.26f, 25);
                break;
            case "banana":
                MeshKit.Part(t, "banana", MeshKit.Tube, Look.Mat(yellow), new Vector3(0, 0.04f, 0), new Vector3(90, 0, 0), Vector3.one * 0.32f);
                break;
            case "orange":
                Ball(t, orange, 0.26f, 0.26f, 0.26f);
                MeshKit.Part(t, "stem", MeshKit.Sphere, Look.Mat(leaf), new Vector3(0, 0.26f, 0), new Vector3(0.05f, 0.03f, 0.05f));
                break;
            case "strawberry":
                MeshKit.Part(t, "berry", MeshKit.Cone, Look.Mat(red), new Vector3(0, 0.13f, 0), new Vector3(180, 0, 0), new Vector3(0.22f, 0.26f, 0.22f));
                MeshKit.Part(t, "cap", MeshKit.Sphere, Look.Mat(leaf), new Vector3(0, 0.25f, 0), new Vector3(0.2f, 0.05f, 0.2f));
                for (int i = 0; i < 4; i++)
                {
                    float a = i * 90 + 45;
                    MeshKit.Part(t, "seed", MeshKit.Sphere, Look.Mat(seeds), new Vector3(0.09f * Mathf.Cos(a * Mathf.Deg2Rad), 0.17f, 0.09f * Mathf.Sin(a * Mathf.Deg2Rad)), 0.025f);
                }
                break;
            case "grapes":
                foreach (var (x, y, z) in new[] { (-0.08f, 0.2f, 0f), (0f, 0.2f, 0.04f), (0.08f, 0.2f, 0f), (-0.04f, 0.12f, 0.02f), (0.04f, 0.12f, -0.02f), (0f, 0.13f, 0.06f), (0f, 0.05f, 0f) })
                    MeshKit.Part(t, "grape", MeshKit.Sphere, Look.Mat(purple), new Vector3(x, y, z), 0.11f);
                MeshKit.Part(t, "stalk", MeshKit.Cylinder, Look.Mat(stem), new Vector3(0, 0.28f, 0.02f), new Vector3(0.02f, 0.08f, 0.02f));
                break;
            case "lemon":
                Ball(t, yellow, 0.3f, 0.22f, 0.22f);
                MeshKit.Part(t, "nub", MeshKit.Sphere, Look.Mat(pale), new Vector3(0.15f, 0.11f, 0), new Vector3(0.06f, 0.05f, 0.05f));
                MeshKit.Part(t, "nub", MeshKit.Sphere, Look.Mat(pale), new Vector3(-0.15f, 0.11f, 0), new Vector3(0.06f, 0.05f, 0.05f));
                break;
            case "cherry":
                MeshKit.Part(t, "cherry", MeshKit.Sphere, Look.Mat(red), new Vector3(-0.08f, 0.09f, 0), 0.18f);
                MeshKit.Part(t, "cherry", MeshKit.Sphere, Look.Mat(red), new Vector3(0.08f, 0.09f, 0.02f), 0.18f);
                MeshKit.Part(t, "stalk", MeshKit.Cylinder, Look.Mat(stem), new Vector3(-0.04f, 0.2f, 0), new Vector3(0, 0, -22), new Vector3(0.015f, 0.22f, 0.015f));
                MeshKit.Part(t, "stalk", MeshKit.Cylinder, Look.Mat(stem), new Vector3(0.04f, 0.2f, 0.01f), new Vector3(0, 0, 22), new Vector3(0.015f, 0.22f, 0.015f));
                break;
            case "pear":
                MeshKit.Part(t, "pear", MeshKit.Pear, Look.Mat(green), Vector3.zero, new Vector3(0.3f, 0.32f, 0.3f));
                Stem(t, 0.3f);
                break;
            case "carrot":
                MeshKit.Part(t, "carrot", MeshKit.Cone, Look.Mat(orange), new Vector3(0, 0.14f, 0), new Vector3(180, 0, 0), new Vector3(0.13f, 0.28f, 0.13f));
                for (int i = -1; i <= 1; i++)
                    MeshKit.Part(t, "leaf", MeshKit.Capsule, Look.Mat(leaf), new Vector3(0.03f * i, 0.33f, 0), new Vector3(0, 0, -25 * i), new Vector3(0.04f, 0.12f, 0.04f));
                break;
            case "potato":
                MeshKit.Part(t, "potato", MeshKit.Sphere, Look.Mat(brown), new Vector3(0, 0.09f, 0), new Vector3(0, 25, 0), new Vector3(0.28f, 0.18f, 0.21f));
                break;
            case "broccoli":
                MeshKit.Part(t, "stem", MeshKit.Cylinder, Look.Mat(broccoliStem), new Vector3(0, 0.08f, 0), new Vector3(0.1f, 0.16f, 0.1f));
                foreach (var (x, z) in new[] { (-0.07f, 0f), (0.07f, 0f), (0f, -0.07f), (0f, 0.07f) })
                    MeshKit.Part(t, "floret", MeshKit.Sphere, Look.Mat(broccoli), new Vector3(x, 0.2f, z), 0.15f);
                MeshKit.Part(t, "floret", MeshKit.Sphere, Look.Mat(broccoli), new Vector3(0, 0.25f, 0), 0.15f);
                break;
            case "mushroom":
                MeshKit.Part(t, "stem", MeshKit.Cylinder, Look.Mat(cream), new Vector3(0, 0.08f, 0), new Vector3(0.11f, 0.16f, 0.11f));
                MeshKit.Part(t, "cap", MeshKit.Dome, Look.Mat(capRed), new Vector3(0, 0.14f, 0), new Vector3(0.3f, 0.3f, 0.3f));
                MeshKit.Part(t, "dot", MeshKit.Sphere, Look.Mat(seeds), new Vector3(-0.06f, 0.23f, 0.06f), 0.04f);
                MeshKit.Part(t, "dot", MeshKit.Sphere, Look.Mat(seeds), new Vector3(0.07f, 0.25f, -0.03f), 0.045f);
                MeshKit.Part(t, "dot", MeshKit.Sphere, Look.Mat(seeds), new Vector3(0.0f, 0.28f, 0.0f), 0.035f);
                break;
            case "chili":
                MeshKit.Part(t, "chili", MeshKit.ThinTube, Look.Mat(red), new Vector3(0, 0.025f, 0), new Vector3(90, 0, 0), Vector3.one * 0.3f);
                MeshKit.Part(t, "stem", MeshKit.Cylinder, Look.Mat(leaf), new Vector3(-0.14f, 0.03f, 0.0f), new Vector3(0, 0, 70), new Vector3(0.03f, 0.06f, 0.03f));
                break;
            case "tomato":
                Ball(t, Theme.Hex("#F2473D"), 0.27f, 0.21f, 0.27f);
                for (int i = 0; i < 3; i++)
                    MeshKit.Part(t, "sepal", MeshKit.Capsule, Look.Mat(leaf), new Vector3(0, 0.215f, 0), new Vector3(0, 60 * i, 90), new Vector3(0.03f, 0.14f, 0.03f));
                break;
            case "corn":
                MeshKit.Part(t, "cob", MeshKit.Capsule, Look.Mat(yellow), new Vector3(0, 0.07f, 0), new Vector3(0, 0, 75), new Vector3(0.13f, 0.3f, 0.13f));
                MeshKit.Part(t, "husk", MeshKit.Capsule, Look.Mat(leaf), new Vector3(0.12f, 0.06f, -0.03f), new Vector3(0, 0, 75), new Vector3(0.1f, 0.26f, 0.04f));
                MeshKit.Part(t, "kernels", MeshKit.Capsule, Look.Mat(cornDark), new Vector3(-0.02f, 0.07f, 0.055f), new Vector3(0, 0, 75), new Vector3(0.11f, 0.26f, 0.03f));
                break;
            default:
                Ball(t, Colour(id), 0.24f, 0.24f, 0.24f);
                break;
        }
        return root;
    }

    static void Ball(Transform t, Color c, float sx, float sy, float sz) =>
        MeshKit.Part(t, "ball", MeshKit.Sphere, Look.Mat(c), new Vector3(0, sy / 2, 0), new Vector3(sx, sy, sz));

    static void Stem(Transform t, float y) =>
        MeshKit.Part(t, "stem", MeshKit.Cylinder, Look.Mat(stem), new Vector3(0, y + 0.02f, 0), new Vector3(0.02f, 0.06f, 0.02f));

    static void Leaf(Transform t, float x, float y, float tilt) =>
        MeshKit.Part(t, "leaf", MeshKit.Sphere, Look.Mat(leaf), new Vector3(x, y, 0), new Vector3(0, 0, tilt), new Vector3(0.09f, 0.02f, 0.05f));
}
