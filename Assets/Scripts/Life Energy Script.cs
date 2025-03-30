using UnityEngine;
public class LifeEnergyScript : MonoBehaviour
{
    [SerializeField] float lifeEnergyGiven;
    [SerializeField] float weaponEnergyGiven;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.GetComponent<CharControl>()!=null&&lifeEnergyGiven!=0){collision.GetComponent<CharControl>().HealthChange(lifeEnergyGiven);}
            if (collision.GetComponent<Buster>()!=null&&weaponEnergyGiven!=0){collision.GetComponent<CharControl>().HealthChange(weaponEnergyGiven);}
            Destroy(gameObject);
        }
    }
}
