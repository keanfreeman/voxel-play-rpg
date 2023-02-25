using Ink.Runtime;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VoxelPlay;

public class PlayerInputContextHandler
{
    public enum ControlState {
        FIRST_PERSON,
        SPRITE_NEUTRAL,
        DETACHED,
        DIALOGUE,
        COMBAT,
        SEQUENCE
    }

    private ControlState controlState;
    private PlayerMovement playerMovement;
    private NonVoxelWorld nonVoxelWorld;
    private Dialogue dialogue;
    private VoxelWorld voxelWorld;
    private InputManager inputManager;
    private ObjectInkMapping objectInkMapping;
    private Combat combat;
    private DetachedCamera detachedCamera;

    public PlayerInputContextHandler(PlayerMovement playerMovement, NonVoxelWorld nonVoxelWorld,
            Dialogue dialogue, VoxelWorld voxelWorld, InputManager inputManager,
            ObjectInkMapping objectInkMapping, DetachedCamera detachedCamera) {
        this.playerMovement = playerMovement;
        this.nonVoxelWorld = nonVoxelWorld;
        this.dialogue = dialogue;
        this.voxelWorld = voxelWorld;
        this.inputManager = inputManager;
        this.objectInkMapping = objectInkMapping;
        this.detachedCamera = detachedCamera;
        controlState = ControlState.SPRITE_NEUTRAL;

        System.Random rng = new System.Random();
        combat = new Combat(nonVoxelWorld, rng, new Dice(rng));
    }

    public void HandlePlayerInput() {

        switch (controlState) {
            case ControlState.FIRST_PERSON:
                break;
            case ControlState.SPRITE_NEUTRAL:
                NPCBehavior npcInCombat = HandleNPCsFreeMovement();
                if (!playerMovement.isMoving && !playerMovement.isRotating
                        && npcInCombat != null) {
                    controlState = ControlState.COMBAT;
                    combat.firstCombatant = npcInCombat;
                    return;
                }
                bool isTransitioning = playerMovement.HandleMovementControls();
                if (isTransitioning) {
                    return;
                }

                HandlePlayerPrimaryInput();
                HandleSwitchInputMode();
                break;
            case ControlState.DETACHED:
                detachedCamera.HandleFrame();
                HandleSwitchInputMode();
                break;
            case ControlState.DIALOGUE:
                HandleDialogueContinue();
                if (!dialogue.isDialogueActive) {
                    controlState = ControlState.SPRITE_NEUTRAL;
                    inputManager.SwitchDialogueToPlayerControlState();
                }
                break;
            case ControlState.COMBAT:
                // determine combat order (initiative)
                // iterate through turns until no players are left or no NPCs are left
                combat.RunCombat();
                break;
            default:
                break;
        }
    }

    private void HandleSwitchInputMode() {
        if (inputManager.WasSwitchInputTypeTriggered()) {
            if (controlState == ControlState.SPRITE_NEUTRAL) {
                controlState = ControlState.DETACHED;
                inputManager.SwitchPlayerToDetachedControlState();
                playerMovement.SetCameraState(false);
                detachedCamera.GetComponentInChildren<Camera>().enabled = true;
            }
            else {
                controlState = ControlState.SPRITE_NEUTRAL;
                inputManager.SwitchDetachedToPlayerControlState();
                playerMovement.SetCameraState(true);
                detachedCamera.GetComponentInChildren<Camera>().enabled = false;
            }
        }
    }

    // returns true if combat started
    private NPCBehavior HandleNPCsFreeMovement() {
        foreach (NPCBehavior npc in nonVoxelWorld.npcs) {
            if (npc.encounteredPlayer) {
                return npc;
            }
            npc.HandleRandomMovement();
        }
        return null;
    }

    private void HandleNPCsCombat() {

    }

    private void HandlePlayerPrimaryInput() {
        if (!inputManager.WasInteractTriggered()) {
            return;
        }

        // check for interactable objects
        Story story = null;

        Vector3Int currPosition = nonVoxelWorld.GetPosition(playerMovement.gameObject);
        List<Vector3Int> interactablePositions = nonVoxelWorld.GetInteractableAdjacentObjects(currPosition);
        if (interactablePositions.Count > 0) {
            Vector3Int firstItem = interactablePositions.First();
            story = objectInkMapping.GetStoryFromObject(nonVoxelWorld.GetObjectFromPosition(firstItem));
        }
        else {
            List<Vector3d> interactableVoxels = 
                voxelWorld.GetInteractableAdjacentVoxels(new Vector3d(currPosition));
            if (interactableVoxels.Count > 0) {
                Vector3d firstItem = interactableVoxels.First();
                story = objectInkMapping.GetStoryFromVoxel(voxelWorld.GetVoxelFromPosition(firstItem));
            }
        }

        if (story == null) {
            Debug.Log("No interactable object near player.");
            return;
        }

        Debug.Log("Interactable thing near player.");
        controlState = ControlState.DIALOGUE;
        inputManager.SwitchPlayerControlStateToDialogue();

        dialogue.StartDialogue(story);
    }

    private void HandleDialogueContinue() {
        if (!inputManager.WasContinueTriggered()) {
            return;
        }

        dialogue.HandleInput();
        if (!dialogue.isDialogueActive) {
            controlState = ControlState.SPRITE_NEUTRAL;
            inputManager.SwitchDialogueToPlayerControlState();
        }
    }
}
