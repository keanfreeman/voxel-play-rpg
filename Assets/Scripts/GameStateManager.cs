using Ink.Runtime;
using InstantiatedEntity;
using NonVoxel;
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

    public ControlState controlState { get; private set; } = ControlState.LOADING;

    public void SetControlState(ControlState newState) {
        controlState = newState;
    }

    public void EnterDialogue(Story story) {
        controlState = ControlState.DIALOGUE;
        inputManager.SwitchPlayerControlStateToUI();
        combatUI.SetDisplayState(false);
        dialogueUI.StartDialogue(story);
    }

    public void ExitDialogue() {
        controlState = ControlState.SPRITE_NEUTRAL;
        combatUI.SetDisplayState(true);
        inputManager.SwitchUIToPlayerControlState();
    }

    public void EnterCombat(NPCBehavior npcInCombat) {
        inputManager.playerInputActions.Player.Disable();
        combatManager.SetFirstCombatant(npcInCombat);
        controlState = ControlState.COMBAT;
        Debug.Log("Entered combat");
        inputManager.SwitchPlayerControlStateToUI();
        combatManager.StartCombat();
    }

    public void ExitCombat() {
        controlState = ControlState.SPRITE_NEUTRAL;
        inputManager.SwitchDetachedToPlayerControlState();
    }

    // TODO - fix dialogue/combat bar clashing
    public void HandleCombatBar(InputAction.CallbackContext obj) {
        if (controlState == ControlState.DIALOGUE) {
            return;
        }

        if (inputManager.WasOpenCombatBarTriggered()) {
            inputManager.SwitchPlayerControlStateToUI();
            combatUI.ApplyFocus();
        }
        else if (inputManager.IsInUIMode() && inputManager.WasUICancelTriggered()) {
            combatUI.RemoveFocus();
            inputManager.SwitchUIToPlayerControlState();
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
        }
    }
    
    public void HandleControllerInteract(InputAction.CallbackContext obj) {
        HashSet<Vector3Int> adjacentPositions = Coordinates.GetPositionsSurroundingTraveller(
            partyManager.currControlledCharacter, 1);
        InstantiatedNVE interactableEntity = null;
        foreach (Vector3Int position in adjacentPositions) {
            if (nonVoxelWorld.IsInteractable(position)) {
                interactableEntity = nonVoxelWorld.GetNVEFromPosition(position);
                break;
            }
        }
        if (interactableEntity == null) {
            Debug.Log("No interactable object near player.");
            return;
        }

        Story story = objectInkMapping.GetStoryFromEntity(interactableEntity);
        if (story == null) {
            Debug.Log("No story for that entity.");
            return;
        }

        EnterDialogue(story);
    }
}
