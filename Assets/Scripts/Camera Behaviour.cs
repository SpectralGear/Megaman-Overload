using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] Vector3 DefaultOffset;
    [SerializeField] GameObject WeaponMenu, healthUI, weaponUI;
    [SerializeField] Image[] WeaponAmmoUI = new Image[10];
    [SerializeField] Image HealthUI, WeaponUI;
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
        weaponUI.SetActive(!menuActive);
    }
    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
        charControl=Player.GetComponent<CharControl>();
        buster=Player.GetComponent<Buster>();
        WeaponMenu.SetActive(menuActive);
        healthUI.SetActive(!menuActive);
        weaponUI.SetActive(!menuActive);
    }
    void Update()
    {
        if (menuActive&&!WeaponAmmoUI.Contains(null))
        {
            WeaponAmmoUI[0].fillAmount=Mathf.Clamp(charControl.HealthPoints/28f,0,1);
            for (int i = 1; i < WeaponAmmoUI.Length; i++)
            {
                WeaponAmmoUI[i].fillAmount=Mathf.Clamp(buster.WeaponEnergy[i-1]/28f,0,1);
            }
        }
        else
        {
            if (HealthUI&&charControl){HealthUI.fillAmount=Mathf.Clamp(charControl.HealthPoints/28f,0,1);}
            if (WeaponUI&&buster)
            {
                if ((int)buster._equippedWeapon<1)
                {
                    WeaponUI.fillAmount=1;
                }
                else
                {
                    WeaponUI.fillAmount=Mathf.Clamp(buster.WeaponEnergy[(int)buster._equippedWeapon-1]/28f,0,1);
                }
            }
        }
    }
    void FixedUpdate()
    {
        Vector3 cameraMovement = Vector3.Lerp(rb.position, Player.transform.position+DefaultOffset, 0.5f);
        rb.MovePosition(cameraMovement);
    }
}
