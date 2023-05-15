using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public class StatusEffect {
        public string displayName;
        public Status status;
        public int turnsLeft;

        public StatusEffect(string displayName, Status status, int turnsLeft) {
            this.displayName = displayName;
            this.status = status;
            this.turnsLeft = turnsLeft;
        }
    }

    public enum Status {
        GhoulParalysis
    }
}
