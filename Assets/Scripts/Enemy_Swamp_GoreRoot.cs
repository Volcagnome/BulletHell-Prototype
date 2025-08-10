using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy_Swamp_GoreRoot : Enemy
{
    Enemy_Swamp_GoreBlossom parentGoreBlossom = null;

    [Header("Gore Root - Status")]
    bool onLand = false;
    bool inPursuit = false;
    bool isBigRoot = false;
    bool isWaitingToRetract = false;
    Vector3 playerPos = Vector3.zero;

    [Header("Gore Root - Components")]
    public float idleTimeBeforeRetract = 5f;
    public float burstDamageAmount = 15f;
    public Vector3 standardScale = new Vector3(1.5f, 1.5f, 1.5f);
    public Vector3 bigRootScale = new Vector3(2.5f, 2.5f, 2.5f);
    public Material bigRootOrig_Mat;
    public Material bigRootDamage_Mat;
    public Material bigRootDead_Mat;
    public Material smallRootOrig_Mat;
    public Material smallRootDamage_Mat;
    public Material smallRootDead_Mat;
    public GameObject VFXSocket;
    public GameObject BurrowVFXPrefab;
    public GameObject BurstVFXPrefab;

    protected override void OnEnable()
    {
        base.OnEnable();

        RollBigRoot();
    }

    protected override IEnumerator Spawn()
    {
        yield return new WaitForSeconds(0.5f);
        //agent.enabled = true;

        playerPos = GameManager.instance.player.transform.position;
        playerPos.y = 0f;
        directionToPlayer = playerPos - transform.position;

        transform.rotation = Quaternion.LookRotation(directionToPlayer);

        Vector3 targetPos = transform.position + Vector3.Normalize(directionToPlayer) * 5f;

        while (!onLand)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
                    onLand = true;

                else
                {
                    playerPos = GameManager.instance.player.transform.position;
                    directionToPlayer = playerPos - transform.position;
                    transform.rotation = Quaternion.LookRotation(directionToPlayer);
                    targetPos = transform.position + Vector3.Normalize(directionToPlayer) * 5f;
                }


            }
            yield return null;

        }

        finishedSpawning = true;
        agent.enabled = true;
        currentState = EnemyState.Pursuing;
    }

    protected override void CheckState()
    {
        if (playerDown)
        {
            currentState = EnemyState.Idle;
            return;
        }

        CheckLOS();

        switch (currentState)
        {
            case EnemyState.Idle:
                if (playerInAttackRange)
                    currentState = EnemyState.Attacking;
                break;

            case EnemyState.Attacking:

                if (!playerInAttackRange)
                    currentState = EnemyState.Idle;
                break;

        }
    }
    protected override void StateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Spawning:
                break;

            case EnemyState.Pursuing:
                if (!inPursuit)
                {
                    inPursuit = true;
                    agent.isStopped = false;
                    agent.speed = walkSpeed;
                    if (onLand)
                        StartCoroutine(BurrowEffect());
                    agent.SetDestination(GameManager.instance.player.transform.position);
                }
                else if (!agent.pathPending && agent.remainingDistance <= 0.5f)
                {
                    inPursuit = false;
                    currentState = EnemyState.Emerging;
                    StopCoroutine(BurrowEffect());
                    anim.SetTrigger("Emerge");
                }
                    break;

            case EnemyState.Emerging:
                agent.isStopped = true;
                break;

            case EnemyState.Attacking:
                agent.isStopped = true;
                RotateToPlayer();
                if (!isAttacking && canAttack && playerInAttackRange)
                    Attack();
                if (isWaitingToRetract)
                    StopCoroutine(RetractTimer());
                break;

            case EnemyState.Idle:
                if (!isWaitingToRetract)
                    StartCoroutine(RetractTimer());
                break;

        }
    }

    IEnumerator BurrowEffect()
    {
        while(inPursuit)
        {
            Instantiate(BurrowVFXPrefab, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RetractTimer()
    {
        isWaitingToRetract = true;

        yield return new WaitForSeconds(idleTimeBeforeRetract);

        currentState = EnemyState.Retreating;
        RetractAndPursue();
        isWaitingToRetract = false;
    }

    void RetractAndPursue()
    {
        anim.SetTrigger("Retract");
       // hasEmerged = false;
    }

    void ToggleModelVisibility(int visible)
    {
        if (visible == 1)
            modelGroup.SetActive(true);
        else
            modelGroup.SetActive(false);
    }

    void FinishEmerge()
    {
       // hasEmerged = true;
        currentState = EnemyState.Idle;
        hitCollider.enabled = true;
    }

    void FinishRetract()
    {
        modelGroup.SetActive(false);
        hitCollider.enabled = false;
        currentState = EnemyState.Pursuing;
    }

    void BurstDamage()
    {
        Instantiate(BurstVFXPrefab,transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, 2f, playerLayer);
        foreach(Collider hit in hits)
        {
            I_Damage player = hit.GetComponent<I_Damage>();

            if (hit.CompareTag("Player") && player != null)
                player.TakeDamage(burstDamageAmount);
        }
    }

    protected override void Attack()
    {
        isAttacking = true;
        canAttack = false;

        if (isBigRoot)
            anim.SetTrigger("Smack");
        else
            anim.SetTrigger("Spin");
    }

    protected override void ResetEnemy()
    {
        base.ResetEnemy();
        parentGoreBlossom = null;
        onLand = false;
        inPursuit = false;  
        isWaitingToRetract = false;
        Vector3 playerPos = Vector3.zero;

        isBigRoot = false;
        transform.localScale = standardScale;
        origMaterial = smallRootOrig_Mat;
        damageMaterial = smallRootDamage_Mat;
        deadMaterial = smallRootDead_Mat;
    }

    void RollBigRoot()
    {
        int bigRoot = UnityEngine.Random.Range(0, 2);

        if (bigRoot == 0)
        {
            isBigRoot = true;
            transform.localScale = bigRootScale;
            origMaterial = bigRootOrig_Mat;
            damageMaterial = bigRootDamage_Mat;
            deadMaterial = bigRootDead_Mat;

            ChangeMaterial(origMaterial);
        }
    }

    public void SetParentGoreBlossom(Enemy_Swamp_GoreBlossom parent) { parentGoreBlossom = parent;}

}


/*Goreblossom has 2 large and 2 small roots attached to model (just for show)
    - When root is deployed, display root will retract in to the ground
    - burrowing VFX (and leaf trail?) will show movement as it pathfinds to player
    - Relatively slow moving but will follow player's movements until they are within 5f 
    - then travel to last known location at that moment so player still has a chance to dodge their initial damage
    - When they reach their destination, they will burst out of the ground dealing damage before starting its normal attack logic
    - Will perform attack when player is in attack range, otherwise plays idle animation
    - Small roots perform spin attack, large vines perform smackus
    - If player leaves attack range will wait for specified cooldown before retracting and pursuing
    - Will continue to pursue the player until killed, at which point they will return to their parent Goreblossom
*/