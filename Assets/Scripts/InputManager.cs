using MovementDirection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private PlayerMovement playerMovement;

    public PlayerInputActions playerInputActions;
    
    private void Awake() {
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Move.performed += playerMovement.HandleControllerMove;
        playerInputActions.Player.Move.canceled += playerMovement.HandleControllerMoveCancel;

        playerInputActions.Player.RotateCamera.performed += cameraManager.Rotate90Degrees;
        playerInputActions.Detached.RotateCamera.performed += cameraManager.RotateDetached;
        playerInputActions.Detached.RotateCamera.canceled += cameraManager.StopRotatingDetached;

        playerInputActions.Player.Enable();
    }

    public bool IsInUIMode() {
        return this.playerInputActions.UINavigation.enabled;
    }

    public void SwitchPlayerControlStateToUI() {
        playerInputActions.Player.Disable();
        playerInputActions.UINavigation.Enable();
        eventSystem.sendNavigationEvents = true;
    }

    public void SwitchUIToPlayerControlState() {
        playerInputActions.UINavigation.Disable();
        playerInputActions.Player.Enable();
        eventSystem.sendNavigationEvents = false;
    }

    public void SwitchPlayerToDetachedControlState() {
        playerInputActions.Player.Disable();
        playerInputActions.Detached.Enable();
    }

    public void SwitchDetachedToPlayerControlState() {
        playerInputActions.Player.Enable();
        playerInputActions.Detached.Disable();
    }

    // DETACHED

    public Vector2 GetDetachedMove() {
        return playerInputActions.Detached.Move.ReadValue<Vector2>();
    }

    public float GetDetachedVerticalMove() {
        return playerInputActions.Detached.MoveVertical.ReadValue<float>();
    }

    public bool WasSelectTriggered() {
        return playerInputActions.Detached.Select.triggered;
    }

    // DIALOGUE

    public bool WasContinueTriggered() {
        return playerInputActions.UINavigation.Submit.triggered;
    }

    public bool WasUICancelTriggered() {
        return playerInputActions.UINavigation.Cancel.triggered;
    }

    // PLAYER

    public bool WasOpenCombatBarTriggered() {
        return playerInputActions.Player.OpenCombatBar.triggered;
    }

    public bool WasInteractTriggered() {
        return playerInputActions.Player.Interact.triggered;
    }

    public bool WasSwitchInputTypeTriggered() {
        return playerInputActions.Player.SwitchInputType.triggered
            || playerInputActions.Detached.SwitchInputType.triggered;
    }
}
