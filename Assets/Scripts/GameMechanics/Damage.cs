using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    [Serializable]
    public class Damage {
        public DamageType damageType;
        public int amount;
        public bool isCriticalHit;

        public Damage(DamageType damageType, int amount, bool isCriticalHit = false) {
            this.damageType = damageType;
            this.amount = amount;
            this.isCriticalHit = isCriticalHit;
        }
    }

    public enum DamageType {
        Acid,
        Bludgeoning,
        Cold,
        Fire,
        Force,
        Lightning,
        Necrotic,
        Piercing,
        Poison,
        Psychic,
        Radiant,
        Slashing,
        Thunder
    }
}
