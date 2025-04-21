using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Buster))]
public class CharControl : MonoBehaviour
{
    [SerializeField] public float runSpeed,slideSpeed,slideTime,jumpForce,gravity,gravityInWater,jumpArcStart,maxVerticalVelocity;
    [SerializeField] GameObject StandingCollision,SlidingCollision,DashingCollision,RollingCollision,Camera,cycloneStrike;
    [SerializeField] public bool DashComboInput,SlideComboInput,inWater;
    public enum upgrades {Armor,ShockAbsorber,AutoRecover, EnergySaver,SuperRecover,PickupFinder, ExtraCharge,QuickerCharge,BeamBuster, SuperSlide,Sprinter,WallKick}
    [SerializeField] public List<upgrades> OwnedUpgrades = new List<upgrades>();
    [SerializeField] public List<bool> EquippedUpgrades = new List<bool>();
    public enum Character {Megaman, Protoman, Bass, Roll}
    [SerializeField] public Character CurrentCharacter;
    private Character SwappedFromCharacter;
    [SerializeField] Image healthBar;
    [SerializeField] List<SurfaceDetectionViaTrigger> 
    FrontDetection = new List<SurfaceDetectionViaTrigger>(),
    BackDetection = new List<SurfaceDetectionViaTrigger>(),
    CeilingDetection = new List<SurfaceDetectionViaTrigger>(),
    FloorDetection = new List<SurfaceDetectionViaTrigger>();
    Animator anim;
    [SerializeField] AnimationClip Sliding,Rolling,Dashing;
    private AnimatorOverrideController overrideController;
    public Rigidbody2D rb;
    private bool isHit=false,slideJumping=false,slidingToTheRight;
    public bool dead,facingRight=true,isSliding=false,groundContact,ceilingContact,frontContact,backContact,velocityOverride;
    private float slideTimer,healthPoints=28,timeSinceJump,invincibiltyTimer,currentMotion;
    public float VelocityY,VelocityX,moveInputX=0,moveInputY=0;
    private DefaultControls playerInputActions;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip jumpSFX, slideSFX, landSFX, hurtSFX, dieSFX, healSFX;
    private void Awake()
    {
        playerInputActions = new DefaultControls();
        playerInputActions.Enable();
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        SwappedFromCharacter=CurrentCharacter;
    }
    public void SetMovesetAnimation(AnimationClip newClip, HashSet<AnimationClip> validClips)
    {
        // First-time setup of the override controller
        if (overrideController == null)
        {
            overrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
            anim.runtimeAnimatorController = overrideController;
        }

        // Get currently active clips and transitions
        AnimatorClipInfo[] currentClips = anim.GetCurrentAnimatorClipInfo(0);
        AnimatorClipInfo[] nextClips = anim.GetNextAnimatorClipInfo(0);

        // Collect all clips currently in use (playing or transitioning)
        HashSet<AnimationClip> clipsInUse = new HashSet<AnimationClip>();
        foreach (var clip in currentClips) clipsInUse.Add(clip.clip);
        foreach (var clip in nextClips) clipsInUse.Add(clip.clip);

        // Only override if the current override target is in the valid list AND not playing or transitioning
        foreach (var originalClip in validClips)
        {
            if (!clipsInUse.Contains(originalClip))
            {
                overrideController[originalClip] = newClip;
            }
        }
    }
    private void OnEnable()
    {
        playerInputActions.Controls.MoveHorizontal.started += OnMoveStarted;  // Subscribe to the Move action
        playerInputActions.Controls.MoveHorizontal.performed += OnHorizontalPerformed;
        playerInputActions.Controls.MoveVertical.performed += OnVerticalPerformed;
        playerInputActions.Controls.MoveHorizontal.canceled += OnHorizontalCanceled;
        playerInputActions.Controls.MoveVertical.canceled += OnVerticalCanceled;
        playerInputActions.Controls.Jump.started += OnJumpStarted;
        playerInputActions.Controls.Jump.canceled += OnJumpCanceled;
        if (DashComboInput){playerInputActions.Controls.Dash.started += OnSlide;}
        playerInputActions.Controls.Slide.started += OnSlide;
    }
    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        playerInputActions.Controls.MoveHorizontal.started -= OnMoveStarted;
        playerInputActions.Controls.MoveHorizontal.performed -= OnHorizontalPerformed;
        playerInputActions.Controls.MoveVertical.performed -= OnVerticalPerformed;
        playerInputActions.Controls.MoveHorizontal.canceled -= OnHorizontalCanceled;
        playerInputActions.Controls.MoveVertical.canceled -= OnVerticalCanceled;
        playerInputActions.Controls.Jump.started -= OnJumpStarted;
        playerInputActions.Controls.Jump.canceled -= OnJumpCanceled;
        playerInputActions.Controls.Dash.started -= OnSlide;
        playerInputActions.Controls.Slide.started -= OnSlide;
    }
    private void OnDestroy()
    {
        playerInputActions.Controls.Disable();
    }
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        anim.SetBool("Running",true);
    }
    private void OnHorizontalPerformed(InputAction.CallbackContext context)
    {
        moveInputX = context.ReadValue<float>();
    }
    private void OnVerticalPerformed(InputAction.CallbackContext context)
    {
        moveInputY = context.ReadValue<float>();
    }
    private void OnMove()
    {
        if (moveInputX!=0&&!anim.GetBool("Hit"))
        {
            facingRight=moveInputX>0;
            if (slideJumping && (EquippedUpgrades[(int)upgrades.SuperSlide]||CurrentCharacter==Character.Bass)) 
            {
                currentMotion = slideSpeed + (EquippedUpgrades[(int)upgrades.SuperSlide]&&CurrentCharacter==Character.Bass?3:1);
            }
            else {currentMotion = EquippedUpgrades[(int)upgrades.Sprinter] ? runSpeed + 2 : runSpeed;}
        }
        if (facingRight){transform.localScale=new Vector3(1,1,1);}
        else {transform.localScale=new Vector3(-1,1,1);}
        anim.SetFloat("Run Speed",currentMotion/2);
    }
    private void OnHorizontalCanceled(InputAction.CallbackContext context)
    {
        if (!anim.GetBool("Hit")&&!isSliding){currentMotion=0;}
        anim.SetBool("Running",false);
        moveInputX=0;
    }
    private void OnVerticalCanceled(InputAction.CallbackContext context)
    {
        moveInputY=0;
    }
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (isSliding&&!ceilingContact)
        {
            slideJumping = true;
            timeSinceJump = 0f;
            VelocityY = jumpForce;
            audioSource.PlayOneShot(jumpSFX);
        }
        else if (SlideComboInput&&moveInputY<0&&groundContact&&!isSliding)
        {
            CollisionDetectionUpdate(CurrentCharacter,true);
            if (!frontContact)
            {   
                isSliding = true;
                audioSource.PlayOneShot(slideSFX);
                slideTimer=0;
                slidingToTheRight = facingRight;
            }
        }
        else if ((!SlideComboInput||moveInputY>=0)&&groundContact)
        {
            VelocityY = jumpForce;
            audioSource.PlayOneShot(jumpSFX);
        }
        else if (EquippedUpgrades[(int)upgrades.WallKick]&&!groundContact)
        {
            switch (CurrentCharacter)
            {
                case Character.Megaman:
                if (frontContact)
                {
                    VelocityY = jumpForce*0.9f;
                    currentMotion=-10;
                    audioSource.PlayOneShot(jumpSFX);
                }
                return;
                case Character.Bass:
                VelocityY = jumpForce;
                audioSource.PlayOneShot(jumpSFX);
                return;
                case Character.Roll:
                return;
                case Character.Protoman:
                VelocityY=0.8f;
                return;
                default:
                return;
            }
        }
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (VelocityY > jumpArcStart)
        {
            VelocityY = jumpArcStart;
            Debug.Log("Jump Stopped");
        }
    }
    private void OnSlide(InputAction.CallbackContext context)
    {
        CollisionDetectionUpdate(CurrentCharacter,true);
        if (groundContact&&!isSliding&&!frontContact)
        {
            isSliding = true;
            audioSource.PlayOneShot(slideSFX);
            slideTimer=0;
            slidingToTheRight = facingRight;
        }
    }
    private void Update()
    {
        OnMove();
        fall(!groundContact);
        slide();
        motion();
        if (CurrentCharacter!=Character.Protoman&&EquippedUpgrades[(int)upgrades.AutoRecover]&&healthPoints>=0&&healthPoints<28){HealthChange(Time.deltaTime/5);}
        if (isHit&&invincibiltyTimer<1){invincibiltyTimer+=Time.deltaTime; if (invincibiltyTimer > 0.5f){anim.SetBool("Hit",false);}}
        else {isHit=false;invincibiltyTimer=0;}
    }
    private void FixedUpdate()
    {
        if (!velocityOverride){rb.velocity = new Vector2(VelocityX,VelocityY);}
        CollisionDetectionUpdate(CurrentCharacter,isSliding);
    }
    void CollisionDetectionUpdate(Character character, bool sliding)
    {
        ceilingContact = CeilingDetection[!sliding?0:(character==Character.Roll?2:1)].InContact;
        groundContact = FloorDetection[!sliding||character==Character.Roll?0:1].InContact;
        frontContact = FrontDetection[!sliding?0:(character==Character.Bass?2:1)].InContact;
        backContact = BackDetection[!sliding?0:(character==Character.Roll?2:1)].InContact;
    }
    void LateUpdate()
    {
        if (Camera.transform.lossyScale.x<0)Camera.transform.localScale=new Vector3(Camera.transform.localScale.x*-1,Camera.transform.localScale.y,Camera.transform.localScale.z);
        if (SwappedFromCharacter!=CurrentCharacter)
        {
            if (CurrentCharacter==Character.Roll){SetMovesetAnimation(Rolling,new HashSet<AnimationClip> {Dashing,Sliding});}
            else if (CurrentCharacter==Character.Bass){SetMovesetAnimation(Dashing,new HashSet<AnimationClip> {Rolling,Sliding});}
            else {SetMovesetAnimation(Sliding,new HashSet<AnimationClip> {Dashing,Rolling});}
            SwappedFromCharacter=CurrentCharacter;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        inWater=collision.gameObject.layer == LayerMask.NameToLayer("Water");
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        inWater=collision.gameObject.layer != LayerMask.NameToLayer("Water")&&inWater;
    }
    private void fall(bool airborne)
    {
        if (anim.GetBool("Jumping")==true&&!airborne)
        {
            if (!GetComponentInChildren<SafetyBallScript>()){audioSource.PlayOneShot(landSFX);}
            VelocityY=0;
        }
        anim.SetBool("Jumping", airborne);
        if (!airborne) {return;}
        float accelerationY = inWater ? gravityInWater : gravity;
        if (ceilingContact&&VelocityY>jumpArcStart){VelocityY=jumpArcStart;}
        VelocityY -= Time.deltaTime * accelerationY;
        VelocityY = Mathf.Clamp(VelocityY, -maxVerticalVelocity, maxVerticalVelocity);
    }
    private void slide()
    {
        if (isSliding&&ceilingContact&&(slideTimer >= slideTime-0.1f)){slideTimer=Mathf.Min(slideTimer,slideTime-0.1f);}
        if (isSliding&&(slideTimer >= slideTime||slidingToTheRight!=facingRight||frontContact||!groundContact)&&!ceilingContact)
        {
            isSliding=false;
            currentMotion=0;
        }
        else if (isSliding&&!groundContact&&ceilingContact)
        {
            isSliding=false;
            currentMotion=0;
            rb.transform.position = new Vector2(rb.transform.position.x,rb.transform.position.y-0.6f);
        }
        else if (isSliding)
        {
            slideTimer+=Time.deltaTime;
        }
        anim.SetBool("Sliding",isSliding);
        if (CurrentCharacter==Character.Roll){SlidingCollision.SetActive(false);DashingCollision.SetActive(false);RollingCollision.SetActive(isSliding);}
        else if (CurrentCharacter==Character.Bass){SlidingCollision.SetActive(false);RollingCollision.SetActive(false);DashingCollision.SetActive(isSliding);}
        else {DashingCollision.SetActive(false);RollingCollision.SetActive(false);SlidingCollision.SetActive(isSliding);}
        StandingCollision.SetActive(!isSliding);
    }
    private void motion()
    {
        if (!groundContact) 
        {
            timeSinceJump += Time.deltaTime;
        }
        else if (timeSinceJump > 0.1f) 
        {
            slideJumping = false;
        }
        if (anim.GetBool("Hit"))
        {
            currentMotion = -0.8f;
        }
        else if (isSliding)
        {
            currentMotion = EquippedUpgrades[(int)upgrades.SuperSlide]?slideSpeed + (CurrentCharacter==Character.Bass?3:1):slideSpeed;
        }
        VelocityX = facingRight ? currentMotion : -currentMotion;
    }
    public void HealthChange(float amountChanged)
    {
        if (amountChanged == 0 || (amountChanged < 0 && isHit)){if (healthBar!=null){healthBar.fillAmount = Mathf.Max(0, healthPoints / 28f);} return;}
        isHit = amountChanged < 0;
        anim.SetBool("Hit", isHit && !EquippedUpgrades[(int)upgrades.ShockAbsorber] && invincibiltyTimer <= 0.5f && !(isSliding && ceilingContact));
        var safetyBall = GetComponentInChildren<SafetyBallScript>();
        if ((safetyBall && safetyBall.isActiveAndEnabled)||(cycloneStrike && cycloneStrike.activeInHierarchy))
        {
            if (safetyBall && safetyBall.isActiveAndEnabled){safetyBall.HealthChange(amountChanged);}
            amountChanged = Mathf.Max(0,amountChanged);
        }
        if (EquippedUpgrades[(int)upgrades.Armor] && isHit) amountChanged += amountChanged / 2;
        else if (EquippedUpgrades[(int)upgrades.SuperRecover] && amountChanged > 0) amountChanged *= 2;
        if (amountChanged>1||amountChanged<-1){audioSource.PlayOneShot(amountChanged>0?healSFX:hurtSFX);}
        healthPoints += (healthPoints >= 5 && -amountChanged > healthPoints) ? -healthPoints : amountChanged;
        healthPoints = Mathf.Clamp(healthPoints, -1, 28);
        dead = healthPoints < 0;
        if (healthBar!=null){healthBar.fillAmount = Mathf.Max(0, healthPoints / 28f);}
        if (dead){GetComponent<Buster>().enabled=false;enabled=false;rb.velocity=Vector2.zero;audioSource.PlayOneShot(dieSFX);}
    }
}
