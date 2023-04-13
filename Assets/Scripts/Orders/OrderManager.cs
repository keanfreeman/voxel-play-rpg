using InstantiatedEntity;
using Nito.Collections;
using NonVoxel;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] MovementManager movementManager;
    [SerializeField] Pathfinder pathfinder;
    [SerializeField] EffectManager effectManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] DialogueUI dialogueUI;

    private Coroutine currCoroutine = null;

    public void ExecuteOrders(OrderGroup orderGroup) {
        if (currCoroutine != null) {
            Debug.LogError("Tried to execute orders when they were already ordered.");
            return;
        }
        currCoroutine = StartCoroutine(ExecuteOrdersCoroutine(orderGroup));
    }

    private IEnumerator ExecuteOrdersCoroutine(OrderGroup orderGroup) {
        yield return gameStateManager.SetControlState(ControlState.FOLLOWING_ORDERS);

        // todo - have generic method for getting instantiations
        foreach (Order order in orderGroup.orders) {
            Type type = order.GetType();

            if (type == typeof(ExclaimOrder)) {
                ExclaimOrder exclaimOrder = (ExclaimOrder)order;
                PlayerMovement exclaimingPlayer = null;
                foreach (PlayerMovement player in partyManager.partyMembers) {
                    if (player.playerInfo == exclaimOrder.exclaimingEntity) {
                        exclaimingPlayer = player;
                        break;
                    }
                }
                if (exclaimingPlayer == null) {
                    continue;
                }
                yield return effectManager.GenerateExclaimEffect(exclaimingPlayer);
            }
            else if (type == typeof(MoveOrder)) {
                MoveOrder moveOrder = (MoveOrder)order;
                PlayerMovement movingPlayer = null;
                foreach (PlayerMovement player in partyManager.partyMembers) {
                    if (player.playerInfo == moveOrder.player) {
                        movingPlayer = player;
                        break;
                    }
                }
                if (movingPlayer == null) {
                    continue;
                }

                CoroutineWithData coroutineWithData = new CoroutineWithData(this,
                    pathfinder.FindPath(movingPlayer, moveOrder.destination));
                yield return coroutineWithData.coroutine;
                Deque<Vector3Int> path = (Deque<Vector3Int>)coroutineWithData.result;
                yield return movementManager.MoveAlongPath(movingPlayer, path);
            }
            else if (type == typeof(CameraFocusOrder)) {
                CameraFocusOrder cameraFocusOrder = (CameraFocusOrder)order;
                NPCBehavior focused = null;
                foreach (NPCBehavior npc in nonVoxelWorld.npcs) {
                    if (npc.npcInfo == cameraFocusOrder.focusTarget) {
                        focused = npc;
                        break;
                    }
                }
                if (focused == null) {
                    continue;
                }

                yield return cameraManager.MoveCameraToTargetCreature(focused);
            }
            else if (type == typeof(DialogueOrder)) {
                DialogueOrder dialogueOrder = (DialogueOrder)order;
                dialogueUI.StartDialogue(dialogueOrder.story);
                while (dialogueUI.IsDisplaying()) {
                    yield return null;
                }
            }
        }

        yield return cameraManager.MoveCameraToTargetCreature(partyManager.currControlledCharacter);
        yield return gameStateManager.SetControlState(ControlState.SPRITE_NEUTRAL);

        currCoroutine = null;
    }
}
