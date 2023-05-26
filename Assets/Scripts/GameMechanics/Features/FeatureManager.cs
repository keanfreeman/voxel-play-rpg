using DieNamespace;
using GameMechanics;
using Instantiated;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static RandomManager;

public class FeatureManager : MonoBehaviour {
    [SerializeField] RandomManager randomManager;
    [SerializeField] EffectManager effectManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] CombatManager combatManager;
    [SerializeField] VisualRollManager visualRollManager;
    [SerializeField] MessageManager messageManager;

    public void SetUpFeatures(Traveller traveller) {
        foreach (FeatureSO feature in traveller.GetStats().features) {
            switch (feature.id) {
                case FeatureID.PackTactics:
                    traveller.onPerformAttack += TriggerPackTactics;
                    break;
                case FeatureID.UndeadFortitude:
                    traveller.onHPChanged += TriggerUndeadFortitude;
                    break;
                case FeatureID.FightingStyleDefense:
                    // todo - make this feature work dynamically
                    break;
                case FeatureID.SecondWind:
                    break;
                case FeatureID.SneakAttack:
                    traveller.onAttackHit += TriggerSneakAttack;
                    break;
                default:
                    throw new System.NotImplementedException($"Did not implement traveller " +
                        $"feature {feature.id}.");
            }
        }

        foreach (ActionSO action in traveller.GetStats().actions) {
            if (action.GetType() == typeof(AttackSO)) {
                AttackSO attack = (AttackSO)action;
                switch (attack.attackFeature) {
                    case AttackFeature.GhoulClaws:
                        traveller.afterDamageDealt += ApplyGhoulClaw;
                        break;
                    case AttackFeature.ShockingGrasp:
                        // todo implement
                        break;
                    case AttackFeature.None:
                        break;
                    default:
                        throw new System.NotImplementedException($"Did not implement attack " +
                            $"feature {attack.attackFeature}.");
                }
            }
        }

        // features for all creatures
        traveller.onPerformAttack += CheckParalyzedAdvantage;
        traveller.onAttackHit += CheckParalyzedCriticalHit;
    }

    // attack is unused
    private IEnumerator TriggerSneakAttack(Traveller target, Advantage advantageState) {
        bool shouldTriggerSneakAttack = false;

        // check if an enemy is next to the target
        if (advantageState == Advantage.Advantage) {
            shouldTriggerSneakAttack = true;
        }
        else if (advantageState != Advantage.Disadvantage) {
            EntityDefinition.Faction targetFaction = target.GetFaction();
            HashSet<Vector3Int> adjacent = Coordinates.GetPositionsSurroundingTraveller(target);
            foreach (Vector3Int position in adjacent) {
                InstantiatedEntity instantiatedEntity = nonVoxelWorld.GetEntityFromPosition(position);
                if (instantiatedEntity != null
                        && TypeUtils.IsSameTypeOrIsSubclass(instantiatedEntity, typeof(Traveller))
                        && target != instantiatedEntity) {
                    Traveller adjacentTraveller = (Traveller)instantiatedEntity;
                    // todo - check that this person is not incapacitated
                    if (adjacentTraveller.GetFaction() != targetFaction) {
                        shouldTriggerSneakAttack = true;
                        break;
                    }
                }
            }
        }

        if (shouldTriggerSneakAttack) {
            messageManager.DisplayMessage("Rogue's sneak attack triggered!");
            Die sneakAttackBonus = new(1, 6);
            yield return new AttackHitModifications() { bonusDamage = new() { sneakAttackBonus } };
        }
        else {
            yield return new AttackHitModifications();
        }
    }

    private IEnumerator CheckParalyzedCriticalHit(Traveller target, Advantage _) {
        yield return new AttackHitModifications() { isNewlyCritical = target.HasCondition(Condition.Paralyzed) };
    }

    private Advantage CheckParalyzedAdvantage(Traveller attacker, Traveller target, Advantage currAdvState) {
        if (target.HasCondition(Condition.Paralyzed)) {
            Debug.Log("Attacker got advantage on a paralyzed creature.");
            return AdvantageCalcs.GetNewAdvantageState(currAdvState, Advantage.Advantage);
        }
        return currAdvState;
    }

    // todo - should not work on elves or undead
    private IEnumerator ApplyGhoulClaw(AttackSO attack, Traveller target) {
        // todo - implement so I don't have to check the attack type is correct
        if (attack.attackFeature != AttackFeature.GhoulClaws) {
            yield break;
        }

        const int DC = 10;
        int modifier = StatModifiers.GetModifierForStat(target.GetStats().constitution);

        CoroutineWithData<int> savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            "Rolling CON saving throw for Ghoul Claw", modifier, DC));
        yield return savingThrowCoroutine.coroutine;
        int roll = savingThrowCoroutine.GetResult();

        if (roll < DC) {
            OngoingEffect ghoulStatusEffect = target.GetStatus(StatusEffect.GhoulClaw);
            if (ghoulStatusEffect == null) {
                target.AddStatus(new OngoingEffect(StatusEffect.GhoulClaw, TimeUtil.MINUTE, 
                    new List<Condition> { Condition.Paralyzed}));
                target.onCombatTurnEnd += CheckGhoulClawEnd;
            }
            else {
                // reset timer if already affected
                ghoulStatusEffect.secondsLeft = TimeUtil.MINUTE;
            }
        }
    }

    private IEnumerator CheckGhoulClawEnd(Traveller currTurnTraveller) {
        OngoingEffect ghoulStatusEffect = currTurnTraveller.GetStatus(StatusEffect.GhoulClaw);
        if (ghoulStatusEffect == null) {
            yield break;
        }

        const int DC = 10;
        int modifier = StatModifiers.GetModifierForStat(currTurnTraveller.GetStats().constitution);

        CoroutineWithData<int> savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            "Rolling CON saving throw to end Paralysis from Ghoul Claw", modifier, 10));
        yield return savingThrowCoroutine.coroutine;
        int roll = savingThrowCoroutine.GetResult();

        if (roll >= DC || ghoulStatusEffect.secondsLeft <= TimeUtil.ONE_TURN) {
            currTurnTraveller.RemoveStatus(StatusEffect.GhoulClaw);
            currTurnTraveller.onCombatTurnEnd -= CheckGhoulClawEnd;
        }
        else {
            ghoulStatusEffect.secondsLeft -= TimeUtil.ONE_TURN;
        }
    }

    public Advantage TriggerPackTactics(Traveller attacker, Traveller target, Advantage currAdvState) {
        Advantage nextAdvantageState = currAdvState;
        
        EntityDefinition.Faction attackerFaction = attacker.GetFaction();

        HashSet<Vector3Int> adjacent = Coordinates.GetPositionsSurroundingTraveller(target);
        foreach (Vector3Int position in adjacent) {
            InstantiatedEntity instantiatedEntity = nonVoxelWorld.GetEntityFromPosition(position);
            if (instantiatedEntity != null 
                    && TypeUtils.IsSameTypeOrIsSubclass(instantiatedEntity, typeof(Traveller))
                    && attacker != instantiatedEntity) {
                Traveller adjacentTraveller = (Traveller)instantiatedEntity;
                if (adjacentTraveller.GetFaction() == attackerFaction) {
                    Debug.Log("Creature took advantage of pack tactics.");
                    return AdvantageCalcs.GetNewAdvantageState(currAdvState, Advantage.Advantage);
                }
            }
        }

        return nextAdvantageState;
    }

    public IEnumerator TriggerUndeadFortitude(Traveller instance, Damage damage) {
        if (instance.CurrHP > 0 
                // todo - convey that these have happened
                || damage.damageType == DamageType.Radiant
                || damage.isCriticalHit) {
            yield break;
        }

        int difficultyClass = 5 + damage.amount;

        int modifier = StatModifiers.GetModifierForStat(instance.GetStats().constitution);

        CoroutineWithData<int> savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            "Rolling CON saving throw for Zombie's Undead Fortitude", modifier, difficultyClass));
        yield return savingThrowCoroutine.coroutine;
        int roll = savingThrowCoroutine.GetResult();

        // todo - revert
        if (roll >= difficultyClass) {
            instance.SetHP(1);
            // todo - make a message of what happened instead of a random effect
            effectManager.GenerateExclaimEffectInstant(instance);
        }
    }
}

public class AttackHitModifications {
    public bool isNewlyCritical = false;
    public List<Die> bonusDamage = new();
}
