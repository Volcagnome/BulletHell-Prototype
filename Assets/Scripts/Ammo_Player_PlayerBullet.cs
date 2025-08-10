using UnityEngine;

public class Ammo_Player_PlayerBullet : Damage
{

    [Header("-----Components-----")]
    public Rigidbody rb;
  

    [Header("-----Ammo Specs-----")]
    public bulletClass ammoClass;
    public bulletType ammoType;
    public float projectileSpeed;
    public float destroyTime;
    public enum bulletClass { Primary, Heavy };
    public enum bulletType { Standard, Cannonball }

    private void Start()
    {
        Invoke("ReturnToPool", destroyTime);
        rb.velocity = (transform.forward * projectileSpeed);
        //Destroy(gameObject, destroyTime);
    }

    private void OnEnable()
    {
        CancelInvoke();
        Invoke("ReturnToPool", destroyTime);
        rb.velocity = (transform.forward * projectileSpeed);
    }

    virtual protected void OnTriggerEnter(Collider other)
    {
        //Checks if triggering object can be damaged, if so applies damage.
        I_Damage dmg = other.GetComponentInParent<I_Damage>();

        if (dmg != null)
        {
            dmg.TakeDamage(damageAmount);

            ReturnToPool();
        }
    }

    virtual protected void ReturnToPool()
    {
        CancelInvoke();
        rb.velocity = Vector3.zero;
        PoolManager.instance.ReturnInstance(gameObject);
    }
}
