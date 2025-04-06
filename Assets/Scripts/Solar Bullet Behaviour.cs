using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarBulletBehaviour : MonoBehaviour
{
    [SerializeField] protected float speed,damage;
    [SerializeField] bool Pierces, PiercesOnKill, ShieldBreaker;
    protected Vector2 vector2;
    protected Rigidbody2D rb;
    enum Projectile {NoCharge, HalfCharge, FullCharge, OverCharge, SickleChainShort, SickleChainLong, SafetyBall, BallBounce, SlagShot, SlagHammer, MegawattSurge, Brickfall, IfritBurstSmall, IfritBurstHuge, WaterHose, CycloneStrike, AnimalFriend}
    [SerializeField] Projectile damageType;
    [SerializeField] AudioClip spawnSound;
    AudioSource audioSource;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(spawnSound);
    }
    private void FixedUpdate()
    {
        Move();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Boss") || collision.CompareTag("Enemy") || (collision.CompareTag("Enemy Shield") && ShieldBreaker))&&collision.GetComponent<EnemyHealth>())
        {
            EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
            enemy.TakeDamage(damage,(int)damageType);
            if (!Pierces || (PiercesOnKill && !enemy.dead)){Destroy(gameObject);}
        }
        else if (collision.gameObject.CompareTag("Enemy Shield")){Destroy(gameObject);}
    }
    protected virtual void Move()
    {
        if (transform.localScale.x>0){vector2 = new Vector2(rb.position.x+speed,rb.position.y);}
        else {vector2 = new Vector2(rb.position.x-speed,rb.position.y);}
        rb.transform.position = vector2;
    }
    void OnBecameInvisible() {Destroy(gameObject);}
}
