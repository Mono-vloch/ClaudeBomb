using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Four wires, numbered 1-4 top down (wireObjects[0] = wire 1, ...).
// Cut with a horizontal or diagonal mouse drag across the wire (not a plain click) -
// cutting the correct wire (Rules.WireAnswer) solves the module; any other cut strikes.
public class ModuleA_Wires : MonoBehaviour
{
    [Serializable]
    public class ColorMaterial { public string colorName; public Material material; }

    [SerializeField] GameObject[] wireObjects = new GameObject[4];
    [SerializeField] ColorMaterial[] wireMaterials; // needs an entry for red, blue, yellow, white, black
    [SerializeField] float minDragPixels = 30f;
    [SerializeField] int dragSamples = 10;

    readonly bool[] cut = new bool[4];
    Dictionary<string, Material> materialByColor;
    Camera cam;
    bool dragging;
    Vector2 dragStart;

    void Start()
    {
        materialByColor = new Dictionary<string, Material>();
        foreach (var cm in wireMaterials)
            materialByColor[cm.colorName] = cm.material;

        var device = GameManager.Instance.Device;
        for (int i = 0; i < wireObjects.Length; i++)
        {
            ApplyColor(wireObjects[i], device.wires[i]);
            ClickRelay.EnsureCollider(wireObjects[i]);
        }
    }

    void ApplyColor(GameObject wire, string colorName)
    {
        if (!materialByColor.TryGetValue(colorName, out var mat)) return;

        // includeInactive: true so the (currently hidden) cut-half children get
        // coloured now too, matching the intact wire before they're ever shown.
        foreach (var renderer in wire.GetComponentsInChildren<Renderer>(true))
            renderer.material = mat;
    }

    void Update()
    {
        if (GameManager.Instance.IsSolved(ModuleId.Wires)) return;
        if (Mouse.current == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            dragging = true;
            dragStart = Mouse.current.position.ReadValue();
        }
        else if (dragging && Mouse.current.leftButton.isPressed)
        {
            Vector2 current = Mouse.current.position.ReadValue();
            Vector2 delta = current - dragStart;

            if (delta.magnitude >= minDragPixels && IsCuttingMotion(delta))
            {
                dragging = false; // resolve at most one cut per drag
                TryCutAlong(dragStart, current);
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }
    }

    // Accepts horizontal and diagonal drags, rejects drags steeper than 45 degrees
    // from horizontal (i.e. mostly-vertical drags don't count as a cut).
    static bool IsCuttingMotion(Vector2 delta) => Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);

    void TryCutAlong(Vector2 from, Vector2 to)
    {
        for (int s = 0; s <= dragSamples; s++)
        {
            Vector2 point = Vector2.Lerp(from, to, s / (float)dragSamples);
            Ray ray = cam.ScreenPointToRay(point);
            if (!Physics.Raycast(ray, out RaycastHit hit)) continue;

            for (int i = 0; i < wireObjects.Length; i++)
            {
                if (hit.transform.IsChildOf(wireObjects[i].transform))
                {
                    CutWire(i);
                    return; // one wire per drag
                }
            }
        }
    }

    void CutWire(int index)
    {
        if (cut[index]) return;
        cut[index] = true;

        SoundManager.Instance.PlayWireCut();
        ShowCutVisual(wireObjects[index]);

        int correct = Rules.WireAnswer(GameManager.Instance.Device);
        if (index + 1 == correct)
            GameManager.Instance.ReportSolved(ModuleId.Wires);
        else
            GameManager.Instance.ReportStrike(ModuleId.Wires);
    }

    // Hides the intact wire mesh and reveals its two pre-authored "cut" child halves.
    static void ShowCutVisual(GameObject wire)
    {
        var renderer = wire.GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        foreach (Transform child in wire.transform)
            child.gameObject.SetActive(true);
    }
}
