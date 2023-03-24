using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public class Attack : GameMechanics.Action
    {
        public Dice attackRoll { get; protected set; }
        public Dice damageRoll { get; protected set; }
        public int shortRange { get; protected set; } = 0;
        public int longRange { get; protected set; } = 0;

        public Attack(string attackName, Dice attackRoll, Dice damageRoll) {
            this.name = $"{attackName} Attack";
            this.attackRoll = attackRoll;
            this.damageRoll = damageRoll;
        }

        public Attack(string attackName, Dice attackRoll, Dice damageRoll, int shortRange, 
                int longRange) : this(attackName, attackRoll, damageRoll) {
            this.shortRange = shortRange;
            this.longRange = longRange;
        }
    }
}
