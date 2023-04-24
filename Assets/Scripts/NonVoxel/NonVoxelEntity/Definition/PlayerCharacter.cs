
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;
using MovementDirection;

namespace EntityDefinition {
    public class PlayerCharacter : Traveller {
        public PlayerCharacter(Vector3Int startPosition, TravellerIdentity travellerIdentity)
                : base(startPosition, travellerIdentity) {
        }
    }
}
