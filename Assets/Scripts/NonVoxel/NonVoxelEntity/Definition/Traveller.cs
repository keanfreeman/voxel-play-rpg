using GameMechanics;
using NonVoxelEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public abstract class Traveller : TangibleEntity {
        public Vector3Int? currSpawnPosition;

        public Traveller(Vector3Int startPosition, string travellerIdentity) 
                : base(startPosition, travellerIdentity) {
        }
    }
}
