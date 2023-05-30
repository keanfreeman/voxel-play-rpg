using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    [CreateAssetMenu(fileName = "New Feature", menuName = "ScriptableObjects/Features/Feature")]
    public class FeatureSO : ScriptableObject
    {
        public FeatureID id;
        public string featureName;
        public string description;
        public List<ActionSO> providedActions;
        public List<Resource> providedResources;
    }

    public enum FeatureID {
        UndeadFortitude,
        PackTactics,

        // fighter
        FightingStyleDefense,
        SecondWind,

        // rogue
        SneakAttack,

        // wizard
        SpellSlots
    }

    public enum AttackFeature {
        None,
        GhoulClaws,
        ShockingGrasp // todo implement
    }
}
