using GameMechanics;
using Instantiated;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RandomManager;

public class FeatureManager : MonoBehaviour {
    [SerializeField] RandomManager randomManager;
    [SerializeField] EffectManager effectManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] CombatManager combatManager;
    [SerializeField] VisualRollManager visualRollManager;

    public void SetUpFeatures(Traveller traveller) {
        foreach (Feature feature in traveller.GetStats().features) {
            switch (feature.id) {
                case FeatureID.PackTactics:
                    traveller.onPerformAttack += TriggerPackTactics;
                    break;
                case FeatureID.UndeadFortitude:
                    traveller.onHPChanged += TriggerUndeadFortitude;
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

    private IEnumerator CheckParalyzedCriticalHit(AttackSO attack, Traveller target) {
        yield return target.statusEffects.IsParalyzed();
    }

    private Advantage CheckParalyzedAdvantage(Traveller attacker, Traveller target, Advantage currAdvState) {
        if (target.statusEffects.IsParalyzed()) {
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

        CoroutineWithData savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            "Rolling CON saving throw for Ghoul Claw", modifier, DC));
        yield return savingThrowCoroutine.coroutine;
        int roll = (int)savingThrowCoroutine.result;

        if (roll < DC) {
            OngoingEffect ghoulStatusEffect = target.statusEffects.Get(StatusEffect.GhoulClaw);
            if (ghoulStatusEffect == null) {
                target.statusEffects.Add(StatusEffect.GhoulClaw, 
                    new OngoingEffect(StatusEffect.GhoulClaw, 
                    new List<Condition> { Condition.Paralyzed}, 10));
                target.onCombatTurnEnd += CheckGhoulClawEnd;
            }
            else {
                // reset timer if already affected
                ghoulStatusEffect.turnsLeft = 10;
            }
        }
    }

    private IEnumerator CheckGhoulClawEnd(Traveller currTurnTraveller) {
        OngoingEffect ghoulStatusEffect = currTurnTraveller.statusEffects.Get(StatusEffect.GhoulClaw);
        if (ghoulStatusEffect == null) {
            yield break;
        }

        const int DC = 10;
        int modifier = StatModifiers.GetModifierForStat(currTurnTraveller.GetStats().constitution);

        CoroutineWithData savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            "Rolling CON saving throw to end Paralysis from Ghoul Claw", modifier, 10));
        yield return savingThrowCoroutine.coroutine;
        int roll = (int)savingThrowCoroutine.result;

        if (roll >= DC || ghoulStatusEffect.turnsLeft <= 1) {
            currTurnTraveller.statusEffects.Remove(StatusEffect.GhoulClaw);
            currTurnTraveller.onCombatTurnEnd -= CheckGhoulClawEnd;
        }
        else {
            ghoulStatusEffect.turnsLeft -= 1;
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
        if (instance.currHP > 0 
                // todo - convey that these have happened
                || damage.damageType == DamageType.Radiant
                || damage.isCriticalHit) {
            yield break;
        }

        int difficultyClass = 5 + damage.amount;

        int modifier = StatModifiers.GetModifierForStat(instance.GetStats().constitution);

        CoroutineWithData savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            "Rolling CON saving throw for Zombie's Undead Fortitude", modifier, difficultyClass));
        yield return savingThrowCoroutine.coroutine;
        int roll = (int)savingThrowCoroutine.result;

        // todo - revert
        if (roll >= difficultyClass) {
            instance.SetHP(1);
            // todo - make a message of what happened instead of a random effect
            effectManager.GenerateExclaimEffectInstant(instance);
        }
    }
}