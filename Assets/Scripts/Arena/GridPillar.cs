using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class GridPillar : MonoBehaviour
{
    public enum PillarPosition { upPos, downPos, defaultPos }
    public enum PillarState { Waiting, Moving, Idle };

    [Header("-----Attributes------")]
    public float defaultY = -6f;
    public float downY = -15f;
    public float upY = -3f;
    public float downMoveSpeed = 8.5f;
    public float upMoveSpeed = 5f;

    [Header("-----Components------")]
    Vector2 rowCol;
    GridPillar nextSquare = null;
    public LayerMask detectionLayers;
    public GameObject pillarObject;
    public GameObject spawnPoint;
    public BoxCollider boxCollider;
    public NavMeshObstacle obstacle;

    [Header("-----Status------")]
    PillarState state = PillarState.Idle;
    bool skip = false;
    bool inPosition = true;
    Coroutine statusCoroutine = null;



    //Performs box cast to see if any player/enemy/batter is in their space.
    public bool CheckIfOccupied()
    {
        Vector3 castSize = new Vector3(3, 15, 3);
        Vector3 castCenter = transform.position;

        //DrawBox(castCenter, castSize, transform.rotation);

        Collider[] hitColliders = Physics.OverlapBox(castCenter, castSize / 2, Quaternion.identity, detectionLayers);

        return hitColliders.Length > 0;
    }

    //If pillar is currently waiting to move, cancels coroutine.
    //Starts new Wait or Move coroutine with current moveTo argument.
    public void CheckIfReadyToMove(PillarPosition moveTo)
    {
        if (state == PillarState.Waiting)
        {
            StopCoroutine(statusCoroutine);
            statusCoroutine = null;
        }

        if(moveTo == PillarPosition.defaultPos || !CheckIfOccupied())
        {
            state = PillarState.Moving;
            statusCoroutine = StartCoroutine(MoveBlock(moveTo));
        }
        else 
        {
            state = PillarState.Waiting;
            statusCoroutine = StartCoroutine(WaitForOccupant(moveTo));
        }
    }


    //Specialized CheckIfReadyToMove function for squares that would leave occupant stranded if surrounding squares moved.
    //If occupied, creates a bridge by preventing appropriate squares from moving until occupant leaves. Otherwise proceeds with move.
    public void CheckDangerSquare(PillarPosition moveTo)
    {
        if (state == PillarState.Waiting)
            StopCoroutine(statusCoroutine);

        if (CheckIfOccupied())
        {
            GetBridge();
            statusCoroutine = StartCoroutine(WaitForBridgeOccupant(moveTo));
        }
        else
        {
            state = PillarState.Moving;
            statusCoroutine = StartCoroutine(MoveBlock(moveTo));
        }

    }

    //Waits for occupant (player/enemy/battery) to move out of their space before calling MoveBlock. 
    IEnumerator WaitForOccupant(PillarPosition moveTo)
    {
        while (CheckIfOccupied())
        {
            yield return new WaitForSeconds(0.25f);
        }

        state = PillarState.Moving;
        statusCoroutine = StartCoroutine(MoveBlock(moveTo));
    }

    //Specialized version of WaitForOccupant funciton that will call same Corutine on the square's nextSquare object (if any) once the square has been vacated. 
    IEnumerator WaitForBridgeOccupant(PillarPosition moveTo)
    {
        state = PillarState.Waiting;

        while (CheckIfOccupied())
        {
            yield return new WaitForSeconds(0.25f);
        }

        state = PillarState.Moving;
        statusCoroutine = StartCoroutine(MoveBlock(moveTo));

        if (nextSquare != null)
        {
            nextSquare.SetSkip(false);
            nextSquare.SetStatusCoroutine(StartCoroutine(nextSquare.WaitForBridgeOccupant(moveTo)));
            nextSquare = null;
        }

    }

    //Gets the square immediately above/below them (depending on if square is in upper or lower half of arena)
    //If that square is scheduled to move down in the current configuration, changes its "skip" bool to true to prevent it. 
    //Then calls GetBridge on that square. Repeats until it hits a square that is part of the mainland (not scheduled to move down).
    void GetBridge()
    {
        int nextSquareRow = 0;
        if (rowCol.x >= 8)
            nextSquareRow = (int)rowCol.x - 1;
        else
            nextSquareRow = (int)rowCol.x + 1;

        if (ArenaManager.instance.CheckDownList(new Vector2(nextSquareRow, rowCol.y)))
        {
            nextSquare = ArenaManager.instance.GetPillar(nextSquareRow, (int)rowCol.y);
            nextSquare.SetSkip(true);
            nextSquare.GetBridge();
        }
    }


    //Moves grid square into specified position.
    public IEnumerator MoveBlock(PillarPosition moveTo)
    {
        float moveSpeed = 0f;
        float targetY = defaultY;
        inPosition = false;

        if (skip)
            yield break;


        //Sets target position/speed as appropriate for moveTo argument
        switch (moveTo)
        {
            case PillarPosition.upPos:
                targetY = upY;
                moveSpeed = upMoveSpeed;
                boxCollider.enabled = true;
                obstacle.enabled = true;
                break;

            case PillarPosition.downPos:
                targetY = downY;
                moveSpeed = downMoveSpeed;
                boxCollider.enabled = true;
                obstacle.enabled = true;
                break;
            case PillarPosition.defaultPos:
                {
                    if (pillarObject.transform.position.y > defaultY)
                        moveSpeed = upMoveSpeed;
                    else if (pillarObject.transform.position.y < defaultY)
                        moveSpeed = downMoveSpeed;
                }
                break;
        }


        //If pillar is in up position and is moving back to default, performs small "wind up" movement prior to moving down. 
        if (moveTo == PillarPosition.defaultPos && pillarObject.transform.position.y > defaultY)
        {
            while (pillarObject.transform.position.y < upY + 0.35f)
            {
                pillarObject.transform.position = Vector3.Lerp(pillarObject.transform.position, new Vector3(pillarObject.transform.position.x, upY + 0.35f, pillarObject.transform.position.z), Time.deltaTime * 5f);
                yield return null;

                if (Mathf.Abs(pillarObject.transform.position.y - (upY + 0.35f)) <= 0.01f)
                    break;
            }
        }

        //Moves pillar into specified position
        while (pillarObject.transform.position.y != targetY)
        {
            pillarObject.transform.position = Vector3.MoveTowards(pillarObject.transform.position, new Vector3(pillarObject.transform.position.x, targetY, pillarObject.transform.position.z), Time.deltaTime * moveSpeed);
            yield return null;

            //Exits loop once pillar is within 0.1f of target y value
            if (Mathf.Abs(pillarObject.transform.position.y - targetY) <= 0.1f)
                break;
        }

        //Makes sure pillar ends up exactly in correct position if MoveTowards result is slightly above/below correct value
        pillarObject.transform.position = new Vector3(pillarObject.transform.position.x, targetY, pillarObject.transform.position.z);


        //If pillar is in default position, disables collider and NavMeshObstacle components. 
        if (pillarObject.transform.position.y == defaultY)
        {
            boxCollider.enabled = false;
            obstacle.enabled = false;
        }

        inPosition = true;
        statusCoroutine = null;
        state = PillarState.Idle;
    }

    // Getters/Setters
    public void SetCoords(int x, int y) { rowCol = new Vector2(x, y);   }

    public Vector2 GetRowCol() { return rowCol; }

    public PillarState GetPillarState() { return state; }

    public Vector3 GetSpawnPoint() { return spawnPoint.transform.position; }

    public void SetStatusCoroutine(Coroutine newCoroutine) { statusCoroutine = newCoroutine; }
    
    public void SetSkip(bool status) { skip = status; }



    //void DrawBox(Vector3 center, Vector3 size, Quaternion rotation)
    //{
    //    // Get the half extents of the box
    //    Vector3 halfExtents = size / 2;

    //    // Define the 8 corners of the box
    //    Vector3[] corners = new Vector3[8];
    //    corners[0] = center + rotation * new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
    //    corners[1] = center + rotation * new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
    //    corners[2] = center + rotation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
    //    corners[3] = center + rotation * new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);
    //    corners[4] = center + rotation * new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
    //    corners[5] = center + rotation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
    //    corners[6] = center + rotation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);
    //    corners[7] = center + rotation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);

    //    // Draw lines between each corner to create a wireframe box
    //    Debug.DrawLine(corners[0], corners[1], Color.red);
    //    Debug.DrawLine(corners[1], corners[2], Color.red);
    //    Debug.DrawLine(corners[2], corners[3], Color.red);
    //    Debug.DrawLine(corners[3], corners[0], Color.red);

    //    Debug.DrawLine(corners[4], corners[5], Color.red);
    //    Debug.DrawLine(corners[5], corners[6], Color.red);
    //    Debug.DrawLine(corners[6], corners[7], Color.red);
    //    Debug.DrawLine(corners[7], corners[4], Color.red);

    //    Debug.DrawLine(corners[0], corners[4], Color.red);
    //    Debug.DrawLine(corners[1], corners[5], Color.red);
    //    Debug.DrawLine(corners[2], corners[6], Color.red);
    //    Debug.DrawLine(corners[3], corners[7], Color.red);
    //}
}
