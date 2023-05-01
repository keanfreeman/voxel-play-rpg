using GameMechanics;
using MovementDirection;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EntityDefinition {
    [Serializable]
    public class TangibleObject : TangibleEntity {
        public Direction startRotation;

        public TangibleObject(Vector3Int startPosition, Direction startRotation, string objectIdentity) 
                : base(startPosition, objectIdentity) {
            this.startRotation = startRotation;
        }
    }
}
