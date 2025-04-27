using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageSelect : MonoBehaviour
{
    private DefaultControls playerInputActions;
    Vector2 vector2;
    private void Awake()
    {
        playerInputActions = new DefaultControls();
        playerInputActions.Enable();
    }
    private void OnEnable()
    {
        playerInputActions.Menu.Move.performed += OnMove;
        playerInputActions.Menu.Move.canceled += OnStop;
        playerInputActions.Menu.Confirm.started += Confirm;
        playerInputActions.Menu.Cancel.started += Cancel;
    }
    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        playerInputActions.Menu.Move.performed -= OnMove;
        playerInputActions.Menu.Move.canceled -= OnStop;
        playerInputActions.Menu.Confirm.started -= Confirm;
        playerInputActions.Menu.Cancel.started -= Cancel;
    }
    private void OnDestroy()
    {
        playerInputActions.Controls.Disable();
    }
    void OnMove(InputAction.CallbackContext context)
    {
        vector2=context.ReadValue<Vector2>().normalized;
    }
    void OnStop(InputAction.CallbackContext context)
    {
        vector2=Vector2.zero;
    }
    void Confirm(InputAction.CallbackContext context)
    {
    }
    void Cancel(InputAction.CallbackContext context)
    {
    }
}
