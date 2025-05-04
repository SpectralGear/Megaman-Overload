using UnityEngine;

[ExecuteAlways]
public class CameraViewGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.blue;

    void OnDrawGizmos()
    {
        Camera cam = GetComponent<Camera>();
        if (!cam || !cam.enabled || !cam.isActiveAndEnabled) return;

        // The z distance from the camera to the z = 0 plane
        float zDistance = -cam.transform.position.z;

        // Four corners of the frustum at z = 0
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, zDistance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, zDistance));
        Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, zDistance));

        // Draw rectangle
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}