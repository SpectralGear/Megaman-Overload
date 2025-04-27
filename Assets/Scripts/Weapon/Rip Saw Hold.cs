using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipSawHold : SolarBulletBehaviour
{
    [SerializeField] float spinSpeed;
    private void FixedUpdate()
    {
        if (transform.lossyScale.x<0){rb.rotation += spinSpeed;}
        else {rb.rotation -= spinSpeed;}
    }
    protected override void Move()
    {
    }
}
