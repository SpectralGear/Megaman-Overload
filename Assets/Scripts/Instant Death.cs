using UnityEngine;
public class InstantDeath : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D collision)
    {
        var player = collision.GetComponent<CharControl>();
        if (collision.CompareTag("Player")&&player)
        {
            if (!player.dead){player.Instantdeath();}
        }
    }
}
