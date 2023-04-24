using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public class ObjectIdentity : Identity {
        public List<Vector3Int> occupiedPositions;

        public ObjectIdentity(GameObject prefab, List<Vector3Int> occupiedPositions)
                : base(prefab) {
            this.occupiedPositions = occupiedPositions;
        }
    }
}
