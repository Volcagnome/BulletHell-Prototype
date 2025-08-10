using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo_EnemyBullet : Damage
{
    
    public enum bulletType { Mushroom, Spitter}

    [Header("-----Attributes-----")]
    public bulletType ammoType;
    public float projectileSpeed;
    public float destroyTime;
    protected Rigidbody rb;


    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        Invoke("ReturnToPool", destroyTime);
        StartTrajectory();
    }

    protected virtual void OnEnable()
    {
        //Ensures rb still holds a valid reference prior to calling start trajectory
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        StartTrajectory();

        //Cancels any previous destroy timer before starting it again
        CancelInvoke();
        Invoke("ReturnToPool", destroyTime);
    }

    protected virtual void StartTrajectory()
    {
        rb.velocity = (transform.forward * projectileSpeed);
    }

    protected virtual void ReturnToPool()
    {
        //Cancels destroy timer prior to returning
        CancelInvoke();
        PoolManager.instance.ReturnInstance(gameObject);
    }

    virtual protected void OnTriggerEnter(Collider other)
    {
            //Checks if triggering object can be damaged, if so applies damage.
            I_Damage dmg = other.GetComponentInParent<I_Damage>();

            if (dmg != null)
                dmg.TakeDamage(damageAmount);

        ReturnToPool();

    }
}
