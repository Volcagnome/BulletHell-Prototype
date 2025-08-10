using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering.Universal;
using GameUtilities;


public class ArenaManager : MonoBehaviour
{
    public enum ConfigName
    {
        Swamp1 = 0,
        Swamp2 = 1,
        Swamp3 = 2,
        defaultConfig = 99
    };

    public struct ArenaConfig
    {
        public string name;
        public List<Vector2> upSquares;     //Grid pillars to move up
        public List<Vector2> downSquares;   //Grid pillars to move down
        public List<Vector2> dangerSquares; //Grid pillars that need extra checks to avoid stranding the player
        public List<Vector3> spawnPoints;   //Spawn points for enemies
 
        public ArenaConfig(string arenaName, List<Vector2> up, List<Vector2> down, List<Vector2> danger, List<Vector3> spawn)
        {
            name = arenaName;
            upSquares = up;
            downSquares = down;
            dangerSquares = danger;
            spawnPoints = spawn;   
        }
    }


    //List that holds a reference to all the grid squares in the arena
    List<List<GridPillar>> arenaRows = new List<List<GridPillar>>(); 

    //Coordinates of grid squares in current config that are in a non-default position
    List<Vector2> currentArenaBlocks = new List<Vector2>();

    //Dictionary of arena configurations
    Dictionary<ConfigName, ArenaConfig> arenaConfigs = new Dictionary<ConfigName, ArenaConfig>();


    [Header("----------Arena Status----------")]
    ConfigName currentConfig = ConfigName.defaultConfig;
    ConfigName previousConfig = ConfigName.defaultConfig;

    //Arena manager singleton and events
    public static ArenaManager instance;
    public event Action OnArenaSetupComplete;
    public event Action OnArenaChangeComplete;
    public event Action OnArenaResetComplete;

    private void Awake()
    {
        instance = this;
        FillArenaRowArray(); 
        SetNameAndCoords();
        InitializeArenaConfigs();
    }

    private void Start()
    {
        OnArenaSetupComplete?.Invoke();
    }

    //Creates list for each row of the arena
    void FillArenaRowArray()
    {
        for(int i = 0; i<18; i++)
        {
            List<GridPillar> newRow = new List<GridPillar>();
            arenaRows.Add(newRow);
        }
    }

    //Names each grid square in hierarchy according to its coordinates in the grid and adds them to the appropriate List
    void SetNameAndCoords()
    {
        int blockCount = transform.childCount;
        int blockX = 0;
        int blockY = 0;

       GridPillar child;

        for (int i =0;  i < blockCount; i++)
        {
            child = transform.GetChild(i).gameObject.GetComponent<GridPillar>();
            child.name = blockX + ", " + blockY;
            child.GetComponent<GridPillar>().SetCoords(blockX, blockY);
            arenaRows[blockX].Add(child);

            if (blockY < 17)
                blockY++;
            else
            {
                blockY = 0;
                blockX++;
            }

            if (blockX == 18)
                blockX = 0;
        }
    }
    
