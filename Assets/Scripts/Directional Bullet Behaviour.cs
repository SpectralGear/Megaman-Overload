using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalBulletBehaviour : SolarBulletBehaviour
{
    protected override void Move()
    {
        float angleInRadians = transform.rotation.z * Mathf.Deg2Rad;
        Vector2 moveDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        rb.velocity = moveDirection * speed;
    }
}
