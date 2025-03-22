using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipSawLaunch : SolarBulletBehaviour
{
    [SerializeField] float spinSpeed;
    void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        Vector3 normal = contact.normal;
        float angle = Vector3.Angle(normal, Vector3.up);
        if (angle >= 90 && collision.gameObject.layer == LayerMask.NameToLayer("Terrain")){transform.localScale = new Vector3(transform.localScale.x*-1,transform.localScale.y,transform.localScale.z);}
    }
    protected override void Move()
    {
        base.Move();
        if (transform.localScale.x>0){rb.rotation -= spinSpeed;}
        else {rb.rotation += spinSpeed;}
    }
}
