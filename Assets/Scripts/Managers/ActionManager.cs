using DieNamespace;
using GameMechanics;
using Instantiated;
using NonVoxel;
using Spells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    [SerializeField] MessageManager messageManager;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] CombatManager combatManager;
    [SerializeField] VisualRollManager visualRollManager;
    [SerializeField] InputManager inputManager;
    [SerializeField] CombatUI combatUI;

    public IEnumerator PerformAction(Traveller performer, ActionSO action) {
        Type actionType = action.GetType();
        if (actionType == typeof(AttackSO) || (actionType == typeof(SpellSO) 
                && ((SpellSO)action).IsSpellAttack())) {
            AttackSO attack = actionType == typeof(AttackSO)
                ? (AttackSO)action : ((SpellSO)action).providedAttack;

            if (!attack.isRanged && gameStateManager.controlState != ControlState.COMBAT) {
                messageManager.DisplayMessage("Cannot perform melee attack outside of combat.");
                yield break;
            }

            CoroutineWithData<HashSet<NPC>> rangedAttackCoroutine = new(this, 
                PerformRangedAttack(performer, attack));
            yield return rangedAttackCoroutine.coroutine;
            HashSet<NPC> enemies = rangedAttackCoroutine.HasResult()
                ? rangedAttackCoroutine.GetResult() : new();
            if (enemies.Count > 0 && gameStateManager.controlState != ControlState.COMBAT) {
                EnterCombatAfterAttacking(enemies);
            }
        }
        else if (actionType == typeof(SpellSO)) {
            SpellSO spell = (SpellSO)action;
            // todo - use non-string identifier
            if (spell.actionName == "Longstrider") {
                yield return PerformLongstrider(performer);
            }
        }
        else if (actionType == typeof(SpecialActionSO)) {
            SpecialActionSO specialActionSO = (SpecialActionSO)action;
            // todo - use non-string identifier
            if (specialActionSO.actionName == "Second Wind") {
                yield return PerformSecondWind(performer);
            }
        }
        else {
            throw new System.NotImplementedException($"Need to implement a description for " +
                $"{action.GetType()}");
        }
    }

    private IEnumerator PerformRangedAttack(Traveller performer, AttackSO attack) {
        messageManager.DisplayMessage(new Message(
            "Please select a creature to attack", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer.origin));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessages();
        if (!cwd.HasResult()) {
            messageManager.DisplayMessage("Cancelled selection.");
            yield return null;
            yield break;
        }
        Vector3Int targetPosition = cwd.GetResult();

        InstantiatedEntity entity = nonVoxelWorld.GetEntityFromPosition(targetPosition);
        if (entity == null || !TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(NPC))) {
            messageManager.DisplayMessage("Must choose an NPC as a target.");
            yield return null;
            yield break;
        }

        NPC npc = (NPC)entity;
        if (npc.GetFaction() != EntityDefinition.Faction.ENEMY) {
            messageManager.DisplayMessage("Cannot target a non-enemy NPC for an attack.");
            yield return null;
            yield break;
        }
        HashSet<NPC> enemies = npc.teammates;
        yield return combatManager.PerformAttack(performer, attack, npc);
        if (!nonVoxelWorld.npcs.Contains(npc)) {
            // NPC was killed
            enemies.Remove(npc);
        }
        yield return enemies;
    }

    private void EnterCombatAfterAttacking(ICollection<NPC> enemies) {
        combatManager.SetEnemies(enemies);
        gameStateManager.EnterCombat(null);
        combatManager.StartCombat();
    }

    private IEnumerator PerformLongstrider(Traveller performer) {
        ResourceStatus resourceStatus = performer.GetResources().resourceStatuses
            .GetValueOrDefault(GameMechanics.ResourceID.SpellSlots, null);
        if (resourceStatus.remainingUses < 1) {
            messageManager.DisplayMessage("No more remaining spell slots.");
            yield break;
        }

        // get a target
        messageManager.DisplayMessage(new Message("Please select an adjacent creature.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer.origin));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessages();
        if (!cwd.HasResult()) {
            messageManager.DisplayMessage("Cancelled selection.");
            yield return null;
            yield break;
        }
        Vector3Int targetPosition = cwd.GetResult();

        InstantiatedEntity entity = nonVoxelWorld.GetEntityFromPosition(targetPosition);
        if (entity == null || !TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(Traveller))) {
            messageManager.DisplayMessage("Must choose a creature as a target.");
            yield return null;
            yield break;
        }
        Traveller target = (Traveller)entity;

        // apply a status
        target.AddStatus(new OngoingEffect(StatusEffect.Longstrider, TimeUtil.HOUR));

        resourceStatus.DecrementUses();
    }

    private IEnumerator PerformSecondWind(Traveller performer) {
        ResourceStatus resourceStatus = performer.GetResources().resourceStatuses
            .GetValueOrDefault(GameMechanics.ResourceID.SecondWind, null);
        if (resourceStatus.remainingUses < 1) {
            messageManager.DisplayMessage("Ran out of uses of Second Wind.");
            yield break;
        }

        int maxHP = performer.GetStats().maxHP;
        int initialHP = performer.CurrHP;
        if (initialHP == maxHP) {
            messageManager.DisplayMessage("You have no need to use Second Wind with full HP.");
            yield break;
        }

        inputManager.LockUIControls();

        resourceStatus.DecrementUses();

        CoroutineWithData<DiceResult> secondWindCoroutine = new(this, visualRollManager.RollGeneric(
            "Rolling for Second Wind", new List<Die> { new(1, 10, 1) }));
        yield return secondWindCoroutine.coroutine;
        DiceResult result = secondWindCoroutine.GetResult();

        // todo - display healing effect
        int newHP = Mathf.Min(maxHP, initialHP + result.sum);
        int gained = newHP - initialHP;

        messageManager.DisplayMessage($"Recovered {gained} hit points!");

        inputManager.UnlockUIControls(combatUI);
    }
}
