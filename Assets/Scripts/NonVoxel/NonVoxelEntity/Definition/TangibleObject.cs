using GameMechanics;
using MovementDirection;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public class TangibleObject : TangibleEntity {
        public Direction startRotation { get; protected set; }

        public TangibleObject(Vector3Int startPosition, Direction startRotation, ObjectIdentity objectIdentity) 
                : base(startPosition, objectIdentity) {
            this.startRotation = startRotation;
        }

        public ObjectIdentity GetObjectIdentity() {
            return (ObjectIdentity)identity;
        }
    }
}
