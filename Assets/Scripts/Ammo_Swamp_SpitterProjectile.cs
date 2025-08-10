using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Ammo_Swamp_SpitterProjectile : Ammo_EnemyBullet
{
    public Vector3 shootAngle;
    public float fallMultiplier = 2.0f;
    public LayerMask damageLayers;
    public GameObject poisonExplosion;
    public GameObject guideLight;

    GameObject poisonDecal;
    Vector3 trajectory;

    // Update is called once per frame
    void Update()
    {
        AdjustLightHeight();
    }

    void AdjustLightHeight()
    {
        float newY = 0f;

        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 20f ))
            newY = hit.point.y + 0.3f;

        Vector3 lightWorldPosition = guideLight.transform.position;
        guideLight.transform.position = new Vector3(lightWorldPosition.x, newY, lightWorldPosition.z);
    }

    protected override void StartTrajectory()
    {
        rb.velocity = trajectory;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4f, damageLayers);

        DrawDebugCircle(transform.position, 4f, 25, Color.green, 5f);

        if(hitColliders.Length > 0)
        {
            I_Damage dmg = hitColliders[0].gameObject.GetComponentInParent<I_Damage>();

            if (dmg != null)
                dmg.TakeDamage(damageAmount);

        }

        Instantiate(poisonExplosion, new Vector3(transform.position.x, -1.5f, transform.position.z), Quaternion.Euler(Vector3.zero));

        ReturnToPool();
    }

    void DrawDebugCircle(Vector3 center, float radius, int segments, Color color, float duration = 0f)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0); // Start point on the X-axis

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            Debug.DrawLine(prevPoint, newPoint, color, duration);
            prevPoint = newPoint;
        }
    }

    public float GetSpeed() { return projectileSpeed; }
    public void SetTrajectory(Vector3 newTraj) { trajectory = newTraj; }
}
