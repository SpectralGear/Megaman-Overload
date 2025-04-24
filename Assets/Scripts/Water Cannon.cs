using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCannon : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] Buster.Projectile damageType;
    [SerializeField] GameObject InAirEffect,InWaterEffect;
    List<GameObject> objects = new List<GameObject>();
    void Start()
    {
        Invoke("SelfDestruct",0.4f);
        bool inWater = false;
        Collider2D[] waterHits = Physics2D.OverlapPointAll(transform.position, LayerMask.NameToLayer("Water"));
        foreach (var hit in waterHits)
        {
            if (hit.isTrigger)
            {
                inWater = true;
                break;
            }
        }
        InWaterEffect.SetActive(inWater);
        InAirEffect.SetActive(!inWater);
        Collider2D collider;
        if (inWater){collider = InWaterEffect.GetComponent<Collider2D>();}
        else {collider = InAirEffect.GetComponent<Collider2D>();}
        List<Collider2D> collisions = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        Physics2D.OverlapCollider(collider, filter, collisions);
        if (collisions.Count>0)
        {
            foreach (Collider2D collision in collisions)
            {
                if ((collision.CompareTag("Boss")||collision.CompareTag("Enemy")||collision.CompareTag("Enemy Shield"))&&!objects.Contains(collision.gameObject))
                {
                    collision.GetComponent<EnemyHealth>().TakeDamage(damage, (int)damageType);
                    objects.Add(collision.gameObject);
                }
                else if (collision.CompareTag("Destructable"))
                {
                    Destroy(collision.gameObject);
                }
            }
        }
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Boss")||collision.CompareTag("Enemy")||collision.CompareTag("Enemy Shield"))&&!objects.Contains(collision.gameObject))
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(damage, (int)damageType);
            objects.Add(collision.gameObject);
        }
        else if (collision.CompareTag("Destructable"))
        {
            Destroy(collision.gameObject);
        }
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("Boss")||collision.CompareTag("Enemy")||collision.CompareTag("Enemy Shield"))&&!objects.Contains(collision.gameObject))
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(damage, (int)damageType);
            objects.Add(collision.gameObject);
        }
        else if (collision.CompareTag("Destructable"))
        {
            Destroy(collision.gameObject);
        }
    }
    void SelfDestruct()
    {
        //bool SelfDestruct = true;
        //ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        //foreach (ParticleSystem particleSystem in particleSystems)
        //{
        //    if (particleSystem.particleCount>0)
        //    {
        //        SelfDestruct = false;
        //    }
        //}
        //if (SelfDestruct)
        {
            Destroy(gameObject);
        }
    }
}
