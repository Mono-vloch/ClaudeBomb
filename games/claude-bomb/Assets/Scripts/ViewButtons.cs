using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// On-screen arrows to turn the bomb, for players without a right mouse button
// (laptop trackpads). Sits at the bottom right of the bomb view, under the manual
// table so it disappears while the table is open. Builds its own UI at runtime.
public class ViewButtons : MonoBehaviour
{
    [SerializeField] BombCameraController cameraController;
    [SerializeField] RectTransform chatArea;   // the arrows stay left of the chat

    const float Cell = 60f, Gap = 8f;

    // The arrows, for the tour to point at.
    public RectTransform Cluster { get; private set; }

    void Start()
    {
        var canvas = Ui.NewCanvas("ViewButtonsCanvas", transform, -2); // under the manual table (-1)

        float chatWidth = chatArea != null ? chatArea.rect.width : 590f;
        Cluster = Ui.NewRect("Arrows", canvas.transform);
        Ui.Place(Cluster, new Vector2(1, 0), new Vector2(-(chatWidth + 24), 24), new Vector2(3 * Cell + 2 * Gap, 3 * Cell + 2 * Gap));

        float step = Cell + Gap;
        AddButton(new Vector2(step, 2 * step), 90f, () => cameraController.Step(0, 1));
        AddButton(new Vector2(0, step), 180f, () => cameraController.Step(1, 0));
        AddButton(new Vector2(2 * step, step), 0f, () => cameraController.Step(-1, 0));
        AddButton(new Vector2(step, 0), -90f, () => cameraController.Step(0, -1));

        var front = AddButton(new Vector2(step, step), null, () => cameraController.ResetView());
        var label = Ui.NewText("Label", front.transform, "Front", 13f, Ui.Text2, TextAlignmentOptions.Center, Ui.Display);
        Ui.Stretch(label.rectTransform);
    }

    // A round glass button; arrow is the direction its triangle points (degrees), or null for none.
    Button AddButton(Vector2 pos, float? arrow, UnityAction onClick)
    {
        var img = Ui.NewImage("Button", Cluster, Color.white);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos + new Vector2(Cell / 2f, Cell / 2f);
        rt.sizeDelta = new Vector2(Cell, Cell);
        var button = img.gameObject.AddComponent<Button>();
        Ui.Style(button, Ui.ButtonStyle.Secondary);
        img.sprite = Ui.Circle; // round, not the rounded square Style gives
        img.type = Image.Type.Simple;
        img.color = Color.white.WithAlpha(0.92f);
        button.onClick.AddListener(onClick);

        var ring = Ui.NewIcon("Ring", img.transform, Ui.Ring, Ui.Border2, Cell);
        Ui.Stretch(ring.rectTransform);
        if (arrow.HasValue)
        {
            var icon = Ui.NewIcon("Arrow", img.transform, Ui.Triangle, Ui.Ink, 30f);
            icon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, arrow.Value);
        }
        return button;
    }
}
