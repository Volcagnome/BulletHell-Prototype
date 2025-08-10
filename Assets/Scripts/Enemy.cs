using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class Enemy : MonoBehaviour, I_Damage
{

    [Header("-----Components-----")]
    public Animator anim;
    public int animSpeedTrans;
    public GameObject modelGroup;
    public Material damageMaterial;
    public Material origMaterial;
    public Material deadMaterial;
    public GameObject viewPoint;
    protected Collider hitCollider;

    [Header("-----Attributes-----")]
    public float maxHP;
    float currentHP;
    public float rotationSpeed;
    public NavMeshAgent agent;
    public int pointValue;
    public Vector3 spawnPositionOffset = new Vector3();
    public Vector3 spawnRotationOffset = new Vector3();
    float despawnTime = 3f;
    public float walkSpeed;
    public float runSpeed;
    public float attackCooldown;
    public GameObject damageCollider;


    [Header("-----Enemy Status-----")]
    protected bool hasLOSToPlayer;
    public float LOSCheckInterval = 0.25f;
    protected Vector3 directionToPlayer;
    public LayerMask playerLayer;
    public LayerMask LOSCheck;
    protected bool isDead = false;
    public enum EnemyState { Spawning, Attacking, Pursuing, Idle, Emerging, Retreating, RangedAttack }
    public enum SpawnType { Pit, None, Underground, Special}
    public SpawnType spawnType;
    protected EnemyState currentState = EnemyState.Spawning;
    protected bool finishedSpawning = false;
    protected bool canAttack = true;

    [Header("-----Attack-----")]
   // public Transform shootPos1, shootPos2, shootPos3;
   // public float fireRate = 3f;
   // public GameObject ammoType;
   
    protected bool playerInAttackRange;
    protected bool isAttacking = false;
    protected bool playerDown = false;
   
    protected virtual void Start()
    {
        GameManager.instance.OnPlayerDown += TogglePlayerDown;
        GameManager.instance.OnPlayerRevive += TogglePlayerDown;
        agent = GetComponent<NavMeshAgent>();
        hitCollider = GetComponent<CapsuleCollider>();

        if (GameManager.instance != null )
            directionToPlayer = GetPlayerDirection();

    }
    protected virtual void OnEnable()
    {
        directionToPlayer = GetPlayerDirection();
        currentHP = maxHP;
        StartCoroutine(Spawn());
    }

    protected virtual void Update()
    {
        if (!finishedSpawning || isDead)
            return; 

        CheckState();
        StateMachine();
    }

    public virtual void TakeDamage(float damageAmount)
    {
        if(!isDead)
        {
            currentHP -= damageAmount;
            StartCoroutine(FlashDamage());

            if (currentHP <= 0)
                Death(); 
        }
    }

    protected void SetMoveAnimationSpeed()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);

        if (distanceToPlayer > 8)
            agent.speed = runSpeed;
        else
            agent.speed = walkSpeed;

        float currentSpeed = agent.velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / runSpeed);

        if (normalizedSpeed < 0.1)
            normalizedSpeed = 0f;
        else if (normalizedSpeed > 0.9)
            normalizedSpeed = 1f;

        anim.SetFloat("Speed", Mathf.MoveTowards(anim.GetFloat("Speed"), normalizedSpeed, 0.1f));
    }

    protected void CheckLOS()
    {
        if(viewPoint == null)
        {
            Debug.Log("No viewPoint object.");
            return;
        }

        directionToPlayer = GameManager.instance.player.transform.position - viewPoint.transform.position;

        RaycastHit hitInfo;
        if(Physics.Raycast(viewPoint.transform.position, directionToPlayer.normalized, out hitInfo, 50f, LOSCheck))
        {
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                hasLOSToPlayer = true;
            else
            {
                hasLOSToPlayer = false;
            }
        }
        else
            hasLOSToPlayer= false;  
    }

    protected virtual void CheckState()
    {
        CheckLOS();

        if (playerDown)
            currentState = EnemyState.Idle;
        else
        {
            if (hasLOSToPlayer && playerInAttackRange)
                currentState = EnemyState.Attacking;
            else
                currentState = EnemyState.Pursuing;
        }
    }

    protected void TogglePlayerDown()
    {
        playerDown = !playerDown;
    }

    protected virtual void StateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Attacking:
                agent.isStopped = true;
                RotateToPlayer();
                if (!isAttacking && canAttack && playerInAttackRange)
                    Attack();
                break;

            case EnemyState.Pursuing:
                agent.isStopped = false;
                agent.speed = walkSpeed;
                agent.SetDestination(GameManager.instance.player.transform.position);
                break;

            case EnemyState.Idle:
                agent.isStopped = false;
                if (!agent.hasPath)
                    agent.SetDestination(RollRandomPosition());
                break;
        }
    }

    protected Vector3 RollRandomPosition()
    {
        float randomX = UnityEngine.Random.Range(0, 20);
        float randomZ = UnityEngine.Random.Range(0, 20);
        float staticY = 0;

        Vector3 randomPosition = new Vector3(randomX, staticY, randomZ);

        NavMeshHit navMeshHit;
        bool foundPosition = false;

        do
        {
            if (NavMesh.SamplePosition(randomPosition, out navMeshHit, 5f, NavMesh.AllAreas))
                foundPosition = true;
        }
        while (!foundPosition);

        return randomPosition;
    }

    protected virtual void RotateToPlayer()
    {
        if (!ShouldRotate())
            return;

        Vector3 flatDirection = directionToPlayer;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);

            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

            if (angleDifference > 2f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    bool ShouldRotate()
    {
        float minRotateDistance = 1.5f;
        float sqrMinRotateDistance = minRotateDistance * minRotateDistance;

        Vector3 toPlayer = GameManager.instance.player.transform.position - transform.position;
        toPlayer.y = 0f;

        return toPlayer.sqrMagnitude > sqrMinRotateDistance;
        
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            playerInAttackRange = true;
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInAttackRange = false;
    }

    public Enemy.SpawnType GetSpawnType() { return spawnType; }

    protected virtual void Attack()
    {
        canAttack = false;
        isAttacking = true;
        //anim.SetBool("Attacking", true);
        anim.SetTrigger("Attack");
    }

    protected virtual void ToggleIsAttacking()
    {
        isAttacking = false;
        // anim.SetBool("Attacking", false);
        StopCoroutine(ResetAttackCooldown());
        StartCoroutine(ResetAttackCooldown());
    }

    void AttackBegin()
    {
        damageCollider.SetActive(true);
    }

    void AttackEnd()
    {
        damageCollider.SetActive(false);
        ToggleIsAttacking();
    }

    protected IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }


    protected virtual void ChangeMaterial(Material newMat)
    {
        foreach (Transform child in modelGroup.transform)
        {
            SkinnedMeshRenderer childRenderer = child.GetComponent<SkinnedMeshRenderer>();

            if (childRenderer != null)
                childRenderer.material = newMat;
        }
    }

    protected virtual IEnumerator FlashDamage()
    {
        ChangeMaterial(damageMaterial);
        yield return new WaitForSeconds(0.1f);
        if (!isDead)
            ChangeMaterial(origMaterial);
        else
            ChangeMaterial(deadMaterial);
    }

    protected virtual IEnumerator Spawn()
    {
        finishedSpawning = true;
        yield break;
    }

    protected virtual void Death()
    {
        isDead = true;
        anim.SetBool("Dead", true);
        if(agent != null)
            agent.enabled = false;
        if(hitCollider != null)
            hitCollider.enabled = false;    
        //GetComponent<CapsuleCollider>().enabled = false;
        GameManager.instance.IncrementKillCounts(transform.position, pointValue);
        //model.material = deadMaterial;
        ChangeMaterial(deadMaterial);
        StartCoroutine(DespawnTimer());
    }

    protected virtual void ResetEnemy()
    {
        modelGroup.SetActive(false);
        isDead = false;
        playerInAttackRange = false;
        isAttacking = false;
        finishedSpawning = false;
        canAttack = true;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        anim.SetBool("Dead", false);
        ChangeMaterial(origMaterial);
        currentState = EnemyState.Spawning;

        PoolManager.instance.ReturnInstance(gameObject);
    }

    protected virtual IEnumerator DespawnTimer()
    {
        yield return new WaitForSeconds(despawnTime);
        ResetEnemy();
    }

    protected virtual Vector3 GetPlayerDirection()
    {
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 direction = playerPos - transform.position;
        direction.y = 0f;
        return direction;
    }

    public Vector3 GetSpawnPositionOffset() { return spawnPositionOffset; }
    public Vector3 GetSpawnRotationOffset() { return spawnRotationOffset; }
}

