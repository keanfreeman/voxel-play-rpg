using GameMechanics;
using MovementDirection;
using Newtonsoft.Json;
using NonVoxelEntity;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public abstract class TangibleEntity : Entity {
        public string identity;
        public OrderGroup interactOrders;

        [JsonConstructor]
        public TangibleEntity(Vector3Int startPosition, string identity, OrderGroup interactOrders)
                 : base(startPosition) {
            this.identity = identity;
            this.interactOrders = interactOrders;
        }

        public TangibleEntity(Vector3Int startPosition, string identity)
                 : base(startPosition) {
            this.identity = identity;
            this.interactOrders = null;
        }
    }
}
