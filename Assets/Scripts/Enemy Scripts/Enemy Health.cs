using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] protected float maxHealth;
    [SerializeField] bool Boss;
    [SerializeField] Image healthBar;
    protected float health;
    public bool dead=false;
    [SerializeField] List<Buster.Projectile> Weakness = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> HalfWeakness = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> Resistance = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> Immunity = new List<Buster.Projectile>();
    [SerializeField] List<Buster.Projectile> Heal = new List<Buster.Projectile>();
    [SerializeField] List<GameObject> drops = new List<GameObject>();
    [SerializeField] List<float> dropChances = new List<float>();
    private void Start()
    {
        health=maxHealth;
    }
    public void TakeDamage(float baseDamage, int damageType)
    {
        float damageMultiplier = 1.0f;
        if (Boss){baseDamage = 1;}
        if (Weakness.Contains((Buster.Projectile)damageType))
        {
            damageMultiplier = Boss?3:2; // Double damage for weaknesses
        }
        else if (HalfWeakness.Contains((Buster.Projectile)damageType))
        {
            damageMultiplier = Boss?2:1.5f; // 50% more damage for half-weakness
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
        if (health <= 0){dead=true;}
        if (healthBar!=null){healthBar.fillAmount = Mathf.Max(0, health / maxHealth);}
    }
    void OnDestroy()
    {
        if (drops.Count>1&&dead)
        {
            var drop = GetRandomDrop();
            if (drop!=null){Instantiate(drop,transform.position,Quaternion.identity);}
        }
    }
    public GameObject GetRandomDrop()
    {
        if (drops.Count != dropChances.Count || drops.Count == 0)
        {
            Debug.LogWarning("Drops and dropChances lists must be the same length and not empty.");
            return null;
        }
        float totalChance = dropChances.Sum();
        float randomValue = Random.Range(0f, totalChance);
        float cumulative = 0f;
        for (int i = 0; i < dropChances.Count; i++)
        {
            cumulative += dropChances[i];
            if (randomValue <= cumulative)
            {
                return drops[i];
            }
        }
        return drops[drops.Count - 1];
    }
}