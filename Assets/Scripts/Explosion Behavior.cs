using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehavior : MonoBehaviour
{
    [SerializeField] GameObject ExplosionEffect;
    public float damage;
    public bool friendlyFire;
    public bool playerOwned;
    public bool destroyEnvironment;
    [SerializeField] public float volume;
    public Buster.Projectile damageType;
    private void Start()
    {
        GameObject Boom = Instantiate(ExplosionEffect,transform.position,Quaternion.identity);
        Boom.transform.localScale = transform.localScale;
        Boom.GetComponent<AudioSource>().volume=Mathf.Clamp(volume,0,1);
        Destroy(gameObject,0.2f);
    }
    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();
    void Update()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, GetComponent<CircleCollider2D>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y));
        foreach (Collider2D collision in hits)
        {
            if (!alreadyHit.Contains(collision))
            {
                if ((collision.CompareTag("Boss")||collision.CompareTag("Enemy"))&&(playerOwned||friendlyFire))
                {
                    var health = collision.GetComponent<EnemyHealth>();
                    if (health != null) {health.TakeDamage(damage, (int)damageType);}
                    alreadyHit.Add(collision);
                }
                else if (collision.CompareTag("Player")&&(!playerOwned||friendlyFire))
                {
                    var health = collision.GetComponent<CharControl>();
                    if (health != null) {health.HealthChange(-damage);}
                    alreadyHit.Add(collision);
                }
                else if (collision.CompareTag("Destructable"))
                {
                    Destroy(collision.gameObject);
                }
            }
        }
    }
}
