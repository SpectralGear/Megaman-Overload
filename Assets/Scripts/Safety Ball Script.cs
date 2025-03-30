using UnityEngine;

public class SafetyBallScript : MonoBehaviour
{
    CharControl cc;
    float defaultPosY;
    float smoothTime = 0.1f; // Adjust this for how quickly you want the transitions to occur
    Vector3 velocityScale = Vector3.zero;
    Vector3 velocityPosition = Vector3.zero;
    [SerializeField] float maxShieldHealth;
    float shieldHealth = 0;
    public Material material;   // Material where the texture will be applied
    public float rotationSpeed = 1f;  // Speed of texture rotation (in seconds)
    private Texture2D originalTexture;  // Store the original texture
    private Texture2D rotatedTexture;   // Store the rotated texture temporarily
    private float timeSinceLastChange = 0f;  // Timer for rotation timing
    AudioSource audioSource;
    void Start()
    {
        cc = GetComponentInParent<CharControl>();
        defaultPosY = transform.localPosition.y;
        if (material != null)
        {
            // Store the original texture from the material
            originalTexture = material.GetTexture("_MainTex") as Texture2D;
        }
        audioSource = GetComponent<AudioSource>();
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
        }
    }
    void Update()
    {
        if (originalTexture != null)
        {
            timeSinceLastChange += Time.deltaTime;
            
            // Change the texture after a certain time has passed
            if (timeSinceLastChange >= rotationSpeed)
            {
                // Rotate the texture and create a new rotated version
                RotateTexture();
                
                // Update the material with the rotated texture
                material.SetTexture("_MainTex", rotatedTexture);

                // Reset the timer
                timeSinceLastChange = 0f;
            }
        }
    }
    void RotateTexture()
    {
       // Clone the original texture to a new one
        rotatedTexture = new Texture2D(originalTexture.width, originalTexture.height);

        // Rotate the texture
        for (int x = 0; x < originalTexture.width; x++)
        {
            for (int y = 0; y < originalTexture.height; y++)
            {
                // Get the pixel at the current position
                Color pixelColor = originalTexture.GetPixel(x, y);
                // Set the pixel to the rotated position
                rotatedTexture.SetPixel(y, originalTexture.width - 1 - x, pixelColor);
            }
        }

        // Apply the changes to the rotated texture
        rotatedTexture.Apply();
    }
    private void OnDestroy()
    {
        // Cleanup: Destroy the rotated texture when done
        if (rotatedTexture != null)
        {
            Destroy(rotatedTexture);
        }
    }
}