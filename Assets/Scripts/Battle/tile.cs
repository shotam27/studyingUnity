using UnityEngine;
using UnityEngine.EventSystems;

// Tile component: stores grid coordinates and logs them when clicked.
// Works with both OnMouseDown (classic) and IPointerClickHandler (EventSystem + PhysicsRaycaster).
public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Grid X coordinate")]
    public int gridX;
    [Tooltip("Grid Y coordinate")]
    public int gridY;

    void Awake()
    {
        EnsureCollider();
    }

    void EnsureCollider()
    {
        // If neither 3D nor 2D collider exists, attempt to add a suitable one so clicks work.
        if (GetComponent<Collider>() == null && GetComponent<Collider2D>() == null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                gameObject.AddComponent<BoxCollider2D>();
            }
            else
            {
                // Fallback to 3D collider
                gameObject.AddComponent<BoxCollider>();
            }
        }
    }

    // For EventSystem-based clicks (requires an EventSystem and a PhysicsRaycaster on the Camera for 3D colliders)
    public void OnPointerClick(PointerEventData eventData)
    {
        PrintCoords("PointerClick");
    }

    // For classic OnMouseDown (works if object has a Collider)
    void OnMouseDown()
    {
        PrintCoords("OnMouseDown");
    }

    private void PrintCoords(string source)
    {
        Debug.Log($"[Tile] Clicked ({source}) grid=({gridX},{gridY}) world={transform.position} name={gameObject.name}");
    }

    // Convenience setter
    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    gameObject.name = $"x{gridX}y{gridY}";
    }
}
