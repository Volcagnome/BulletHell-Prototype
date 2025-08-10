using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Swamp_Mushroom : Enemy, I_Damage
{
    [Header("-----Components-----")]
    public SkinnedMeshRenderer bodyModel;
    public SkinnedMeshRenderer headModel;
    public SkinnedMeshRenderer tentacleModel;
    public ParticleSystem fungusSpore;
    public GameObject mushroomSporePrefab;

    [Header("-----Attributes-----")]
    public int numberSpores;
    public int sporeCircleRadius;

    public event Action OnMushroomDeath;

    protected override void Update()
    {
        base.Update();

        if (agent != null && agent.enabled == true)
            SetMoveAnimationSpeed();
    }

    protected override IEnumerator Spawn()
    {
        anim.SetBool("Spawning", true);
        modelGroup.SetActive(true);

        //Calculates target position to move towards to finish spawning (will rise up out of the ground)
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z);

        while (transform.position.y < targetPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
            yield return null;
        }

        finishedSpawning = true;
        GetComponent<CapsuleCollider>().enabled = true;
        agent.enabled = true;
        anim.SetBool("Spawning", false);
    }

    protected override void CheckState()
    {
        CheckLOS();

        if (hasLOSToPlayer && playerInAttackRange)
            currentState = EnemyState.Attacking;
        else
            currentState = EnemyState.Pursuing;
    }

    protected override void StateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Attacking:
                agent.isStopped = true;
                RotateToPlayer();
                if (!isAttacking && canAttack && playerInAttackRange)
                    Invoke("DelayedAttack", 0.5f);
                break;

            case EnemyState.Pursuing:
                agent.isStopped = false;
                agent.SetDestination(GameManager.instance.player.transform.position);
                break;
        }
    }

    void DelayedAttack()
    {
        Attack();
    }

    public void Shoot()
    {
        //Plays attack animation
        fungusSpore.Play();

        //Calculates the amount to rotate when spawning each spore so they are evenly spaced in a ring
        float angleStep = 360f / numberSpores;

        //Calculates the center to spawn the spore ring around
        Vector3 center = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        for (int i = 0; i < numberSpores; i++)
        {
            float angle = i * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            float posX = center.x + sporeCircleRadius * Mathf.Cos(rad);
            float posZ = center.z + sporeCircleRadius * Mathf.Sin(rad);
            Vector3 spawnPos = new Vector3(posX, center.y, posZ);

            // Calculate the direction from the center to this spawn position
            Vector3 direction = (spawnPos - center).normalized;

            // Create a rotation that faces away from the center
            //Quaternion spawnRot = Quaternion.LookRotation(direction);

            GameObject mushBullet = PoolManager.instance.GetInstance(PoolManager.ObjType.Bullet_SwampMushroom);

            mushBullet.transform.position = spawnPos;
           // mushBullet.transform.rotation = spawnRot;

            Ammo_Swamp_MushroomSpore spore = mushBullet.GetComponent<Ammo_Swamp_MushroomSpore>();
            spore.SubscribeToParentMushroom(this);
            spore.SetDirection(direction);

            mushBullet.SetActive(true);
   
        }

        if(isDead)
            OnMushroomDeath?.Invoke();
    }

    protected override void Death()
    {
        OnMushroomDeath?.Invoke();
        base.Death();

        PoolManager.instance.DecrementCurrEnemyType(PoolManager.ObjType.Enemy_SwampMushroom);
    }

    protected override void ResetEnemy()
    {
        modelGroup.SetActive(false);
        base.ResetEnemy();
        ChangeMaterial(origMaterial);
    }
}
