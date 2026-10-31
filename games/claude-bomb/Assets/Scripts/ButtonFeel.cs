using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The feel of every button in the game (added by Ui.Style): it grows a little under
// the pointer, dips when pressed and ticks when clicked. Unscaled time, so buttons
// stay lively while the game is paused.
public class ButtonFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    const float Hover = 1.04f, Pressed = 0.96f;

    Selectable selectable;
    float scale = 1f, target = 1f;
    bool over;

    void Awake() => selectable = GetComponent<Selectable>();

    bool Live => selectable == null || selectable.IsInteractable();

    public void OnPointerEnter(PointerEventData e) { over = true; if (Live) target = Hover; }
    public void OnPointerExit(PointerEventData e) { over = false; target = 1f; }
    public void OnPointerDown(PointerEventData e) { if (Live) target = Pressed; }
    public void OnPointerUp(PointerEventData e) => target = over && Live ? Hover : 1f;

    public void OnPointerClick(PointerEventData e)
    {
        if (Live && SoundManager.Instance != null) SoundManager.Instance.PlayUiTick();
    }

    void Update()
    {
        if (!Live) target = 1f;
        scale = Mathf.Lerp(scale, target, 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    void OnDisable()
    {
        over = false;
        scale = target = 1f;
        transform.localScale = Vector3.one;
    }
}
