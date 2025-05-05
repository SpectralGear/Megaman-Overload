using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] Vector3 DefaultOffset;
    [SerializeField] GameObject WeaponMenu, healthUI, WeaponUI;
    CharControl charControl;
    Buster buster;
    Rigidbody2D rb;
    bool menuActive=false;
    private DefaultControls playerInputActions;
    private void Awake()
    {
        playerInputActions = new DefaultControls();
        playerInputActions.Enable();
    }
    private void OnEnable()
    {
        playerInputActions.Controls.Pause.started += ToggleMenu;
        playerInputActions.Controls.WeaponMenu.started += ToggleMenu;
    }
    private void OnDisable()
    {
        playerInputActions.Controls.Pause.started -= ToggleMenu;
        playerInputActions.Controls.WeaponMenu.started -= ToggleMenu;
    }
    private void OnDestroy()
    {
        playerInputActions.Controls.Disable();
    }
    void ToggleMenu(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.GamePaused==menuActive)
        {
            GameManager.Instance.TogglePause();
            menuActive=!menuActive;
        }
        WeaponMenu.SetActive(menuActive);
        healthUI.SetActive(!menuActive);
        WeaponUI.SetActive(!menuActive);
    }
    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
        charControl=Player.GetComponent<CharControl>();
        buster=Player.GetComponent<Buster>();
        WeaponMenu.SetActive(menuActive);
        healthUI.SetActive(!menuActive);
        WeaponUI.SetActive(!menuActive);
    }
    void FixedUpdate()
    {
        Vector3 cameraMovement = Vector3.Lerp(rb.position, Player.transform.position+DefaultOffset, 0.5f);
        rb.MovePosition(cameraMovement);
    }
}
