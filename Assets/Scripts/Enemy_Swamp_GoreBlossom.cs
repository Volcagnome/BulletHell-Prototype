using GameUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy_Swamp_GoreBlossom : Enemy
{
    [Header("-----Attributes-----")]
    public float spawnSpeed;
    public float rootAttackCoolTime;
    public int maxRoots;
    int currRoots = 0;
    Coroutine rootCoolRoutine = null;
    bool canSpawnRoot = true;

    public event Action<GameObject> OnGoreBlossomDeath;

    protected override IEnumerator Spawn()
    {
        anim.SetBool("Spawning", true);

        //Pauses to the end of the frame to let spawn position finish being set.
        yield return null;

        transform.rotation = Quaternion.LookRotation(directionToPlayer);

        //Calculates target position to move towards to finish spawning (will rise up out of the water)
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y + 5f, transform.position.z);

        while (transform.position.y < targetPosition.y)
        {
            if(transform.position.y < targetPosition.y - 0.2f)
                anim.SetBool("Spawning", false);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, spawnSpeed * Time.deltaTime);
            yield return null;
        }

        anim.SetTrigger("Roar");
        finishedSpawning = true;
        hitCollider.enabled = true;
    }

    protected override void Death()
    {
        base.Death();

        OnGoreBlossomDeath?.Invoke(gameObject);
        PoolManager.instance.DecrementCurrEnemyType(PoolManager.ObjType.Enemy_SwampGoreBlossom);
    }

    protected override void ResetEnemy()
    {
        base.ResetEnemy();

        canSpawnRoot = true;
        if(rootCoolRoutine != null)
            StopCoroutine(rootCoolRoutine);
        currRoots = 0;
    }

    protected override void CheckState()
    {
        CheckLOS();

        if (playerDown)
            currentState = EnemyState.Idle;
        else
        {
            if (hasLOSToPlayer && playerInAttackRange)
                currentState = EnemyState.Attacking;
            else
                currentState = EnemyState.RangedAttack;
        }
    }

    protected override void StateMachine()
    {
        RotateToPlayer();

        switch (currentState)
        {
            case EnemyState.Attacking:
                if (!isAttacking && canAttack && playerInAttackRange)
                    Attack();
                break;
            case EnemyState.RangedAttack:
                if (canSpawnRoot && currRoots < maxRoots)
                    SpawnRoot();
                break;
                
        }
    }

    IEnumerator RootCooldown()
    {
        canSpawnRoot = false;

        yield return new WaitForSeconds(rootAttackCoolTime);        

        canSpawnRoot = true;
    }

    void SpawnRoot()
    {
        rootCoolRoutine = StartCoroutine(RootCooldown());

        anim.SetTrigger("SpawnRoot");

        GameObject newRoot = PoolManager.instance.GetInstance(PoolManager.ObjType.Enemy_SwampGoreRoot);
        Enemy_Swamp_GoreRoot rootEnemyScript = newRoot.GetComponent<Enemy_Swamp_GoreRoot>();
        Vector3 offset = Vector3.zero;

        if (rootEnemyScript != null)
        {
            offset = rootEnemyScript.GetSpawnPositionOffset();
            rootEnemyScript.SetParentGoreBlossom(this);
        }

        newRoot.transform.position = new Vector3(transform.position.x + offset.x, 
                                                 transform.position.y + offset.y, 
                                                 transform.position.z + offset.z);

        newRoot.SetActive(true);

        currRoots++;
    }

}
