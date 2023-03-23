using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics.Wolf {
    public class Bite : GameMechanics.Attack
    {
        public Bite(string attackName, Dice attackRoll, Dice damageRoll) 
                : base(attackName, attackRoll, damageRoll) {}

        // TODO - add STR saving throw mechanics
    }
}
