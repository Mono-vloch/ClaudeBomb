using System.Collections;
using UnityEngine;

// Five switches, numbered 1-5 left to right (switchObjects[0] = switch 1, ...).
// The switch model's lever points down at local Z rotation 0, so up = -180 and
// down = 0: what the player sees must match the state Claude's rules use. Two
// safety interlocks, checked after every single flip: switches 2&3 (index 1,2)
// and switches 4&5 (index 3,4) must never both be up at once.
//
// Each switch has its own indicator light: green when up, dark when down.
public class ModuleE_Switches : MonoBehaviour
{
    [SerializeField] GameObject[] switchObjects = new GameObject[5];
    [SerializeField] GameObject[] switchCubes = new GameObject[5]; // same order as switchObjects
    [SerializeField] Material downMaterial;  // dark - shown when the switch is down
    [SerializeField] Material upMaterial;    // green - shown when the switch is up
    [SerializeField] float flipDuration = 0.2f;

    readonly bool[] state = new bool[5]; // true = up

    void Start()
    {
        var device = GameManager.Instance.Device;
        for (int i = 0; i < switchObjects.Length; i++)
        {
            state[i] = device.switches[i];
            SnapVisual(switchObjects[i], state[i]);
            ApplyCubeMaterial(i);
            ClickRelay.Attach(switchObjects[i], i, OnSwitchClicked, debugLog: true);
        }
    }

    void OnSwitchClicked(int index)
    {
        if (GameManager.Instance.IsSolved(ModuleId.Switches)) return;

        SoundManager.Instance.PlaySwitchFlip();
        state[index] = !state[index];

        // Report state changes before touching any visuals, so a problem with the
        // cube/material setup can never silently block an actual strike or solve.
        CheckInterlocks();
        CheckSolved();

        ApplyCubeMaterial(index);
        StartCoroutine(AnimateVisual(index));
    }

    void CheckInterlocks()
    {
        if (state[1] && state[2]) GameManager.Instance.ReportStrike(ModuleId.Switches);
        if (state[3] && state[4]) GameManager.Instance.ReportStrike(ModuleId.Switches);
    }

    void CheckSolved()
    {
        bool[] target = Rules.SwitchTarget(GameManager.Instance.Device);
        for (int i = 0; i < state.Length; i++)
            if (state[i] != target[i]) return;

        GameManager.Instance.ReportSolved(ModuleId.Switches);
    }

    void ApplyCubeMaterial(int index)
    {
        if (index < 0 || index >= switchCubes.Length || switchCubes[index] == null) return;

        var renderer = switchCubes[index].GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        // Guard against a null Material reference (e.g. Up/Down Material not yet
        // assigned in the Inspector) - this runs before ClickRelay.Attach in Start(),
        // so if this throws, NO switch would get click detection at all.
        Material mat = state[index] ? upMaterial : downMaterial;
        if (mat != null) renderer.material = mat;
        else Debug.LogWarning($"[ModuleE_Switches] {(state[index] ? "Up" : "Down")} Material is not assigned - skipping color for switch {index + 1}.");
    }

    void SnapVisual(GameObject go, bool up)
    {
        Vector3 e = go.transform.localEulerAngles;
        go.transform.localEulerAngles = new Vector3(e.x, e.y, up ? -180f : 0f);
    }

    IEnumerator AnimateVisual(int index)
    {
        Transform t = switchObjects[index].transform;
        float fromZ = state[index] ? 0f : -180f;
        float toZ = state[index] ? -180f : 0f;
        Vector3 euler = t.localEulerAngles;

        float elapsed = 0f;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float z = Mathf.LerpAngle(fromZ, toZ, elapsed / flipDuration);
            t.localEulerAngles = new Vector3(euler.x, euler.y, z);
            yield return null;
        }
        SnapVisual(switchObjects[index], state[index]);
    }
}
