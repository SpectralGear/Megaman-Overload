using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerBehaviour : MonoBehaviour
{
    [SerializeField] Transform PlayerPos;
    [SerializeField] Vector3 DefaultOffset;
    Rigidbody2D rb;
    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        rb.MovePosition(PlayerPos.position+DefaultOffset);
    }
}
