using Ink.Runtime;
using NonVoxel;
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
        DIALOGUE,
        SEQUENCE
    }

    private ControlState controlState;
    private PlayerMovement playerMovement;
    private NonVoxelWorld nonVoxelWorld;
    private Dialogue dialogue;
    private VoxelWorld voxelWorld;
    private InputManager inputManager;
    private ObjectInkMapping objectInkMapping;

    public PlayerInputContextHandler(PlayerMovement playerMovement, NonVoxelWorld nonVoxelWorld,
            Dialogue dialogue, VoxelWorld voxelWorld, InputManager inputManager,
            ObjectInkMapping objectInkMapping) {
        this.playerMovement = playerMovement;
        this.nonVoxelWorld = nonVoxelWorld;
        this.dialogue = dialogue;
        this.voxelWorld = voxelWorld;
        this.inputManager = inputManager;
        this.objectInkMapping = objectInkMapping;
        controlState = ControlState.SPRITE_NEUTRAL;
    }

    public void HandlePlayerInput() {

        switch (controlState) {
            case ControlState.FIRST_PERSON:
                break;
            case ControlState.SPRITE_NEUTRAL:
                bool isTransitioning = playerMovement.HandleMovementControls();
                if (isTransitioning) {
                    return;
                }
                HandlePlayerPrimaryInput();
                break;
            case ControlState.DIALOGUE:
                HandleDialogueContinue();
                if (!dialogue.isDialogueActive) {
                    controlState = ControlState.SPRITE_NEUTRAL;
                    inputManager.SwitchDialogueToPlayerControlState();
                }
                break;
            default:
                break;
        }
    }

    //private void HandleSwapCameraState() {
    //    if (inputManager.WasSwitchInputTypeTriggered()) {
    //        playerMovement.ToggleFreeCamera();
    //        controlState = (controlState == ControlState.FIRST_PERSON) 
    //            ? ControlState.SPRITE_NEUTRAL : ControlState.FIRST_PERSON;
    //    }
    //}

    private void HandlePlayerPrimaryInput() {
        if (!inputManager.WasInteractTriggered()) {
            return;
        }

        // check for interactable objects
        Story story = null;

        Vector3Int currPosition = nonVoxelWorld.GetPosition(playerMovement.spriteContainer);
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
