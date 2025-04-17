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
    [SerializeField] GameObject StandingCollision,SlidingCollision,Camera;
    [SerializeField] public bool DashComboInput,SlideComboInput,inWater;
    public enum upgrades {Armor,ShockAbsorber,AutoRecover, EnergySaver,SuperRecover,PickupFinder, ExtraCharge,QuickerCharge,BeamBuster, SuperSlide,Sprinter,WallKick}
    [SerializeField] public List<upgrades> OwnedUpgrades = new List<upgrades>();
    [SerializeField] public List<bool> EquippedUpgrades = new List<bool>();
    public enum Character {Megaman, Protoman, Bass, Roll}
    [SerializeField] public Character CurrentCharacter;
    [SerializeField] Image healthBar;
    Animator anim;
    public Rigidbody2D rb;
    private bool isHit=false,slideJumping=false,slidingToTheRight;
    public bool dead,facingRight=true,isSliding=false,groundContact,ceilingContact,velocityOverride;
    public bool[] wallContact = new bool[2];
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
            isSliding = true;
            audioSource.PlayOneShot(slideSFX);
            slideTimer=0;
            slidingToTheRight = facingRight;
        }
        else if ((!SlideComboInput||moveInputY>=0)&&groundContact)
        {
            VelocityY = jumpForce;
            audioSource.PlayOneShot(jumpSFX);
        }
        else if (CurrentCharacter==Character.Megaman && EquippedUpgrades[(int)upgrades.WallKick] && (wallContact[1] || wallContact[0]))
        {
            VelocityY = jumpForce/5*3; 
            if (wallContact[1]) 
            {
                currentMotion = -10;
            }
            else if (wallContact[0])
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
        if (groundContact&&!isSliding&&!((Contacts(true,facingRight)[0]&&!facingRight)||(Contacts(true,facingRight)[1]&&facingRight)))
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
        bool[] contacts = Contacts(isSliding&&CurrentCharacter!=Character.Roll,facingRight);
        groundContact = contacts[2];
        wallContact[0] = contacts[0];
        wallContact[1] = contacts[1];
        ceilingContact = contacts[3];
        if (!velocityOverride){rb.velocity = new Vector2(VelocityX,VelocityY);}
    }
    void LateUpdate()
    {
        if (Camera.transform.lossyScale.x<0)Camera.transform.localScale=new Vector3(Camera.transform.localScale.x*-1,Camera.transform.localScale.y,Camera.transform.localScale.z);
    }
    public bool[] Contacts(bool sliding, bool lookingRight)
    {
        RaycastHit2D leftWallCheck = Physics2D.Raycast(new Vector2(transform.position.x - (lookingRight&&sliding&&CurrentCharacter!=Character.Roll?1.3f:0.5f), transform.position.y+0.05f), Vector2.up, sliding?0.7f:1.2f, LayerMask.GetMask("Terrain"));
        RaycastHit2D rightWallCheck = Physics2D.Raycast(new Vector2(transform.position.x + (!lookingRight&&sliding&&CurrentCharacter!=Character.Roll?1.3f:0.5f), transform.position.y+0.05f), Vector2.up, sliding?0.7f:1.2f, LayerMask.GetMask("Terrain"));
        RaycastHit2D floorCheck = Physics2D.Raycast(new Vector2(transform.position.x + (lookingRight?0.45f:-0.45f), transform.position.y-0.05f), lookingRight?Vector2.left:Vector2.right, sliding&&CurrentCharacter!=Character.Roll?1.7f:0.9f, LayerMask.GetMask("Terrain"));
        RaycastHit2D ceilingCheck = Physics2D.Raycast(new Vector2(transform.position.x + (lookingRight?0.33f:-0.33f), transform.position.y+1.7f), lookingRight?Vector2.left:Vector2.right, sliding&&CurrentCharacter!=Character.Roll?1f:0.66f, LayerMask.GetMask("Terrain"));
        if (leftWallCheck){Debug.DrawRay(leftWallCheck.point,Vector2.right/2f,Color.blue,0.5f);}
        if (rightWallCheck){Debug.DrawRay(rightWallCheck.point,Vector2.left/2f,Color.red,0.5f);}
        if (floorCheck){Debug.DrawRay(floorCheck.point,Vector2.up/2f,Color.green,0.5f);}
        if (ceilingCheck){Debug.DrawRay(ceilingCheck.point,Vector2.down/2f,Color.yellow,0.5f);}
        return new bool[4] {leftWallCheck,rightWallCheck,floorCheck,ceilingCheck};
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
        if (ceilingContact&&VelocityY>jumpArcStart){VelocityY=jumpArcStart;}
        VelocityY -= Time.deltaTime * accelerationY;
        VelocityY = Mathf.Clamp(VelocityY, -maxVerticalVelocity, maxVerticalVelocity);
    }
    private void slide()
    {
        if (isSliding&&(slideTimer >= slideTime||slidingToTheRight!=facingRight||(Contacts(true,facingRight)[0]&&!slidingToTheRight)||(Contacts(true,facingRight)[1]&&slidingToTheRight)||!groundContact)&&!ceilingContact)
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
        SlidingCollision.SetActive(isSliding);
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
        var cycloneStrike = GetComponentInChildren<CycloneStrikeBehaviour>();
        if ((safetyBall && safetyBall.isActiveAndEnabled)||(cycloneStrike && cycloneStrike.isActiveAndEnabled))
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
