using UnityEngine;

public class TileNeighborCheck : MonoBehaviour
{
    public LayerMask tileLayer; // Layer where all tiles exist
    private Renderer rend;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        CheckNeighbors();
    }

    void CheckNeighbors()
    {
        Vector4 hiddenFaces = Vector4.zero;

        // Check in all 6 directions
        if (Physics.Raycast(transform.position, Vector3.left, 1f, tileLayer)) hiddenFaces.x = 1;  // Left
        if (Physics.Raycast(transform.position, Vector3.right, 1f, tileLayer)) hiddenFaces.y = 1; // Right
        if (Physics.Raycast(transform.position, Vector3.up, 1f, tileLayer)) hiddenFaces.z = 1;    // Top
        if (Physics.Raycast(transform.position, Vector3.down, 1f, tileLayer)) hiddenFaces.w = 1;  // Bottom

        // Apply to shader
        rend.material.SetVector("_HiddenFaces", hiddenFaces);
    }
}
