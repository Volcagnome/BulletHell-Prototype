using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ammo_Player_Cannonball : Ammo_Player_PlayerBullet
{
    bool isActive = true;

//Slow moving projectile but penetrates through enemies and applies large amount of damage.

    override protected void OnTriggerEnter(Collider other)
    {
        //If other is the player, exits function.
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Player Bullet")) 
            return;

        else
        {
            //Checks if triggering object can be damaged, if so applies damage.
            I_Damage dmg = other.GetComponentInParent<I_Damage>();

            if (dmg != null && isActive)
                dmg.TakeDamage(damageAmount);

            //If it hits an obstacle/wall. Falls to the ground but does not apply any more damage
            else if(dmg == null)
            {
                Debug.Log(other.gameObject.name);

                isActive = false;
                rb.useGravity = true;
                MeshRenderer mr = GetComponent<MeshRenderer>();
                mr.material.DisableKeyword("_EMISSION");
            }
        }
    }
}
