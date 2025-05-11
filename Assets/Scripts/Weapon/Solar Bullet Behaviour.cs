using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class SolarBulletBehaviour : MonoBehaviour
{
    [SerializeField] public float speed=12,damage;
    [SerializeField] public bool Pierces, PiercesOnKill, ShieldBreaker, BreakFromObstacle, disappearOffScreen=true;
    public bool damageChanged=false;
    protected Rigidbody2D rb;
    [SerializeField] Buster.Projectile damageType;
    [SerializeField] AudioClip spawnSound;
    [SerializeField] List<ConditionLink> conditionalVFX = new List<ConditionLink>();
    AudioSource audioSource;
    bool enemyOwned;
    protected bool hitTarget=false;
    private void Start()
    {
        rb = gameObject.GetAny<Rigidbody2D>();
        audioSource = gameObject.GetAny<AudioSource>();
        if (audioSource&&spawnSound){audioSource.PlayOneShot(spawnSound);}
        enemyOwned=gameObject.CompareTag("Enemy Projectile");
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
            if (enemyOwned)
            {
                if (collision.CompareTag("Player"))
                {
                    CharControl player = collision.gameObject.GetAny<CharControl>();
                    player.HealthChange(-damage);
                    hitTarget=true;
                    if (!Pierces || (PiercesOnKill && !player.dead)){Destroy(gameObject);}
                }
                else if (BreakFromObstacle&&collision.gameObject.layer==LayerMask.NameToLayer("Terrain")){Destroy(gameObject);}
            }
            else
            {
                if (collision.CompareTag("Boss") || collision.CompareTag("Enemy") || (collision.CompareTag("Enemy Shield") && ShieldBreaker))
                {
                    EnemyHealth enemy = collision.gameObject.GetAny<EnemyHealth>();
                    enemy.TakeDamage(damage,(int)damageType);
                    hitTarget=true;
                    if (!Pierces || (PiercesOnKill && enemy.dead)){Destroy(gameObject);}
                }
                else if (collision.CompareTag("Enemy Shield")||(BreakFromObstacle&&collision.gameObject.layer==LayerMask.NameToLayer("Terrain"))){Destroy(gameObject);}
            }
        }
    }
    protected virtual void Move()
    {
        Vector2 moveDirection = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;
        rb.velocity = moveDirection.normalized * speed;
    }
    void OnBecameInvisible() {if (disappearOffScreen){Destroy(gameObject);}}
}
