using UnityEngine;
public static class ComponentExtensions
{
    public static T GetAny<T>(this Component component) where T : Component
    {
        return component.GetComponent<T>()
            ?? component.GetComponentInChildren<T>()
            ?? component.GetComponentInParent<T>();
    }
    public static T GetAny<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>()
            ?? gameObject.GetComponentInChildren<T>()
            ?? gameObject.GetComponentInParent<T>();
    }
}
