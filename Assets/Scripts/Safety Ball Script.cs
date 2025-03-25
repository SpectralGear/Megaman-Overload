using UnityEngine;

public class SafetyBallScript : MonoBehaviour
{
    CharControl cc;
    float defaultPosY;
    float smoothTime = 0.1f; // Adjust this for how quickly you want the transitions to occur
    Vector3 velocityScale = Vector3.zero;
    Vector3 velocityPosition = Vector3.zero;

    void Start()
    {
        cc = GetComponentInParent<CharControl>();
        defaultPosY = transform.localPosition.y;
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

            // Apply rotation only if there is velocity along the x-axis (cc.rb.velocity.x != 0)
            //if (cc.rb.velocity.x != 0)
            //{
                // Rotate the object around the z-axis based on the x-velocity
              //  transform.Rotate(0, 0, cc.rb.velocity.x*(cc.facingRight?-1:1)*2.5f); // Multiplier added for visible rotation speed
            //}
        }
    }
}