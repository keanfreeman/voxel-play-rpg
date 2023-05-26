using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spells {
    [CreateAssetMenu(fileName = "New Spell", menuName = "ScriptableObjects/Actions/Spell")]
    public class SpellSO : ActionSO
    {
        public CastingTime castingTime;
        public string range;
        public SpellComponents spellComponents;
        public bool isConcentration;
        // todo - use something more useful, like ints for time above instantaneous
        public string duration;
        public string description;
        public AttackSO providedAttack;

        public bool IsSpellAttack() {
            return providedAttack != null;
        }
    }

    public enum CastingTime {
        Action,
        BonusAction,
        Reaction
    }

    [Serializable]
    public class SpellComponents {
        public bool isVerbal;
        public bool isSomatic;
        public List<SpellMaterial> materialComponents;

        public SpellComponents(bool isVerbal, bool isSomatic,
                List<SpellMaterial> materialComponents) {
            this.isVerbal = isVerbal;
            this.isSomatic = isSomatic;
            this.materialComponents = materialComponents;
        }

        [Serializable]
        public struct SpellMaterial {
            public string name;
            public int goldCost;
        }
    }
}
