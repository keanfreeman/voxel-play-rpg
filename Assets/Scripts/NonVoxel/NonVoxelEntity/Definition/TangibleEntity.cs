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
        public string identity;

        public TangibleEntity(Vector3Int startPosition, string identity) : base(startPosition) {
            this.identity = identity;
        }
    }
}
