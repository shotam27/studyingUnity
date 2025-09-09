using UnityEngine;

[AddComponentMenu("Game/GameManager")]
public class GameManager : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera to control. If empty, Camera.main will be used.")]
    public Camera controlledCamera;

    [Header("Movement")]
    public float speed = 10f;                      // base move speed (units/sec)
    public float sprintMultiplier = 2f;            // multiplier when holding Shift
    public bool smoothMovement = true;
    public float smoothSpeed = 8f;                 // higher = snappier

    [Header("Plane")]
    [Tooltip("If true, camera moves on X/Y plane (2D projects). Otherwise moves on X/Z (3D top-down).")]
    public bool moveOnXY = true;

    [Header("Bounds (optional)")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-50, -50);
    public Vector2 maxBounds = new Vector2(50, 50);

    [Header("Zoom (mouse wheel)")]
    public bool allowZoom = true;
    public float zoomSpeed = 5f;
    public float minOrthoSize = 2f;
    public float maxOrthoSize = 50f;

    Vector3 targetPosition;

    void Awake()
    {
        if (controlledCamera == null) controlledCamera = Camera.main;
        if (controlledCamera == null) Debug.LogWarning("GameManager: No camera assigned and Camera.main is null.");
        if (controlledCamera != null) targetPosition = controlledCamera.transform.position;
    }

    void Update()
    {
        if (controlledCamera == null) return;

        // Read input (WASD or arrow keys map to Horizontal/Vertical)
        float h = Input.GetAxisRaw("Horizontal"); // A/D / Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S / Up/Down

        float sprint = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? sprintMultiplier : 1f;

        Vector3 move = Vector3.zero;
        if (moveOnXY)
        {
            // X = horizontal, Y = vertical
            move = new Vector3(h, v, 0f) * speed * sprint * Time.deltaTime;
        }
        else
        {
            // X = horizontal, Z = vertical input
            move = new Vector3(h, 0f, v) * speed * sprint * Time.deltaTime;
        }

        targetPosition += move;

        if (useBounds)
        {
            if (moveOnXY)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }
            else
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
            }
        }

        // Zoom (mouse wheel) — supports orthographic camera
        if (allowZoom && controlledCamera.orthographic)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                float size = controlledCamera.orthographicSize - scroll * zoomSpeed;
                controlledCamera.orthographicSize = Mathf.Clamp(size, minOrthoSize, maxOrthoSize);
            }
        }
    }

    void LateUpdate()
    {
        if (controlledCamera == null) return;

        if (smoothMovement)
        {
            controlledCamera.transform.position = Vector3.Lerp(controlledCamera.transform.position, targetPosition, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        }
        else
        {
            controlledCamera.transform.position = targetPosition;
        }
    }

    // Utility to reset camera to origin
    [ContextMenu("Reset Camera Position")]
    public void ResetCameraPosition()
    {
        if (controlledCamera == null) return;
        targetPosition = controlledCamera.transform.position = Vector3.zero;
    }
}
