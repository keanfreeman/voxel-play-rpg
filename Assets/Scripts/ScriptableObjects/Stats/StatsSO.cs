using DieNamespace;
using GameMechanics;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class StatsSO : ScriptableObject
{
    // Speed in feet per 6 seconds
    public int baseSpeed = 30;
    public int maxHP;
    public Die hitDice;
    public EntitySize size = EntitySize.Medium;
    public CreatureType creatureType = CreatureType.Humanoid;

    // todo - add GetSavingThrowModifier since NPCs may have special one, and PCs have additives too
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    public List<ActionSO> actions;
    public List<FeatureSO> features;

    public SpellcastingFeatureSO GetSpellcastingFeature() {
        return features
            .Where((FeatureSO feature) => TypeUtils
                .IsSameTypeOrIsSubclass(feature, typeof(SpellcastingFeatureSO)))
            .Select((FeatureSO feature) => (SpellcastingFeatureSO)feature)
            .First();
    }

    public abstract int CalculateArmorClass(CurrentStatus currentStatus);

    public bool HasFeature(FeatureID featureID) {
        return features.Where((FeatureSO feature) => feature.id == featureID).Count() > 0;
    }
}

public enum CreatureType {
    Aberration,
    Beast,
    Celestial,
    Construct,
    Dragon,
    Elemental,
    Fey,
    Fiend,
    Giant,
    Humanoid,
    Monstrosity,
    Ooze,
    Plant,
    Undead
}
