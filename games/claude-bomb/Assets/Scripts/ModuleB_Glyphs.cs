using System;
using UnityEngine;
using UnityEngine.UI;

// Four squares in a row, positions 1-4 left to right (squareObjects[0] = position 1, ...).
// Each shows one of 7 glyphs via a UI Image; press the shown glyphs in the chosen set's
// order. Pressing out of order resets progress and gives a strike.
//
// Each square also has 4 "progress" spheres. When a correct press lands, the clicked
// square's sphere at index (current press number) turns green - so square lights up
// sphere 1 if it was pressed first, sphere 2 if pressed second, etc. A wrong press
// resets every sphere on every square back to how it looked originally.
public class ModuleB_Glyphs : MonoBehaviour
{
    [Serializable]
    public class SquareSpheres { public GameObject[] spheres = new GameObject[4]; }

    [SerializeField] GameObject[] squareObjects = new GameObject[4];
    [SerializeField] Image[] iconImages = new Image[4]; // same order as squareObjects
    [SerializeField] Sprite[] glyphSprites = new Sprite[7]; // index 0 = g1 ... index 6 = g7
    [SerializeField] SquareSpheres[] progressSpheres = new SquareSpheres[4]; // same order as squareObjects
    [SerializeField] Color progressColor = Color.green;

    int[] pressOrder; // positions (1-4) to press, in order
    int progress;
    Renderer[][] sphereRenderers;
    Color[][] sphereOriginalColors;

    void Start()
    {
        var device = GameManager.Instance.Device;
        pressOrder = Rules.GlyphAnswer(device);

        CacheSphereRenderers();

        for (int i = 0; i < squareObjects.Length; i++)
        {
            int glyphId = device.glyphAtPosition[i];
            if (iconImages[i] != null && glyphId >= 1 && glyphId <= glyphSprites.Length)
                iconImages[i].sprite = glyphSprites[glyphId - 1];

            ClickRelay.Attach(squareObjects[i], i, OnSquareClicked);
        }
    }

    void CacheSphereRenderers()
    {
        sphereRenderers = new Renderer[progressSpheres.Length][];
        sphereOriginalColors = new Color[progressSpheres.Length][];

        for (int i = 0; i < progressSpheres.Length; i++)
        {
            GameObject[] spheres = progressSpheres[i].spheres;
            sphereRenderers[i] = new Renderer[spheres.Length];
            sphereOriginalColors[i] = new Color[spheres.Length];

            for (int j = 0; j < spheres.Length; j++)
            {
                if (spheres[j] == null) continue;
                var renderer = spheres[j].GetComponentInChildren<Renderer>();
                sphereRenderers[i][j] = renderer;
                if (renderer != null) sphereOriginalColors[i][j] = renderer.material.color;
            }
        }
    }

    void OnSquareClicked(int index)
    {
        if (GameManager.Instance.IsSolved(ModuleId.Glyphs)) return;

        SoundManager.Instance.PlayButtonClick();

        int position = index + 1;
        if (position == pressOrder[progress])
        {
            int pressNumber = progress; // 0-based: 0 = this was the 1st correct press
            progress++;

            // Report the state change BEFORE touching any visuals, so a problem with
            // the sphere setup can never silently block the actual solve/strike.
            if (progress == pressOrder.Length)
                GameManager.Instance.ReportSolved(ModuleId.Glyphs);

            LightSpheresUpTo(index, pressNumber);
        }
        else
        {
            progress = 0;
            GameManager.Instance.ReportStrike(ModuleId.Glyphs);
            ResetAllSpheres();
        }
    }

    // Lights this square's spheres 0..pressNumber (inclusive) - so the square pressed
    // Nth shows N green spheres, cumulative, not just a single one.
    void LightSpheresUpTo(int squareIndex, int pressNumber)
    {
        if (squareIndex < 0 || squareIndex >= sphereRenderers.Length) return;
        var renderers = sphereRenderers[squareIndex];
        if (renderers == null) return;

        for (int i = 0; i <= pressNumber && i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].material.color = progressColor;
    }

    void ResetAllSpheres()
    {
        for (int i = 0; i < sphereRenderers.Length; i++)
        {
            if (sphereRenderers[i] == null) continue;
            for (int j = 0; j < sphereRenderers[i].Length; j++)
                if (sphereRenderers[i][j] != null)
                    sphereRenderers[i][j].material.color = sphereOriginalColors[i][j];
        }
    }
}
