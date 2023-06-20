using DieNamespace;
using GameMechanics;
using Instantiated;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RandomManager;

public class FeatureManager : MonoBehaviour {
    [SerializeField] RandomManager randomManager;
    [SerializeField] EffectManager effectManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] CombatManager combatManager;
    [SerializeField] VisualRollManager visualRollManager;
    [SerializeField] MessageManager messageManager;
    [SerializeField] PromptUIController promptUIController;

    public void SetUpFeatures(Traveller traveller) {
        List<Resource> resourcesToAdd = new();
        foreach (FeatureSO feature in traveller.GetStats().features) {
            switch (feature.id) {
                case FeatureID.PackTactics:
                    traveller.onAttackRollStart += TriggerPackTactics;
                    break;
                case FeatureID.UndeadFortitude:
                    traveller.onTakeDamage += TriggerUndeadFortitude;
                    break;
                case FeatureID.FightingStyleDefense:
                    // todo - make this feature work dynamically
                    break;
                case FeatureID.SecondWind:
                    resourcesToAdd.Add(feature.providedResources[0]);
                    break;
                case FeatureID.SpellSlots:
                    resourcesToAdd.Add(feature.providedResources[0]);
                    break;
                case FeatureID.BardicInspiration:
                    resourcesToAdd.Add(feature.providedResources[0]);
                    break;
                case FeatureID.SneakAttack:
                    traveller.onAttackHit += TriggerSneakAttack;
                    break;
                case FeatureID.ArcaneRecovery:
                    break;
                default:
                    throw new System.NotImplementedException($"Did not implement traveller " +
                        $"feature {feature.id}.");
            }
        }
        if (traveller.GetResources() == null) {
            traveller.InitResources(resourcesToAdd);
        }

        foreach (ActionSO action in traveller.GetStats().actions) {
            if (action.GetType() == typeof(AttackSO)) {
                AttackSO attack = (AttackSO)action;
                switch (attack.attackFeature) {
                    case AttackFeature.GhoulClaws:
                        traveller.onDealDamage += ApplyGhoulClaw;
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
        traveller.onAttackRollStart += CheckAdvantageFromStatus;
        traveller.onAttackHit += CheckCriticalHitFromStatus;
    }

    public IEnumerator CheckBardicInspiration(Traveller attacker) {
        // for now, only support PCs
        if (attacker.GetType() != typeof(PlayerCharacter) 
                || !attacker.HasStatus(StatusEffect.BardicInspiration)) {
            yield return false;
            yield break;
        }

        CoroutineWithData<bool> cwd = new(this, promptUIController.DisplayPrompt(
            "Bardic Inspiration", "Would you like to use Bardic Inspiration to get a bonus to this roll?",
            ControlState.COMBAT));
        yield return cwd.coroutine;
        bool respondedYes = cwd.GetResult();
        if (respondedYes) attacker.RemoveStatus(StatusEffect.BardicInspiration);
        yield return respondedYes;
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
                    if (adjacentTraveller.GetFaction() != targetFaction 
                            && !adjacentTraveller.HasCondition(Condition.Unconscious)
                            && !adjacentTraveller.HasCondition(Condition.Paralyzed)
                            && !adjacentTraveller.HasCondition(Condition.Incapacitated)) {
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

    private IEnumerator CheckCriticalHitFromStatus(Traveller target, Advantage _) {
        yield return new AttackHitModifications() { isNewlyCritical
            = target.HasCondition(Condition.Paralyzed) || target.HasCondition(Condition.Unconscious)
        };
    }

    // todo - have the conditions themselves intervene and change the advantage rather than checking them all 
    // individually
    private Advantage CheckAdvantageFromStatus(Traveller attacker, Traveller target, Advantage startAdvState) {
        Advantage currAdvState = startAdvState;
        if (attacker.HasCondition(Condition.Blinded) || attacker.HasStatus(StatusEffect.ViciousMockery)) {
            currAdvState = AdvantageCalcs.GetNewAdvantageState(currAdvState, Advantage.Disadvantage);
        }

        if (target.HasCondition(Condition.Paralyzed) || target.HasCondition(Condition.Unconscious)
                || target.HasCondition(Condition.Blinded)) {
            currAdvState = AdvantageCalcs.GetNewAdvantageState(currAdvState, Advantage.Advantage);
        }
        return currAdvState;
    }

    public IEnumerator CheckSleepSpellEnd(Traveller victim, Damage damage) {
        OngoingEffect ongoingEffect = victim.GetStatus(StatusEffect.SleepSpell);
        if (ongoingEffect == null) {
            victim.onTakeDamage -= CheckSleepSpellEnd;
            yield break;
        }

        if (damage.amount > 0) {
            victim.RemoveStatus(StatusEffect.SleepSpell);
        }
    }

    // todo - should not work on elves/undead
    private IEnumerator ApplyGhoulClaw(ActionSO damageAction, Traveller target) {
        if (!TypeUtils.IsSameTypeOrIsSubclass(damageAction, typeof(AttackSO))) {
            yield break;
        }
        AttackSO attack = (AttackSO)damageAction;
        // todo - implement so I don't have to check the attack type is correct
        if (attack.attackFeature != AttackFeature.GhoulClaws) {
            yield break;
        }

        const int DC = 10;
        int modifier = StatModifiers.GetModifierForStat(target.GetStats().constitution);

        CoroutineWithData<int> savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            target, "Rolling CON saving throw for Ghoul Claw", modifier, DC));
        yield return savingThrowCoroutine.coroutine;
        int roll = savingThrowCoroutine.GetResult();

        if (roll < DC) {
            OngoingEffect ghoulStatusEffect = target.GetStatus(StatusEffect.GhoulClaw);
            if (ghoulStatusEffect == null) {
                // todo - permanently associate ghoul claws with its conditions somehow
                target.AddStatus(new OngoingEffect(StatusEffect.GhoulClaw, 
                    new HashSet<Condition> { Condition.Paralyzed }, TimeUtil.MINUTE));
                target.onCombatTurnEnd += CheckGhoulClawEnd;
            }
            else {
                // reset timer if already affected
                ghoulStatusEffect.secondsLeft = TimeUtil.MINUTE;
            }
        }
    }

    public IEnumerator EndViciousMockeryTurnEnd(Traveller endTurnTraveller) {
        EndViciousMockery(endTurnTraveller);
        yield break;
    }

    public void EndViciousMockeryAttackPerformed(Traveller attacker) {
        EndViciousMockery(attacker);
    }

    private void EndViciousMockery(Traveller statused) {
        statused.RemoveStatus(StatusEffect.ViciousMockery);
        statused.onCombatTurnEnd -= EndViciousMockeryTurnEnd;
        statused.onAttackRollFinished -= EndViciousMockeryAttackPerformed;
    }

    private IEnumerator CheckGhoulClawEnd(Traveller endTurnTraveller) {
        OngoingEffect ghoulStatusEffect = endTurnTraveller.GetStatus(StatusEffect.GhoulClaw);
        if (ghoulStatusEffect == null) {
            yield break;
        }

        const int DC = 10;
        int modifier = StatModifiers.GetModifierForStat(endTurnTraveller.GetStats().constitution);

        CoroutineWithData<int> savingThrowCoroutine = new(this, visualRollManager.RollSavingThrow(
            endTurnTraveller, "Rolling CON saving throw to end Paralysis from Ghoul Claw", modifier, 10));
        yield return savingThrowCoroutine.coroutine;
        int roll = savingThrowCoroutine.GetResult();

        if (roll >= DC || ghoulStatusEffect.secondsLeft <= TimeUtil.ONE_TURN) {
            endTurnTraveller.RemoveStatus(StatusEffect.GhoulClaw);
            endTurnTraveller.onCombatTurnEnd -= CheckGhoulClawEnd;
        }
    }

    // todo - associate color spray status with a caster so if two people cast it I can 
    // disambiguate whose condition should end
    public IEnumerator EndColorSpray(Traveller spellPerformer) {
        foreach (var pair in combatManager.initiatives) {
            Traveller combatant = pair.Value;
            OngoingEffect colorSprayEffect = combatant.GetStatus(StatusEffect.ColorSpray);
            if (colorSprayEffect != null) {
                combatant.RemoveStatus(StatusEffect.ColorSpray);
            }
        }

        spellPerformer.onCombatTurnStart -= EndColorSpray;
        yield break;
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
            instance, "Rolling CON saving throw for Zombie's Undead Fortitude", modifier, difficultyClass));
        yield return savingThrowCoroutine.coroutine;
        int roll = savingThrowCoroutine.GetResult();

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
