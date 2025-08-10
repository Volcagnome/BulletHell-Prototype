using System.Collections;
using UnityEngine;


public class BatteryDrop : MonoBehaviour
{
    [Header("-----Attributes-----")]
    public float maxSecondsBeforeDespawn;
    float secondsBeforeDespawn;
    public int pointValue;
    public float batteryPickupAmount;

    [Header("-----Components-----")]
    public SpriteRenderer spriteRend;
    public Sprite green;
    public Sprite red;
    public Light light;

    [Header("-----Status-----")]
    bool flashing = false;


    void Start()
    {
        secondsBeforeDespawn = maxSecondsBeforeDespawn;
        StartCoroutine(Timer());
        UIManager.instance.ToggleBatteryPrompt(true);
    }


    void Update()
    {
        //Stars flashing red if 75% of time before despawn has passed
        if (!flashing && secondsBeforeDespawn < maxSecondsBeforeDespawn * 0.25f)
            StartCoroutine(Flash());
    }
    
    //Waits for configured number of seconds before despawning
    IEnumerator Timer()
    {
        while(secondsBeforeDespawn > 0)
        {
            secondsBeforeDespawn--;

            yield return new WaitForSeconds(1f);
        }

        UIManager.instance.ToggleBatteryPrompt(false);
        Destroy(gameObject);
    }
    

    //Flashes red
    IEnumerator Flash()
    {
        flashing = true;
        light.color = Color.red;

        while (flashing)
        {
            light.enabled = false;
            spriteRend.sprite = green;

            yield return new WaitForSeconds(0.25f);

            light.enabled = true;
            spriteRend.sprite = red;

            yield return new WaitForSeconds(0.25f);
        }
    }

    //When player enters range, applies battery charge and destroys battery
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            UIManager.instance.ToggleBatteryPrompt(false);
            GameManager.instance.IncrementScore(pointValue);
            GameManager.instance.playerController.StartBatteryRecharge(batteryPickupAmount);
            Destroy(gameObject);
        }  
    }
}
