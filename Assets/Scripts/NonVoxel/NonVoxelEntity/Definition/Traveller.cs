using GameMechanics;
using Newtonsoft.Json;
using NonVoxelEntity;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public abstract class Traveller : TangibleEntity {
        public Vector3Int? currSpawnPosition;

        [JsonConstructor]
        public Traveller(Vector3Int startPosition, string travellerIdentity, OrderGroup interactOrders) 
                : base(startPosition, travellerIdentity, interactOrders) {
        }

        public Traveller(Vector3Int startPosition, string travellerIdentity) 
                : base(startPosition, travellerIdentity) {
        }
    }
}
