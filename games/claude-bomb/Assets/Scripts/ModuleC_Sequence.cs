using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Circles flash a 4-step colour sequence when the play button is pressed; squares are
// the press buttons. Convert each flashed colour (Rules.SequenceAnswer) and press the
// matching-coloured square in the same order. A wrong press resets progress and strikes.
public class ModuleC_Sequence : MonoBehaviour
{
    [Serializable]
    public class ColorSlot { public string colorName; public GameObject target; }

    [Serializable]
    public class ColorMaterial { public string colorName; public Material material; }

    [SerializeField] ColorSlot[] lamps = new ColorSlot[4];   // circles
    [SerializeField] ColorSlot[] presses = new ColorSlot[4]; // squares
    [SerializeField] ColorMaterial[] slotMaterials; // needs an entry for red, blue, green, yellow
    [SerializeField] GameObject playButton;
    [SerializeField] float flashScale = 1.25f;
    [SerializeField] float flashEmissionIntensity = 3f;
    [SerializeField] float flashDuration = 0.4f;
    [SerializeField] float gapDuration = 0.25f;
    [SerializeField] float pressedZ = -0.45f;
    [SerializeField] float pressAnimationDuration = 1f; // total time for press-in + release

    const string EmissionProperty = "_EmissionColor";
    static readonly string[] RequiredColors = { "red", "blue", "green", "yellow" };

    string[] answer;
    int progress;
    bool playing;

    void Start()
    {
        ValidateSlots(lamps, "Lamps");
        ValidateSlots(presses, "Presses");

        var materialByColor = new Dictionary<string, Material>();
        foreach (var cm in slotMaterials)
            materialByColor[cm.colorName] = cm.material;

        answer = Rules.SequenceAnswer(GameManager.Instance.Device);

        foreach (var lamp in lamps) ApplyColor(lamp, materialByColor);
        foreach (var press in presses) ApplyColor(press, materialByColor);

        ClickRelay.Attach(playButton, 0, _ =>
        {
            SoundManager.Instance.PlayButtonClick();
            PlaySequenceIfIdle();
        });

        foreach (var press in presses)
            ClickRelay.Attach(press.target, 0, _ => OnPressClicked(press));
    }

    static void ApplyColor(ColorSlot slot, Dictionary<string, Material> materialByColor)
    {
        if (!materialByColor.TryGetValue(slot.colorName, out var mat)) return;
        var renderer = slot.target.GetComponentInChildren<Renderer>();
        if (renderer != null) renderer.material = mat;
    }

    // Catches Inspector data-entry mistakes early: a duplicate or missing color name
    // fails silently otherwise (that color's step just never flashes or can't be pressed).
    static void ValidateSlots(ColorSlot[] slots, string label)
    {
        var seen = new HashSet<string>();
        foreach (var slot in slots)
        {
            if (!seen.Add(slot.colorName))
                Debug.LogWarning($"[ModuleC_Sequence] {label} has a duplicate color name '{slot.colorName}' - each of the 4 must be unique.");
        }
        foreach (var required in RequiredColors)
        {
            if (!seen.Contains(required))
                Debug.LogWarning($"[ModuleC_Sequence] {label} is missing color '{required}' - nothing will flash or respond to it.");
        }
    }

    void PlaySequenceIfIdle()
    {
        if (!playing && !GameManager.Instance.IsSolved(ModuleId.Sequence))
            StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        playing = true;
        foreach (var colorName in GameManager.Instance.Device.sequence)
        {
            var lamp = Array.Find(lamps, l => l.colorName == colorName);
            if (lamp != null) yield return StartCoroutine(Flash(lamp.target.transform));
            yield return new WaitForSeconds(gapDuration);
        }
        playing = false;
    }

    IEnumerator Flash(Transform t)
    {
        Vector3 originalScale = t.localScale;
        var renderer = t.GetComponentInChildren<Renderer>();
        Material mat = renderer != null ? renderer.material : null;
        Color originalEmission = Color.black;

        if (mat != null)
        {
            originalEmission = mat.GetColor(EmissionProperty);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor(EmissionProperty, mat.color * flashEmissionIntensity);
        }

        t.localScale = originalScale * flashScale;
        yield return new WaitForSeconds(flashDuration);
        t.localScale = originalScale;

        if (mat != null)
            mat.SetColor(EmissionProperty, originalEmission);
    }

    void OnPressClicked(ColorSlot slot)
    {
        if (GameManager.Instance.IsSolved(ModuleId.Sequence) || playing) return;

        SoundManager.Instance.PlayButtonClick();
        StartCoroutine(AnimatePress(slot.target));

        if (slot.colorName == answer[progress])
        {
            progress++;
            if (progress == answer.Length)
                GameManager.Instance.ReportSolved(ModuleId.Sequence);
        }
        else
        {
            progress = 0;
            GameManager.Instance.ReportStrike(ModuleId.Sequence);
        }
    }

    IEnumerator AnimatePress(GameObject obj)
    {
        Transform t = obj.transform;
        Vector3 start = t.localPosition;
        Vector3 pressed = new Vector3(start.x, start.y, pressedZ);
        float half = pressAnimationDuration / 2f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localPosition = Vector3.Lerp(start, pressed, elapsed / half);
            yield return null;
        }
        t.localPosition = pressed;

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localPosition = Vector3.Lerp(pressed, start, elapsed / half);
            yield return null;
        }
        t.localPosition = start;
    }
}
