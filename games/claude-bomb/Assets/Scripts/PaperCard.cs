using UnityEngine;
using UnityEngine.EventSystems;

// A page lying on the manual table. Click to read it up close; drag it onto the
// chat to attach it to the next message. Created at runtime by PaperDesk.
public class PaperCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    PaperDesk desk;
    Paper paper;

    public void Init(PaperDesk owner, Paper page)
    {
        desk = owner;
        paper = page;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left && !e.dragging) desk.OpenReader(paper);
    }

    public void OnBeginDrag(PointerEventData e) => desk.BeginCardDrag(paper, e.position);
    public void OnDrag(PointerEventData e) => desk.MoveCardDrag(e.position);
    public void OnEndDrag(PointerEventData e) => desk.EndCardDrag(paper, e.position);
}
