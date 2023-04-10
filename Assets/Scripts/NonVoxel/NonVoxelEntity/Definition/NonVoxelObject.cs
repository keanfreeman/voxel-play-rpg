using GameMechanics;
using MovementDirection;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public class NonVoxelObject : Entity {
        public Direction startRotation { get; protected set; }

        public NonVoxelObject(Vector3Int startPosition, EntityDisplay entityDisplay,
                List<Vector3Int> occupiedPositions, Direction startRotation) 
                : base(startPosition, entityDisplay, occupiedPositions) {
            this.startRotation = startRotation;
        }
    }
}
