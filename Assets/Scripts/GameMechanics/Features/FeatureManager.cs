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
                    throw new System.NotImplementedException($"Did not implement traveller feature.");
            }
        }
    }

    public Advantage TriggerPackTactics(Traveller attacker, Traveller target) {
        EntityDefinition.Faction attackerFaction = attacker.GetFaction();

        HashSet<Vector3Int> adjacent = Coordinates.GetPositionsSurroundingTraveller(target);
        foreach (Vector3Int position in adjacent) {
            InstantiatedEntity instantiatedEntity = nonVoxelWorld.GetEntityFromPosition(position);
            if (instantiatedEntity != null 
                    && TypeUtils.IsSameTypeOrIsSubclass(instantiatedEntity, typeof(Traveller))
                    && attacker != instantiatedEntity) {
                Traveller adjacentTraveller = (Traveller)instantiatedEntity;
                if (adjacentTraveller.GetFaction() == attackerFaction) {
                    return Advantage.Advantage;
                }
            }
        }

        return Advantage.None;
    }

    // todo - do not activate on critical hit
    public void TriggerUndeadFortitude(Traveller instance, Damage damage) {
        if (instance.currHP > 0 
                // todo - convey that radiant damage overrides this feature
                || damage.damageType == DamageType.Radiant) {
            return;
        }

        int difficultyClass = 5 + damage.amount;

        int modifier = StatModifiers.GetModifierForStat(instance.GetStats().constitution);
        int roll = randomManager.RollSavingThrow(modifier);
        if (roll >= difficultyClass) {
            instance.SetHP(1);
            // todo - make a message of what happened instead of a random effect
            effectManager.GenerateExclaimEffectInstant(instance);
        }
    }
}
