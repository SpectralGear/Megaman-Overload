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
    public Buster.Projectile damageType;
    private void Start()
    {
        GameObject Boom = Instantiate(ExplosionEffect,transform.position,Quaternion.identity);
        Boom.transform.localScale = transform.localScale;
        Destroy(gameObject,0.2f);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Boss")||collision.CompareTag("Enemy"))&&(playerOwned||friendlyFire))
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(damage,(int)damageType);
        }
        if (collision.CompareTag("Player")&&(!playerOwned||friendlyFire))
        {
            collision.GetComponent<CharControl>().HealthChange(-damage);
        }
        if (collision.CompareTag("Destructable"))
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(float.PositiveInfinity,(int)damageType);
        }
    }
}
