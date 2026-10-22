using UnityEngine;
using UnityEngine.EventSystems;

// The black marker on the close-up page: press on any text to black it out (or
// un-black it), keep the button down and drag to paint more. The page text is a
// TMP text whose segments are <link>s; PaperDesk does the actual marking.
public class RedactableText : MonoBehaviour, IPointerDownHandler, IDragHandler, IInitializePotentialDragHandler
{
    PaperDesk desk;

    public void Init(PaperDesk owner) => desk = owner;

    // Paint from the first pixel of movement instead of after the drag threshold.
    public void OnInitializePotentialDrag(PointerEventData e) => e.useDragThreshold = false;

    public void OnPointerDown(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left) desk.MarkerDown(e.position);
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left) desk.MarkerDrag(e.position);
    }
}
