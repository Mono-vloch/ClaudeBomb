using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Nine cells, numbered 1-9 reading left-to-right, top row then middle then bottom
// (cellObjects[0] = cell 1, ... cellObjects[4] = cell 5/centre, ... cellObjects[8] = cell 9).
// One press solves it (Rules.GridAnswer); any other press is a strike.
public class ModuleD_Grid : MonoBehaviour
{
    [Serializable]
    public class ColorMaterial { public string colorName; public Material material; }

    [SerializeField] GameObject[] cellObjects = new GameObject[9];
    [SerializeField] ColorMaterial[] cellMaterials; // needs an entry for red, blue, yellow
    [SerializeField] float pressedZ = -0.38f;
    [SerializeField] float pressAnimationDuration = 1f; // total time for press-in + release

    readonly bool[] clicked = new bool[9];
    Dictionary<string, Material> materialByColor;

    void Start()
    {
        materialByColor = new Dictionary<string, Material>();
        foreach (var cm in cellMaterials)
            materialByColor[cm.colorName] = cm.material;

        var device = GameManager.Instance.Device;
        for (int i = 0; i < cellObjects.Length; i++)
        {
            ApplyColor(cellObjects[i], device.grid[i]);
            ClickRelay.Attach(cellObjects[i], i, OnCellClicked);
        }
    }

    void ApplyColor(GameObject cell, string colorName)
    {
        if (materialByColor.TryGetValue(colorName, out var mat))
        {
            var renderer = cell.GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.material = mat;
        }
    }

    void OnCellClicked(int index)
    {
        if (GameManager.Instance.IsSolved(ModuleId.Grid) || clicked[index]) return;
        clicked[index] = true;

        SoundManager.Instance.PlayButtonClick();
        StartCoroutine(AnimatePress(cellObjects[index]));

        int correct = Rules.GridAnswer(GameManager.Instance.Device);
        if (index + 1 == correct)
            GameManager.Instance.ReportSolved(ModuleId.Grid);
        else
            GameManager.Instance.ReportStrike(ModuleId.Grid);
    }

    IEnumerator AnimatePress(GameObject cell)
    {
        Transform t = cell.transform;
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
