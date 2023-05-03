using EntityDefinition;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class MoveOrder : Order
    {
        public Vector3Int destination;
        public Guid travellerGuid;

        [JsonConstructor]
        public MoveOrder(Vector3Int destination, Guid travellerGuid) {
            this.destination = destination;
            this.travellerGuid = travellerGuid;
        }

        public MoveOrder(Vector3Int destination, Traveller traveller) {
            this.destination = destination;
            this.travellerGuid = traveller.guid;
        }
    }
}
