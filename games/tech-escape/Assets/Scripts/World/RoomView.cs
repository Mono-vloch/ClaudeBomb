using UnityEngine;

// How a room is seen from inside: the field of view its camera point was measured for
// (Tech Escape > Build facility kit fits each room's prop to about the same share of the
// frame, so a tall tape library gets a wider lens than a lockbox on a desk).
public class RoomView : MonoBehaviour
{
    public float fov = 48f;
}
