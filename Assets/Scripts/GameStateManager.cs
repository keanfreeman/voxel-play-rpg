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

    public ControlState controlState { get; set; } = ControlState.LOADING;

    void Update() {
        switch (controlState) {
            case ControlState.FIRST_PERSON:
                break;
            case ControlState.SPRITE_NEUTRAL:
                NPCBehavior npcInCombat = HandleNPCsFreeMovement();
                if (npcInCombat != null) {
                    inputManager.playerInputActions.Player.Disable();
                    combatManager.SetFirstCombatant(npcInCombat);
                    controlState = ControlState.COMBAT;
                    Debug.Log("Entered combat");
                    inputManager.SwitchPlayerControlStateToUI();
                    return;
                }
                if (partyManager.currControlledCharacter.isMoving || cameraManager.isRotating) {
                    return;
                }

                HandlePlayerPrimaryInput();
                HandleSwitchInputMode();
                HandleCombatBar();
                break;
            case ControlState.DETACHED:
                HandleSwitchInputMode();
                break;
            case ControlState.DIALOGUE:
                HandleDialogueContinue();
                if (!dialogueUI.isDialogueActive) {
                    controlState = ControlState.SPRITE_NEUTRAL;
                    inputManager.SwitchUIToPlayerControlState();
                }
                break;
            case ControlState.COMBAT:
                combatManager.StartCombat();
                break;
            default:
                break;
        }
    }

    public void ExitCombat() {
        controlState = ControlState.SPRITE_NEUTRAL;
        inputManager.SwitchDetachedToPlayerControlState();
    }

    private void HandleCombatBar() {
        if (inputManager.WasOpenCombatBarTriggered()) {
            inputManager.SwitchPlayerControlStateToUI();
            combatUI.ApplyFocus();
        }
        else if (inputManager.IsInUIMode() && inputManager.WasUICancelTriggered()) {
            combatUI.RemoveFocus();
            inputManager.SwitchUIToPlayerControlState();
        }
    }

    private void HandleSwitchInputMode() {
        if (inputManager.WasSwitchInputTypeTriggered()) {
            if (controlState == ControlState.SPRITE_NEUTRAL) {
                controlState = ControlState.DETACHED;
                inputManager.SwitchPlayerToDetachedControlState(
                    partyManager.currControlledCharacter.currVoxel);
            }
            else {
                controlState = ControlState.SPRITE_NEUTRAL;
                inputManager.SwitchDetachedToPlayerControlState();
            }
        }
    }

    private NPCBehavior HandleNPCsFreeMovement() {
        foreach (NPCBehavior npc in nonVoxelWorld.npcs) {
            if (npc.encounteredPlayer) {
                return npc;
            }
            npc.HandleRandomMovement();
        }
        return null;
    }

    private void HandlePlayerPrimaryInput() {
        if (!inputManager.WasInteractTriggered()) {
            return;
        }

        // check for interactable objects
        Story story = null;

        Vector3Int currPosition = partyManager.currControlledCharacter.currVoxel;
        List<Vector3Int> interactablePositions = nonVoxelWorld.GetInteractableAdjacentObjects(currPosition);
        if (interactablePositions.Count > 0) {
            Vector3Int firstItem = interactablePositions.First();
            story = objectInkMapping.GetStoryFromObject(nonVoxelWorld.GetNVEFromPosition(firstItem).gameObject);
        }
        else {
            List<Vector3d> interactableVoxels = 
                voxelWorldManager.GetInteractableAdjacentVoxels(new Vector3d(currPosition));
            if (interactableVoxels.Count > 0) {
                Vector3d firstItem = interactableVoxels.First();
                story = objectInkMapping.GetStoryFromVoxel(voxelWorldManager.GetVoxelFromPosition(firstItem));
            }
        }

        if (story == null) {
            Debug.Log("No interactable object near player.");
            return;
        }

        Debug.Log("Interactable thing near player.");
        controlState = ControlState.DIALOGUE;
        inputManager.SwitchPlayerControlStateToUI();

        dialogueUI.StartDialogue(story);
    }

    private void HandleDialogueContinue() {
        if (!inputManager.WasContinueTriggered()) {
            return;
        }

        dialogueUI.HandleInput();
        if (!dialogueUI.isDialogueActive) {
            controlState = ControlState.SPRITE_NEUTRAL;
            inputManager.SwitchUIToPlayerControlState();
        }
    }
}
