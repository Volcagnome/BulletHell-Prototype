using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Damage_MeleeCollider : Damage
{
    private void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.CompareTag("Player"))
        {
            I_Damage dmg = other.GetComponentInParent<I_Damage>();

            if (dmg != null)
            {
                dmg.TakeDamage(damageAmount);
                gameObject.SetActive(false);

            }
        }
    }

}
