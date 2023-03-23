using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public class Attack : GameMechanics.Action
    {
        public Dice attackRoll { get; protected set; }
        public Dice damageRoll { get; protected set; }

        public Attack(string attackName, Dice attackRoll, Dice damageRoll) {
            this.name = $"{attackName} Attack";
            this.attackRoll = attackRoll;
            this.damageRoll = damageRoll;
        }
    }
}
