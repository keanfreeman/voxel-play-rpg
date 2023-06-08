using Instantiated;
using EntityDefinition;
using Orders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace Orders {
    [Serializable]
    public class MoveImmediateOrder : Order
    {
        public Vector3Int destination;
        public MoveOrderType moveOrderType;
        public Guid travellerGuid;

        [JsonConstructor]
        public MoveImmediateOrder(Vector3Int destination, MoveOrderType moveOrderType, Guid travellerGuid) {
            this.destination = destination;
            this.moveOrderType = moveOrderType;
            this.travellerGuid = travellerGuid;
        }

        public MoveImmediateOrder(Vector3Int destination, MoveOrderType moveOrderType) {
            if (moveOrderType != MoveOrderType.Party) {
                throw new ArgumentException("This constructor is for moving parties.");
            }
            this.destination = destination;
            this.moveOrderType = moveOrderType;
            travellerGuid = Guid.Empty;
        }
    }

    public enum MoveOrderType {
        Entity,
        Party
    }
}
