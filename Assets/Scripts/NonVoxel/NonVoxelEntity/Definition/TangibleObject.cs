using GameMechanics;
using MovementDirection;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Orders;
using Newtonsoft.Json;

namespace EntityDefinition {
    [Serializable]
    public class TangibleObject : TangibleEntity {
        public Direction startRotation;

        [JsonConstructor]
        public TangibleObject(Vector3Int startPosition, string identity, OrderGroup interactOrders, 
                Direction startRotation) 
                : base(startPosition, identity, interactOrders) {
            this.startRotation = startRotation;
        }

        public TangibleObject(Vector3Int startPosition, string identity,
                Direction startRotation)
                : base(startPosition, identity) {
            this.startRotation = startRotation;
        }
    }
}
