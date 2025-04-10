using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharControl))]
public class Buster : MonoBehaviour
{
    [SerializeField] float FullCharge,ShotTiming;
    [SerializeField] GameObject projectileSpawn, projectileSpawnRotate,leftBuster;
    [SerializeField] List<GameObject> attackPrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> BassBulletVariants = new List<GameObject>();
    [SerializeField] ParticleSystem ChargingEffect,FullChargeEffect,OverChargeEffect;
    Animator anim;
    public enum upgrades {Armor,ShockAbsorber,AutoRecover, EnergySaver,SuperRecover,PickupFinder, ExtraCharge,QuickerCharge,BeamBuster, SuperSlide,Sprinter,WallKick}
    private DefaultControls playerInputActions;
    private bool facingRight=true,ChargingWeapon;
    float BusterCharge,HalfCharge,OverCharge,FireTimer,pointBuster=0,ChargeSpeed;
    private List<GameObject> projectilesAndAttacks = new List<GameObject>();
    enum Projectile {NoCharge, HalfCharge, FullCharge, OverCharge, SickleChainShort, SickleChainLong, SafetyBall, BallBounce, SlagShot, SlagHammer, MegawattSurge, Brickfall, IfritBurstSmall, IfritBurstHuge, WaterHose, CycloneStrike, AnimalFriend}
    public enum Weapon {MegaBuster,SickleChain,SafetyBall,SlagShot,MegawattSurge,Brickfall,IfritBurst,WaterHose,CycloneStrike,AnimalFriend};
    private List<Weapon> ChargeableWeapons = new List<Weapon>();
    public List<Weapon> OwnedWeapons = new List<Weapon>();
    CharControl cc;
    [SerializeField] Weapon EquippedWeapon;
    [SerializeField] Texture2D Ball, BallAttack;
    [SerializeField] Material charMat;
    [SerializeField] AudioSource chargingAudioSource;
    private void Awake()
    {
        playerInputActions = new DefaultControls();
        playerInputActions.Enable();
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        HalfCharge=FullCharge/2;
        OverCharge=FullCharge*1.5f;
        ChargeableWeapons.Add(Weapon.MegaBuster);
        ChargeableWeapons.Add(Weapon.SlagShot);
        ChargeableWeapons.Add(Weapon.IfritBurst);
        cc = GetComponent<CharControl>();
        //UpdateInventory();
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
        Debug.Log($"Swapped to Weapon {EquippedWeapon}");
    }
    private void OnShootStarted(InputAction.CallbackContext context)
    {
        if (ChargeableWeapons.Contains(EquippedWeapon)&&(cc.CurrentCharacter!=CharControl.Character.Bass||cc.EquippedUpgrades[(int)upgrades.ExtraCharge])){ChargingWeapon=true;chargingAudioSource.Play();}
        else if (cc.CurrentCharacter==CharControl.Character.Bass&&!cc.EquippedUpgrades[(int)upgrades.ExtraCharge]&&EquippedWeapon==Weapon.MegaBuster){Invoke("rapidFire",FireTimer);}
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
            ChargeSpeed = cc.EquippedUpgrades[(int)upgrades.QuickerCharge] ? 2 : 1;
            if (BusterCharge<OverCharge){BusterCharge+=Time.deltaTime*ChargeSpeed;BusterCharge=Mathf.Clamp(BusterCharge,0,cc.CurrentCharacter!=CharControl.Character.Bass?OverCharge:FullCharge);}
            if (BusterCharge>=OverCharge&&cc.EquippedUpgrades[(int)upgrades.ExtraCharge]&&cc.CurrentCharacter!=CharControl.Character.Bass){charMat.SetColor("_OtlColor",new Color32(255,52,55,255));FullChargeEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);OverChargeEffect.Play();}
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
        if (cc.CurrentCharacter==CharControl.Character.Bass&&!cc.EquippedUpgrades[(int)upgrades.ExtraCharge]&&EquippedWeapon==Weapon.MegaBuster&&FireTimer>0&&cc.grounded){cc.VelocityX=0;anim.SetBool("Running",false);}
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
        if (cc.EquippedUpgrades[(int)upgrades.QuickerCharge]){ChargeSpeed=2;}
        else {ChargeSpeed=1;}
        switch (EquippedWeapon)
        {
            case Weapon.AnimalFriend:
                Shoot(Projectile.AnimalFriend);
                break;
            case Weapon.SickleChain:
                if (BusterCharge>=HalfCharge){Shoot(Projectile.SickleChainLong);}
                else {Shoot(Projectile.SickleChainShort);}
                break;
            case Weapon.SafetyBall:
                if (attackPrefabs[(int)Projectile.SafetyBall].activeInHierarchy){Shoot(Projectile.BallBounce);}
                else {Shoot(Projectile.SafetyBall);}
                break;
            case Weapon.SlagShot:
                if (BusterCharge==HalfCharge){Shoot(Projectile.SlagHammer);}
                else {Shoot(Projectile.SlagShot);}
                break;
            case Weapon.MegawattSurge:
                Shoot(Projectile.MegawattSurge);
                break;
            case Weapon.Brickfall:
                Shoot(Projectile.Brickfall);
                break;
            case Weapon.IfritBurst:
                if (BusterCharge==FullCharge){Shoot(Projectile.IfritBurstHuge);}
                else {Shoot(Projectile.IfritBurstSmall);}
                break;
            case Weapon.WaterHose:
                Shoot(Projectile.WaterHose);
                break;
            case Weapon.CycloneStrike:
                Shoot(Projectile.CycloneStrike);
                break;
            default:
                if (BusterCharge>=OverCharge&&cc.EquippedUpgrades[(int)upgrades.ExtraCharge]){Shoot(Projectile.OverCharge);BusterCharge=0;}
                else if (BusterCharge>=FullCharge){Shoot(Projectile.FullCharge);BusterCharge=0;}
                else if (BusterCharge>=HalfCharge){Shoot(Projectile.HalfCharge);BusterCharge=0;}
                else {Shoot(Projectile.NoCharge);BusterCharge=0;}
                break;
        }
    }
    void Shoot(Projectile type)
    {
        if (projectilesAndAttacks.Count>0){projectilesAndAttacks.RemoveAll(item => item == null);}
        switch (type)
        {
            case Projectile.SickleChainShort:
                attackPrefabs[(int)Projectile.SickleChainShort].SetActive(true);
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
                }
                break;
            case Projectile.SafetyBall:
                attackPrefabs[(int)Projectile.BallBounce].gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex",Ball);
                attackPrefabs[(int)Projectile.SafetyBall].GetComponent<SafetyBallScript>().Attack=false;
                attackPrefabs[(int)Projectile.SafetyBall].SetActive(true);
                break;
            case Projectile.BallBounce:
                attackPrefabs[(int)Projectile.BallBounce].gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex",BallAttack);
                attackPrefabs[(int)Projectile.SafetyBall].GetComponent<SafetyBallScript>().Attack=true;
                break;
            case Projectile.SlagShot:
                if (FireTimer==0)
                {
                    if (facingRight)
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.SlagShot],projectileSpawn.transform.position,Quaternion.identity);
                        projectilesAndAttacks.Add(bullet);
                    }
                    else
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.SlagShot],projectileSpawn.transform.position,Quaternion.identity);
                        bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);
                        projectilesAndAttacks.Add(bullet);
                    }
                    FireTimer=ShotTiming;
                }
                break;
            case Projectile.SlagHammer:
                if (facingRight)
                {
                    GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.SlagHammer],projectileSpawn.transform.position,Quaternion.identity);
                    projectilesAndAttacks.Add(bullet);
                }
                else
                {
                    GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.SlagHammer],projectileSpawn.transform.position,Quaternion.identity);
                    bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);
                    projectilesAndAttacks.Add(bullet);
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
                }
                break;
            case Projectile.Brickfall:
                if (FireTimer==0)
                {
                    if (facingRight)
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.Brickfall],projectileSpawn.transform.position,Quaternion.identity);
                        projectilesAndAttacks.Add(bullet);
                    }
                    else
                    {
                        GameObject bullet = Instantiate(attackPrefabs[(int)Projectile.Brickfall],projectileSpawn.transform.position,Quaternion.identity);
                        bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);
                        projectilesAndAttacks.Add(bullet);
                    }
                }
                FireTimer=ShotTiming;
                break;
            case Projectile.IfritBurstSmall:
                if (FireTimer==0)
                {
                    GameObject bombs = Instantiate(attackPrefabs[(int)Projectile.IfritBurstSmall],projectileSpawn.transform.position,Quaternion.identity);
                    projectilesAndAttacks.Add(bombs);
                    FireTimer=ShotTiming;
                }
                break;
            case Projectile.IfritBurstHuge:
                attackPrefabs[(int)Projectile.IfritBurstHuge].SetActive(true);
                break;
            case Projectile.WaterHose:
                attackPrefabs[(int)Projectile.WaterHose].SetActive(true);
                break;
            case Projectile.CycloneStrike:
                attackPrefabs[(int)Projectile.CycloneStrike].SetActive(true);
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
                break;
            case Projectile.FullCharge:
                {GameObject bullet = (cc.CurrentCharacter==CharControl.Character.Bass&&cc.EquippedUpgrades[(int)upgrades.ExtraCharge])?Instantiate(BassBulletVariants[3],projectileSpawn.transform.position,Quaternion.identity):Instantiate(attackPrefabs[(int)Projectile.FullCharge],projectileSpawn.transform.position,Quaternion.identity);
                if (!facingRight&&cc.CurrentCharacter!=CharControl.Character.Bass){bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);}
                else
                {
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
                    bullet.GetComponent<SolarBulletBehaviour>().BreakFromObstacle=!cc.EquippedUpgrades[(int)upgrades.BeamBuster];
                }
                projectilesAndAttacks.Add(bullet);
                FireTimer=ShotTiming;}
                break;
            case Projectile.HalfCharge:
                {GameObject bullet = (cc.CurrentCharacter==CharControl.Character.Bass&&cc.EquippedUpgrades[(int)upgrades.ExtraCharge])?Instantiate(BassBulletVariants[2],projectileSpawn.transform.position,Quaternion.identity):Instantiate(attackPrefabs[(int)Projectile.HalfCharge],projectileSpawn.transform.position,Quaternion.identity);
                if (!facingRight&&cc.CurrentCharacter!=CharControl.Character.Bass){bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);}
                else
                {
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
                    bullet.GetComponent<SolarBulletBehaviour>().BreakFromObstacle=!cc.EquippedUpgrades[(int)upgrades.BeamBuster];
                }
                projectilesAndAttacks.Add(bullet);
                FireTimer=ShotTiming;}
                break;
            case Projectile.NoCharge:
                if (FireTimer==0)
                {
                    if (projectilesAndAttacks.Count(obj => obj != null && obj.name.StartsWith(attackPrefabs[(int)Projectile.NoCharge].name))<(cc.CurrentCharacter==CharControl.Character.Bass?(cc.EquippedUpgrades[(int)upgrades.ExtraCharge]?2:5):3))
                    {   
                        GameObject bullet = (cc.CurrentCharacter==CharControl.Character.Bass)?(cc.EquippedUpgrades[(int)upgrades.ExtraCharge]?Instantiate(BassBulletVariants[1],projectileSpawn.transform.position,Quaternion.identity):Instantiate(BassBulletVariants[0],projectileSpawn.transform.position,Quaternion.identity)):Instantiate(attackPrefabs[(int)Projectile.NoCharge],projectileSpawn.transform.position,Quaternion.identity);
                        if (!facingRight&&cc.CurrentCharacter!=CharControl.Character.Bass){bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*-1,bullet.transform.localScale.y,bullet.transform.localScale.z);}
                        else
                        {
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
                            bullet.GetComponent<SolarBulletBehaviour>().BreakFromObstacle=!cc.EquippedUpgrades[(int)upgrades.BeamBuster];
                        }
                        projectilesAndAttacks.Add(bullet);
                        FireTimer=ShotTiming;
                    }
                }
                break;
            case Projectile.AnimalFriend:
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
    void UpdateInventory()
    {
        for (int i = 1; i <= 9; i++)
        {
            GetComponent<CharControl>().HealthChange(0);
            if (PlayerPrefs.HasKey(((Weapon)i).ToString())&&!OwnedWeapons.Contains((Weapon)i))
            {
                OwnedWeapons.Insert(i,(Weapon)i);
            }
            else if (!PlayerPrefs.HasKey(((Weapon)i).ToString())&&OwnedWeapons.Contains((Weapon)i))
            {
                OwnedWeapons.Remove((Weapon)i);
            }
        }
    }
}
