using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damageAmount);
            }
        }
    }
}
