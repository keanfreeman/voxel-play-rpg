using EntityDefinition;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public class NPCStats : Stats {
        public string challengeRating { get; private set; }

        private int armorClass;

        public NPCStats(string name, int baseSpeed, int hitPoints, EntitySize size,
            int strength, int dexterity, int constitution, int intelligence,
            int wisdom, int charisma, List<GameMechanics.Action> actions, string challengeRating, 
            int armorClass)
            : base(name, baseSpeed, hitPoints, size, strength, dexterity, constitution, intelligence,
                  wisdom, charisma, actions) {
            this.challengeRating = challengeRating;
            this.armorClass = armorClass;
        }

        public override int CalculateArmorClass() {
            return armorClass;
        }
    }
}
