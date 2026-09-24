using UnityEngine;

// Attach to a module's status lamp. Watches GameManager for that specific module being
// solved and swaps the lamp's base + emission color (red -> green). One script, reused
// on every module's lamp - just set which ModuleId it watches.
public class ModuleStatusLamp : MonoBehaviour
{
    [SerializeField] ModuleId watchedModule;
    [SerializeField] Renderer lampRenderer;
    [SerializeField] Color solvedColor = Color.green;
    [SerializeField] string emissionProperty = "_EmissionColor";

    void Start()
    {
        // Start (unlike OnEnable) is only called after every object's Awake has run,
        // so GameManager.Instance is guaranteed to exist here.
        GameManager.Instance.OnModuleSolved += HandleModuleSolved;

        if (GameManager.Instance.IsSolved(watchedModule))
            SetSolvedVisual();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnModuleSolved -= HandleModuleSolved;
    }

    void HandleModuleSolved(ModuleId module)
    {
        if (module == watchedModule)
            SetSolvedVisual();
    }

    void SetSolvedVisual()
    {
        if (lampRenderer == null) return;

        var mat = lampRenderer.material; // instance, so it doesn't affect other lamps sharing the asset
        mat.color = solvedColor;

        // A plain Color.green has no HDR boost. If the red emission was set above
        // 1.0 intensity (which is what makes it actually glow / trigger Bloom),
        // swapping it in directly keeps the hue but kills the glow. Preserve
        // whatever intensity the original emission had instead of discarding it.
        Color currentEmission = mat.GetColor(emissionProperty);
        float intensity = Mathf.Max(currentEmission.maxColorComponent, 1f);
        mat.SetColor(emissionProperty, solvedColor * intensity);
        mat.EnableKeyword("_EMISSION");
    }
}
