using Ink.Runtime;
using Instantiated;
using NonVoxel;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VoxelPlay;

public enum ControlState {
    FIRST_PERSON,
    SPRITE_NEUTRAL,
    DETACHED,
    DIALOGUE,
    COMBAT,
    FOLLOWING_ORDERS,
    LOADING
}

public class GameStateManager : MonoBehaviour
{
    [SerializeField] CombatManager combatManager;
    [SerializeField] CombatUI combatUI;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] DialogueUI dialogueUI;
    [SerializeField] InputManager inputManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] ObjectInkMapping objectInkMapping;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] MovementManager movementManager;
    [SerializeField] OrderManager orderManager;

    public ControlState controlState { get; private set; } = ControlState.LOADING;

    public IEnumerator SetControlState(ControlState newState) {
        if (newState == ControlState.FOLLOWING_ORDERS) {
            inputManager.LockPlayerControls();

            PlayerCharacter playerMovement = partyManager.currControlledCharacter;
            if (movementManager.IsMoving(playerMovement)) {
                movementManager.CancelMovement(playerMovement);
                while (playerMovement.isMoving) {
                    yield return null;
                }
            }

            if (controlState == ControlState.DETACHED) {
                cameraManager.AttachCameraToPlayer(playerMovement);
            }

            combatUI.SetDisplayState(false);
        }
        else if (controlState == ControlState.FOLLOWING_ORDERS) {
            combatUI.SetDisplayState(true);
            inputManager.UnlockPlayerControls();
        }
        else if (controlState == ControlState.SPRITE_NEUTRAL && newState == ControlState.LOADING) {
            inputManager.LockPlayerControls();
        }
        else if (controlState == ControlState.LOADING && newState == ControlState.SPRITE_NEUTRAL) {
            inputManager.UnlockPlayerControls();
        }

        controlState = newState;
    }

    public void ExitDialogue() {
        controlState = ControlState.SPRITE_NEUTRAL;
        combatUI.SetDisplayState(true);
        inputManager.UnlockPlayerControls();
    }

    public void EnterCombat(NPC npcInCombat) {
        inputManager.LockPlayerControls();
        combatManager.SetFirstCombatant(npcInCombat);
        controlState = ControlState.COMBAT;
        Debug.Log("Entered combat");
        combatManager.StartCombat();
    }

    public void ExitCombat() {
        controlState = ControlState.SPRITE_NEUTRAL;
        inputManager.SwitchDetachedToPlayerControlState();
    }

    public void HandleCombatBar(InputAction.CallbackContext obj) {
        inputManager.LockPlayerControls();
        combatUI.SetFocus();
        inputManager.UnlockUIControls(combatUI);
    }

    public void CloseCombatBar() {
        inputManager.LockUIControls();
        inputManager.UnlockPlayerControls();
    }

    public void HandleSwitchInputMode(InputAction.CallbackContext obj) {
        if (controlState == ControlState.SPRITE_NEUTRAL) {
            controlState = ControlState.DETACHED;
            inputManager.SwitchPlayerToDetachedControlState(
                partyManager.currControlledCharacter.origin);
        }
        else {
            controlState = ControlState.SPRITE_NEUTRAL;
            inputManager.SwitchDetachedToPlayerControlState();
        }
    }
    
    public void HandleControllerInteract(InputAction.CallbackContext obj) {
        HashSet<Vector3Int> adjacentPositions = Coordinates.GetPositionsSurroundingTraveller(
            partyManager.currControlledCharacter, 1);
        TangibleEntity interactableEntity = null;
        foreach (Vector3Int position in adjacentPositions) {
            interactableEntity = nonVoxelWorld.GetInteractableEntity(position);
            if (interactableEntity != null) {
                break;
            }
        }
        if (interactableEntity == null) {
            Debug.Log("No interactable object near player.");
            return;
        }

        if (interactableEntity.GetType() == typeof(TangibleObject)) {
            TangibleObject tangibleObject = (TangibleObject)interactableEntity;
            OrderGroup orderGroup = tangibleObject.objectInfo.interactOrders;
            orderManager.ExecuteOrders(orderGroup);
        }
    }
}
