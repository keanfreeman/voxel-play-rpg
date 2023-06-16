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
                List<Traveller> meleeEnemies = nonVoxelWorld.GetAdjacentTravellers(performer, oppositeFaction);
                if (meleeEnemies.Count < 1) {
                    messageManager.DisplayMessage("No adjacent enemies for melee attack.");
                    yield break;
                }
            }

            CoroutineWithData<HashSet<NPC>> selectedAttackCoroutine = new(this, 
                PerformSelectedAttack(performer, attack));
            yield return selectedAttackCoroutine.coroutine;
            HashSet<NPC> enemies = selectedAttackCoroutine.HasResult()
                ? selectedAttackCoroutine.GetResult() : new();
            if (enemies.Count > 0 && gameStateManager.controlState != ControlState.COMBAT) {
                EnterCombatAfterAttacking(enemies);
            }
        }
        else if (actionType == typeof(SpellSO)) {
            SpellSO spell = (SpellSO)action;

            // check spell slots remaining
            ResourceStatus spellSlots = performer.GetResources().resourceStatuses
                .GetValueOrDefault(GameMechanics.ResourceID.SpellSlots, null);
            if (spell.spellLevel > 0 && spellSlots.remainingUses < 1) {
                messageManager.DisplayMessage("No more remaining spell slots.");
                yield break;
            }

            // todo - use non-string identifier
            if (spell.actionName == "Longstrider") {
                yield return PerformLongstrider(performer, spell, spellSlots);
            }
            else if (spell.actionName == "Light") {
                yield return PerformLight(performer, spell);
            }
            else if (spell.actionName == "Mage Armor") {
                yield return PerformMageArmor(performer, spell, spellSlots);
            }
            else if (spell.actionName == "Sleep") {
                yield return PerformSleepSpell(performer, spell, spellSlots);
            }
            else if (spell.actionName == "Color Spray") {
                yield return PerformColorSpray(performer, spell, spellSlots);
            }
            else if (spell.actionName == "Burning Hands") {
                yield return PerformBurningHands(performer, spell, spellSlots);
            }
            else if (spell.actionName == "Vicious Mockery") {
                yield return PerformViciousMockery(performer, spell);
            }
            else if (spell.actionName == "Cure Wounds") {
                yield return PerformCureWounds(performer, spell, spellSlots);
            }
            else if (spell.actionName == "Healing Word") {
                yield return PerformHealingWord(performer, spell, spellSlots);
            }
        }
        else if (actionType == typeof(SpecialActionSO)) {
            SpecialActionSO specialActionSO = (SpecialActionSO)action;
            // todo - use non-string identifier
            if (specialActionSO.actionName == "Second Wind") {
                yield return PerformSecondWind(performer, specialActionSO);
            }
            if (specialActionSO.actionName == "Bardic Inspiration") {
                yield return PerformBardicInspiration(performer, specialActionSO);
            }
        }
        else {
            throw new System.NotImplementedException($"Need to implement a description for " +
                $"{action.GetType()}");
        }
    }

    private IEnumerator PerformSelectedAttack(Traveller performer, AttackSO attack) {
        messageManager.DisplayMessage(new Message(
            "Please select a creature to attack", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
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

        Tuple<Vector3Int, Vector3Int> closestPoints = performer.GetNearestPoints(npc);
        int distance = Coordinates.NumPointsBetween(closestPoints.Item1, closestPoints.Item2);
        bool isOutOfRange = attack.isRanged ? (attack.longRange / CombatManager.TILE_TO_FEET) < distance
            : distance > 1;
        if (isOutOfRange) {
            messageManager.DisplayMessage($"Target is out of attack's range ({attack.longRange} feet)");
            yield return null;
            yield break;
        }

        if (!combatManager.CreatureHasLineOfSightToCreature(performer, npc)) {
            messageManager.DisplayMessage($"No line of sight!");
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

    private IEnumerator PerformLongstrider(Traveller performer, SpellSO spell, ResourceStatus spellSlots) {
        messageManager.DisplayMessage(new Message("Please select an adjacent creature.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
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

        int selectedDistance = Coordinates.NumPointsBetween(
            performer.GetPointInEntityClosestTo(targetPosition), targetPosition);
        if (selectedDistance > 1) {
            messageManager.DisplayMessage("Must choose an adjacent creature.");
            yield break;
        }

        target.AddStatus(new OngoingEffect(StatusEffect.Longstrider, TimeUtil.HOUR));

        spellSlots.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
    }

    private IEnumerator PerformCureWounds(Traveller performer, SpellSO spell, ResourceStatus spellSlots) {
        messageManager.DisplayMessage(new Message("Please select an adjacent creature.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
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

        int selectedDistance = Coordinates.NumPointsBetween(
            performer.GetPointInEntityClosestTo(targetPosition), targetPosition);
        if (selectedDistance > 1) {
            messageManager.DisplayMessage("Must choose an adjacent creature.");
            yield break;
        }

        if (target.GetStats().creatureType == CreatureType.Construct 
                || target.GetStats().creatureType == CreatureType.Undead) {
            messageManager.DisplayMessage("Has no effect on constructs/undead.");
            yield return null;
            yield break;
        }

        CoroutineWithData<DiceResult> healRollCoroutine = new(this, visualRollManager.RollGeneric(
            "Rolling for Cure Wounds", new List<Die> { new(1, 8, 3) }));
        yield return healRollCoroutine.coroutine;
        int sum = healRollCoroutine.GetResult().sum;
        messageManager.DisplayMessage($"Rolled {sum} HP!");

        yield return target.RecoverHP(sum);

        spellSlots.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
    }

    private IEnumerator PerformHealingWord(Traveller performer, SpellSO spell, ResourceStatus spellSlots) {
        messageManager.DisplayMessage(new Message("Please select a creature in range.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
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

        int healingWordRangeInPoints = 60 / 5;
        Vector3Int closestTravellerPoint = performer.GetPointInEntityClosestTo(targetPosition);
        int selectedDistance = Coordinates.NumPointsBetween(closestTravellerPoint, targetPosition);
        if (selectedDistance > healingWordRangeInPoints) {
            messageManager.DisplayMessage("Selected a position out of range.");
            yield break;
        }

        if (!combatManager.CreatureHasLineOfSightToCreature(performer, target)) {
            messageManager.DisplayMessage($"No line of sight!");
            yield return null;
            yield break;
        }

        if (target.GetStats().creatureType == CreatureType.Construct
                || target.GetStats().creatureType == CreatureType.Undead) {
            messageManager.DisplayMessage("Has no effect on constructs/undead.");
            yield return null;
            yield break;
        }

        CoroutineWithData<DiceResult> healRollCoroutine = new(this, visualRollManager.RollGeneric(
            "Rolling for Healing Word", new List<Die> { new(1, 4, 3) }));
        yield return healRollCoroutine.coroutine;
        int sum = healRollCoroutine.GetResult().sum;
        messageManager.DisplayMessage($"Rolled {sum} HP!");

        yield return target.RecoverHP(sum);

        spellSlots.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
    }

    private IEnumerator PerformSleepSpell(Traveller performer, SpellSO spell, ResourceStatus spellSlots) {
        int sleepRadius = 20;
        int radiusInPoints = sleepRadius / 5;
        messageManager.DisplayMessage(new Message("Please select a point in range.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer, 
            SelectModeShape.Sphere, radiusInPoints));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessages();
        if (!cwd.HasResult()) {
            messageManager.DisplayMessage("Cancelled selection.");
            yield break;
        }
        Vector3Int targetPosition = cwd.GetResult();

        int sleepRangeInPoints = 90 / 5;
        Vector3Int closestTravellerPoint = performer.GetPointInEntityClosestTo(targetPosition);
        int selectedDistance = Coordinates.NumPointsBetween(closestTravellerPoint, targetPosition);
        if (selectedDistance > sleepRangeInPoints) {
            messageManager.DisplayMessage("Selected a position out of range.");
            yield break;
        }

        if (!combatManager.CreatureHasLineOfSight(performer, targetPosition)) {
            messageManager.DisplayMessage($"No line of sight!");
            yield return null;
            yield break;
        }

        // get creatures in radius
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
        messageManager.DisplayMessage($"Rolled {sleepSum} HP for Sleep!");

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

        spellSlots.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }

        List<NPC> affectedEnemies = affectedCreatures
            .Where((Traveller traveller) => traveller.GetType() == typeof(NPC))
            .Select((Traveller traveller) => (NPC)traveller)
            .ToList();
        if (affectedEnemies.Count > 0) {
            EnterCombatAfterAttacking(affectedEnemies[0].teammates);
        }
    }

    private IEnumerator PerformBurningHands(Traveller performer, SpellSO spell, ResourceStatus spellSlots) {
        int burningHandsConeLength = 15;
        int lengthInPoints = burningHandsConeLength / 5;
        messageManager.DisplayMessage(new Message("Please select a point in range.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer,
            SelectModeShape.Cone, lengthInPoints));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessages();
        if (!cwd.HasResult()) {
            messageManager.DisplayMessage("Cancelled selection.");
            yield break;
        }
        Vector3Int targetPosition = cwd.GetResult();

        // get creatures in radius
        List<Vector3Int> pointsInCone = Coordinates.GetPointsInCone(performer, targetPosition,
            lengthInPoints);
        HashSet<Traveller> affectedCreatures = nonVoxelWorld.GetTravellersInPoints(pointsInCone);
        if (affectedCreatures.Count < 1) {
            messageManager.DisplayMessage("No creatures in radius.");
            yield break;
        }

        // roll damage, then check saves
        CoroutineWithData<int> damageCoroutine = new(this, visualRollManager.RollDamage(
                new List<Die> { new(3, 6) }, isCritical: false));
        yield return damageCoroutine.coroutine;
        int damageSum = damageCoroutine.GetResult();

        int saveDC = performer.GetStats().GetSpellcastingFeature().spellSaveDC;
        foreach (Traveller affectedCreature in affectedCreatures) {
            int dexMod = StatModifiers.GetModifierForStat(affectedCreature.GetStats().dexterity);
            CoroutineWithData<int> burningHandsCoroutine = new(this, visualRollManager.RollSavingThrow(
                affectedCreature, "Rolling Dexterity saving throw.", dexMod, saveDC));
            yield return burningHandsCoroutine.coroutine;
            int saveResult = burningHandsCoroutine.GetResult();

            int damageAmount = damageSum;
            if (saveResult < saveDC) {
                damageAmount /= 2;
            }
            yield return performer.DealDamage(spell, new Damage(DamageType.Fire, damageAmount), 
                affectedCreature);
        }

        spellSlots.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }

        List<NPC> affectedEnemies = affectedCreatures
            .Where((Traveller traveller) => traveller.GetType() == typeof(NPC))
            .Select((Traveller traveller) => (NPC)traveller)
            .ToList();
        if (affectedEnemies.Count > 0) {
            EnterCombatAfterAttacking(affectedEnemies[0].teammates);
        }
    }

    private IEnumerator PerformColorSpray(Traveller performer, SpellSO spell, ResourceStatus spellSlots) {
        int colorSprayConeLength = 15;
        int lengthInPoints = colorSprayConeLength / 5;
        messageManager.DisplayMessage(new Message("Please select a point in range.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer,
            SelectModeShape.Cone, lengthInPoints));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessages();
        if (!cwd.HasResult()) {
            messageManager.DisplayMessage("Cancelled selection.");
            yield break;
        }
        Vector3Int targetPosition = cwd.GetResult();

        // get creatures in radius
        List<Vector3Int> pointsInCone = Coordinates.GetPointsInCone(performer, targetPosition,
            lengthInPoints);
        HashSet<Traveller> affectedCreatures = nonVoxelWorld.GetTravellersInPoints(pointsInCone);

        if (affectedCreatures.Count < 1) {
            messageManager.DisplayMessage("No creatures in radius.");
            yield break;
        }

        // roll HP amount
        CoroutineWithData<DiceResult> colorSprayCoroutine = new(this, visualRollManager.RollGeneric(
            "Rolling for Color Spray", new List<Die> { new(6, 10) }));
        yield return colorSprayCoroutine.coroutine;
        int colorSpraySum = colorSprayCoroutine.GetResult().sum;
        messageManager.DisplayMessage($"Rolled {colorSpraySum} HP for Color Spray.");

        List<Traveller> sorted = affectedCreatures.ToList();
        sorted.Sort((x, y) => -x.CurrHP.CompareTo(y.CurrHP));
        foreach (Traveller affectedCreature in sorted) {
            if (colorSpraySum < affectedCreature.CurrHP) {
                break;
            }

            colorSpraySum -= affectedCreature.CurrHP;

            // todo - do not affect unconscious creatures / creatures who can't see
            // todo - make it possible to specify when a status ends beyond raw time (e.g.
            // start of X creature's turn)
            affectedCreature.AddStatus(new OngoingEffect(StatusEffect.ColorSpray,
                new HashSet<Condition> { Condition.Blinded }, TimeUtil.MINUTE));
            performer.onCombatTurnStart += featureManager.EndColorSpray;
        }

        // todo - play debuff effect

        spellSlots.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }

        List<NPC> affectedEnemies = affectedCreatures
            .Where((Traveller traveller) => traveller.GetType() == typeof(NPC))
            .Select((Traveller traveller) => (NPC)traveller)
            .ToList();
        if (affectedEnemies.Count > 0) {
            EnterCombatAfterAttacking(affectedEnemies[0].teammates);
        }
    }

    private IEnumerator PerformMageArmor(Traveller performer, SpellSO spell, ResourceStatus spellSlots) {
        messageManager.DisplayMessage(new Message("Please select a willing creature in range.", 
            isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
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

        int selectedDistance = Coordinates.NumPointsBetween(
            performer.GetPointInEntityClosestTo(targetPosition), targetPosition);
        if (selectedDistance > 1) {
            messageManager.DisplayMessage("Must choose an adjacent creature.");
            yield break;
        }

        // todo - use time specified in spell
        target.AddStatus(new OngoingEffect(StatusEffect.MageArmor, TimeUtil.HOUR * 8));

        spellSlots.DecrementUses();
        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
    }

    private IEnumerator PerformViciousMockery(Traveller performer, SpellSO spell) {
        messageManager.DisplayMessage(new Message("Please select a creature in range.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessages();
        if (!cwd.HasResult()) {
            messageManager.DisplayMessage("Cancelled selection.");
            yield return null;
            yield break;
        }
        Vector3Int targetPosition = cwd.GetResult();

        // todo - do things like range enforcement at a higher level so I don't have to check per-spell.
        // will need to have range broken out of string, which is difficult
        int viciousMockeryRangeInPoints = 60 / 5;
        Vector3Int closestTravellerPoint = performer.GetPointInEntityClosestTo(targetPosition);
        int selectedDistance = Coordinates.NumPointsBetween(closestTravellerPoint, targetPosition);
        if (selectedDistance > viciousMockeryRangeInPoints) {
            messageManager.DisplayMessage("Selected a position out of range.");
            yield break;
        }

        InstantiatedEntity entity = nonVoxelWorld.GetEntityFromPosition(targetPosition);
        if (entity == null || !TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(Traveller))) {
            messageManager.DisplayMessage("Must choose a creature as a target.");
            yield return null;
            yield break;
        }
        Traveller target = (Traveller)entity;

        if (!combatManager.CreatureHasLineOfSightToCreature(performer, target)) {
            messageManager.DisplayMessage($"No line of sight!");
            yield return null;
            yield break;
        }

        // todo - enforce hearing requirement

        int wisMod = StatModifiers.GetModifierForStat(target.GetStats().wisdom);
        int dc = performer.GetStats().GetSpellcastingFeature().spellSaveDC;
        CoroutineWithData<int> wisSaveCoroutine = new(this, visualRollManager.RollSavingThrow(
                target, "Rolling saving throw for Vicious Mockery", wisMod, dc));
        yield return wisSaveCoroutine.coroutine;
        int rollResult = wisSaveCoroutine.GetResult();

        if (rollResult < dc) {
            CoroutineWithData<int> damageCoroutine = new(this, visualRollManager
                .RollDamage(new List<Die> { new(1, 4) }));
            yield return damageCoroutine.coroutine;
            int totalDamage = damageCoroutine.GetResult();

            yield return performer.DealDamage(spell, new Damage(DamageType.Psychic, totalDamage), target);
            target.AddStatus(new OngoingEffect(StatusEffect.ViciousMockery, TimeUtil.MINUTE));
            target.onCombatTurnEnd += featureManager.EndViciousMockeryTurnEnd;
            target.onAttackRollFinished += featureManager.EndViciousMockeryAttackPerformed;
        }

        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, spell.actionType);
        }
        if (target.GetType() == typeof(NPC)) {
            EnterCombatAfterAttacking(((NPC)target).teammates);
        }
    }

    private IEnumerator PerformLight(Traveller performer, SpellSO spell) {
        messageManager.DisplayMessage(new Message("Please select an adjacent creature.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
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

        int selectedDistance = Coordinates.NumPointsBetween(performer.GetPointInEntityClosestTo(targetPosition), 
            targetPosition);
        if (selectedDistance > 1) {
            messageManager.DisplayMessage("Must choose an adjacent creature.");
            yield break;
        }

        if (target.GetFaction() != EntityDefinition.Faction.PLAYER) {
            int spellSaveDC = performer.GetStats().GetSpellcastingFeature().spellSaveDC;
            CoroutineWithData<int> dexSaveCoroutine = new(this, visualRollManager.RollSavingThrow(
                target, "Rolling saving throw for Light", 
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

    private IEnumerator PerformSecondWind(Traveller performer, SpecialActionSO specialActionSO) {
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
            combatManager.CombatResources.ConsumeResource(performer, specialActionSO.actionType);
        }

        // todo - scale with fighter level
        CoroutineWithData<DiceResult> secondWindCoroutine = new(this, visualRollManager.RollGeneric(
            "Rolling for Second Wind", new List<Die> { new(1, 10, 1) }));
        yield return secondWindCoroutine.coroutine;
        DiceResult result = secondWindCoroutine.GetResult();

        // todo - display healing effect
        int newHP = Mathf.Min(maxHP, initialHP + result.sum);
        int gained = newHP - initialHP;
        yield return performer.RecoverHP(gained);

        messageManager.DisplayMessage($"Recovered {gained} hit points!");

        inputManager.UnlockUIControls(combatUI);
    }

    private IEnumerator PerformBardicInspiration(Traveller performer, SpecialActionSO specialActionSO) {
        ResourceStatus bardicInspirationResource = performer.GetResources().resourceStatuses
            .GetValueOrDefault(GameMechanics.ResourceID.BardicInspiration, null);
        if (bardicInspirationResource.remainingUses < 1) {
            messageManager.DisplayMessage("Ran out of uses of Bardic Inspiration.");
            yield break;
        }

        messageManager.DisplayMessage(new Message("Please select a creature in range.", isPermanent: true));
        CoroutineWithData<Vector3Int> cwd = new(this, detachedCamera.EnterSelectMode(performer));
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

        int bardicInspirationRangeInPoints = 60 / 5;
        Vector3Int closestTravellerPoint = performer.GetPointInEntityClosestTo(targetPosition);
        int selectedDistance = Coordinates.NumPointsBetween(closestTravellerPoint, targetPosition);
        if (selectedDistance > bardicInspirationRangeInPoints) {
            messageManager.DisplayMessage("Selected a position out of range.");
            yield break;
        }

        // apply a status
        target.AddStatus(new OngoingEffect(StatusEffect.BardicInspiration, TimeUtil.MINUTE * 10));
        performer.GetResources().DeductUses(GameMechanics.ResourceID.BardicInspiration);

        if (gameStateManager.controlState == ControlState.COMBAT) {
            combatManager.CombatResources.ConsumeResource(performer, specialActionSO.actionType);
        }
    }
}
