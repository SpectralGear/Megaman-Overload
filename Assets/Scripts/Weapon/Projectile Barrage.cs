using System.Collections.Generic;
using UnityEngine;

public class ProjectileBarrage : MonoBehaviour
{
    List<GameObject> projectileList;

    void Start()
    {
        projectileList = new List<GameObject>();

        // Collect all direct child GameObjects
        foreach (Transform child in transform)
        {
            if (child.gameObject != gameObject)
            {
                projectileList.Add(child.gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        // Check if all child projectiles are null (destroyed)
        bool allNull = true;
        foreach (var projectile in projectileList)
        {
            if (projectile != null)
            {
                allNull = false;
                break;
            }
        }

        if (allNull)
        {
            Destroy(gameObject);
        }
    }
}