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
        public OrderGroup interactOrders;

        [JsonConstructor]
        public TangibleObject(Direction startRotation, OrderGroup interactOrders, 
                Vector3Int startPosition, string objectIdentity)
                : base(startPosition, objectIdentity) {
            this.startRotation = startRotation;
            this.interactOrders = interactOrders;
        }

        public TangibleObject(Vector3Int startPosition, Direction startRotation, string objectIdentity) 
                : base(startPosition, objectIdentity) {
            this.startRotation = startRotation;
        }
    }
}
