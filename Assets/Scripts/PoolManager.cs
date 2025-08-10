using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using System;
using GameUtilities;
using System.Collections.ObjectModel;

public class PoolManager : MonoBehaviour
{
    public enum ObjType
    {
        Enemy_SwampChomper,
        Enemy_SwampSpitter,
        Enemy_SwampMushroom,
        Enemy_SwampGoreBlossom,
        Enemy_SwampGoreRoot,
        Bullet_PlayerStandard,
        Bullet_PlayerCannon,
        Bullet_SwampMushroom,
        Bullet_SwampSpitter
    }

    [Header("-----Attributes-----")]
    public float maxNavMeshSampleDistance = 5f;
    int numberOfEnemiesToSpawn = 0;
    Dictionary<GameObject,SpawnData> occupiedSpawns;

    [Header("-----EnemyPrefabs-----")]
    public GameObject prefab_Enemy_SwampChomper;
    public GameObject prefab_Enemy_SwampSpitter;
    public GameObject prefab_Enemy_SwampMushroom;
    public GameObject prefab_Enemy_SwampGoreBlossom;
    public GameObject prefab_Enemy_SwampGoreRoot;

    [Header("-----AmmoPrefabs-----")]
    public GameObject prefab_Bullet_PlayerStandard;
    public GameObject prefab_Bullet_PlayerCannon;
    public GameObject prefab_Bullet_SwampMushroom;
    public GameObject prefab_Bullet_SwampSpitter;

    [Header("-----Temp Variables-----")]
    int totalActiveEnemies = 0;
    public float spawnEveryNumSeconds;
    public bool spawnChompers;
    public bool spawnSpitters;
    public bool spawnMushrooms;
    public bool spawnGoreBlossoms;

    [Header("-----Pools-----")]
    Dictionary<ObjType, List<GameObject>> objPools;
    Dictionary<ObjType, GameObject> objPrefabs;
    Dictionary<ObjType, int> objQty;
    
    List<GameObject> pool_Bullet_PlayerStandard;
    List<GameObject> pool_Bullet_PlayerCannon;
    List<GameObject> pool_Bullet_SwampMushroom;
    List<GameObject> pool_Bullet_SwampSpitter;

    List<GameObject> pool_Enemy_SwampChomper;
    List<GameObject> pool_Enemy_SwampSpitter;
    List<GameObject> pool_Enemy_SwampMushroom;
    List<GameObject> pool_Enemy_SwampGoreBlossom;
    List<GameObject> pool_Enemy_SwampGoreRoot;

    [Header("-----Pool Sizes-----")]
    public int qty_Bullet_PlayerStandard;
    public int qty_Bullet_PlayerCannon;
    public int qty_Bullet_SwampMushroom;
    public int qty_Bullet_SwampSpitter;

    public int qty_Enemy_SwampChomper;
    public int qty_Enemy_SwampSpitter;
    public int qty_Enemy_SwampMushroom;
    public int qty_Enemy_SwampGoreBlossom;
    public int qty_Enemy_SwampGoreRoot;

    public int currQty_Enemy_SwampChomper = 0;
    public int currQty_Enemy_SwampSpitter = 0;
    public int currQty_Enemy_SwampMushroom = 0;
    public int currQty_Enemy_SwampGoreBlossom = 0;
    public int currQty_Enemy_SwampGoreRoot = 0;

    public static PoolManager instance;
    public event Action OnPoolSetupComplete;
    
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        occupiedSpawns = new Dictionary<GameObject,SpawnData>();

        //Initializes dictionaries for pools, prefabs, and pool sizes
        objPools = new Dictionary<ObjType, List<GameObject>>();
        objPrefabs = new Dictionary<ObjType, GameObject>();
        objQty = new Dictionary<ObjType, int>();

        //Initializes object pools
        pool_Bullet_PlayerStandard = new List<GameObject>();
        pool_Bullet_PlayerCannon = new List<GameObject>();
        pool_Bullet_SwampMushroom = new List<GameObject>();
        pool_Bullet_SwampSpitter = new List<GameObject>();

        pool_Enemy_SwampChomper = new List<GameObject>();
        pool_Enemy_SwampSpitter = new List<GameObject>();
        pool_Enemy_SwampMushroom = new List<GameObject>();
        pool_Enemy_SwampGoreBlossom = new List<GameObject>();
        pool_Enemy_SwampGoreRoot = new List<GameObject>();

        FillDictionaries();
        FillPools();

