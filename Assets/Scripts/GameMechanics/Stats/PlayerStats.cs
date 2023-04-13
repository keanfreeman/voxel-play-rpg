using EntityDefinition;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public class PlayerStats : Stats {
        public int level { get; private set; }

        public PlayerStats(string name, int baseSpeed, int hitPoints, EntitySize size,
                int strength, int dexterity, int constitution, int intelligence,
                int wisdom, int charisma, List<GameMechanics.Action> actions, int level)
                : base(name, baseSpeed, hitPoints, size, strength, dexterity, constitution, intelligence,
                      wisdom, charisma, actions)
                {
            this.level = level;
        }

        public override int CalculateArmorClass() {
            return 10 + StatModifiers.GetModifierForStat(dexterity);
        }
    }
}
