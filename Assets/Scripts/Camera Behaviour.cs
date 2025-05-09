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
    [SerializeField] Color[] WeaponAmmoColors = new Color[10];
    [SerializeField] Image[] WeaponAmmoUI = new Image[10];
    [SerializeField] Sprite[] WeaponIcons = new Sprite[10];
    [SerializeField] Image HealthUI, WeaponUI, WeaponIcon;
    [SerializeField] Material WeaponPickupMat;
    CharControl charControl;
    Buster buster;
    Rigidbody2D rb;
    bool menuActive=false;
    int bolts;
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
        SaveData saveData = SaveManager.LoadGame(0);
        bolts = Mathf.Clamp(saveData.bolts,0,999);
    }
    void Update()
    {
        if (menuActive && WeaponAmmoUI.Length == 10 && WeaponAmmoUI.All(img => img != null))
        {
            for (int i = 1; i < 10; i++)
            {
                if (i < buster.WeaponEnergy.Length)
                    WeaponAmmoUI[i].fillAmount = Mathf.Clamp(buster.WeaponEnergy[i] / 28f, 0f, 1f);
            }
            WeaponAmmoUI[0].fillAmount = Mathf.Clamp(charControl.HealthPoints / 28f, 0f, 1f);
        }
        else
        {
            if (HealthUI&&charControl){HealthUI.fillAmount=Mathf.Clamp(charControl.HealthPoints/28f,0,1);}
            if (WeaponUI&&buster)
            {
                WeaponUI.fillAmount=Mathf.Clamp(buster.WeaponEnergy[(int)buster._equippedWeapon]/28f,0,1);
                WeaponUI.color=WeaponAmmoColors[(int)buster._equippedWeapon];
            }
            if (buster._equippedWeapon==Buster.Weapon.MegaBuster){weaponUI.SetActive(false);}
            else 
            {
                weaponUI.SetActive(true);
                
            }
            WeaponPickupMat.color=WeaponAmmoColors[(int)buster._equippedWeapon];
            WeaponIcon.sprite=WeaponIcons[(int)buster._equippedWeapon];
        }
    }
    void FixedUpdate()
    {
        Vector3 cameraMovement = Vector3.Lerp(rb.position, Player.transform.position+DefaultOffset, 0.5f);
        rb.MovePosition(cameraMovement);
    }
}
