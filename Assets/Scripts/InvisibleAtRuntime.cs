using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class InvisibleAtRuntime : MonoBehaviour
{
    private Renderer tilemapRenderer;

    void Awake()
    {
        tilemapRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Editor: show the tilemap
            if (tilemapRenderer != null)
                tilemapRenderer.enabled = true;
        }
        else
        {
            // Runtime: hide the tilemap
            if (tilemapRenderer != null)
                tilemapRenderer.enabled = false;
        }
#else
        // Build: always hide
        if (tilemapRenderer != null)
            tilemapRenderer.enabled = false;
#endif
    }
}