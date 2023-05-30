using Combat;
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
    [SerializeField] FeatureManager featureManager;

    public IEnumerator PerformAction(Traveller performer, ActionSO action) {
        if (gameStateManager.controlState == ControlState.COMBAT
                && !combatManager.CombatResources.HasResource(performer, action.actionType)) {
            messageManager.DisplayMessage($"Cannot use this resource twice: {action.actionType}");
            yield break;
        }

        // todo - unify this logic with combatmanager
        Condition? preventActionCondition = performer.HasCondition(Condition.Paralyzed)
            ? Condition.Paralyzed : performer.HasCondition(Condition.Unconscious)
            ? Condition.Unconscious
            : null;
        if (preventActionCondition.HasValue) {
            // todo - show an effect and disable UI components.
            messageManager.DisplayMessage($"Player is {preventActionCondition.Value} and cannot act.");
            yield break;
        }

        Type actionType = action.GetType();
        if (actionType == typeof(AttackSO) || (actionType == typeof(SpellSO) 
                && ((SpellSO)action).IsSpellAttack())) {
            AttackSO attack = actionType == typeof(AttackSO)
                ? (AttackSO)action : ((SpellSO)action).providedAttack;

            if (!attack.isRanged) {
                if (gameStateManager.controlState != ControlState.COMBAT) {
                    messageManager.DisplayMessage("Cannot perform melee attack outside of combat.");
                    yield break;
                }
                EntityDefinition.Faction oppositeFaction = performer.GetFaction() 
                    == EntityDefinition.Faction.PLAYER ? EntityDefinition.Faction.ENEMY 
                    : EntityDefinition.Faction.PLAYER;
                if (nonVoxelWorld.GetAdjacentTravellers(performer, oppositeFaction).Count < 1) {
                    messageManager.DisplayMessage("No adjacent enemies for melee attack.");
                    yield break;
                }
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
                yield return PerformLongstrider(performer, spell);
            }
            else if (spell.actionName == "Light") {
                yield return PerformLight(performer, spell);
            }
            else if (spell.actionName == "Mage Armor") {
                yield return PerformMageArmor(performer, spell);
            }
            else if (spell.actionName == "Sleep") {
                yield return PerformSleep(performer, spell);
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

    private IEnumerator PerformLongstrider(Traveller performer, SpellSO spell) {
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
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
    }

    private IEnumerator PerformSleep(Traveller performer, SpellSO spell) {
        ResourceStatus resourceStatus = performer.GetResources().resourceStatuses
            .GetValueOrDefault(GameMechanics.ResourceID.SpellSlots, null);
        if (resourceStatus.remainingUses < 1) {
            messageManager.DisplayMessage("No more remaining spell slots.");
            yield break;
        }

        // todo - show radius preview
        messageManager.DisplayMessage(new Message("Please select a point in range.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer.origin));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessages();
        if (!cwd.HasResult()) {
            messageManager.DisplayMessage("Cancelled selection.");
            yield break;
        }
        Vector3Int targetPosition = cwd.GetResult();

        // get creatures in radius
        int sleepRadius = 20;
        int radiusInPoints = sleepRadius / 5;
        List<Vector3Int> pointsInSphere = Coordinates.GetPointsInSphereCenteredOn(targetPosition, 
            radiusInPoints);
        HashSet<Traveller> affectedCreatures = nonVoxelWorld.GetTravellersInPoints(pointsInSphere);

        if (affectedCreatures.Count < 1) {
            messageManager.DisplayMessage("No creatures in radius.");
            yield break;
        }

        // roll HP amount
        CoroutineWithData<DiceResult> sleepCoroutine = new(this, visualRollManager.RollGeneric(
            "Rolling for Sleep", new List<Die> { new(5, 8) }));
        yield return sleepCoroutine.coroutine;
        int sleepSum = sleepCoroutine.GetResult().sum;

        List<Traveller> sorted = affectedCreatures.ToList();
        sorted.Sort((x, y) => -x.CurrHP.CompareTo(y.CurrHP));
        foreach (Traveller affectedCreature in sorted) {
            if (sleepSum < affectedCreature.CurrHP) {
                break;
            }

            sleepSum -= affectedCreature.CurrHP;

            // todo - do not affect undead / creatures immune to charmed
            affectedCreature.AddStatus(new OngoingEffect(StatusEffect.SleepSpell,
                new HashSet<Condition> { Condition.Unconscious }, TimeUtil.MINUTE));
            affectedCreature.onTakeDamage += featureManager.CheckSleepSpellEnd;
        }

        // todo - play debuff effect

        resourceStatus.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
    }

    private IEnumerator PerformMageArmor(Traveller performer, SpellSO spell) {
        ResourceStatus spellSlotResource = performer.GetResources().resourceStatuses
            .GetValueOrDefault(GameMechanics.ResourceID.SpellSlots, null);
        if (spellSlotResource.remainingUses < 1) {
            messageManager.DisplayMessage("No more remaining spell slots.");
            yield break;
        }

        messageManager.DisplayMessage(new Message("Please select a willing creature in range.", 
            isPermanent: true));
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

        if (target.GetFaction() != performer.GetFaction()) {
            messageManager.DisplayMessage("Must choose a willing creature.");
            yield return null;
            yield break;
        }

        // todo - use time specified in spell
        target.AddStatus(new OngoingEffect(StatusEffect.MageArmor, TimeUtil.HOUR * 8));

        spellSlotResource.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
    }

    private IEnumerator PerformLight(Traveller performer, SpellSO spell) {
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

        if (target.GetFaction() != EntityDefinition.Faction.PLAYER) {
            int spellSaveDC = performer.GetStats().GetSpellcastingFeature().spellSaveDC;
            CoroutineWithData<int> dexSaveCoroutine = new(this, visualRollManager.RollSavingThrow(
                "Rolling saving throw for Light", 
                StatModifiers.GetModifierForStat(target.GetStats().dexterity), spellSaveDC));
            yield return dexSaveCoroutine.coroutine;
            int result = dexSaveCoroutine.GetResult();
            if (result < spellSaveDC) {
                messageManager.DisplayMessage("Enemy saved against Light!");
                yield break;
            }
        }

        // apply a status
        target.AddStatus(new OngoingEffect(StatusEffect.Light, TimeUtil.HOUR));

        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
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
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, ActionType.BonusAction);
        }

        // todo - scale with fighter level
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
