using System.Collections.Generic;
using UnityEngine;

// The circuit's effects, pooled: one particle system per kind, reused with Emit, never
// made and destroyed per event. Colours from the art bible, lifetimes short, sizes that
// shrink to nothing. Sparks on a crash, confetti at the finish, dust under the car at
// speed. The teal tyre trails and the ghost lines of earlier attempts live in Circuit.
public class Effects : MonoBehaviour
{
    ParticleSystem sparks, confetti, dust;
    static Material additive, soft;

    static Material Mat(bool add)
    {
        var shader = Resources.Load<Shader>("Shaders/Spark");
        var m = new Material(shader) { name = add ? "spark-add" : "spark-soft" };
        m.mainTexture = Theme.Circle != null ? Theme.Circle.texture : Texture2D.whiteTexture;
        m.SetFloat("_Dst", (float)(add ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
        return m;
    }

    void Awake()
    {
        additive = additive != null ? additive : Mat(true);
        soft = soft != null ? soft : Mat(false);
        // The kit's camera rides a metre behind the car: smaller bits, and sparks drawn as streaks.
        float k = Circuit.HasKit ? 0.35f : 1f;
        sparks = Make("sparks", additive, 0.25f, 0.6f, 2.5f * k, 4.5f * k, 0.05f * k * 0.6f, 0.11f * k * 0.6f, 1.4f, 160);
        confetti = Make("confetti", additive, 0.9f, 1.3f, 2.2f * k, 4f * k, 0.07f * k, 0.13f * k, 0.55f, 200);
        dust = Make("dust", soft, 0.35f, 0.6f, 0.2f * k, 0.5f * k, 0.12f * k, 0.22f * k, -0.05f, 120);
        if (Circuit.HasKit)
        {
            var sr = sparks.GetComponent<ParticleSystemRenderer>(); sr.renderMode = ParticleSystemRenderMode.Stretch; sr.velocityScale = 0.02f; sr.lengthScale = 4f;
            var ss = sparks.shape; ss.radius = 0.05f;
        }
        var ds = dust.shape; ds.shapeType = ParticleSystemShapeType.Sphere; ds.radius = 0.12f;
        var dm = dust.main; dm.startColor = new ParticleSystem.MinMaxGradient(Theme.Hex("#C9C2B4") * new Color(1, 1, 1, 0.45f), Theme.Hex("#8FBF6E") * new Color(1, 1, 1, 0.3f));
        var cs = confetti.shape; cs.shapeType = ParticleSystemShapeType.Cone; cs.angle = 28f; cs.radius = 0.08f; cs.rotation = new Vector3(-90f, 0f, 0f);
    }

    ParticleSystem Make(string name, Material material, float life0, float life1, float speed0, float speed1, float size0, float size1, float gravity, int max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false; main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(life0, life1);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed0, speed1);
        main.startSize = new ParticleSystem.MinMaxCurve(size0, size1);
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = max;
        var emission = ps.emission; emission.enabled = true; emission.rateOverTime = 0f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.15f;
        var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        var colour = ps.colorOverLifetime; colour.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        colour.color = new ParticleSystem.MinMaxGradient(g);
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = material; r.renderMode = ParticleSystemRenderMode.Billboard; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
        return ps;
    }

    static void Tint(ParticleSystem ps, Color a, Color b) { var m = ps.main; m.startColor = new ParticleSystem.MinMaxGradient(a, b); }

    // A crash: amber and red sparks, a few grey bits, out of the point of impact.
    public void Sparks(Vector3 at, float strength = 1f)
    {
        Tint(sparks, Theme.Amber, Theme.Danger);
        sparks.transform.position = at;
        sparks.Emit(Mathf.RoundToInt(22 * strength));
        Tint(sparks, Look.Grey, Look.Shell);
        sparks.Emit(Mathf.RoundToInt(6 * strength));
    }

    // A finish: teal, amber and white confetti, up and out.
    public void Confetti(Vector3 at, int count = 42)
    {
        Tint(confetti, Theme.Teal, Theme.Amber);
        confetti.transform.position = at;
        confetti.Emit(count * 2 / 3);
        Tint(confetti, Look.Shell, Theme.Teal);
        confetti.Emit(count / 3);
    }

    // Dust under the car, a puff per call: call it as the car moves at speed.
    public void Dust(Vector3 at, int count = 2)
    {
        dust.transform.position = at;
        dust.Emit(count);
    }

    public int Alive => sparks.particleCount + confetti.particleCount + dust.particleCount;
}
