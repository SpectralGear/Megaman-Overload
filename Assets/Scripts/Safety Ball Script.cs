using UnityEngine;

public class SafetyBallScript : MonoBehaviour
{
    CharControl cc;
    float defaultPosY;
    [SerializeField] float smoothTime = 0.1f; // Adjust this for how quickly you want the transitions to occur
    Vector3 velocityScale = Vector3.zero;
    Vector3 velocityPosition = Vector3.zero;
    [SerializeField] float maxShieldHealth;
    float shieldHealth = 0;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip popSFX,bounceSFX;
    [SerializeField] GameObject ball;
    [SerializeField] float AttackFallMagnitude, DamageDealt;
    public bool Attack;
    bool wasAttacking;
    int direction;
    void Start()
    {
        cc = GetComponentInParent<CharControl>();
        defaultPosY = transform.localPosition.y;
    }
    void OnEnable()
    {
        shieldHealth = maxShieldHealth;
        audioSource.Play();
    }
    void OnDisable()
    {
        PlaySoundAtPosition(popSFX,transform.position);
        Attack=false;
        wasAttacking=false;
        cc.velocityOverride=false;
    }
    void PlaySoundAtPosition(AudioClip clip, Vector3 position)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.Play();
        Destroy(tempGO, clip.length);
    }
    public void HealthChange(float amountChanged)
    {
        shieldHealth+=amountChanged;
        shieldHealth=Mathf.Clamp(shieldHealth,0,maxShieldHealth);
        gameObject.SetActive(shieldHealth<=0?false:true);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (Attack&&(collision.CompareTag("Enemy")||collision.CompareTag("Boss"))&&collision.GetComponent<EnemyHealth>()){collision.GetComponent<EnemyHealth>().TakeDamage(DamageDealt,(int)Buster.Projectile.BallBounce);Attack=false;}
    }
    void Update()
    {
        if (!Attack)
        {
            switch (cc.moveInputX)
            {
                case >0:
                direction=1;
                return;
                case <0:
                direction=-1;
                return;
                default:
                direction=0;
                return;
            }
        }
    }
    void FixedUpdate()
    {
        if (cc != null)
        {
            // Default values if no specific conditions are met
            Vector3 newScale = Vector3.one;
            Vector3 newPosition = new Vector3(0, defaultPosY, 0);

            if (!cc.groundContact)
            {
                newScale = new Vector3(0.9f, 1.2f, 0.9f);
                newPosition = new Vector3(0, defaultPosY * 0.8f, 0);
            }
            else if (cc.isSliding)
            {
                newScale = new Vector3(1.3f, 0.5f, 1.3f);
                newPosition = new Vector3(-0.5f, defaultPosY * 0.5f, 0);
            }

            // Smoothly transition the scale and position
            transform.localScale = Vector3.SmoothDamp(transform.localScale, newScale, ref velocityScale, smoothTime);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, newPosition, ref velocityPosition, smoothTime);

            // Spin the ball
            if (ball!=null){ball.transform.Rotate(new Vector3(0,0,Attack?-20:(cc.rb.velocity.x==0&&cc.groundContact?0:(-1*cc.rb.velocity.magnitude))));}

            if (Attack)
            {
                cc.velocityOverride=true;
                if ((cc.wallContact[1]&&direction<0)||(cc.wallContact[0]&&direction>0)){direction*=-1;audioSource.PlayOneShot(bounceSFX);}
                cc.rb.velocity = direction==0?Vector2.down*AttackFallMagnitude:new Vector2(direction,-2).normalized*AttackFallMagnitude;
                wasAttacking=true;
            }
            else
            {
                cc.velocityOverride=false;
                if (wasAttacking){cc.VelocityY=cc.jumpForce*1.3f;wasAttacking=false;audioSource.PlayOneShot(bounceSFX);}
            }
        }
    }
}