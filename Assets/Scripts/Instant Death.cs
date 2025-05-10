using UnityEngine;
public class InstantDeath : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D collision)
    {
        var player = collision.GetAny<CharControl>();
        if (collision.CompareTag("Player")&&player)
        {
            if (!player.dead){player.Instantdeath();}
        }
    }
}
