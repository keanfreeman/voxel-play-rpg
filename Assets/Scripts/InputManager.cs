using InstantiatedEntity;
using MovementDirection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] CombatManager combatManager;
    [SerializeField] PartyManager partyManager;

    public PlayerInputActions playerInputActions;
    
    private void Awake() {
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.RotateCamera.performed += cameraManager.Rotate90Degrees;
        playerInputActions.Detached.RotateCamera.performed += cameraManager.RotateDetached;
        playerInputActions.Detached.RotateCamera.canceled += cameraManager.StopRotatingDetached;

        playerInputActions.Detached.Select.performed += combatManager.HandleDetachedSelect;
        playerInputActions.Detached.Cancel.performed += combatManager.HandleDetachedCancel;

        playerInputActions.Player.Enable();
    }

    public void SetPlayerMovementControls(PlayerMovement oldPlayerMovement,
            PlayerMovement newPlayerMovement) {
        if (oldPlayerMovement != null) {
            playerInputActions.Player.Move.performed -= oldPlayerMovement.HandleControllerMove;
            playerInputActions.Player.Move.canceled -= oldPlayerMovement.HandleControllerMoveCancel;
        }

        playerInputActions.Player.Move.performed += newPlayerMovement.HandleControllerMove;
        playerInputActions.Player.Move.canceled += newPlayerMovement.HandleControllerMoveCancel;
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

        cameraManager.AttachCameraToDetached();
        detachedCamera.BecomeActive();
    }

    public void SwitchDetachedToPlayerControlState() {
        playerInputActions.Player.Enable();
        playerInputActions.Detached.Disable();

        detachedCamera.BecomeInactive();
        cameraManager.AttachCameraToPlayer(partyManager.currControlledCharacter);
    }

    public void SwitchDetachedToWatchControlState() {
        playerInputActions.Detached.Disable();

        detachedCamera.BecomeInactive();
        cameraManager.AttachCameraToPlayer(partyManager.currControlledCharacter);
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
