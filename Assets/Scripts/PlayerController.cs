using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour, I_Damage
{
    [Header("Components")]
    public Transform playerBody;
    public CharacterController controller;
    public Camera playerCam;
    public float cameraHeight;
    public GameObject reticle;

    [Header("Controls")]
    float horizontalInput;
    float verticalInput;

    [Header("Attributes")]
    public float rotationSpeed;
    public float speed;
    float currentSpeed;
    public float maxHP;
    public float currentHP;
    public float maxBattery = 60;
    float currentBattery;
    public float batteryRechargeTime = 3f;

    [Header("Health Regeneration")]
    public float hpRegenRate = 2f;
    public float damageCooldown = 3f;
    Coroutine hpRegenCoroutine;
    float lastDamageTime;

    [Header("Weapons")]
    public Transform shootPos;
    public float primaryFireRate = 0.25f;
    public float weaponSwitchTime = 0.5f;
    public float heavyCooldown = 3f;

    bool isSwitchingWeapon = false;
    bool isShootingPrimaryWeapon = false;
    bool primaryWeaponActive = true;
    bool heavyWeaponReady = true;
    bool isDown = false;
    bool isRecharging = false;
    bool batteryDepleted = false;
    public event Action OnPlayerDown;
    public event Action OnBatteryDepleted;

    public GameObject primaryAmmo;
    public GameObject heavyAmmo;


    // Start is called before the first frame update
    void Start()
    {
        cameraHeight = playerCam.transform.position.y;
        currentHP = maxHP;
        currentBattery = maxBattery;
        StartCoroutine(DepleteBattery());
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        CheckHealthRegen();

        if (!isDown && !batteryDepleted)
            RotatePlayerToMouse();
        
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void GetInput()
    {
        //Gets input from player and stores it in variables
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (!isDown && !batteryDepleted)
        {
            //Fire primary weapon
            if (Input.GetButton("Fire1") && !isShootingPrimaryWeapon && primaryWeaponActive && !isSwitchingWeapon)
                StartCoroutine(ShootPrimaryWeapon());

            //Fire Heavy Weapon
            if (Input.GetButton("Fire2") && heavyWeaponReady && !isSwitchingWeapon)
                StartCoroutine(SwitchWeapon());
        }
    }

    void MovePlayer()
    {
        if (!isDown && !batteryDepleted)
        {
            //Calculates move direction based on horizontal/vertical input
            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

            //Passes modified vector to controller.Move
            moveDirection.Normalize();
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
    }

    void RotatePlayerToMouse()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = playerCam.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, cameraHeight));

        mouseWorldPosition.y = transform.position.y;

        reticle.transform.position = mouseWorldPosition;

        Vector3 direction = mouseWorldPosition - transform.position;

        if (direction.sqrMagnitude > 0.1f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    IEnumerator ShootPrimaryWeapon()
    {
        isShootingPrimaryWeapon = true;

        GameObject bullet1 = PoolManager.instance.GetInstance(PoolManager.ObjType.Bullet_PlayerStandard);
        bullet1.transform.position = shootPos.position + new Vector3(0, 0, 0.5f);
        bullet1.transform.rotation = shootPos.transform.rotation;
        bullet1.SetActive(true);

        GameObject bullet2 = PoolManager.instance.GetInstance(PoolManager.ObjType.Bullet_PlayerStandard);
        bullet2.transform.position = shootPos.position + new Vector3(-0.5f, 0, 0);
        bullet2.transform.rotation = shootPos.transform.rotation;
        bullet2.SetActive(true);

        GameObject bullet3 = PoolManager.instance.GetInstance(PoolManager.ObjType.Bullet_PlayerStandard);
        bullet3.transform.position = shootPos.position + new Vector3(0.5f, 0, 0);
        bullet3.transform.rotation = shootPos.transform.rotation;
        bullet3.SetActive(true);

        //Instantiate(primaryAmmo, shootPos.position + new Vector3 (0, 0, 0.5f), shootPos.transform.rotation);
        //Instantiate(primaryAmmo, shootPos.position + new Vector3(-0.5f, 0,0), shootPos.transform.rotation);
        //Instantiate(primaryAmmo, shootPos.position + new Vector3(0.5f, 0, 0), shootPos.transform.rotation);

        yield return new WaitForSeconds(primaryFireRate);
        isShootingPrimaryWeapon = false;
    }

    void ShootHeavyWeapon()
    {
        heavyWeaponReady = false;
        Instantiate(heavyAmmo, shootPos.position, shootPos.rotation);

        StartCoroutine(SwitchWeapon());

        Invoke("ResetHeavyWeapon", heavyCooldown);
    }

    void ResetHeavyWeapon()
    {
        heavyWeaponReady=true;
    }

    IEnumerator SwitchWeapon()
    {   
        isSwitchingWeapon = true;

        yield return new WaitForSeconds(weaponSwitchTime);

        if (primaryWeaponActive)
        {
            primaryWeaponActive = false;
            ShootHeavyWeapon();
        }
        else
            primaryWeaponActive = true;

        isSwitchingWeapon = false;
    }

    public void TakeDamage(float damageAmount)
    {
        if (!isDown && !batteryDepleted)
        {
            currentHP -= damageAmount;

            lastDamageTime = Time.time;

            if (hpRegenCoroutine != null)
            {
                StopCoroutine(hpRegenCoroutine);
                hpRegenCoroutine = null;
            }

            if (currentHP <= 0)
            {
                PlayerDown();
            }
        }
    }

    void PlayerDown()
    {
        isDown = true;
        OnPlayerDown?.Invoke();
    }

    public void RebootPowerArmor()
    {
        currentHP = maxHP;
        isDown = false;
    }


    public float GetBattery()
    {
        return currentBattery/maxBattery;
    }

    public float GetHealth()
    {
        return currentHP / maxHP;
    }

    IEnumerator DepleteBattery()
    {
        while (currentBattery > 0)
        {
            currentBattery -= 1 * Time.deltaTime;

            if (currentBattery <= 0)
            {
                OnBatteryDepleted?.Invoke();
                batteryDepleted = true;
            }

            yield return null; 
        }
    }

    IEnumerator RegenerateHealth()
    {
        while (currentHP < maxHP)
        {
            if (Time.time - lastDamageTime >= damageCooldown)
            {

                currentHP += hpRegenRate * Time.deltaTime;

                if (currentHP > maxHP)
                {
                    currentHP = maxHP;
                    yield break; 
                }
            }
            yield return null; 
        }
    }

    void CheckHealthRegen()
    {
        if (Time.time - lastDamageTime >= damageCooldown && currentHP < maxHP)
        {
            if (hpRegenCoroutine == null)
                StartCoroutine(RegenerateHealth());
        }
    }

    public void StartBatteryRecharge(float amount)
    {
        StartCoroutine(BatteryPickup(amount));
    }

    public IEnumerator BatteryPickup(float amount)
    {
        isRecharging = true;
        float chargeTimeElapsed = 0f;
        float startingBatteryCharge = currentBattery;
        float targetCharge = currentBattery + amount;

        if (targetCharge > maxBattery)
            targetCharge = maxBattery;

        while (chargeTimeElapsed < batteryRechargeTime)
        {
            chargeTimeElapsed += Time.deltaTime; 
            currentBattery = Mathf.Lerp(startingBatteryCharge, targetCharge, chargeTimeElapsed / batteryRechargeTime);

            yield return null;
        }

        isRecharging = false;
    }
}

// primaryActive
// heavyReady
// isSwitchingWeapon

//Right mouse button clicked: 

// switch weapon delay ----------------------------------- primaryActive = false, heavyReady = true, isSwitchingWeapon = true
// heavy weapon fired once ------------------------------- primaryActive = false, heavyReady = true, isSwitchingWeapon = false
// heavy weapon cooldown started  ------------------------ primaryActive = false, heavyReady = false, isSwitchingWeapon = false
// switch weapon delay ----------------------------------- primaryActive = true,  heavyReady = false, isSwitchingWeapon = true
// primary weapon able to be fired again ----------------- primaryActive = true, heavyReady = false, isSwitchingWeapon = false
// heavy weapon ready to fire after cooldown expires------ primaryActive = true, heavyReady = true, isSwitchingWeapon = false
