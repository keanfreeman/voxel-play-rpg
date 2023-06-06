using Instantiated;
using Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] CombatManager combatManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] DialogueUI dialogueUI;
    [SerializeField] ConstructionUI constructionUI;
    [SerializeField] UIDocument uiDocument;
    [SerializeField] BuildShadow buildShadow;
    [SerializeField] SaveManager saveManager;

    public PlayerInputActions playerInputActions;

    private UIHandler currUIHandler;
    
    private void Awake() {
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Save.performed += Save_performed;
        playerInputActions.Player.Load.performed += Load_performed;

        playerInputActions.Player.RotateCamera.performed += cameraManager.Rotate90Degrees;
        playerInputActions.Detached.RotateCamera.performed += cameraManager.RotateDetached;
        playerInputActions.Detached.RotateCamera.canceled += cameraManager.StopRotatingDetached;

        playerInputActions.Player.SwitchCharacter.performed += partyManager.SwitchToNextCharacter;
        playerInputActions.Detached.SwitchCharacter.performed += partyManager.SwitchToNextCharacter;

        playerInputActions.Detached.Select.performed += detachedCamera.HandleSelect;
        playerInputActions.Detached.Cancel.performed += detachedCamera.HandleCancel;

        playerInputActions.Watch.RotateCamera.performed += cameraManager.RotateDetached;
        playerInputActions.Watch.RotateCamera.canceled += cameraManager.StopRotatingDetached;

        playerInputActions.Player.Interact.performed += gameStateManager.HandleControllerInteract;

        playerInputActions.Player.SwitchInputType.performed += gameStateManager.HandleSwitchInputMode;
        playerInputActions.Detached.SwitchInputType.performed += gameStateManager.HandleSwitchInputMode;

        playerInputActions.Player.OpenCombatBar.performed += gameStateManager.HandleCombatBar;

        playerInputActions.Detached.ToggleBuildMode.performed += detachedCamera.HandleToggleBuildMode;
        
        playerInputActions.Detached.SwitchToUI.performed += detachedCamera.HandleSwitchToUI;
        playerInputActions.Detached.RotateObject.performed += buildShadow.HandleRotateObject;
        playerInputActions.Detached.RotateObject.canceled += buildShadow.HandleCancelRotateObject;

        playerInputActions.Player.Enable();
        eventSystem.sendNavigationEvents = false;
    }

    private void Save_performed(InputAction.CallbackContext obj) {
        saveManager.Save();
    }

    private void Load_performed(InputAction.CallbackContext obj) {
        saveManager.Load();
    }

    public void SetDetachedToCombat() {
        playerInputActions.Detached.Select.performed -= detachedCamera.HandleSelect;
        playerInputActions.Detached.Cancel.performed -= detachedCamera.HandleCancel;

        playerInputActions.Detached.Select.performed += combatManager.HandleDetachedSelect;
        playerInputActions.Detached.Cancel.performed += combatManager.HandleDetachedCancel;
    }

    public void SetDetachedToNormal() {
        playerInputActions.Detached.Select.performed -= combatManager.HandleDetachedSelect;
        playerInputActions.Detached.Cancel.performed -= combatManager.HandleDetachedCancel;

        playerInputActions.Detached.Select.performed += detachedCamera.HandleSelect;
        playerInputActions.Detached.Cancel.performed += detachedCamera.HandleCancel;
    }

    private void SetUIHandlers(UIHandler uiHandler) {
        if (currUIHandler != null) {
            playerInputActions.UINavigation.Navigate.performed -= currUIHandler.HandleNavigate;
            playerInputActions.UINavigation.Navigate.canceled -= currUIHandler.HandleCancelNavigate;
            playerInputActions.UINavigation.Submit.performed -= currUIHandler.HandleSubmit;
            playerInputActions.UINavigation.Cancel.performed -= currUIHandler.HandleCancel;
        }
        currUIHandler = uiHandler;
        if (uiHandler != null) {
            playerInputActions.UINavigation.Navigate.performed += uiHandler.HandleNavigate;
            playerInputActions.UINavigation.Navigate.canceled += uiHandler.HandleCancelNavigate;
            playerInputActions.UINavigation.Submit.performed += uiHandler.HandleSubmit;
            playerInputActions.UINavigation.Cancel.performed += uiHandler.HandleCancel;
        }
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
        Focusable focusable = uiDocument.rootVisualElement.focusController.focusedElement;
        if (focusable != null) {
            focusable.Blur();
        }
        eventSystem.sendNavigationEvents = false;
        SetUIHandlers(null);
    }

    public void UnlockUIControls(UIHandler uiHandler) {
        playerInputActions.UINavigation.Enable();
        eventSystem.sendNavigationEvents = true;
        SetUIHandlers(uiHandler);
    }

    public void UnlockDetachedControls() {
        playerInputActions.Detached.Enable();
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
