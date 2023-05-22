using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    [System.Serializable]
    public class Feature
    {
        public string name;
        public string description;
        public FeatureID id;
        public List<ActionSO> providedActions;

        public Feature(string name, string description, FeatureID id, List<ActionSO> providedActions) {
            this.name = name;
            this.description = description;
            this.id = id;
            this.providedActions = providedActions;
        }
    }

    public enum FeatureID {
        UndeadFortitude,
        PackTactics,

        // fighter
        FightingStyleDefense,
        SecondWind
    }

    public enum AttackFeature {
        None,
        GhoulClaws
    }
}
