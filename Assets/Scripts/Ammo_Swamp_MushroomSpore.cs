using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class Ammo_Swamp_MushroomSpore : Ammo_EnemyBullet
{

    Enemy_Swamp_Mushroom parentMushroom;
    Vector3 direction;

    //Subscribes to parent mushroom's death event and saves reference to parent
    public void SubscribeToParentMushroom(Enemy_Swamp_Mushroom parent)
    {
        parent.OnMushroomDeath += ReturnToPool;
        parentMushroom = parent;
    }

    protected override void StartTrajectory()
    {
        rb.velocity = (direction * projectileSpeed);
    }

    //Unsubscribes from current parent's death event and sets reference to null before returning to pool
    protected override void ReturnToPool()
    {
        if (parentMushroom != null)
        {
            parentMushroom.OnMushroomDeath -= ReturnToPool;
            parentMushroom = null;
        }
        base.ReturnToPool();    
    }
    public void SetDirection(Vector3 newDirection) { direction = newDirection; }

}
