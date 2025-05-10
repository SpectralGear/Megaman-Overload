using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickFall : MonoBehaviour
{
    [SerializeField] float damage,maxFallSpeed,gravity;
    [SerializeField] Buster.Projectile damageType;
    [SerializeField] SurfaceDetectionViaTrigger floorContact;
    bool FloorContact => floorContact.InContact;
    Rigidbody2D rb;
    bool inWater=false;
    float currentFallSpeed=0;
    private void Start()
    {
        rb = gameObject.GetAny<Rigidbody2D>();
        Collider2D collider = gameObject.GetAny<Collider2D>();
        List<Collider2D> collisions = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        Physics2D.OverlapCollider(collider, filter, collisions);
        if (collisions.Count>0)
        {
            foreach (Collider2D collision in collisions)
            {
                if (rb.velocity.y<0)
                {
                    if (collision.gameObject.CompareTag("Boss")||collision.gameObject.CompareTag("Enemy"))
                    {
                        EnemyHealth enemy = collision.GetAny<EnemyHealth>();
                        enemy.TakeDamage(damage, (int)damageType);
                        if (!enemy.dead){Destroy(gameObject);}
                    }
                    else if (collision.gameObject.CompareTag("Enemy Shield"))
                    {
                        Destroy(gameObject);
                }
                }
                else if (collision.gameObject.layer==LayerMask.NameToLayer("Water"))
                {
                    inWater=true;
                }
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        Collision(collision);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer==LayerMask.NameToLayer("Water"))
        {
            inWater=true;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer==LayerMask.NameToLayer("Water"))
        {
            inWater=false;
        }
    }
    void Collision(Collision2D collision)
    {
        if (rb.velocity.y<0)
        {
            if (collision.gameObject.CompareTag("Boss")||collision.gameObject.CompareTag("Enemy"))
            {
                EnemyHealth enemy = collision.gameObject.GetAny<EnemyHealth>();
                enemy.TakeDamage(damage, (int)damageType);
                if (!enemy.dead){Destroy(gameObject);}
            }
            else if (collision.gameObject.CompareTag("Enemy Shield"))
            {
                Destroy(gameObject);
            }
        }
    }
    private void FixedUpdate()
    {
        Vector2 vector = Vector2.zero;
        vector.y=currentFallSpeed*(inWater?0.5f:1);
        rb.velocity=vector;
        if (!FloorContact)
        {
            currentFallSpeed=Mathf.Min(currentFallSpeed-(Time.deltaTime*gravity),maxFallSpeed);
        }
        else
        {
            currentFallSpeed=rb.velocity.y;
        }
    }
    private void OnBecameInvisible() 
    {
        Destroy(gameObject);
    }
}
