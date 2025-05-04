using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
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
        Vector3 cameraMovement = Vector3.Lerp(rb.position, PlayerPos.position+DefaultOffset, 0.5f);
        rb.MovePosition(cameraMovement);
    }
}
