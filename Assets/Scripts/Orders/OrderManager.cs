using Instantiated;
using Nito.Collections;
using NonVoxel;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.Linq;

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
    [SerializeField] NonVoxelManager nonVoxelManager;

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

        foreach (Order order in orderGroup.orders) {
            yield return ExecuteOrder(order);
        }

        if (cameraManager.attachedEntity == null 
                || cameraManager.attachedEntity != partyManager.currControlledCharacter) {
            yield return cameraManager.MoveCameraToTargetCreature(partyManager.currControlledCharacter);
        }
        yield return gameStateManager.SetControlState(ControlState.SPRITE_NEUTRAL);

        currCoroutine = null;
    }

    private IEnumerator ExecuteOrder(Order order) {
        Type type = order.GetType();

        if (type == typeof(ExclaimOrder)) {
            ExclaimOrder exclaimOrder = (ExclaimOrder)order;
            InstantiatedEntity exclaimer = nonVoxelWorld.GetInstanceFromID(
                exclaimOrder.travellerGuid);
            if (exclaimer == null || !TypeUtils.IsSameTypeOrIsSubclass(exclaimer, typeof(Traveller))) {
                yield break;
            }

            Traveller traveller = (Traveller)exclaimer;
            yield return effectManager.GenerateExclaimEffect(traveller);
        }
        else if (type == typeof(MoveOrder)) {
            MoveOrder moveOrder = (MoveOrder)order;
            InstantiatedEntity mover = nonVoxelWorld.GetInstanceFromID(moveOrder.travellerGuid);
            if (mover == null || !TypeUtils.IsSameTypeOrIsSubclass(mover, typeof(Traveller))) {
                yield break;
            }

            Traveller traveller = (Traveller)mover;
            CoroutineWithData coroutineWithData = new CoroutineWithData(this,
                pathfinder.FindPath(traveller, moveOrder.destination));
            yield return coroutineWithData.coroutine;
            Deque<Vector3Int> path = (Deque<Vector3Int>)coroutineWithData.result;

            if (moveOrder.waitForCompletion) {
                yield return movementManager.MoveAlongPath(traveller, path);
            }
            else {
                StartCoroutine(movementManager.MoveAlongPath(traveller, path));
            }
        }
        else if (type == typeof(CameraFocusOrder)) {
            CameraFocusOrder cameraFocusOrder = (CameraFocusOrder)order;
            InstantiatedEntity focused = nonVoxelWorld.GetInstanceFromID(
                cameraFocusOrder.focusTargetTravellerGuid);
            if (focused == null || !TypeUtils.IsSameTypeOrIsSubclass(focused, typeof(Traveller))) {
                yield break;
            }

            Traveller traveller = (Traveller)focused;
            yield return cameraManager.MoveCameraToTargetCreature(traveller);
        }
        else if (type == typeof(DialogueOrder)) {
            DialogueOrder dialogueOrder = (DialogueOrder)order;
            dialogueUI.StartDialogue(new Story(dialogueOrder.storyText), dialogueOrder);

            while (dialogueUI.IsDisplaying() || dialogueUI.HasStoryOrder()) {
                while (dialogueUI.HasStoryOrder()) {
                    Order storyOrder = dialogueUI.PopLatestStoryOrder();
                    yield return ExecuteOrder(storyOrder);
                }
                yield return null;
            }
        }
        else if (type == typeof(DestroyOrder)) {
            DestroyOrder destroyOrder = (DestroyOrder)order;
            InstantiatedEntity instantiation = nonVoxelWorld.GetInstanceFromID(destroyOrder.entityGuid);
            nonVoxelWorld.DestroyEntity(instantiation);
        }
        else if (type == typeof(DoMultipleOrder)) {
            DoMultipleOrder doMultipleOrder = (DoMultipleOrder)order;

            foreach (Order orderRecursive in doMultipleOrder.orderGroup.orders) {
                yield return ExecuteOrder(orderRecursive);
            }
        }
        else if (type == typeof(ChangeOrdersOrder)) {
            ChangeOrdersOrder changeOrdersOrder = (ChangeOrdersOrder)order;
            TangibleObject tangibleObject = (TangibleObject)nonVoxelWorld
                .GetInstanceFromID(changeOrdersOrder.orderHolderGuid);
            if (tangibleObject == null) {
                Debug.Log($"Could not change orders of {changeOrdersOrder.orderHolderGuid} because " +
                    $"it doesn't exist.");
            }
            tangibleObject.objectInfo.interactOrders = changeOrdersOrder.newOrders;
        }
        else if (type == typeof(JoinPartyOrder)) {
            JoinPartyOrder joinPartyOrder = (JoinPartyOrder)order;
            InstantiatedEntity entity = nonVoxelWorld.GetInstanceFromID(joinPartyOrder.newPartyMemberID);
            if (entity.GetType() != typeof(NPC)) {
                Debug.LogError($"Tried to add entity of type {type} to party: {entity}");
                yield break;
            }

            NPC npc = (NPC)entity;
            nonVoxelManager.ConvertNPCToPlayer(npc);
            // todo play fanfare
        }
        else if (type == typeof(CreateEntityOrder)) {
            CreateEntityOrder createEntityOrder = (CreateEntityOrder)order;

            // todo - remove use of dummy variable
            EntityDefinition.EnvChangeDestination dummy = new(0, Vector3Int.zero);
            nonVoxelManager.CreateEntities(new List<EntityDefinition.Entity>() { createEntityOrder.entity },
                dummy);
        }
        else {
            Debug.LogError($"Found an order type not implemented: {type}");
        }
    }
}
