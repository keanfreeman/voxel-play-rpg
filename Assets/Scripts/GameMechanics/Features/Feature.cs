using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    [Serializable]
    public class Feature
    {
        public string name;
        public string description;
        public FeatureID id;

        public Feature(string name, string description, FeatureID id) {
            this.name = name;
            this.description = description;
            this.id = id;
        }
    }

    public enum FeatureID {
        UndeadFortitude
    }
}
