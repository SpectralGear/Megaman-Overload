using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

public class SolarBulletBehaviour : MonoBehaviour
{
    [SerializeField] public float speed=12,damage;
    [SerializeField] public bool Pierces, PiercesOnKill, ShieldBreaker;
    [SerializeField] public bool BreakFromObstacle;
    public bool damageChanged=false;
    protected Rigidbody2D rb;
    [SerializeField] Buster.Projectile damageType;
    [SerializeField] AudioClip spawnSound;
    [SerializeField] List<ConditionLink> conditionalVFX = new List<ConditionLink>();
    AudioSource audioSource;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(spawnSound);
        UpdateVFX();
    }
    public void UpdateVFX()
    {
        foreach (var link in conditionalVFX) {
            FieldInfo field = GetType().GetField(link.boolFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(bool)) {
                Debug.LogWarning($"Invalid bool field: {link.boolFieldName}");
                continue;
            }
            bool value = (bool)field.GetValue(this);
            bool condition = link.reverseCondition ? !value : value;
            if (link.targetVFX != null) {
                link.targetVFX.gameObject.SetActive(condition);
            }
        }
    }
    public void damageChange(float amountChanged)
    {
        damageChanged=amountChanged!=0;
        damage+=amountChanged;
        UpdateVFX();
    }
    private void FixedUpdate()
    {
        Move();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damage!=0)
        {
            if ((collision.CompareTag("Boss") || collision.CompareTag("Enemy") || (collision.CompareTag("Enemy Shield") && ShieldBreaker))&&collision.GetComponent<EnemyHealth>())
            {
                EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
                enemy.TakeDamage(damage,(int)damageType);
                if (!Pierces || (PiercesOnKill && !enemy.dead)){Destroy(gameObject);}
            }
            else if (collision.CompareTag("Enemy Shield")||(BreakFromObstacle&&collision.gameObject.layer==LayerMask.NameToLayer("Terrain"))){Destroy(gameObject);}
        }
    }
    protected virtual void Move()
    {
        Vector2 moveDirection = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;
        rb.velocity = moveDirection.normalized * speed;
    }
    void OnBecameInvisible() {Destroy(gameObject);}
}
