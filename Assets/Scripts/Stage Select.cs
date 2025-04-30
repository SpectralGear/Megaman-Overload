using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class StageSelect : MonoBehaviour
{
    private DefaultControls playerInputActions;
    [SerializeField] Transform cursor;
    [SerializeField] float[] CursorPositionsX = new float[5];
    [SerializeField] float[] CursorPositionsY = new float[3];
    [SerializeField] List<string> scenes = new List<string>();
    private void Awake()
    {
        playerInputActions = new DefaultControls();
        playerInputActions.Enable();
    }
    private void OnEnable()
    {
        playerInputActions.Menu.Move.started += OnMove;
        playerInputActions.Menu.Confirm.started += Confirm;
        playerInputActions.Menu.Cancel.started += Cancel;
    }
    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        playerInputActions.Menu.Move.started -= OnMove;
        playerInputActions.Menu.Confirm.started -= Confirm;
        playerInputActions.Menu.Cancel.started -= Cancel;
    }
    private void OnDestroy()
    {
        playerInputActions.Disable();
    }
    void Start()
    {
    }
    void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>().normalized;
        Vector2 currentPos = getPosIndex();
        Vector3 nextCanvasPos = new Vector3(CursorPositionsX[(int)currentPos.x],CursorPositionsY[(int)currentPos.y],0);
        if (inputVector.x!=0)
        {
            int index = (int)currentPos.x+(inputVector.x>0?1:-1);
            if (index<0){index=CursorPositionsX.Length-1;}
            else if (index>=CursorPositionsX.Length){index=0;}
            nextCanvasPos.x = CursorPositionsX[index];
        }
        else if (inputVector.y!=0)
        {
            int index = (int)currentPos.y+(inputVector.y>0?1:-1);
            if (index<0){index=CursorPositionsY.Length-1;}
            else if (index>=CursorPositionsY.Length){index=0;}
            nextCanvasPos.y = CursorPositionsY[index];
        }
        cursor.localPosition=nextCanvasPos;
    }
    void Confirm(InputAction.CallbackContext context)
    {
        Vector2 currentPos = getPosIndex();
        int index = (int)(currentPos.x+(currentPos.y*CursorPositionsX.Length));
        if (index==9)
        {
            SaveData saveData = new SaveData();
            saveData.ObtainedWeapons = new Buster.Weapon[6] {Buster.Weapon.MegaBuster,Buster.Weapon.SafetyBall,Buster.Weapon.CycloneStrike,Buster.Weapon.Brickfall,Buster.Weapon.Firecracker,Buster.Weapon.WaterCannon};
            SaveManager.SaveGame(saveData,0);
        }
        else
        {
            if (index >= 0 && index < scenes.Count && !string.IsNullOrEmpty(scenes[index])){SceneManager.LoadScene(scenes[index]);}
            else {Debug.Log($"Scene loading of "+index+" failed.");}
        }
    }
    void Cancel(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0);
    }
    Vector2 getPosIndex()
    {
         int closestIndexX = GetClosestIndex(CursorPositionsX, cursor.localPosition.x);
        int closestIndexY = GetClosestIndex(CursorPositionsY, cursor.localPosition.y);
        return new Vector2(closestIndexX, closestIndexY);
    }
    int GetClosestIndex(float[] positions, float value)
    {
        int closestIndex = 0;
        float smallestDifference = Mathf.Abs(positions[0] - value);
        for (int i = 1; i < positions.Length; i++)
        {
            float difference = Mathf.Abs(positions[i] - value);
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape)){SceneManager.LoadScene(0);}
    }
}