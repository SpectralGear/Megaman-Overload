using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]protected float maxHealth;
    protected float health;
    public bool dead=false;
    enum Projectile {NoCharge, HalfCharge, FullCharge, OverCharge, SickleChainShort, SickleChainLong, SafetyBall, BallBounce, SlagShot, SlagHammer, MegawattSurge, Brickfall, IfritBurstSmall, IfritBurstHuge, WaterHose, CycloneStrike, AnimalFriend}
    [SerializeField] List<Projectile> Weakness = new List<Projectile>();
    [SerializeField] List<Projectile> HalfWeakness = new List<Projectile>();
    [SerializeField] List<Projectile> Resistance = new List<Projectile>();
    [SerializeField] List<Projectile> Immunity = new List<Projectile>();
    [SerializeField] List<Projectile> Heal = new List<Projectile>();
    private void Start()
    {
        health=maxHealth;
    }
    public void TakeDamage(float baseDamage, int damageType)
    {
        float damageMultiplier = 1.0f;
        if (Weakness.Contains((Projectile)damageType))
        {
            damageMultiplier = 2.0f; // Double damage for weaknesses
        }
        else if (HalfWeakness.Contains((Projectile)damageType))
        {
            damageMultiplier = 1.5f; // 50% more damage for half-weakness
        }
        else if (Resistance.Contains((Projectile)damageType))
        {
            damageMultiplier = 0.5f; // Half damage for resistance
        }
        else if (Immunity.Contains((Projectile)damageType))
        {
            damageMultiplier = 0.0f;// No damage for immunity
        }
        else if (Heal.Contains((Projectile)damageType))
        {
            damageMultiplier = -1.0f;// Reverse damage for heal
        }
        float finalDamage = baseDamage * damageMultiplier;
        health -= finalDamage;
        health = Mathf.Clamp(health,0,health);
        if (health <= 0){dead=true;}
        
    }
}