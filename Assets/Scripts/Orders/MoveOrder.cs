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
        // Guid.Empty signals moving the party
        public Guid travellerGuid;
        public bool waitForCompletion;

        [JsonConstructor]
        public MoveOrder(Vector3Int destination, Guid travellerGuid, bool waitForCompletion) {
            this.destination = destination;
            this.travellerGuid = travellerGuid;
            this.waitForCompletion = waitForCompletion;
        }

        public MoveOrder(Vector3Int destination, Traveller traveller) {
            this.destination = destination;
            this.travellerGuid = traveller.guid;
            this.waitForCompletion = true;
        }

        public MoveOrder(Vector3Int destination, Traveller traveller, bool waitForCompletion) {
            this.destination = destination;
            this.travellerGuid = traveller.guid;
            this.waitForCompletion = waitForCompletion;
        }
    }
}
