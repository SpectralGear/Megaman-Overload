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
    [SerializeField] GameObject ball;
    public bool Attack;
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
    public void HealthChange(float amountChanged)
    {
        shieldHealth+=amountChanged;
        shieldHealth=Mathf.Clamp(shieldHealth,0,maxShieldHealth);
        gameObject.SetActive(shieldHealth<=0?false:true);
    }
    void FixedUpdate()
    {
        if (cc != null)
        {
            // Default values if no specific conditions are met
            Vector3 newScale = Vector3.one;
            Vector3 newPosition = new Vector3(0, defaultPosY, 0);

            if (!cc.groundContact())
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
            if (ball!=null&&cc.rb.velocity.x!=0){ball.transform.Rotate(new Vector3(0,0,Attack?-20:-1*cc.rb.velocity.magnitude));}
        }
    }
}