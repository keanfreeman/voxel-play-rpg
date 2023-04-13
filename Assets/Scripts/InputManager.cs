using Instantiated;
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
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] DialogueUI dialogueUI;

    public PlayerInputActions playerInputActions;
    
    private void Awake() {
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.RotateCamera.performed += cameraManager.Rotate90Degrees;
        playerInputActions.Detached.RotateCamera.performed += cameraManager.RotateDetached;
        playerInputActions.Detached.RotateCamera.canceled += cameraManager.StopRotatingDetached;

        // TODO - only hook these up to the combat manager when combat is active
        playerInputActions.Detached.Select.performed += combatManager.HandleDetachedSelect;
        playerInputActions.Detached.Cancel.performed += combatManager.HandleDetachedCancel;

        playerInputActions.Player.SwitchCharacter.performed += partyManager.SwitchToNextCharacter;
        playerInputActions.Detached.SwitchCharacter.performed += partyManager.SwitchToNextCharacter;

        playerInputActions.Detached.Select.performed += detachedCamera.HandleSelect;

        playerInputActions.Watch.RotateCamera.performed += cameraManager.RotateDetached;
        playerInputActions.Watch.RotateCamera.canceled += cameraManager.StopRotatingDetached;

        playerInputActions.Player.Interact.performed += gameStateManager.HandleControllerInteract;

        playerInputActions.UINavigation.Submit.performed += dialogueUI.HandleSubmit;

        playerInputActions.Player.SwitchInputType.performed += gameStateManager.HandleSwitchInputMode;
        playerInputActions.Detached.SwitchInputType.performed += gameStateManager.HandleSwitchInputMode;

        playerInputActions.Player.OpenCombatBar.performed += gameStateManager.HandleCombatBar;
        playerInputActions.UINavigation.Cancel.performed += gameStateManager.HandleCombatBar;

        playerInputActions.Player.Enable();
    }

    public void LockPlayerControls() {
        playerInputActions.Player.Disable();
        playerInputActions.Detached.Disable();
    }

    public void UnlockPlayerControls() {
        playerInputActions.Player.Enable();
    }

    public void LockUIControls() {
        playerInputActions.UINavigation.Disable();
    }

    public void UnlockUIControls() {
        playerInputActions.UINavigation.Enable();
    }

    public void SetPlayerMovementControls(PlayerCharacter oldPlayerMovement,
            PlayerCharacter newPlayerMovement) {
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

    public void SwitchPlayerToDetachedControlState(Vector3Int startPosition) {
        playerInputActions.Player.Disable();
        playerInputActions.Detached.Enable();

        cameraManager.AttachCameraToDetached();
        detachedCamera.BecomeActive(startPosition);
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

        playerInputActions.Watch.Enable();
    }

    public void DisableWatchState() {
        playerInputActions.Watch.Disable();
    }

    // DETACHED

    public Vector2 GetDetachedMove() {
        return playerInputActions.Detached.Move.ReadValue<Vector2>();
    }

    public float GetDetachedVerticalMove() {
        return playerInputActions.Detached.MoveVertical.ReadValue<float>();
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