    //Fills all the lists for each configuration with the specified grid squares.
    void InitializeArenaConfigs()
    {
        arenaConfigs.Add(ConfigName.Swamp1, new ArenaConfig(
            "Swamp1",
            //Up squares
            new List<Vector2> { new Vector2(3, 13), new Vector2(3, 14), new Vector2(4,13), new Vector2(4, 14),
                                new Vector2(5, 13), new Vector2(5, 14), new Vector2(8,8), new Vector2(8, 9),
                                new Vector2(9, 8), new Vector2(9, 9), new Vector2(12,3), new Vector2(12, 4),
                                new Vector2(13, 3), new Vector2(13, 4), new Vector2(14,3), new Vector2(14,4)},

            //Down squares
            new List<Vector2> { new Vector2(3, 3), new Vector2(3, 4), new Vector2(3, 5), new Vector2(3, 6),
                                new Vector2(4, 3), new Vector2(4, 5), new Vector2(4, 6), new Vector2(5, 3), 
                                new Vector2(5, 4), new Vector2(12, 13), new Vector2(12, 14), new Vector2(13, 11), 
                                new Vector2(13, 12),  new Vector2(13, 14), new Vector2(14, 11), new Vector2(14, 12), 
                                new Vector2(14, 13), new Vector2(14, 14) },

            //Danger Squares
            new List<Vector2> { new Vector2(4,4), new Vector2(13,13) },

            //Spawn Points
            new List<Vector3> { arenaRows[3][10].GetSpawnPoint(), arenaRows[8][3].GetSpawnPoint(),
                                arenaRows[9][14].GetSpawnPoint(), arenaRows[14][7].GetSpawnPoint(),}));



        arenaConfigs.Add(ConfigName.Swamp2, new ArenaConfig(
      "Swamp2",         //Up Squares
      new List<Vector2> { new Vector2(7, 3), new Vector2(7, 4), new Vector2(7, 13), new Vector2(7, 14), new Vector2(8, 3),
                          new Vector2(8, 4), new Vector2(8, 13), new Vector2(8, 14), new Vector2(9, 3), new Vector2(9, 4),
                          new Vector2(9, 13), new Vector2(9, 14), new Vector2(10, 3), new Vector2(10, 4), new Vector2(10, 13),
                          new Vector2(10, 14) },

      //Down Squares
      new List<Vector2> { new Vector2(0, 4), new Vector2(0, 13),
                          new Vector2(1, 5), new Vector2(1, 12), new Vector2(2, 6), new Vector2(2, 7),
                          new Vector2(2, 8), new Vector2(2, 9), new Vector2(2, 10),new Vector2(2, 11), new Vector2(15, 6),
                          new Vector2(15, 7), new Vector2(15, 8), new Vector2(15, 9), new Vector2(15, 10),new Vector2(15, 11),
                          new Vector2(16, 5), new Vector2(16, 12), new Vector2(17, 4), new Vector2(17, 13)},

      //Danger Squares
      new List<Vector2> { new Vector2(0, 5), new Vector2(0, 6), new Vector2(0, 7), new Vector2(0, 8),
                          new Vector2(0, 9), new Vector2(0, 10), new Vector2(0, 11), new Vector2(0, 12), new Vector2(1, 6), 
                          new Vector2(1, 7), new Vector2(1, 8), new Vector2(1, 9), new Vector2(1, 10), new Vector2(1, 11),
                          new Vector2(16, 6), new Vector2(16, 7), new Vector2(16, 8), new Vector2(16, 9), new Vector2(16, 10), 
                          new Vector2(16, 11), new Vector2(17, 5), new Vector2(17, 6), new Vector2(17, 7), new Vector2(17, 8), 
                          new Vector2(17, 9), new Vector2(17, 10), new Vector2(17, 11), new Vector2(17, 12)},


            //Spawn Points
            new List<Vector3> { arenaRows[4][3].GetSpawnPoint(), arenaRows[4][14].GetSpawnPoint(),
                                arenaRows[13][3].GetSpawnPoint(), arenaRows[13][14].GetSpawnPoint(),}));

        arenaConfigs.Add(ConfigName.Swamp3, new ArenaConfig(
    "Swamp3",
    //Up squares
    new List<Vector2> { new Vector2(3, 3), new Vector2(3, 4), new Vector2(3, 13), new Vector2(3, 14), new Vector2(4, 3), 
                        new Vector2(4, 4), new Vector2(4, 13), new Vector2(4, 14), new Vector2(13, 3), new Vector2(13, 4), 
                        new Vector2(13, 13), new Vector2(13, 14), new Vector2(14, 3), new Vector2(14, 4), new Vector2(14, 13), 
                        new Vector2(14, 14)},

    //Down squares
    new List<Vector2> { new Vector2(5, 7), new Vector2(5, 8), new Vector2(5, 9), new Vector2(5, 10), new Vector2(6, 6),
                        new Vector2(6, 11),
                        new Vector2(7, 6), new Vector2(7, 7), new Vector2(7, 8),new Vector2(7, 9), new Vector2(7, 10), 
                        new Vector2(7, 11), new Vector2(10, 6),new Vector2(10, 7),new Vector2(10, 8), new Vector2(10, 9),
                        new Vector2(10, 10),new Vector2(10, 11), new Vector2(11, 6), new Vector2(11,11), new Vector2(12,7),
                        new Vector2(12,8), new Vector2(12,9), new Vector2(12,10) },

      //Danger Squares
      new List<Vector2> { new Vector2(6, 7), new Vector2(6, 8), new Vector2(6, 9), new Vector2(6, 10), new Vector2(11, 7), 
                          new Vector2(11, 8), new Vector2(11, 9), new Vector2(11, 9), new Vector2(11,10) },

      //Spawn Points
      new List<Vector3> { arenaRows[1][9].GetSpawnPoint(), arenaRows[9][2].GetSpawnPoint(),
                          arenaRows[8][15].GetSpawnPoint(), arenaRows[16][8].GetSpawnPoint(),}));

    }


