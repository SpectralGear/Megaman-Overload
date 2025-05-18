using System.Collections.Generic;
using Unity.VisualScripting;
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
    public static List<T> GetAll<T>(this Component component) where T : Component
    {
        List<T> components = new List<T>();
        components.AddRange(component.GetComponents<T>());
        components.AddRange(component.GetComponentsInChildren<T>());
        components.AddRange(component.GetComponentsInParent<T>());
        return components;
    }
    public static List<T> GetAll<T>(this GameObject gameObject) where T : Component
    {
        List<T> components = new List<T>();
        components.AddRange(gameObject.GetComponents<T>());
        components.AddRange(gameObject.GetComponentsInChildren<T>());
        components.AddRange(gameObject.GetComponentsInParent<T>());
        return components;
    }
}
