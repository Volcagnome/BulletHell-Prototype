using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [Range(0.1f, 1f)]
    public float followDamping;
    public Transform playerTransform;

    //Follow's player movement with slight damping
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, playerTransform.position, 1 /followDamping * Time.fixedDeltaTime);
    }
}

