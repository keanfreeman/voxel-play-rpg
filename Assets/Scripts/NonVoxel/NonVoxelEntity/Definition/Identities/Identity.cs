using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public abstract class Identity {
        public GameObject prefab;

        protected Identity(GameObject prefab) {
            this.prefab = prefab;
        }
    }
}
