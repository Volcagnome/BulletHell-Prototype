using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("----------Counts----------")]
    public TextMeshProUGUI waveCountText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI enemyCountText;
    public GameObject addScore;
    float currentDisplayTime;
    float addScoreDisplayTime;
    Coroutine addScoreCoroutine;

    [Header("----------Reboot Menu----------")]
    List<Direction> sequence = new List<Direction>();
    public enum Direction { NONE = 0, UP = 1, DOWN = 2, LEFT = 3, RIGHT = 4 };
    public GameObject rebootMenu;
    public GameObject sequenceHandler;
    public GameObject arrowPrefab;
    public Sprite greyedArrow;
    public Sprite correctArrow;
    int currentSequenceElement;
    bool playerDown = false;

    [Header("----------Player UI----------")]
    public UnityEngine.UI.Image batteryFill;
    public UnityEngine.UI.Image batteryFrame;
    public Color batteryLowColor;
    public Color batteryNormColor;
    public GameObject batteryDropped;
    public UnityEngine.UI.Image healthBarFill;
   

    [Header("----------Game Over Menu----------")]
    public GameObject GameOverMenu;
    public TextMeshProUGUI totalStageCountText;
    public TextMeshProUGUI totalGameKillCountText;
    public TextMeshProUGUI totalScoreCountText;

    public static UIManager instance;
    public event Action OnUISetupComplete;

    bool isPaused = false;
    public GameObject pauseMenu;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnUISetupComplete?.Invoke();
        GameManager.instance.OnPlayerDown += OpenRebootMenu;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDown)
        {
            CheckDirectionInput();
        }

        if (Input.GetButtonDown("Cancel"))
            Pause();

        UpdateUI();
    }

    void UpdateUI()
    {
        BatteryUI();
        CountsUI();
        HealthUI();
    }

    private void Pause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);

        if (isPaused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f; 
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();  // Quit the game in a build
#endif
    }

    private void CountsUI()
    {
        waveCountText.text = GameManager.instance.stageCount.ToString();
        totalScoreText.text = GameManager.instance.totalScore.ToString();
        enemyCountText.text = PoolManager.instance.GetTotalActiveEnemies().ToString();
    }

    void BatteryUI()
    {
        float playerBattery = GameManager.instance.playerController.GetBattery();

       
        batteryFill.fillAmount = Mathf.Lerp(batteryFill.fillAmount, playerBattery, Time.deltaTime * 1f);

        if (batteryFill.fillAmount < 0.4f)
        {
            batteryFill.color = batteryLowColor;
            batteryFrame.color = batteryLowColor;
        }
        else
        {
            batteryFill.color = batteryNormColor;
            batteryFrame.color = batteryNormColor;
        }
    }

    void HealthUI()
    {
        healthBarFill.fillAmount = GameManager.instance.playerController.GetHealth();
    }

    public void OpenRebootMenu()
    {
        playerDown = true;
        rebootMenu.SetActive(true);
        sequence = GenerateRandomRebootSequence(5);
        ShowArrowSequence();
    }

    List<Direction> GenerateRandomRebootSequence(int length)
    {
        List<Direction> sequence = new List<Direction>();

        for (int i = 0; i < length; i++)
        {
            Direction randDirection = (Direction)UnityEngine.Random.Range(1, 4);
            sequence.Add(randDirection);
        }
        return sequence;
    }

    public void ShowArrowSequence()
    {
        currentSequenceElement = 0;

        foreach (Transform child in sequenceHandler.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Direction dir in sequence)
        {

            GameObject arrow = Instantiate(arrowPrefab, sequenceHandler.transform);

            switch (dir)
            {
                case Direction.UP:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Direction.DOWN:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, -180);
                    break;
                case Direction.LEFT:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
                    break;
                case Direction.RIGHT:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
                    break;
            }

        }
    }

    void CheckDirectionInput()
    {
        Direction input = GetDirectionInput();

        if (input != Direction.NONE)
        {
            if (input == sequence[currentSequenceElement])
            {
                sequenceHandler.transform.GetChild(currentSequenceElement).GetComponent<UnityEngine.UI.Image>().sprite = correctArrow;
                currentSequenceElement++;
                if (currentSequenceElement == sequence.Count)
                {
                    playerDown = false;
                    currentSequenceElement = 0;
                    rebootMenu.SetActive(false);
                    GameManager.instance.RevivePlayer();
                }
            }
        }
    }

    Direction GetDirectionInput()
    {
        Direction input = Direction.NONE;

        if (Input.GetButtonDown("Up"))
        {
            input = Direction.UP;
        }
        else if (Input.GetButtonDown("Down"))
        {
            input = Direction.DOWN;
        }
        else if (Input.GetButtonDown("Left"))
        {
            input = Direction.LEFT;
        }
        else if (Input.GetButtonDown("Right"))
        {
            input = Direction.RIGHT;
        }

        return input;
    }

    public void GameOver()
    {
        GameOverMenu.SetActive(true);
        totalStageCountText.text = GameManager.instance.GetTotalStageCount().ToString();
        totalGameKillCountText.text = GameManager.instance.GetTotalKillCount().ToString();
        totalScoreCountText.text = GameManager.instance.GetTotalScore().ToString();    
    }


    public void RestartGame()
    {
        if (isPaused)
            Pause();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    public void ToggleBatteryPrompt(bool status)
    {
        batteryDropped.SetActive(status);
    }

    public void ShowPoints(int points)
    {
        if(addScoreCoroutine != null)
            StopCoroutine(addScoreCoroutine);

        addScoreCoroutine = StartCoroutine(AddScore(points));
    }

    IEnumerator AddScore(int points)
    {
        addScore.SetActive(true);
        Color color;
        string sign = "";

        if (points > 0)
        {
            color = Color.green;
            sign = "+";
        }
        else
        {
            color = Color.red;
        }

        TextMeshProUGUI addScoreText = addScore.GetComponent<TextMeshProUGUI>();
        addScoreText.text =  sign + points.ToString();
        addScoreText.color = color; 

        yield return new WaitForSeconds(2f);

        addScore.SetActive(false);

        addScoreCoroutine = null;
    }

}
