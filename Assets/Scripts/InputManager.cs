using MovementDirection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager
{
    private PlayerInputActions playerInputActions;

    public SpriteMoveDirection moveDirection = SpriteMoveDirection.NONE;

    public InputManager(PlayerInputActions playerInputActions) {
        this.playerInputActions = playerInputActions;

        this.playerInputActions.Player.MoveUp.performed += MoveUp_performed;
        this.playerInputActions.Player.MoveDown.performed += MoveDown_performed;
        this.playerInputActions.Player.MoveLeft.performed += MoveLeft_performed;
        this.playerInputActions.Player.MoveRight.performed += MoveRight_performed;

        this.playerInputActions.Player.MoveUp.canceled += MoveUp_canceled;
        this.playerInputActions.Player.MoveDown.canceled += MoveDown_canceled;
        this.playerInputActions.Player.MoveLeft.canceled += MoveLeft_canceled;
        this.playerInputActions.Player.MoveRight.canceled += MoveRight_canceled;
    }

    public void SwitchPlayerControlStateToDialogue() {
        playerInputActions.Player.Disable();
        playerInputActions.Dialogue.Enable();
    }

    public void SwitchDialogueToPlayerControlState() {
        playerInputActions.Dialogue.Disable();
        playerInputActions.Player.Enable();
    }

    // DIALOGUE

    public bool WasContinueTriggered() {
        return playerInputActions.Dialogue.Continue.triggered;
    }

    // PLAYER

    public bool WasInteractTriggered() {
        return playerInputActions.Player.Interact.triggered;
    }

    public bool WasSwitchInputTypeTriggered() {
        return playerInputActions.Player.SwitchInputType.triggered;
    }

    private void MoveRight_canceled(InputAction.CallbackContext obj) {
        if (moveDirection == SpriteMoveDirection.RIGHT) {
            moveDirection = SpriteMoveDirection.NONE;
        }
    }

    private void MoveLeft_canceled(InputAction.CallbackContext obj) {
        if (moveDirection == SpriteMoveDirection.LEFT) {
            moveDirection = SpriteMoveDirection.NONE;
        }
    }

    private void MoveDown_canceled(InputAction.CallbackContext obj) {
        if (moveDirection == SpriteMoveDirection.BACK) {
            moveDirection = SpriteMoveDirection.NONE;
        }
    }

    private void MoveUp_canceled(InputAction.CallbackContext obj) {
        if (moveDirection == SpriteMoveDirection.FORWARD) {
            moveDirection = SpriteMoveDirection.NONE;
        }
    }

    private void MoveRight_performed(InputAction.CallbackContext obj) {
        moveDirection = SpriteMoveDirection.RIGHT;
    }

    private void MoveLeft_performed(InputAction.CallbackContext obj) {
        moveDirection = SpriteMoveDirection.LEFT;
    }

    private void MoveDown_performed(InputAction.CallbackContext obj) {
        moveDirection = SpriteMoveDirection.BACK;
    }

    private void MoveUp_performed(InputAction.CallbackContext obj) {
        moveDirection = SpriteMoveDirection.FORWARD;
    }
}