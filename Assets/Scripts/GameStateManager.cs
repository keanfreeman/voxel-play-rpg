using Ink.Runtime;
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

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private CombatUI combatUI;
    [SerializeField] private DetachedCamera detachedCamera;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private NonVoxelWorld nonVoxelWorld;
    [SerializeField] private ObjectInkMapping objectInkMapping;
    [SerializeField] private VoxelWorldManager voxelWorldManager;

    public ControlState controlState { get; set; }

    void Awake() {
        DontDestroyOnLoad(gameObject);
        controlState = ControlState.LOADING;
    }

    void Update() {
        switch (controlState) {
            case ControlState.FIRST_PERSON:
                break;
            case ControlState.SPRITE_NEUTRAL:
                NPCBehavior npcInCombat = HandleNPCsFreeMovement();
                if (!playerMovement.isMoving && !playerMovement.isRotating
                        && npcInCombat != null) {
                    controlState = ControlState.COMBAT;
                    combatManager.SetFirstCombatant(npcInCombat);
                    return;
                }
                bool isTransitioning = playerMovement.HandleMovementControls();
                if (isTransitioning) {
                    return;
                }

                HandlePlayerPrimaryInput();
                HandleSwitchInputMode();
                HandleCombatBar();
                break;
            case ControlState.DETACHED:
                detachedCamera.HandleFrame();
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
                combatManager.RunCombat();
                break;
            default:
                break;
        }
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
                inputManager.SwitchPlayerToDetachedControlState();
                playerMovement.SetCameraState(false);
                detachedCamera.BecomeActive();
            }
            else {
                controlState = ControlState.SPRITE_NEUTRAL;
                inputManager.SwitchDetachedToPlayerControlState();
                detachedCamera.BecomeInactive();
                playerMovement.SetCameraState(true);
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
