using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MettBehaviour : MonoBehaviour
{
    Animator anim;
    EnemyHealth HP;
    [SerializeField] GameObject HidingCollision,StandingCollision,bullet,bulletSpawn;
    bool detected,contact;
    CharControl Player;
    bool facingRight => transform.localScale.x>0;
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        HP = GetComponent<EnemyHealth>();
        HidingCollision.SetActive(!anim.GetBool("Shoot"));
        StandingCollision.SetActive(anim.GetBool("Shoot"));
        HP.enabled=anim.GetBool("Shoot");
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("triggered");
        Player = collision.gameObject.GetComponent<CharControl>();
        if (collision.gameObject.CompareTag("Player")&&Player)
        {
            if (!detected){detected=true;Attack();}
            else {contact=true;}

        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (contact){contact=false;}
        else if (detected){detected = false;}
    }
    void Update()
    {
        if (detected&&Player)
        {
            if (Player.gameObject.transform.position.x<transform.position.x){transform.localScale=new Vector3(-1,1,1);}
            else if (Player.gameObject.transform.position.x>transform.position.x){transform.localScale=new Vector3(1,1,1);}
            if (contact){Player.HealthChange(2);}
        }
        HidingCollision.SetActive(!anim.GetBool("Shoot"));
        StandingCollision.SetActive(anim.GetBool("Shoot"));
        HP.enabled=anim.GetBool("Shoot");
        if (HP.dead){Destroy(gameObject);}
    }
    void Attack()
    {
        anim.SetBool("Shoot",true);
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
        anim.SetBool("Shoot",false);
        if (detected){Invoke("Attack",1);}
    }
}
