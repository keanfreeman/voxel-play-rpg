using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private TextAsset textAsset;

    public PlayerInputContextHandler(PlayerMovement playerMovement, NonVoxelWorld nonVoxelWorld,
            Dialogue dialogue, VoxelWorld voxelWorld, InputManager inputManager, TextAsset textAsset) {
        this.playerMovement = playerMovement;
        this.nonVoxelWorld = nonVoxelWorld;
        this.dialogue = dialogue;
        this.voxelWorld = voxelWorld;
        this.inputManager = inputManager;
        this.textAsset = textAsset;
        controlState = ControlState.FIRST_PERSON;
    }

    public void HandlePlayerInput() {
        if (Input.GetKeyUp(KeyCode.J)) {
            Debug.Log("Debug key pressed.");
        }
        HandleSwapCameraState();

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

    private void HandleSwapCameraState() {
        if (inputManager.WasSwitchInputTypeTriggered()) {
            playerMovement.ToggleFreeCamera();
            controlState = (controlState == ControlState.FIRST_PERSON) 
                ? ControlState.SPRITE_NEUTRAL : ControlState.FIRST_PERSON;
        }
    }

    private void HandlePlayerPrimaryInput() {
        if (!inputManager.WasInteractTriggered()) {
            return;
        }

        // check for interactable objects
        Vector3Int currPosition = nonVoxelWorld.GetPosition(playerMovement.spriteContainer);
        List<Vector3Int> interactablePositions = nonVoxelWorld.GetInteractableAdjacentObjects(currPosition);
        List<VoxelPlay.Vector3d> interactableVoxels = 
            voxelWorld.GetInteractableAdjacentVoxels(new VoxelPlay.Vector3d(currPosition));
        if (interactablePositions.Count == 0 && interactableVoxels.Count == 0) {
            Debug.Log("No interactable object near player.");
            return;
        }

        Debug.Log("Interactable thing near player.");
        controlState = ControlState.DIALOGUE;
        inputManager.SwitchPlayerControlStateToDialogue();

        dialogue.StartDialogue(textAsset);
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
