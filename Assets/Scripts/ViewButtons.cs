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
    [SerializeField] TMP_FontAsset arrowFont; // needs the ◀▶▲▼ glyphs (Noto Sans JP has them)
    [SerializeField] TMP_FontAsset labelFont;
    [SerializeField] RectTransform chatArea;   // the arrows stay left of the chat

    const float Cell = 64f, Gap = 10f;

    void Start()
    {
        var canvasGo = new GameObject("ViewButtonsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -2; // under the manual table (-1)
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        float chatWidth = chatArea != null ? chatArea.rect.width : 590f;
        var cluster = new GameObject("Arrows", typeof(RectTransform)).GetComponent<RectTransform>();
        cluster.SetParent(canvasGo.transform, false);
        cluster.anchorMin = cluster.anchorMax = cluster.pivot = new Vector2(1, 0);
        cluster.anchoredPosition = new Vector2(-(chatWidth + 24), 24);
        cluster.sizeDelta = new Vector2(3 * Cell + 2 * Gap, 3 * Cell + 2 * Gap);

        float step = Cell + Gap;
        AddButton(cluster, "▲", arrowFont, 30, new Vector2(step, 2 * step), () => cameraController.Step(0, 1));
        AddButton(cluster, "◀", arrowFont, 30, new Vector2(0, step), () => cameraController.Step(1, 0));
        AddButton(cluster, "FRONT", labelFont, 13, new Vector2(step, step), () => cameraController.ResetView());
        AddButton(cluster, "▶", arrowFont, 30, new Vector2(2 * step, step), () => cameraController.Step(-1, 0));
        AddButton(cluster, "▼", arrowFont, 30, new Vector2(step, 0), () => cameraController.Step(0, -1));
    }

    static void AddButton(RectTransform parent, string label, TMP_FontAsset font, float size, Vector2 pos, UnityAction onClick)
    {
        var go = new GameObject(label, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(Cell, Cell);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.95f, 0.94f, 0.9f, 0.85f);
        go.AddComponent<Button>().onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var trt = (RectTransform)textGo.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var text = textGo.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.text = label;
        text.fontSize = size;
        text.color = new Color(0.13f, 0.13f, 0.15f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }
}
