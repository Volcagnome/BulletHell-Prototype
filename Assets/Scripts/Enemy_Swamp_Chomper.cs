using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Enemy_Swamp_Chomper : Enemy, I_Damage
{

    protected override void OnEnable()
    {
        base.OnEnable();

        if (finishedSpawning)
            InvokeRepeating("CheckLOS", 0f, LOSCheckInterval);
    }

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

        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + transform.forward * 5f;

        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        float step = walkSpeed * Time.deltaTime;

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 12 * Time.deltaTime);

            yield return null;
        }

        finishedSpawning = true;
        hitCollider.enabled = true;
        agent.enabled = true;
        anim.SetBool("Spawning", false);
    }

    protected override void Death()
    {
        base.Death();
        PoolManager.instance.DecrementCurrEnemyType(PoolManager.ObjType.Enemy_SwampChomper);

    }
}
