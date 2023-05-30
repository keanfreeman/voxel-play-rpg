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
    LOADING,
    UI
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
        else if (newState == ControlState.LOADING) {
            inputManager.LockPlayerControls();
        }
        else if (controlState == ControlState.LOADING && newState == ControlState.SPRITE_NEUTRAL) {
            inputManager.UnlockPlayerControls();
        }
        else if (newState == ControlState.UI) {
            inputManager.LockPlayerControls();
            inputManager.UnlockUIControls(null);
        }
        else if (controlState == ControlState.UI && newState == ControlState.DETACHED) {
            inputManager.UnlockDetachedControls();
            inputManager.LockUIControls();
        }

        controlState = newState;
    }

    public void ExitDialogue() {
        controlState = ControlState.SPRITE_NEUTRAL;
        combatUI.SetDisplayState(true);
        inputManager.UnlockPlayerControls();
    }

    public void EnterCombat(NPC npcInCombat) {
        if (combatManager.IsInCombat()) {
            return;
        }

        inputManager.LockPlayerControls();
        if (npcInCombat != null) {
            HashSet<NPC> enemies = npcInCombat.teammates;
            if (npcInCombat.teammates == null) {
                enemies = new HashSet<NPC> { npcInCombat };
            }
            combatManager.SetEnemies(enemies);
        }
        controlState = ControlState.COMBAT;
        Debug.Log("Entered combat");
        combatManager.StartCombat();
        inputManager.SetDetachedToCombat();
    }

    public IEnumerator ExitCombat() {
        yield return SetControlState(ControlState.SPRITE_NEUTRAL);
        inputManager.SwitchDetachedToPlayerControlState();
        inputManager.SetDetachedToNormal();
    }

    public void HandleCombatBar(InputAction.CallbackContext obj) {
        inputManager.LockPlayerControls();
        combatUI.SetFocus();
        inputManager.UnlockUIControls(combatUI);
    }

    public void CloseCombatBar() {
        inputManager.LockUIControls();
        if (controlState == ControlState.COMBAT) {
            inputManager.UnlockDetachedControls();
        }
        else {
            inputManager.UnlockPlayerControls();
        }
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
            if (detachedCamera.isBuildMode) {
                detachedCamera.ToggleBuildMode();
            }
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

        EntityDefinition.TangibleEntity entityDef = interactableEntity.GetEntity();
        OrderGroup orderGroup = entityDef.interactOrders;
        if (orderGroup == null) {
            Debug.LogError("Entity is marked as interactable but has no interactions.");
            return;
        }
        orderManager.ExecuteOrders(orderGroup);
    }
}
