using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class EllipseCollider2D : MonoBehaviour
{
    private PolygonCollider2D polyCollider;

    [Header("Ellipse Settings")]
    public Vector2 baseSize = new Vector2(1f, 1f); // Base width and height
    public int resolution = 32; // Number of points on the ellipse

    void Start()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        UpdateEllipse();
    }

    void Update()
    {
        if (transform.hasChanged) // Detects scale/transform changes
        {
            UpdateEllipse();
            transform.hasChanged = false; // Reset flag
        }
    }

    void UpdateEllipse()
    {
        // Use baseSize directly to avoid double-scaling
        float a = 0.5f * baseSize.x; // X radius (width)
        float b = 0.5f * baseSize.y; // Y radius (height)

        Vector2[] points = new Vector2[resolution];
        float angleStep = 2 * Mathf.PI / resolution;

        for (int i = 0; i < resolution; i++)
        {
            float angle = i * angleStep;
            float x = a * Mathf.Cos(angle);
            float y = b * Mathf.Sin(angle);
            points[i] = new Vector2(x, y);
        }

        polyCollider.SetPath(0, points);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // Apply localScale to match the ball's scale
        float a = 0.5f * baseSize.x * transform.localScale.x; // X radius
        float b = 0.5f * baseSize.y * transform.localScale.y; // Y radius

        Vector3 position = transform.position;

        for (int i = 0; i < resolution; i++)
        {
            float angle = 2 * Mathf.PI * i / resolution;
            float x = a * Mathf.Cos(angle);
            float y = b * Mathf.Sin(angle);

            Gizmos.DrawSphere(position + new Vector3(x, y, 0), 0.02f);
        }
    }

    // Call this method to manually update the collider if needed
    public void SetBaseSize(Vector2 newSize)
    {
        baseSize = newSize;
        UpdateEllipse();
    }
}