    //Rolls a random arena configuration and initiates arena change.
    public void RollNewArena()
    {
        ConfigName newArena = RollRandomConfig();
        previousConfig = currentConfig;
        currentConfig = newArena;
        ChangeArenaConfig(newArena);
    }


    //Rolls a random configuration that is different than the current or previous config.
    public ConfigName RollRandomConfig(){

        ConfigName newConfig = currentConfig;

        if (arenaConfigs.Count > 0)
        {
            do
            { 
                newConfig = (ConfigName)UnityEngine.Random.Range(0, arenaConfigs.Count);

            }
            while (newConfig == previousConfig | newConfig == currentConfig);
        }
        else Debug.Log("No arena configs.");

        return newConfig;
    }

    //Checks all danger squares first to create any needed bridges then calls up/down on all squares on up/down lists. 
    public void ChangeArenaConfig(ConfigName name){

        ArenaConfig newConfig = arenaConfigs[name];
        GridPillar pillar;

    
        for (int i = 0; i < newConfig.dangerSquares.Count; i++)
        {
            int row = (int)newConfig.dangerSquares[i].x;
            int col = (int)newConfig.dangerSquares[i].y;

            pillar = arenaRows[row][col];
            currentArenaBlocks.Add(pillar.GetRowCol());

            pillar.CheckDangerSquare(GridPillar.PillarPosition.downPos);

        }


        for (int i = 0; i < newConfig.upSquares.Count; i++)
            {
                int x = (int)newConfig.upSquares[i].x;
                int y = (int)newConfig.upSquares[i].y;

                pillar = arenaRows[x][y];
                currentArenaBlocks.Add(pillar.GetRowCol());

                pillar.CheckIfReadyToMove(GridPillar.PillarPosition.upPos);
            }

        for (int i = 0; i < newConfig.downSquares.Count; i++)
            {
                int x = (int)newConfig.downSquares[i].x;
                int y = (int)newConfig.downSquares[i].y;

                pillar = arenaRows[x][y];
                currentArenaBlocks.Add(pillar.GetRowCol());

                pillar.CheckIfReadyToMove(GridPillar.PillarPosition.downPos);
            }

        StartCoroutine(WaitForPillars(false));
    }


    //Resets all grid squares to default positions.
    public void ResetArena()
    { 
        GridPillar pillar;
        Vector2 coords;


            for (int i = 0; i < currentArenaBlocks.Count; i++)
            {
                coords = currentArenaBlocks[i];
                pillar = arenaRows[(int)coords.x][(int)coords.y];

            pillar.CheckIfReadyToMove(GridPillar.PillarPosition.defaultPos);
      
            }

        StartCoroutine(WaitForPillars(true));
    }

