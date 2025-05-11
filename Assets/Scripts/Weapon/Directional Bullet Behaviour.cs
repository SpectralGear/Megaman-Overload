using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalBulletBehaviour : SolarBulletBehaviour
{
    protected override void Move()
    {
        float angleInRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 moveDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        if (rb){rb.velocity = moveDirection.normalized * speed;}
        else {rb = gameObject.GetAny<Rigidbody2D>();}
    }
}
