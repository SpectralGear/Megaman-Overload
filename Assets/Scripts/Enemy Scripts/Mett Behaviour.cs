using System.Collections.Generic;
using UnityEngine;

public class MettBehaviour : MonoBehaviour
{
    Animator anim;
    [SerializeField] EnemyHealth HP;
    [SerializeField] GameObject HidingCollision,StandingCollision,bullet,bulletSpawn;
    [SerializeField] GameObject Explosion;
    GameObject ClosestPlayer;
    bool facingRight => transform.localScale.x>0;
    bool attacking;
    List<GameObject> PlayersInVicinity = new List<GameObject>();
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        HidingCollision.SetActive(true);
        StandingCollision.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!PlayersInVicinity.Contains(collision.gameObject))
            {
                PlayersInVicinity.Add(collision.gameObject);
                SortList();
                if (ClosestPlayer)
                {
                    if (ClosestPlayer.gameObject.transform.position.x<transform.position.x){transform.localScale=new Vector3(-1,1,1);}
                    else if (ClosestPlayer.gameObject.transform.position.x>transform.position.x){transform.localScale=new Vector3(1,1,1);}
                }
            }
            else if (Vector2.Distance(transform.position, collision.gameObject.transform.position)<1)
            {collision.gameObject.GetAny<CharControl>().HealthChange(-2);}
            if (!attacking){attacking=true;Attack();}
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (PlayersInVicinity.Contains(collision.gameObject)&&Vector2.Distance(collision.gameObject.transform.position, transform.position)>2)
        {
            PlayersInVicinity.Remove(collision.gameObject);
        }
    }
    void LateUpdate()
    {
        if (HP.dead)
        {
            Destroy(gameObject);
            GameObject boom = Instantiate(Explosion,new Vector3(transform.position.x,transform.position.y+0.48f,transform.position.z),Quaternion.identity);
            boom.GetComponent<ExplosionBehavior>().volume=1;
        }
    }
    private void FixedUpdate()
    {
        SortList();
        if (ClosestPlayer)
        {
            if (ClosestPlayer.gameObject.transform.position.x<transform.position.x){transform.localScale=new Vector3(-1,1,1);}
            else if (ClosestPlayer.gameObject.transform.position.x>transform.position.x){transform.localScale=new Vector3(1,1,1);}
        }
        foreach (GameObject player in PlayersInVicinity)
        {
            if (Vector2.Distance(player.transform.position, transform.position)<1)
            {
                player.GetAny<CharControl>().HealthChange(-2);
            }
        }
    }
    void Attack()
    {
        if (PlayersInVicinity.Count<1){attacking=false;return;}
        HidingCollision.SetActive(false);
        StandingCollision.SetActive(true);
        anim.SetBool("Shoot",true);
        anim.Update(0f);
        Invoke("Fire",0.3f);
    }
    void Fire()
    {
        int angle = 45;
        if (!facingRight){angle+=180;}
        for (int i = 0; i < 3; i++)
        {
            Instantiate(bullet,bulletSpawn.transform.position,Quaternion.Euler(0,0,angle));
            angle-=45;
        }
        Invoke("Hide",0.1f);
    }
    void Hide()
    {
        HidingCollision.SetActive(true);
        StandingCollision.SetActive(false);
        anim.SetBool("Shoot",false);
        anim.Update(0f);
        Invoke("Attack",1);
    }
    void SortList()
    {
        PlayersInVicinity.RemoveAll(player => player == null);
        if (PlayersInVicinity.Count>1)
        {
            PlayersInVicinity.Sort
            (
                (a,b) =>
                {
                    float distA = Vector2.Distance(transform.position, a.transform.position);
                    float distB = Vector2.Distance(transform.position, b.transform.position);
                    return distA.CompareTo(distB);
                }
            );
        }
        if (PlayersInVicinity.Count>0&&ClosestPlayer != PlayersInVicinity[0])
        {
            ClosestPlayer = PlayersInVicinity[0];
        }
    }
    void OnBecameInvisible() {enabled=false;}
    void OnBecameVisible() {enabled=true;}
}