    //Monitor's progress of all grid squares until they have reached their correct positions. 
    IEnumerator WaitForPillars(bool isResettingArena)
    {
        Vector2 coords;
        bool blocksReady = false;

        while (!blocksReady)
        {
            blocksReady = true;

            for (int i = 0; i < currentArenaBlocks.Count; i++)
            {
                coords = currentArenaBlocks[i];
                if (arenaRows[(int)coords.x][(int)coords.y].GetPillarState() != GridPillar.PillarState.Idle )
                {
                    blocksReady = false;
                    break;
                }
            }

            yield return new WaitForSeconds(0.25f);
        }

        if (isResettingArena)
        {
            currentArenaBlocks.Clear();
            OnArenaResetComplete?.Invoke();
            
        }
        else
            OnArenaChangeComplete?.Invoke();
    }


    public bool CheckList(Vector2 coords, List<Vector2> list)
    {
        if (list.Contains(coords))
            return true;
        else
            return false;
    }

    public bool CheckDownList(Vector2 coords)
    {
        if (arenaConfigs[currentConfig].downSquares.Contains(coords))
            return true;
        else
            return false;
    }

    bool CheckNeighbor(Vector2 coords, int xOffset, int yOffset)
    {
        ArenaConfig current = arenaConfigs[currentConfig];
        Vector2 neighborCoords = new Vector2(coords.x + xOffset,coords.y + yOffset);

        if (!current.downSquares.Contains(neighborCoords) && !current.upSquares.Contains(neighborCoords) &&
           !current.dangerSquares.Contains(neighborCoords))
            return true;
        else 
            return false;   
    }

    public SpawnData GetUndergroundSpawn()
    {
        bool foundSafeSpawn = false;
        Vector2 coords = new Vector2(); 
        ArenaConfig config = arenaConfigs[currentConfig];

        while (!foundSafeSpawn)
        {
            coords.x = UnityEngine.Random.Range(0, 17);
            coords.y = UnityEngine.Random.Range(0, 17);

            if (!CheckList(coords, config.downSquares) && 
                !CheckList(coords, config.upSquares) && 
                !CheckList(coords, config.dangerSquares))
                    foundSafeSpawn = true;
        }

        Vector3 spawnPosition = arenaRows[(int)coords.x][(int)coords.y].transform.position;

        SpawnData spawnPoint = new SpawnData(spawnPosition, Quaternion.identity);

        return spawnPoint;
    }

    public SpawnData GetPitSpawn()
    {

        Vector2 coords = arenaConfigs[currentConfig].downSquares[UnityEngine.Random.Range(0, arenaConfigs[currentConfig].downSquares.Count)];
        GridPillar randomDownSquare = arenaRows[(int)coords.x][(int)coords.y];

        ArenaConfig current = arenaConfigs[currentConfig];

        bool safeSpawnFound = false;

        Vector3 spawnPosition = randomDownSquare.transform.position;

        Quaternion spawnRotation = Quaternion.identity;

        //Check square above
        if (coords.x > 0)
        {
            if (!safeSpawnFound && CheckNeighbor(coords, -1, 0))
            {
                safeSpawnFound = true;
                spawnRotation = Quaternion.Euler(0, 0, 0);
            }    
        }

        //Check square to the right
        if (coords.y < 17)
        {
            if (!safeSpawnFound && CheckNeighbor(coords, 0, 1))
            {
                safeSpawnFound = true;
                spawnRotation = Quaternion.Euler(0, 90, 0);
            }
        }

        //Check square below
        if (coords.x < 17)
        {
            if (!safeSpawnFound && CheckNeighbor(coords, 1, 0))
            {  
                safeSpawnFound = true;
                spawnRotation = Quaternion.Euler(0, 180, 0);
            }
        }

        //Check square to the left
        if (coords.y > 0)
        {
            if (!safeSpawnFound && CheckNeighbor(coords, 0, -1))
            {
                safeSpawnFound = true;
                spawnRotation = Quaternion.Euler(0, -90, 0);
            }
        }

        SpawnData spawnPoint = new SpawnData(spawnPosition, spawnRotation); 

        return spawnPoint;

    }


    // Getters/Setters
    public List<Vector3> GetSpawnPoints() { return arenaConfigs[currentConfig].spawnPoints; }

    public GridPillar GetPillar(int x, int y) { return arenaRows[(int)x][(int)y]; }

}
