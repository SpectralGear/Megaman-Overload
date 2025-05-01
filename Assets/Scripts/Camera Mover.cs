using UnityEngine;
public class CameraMover : MonoBehaviour
{
    [SerializeField] float followSpeed = 5f;
    [SerializeField] Transform endpointA,endpointB;
    Rigidbody2D CameraRigidbody2D;
    bool CameraHeld=false;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("MainCamera"))
        {
            if (CameraRigidbody2D){CameraRigidbody2D=collision.GetComponent<Rigidbody2D>();}
            CameraHeld=true;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("MainCamera")){CameraHeld=false;}
    }
    private void FixedUpdate()
    {
        if (!CameraHeld || !CameraRigidbody2D || !endpointA || !endpointB)
            return;

        Vector3 railStart = endpointA.position;
        Vector3 railEnd = endpointB.position;
        Vector3 railDirection = (railEnd - railStart).normalized;
        float railLength = Vector3.Distance(railStart, railEnd);

        Vector3 toTarget = CameraRigidbody2D.position - (Vector2)railStart;
        float projectedLength = Mathf.Clamp(Vector3.Dot(toTarget, railDirection), 0, railLength);
        Vector3 targetPosition = railStart + railDirection * projectedLength;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
    }
}
