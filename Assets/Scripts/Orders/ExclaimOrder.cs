using EntityDefinition;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class ExclaimOrder : Order
    {
        public Guid travellerGuid;

        [JsonConstructor]
        public ExclaimOrder(Guid travellerGuid) {
            this.travellerGuid = travellerGuid;
        }

        public ExclaimOrder(Traveller traveller) {
            this.travellerGuid = traveller.guid;
        }
    }
}
