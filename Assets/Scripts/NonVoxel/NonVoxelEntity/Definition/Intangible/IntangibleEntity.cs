using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public abstract class IntangibleEntity : Spawnable {
        public Vector3Int startPosition { get; private set; }
        public GameObject prefab { get; private set; }

        public IntangibleEntity(Vector3Int startPosition, GameObject prefab) {
            this.startPosition = startPosition;
            this.prefab = prefab;
        }
    }
}
