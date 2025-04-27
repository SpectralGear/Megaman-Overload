using System.Collections.Generic;
using UnityEngine;

public class SurfaceDetectionViaTrigger : MonoBehaviour
{
    [SerializeField] List<string> TagsToIgnore = new List<string>();
    private bool _InContact = false;
    public bool InContact => _InContact;
    private HashSet<GameObject> gameObjects = new HashSet<GameObject>();

    void OnTriggerEnter2D(Collider2D collision)
    {
        AddGameObjectToSet(collision.gameObject);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        AddGameObjectToSet(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (gameObjects.Contains(collision.gameObject))
        {
            gameObjects.Remove(collision.gameObject);
            _InContact = gameObjects.Count > 0;
        }
    }

    private void AddGameObjectToSet(GameObject obj)
    {
        if (gameObjects.Add(obj)&&!TagsToIgnore.Contains(obj.tag))  // Only add if it's not already in the set
        {
            _InContact = true;  // We know we're in contact because we just added a new object
        }
    }
}