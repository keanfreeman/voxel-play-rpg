using GameMechanics;
using MovementDirection;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public class TangibleObject : TangibleEntity {
        public Direction startRotation { get; protected set; }

        public TangibleObject(Vector3Int startPosition, EntityDisplay entityDisplay,
                List<Vector3Int> occupiedPositions, Direction startRotation) 
                : base(startPosition, entityDisplay, occupiedPositions) {
            this.startRotation = startRotation;
        }
    }
}
