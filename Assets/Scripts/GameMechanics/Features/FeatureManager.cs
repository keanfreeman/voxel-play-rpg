using GameMechanics;
using Instantiated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureManager : MonoBehaviour {
    [SerializeField] RandomManager randomManager;
    [SerializeField] EffectManager effectManager;

    public void SetUpFeatures(Traveller traveller) {
        foreach (Feature feature in traveller.GetStats().features) {
            switch (feature.id) {
                case FeatureID.UndeadFortitude:
                    traveller.onHPChanged += TriggerUndeadFortitude;
                    break;
                default:
                    throw new System.NotImplementedException($"Did not implement traveller feature.");
            }
        }
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
