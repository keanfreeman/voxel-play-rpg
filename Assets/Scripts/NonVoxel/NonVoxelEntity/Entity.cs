using GameMechanics;
using MovementDirection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public abstract class Entity : Spawnable {
        public Vector3Int startPosition { get; protected set; }
        // all points occupied by this Entity, relative to the startPosition (for example, (0,0,0) and (1,0,0))
        public List<Vector3Int> occupiedPositions { get; protected set; }
        public EntityDisplay entityDisplay { get; protected set; }

        public Entity(Vector3Int startPosition, EntityDisplay entityDisplay) {
            this.startPosition = startPosition;
            this.entityDisplay = entityDisplay;
            this.occupiedPositions = new List<Vector3Int> { Vector3Int.zero };
        }

        public Entity(Vector3Int startPosition, EntitySize entitySize, EntityDisplay entityDisplay) {
            this.startPosition = startPosition;
            this.entityDisplay = entityDisplay;

            if (entitySize < EntitySize.LARGE) {
                occupiedPositions = new List<Vector3Int> { Vector3Int.zero };
            }
            else {
                List<Vector3Int> positions = Coordinates.GetPositionsFromSizeCategory(startPosition, entitySize, 
                    false);
                occupiedPositions = new List<Vector3Int>(positions.Count);
                foreach (Vector3Int position in positions) {
                    occupiedPositions.Add(position - startPosition);
                }
            }
        }
    }
}
