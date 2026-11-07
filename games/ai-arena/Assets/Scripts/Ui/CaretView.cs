using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TextMesh Pro's own caret does not show in the web player, so every field
// draws its own: a thin bar after the character before the caret, from the
// text's own layout, blinking while the field has the focus.
public class CaretView : MonoBehaviour
{
    TMP_InputField field;
    TMP_Text text;
    Image bar;
    string lastText;
    float focusedAt;
    bool wasFocused;

    public void Bind(TMP_InputField field, TMP_Text text)
    {
        this.field = field;
        this.text = text;
        bar = Ui.Box(text.transform, "Caret", Theme.Chalk, false);
        bar.raycastTarget = false;
        var rt = bar.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); // the text's local origin
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(2.5f, text.fontSize * 1.15f);
        bar.enabled = false;
    }

    void LateUpdate()
    {
        if (field == null || bar == null) return;
        bool focused = field.isFocused;
        if (focused && !wasFocused) focusedAt = Time.unscaledTime;
        wasFocused = focused;
        if (!focused) { bar.enabled = false; return; }

        if (text.text != lastText) { text.ForceMeshUpdate(); lastText = text.text; }
        var info = text.textInfo;
        int pos = Mathf.Clamp(field.caretPosition, 0, text.text.Length);
        float x, top, height;
        var rect = text.rectTransform.rect;
        if (pos == 0 || info.characterCount == 0)
        {
            x = rect.xMin;
            var line0 = info.lineCount > 0 ? info.lineInfo[0] : default;
            top = info.lineCount > 0 && info.characterCount > 0 ? line0.ascender : rect.yMax;
            height = info.lineCount > 0 && info.characterCount > 0 ? line0.ascender - line0.descender : text.fontSize * 1.15f;
        }
        else
        {
            var c = info.characterInfo[Mathf.Min(pos, info.characterCount) - 1];
            var line = info.lineInfo[c.lineNumber];
            x = c.xAdvance;
            top = line.ascender;
            height = line.ascender - line.descender;
        }
        bar.rectTransform.anchoredPosition = new Vector2(x, top);
        bar.rectTransform.sizeDelta = new Vector2(2.5f, height);
        bar.enabled = (Time.unscaledTime - focusedAt) % 1.1f < 0.6f;
    }
}
