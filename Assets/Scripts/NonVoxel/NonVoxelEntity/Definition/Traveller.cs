using GameMechanics;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public abstract class Traveller : TangibleEntity {
        public Traveller(Vector3Int startPosition, TravellerIdentity travellerIdentity) 
                : base(startPosition, travellerIdentity) {
        }

        public TravellerIdentity GetTravellerIdentity() {
            return (TravellerIdentity)identity;
        }
    }
}
