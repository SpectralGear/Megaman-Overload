using UnityEngine;
public class CameraMover : MonoBehaviour
{
    enum Direction {x,y}
    [SerializeField] Direction direction;
    [SerializeField] float transitionSpeed = 1;
    [SerializeField] Transform camEndPoint, playerEndPoint;
    GameObject mainCamera, player;
    CameraBehaviour camBehavior;
    Rigidbody2D camRB;
    bool CameraHeld=false, playerAtTransition=false;
    Vector3 startPos, endPos, playerStartPos, playerEndPos;
    float timer = 0;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MainCamera"))
        {
            if (!mainCamera)
            {
                mainCamera=collision.gameObject;
                camBehavior = mainCamera.GetComponent<CameraBehaviour>();
                camRB = mainCamera.GetComponent<Rigidbody2D>();
            }
            startPos = new Vector3(transform.position.x,transform.position.y,mainCamera.transform.position.z);
            endPos = new Vector3(camEndPoint.position.x,camEndPoint.position.y,mainCamera.transform.position.z);
            CameraHeld=true;
            camBehavior.enabled=false;
            camRB.bodyType=RigidbodyType2D.Kinematic;
        }
        else if (collision.CompareTag("Player"))
        {
            if (!player)
            {
                player=collision.gameObject;
            }
            playerAtTransition=true;
            playerStartPos = player.transform.position;
            playerEndPos = new Vector3
            (
                direction==Direction.x?playerEndPoint.position.x:playerStartPos.x,
                direction==Direction.y?playerEndPoint.position.y:playerStartPos.y,
                playerStartPos.z
            );
        }
    }
    void Update()
    {
        if (!CameraHeld || !mainCamera || !camEndPoint || !playerAtTransition || !playerEndPoint)
        {
            timer=0;
        }
        else
        {
            if (!GameManager.Instance.GamePaused){GameManager.Instance.PauseGame();}
            if (timer>=1)
            {
                GameManager.Instance.UnpauseGame();
                CameraHeld=false;
                camBehavior.enabled=true;
                camRB.bodyType=RigidbodyType2D.Dynamic;
                playerAtTransition=false;
                return;
            }
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, timer);
            player.transform.position = Vector3.Lerp(playerStartPos, playerEndPos, timer);
            timer+=Time.unscaledDeltaTime*transitionSpeed;
            timer = Mathf.Min(timer, 1f);
        }
    }
}
