
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;
using MovementDirection;
using System;

namespace EntityDefinition {
    [Serializable]
    public class PlayerCharacter : Traveller {
        public PlayerCharacter(Vector3Int startPosition, string travellerIdentity)
                : base(startPosition, travellerIdentity) {
        }
    }
}
