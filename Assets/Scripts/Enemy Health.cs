using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] protected float maxHealth;
    [SerializeField] bool Boss;
    protected float health;
    public bool dead=false;
    [SerializeField] List<Buster.Projectile> Weakness = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> HalfWeakness = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> Resistance = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> Immunity = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> Heal = new List<Buster.Projectile>();
    private void Start()
    {
        health=maxHealth;
    }
    public void TakeDamage(float baseDamage, int damageType)
    {
        float damageMultiplier = 1.0f;
        if (Weakness.Contains((Buster.Projectile)damageType))
        {
            damageMultiplier = 2.0f; // Double damage for weaknesses
        }
        else if (HalfWeakness.Contains((Buster.Projectile)damageType))
        {
            damageMultiplier = 1.5f; // 50% more damage for half-weakness
        }
        else if (Resistance.Contains((Buster.Projectile)damageType))
        {
            damageMultiplier = 0.5f; // Half damage for resistance
        }
        else if (Immunity.Contains((Buster.Projectile)damageType))
        {
            damageMultiplier = 0.0f;// No damage for immunity
        }
        else if (Heal.Contains((Buster.Projectile)damageType))
        {
            damageMultiplier = -1.0f;// Reverse damage for heal
        }
        float finalDamage = baseDamage * damageMultiplier;
        health -= finalDamage;
        health = Mathf.Clamp(health,0,health);
        if (health <= 0){dead=true;}
        
    }
}