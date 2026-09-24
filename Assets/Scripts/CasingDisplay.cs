using TMPro;
using UnityEngine;

// Displays the casing's stamp, its 3 lamps (VENT/SYNC/HOLD), and the power cell count
// on the physical device. Every module's rules depend on this info, so the player
// needs to be able to see and report it to Claude - this makes it actually visible.
public class CasingDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text stampText;

    [SerializeField] GameObject ventLamp;
    [SerializeField] GameObject syncLamp;
    [SerializeField] GameObject holdLamp;
    [SerializeField] Material lampLitMaterial;  // green
    [SerializeField] Material lampDarkMaterial;

    [SerializeField] GameObject[] powerCellCubes = new GameObject[3]; // in order, cell 1..3
    [SerializeField] Material cellOnMaterial;   // green
    [SerializeField] Material cellOffMaterial;  // black

    void Start()
    {
        Casing casing = GameManager.Instance.Device.casing;

        if (stampText != null) stampText.text = casing.stamp;

        SetLampVisual(ventLamp, casing.vent);
        SetLampVisual(syncLamp, casing.sync);
        SetLampVisual(holdLamp, casing.hold);

        for (int i = 0; i < powerCellCubes.Length; i++)
            ApplyMaterialToAll(powerCellCubes[i], i < casing.cells ? cellOnMaterial : cellOffMaterial);
    }

    void SetLampVisual(GameObject lamp, bool lit)
    {
        ApplyMaterialToAll(lamp, lit ? lampLitMaterial : lampDarkMaterial);
    }

    // Applies to every renderer under root (the box itself plus all 3 spheres, or
    // whatever the actual hierarchy is), so it doesn't matter exactly how each
    // lamp/cell object is built - everything under it changes together.
    static void ApplyMaterialToAll(GameObject root, Material mat)
    {
        if (root == null || mat == null) return;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            renderer.material = mat;
    }
}
