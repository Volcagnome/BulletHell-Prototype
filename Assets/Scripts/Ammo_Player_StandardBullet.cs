using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo_Player_StandardBullet : Ammo_Player_PlayerBullet
{
    override protected void OnTriggerEnter(Collider other)
    {
        //If other is the player, exits function.
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Player Projectile")) return;

        else
        {
            //Checks if triggering object can be damaged, if so applies damage.
            I_Damage dmg = other.GetComponentInParent<I_Damage>();

            if (dmg != null)
                dmg.TakeDamage(damageAmount);

            ReturnToPool();
        }
    }
}


