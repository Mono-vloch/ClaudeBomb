using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Orbit camera around the bomb. Right-click-drag rotates freely (yaw AND pitch,
// no clamp - full 360 both ways so the player can inspect every side). Mouse
// scroll and W/S both control distance (closer/farther). A/D pan the look-at
// point left/right (screen-relative) instead of rotating, so the player can
// shift focus onto a specific module without doing a full orbit.
// All keyboard/scroll camera input is suppressed while the chat input field is
// focused, so typing "wasd" into a message doesn't also move the camera.
public class BombCameraController : MonoBehaviour
{
    [SerializeField] Transform target; // empty object at the bomb's visual center to orbit around
    [SerializeField] float distance = 1f;
    [SerializeField] float minDistance = 0.75f;
    [SerializeField] float maxDistance = 1.2f;
    [SerializeField] float zoomSpeed = 0.5f;
    [SerializeField] bool logScrollValue = false; // dev-only: prints the raw scroll delta if you need to recalibrate

    [SerializeField] float rotationSpeed = 0.2f; // degrees per pixel of mouse drag
    [SerializeField] float keyboardZoomSpeed = 2f;  // units per second, W/S
    [SerializeField] float keyboardPanSpeed = 1f;   // units per second, A/D
    [SerializeField] float maxPanDistance = 1f;     // how far left/right the look-at point can shift

    float yaw;
    float pitch;
    bool dragging;
    Vector2 lastMousePosition;
    Vector3 panOffset;

    void Start()
    {
        // Start from wherever the camera already is, so it doesn't snap on frame 1.
        Vector3 offset = transform.position - target.position;
        distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);

        Vector3 euler = Quaternion.LookRotation(-offset.normalized).eulerAngles;
        yaw = euler.y;
        pitch = NormalizePitch(euler.x);

        UpdateCameraTransform();
    }

    void Update()
    {
        if (target == null) return;

        bool typing = IsTypingInInputField();

        if (Mouse.current != null)
        {
            HandleMouseRotation();
            if (!typing) HandleScrollZoom();
        }

        if (Keyboard.current != null && !typing)
            HandleKeyboardMovement();

        UpdateCameraTransform();
    }

    static bool IsTypingInInputField()
    {
        if (EventSystem.current == null) return false;
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        return selected != null && selected.GetComponent<TMP_InputField>() != null;
    }

    void HandleMouseRotation()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            dragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            dragging = false;
        }

        if (!dragging) return;

        Vector2 current = Mouse.current.position.ReadValue();
        Vector2 delta = current - lastMousePosition;
        lastMousePosition = current;

        yaw += delta.x * rotationSpeed;
        pitch -= delta.y * rotationSpeed; // no clamp - fully free rotation
    }

    void HandleScrollZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.001f) return;

        if (logScrollValue) Debug.Log($"[BombCameraController] raw scroll value: {scroll}");

        // Scroll is a discrete per-event delta, not a held value - no Time.deltaTime here.
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
    }

    void HandleKeyboardMovement()
    {
        float distanceInput = 0f;
        if (Keyboard.current.wKey.isPressed) distanceInput -= 1f;
        if (Keyboard.current.sKey.isPressed) distanceInput += 1f;
        if (distanceInput != 0f)
            distance = Mathf.Clamp(distance + distanceInput * keyboardZoomSpeed * Time.deltaTime, minDistance, maxDistance);

        float panInput = 0f;
        if (Keyboard.current.dKey.isPressed) panInput += 1f;
        if (Keyboard.current.aKey.isPressed) panInput -= 1f;
        if (panInput != 0f)
        {
            panOffset += transform.right * panInput * keyboardPanSpeed * Time.deltaTime;
            panOffset = Vector3.ClampMagnitude(panOffset, maxPanDistance);
        }
    }

    void UpdateCameraTransform()
    {
        Vector3 pivot = target.position + panOffset;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 position = pivot - rotation * Vector3.forward * distance;
        transform.SetPositionAndRotation(position, rotation);
    }

    static float NormalizePitch(float angle) => angle > 180f ? angle - 360f : angle;
}
