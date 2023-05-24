using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    [Serializable]
    public class Resource
    {
        public ResourceID id;
        public bool recoversOnShortRest;
        public bool recoversOnLongRest;

        [JsonConstructor]
        protected Resource(ResourceID resourceID, bool recoversOnShortRest, bool recoversOnLongRest) {
            this.id = resourceID;
            this.recoversOnShortRest = recoversOnShortRest;
            this.recoversOnLongRest = recoversOnLongRest;
        }
    }

    public enum ResourceID {
        SecondWind
    }
}
