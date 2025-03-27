using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeEnergyScript : MonoBehaviour
{
    [SerializeField] float energyAmountGiven;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<CharControl>().HealthChange(energyAmountGiven);
            Destroy(gameObject);
        }
    }
}
