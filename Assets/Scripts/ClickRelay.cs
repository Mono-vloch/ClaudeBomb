using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Shared by every module: makes a 3D object clickable and forwards the click with an
// index. Does its own raycast against the new Input System's mouse (Mouse.current)
// instead of relying on OnMouseDown, which never fires when Active Input Handling is
// set to "Input System Package (New)" - the default for this project.
public class ClickRelay : MonoBehaviour
{
    public int Index { get; private set; }
    public bool DebugLog { get; set; }
    Action<int> callback;
    static Camera cam;

    // Ensures target has a properly-fitted Collider, without attaching a ClickRelay -
    // for modules (like wires) that drive their own input handling but still want the
    // same reliable collider-fitting behaviour.
    public static void EnsureCollider(GameObject target)
    {
        if (target.GetComponentInChildren<Collider>() == null)
            FitCollider(target, target.AddComponent<BoxCollider>());
    }

    // debugLog: pass true only on the object(s) you're actively troubleshooting -
    // logging every relay at once (there are ~20 in this game) is unreadable.
    public static ClickRelay Attach(GameObject target, int index, Action<int> onClick, bool debugLog = false)
    {
        EnsureCollider(target);

        var relay = target.GetComponent<ClickRelay>();
        if (relay == null) relay = target.AddComponent<ClickRelay>();
        relay.Index = index;
        relay.callback = onClick;
        relay.DebugLog = debugLog;
        return relay;
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (DebugLog) Debug.Log($"[ClickRelay:{name}] click ignored - pointer is over a UI element");
            return;
        }

        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            if (DebugLog) Debug.Log($"[ClickRelay:{name}] no Camera.main found");
            return;
        }

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (DebugLog) Debug.Log($"[ClickRelay:{name}] raycast hit '{hit.collider.gameObject.name}' (this object is '{name}')");
            if (hit.collider.gameObject == gameObject)
                callback?.Invoke(Index);
        }
        else
        {
            if (DebugLog) Debug.Log($"[ClickRelay:{name}] raycast hit nothing");
        }
    }

    // A BoxCollider added via script (not through the Editor's Add Component button)
    // does NOT auto-fit to the mesh - it defaults to a 1x1x1 box at the origin. Fit it
    // to the object's own mesh bounds, or its renderers' combined bounds if the mesh
    // lives on a child (common with imported models).
    static void FitCollider(GameObject target, BoxCollider box)
    {
        var meshFilter = target.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            box.center = meshFilter.sharedMesh.bounds.center;
            box.size = meshFilter.sharedMesh.bounds.size;
            return;
        }

        var renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        Vector3 lossy = target.transform.lossyScale;
        box.center = target.transform.InverseTransformPoint(worldBounds.center);
        box.size = new Vector3(
            Mathf.Abs(lossy.x) > 0.0001f ? worldBounds.size.x / Mathf.Abs(lossy.x) : worldBounds.size.x,
            Mathf.Abs(lossy.y) > 0.0001f ? worldBounds.size.y / Mathf.Abs(lossy.y) : worldBounds.size.y,
            Mathf.Abs(lossy.z) > 0.0001f ? worldBounds.size.z / Mathf.Abs(lossy.z) : worldBounds.size.z);
    }
}
