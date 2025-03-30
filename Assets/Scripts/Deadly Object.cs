using UnityEngine;
public class DeadlyObject : MonoBehaviour
{
    [SerializeField] float damageDealt;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<CharControl>())
        {
            collision.gameObject.GetComponent<CharControl>().HealthChange(-damageDealt);
        }
    }
}

