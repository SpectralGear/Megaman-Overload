using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using System.ComponentModel;
using System;

[RequireComponent(typeof(CharControl))]
public class Buster : MonoBehaviour
{
    [SerializeField] float FullCharge,ShotTiming;
    [SerializeField] GameObject projectileSpawn, projectileSpawnRotate,leftBuster;
    [SerializeField] List<GameObject> attackPrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> BassBulletVariants = new List<GameObject>();
    [SerializeField] ParticleSystem ChargingEffect,FullChargeEffect,OverChargeEffect;
    Animator anim;
    private DefaultControls playerInputActions;
    private bool facingRight=true,ChargingWeapon;
    public bool armLaunched=false;
    float BusterCharge,HalfCharge,OverCharge,FireTimer,pointBuster=0,ChargeSpeed;
    private List<GameObject> projectilesAndAttacks = new List<GameObject>();
    public enum Projectile {NoCharge, HalfCharge, FullCharge, OverCharge, SickleChainShort, SickleChainLong, SafetyBall, BallBounce, ResinGlob, MegawattSurge, Brickfall, Firecracker, FirecrackerBarrage, WaterCannon, CycloneStrike, CycloneStrikeSlash, AnimalAdaptor}
    public enum Weapon {MegaBuster,SickleChain,SafetyBall,MegawattSurge,CycloneStrike,ResinGlob,WaterCannon,Firecracker,Brickfall,AnimalAdaptor};
    public float[] WeaponEnergy = new float[10] {float.PositiveInfinity,28,28,28,28,28,28,28,28,28};
    private List<Weapon> ChargeableWeapons = new List<Weapon>(){Weapon.MegaBuster,Weapon.SickleChain,Weapon.Firecracker, Weapon.AnimalAdaptor};
    public List<Weapon> OwnedWeapons = new List<Weapon>();
    CharControl cc;
    [SerializeField] Weapon EquippedWeapon = Weapon.MegaBuster;
    [SerializeField] Texture2D Ball, BallAttack;
    [SerializeField] Material charMat;
    [SerializeField] AudioSource chargingAudioSource;
    [SerializeField] AudioClip WeaponEnergyGainAudio;
    [SerializeField] Texture[] WeaponColors = new Texture[10];
    public Weapon _equippedWeapon => EquippedWeapon;
    private void Awake()
    {
        playerInputActions = new DefaultControls();
        playerInputActions.Enable();
    }
    void Start()
    {
        anim = gameObject.GetAny<Animator>();
        HalfCharge=FullCharge/2;
        OverCharge=FullCharge*1.5f;
        cc = gameObject.GetAny<CharControl>();
        SaveData saveData = SaveManager.LoadGame(0);
        //if (saveData!=null){OwnedWeapons = new List<Weapon>(saveData.ObtainedWeapons);}
        if (!OwnedWeapons.Contains(Weapon.MegaBuster)){OwnedWeapons.Insert(0,Weapon.MegaBuster);}
        charMat.SetTexture("_MainTex", WeaponColors[(int)EquippedWeapon]);
    }
    private void OnEnable()
    {
        playerInputActions.Controls.Shoot.started += OnShootStarted;
        playerInputActions.Controls.Shoot.canceled += OnShootCanceled;
        playerInputActions.WeaponQuickSwap.WeaponWheel.performed += OnQuickSwap;
        playerInputActions.WeaponQuickSwap.PreviousNext.performed += OnQuickSwap;
        playerInputActions.WeaponQuickSwap.AnimalAdaptor.performed += OnQuickSwap;
    }
    private void OnDisable()
    {
        playerInputActions.Controls.Shoot.started -= OnShootStarted;
        playerInputActions.Controls.Shoot.canceled -= OnShootCanceled;
        playerInputActions.WeaponQuickSwap.WeaponWheel.performed -= OnQuickSwap;
        playerInputActions.WeaponQuickSwap.PreviousNext.performed -= OnQuickSwap;
        playerInputActions.WeaponQuickSwap.AnimalAdaptor.performed -= OnQuickSwap;
    }
    private void OnDestroy()
    {
        playerInputActions.Controls.Disable();
    }
    private void OnQuickSwap(InputAction.CallbackContext context)
    {
        int weaponIndex;
        string controlPath = context.control.path;
        if (context.control.device is Keyboard)
        {
            if (char.IsDigit(controlPath.Last()))
            {
                if (controlPath.Contains("numpad"))
                {
                    weaponIndex = GetWeaponFromKeyboard(int.Parse(controlPath.Last().ToString()));
                    EquipWeapon(weaponIndex);
                }
                else if (controlPath.Contains("Keyboard"))
                {
                    float PreOrNext = playerInputActions.WeaponQuickSwap.PreviousNext.ReadValue<float>();
                    if (PreOrNext!=0){weaponIndex=PreOrNext>0 ? (int)EquippedWeapon+1 : (int)EquippedWeapon-1;}
                    else if (int.Parse(controlPath.Last().ToString())==0){weaponIndex = 9;}
                    else {weaponIndex = int.Parse(controlPath.Last().ToString())-1;}
                    EquipWeapon(weaponIndex);
                }
            }
            else
            {
                float PreOrNext = playerInputActions.WeaponQuickSwap.PreviousNext.ReadValue<float>();
                if (PreOrNext!=0){weaponIndex=PreOrNext>0 ? 10 : -1;}
                else {weaponIndex = (int)EquippedWeapon;}
                EquipWeapon(weaponIndex);
            }
        }
        else if (context.control.device is Gamepad)
        {
            if (controlPath.Contains("rightStickPress"))
            {
                EquipWeapon(0);
            }
            else
            {
                Vector2 rightStick = playerInputActions.WeaponQuickSwap.WeaponWheel.ReadValue<Vector2>()!=null ? playerInputActions.WeaponQuickSwap.WeaponWheel.ReadValue<Vector2>() : Vector2.zero;
                float PreOrNext = playerInputActions.WeaponQuickSwap.PreviousNext.ReadValue<float>();
                if (PreOrNext!=0){weaponIndex=PreOrNext>0 ? 10 : -1;}
                else {weaponIndex = GetWeaponFromStickDirection(rightStick);}
                EquipWeapon(weaponIndex);
            }
        }
    }
    private int GetWeaponFromStickDirection(Vector2 stick)
    {
        float angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360; // Normalize angle
        switch (angle)
        {
            case >= 337.5f or < 22.5f: return 5;
            case >= 22.5f and < 67.5f: return 3;
            case >= 67.5f and < 112.5f: return 2;
            case >= 112.5f and < 157.5f: return 1;
            case >= 157.5f and < 202.5f: return 4;
            case >= 202.5f and < 247.5f: return 6;
            case >= 247.5f and < 292.5f: return 7;
            case >= 292.5f and < 337.5f: return 8;
            default: return 0;
        }
    }
    private int GetWeaponFromKeyboard(int numpadNumber)
    {
        switch (numpadNumber)
        {
            case 1: return 6;
            case 2: return 7;
            case 3: return 8;
            case 4: return 4;
            case 6: return 5;
            case 7: return 1;
            case 8: return 2;
            case 9: return 3;
            case 0: return 9;
            default: return 0;
        }
    }
    private void EquipWeapon(int index)
    {
        attackPrefabs[(int)Projectile.SafetyBall].SetActive(false);
        if (OwnedWeapons == null || OwnedWeapons.Count == 0)
        {
            Debug.LogWarning("No owned weapons to equip.");
            return;
        }
        else if (index <= 9 && index >= 0 && OwnedWeapons.Contains((Weapon)index))
        {
            EquippedWeapon = (Weapon)index;
            charMat.SetTexture("_MainTex", WeaponColors[(int)EquippedWeapon]);
            Debug.Log($"Swapped to Weapon {EquippedWeapon}");
            return;
        }
        var cycleableWeapons = OwnedWeapons.Where(w => (int)w != 9).ToList();
        if (cycleableWeapons.Count <= 1)
        {
            Debug.LogWarning("No valid weapons to cycle through.");
            return;
        }
        else if (index < 0) 
        {
            int currentIndex = cycleableWeapons.IndexOf(EquippedWeapon);
            currentIndex = (currentIndex - 1 + cycleableWeapons.Count) % cycleableWeapons.Count;
            EquippedWeapon = cycleableWeapons[currentIndex];
        }
        else if (index > 9) 
        {
            int currentIndex = cycleableWeapons.IndexOf(EquippedWeapon);
            currentIndex = (currentIndex + 1) % cycleableWeapons.Count;
            EquippedWeapon = cycleableWeapons[currentIndex];
        }
        charMat.SetTexture("_MainTex", WeaponColors[(int)EquippedWeapon]);
        Debug.Log($"Swapped to Weapon {EquippedWeapon}");
    }
    private void OnShootStarted(InputAction.CallbackContext context)
    {
        bool isChargeableNonBuster = ChargeableWeapons.Contains(EquippedWeapon) && EquippedWeapon != Weapon.MegaBuster;
        bool isMegaBuster = EquippedWeapon == Weapon.MegaBuster;
        bool isMegamanOrRoll = cc.CurrentCharacter == CharControl.Character.Megaman || cc.CurrentCharacter == CharControl.Character.Roll;
        bool isProtomanWithValidUpgrade = cc.CurrentCharacter == CharControl.Character.Protoman && (cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge] || !cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster]);
        bool isBassWithExtraCharge = cc.CurrentCharacter == CharControl.Character.Bass && cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge];
        bool canChargeMegaBuster = isMegaBuster && (isMegamanOrRoll || isProtomanWithValidUpgrade || isBassWithExtraCharge);
        if (isChargeableNonBuster || canChargeMegaBuster){ChargingWeapon = true;chargingAudioSource.Play();}
        else if (cc.CurrentCharacter==CharControl.Character.Bass&&!cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge]&&EquippedWeapon==Weapon.MegaBuster){Invoke("rapidFire",FireTimer);}
        if (!anim.GetBool("Sliding"))
        {
            pointBuster=0.5f;
            ShootEquippedWeapon();
        }
    }
    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        if (BusterCharge>=HalfCharge&&ChargeableWeapons.Contains(EquippedWeapon)&&!anim.GetBool("Sliding")){pointBuster=0.5f;ShootEquippedWeapon();}
        StopCharge();
        CancelInvoke("rapidFire");
    }
    void rapidFire()
    {
        ShootEquippedWeapon();
        pointBuster=0.5f;
        Invoke("rapidFire",FireTimer);
    }
    void Update()
    {
        if (cc.CurrentCharacter==CharControl.Character.Bass&&EquippedWeapon==Weapon.MegaBuster&&pointBuster>0)
        {
            if (cc.moveInputY>0)
            {
                projectileSpawnRotate.transform.localRotation = Quaternion.Euler(0,0,cc.moveInputX!=0?45:90);
            }
            else if (cc.moveInputY<0)
            {
                projectileSpawnRotate.transform.localRotation = Quaternion.Euler(0,0,-45);
            }
            else {projectileSpawnRotate.transform.localRotation = Quaternion.Euler(0,0,0);}
        }
        facingRight = cc.facingRight;
        if (ChargingWeapon)
        {
            ChargeSpeed = cc.EquippedUpgrades[(int)CharControl.upgrades.QuickerCharge] ? 2 : 1;
            if (BusterCharge<OverCharge){BusterCharge+=Time.deltaTime*ChargeSpeed;BusterCharge=Mathf.Clamp(BusterCharge,0,cc.CurrentCharacter!=CharControl.Character.Bass?OverCharge:FullCharge);}
            if (BusterCharge>=OverCharge&&cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge]&&cc.CurrentCharacter!=CharControl.Character.Bass){charMat.SetColor("_OtlColor",new Color32(255,52,55,255));FullChargeEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);OverChargeEffect.Play();}
            else if (BusterCharge>=FullCharge){charMat.SetColor("_OtlColor",new Color32(151,255,255,255));ChargingEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);FullChargeEffect.Play();}
            else if (BusterCharge>=HalfCharge){charMat.SetColor("_OtlColor",new Color32(247,255,146,255));ChargingEffect.Play();}
            if (chargingAudioSource.isPlaying&&chargingAudioSource.time>2.5f){chargingAudioSource.volume=Mathf.Clamp((float)((chargingAudioSource.clip.length-chargingAudioSource.time)/(chargingAudioSource.clip.length-2.5f)),0,0.5f);}
            else {chargingAudioSource.volume=0.5f;}
        }
        else{StopCharge();}
        if (FireTimer>0){FireTimer-=Time.deltaTime; FireTimer=Mathf.Clamp(FireTimer,0,FireTimer);}
        if (pointBuster>0){pointBuster-=Time.deltaTime;pointBuster=Mathf.Clamp(pointBuster,0,pointBuster);}
        SetLayerWeight("Shoot",pointBuster>0.2f ? 1 : pointBuster/0.2f);
        if (!anim.GetBool("Jumping")&&attackPrefabs[(int)Projectile.SafetyBall].activeInHierarchy){Shoot(Projectile.SafetyBall);}
    }
    private void LateUpdate()
    {
        if (pointBuster>0.2f)
        {
            leftBuster.transform.LookAt(projectileSpawn.transform.position, Vector3.Cross(transform.right, projectileSpawn.transform.position - transform.position));
            leftBuster.transform.rotation *= Quaternion.Euler(90, 0, 0);
        }
        if (cc.CurrentCharacter==CharControl.Character.Bass&&!cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge]&&EquippedWeapon==Weapon.MegaBuster&&FireTimer>0&&cc.groundContact){cc.VelocityX=0;anim.SetBool("Running",false);}
    }
    void StopCharge()
    {
        chargingAudioSource.Stop();
        ChargingWeapon=false;
        BusterCharge=0;
        ChargingEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        FullChargeEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        OverChargeEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        charMat.SetColor("_OtlColor",new Color32(0,0,0,255));
    }
    void ShootEquippedWeapon()
    {
        switch (EquippedWeapon)
        {
            case Weapon.AnimalAdaptor:
                if (!armLaunched)
                {
                    if (BusterCharge>=FullCharge){Shoot(Projectile.AnimalAdaptor);BusterCharge=0;}
                    else {Shoot(Projectile.NoCharge);BusterCharge=0;}
                }
                break;
            case Weapon.SickleChain:
                if (WeaponEnergy[(int)Weapon.SickleChain]>0)
                {
                    if (BusterCharge>=HalfCharge){Shoot(Projectile.SickleChainLong);}
                    else {Shoot(Projectile.SickleChainShort);}
                }
                break;
            case Weapon.SafetyBall:
                if (attackPrefabs[(int)Projectile.SafetyBall].activeInHierarchy){Shoot(Projectile.BallBounce);}
                else if (WeaponEnergy[(int)Weapon.SafetyBall]>0){Shoot(Projectile.SafetyBall);}
                break;
            case Weapon.ResinGlob:
                if (WeaponEnergy[(int)Weapon.ResinGlob]>0){Shoot(Projectile.ResinGlob);}
                break;
            case Weapon.MegawattSurge:
                if (WeaponEnergy[(int)Weapon.MegawattSurge]>0){Shoot(Projectile.MegawattSurge);}
                break;
            case Weapon.Brickfall:
                if (WeaponEnergy[(int)Weapon.Brickfall]>0){Shoot(Projectile.Brickfall);}
                break;
            case Weapon.Firecracker:
                if (WeaponEnergy[(int)Weapon.Firecracker]>0)
                {
                    if (BusterCharge>=FullCharge){Shoot(Projectile.FirecrackerBarrage);}
                    else {Shoot(Projectile.Firecracker);}
                }
                break;
            case Weapon.WaterCannon:
                if (WeaponEnergy[(int)Weapon.WaterCannon]>0){Shoot(Projectile.WaterCannon);}
                break;
            case Weapon.CycloneStrike:
                if (attackPrefabs[(int)Projectile.CycloneStrike].activeInHierarchy){Shoot(Projectile.CycloneStrikeSlash);}
                else if (WeaponEnergy[(int)Weapon.CycloneStrike]>0){Shoot(Projectile.CycloneStrike);}
                break;
            default:
                if (cc.CurrentCharacter==CharControl.Character.Protoman&&cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster]){BusterCharge=Mathf.Max(FullCharge,BusterCharge);}
                if (BusterCharge>=OverCharge&&cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge]){Shoot(Projectile.OverCharge);BusterCharge=0;}
                else if (BusterCharge>=FullCharge){Shoot(Projectile.FullCharge);BusterCharge=0;}
                else if (BusterCharge>=HalfCharge){Shoot(Projectile.HalfCharge);BusterCharge=0;}
                else {Shoot(Projectile.NoCharge);BusterCharge=0;}
                break;
        }
    }
    public void EnergyChange(float amountChanged, Weapon weapon = Weapon.MegaBuster, bool EnergyBalancer = true)
    {
        if (amountChanged>0&&WeaponEnergy.Any(value => value < 28f)){chargingAudioSource.PlayOneShot(WeaponEnergyGainAudio);}
        if (amountChanged>0&&(WeaponEnergy[(int)weapon]>=28||weapon==Weapon.MegaBuster||weapon==Weapon.AnimalAdaptor)&&EnergyBalancer)
        {
            float[] values = WeaponEnergy;
            int[] sortedIndexes = values
                .Select((value, index) => new { value, index })  // Pair value with its index
                .OrderBy(pair => pair.value)                    // Sort by value
                .Select(pair => pair.index)                     // Get the indexes
                .ToArray();
            foreach (int index in sortedIndexes)
            {
                if (OwnedWeapons.Contains((Weapon)index)&&weapon!=Weapon.MegaBuster&&weapon!=Weapon.AnimalAdaptor)
                {
                    WeaponEnergy[index]=Mathf.Clamp(WeaponEnergy[index]+amountChanged,0,28);
                    break;
                }
            }
        }
        else if (weapon!=Weapon.MegaBuster)
        {
            WeaponEnergy[(int)weapon]=Mathf.Clamp(WeaponEnergy[(int)weapon]+amountChanged,0,28);
        }
    }
    void Shoot(Projectile type)
    {
        if (projectilesAndAttacks.Count>0){projectilesAndAttacks.RemoveAll(item => item == null);}
        switch (type)
        {
            case Projectile.SickleChainShort:
                attackPrefabs[(int)Projectile.SickleChainShort].SetActive(true);
                EnergyChange(-2, Weapon.SickleChain);
                break;
            case Projectile.SickleChainLong:
                attackPrefabs[(int)Projectile.SickleChainShort].SetActive(false);
                if (FireTimer==0)
                {
                    if (facingRight)
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.SickleChainLong],projectileSpawn.transform.position,Quaternion.identity);
                        projectilesAndAttacks.Add(bullet);
                    }
                    else
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.SickleChainLong],projectileSpawn.transform.position,Quaternion.identity);
                        bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);
                        projectilesAndAttacks.Add(bullet);
                    }
                    FireTimer=ShotTiming;
                    EnergyChange(-3, Weapon.SickleChain);
                }
                break;
            case Projectile.SafetyBall:
                attackPrefabs[(int)Projectile.BallBounce].gameObject.GetAny<Renderer>().material.SetTexture("_MainTex",Ball);
                attackPrefabs[(int)Projectile.SafetyBall].GetAny<SafetyBallScript>().Attack=false;
                attackPrefabs[(int)Projectile.SafetyBall].SetActive(true);
                EnergyChange(-6, Weapon.SafetyBall);
                break;
            case Projectile.BallBounce:
                attackPrefabs[(int)Projectile.BallBounce].gameObject.GetAny<Renderer>().material.SetTexture("_MainTex",BallAttack);
                attackPrefabs[(int)Projectile.SafetyBall].GetAny<SafetyBallScript>().Attack=true;
                break;
            case Projectile.ResinGlob:
                if (FireTimer==0)
                {
                    if (facingRight)
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.ResinGlob],projectileSpawn.transform.position,Quaternion.identity);
                        projectilesAndAttacks.Add(bullet);
                    }
                    else
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.ResinGlob],projectileSpawn.transform.position,Quaternion.identity);
                        bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);
                        projectilesAndAttacks.Add(bullet);
                    }
                    FireTimer=ShotTiming;
                    EnergyChange(-4, Weapon.ResinGlob);
                }
                break;
            case Projectile.MegawattSurge:
                if (FireTimer==0)
                {
                    if (facingRight)
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.MegawattSurge],projectileSpawn.transform.position,Quaternion.identity);
                        projectilesAndAttacks.Add(bullet);
                    }
                    else
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.MegawattSurge],projectileSpawn.transform.position,Quaternion.identity);
                        bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);
                        projectilesAndAttacks.Add(bullet);
                    }
                    FireTimer=ShotTiming;
                    EnergyChange(-3, Weapon.MegawattSurge);
                }
                break;
            case Projectile.Brickfall:
                if (FireTimer==0)
                {
                    int maxBrickfalls = 5;
                    var brickfallObjects = projectilesAndAttacks
                        .Where(obj => obj != null && obj.name.StartsWith(attackPrefabs[(int)Projectile.Brickfall].name))
                        .ToList();
                    if (brickfallObjects.Count >= maxBrickfalls)
                    {
                        int toRemove = brickfallObjects.Count - maxBrickfalls + 1;
                        for (int i = 0; i < toRemove; i++)
                        {
                            Destroy(brickfallObjects[i]);
                        }
                    }
                    GameObject brick = Instantiate(attackPrefabs[(int)Projectile.Brickfall],cc.groundContact?projectileSpawn.transform.position:new Vector2(transform.position.x,transform.position.y-0.5f),Quaternion.identity);
                    projectilesAndAttacks.Add(brick);
                    EnergyChange(-3, Weapon.Brickfall);
                }
                FireTimer=ShotTiming;
                break;
            case Projectile.Firecracker:
                if (projectilesAndAttacks.Count(obj => obj != null && obj.name.StartsWith(attackPrefabs[(int)Projectile.Firecracker].name))<1)
                {
                    GameObject bomb = Instantiate(attackPrefabs[(int)Projectile.Firecracker],projectileSpawn.transform.position,Quaternion.Euler(0,0,facingRight?0:180));
                    projectilesAndAttacks.Add(bomb);
                    FireTimer=ShotTiming;
                    EnergyChange(-3, Weapon.Firecracker);
                }
                break;
            case Projectile.FirecrackerBarrage:
                {
                    int angle = -90;
                    for (int i=0; i<7;i++)
                    {
                        angle+=45;
                        GameObject bomb = Instantiate(attackPrefabs[(int)Projectile.Firecracker],projectileSpawn.transform.position,Quaternion.Euler(0,0,angle));
                        bomb.GetAny<AudioSource>().volume*=1f/7f;
                        projectilesAndAttacks.Add(bomb);
                    }
                    FireTimer=ShotTiming;
                    EnergyChange(-6, Weapon.Firecracker);
                }
                break;
            case Projectile.WaterCannon:
                if (FireTimer<=0)
                {
                    GameObject WaterBurst = Instantiate(attackPrefabs[(int)Projectile.WaterCannon],projectileSpawn.transform.position,Quaternion.Euler(0,0,facingRight?0:180));
                    projectilesAndAttacks.Add(WaterBurst);
                    FireTimer=0.5f;
                    EnergyChange(-4, Weapon.WaterCannon);
                }
                break;
            case Projectile.CycloneStrike:
                attackPrefabs[(int)Projectile.CycloneStrike].SetActive(true);
                EnergyChange(-7, Weapon.CycloneStrike);
                break;
            case Projectile.CycloneStrikeSlash:
                attackPrefabs[(int)Projectile.CycloneStrike].GetAny<CycloneStrikeBehaviour>().SlashAttack();
                break;
            case Projectile.OverCharge:
                if (facingRight)
                {
                    GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.OverCharge],projectileSpawn.transform.position,Quaternion.identity);
                    projectilesAndAttacks.Add(bullet);
                }
                else
                {
                    GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.OverCharge],projectileSpawn.transform.position,Quaternion.identity);
                    bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);
                    projectilesAndAttacks.Add(bullet);
                }
                if (cc.CurrentCharacter==CharControl.Character.Protoman){cc.HealthChange(0);}
                FireTimer=ShotTiming;
                break;
            case Projectile.FullCharge:
                if (cc.CurrentCharacter==CharControl.Character.Bass)
                {
                    GameObject bullet = Instantiate(BassBulletVariants[3],projectileSpawn.transform.position,Quaternion.identity);
                    if (cc.moveInputY>0)
                    {
                        bullet.transform.rotation = Quaternion.Euler(0,0,cc.moveInputX!=0?(facingRight?45:135):90);
                    }
                    else if (cc.moveInputY<0)
                    {
                        bullet.transform.rotation = Quaternion.Euler(0,0,facingRight?-45:-135);
                    }
                    else if (!facingRight)
                    {
                        bullet.transform.rotation = Quaternion.Euler(0,0,180);
                    }
                    var behaviour = bullet.GetAny<DirectionalBulletBehaviour>();
                    behaviour.BreakFromObstacle=!cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster];
                    behaviour.Pierces=!cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster];
                    if (cc.EquippedUpgrades[(int)CharControl.upgrades.QuickerCharge]) {behaviour.damageChange(1f);}
                    projectilesAndAttacks.Add(bullet);
                    FireTimer=ShotTiming;
                }
                else if (cc.CurrentCharacter==CharControl.Character.Roll)
                {
                    FireTimer=ShotTiming;
                }
                else
                {
                    if ((FireTimer==0||!(cc.CurrentCharacter==CharControl.Character.Protoman&&cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster]))&&projectilesAndAttacks.Count(obj => obj != null && obj.name.StartsWith(attackPrefabs[(int)Projectile.FullCharge].name))<2)
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.FullCharge],projectileSpawn.transform.position,Quaternion.identity);
                        if (!facingRight){bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);}
                        projectilesAndAttacks.Add(bullet);
                        FireTimer=ShotTiming;
                    }
                }
                break;
            case Projectile.HalfCharge:
                if (cc.CurrentCharacter==CharControl.Character.Bass)
                {
                    GameObject bullet = Instantiate(BassBulletVariants[2],projectileSpawn.transform.position,Quaternion.identity);
                    if (cc.moveInputY>0)
                    {
                        bullet.transform.rotation = Quaternion.Euler(0,0,cc.moveInputX!=0?(facingRight?45:135):90);
                    }
                    else if (cc.moveInputY<0)
                    {
                        bullet.transform.rotation = Quaternion.Euler(0,0,facingRight?-45:-135);
                    }
                    else if (!facingRight)
                    {
                        bullet.transform.rotation = Quaternion.Euler(0,0,180);
                    }
                    var behaviour = bullet.GetAny<DirectionalBulletBehaviour>();
                    behaviour.BreakFromObstacle=!cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster];
                    behaviour.Pierces=!cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster];
                    if (cc.EquippedUpgrades[(int)CharControl.upgrades.QuickerCharge]) {behaviour.damageChange(0.5f);}
                    projectilesAndAttacks.Add(bullet);
                    FireTimer=ShotTiming;
                }
                else if (cc.CurrentCharacter!=CharControl.Character.Roll)
                {
                    GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.HalfCharge],projectileSpawn.transform.position,Quaternion.identity);
                    if (!facingRight){bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);}
                    projectilesAndAttacks.Add(bullet);
                }
                break;
            case Projectile.NoCharge:
                if (FireTimer==0)
                {
                    if (cc.CurrentCharacter==CharControl.Character.Bass)
                    {
                        if (projectilesAndAttacks.Count(obj => obj != null && obj.name.StartsWith(BassBulletVariants[cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge]?1:0].name))<(cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge]?2:5))
                        {   
                            GameObject bullet = cc.EquippedUpgrades[(int)CharControl.upgrades.ExtraCharge]?Instantiate(BassBulletVariants[1],projectileSpawn.transform.position,Quaternion.identity):Instantiate(BassBulletVariants[0],projectileSpawn.transform.position,Quaternion.identity);
                            if (cc.moveInputY>0)
                            {
                                bullet.transform.rotation = Quaternion.Euler(0,0,cc.moveInputX!=0?(facingRight?45:135):90);
                            }
                            else if (cc.moveInputY<0)
                            {
                                bullet.transform.rotation = Quaternion.Euler(0,0,facingRight?-45:-135);
                            }
                            else if (!facingRight)
                            {
                                bullet.transform.rotation = Quaternion.Euler(0,0,180);
                            }
                            var behaviour = bullet.GetAny<DirectionalBulletBehaviour>();
                            behaviour.BreakFromObstacle=!cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster];
                            behaviour.Pierces=!cc.EquippedUpgrades[(int)CharControl.upgrades.BeamBuster];
                            if (cc.EquippedUpgrades[(int)CharControl.upgrades.QuickerCharge]) {behaviour.damageChange(0.5f);}
                            projectilesAndAttacks.Add(bullet);
                            FireTimer=ShotTiming;
                        }
                    }
                    else if (cc.CurrentCharacter==CharControl.Character.Roll)
                    {
                        FireTimer=ShotTiming;
                    }
                    else
                    {
                        if (projectilesAndAttacks.Count(obj => obj != null && obj.name.StartsWith(attackPrefabs[(int)Projectile.NoCharge].name))<3)
                        {   
                            GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.NoCharge],projectileSpawn.transform.position,Quaternion.identity);
                            if (!facingRight){bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);}
                            projectilesAndAttacks.Add(bullet);
                            FireTimer=ShotTiming;
                        }
                    }
                }
                break;
            case Projectile.AnimalAdaptor:
                if (!armLaunched)
                {
                    GameObject rocketFist = Instantiate(attackPrefabs[(int)Projectile.AnimalAdaptor],projectileSpawn.transform.position,facingRight?Quaternion.identity:Quaternion.Euler(0,0,180));
                    if (!facingRight){rocketFist.transform.localScale=new Vector3(rocketFist.transform.localScale.x,rocketFist.transform.localScale.y*-1,rocketFist.transform.localScale.z);}
                    rocketFist.GetAny<HomingBullet>().returnPoint=transform;
                    projectilesAndAttacks.Add(rocketFist);
                    FireTimer=ShotTiming;
                    armLaunched=true;
                }
                break;
            default:
                break;
        }
        ChargingEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        FullChargeEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        OverChargeEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    void SetLayerWeight(string layerName, float weight)
    {
        int layerIndex = anim.GetLayerIndex(layerName);
        if (layerIndex != -1) // Ensure the layer exists
        {
            anim.SetLayerWeight(layerIndex, weight);
        }
    }
}