        //Reports ready status to GameManager
        OnPoolSetupComplete?.Invoke();
    }

    void FillDictionaries()
    {
        //Fill Pool Dictionary
        objPools.Add(ObjType.Bullet_PlayerStandard, pool_Bullet_PlayerStandard);
        objPools.Add(ObjType.Bullet_PlayerCannon, pool_Bullet_PlayerCannon);
        objPools.Add(ObjType.Bullet_SwampMushroom, pool_Bullet_SwampMushroom);
        objPools.Add(ObjType.Bullet_SwampSpitter, pool_Bullet_SwampSpitter);

        objPools.Add(ObjType.Enemy_SwampChomper, pool_Enemy_SwampChomper);
        objPools.Add(ObjType.Enemy_SwampSpitter, pool_Enemy_SwampSpitter);
        objPools.Add(ObjType.Enemy_SwampMushroom, pool_Enemy_SwampMushroom);
        objPools.Add(ObjType.Enemy_SwampGoreBlossom, pool_Enemy_SwampGoreBlossom);
        objPools.Add(ObjType.Enemy_SwampGoreRoot, pool_Enemy_SwampGoreRoot);

        //Fill Prefab Dictionary
        objPrefabs.Add(ObjType.Bullet_PlayerStandard, prefab_Bullet_PlayerStandard);
        objPrefabs.Add(ObjType.Bullet_PlayerCannon, prefab_Bullet_PlayerCannon);
        objPrefabs.Add(ObjType.Bullet_SwampMushroom, prefab_Bullet_SwampMushroom);
        objPrefabs.Add(ObjType.Bullet_SwampSpitter, prefab_Bullet_SwampSpitter);

        objPrefabs.Add(ObjType.Enemy_SwampChomper, prefab_Enemy_SwampChomper);
        objPrefabs.Add(ObjType.Enemy_SwampSpitter, prefab_Enemy_SwampSpitter);
        objPrefabs.Add(ObjType.Enemy_SwampMushroom, prefab_Enemy_SwampMushroom);
        objPrefabs.Add(ObjType.Enemy_SwampGoreBlossom, prefab_Enemy_SwampGoreBlossom);
        objPrefabs.Add(ObjType.Enemy_SwampGoreRoot, prefab_Enemy_SwampGoreRoot);

        //Fill Quantity Dictionary
        objQty.Add(ObjType.Bullet_PlayerStandard, qty_Bullet_PlayerStandard);
        objQty.Add(ObjType.Bullet_PlayerCannon, qty_Bullet_PlayerCannon);
        objQty.Add(ObjType.Bullet_SwampMushroom, qty_Bullet_SwampMushroom);
        objQty.Add(ObjType.Bullet_SwampSpitter, qty_Bullet_SwampSpitter);

        objQty.Add(ObjType.Enemy_SwampChomper, qty_Enemy_SwampChomper);
        objQty.Add(ObjType.Enemy_SwampSpitter, qty_Enemy_SwampSpitter);
        objQty.Add(ObjType.Enemy_SwampMushroom, qty_Enemy_SwampMushroom);
        objQty.Add(ObjType.Enemy_SwampGoreBlossom, qty_Enemy_SwampGoreBlossom);
        objQty.Add(ObjType.Enemy_SwampGoreRoot, qty_Enemy_SwampGoreRoot);

    }

    void FillPools()
    {
        foreach (ObjType obj in Enum.GetValues(typeof(ObjType)))
        {
            for(int qty = 0; qty < objQty[obj]; qty++)
            {
                GameObject newObj = Instantiate(objPrefabs[obj]);
                newObj.SetActive(false);
                objPools[obj].Add(newObj);
            }
        }
    }

    //Returns instance of given type from correct pool, if none available expands pool
    public GameObject GetInstance(ObjType type)
    {
        List<GameObject> pool = objPools[type];

        if (pool != null && pool.Count > 0)
        {
            foreach (var obj in pool)
            {
                if (!obj.activeInHierarchy)
                    return obj;
            }
        }

        GameObject newObj = objPrefabs[type];

        if (newObj != null)
        {
            newObj = Instantiate(newObj);        
            pool.Add(newObj);
        }

        return newObj;
    }

    //Returns instance to pool
    public void ReturnInstance(GameObject obj)
    {
       obj.SetActive(false);
    }

    public int GetTotalActiveEnemies() { return totalActiveEnemies; }

    public IEnumerator StartSpawning(int numEnemies, List<Vector3> spawnPoints)
    {
        numberOfEnemiesToSpawn = numEnemies;

        while (numberOfEnemiesToSpawn > 0)
        {
            if (spawnSpitters && currQty_Enemy_SwampSpitter < qty_Enemy_SwampSpitter)
            {
                SpawnEnemy(ArenaManager.instance.GetPitSpawn(), ObjType.Enemy_SwampSpitter);
                numberOfEnemiesToSpawn--;
                totalActiveEnemies++;
                currQty_Enemy_SwampSpitter++;
            }

            if (spawnChompers && currQty_Enemy_SwampChomper < qty_Enemy_SwampChomper)
            {
                SpawnEnemy(ArenaManager.instance.GetPitSpawn(), ObjType.Enemy_SwampChomper);
                SpawnEnemy(ArenaManager.instance.GetPitSpawn(), ObjType.Enemy_SwampChomper);
                numberOfEnemiesToSpawn-=2;
                totalActiveEnemies+=2;
                currQty_Enemy_SwampChomper+=2;
            }

            if (spawnMushrooms && currQty_Enemy_SwampMushroom < qty_Enemy_SwampMushroom)
            {
                SpawnEnemy(ArenaManager.instance.GetUndergroundSpawn(), ObjType.Enemy_SwampMushroom);
                numberOfEnemiesToSpawn--;
                totalActiveEnemies++;
                currQty_Enemy_SwampMushroom++;
            }

            if (spawnGoreBlossoms && currQty_Enemy_SwampGoreBlossom < qty_Enemy_SwampGoreBlossom)
            {
                SpawnGoreBlossom();
                numberOfEnemiesToSpawn--;
                totalActiveEnemies++;
                currQty_Enemy_SwampGoreBlossom++;
            }

            yield return new WaitForSeconds(spawnEveryNumSeconds);
        }
    }

    SpawnData GetStationaryPitSpawn()
    {
        SpawnData spawn = new SpawnData(new Vector3(-1,-1,-1),Quaternion.identity);
        bool foundAvailableSpawn = false;
        int attempts = 0;

        while(!foundAvailableSpawn && attempts < 25)
        {
            spawn = ArenaManager.instance.GetPitSpawn();

            if (!occupiedSpawns.ContainsValue(spawn))
                foundAvailableSpawn = true;
            else
                Debug.Log("Spawn: " + spawn.position);

                attempts++;
        }

        return spawn;
    }

    void SpawnGoreBlossom()
    {
        SpawnData spawn = GetStationaryPitSpawn();

        if (spawn.position.x == -1)
            return;

        GameObject newObj = SpawnEnemy(spawn, ObjType.Enemy_SwampGoreBlossom);
        occupiedSpawns.Add(newObj, spawn);
        Enemy_Swamp_GoreBlossom newGoreBlossom = newObj.GetComponent<Enemy_Swamp_GoreBlossom>();

        if (newGoreBlossom != null)
            newGoreBlossom.OnGoreBlossomDeath += (deadGoreBlossom) => FreeSpawnPoint(deadGoreBlossom);

        else
            Debug.Log("GoreBlossom script not found.");
    }

    void FreeSpawnPoint(GameObject deadGoreBlossom)
    {
        occupiedSpawns.Remove(deadGoreBlossom);
    }

    GameObject SpawnEnemy(SpawnData spawnPoint, ObjType type)
    {
        Vector3 positionOffset;
        Vector3 rotationOffset;

        GameObject newEnemy = GetInstance(type);
        if (newEnemy != null)
        { 
            newEnemy.transform.position = spawnPoint.position;
            newEnemy.transform.rotation = spawnPoint.rotation;

            positionOffset = newEnemy.GetComponent<Enemy>().GetSpawnPositionOffset();
            rotationOffset = newEnemy.GetComponent<Enemy>().GetSpawnRotationOffset();
        
            newEnemy.transform.position += newEnemy.transform.up * positionOffset.y;
            newEnemy.transform.position += newEnemy.transform.forward * positionOffset.z;
            newEnemy.transform.Rotate(rotationOffset.x, 0f, 0f, Space.Self);

            newEnemy.SetActive(true);
        }

        return newEnemy;
    }

    public void DecrementCurrEnemyType(ObjType type)
    {
        switch(type)
        {
            case (ObjType.Enemy_SwampChomper):
                currQty_Enemy_SwampChomper--;
                break;
            case (ObjType.Enemy_SwampSpitter):
                currQty_Enemy_SwampSpitter--;
                break;
            case (ObjType.Enemy_SwampMushroom):
                currQty_Enemy_SwampMushroom--;
                break;
            case (ObjType.Enemy_SwampGoreBlossom):
                currQty_Enemy_SwampGoreBlossom--;
                break;
            default:
                break;
        }
    }
}




//New environment/arena starts new stage

//Each stage divided into several waves. Each wave has its own roster of enemies to spawn.
//Each wave has a starting number of enemies that are all spawned in at once as well as a minimum number of enemies to keep on the map at once.
//As the player kills enemies, if the total number dips below minimum, more enemies from the roster are spawned in unil the roster is empty. Then next wave is started.
//Each new wave increases minimum and total number of enemies for stage. Also increases the kills needed for a battery to spawn.
//
//"Number" of enemies to be replaced with "total threat" of enemies.
//Each enemy type will have a "threat level" based on how easy they are to kill. Weak enemies may have a 0.5 - 2 threat level while tanky enemies may have up to 10 threat level. 
//Their threat level will serve as their "weight" in the roster. When the roster is generated, killing an enemy with a threat level of 10 will add the same amount of progress towards a battery as 
//killing 10 enemies with a threat level of 1. 

//Basic enemy has threat level of 2

//Level 1 - 3 Waves
    //Wave 1 = total threat level of 20:  total roster of 10 basic enemies, minimum to on map at once is 3, start
    //Wave 2 = total threat level of 30
    //Wave 3 = total threat level of 50