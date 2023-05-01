using GameMechanics;
using MovementDirection;
using NonVoxelEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public abstract class TangibleEntity : Entity {
        // all points occupied by this Entity, relative to the startPosition (for example, (0,0,0) and (1,0,0))
        public string identity;

        public TangibleEntity(Vector3Int startPosition, string identity) {
            this.spawnPosition = startPosition;
            this.identity = identity;
        }
    }
}
