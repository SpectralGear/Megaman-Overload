using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class InvisibleAtRuntime : MonoBehaviour
{
    [SerializeField] bool UnrenderSelf, UnrenderParent, UnrenderChild;
    private List<Renderer> renderers = new List<Renderer>();
    void Awake()
    {
        if (UnrenderSelf){renderers.AddRange(GetComponents<Renderer>());}
        if (UnrenderParent){renderers.AddRange(GetComponentsInParent<Renderer>(true));}
        if (UnrenderChild){renderers.AddRange(GetComponentsInParent<Renderer>(true));}
    }
    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Editor: show the tilemap
            if (renderers != null)
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null){renderer.enabled=true;}
                }
        }
        else
        {
            // Runtime: hide the tilemap
            if (renderers != null)
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null){renderer.enabled=false;}
                }
        }
#else
        // Build: always hide
        if (renderers != null)
            foreach (Renderer renderer in renderers)
                {
                    if (renderer != null){renderer.enabled=true;}
                }
#endif
    }
}