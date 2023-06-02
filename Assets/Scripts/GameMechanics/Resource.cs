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
        public int maxUses;

        [JsonConstructor]
        public Resource(ResourceID id, bool recoversOnShortRest, bool recoversOnLongRest, int maxUses) {
            this.id = id;
            this.recoversOnShortRest = recoversOnShortRest;
            this.recoversOnLongRest = recoversOnLongRest;
            this.maxUses = maxUses;
        }
    }

    public enum ResourceID {
        SecondWind,
        SpellSlots,
        BardicInspiration
    }
}
