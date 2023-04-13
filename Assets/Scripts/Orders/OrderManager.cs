using Instantiated;
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
                InstantiatedEntity exclaimer = nonVoxelWorld.GetEntityFromDefinition(
                    exclaimOrder.exclaimingEntity);
                if (exclaimer == null || !TypeUtil.IsSameTypeOrSubclass(exclaimer, typeof(Traveller))) {
                    continue;
                }

                Traveller traveller = (Traveller)exclaimer;
                yield return effectManager.GenerateExclaimEffect(traveller);
            }
            else if (type == typeof(MoveOrder)) {
                MoveOrder moveOrder = (MoveOrder)order;
                InstantiatedEntity mover = nonVoxelWorld.GetEntityFromDefinition(moveOrder.player);
                if (mover == null || !TypeUtil.IsSameTypeOrSubclass(mover, typeof(Traveller))) {
                    continue;
                }

                Traveller traveller = (Traveller)mover;
                CoroutineWithData coroutineWithData = new CoroutineWithData(this,
                    pathfinder.FindPath(traveller, moveOrder.destination));
                yield return coroutineWithData.coroutine;
                Deque<Vector3Int> path = (Deque<Vector3Int>)coroutineWithData.result;
                yield return movementManager.MoveAlongPath(traveller, path);
            }
            else if (type == typeof(CameraFocusOrder)) {
                CameraFocusOrder cameraFocusOrder = (CameraFocusOrder)order;
                InstantiatedEntity focused = nonVoxelWorld.GetEntityFromDefinition(
                    cameraFocusOrder.focusTarget);
                if (focused == null || !TypeUtil.IsSameTypeOrSubclass(focused, typeof(Traveller))) {
                    continue;
                }

                Traveller traveller = (Traveller)focused;
                yield return cameraManager.MoveCameraToTargetCreature(traveller);
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
