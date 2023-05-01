using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public abstract class IntangibleEntity : Entity {
        public string prefabName;

        public IntangibleEntity(Vector3Int startPosition, string prefabName) {
            this.spawnPosition = startPosition;
            this.prefabName = prefabName;
        }
    }
}
