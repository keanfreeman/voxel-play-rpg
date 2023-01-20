using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputContextHandler : MonoBehaviour
{
    public enum ControlState {
        FIRST_PERSON,
        SPRITE_NEUTRAL,
        DIALOGUE
    }

    private ControlState controlState;
    private PlayerMovement playerMovement;
    private NonVoxelWorld nonVoxelWorld;
    private Dialogue dialogue;

    public PlayerInputContextHandler(PlayerMovement playerMovement, NonVoxelWorld nonVoxelWorld,
            Dialogue dialogue) {
        this.playerMovement = playerMovement;
        this.nonVoxelWorld = nonVoxelWorld;
        this.dialogue = dialogue;
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
                HandleReturnInDialogue();
                break;
            default:
                break;
        }
    }

    private void HandleSwapCameraState() {
        if (Input.GetKeyUp(KeyCode.K)) {
            playerMovement.ToggleFreeCamera();
            controlState = (controlState == ControlState.FIRST_PERSON) 
                ? ControlState.SPRITE_NEUTRAL : ControlState.FIRST_PERSON;
        }
    }

    private void HandlePlayerPrimaryInput() {
        if (!Input.GetKeyDown(KeyCode.Return)) {
            return;
        }

        // check for interactable objects
        Vector3Int currPosition = nonVoxelWorld.GetPosition(playerMovement.spriteContainer);
        List<Vector3Int> occupiedPositions = nonVoxelWorld.GetInteractableAdjacentObjects(currPosition);
        if (occupiedPositions.Count == 0) {
            Debug.Log("No interactable object near player.");
            return;
        }

        Debug.Log("Object near player.");
        controlState = ControlState.DIALOGUE;
        List<string> sentences = new List<string> { "This is the first sentence.",
            "This is a slightly longer second sentence that probably takes up several lines." };
         dialogue.StartDialogue(sentences);
    }

    private void HandleReturnInDialogue() {
        if (!Input.GetKeyDown(KeyCode.Return)) {
            return;
        }

        dialogue.HandleReturn();
        if (!dialogue.gameObject.activeSelf) {
            controlState = ControlState.SPRITE_NEUTRAL;
        }
    }
}
