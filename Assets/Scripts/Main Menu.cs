using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class MainMenu : MonoBehaviour
{
    private DefaultControls playerInputActions;
    private void Awake()
    {
        playerInputActions = new DefaultControls();
        playerInputActions.Enable();
    }
    private void OnEnable()
    {
        playerInputActions.Menu.Confirm.started += Confirm;
        playerInputActions.Menu.Cancel.started += Cancel;
    }
    private void OnDisable()
    {
        playerInputActions.Menu.Confirm.started -= Confirm;
        playerInputActions.Menu.Cancel.started -= Cancel;
    }
    private void OnDestroy()
    {
        playerInputActions.Disable();
    }
    void Confirm(InputAction.CallbackContext context)
    {
       SceneManager.LoadScene(1);
    }
    void Cancel(InputAction.CallbackContext context)
    {
        Application.Quit();
    }
}
