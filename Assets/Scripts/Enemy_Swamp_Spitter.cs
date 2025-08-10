using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Swamp_Spitter : Enemy_Swamp_Chomper, I_Damage
{
    public GameObject projectile;
    public Transform shootPos;
    float projectileSpeed;
    public float projectileGravity;

    protected override void Start()
    {
        base.Start();
        Ammo_Swamp_SpitterProjectile bullet = projectile.GetComponent<Ammo_Swamp_SpitterProjectile>();
        projectileSpeed = bullet.GetSpeed();

    }

    void Shoot()
    {
        GameObject bullet = PoolManager.instance.GetInstance(PoolManager.ObjType.Bullet_SwampSpitter);
        bullet.transform.position = shootPos.position;
        bullet.transform.rotation = shootPos.rotation;

        Ammo_Swamp_SpitterProjectile spitterBullet = bullet.GetComponent<Ammo_Swamp_SpitterProjectile>();
        spitterBullet.SetTrajectory(CalculateTrajectory());

        bullet.SetActive(true);
    }

    Vector3 CalculateTrajectory()
    {
        Vector3 playerDir = (GameManager.instance.player.transform.position - transform.position);
        playerDir.y = 0;
        float playerDist = playerDir.magnitude;
        playerDir.Normalize();

        float airTime = playerDist / projectileSpeed;
        float vertVel = 0.5f * projectileGravity * airTime;

        return (playerDir * projectileSpeed) + (Vector3.up * vertVel);

    }

    protected override void Death()
    {
        base.Death();
        PoolManager.instance.DecrementCurrEnemyType(PoolManager.ObjType.Enemy_SwampSpitter);

    }

    //protected override IEnumerator Spawn()
    //{
    //    anim.SetBool("Spawning", true);
    //    model.enabled = true;

    //    Vector3 initialPosition = transform.position;
    //    Vector3 targetPosition = initialPosition + transform.forward * 5f;

    //    Quaternion initialRotation = transform.rotation;
    //    Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

    //    float step = walkSpeed * Time.deltaTime;

    //    while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
    //    {
    //        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    //        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 12 * Time.deltaTime);

    //        yield return null;
    //    }

    //    finishedSpawning = true;
    //    GetComponent<CapsuleCollider>().enabled = true;
    //    agent.enabled = true;
    //    anim.SetBool("Spawning", false);
    //}
}
