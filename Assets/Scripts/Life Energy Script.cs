using UnityEngine;
public class LifeEnergyScript : MonoBehaviour
{
    [SerializeField] float lifeEnergyGiven;
    [SerializeField] float weaponEnergyGiven;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var cc = collision.GetAny<CharControl>();
            var buster = collision.GetAny<Buster>();
            if (cc&&lifeEnergyGiven!=0){cc.HealthChange(lifeEnergyGiven);}
            if (buster&&weaponEnergyGiven!=0)
            {
                buster.WeaponEnergy[0]+=weaponEnergyGiven;
            }
            Destroy(gameObject);
        }
    }
}
