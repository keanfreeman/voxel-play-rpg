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
    [SerializeField] PromptUIController promptUIController;
    [SerializeField] TimerUIController timerUIController;
    [SerializeField] MessageManager messageManager;
    [SerializeField] VisualRollManager visualRollManager;

    private Coroutine currCoroutine = null;

    public void ClearData() {
        currCoroutine = null;
    }

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
            if (moveOrder.travellerGuid == Guid.Empty) {
                mover = partyManager.currControlledCharacter;
            }
            if (mover == null || !TypeUtils.IsSameTypeOrIsSubclass(mover, typeof(Traveller))) {
                yield break;
            }

            Traveller traveller = (Traveller)mover;
            CoroutineWithData<Deque<Vector3Int>> coroutineWithData = new(this,
                pathfinder.FindPath(traveller, moveOrder.destination));
            yield return coroutineWithData.coroutine;
            Deque<Vector3Int> path = coroutineWithData.GetResult();

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
            if (traveller.GetType() == typeof(PlayerCharacter)) {
                cameraManager.AttachCameraToPlayer((PlayerCharacter)traveller);
            }
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
            TangibleEntity tangibleEntity = (TangibleEntity)nonVoxelWorld
                .GetInstanceFromID(changeOrdersOrder.orderHolderGuid);
            if (tangibleEntity == null) {
                Debug.Log($"Could not change orders of {changeOrdersOrder.orderHolderGuid} because " +
                    $"it doesn't exist.");
            }
            tangibleEntity.SetInteractionOrders(changeOrdersOrder.newOrders);
        }
        else if (type == typeof(JoinPartyOrder)) {
            JoinPartyOrder joinPartyOrder = (JoinPartyOrder)order;
            InstantiatedEntity instance = nonVoxelWorld.GetInstanceFromID(joinPartyOrder.newPartyMemberID);
            if (instance.GetType() != typeof(NPC)) {
                Debug.LogError($"Tried to add entity of type {type} to party: {instance}");
                yield break;
            }

            // detach camera from creature if it's about to be destroyed.
            bool isAttached = false;
            if (cameraManager.GetMainCameraTarget() == instance.gameObject) {
                cameraManager.DeParentCamera();
                isAttached = true;
            }
            cameraManager.DeParentCamera();

            NPC npc = (NPC)instance;
            PlayerCharacter pc = nonVoxelManager.ConvertNPCToPlayer(npc);
            if (isAttached) {
                cameraManager.AttachCameraToPlayer(pc);
            }

            // todo play fanfare
        }
        else if (type == typeof(CreateEntityOrder)) {
            CreateEntityOrder createEntityOrder = (CreateEntityOrder)order;

            // todo - remove use of dummy variable
            EntityDefinition.EnvChangeDestination dummy = new(0, Vector3Int.zero);
            nonVoxelManager.CreateEntities(new List<EntityDefinition.Entity>() { createEntityOrder.entity },
                dummy);
        }
        else if (type == typeof(PromptRestOrder)) {
            if (partyManager.usedShortRest) {
                messageManager.DisplayMessage("Already used your short rest.");
                yield break;
            }

            CoroutineWithData<bool> cwd = new(this, promptUIController.DisplayPrompt(
                "Short Rest", "Would you like to perform a short rest to recover resources? " +
                "You may only do this once. This will cost 1 hour.",
                ControlState.FOLLOWING_ORDERS));
            yield return cwd.coroutine;
            bool respondedYes = cwd.GetResult();
            if (!respondedYes) {
                yield break;
            }

            partyManager.usedShortRest = true;
            timerUIController.DeductHours(1);
            foreach (PlayerCharacter pc in partyManager.partyMembers) {
                pc.GetResources().ResetForShortRest();
                if (pc.CurrHP < pc.GetStats().maxHP) {
                    CoroutineWithData<DiceResult> coroutineWithData = new(this,
                        visualRollManager.RollGeneric("Rolling hit dice.",
                        new List<DieNamespace.Die> { pc.GetStats().hitDice }));
                    yield return coroutineWithData.coroutine;
                    DiceResult rollResult = coroutineWithData.GetResult();
                    int hpHeal = rollResult.sum + StatModifiers.GetModifierForStat(pc.GetStats().constitution);

                    yield return pc.RecoverHP(hpHeal);
                }

                if (pc.GetStats().HasFeature(GameMechanics.FeatureID.ArcaneRecovery)) {
                    pc.GetResources().GetResource(GameMechanics.ResourceID.SpellSlots).IncrementUses();
                    messageManager.DisplayMessage("Wizard recovered a spell slot from Arcane Recovery!");
                }
            }
        }
        else if (type == typeof(MoveImmediateOrder)) {
            MoveImmediateOrder moveImmediateOrder = (MoveImmediateOrder)order;
            List<Traveller> toMove = new();
            if (moveImmediateOrder.moveOrderType == MoveOrderType.Party) {
                toMove.AddRange(partyManager.partyMembers);
            }
            else {
                Traveller target = (Traveller)nonVoxelWorld.GetInstanceFromID(
                    moveImmediateOrder.travellerGuid);
                toMove.Add(target);
            }

            Vector3Int currTargetPosition = moveImmediateOrder.destination;
            foreach (Traveller traveller in toMove) {
                traveller.MoveOriginImmediately(currTargetPosition);
                currTargetPosition += Vector3Int.right;
            }

            // todo - play transition effects
        }
        else {
            Debug.LogError($"Found an order type not implemented: {type}");
        }
    }
}
