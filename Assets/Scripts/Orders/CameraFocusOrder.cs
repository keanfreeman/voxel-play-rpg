using EntityDefinition;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class CameraFocusOrder : Order
    {
        public Guid focusTargetTravellerGuid;

        [JsonConstructor]
        public CameraFocusOrder(Guid focusTargetTravellerGuid) {
            this.focusTargetTravellerGuid = focusTargetTravellerGuid;
        }

        public CameraFocusOrder(Traveller traveller) {
            this.focusTargetTravellerGuid = traveller.guid;
        }
    }
}
