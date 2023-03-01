using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public abstract class Entity {
        public GameObject prefab { get; protected set; }
        public Vector3Int startPosition { get; protected set; }

        public Entity(GameObject prefab, Vector3Int startPosition) {
            this.prefab = prefab;
            this.startPosition = startPosition;
        }
    }
}
