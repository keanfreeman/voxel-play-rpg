using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public abstract class Stats {
        public string name { get; private set; }

        // Speed in feet per 6 seconds
        public int baseSpeed { get; private set; }
        public int hitPoints { get; private set; }

        public int strength { get; private set; }
        public int dexterity { get; private set; }
        public int constitution { get; private set; }
        public int intelligence { get; private set; }
        public int wisdom { get; private set; }
        public int charisma { get; private set; }

        public List<GameMechanics.Action> actions { get; private set; }

        public Stats(string name, int baseSpeed, int hitPoints,
                int strength, int dexterity, int constitution, int intelligence,
                int wisdom, int charisma, List<GameMechanics.Action> actions) {
            this.name = name;
            this.baseSpeed = baseSpeed;
            this.hitPoints = hitPoints;
            this.strength = strength;
            this.dexterity = dexterity;
            this.constitution = constitution;
            this.intelligence = intelligence;
            this.wisdom = wisdom;
            this.charisma = charisma;
            this.actions = actions;
        }

        public abstract int CalculateArmorClass();
    }
}
