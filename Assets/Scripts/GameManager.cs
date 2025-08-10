using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using static ArenaManager;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("----------Components----------")]
    public static GameManager instance;
    public ArenaManager arenaManager;
    public PoolManager poolManager;
    public UIManager uiManager;
    public GameObject player;
    public PlayerController playerController;
    public GameObject batteryPrefab;

    [Header("----------Compontent Status----------")]
    bool poolReady = false;
    bool arenaReady = false;
    bool UIReady = false;
    bool playerDown = false;
    

    [Header("----------Game Stats----------")]
    public int stageCount = 0;
    public int totalKillCount = 0;
    public int stageKillCount = 0;
    public int stageKillTarget = 0;
    public int baseEnemyNum = 10;
    public int totalScore = 0;
    int killsSinceLastBattery = 0;
    public int OGkillsNeededForBattery = 20;
    int killsNeededForBattery;
    List<Vector3> spawnPoints;
    public int stageCompletePoints = 250;

    public event Action OnPlayerDown;
    public event Action OnPlayerRevive;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        killsNeededForBattery = OGkillsNeededForBattery;
        Subscribe();
    }

    private void Subscribe()
    {
        arenaManager.OnArenaSetupComplete += HandleArenaSetupComplete;
        arenaManager.OnArenaChangeComplete += HandleArenaChangeComplete;
        arenaManager.OnArenaResetComplete += HandleArenaResetComplete;
        poolManager.OnPoolSetupComplete += HandlePoolSetupComplete;

        playerController.OnPlayerDown += HandlePlayerDown;
        playerController.OnBatteryDepleted += HandleBatteryDepleted;

        uiManager.OnUISetupComplete += HandleUISetupComplete;
    }
    
    private void HandleArenaSetupComplete()
    {
        if (poolReady && UIReady)
            StartNewStage();
        
        arenaReady = true;
    }

    private void HandleArenaChangeComplete()
    {
        stageKillTarget = baseEnemyNum + (stageCount * 2);

        spawnPoints = ArenaManager.instance.GetSpawnPoints();

        StartCoroutine(PoolManager.instance.StartSpawning(stageKillTarget, spawnPoints));
    }
    private void HandleArenaResetComplete()
    {
   

        StartNewStage();
    }

    private void HandlePoolSetupComplete()
    {

        if (arenaReady && UIReady)
            StartNewStage();

        poolReady = true;
    }

    private void HandleUISetupComplete()
    {
   

        if (arenaReady && poolReady)
            StartNewStage();
        
        UIReady = true;
    }

    private void HandlePlayerDown()
    {
        OnPlayerDown?.Invoke();
        IncrementScore((int)(totalScore * -0.1f));
        UIManager.instance.OpenRebootMenu();
    }

    public void RevivePlayer()
    {
        OnPlayerRevive?.Invoke();
        playerController.RebootPowerArmor();
    }

    public void IncrementKillCounts(Vector3 enemyPosition, int pointValue)
    {
        totalKillCount++;
        stageKillCount++;
        killsSinceLastBattery++;
        IncrementScore(pointValue);
        

        if(killsSinceLastBattery == killsNeededForBattery) 
        {
            killsSinceLastBattery = 0;
            SpawnBattery(enemyPosition);
        }

        if (stageKillCount == stageKillTarget)
        {
            IncrementScore(stageCompletePoints);
            ArenaManager.instance.ResetArena();
        }
    }

    public void IncrementScore(int points)
    {
        int addScore = points;

        if (points > 0)
            addScore *= stageCount;

        totalScore += addScore;

        UIManager.instance.ShowPoints(addScore);
    }

    void SpawnBattery(Vector3 enemyPosition)
    {
        Instantiate(batteryPrefab, enemyPosition, Quaternion.identity);
    }

    void StartNewStage()
    {
        stageCount++;
        killsNeededForBattery += stageCount;
        stageKillCount = 0;
        ArenaManager.instance.RollNewArena();
    }
    void HandleBatteryDepleted()
    {
        OnPlayerDown?.Invoke();
        UIManager.instance.GameOver();
    }

    public int GetTotalStageCount()
    {
        return stageCount;
    }

    public int GetTotalKillCount()
    {
        return totalKillCount;
    }

    public int GetTotalScore()
    {
        return totalScore;
    }


}
