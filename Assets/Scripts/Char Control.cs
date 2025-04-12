using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Buster))]
public class CharControl : MonoBehaviour
{
    [SerializeField] public float runSpeed,slideSpeed,slideTime,jumpForce,gravity,gravityInWater,jumpArcStart,maxVerticalVelocity;
    [SerializeField] GameObject StandingCollision,SlidingCollision;
    [SerializeField] public bool DashComboInput,SlideComboInput,inWater;
    public enum upgrades {Armor,ShockAbsorber,AutoRecover, EnergySaver,SuperRecover,PickupFinder, ExtraCharge,QuickerCharge,BeamBuster, SuperSlide,Sprinter,WallKick}
    [SerializeField] public List<upgrades> OwnedUpgrades = new List<upgrades>();
    [SerializeField] public List<bool> EquippedUpgrades = new List<bool>();
    public enum Character {Megaman, Protoman, Bass, Roll}
    [SerializeField] public Character CurrentCharacter;
    Image healthBar;
    Animator anim;
    public Rigidbody2D rb;
    private bool isHit=false,slideJumping=false,slidingToTheRight;
    public bool dead,facingRight=true,isSliding=false,grounded,velocityOverride;
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
        healthBar = GameObject.Find("HP").GetComponent<Image>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
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
        if (isSliding&&!ceilingAbove())
        {
            slideJumping = true;
            timeSinceJump = 0f;
            VelocityY = jumpForce;
            audioSource.PlayOneShot(jumpSFX);
        }
        else if (SlideComboInput&&moveInputY<0&&grounded&&!isSliding)
        {
            isSliding = true;
            audioSource.PlayOneShot(slideSFX);
            slideTimer=0;
            slidingToTheRight = facingRight;
        }
        else if ((!SlideComboInput||moveInputY>=0)&&grounded)
        {
            VelocityY = jumpForce;
            audioSource.PlayOneShot(jumpSFX);
        }
        else if (CurrentCharacter==Character.Megaman && EquippedUpgrades[(int)upgrades.WallKick] && (rightWallContact(false) || leftWallContact(false)))
        {
            VelocityY = jumpForce/5*3; 
            if (rightWallContact(false)) 
            {
                currentMotion = -10;
            }
            else if (leftWallContact(false)) 
            {
                currentMotion = 10;
            }
            audioSource.PlayOneShot(jumpSFX);
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
        if (grounded&&!isSliding)
        {
            isSliding = true;
            audioSource.PlayOneShot(slideSFX);
            slideTimer=0;
            slidingToTheRight = facingRight;
        }
    }
    private void Update()
    {
        grounded=groundContact();
        OnMove();
        fall(!grounded);
        slide();
        motion();
        if (CurrentCharacter!=Character.Protoman&&EquippedUpgrades[(int)upgrades.AutoRecover]&&healthPoints>=0&&healthPoints<28){HealthChange(Time.deltaTime/5);}
        if (isHit&&invincibiltyTimer<1){invincibiltyTimer+=Time.deltaTime; if (invincibiltyTimer > 0.5f){anim.SetBool("Hit",false);}}
        else {isHit=false;invincibiltyTimer=0;}
    }
    private void FixedUpdate()
    {
        if (!velocityOverride){rb.velocity = new Vector2(VelocityX,VelocityY);}
    }
    bool groundContact()
    {
        float groundCheckDistance = 0.2f;
        RaycastHit2D hitCenter = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y+0.1f), Vector2.down, groundCheckDistance, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(transform.position.x - 0.45f, transform.position.y+0.1f), Vector2.down, groundCheckDistance, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(transform.position.x + 0.45f, transform.position.y+0.1f), Vector2.down, groundCheckDistance, LayerMask.GetMask("Terrain"));
        if (hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null)
        {return true;}
        else {return false;}
    }
    public bool leftWallContact(bool sliding)
    {
        float wallCheckDistance = 0.2f;
        float wallCheckHeight;
        wallCheckHeight = !sliding ? 1.2f : 0.7f;
        RaycastHit2D hitTop = Physics2D.Raycast(new Vector2(transform.position.x - 0.44f, wallCheckHeight), Vector2.left, wallCheckDistance, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitCenter = Physics2D.Raycast(new Vector2(transform.position.x - 0.44f, (wallCheckHeight+0.1f)/2), Vector2.left, wallCheckDistance, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitBottom = Physics2D.Raycast(new Vector2(transform.position.x - 0.44f, transform.position.y+0.1f), Vector2.left, wallCheckDistance, LayerMask.GetMask("Terrain"));
        if (hitCenter.collider != null || hitTop.collider != null || hitBottom.collider != null)
        {return true;}
        else {return false;}
    }
    public bool rightWallContact(bool sliding)
    {
        float wallCheckDistance = 0.2f;
        float wallCheckHeight;
        wallCheckHeight = !sliding ? 1.2f : 0.7f;
        RaycastHit2D hitTop = Physics2D.Raycast(new Vector2(transform.position.x + 0.44f, wallCheckHeight), Vector2.right, wallCheckDistance, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitCenter = Physics2D.Raycast(new Vector2(transform.position.x + 0.44f, (wallCheckHeight+0.1f)/2), Vector2.right, wallCheckDistance, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitBottom = Physics2D.Raycast(new Vector2(transform.position.x + 0.44f, transform.position.y+0.1f), Vector2.right, wallCheckDistance, LayerMask.GetMask("Terrain"));
        if (hitCenter.collider != null || hitTop.collider != null || hitBottom.collider != null)
        {return true;}
        else {return false;}
    }
    bool ceilingAbove()
    {
        float ceilingCheckDistance = 0.95f;
        RaycastHit2D hitCenter = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y+0.7f), Vector2.up, ceilingCheckDistance, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(transform.position.x - 0.44f, transform.position.y+0.7f), Vector2.up, ceilingCheckDistance-0.25f, LayerMask.GetMask("Terrain"));
        RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(transform.position.x + 0.44f, transform.position.y+0.7f), Vector2.up, ceilingCheckDistance-0.25f, LayerMask.GetMask("Terrain"));
        if (hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null){return true;}
        else {return false;}
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
        if (anim.GetBool("Jumping")==true&&!airborne&&!GetComponentInChildren<SafetyBallScript>()){audioSource.PlayOneShot(landSFX);}
        anim.SetBool("Jumping", airborne);
        if (!airborne) {return;}
        float accelerationY = inWater ? gravityInWater : gravity;
        if (ceilingAbove()&&VelocityY>jumpArcStart){VelocityY=jumpArcStart;}
        VelocityY -= Time.deltaTime * accelerationY;
        VelocityY = Mathf.Clamp(VelocityY, -maxVerticalVelocity, maxVerticalVelocity);
    }
    private void slide()
    {
        if (isSliding&&(slideTimer >= slideTime||slidingToTheRight!=facingRight||(leftWallContact(true)&&!slidingToTheRight)||(rightWallContact(true)&&slidingToTheRight)||!grounded)&&!ceilingAbove())
        {
            isSliding=false;
            currentMotion=0;
        }
        else if (isSliding&&!grounded&&ceilingAbove())
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
        SlidingCollision.SetActive(isSliding);
        StandingCollision.SetActive(!isSliding);
    }
    private void motion()
    {
        if (!grounded) 
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
        anim.SetBool("Hit", isHit && !EquippedUpgrades[(int)upgrades.ShockAbsorber] && invincibiltyTimer <= 0.5f && !(isSliding && ceilingAbove()));
        var safetyBall = GetComponentInChildren<SafetyBallScript>();
        if (safetyBall && safetyBall.isActiveAndEnabled)
        {
            safetyBall.HealthChange(amountChanged);
            amountChanged = 0;
        }
        else
        {
            if (EquippedUpgrades[(int)upgrades.Armor] && isHit) amountChanged += amountChanged / 2;
            else if (EquippedUpgrades[(int)upgrades.SuperRecover] && amountChanged > 0) amountChanged *= 2;
        }
        if (amountChanged>1||amountChanged<-1){audioSource.PlayOneShot(amountChanged>0?healSFX:hurtSFX);}
        healthPoints += (healthPoints >= 5 && -amountChanged > healthPoints) ? -healthPoints : amountChanged;
        healthPoints = Mathf.Clamp(healthPoints, -1, 28);
        dead = healthPoints < 0;
        if (healthBar!=null){healthBar.fillAmount = Mathf.Max(0, healthPoints / 28f);}
        if (dead){GetComponent<Buster>().enabled=false;enabled=false;rb.velocity=Vector2.zero;audioSource.PlayOneShot(dieSFX);}
    }
}
