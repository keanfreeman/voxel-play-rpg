using GameMechanics;
using MovementDirection;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public abstract class TangibleEntity : Spawnable {
        public Vector3Int startPosition { get; protected set; }
        // all points occupied by this Entity, relative to the startPosition (for example, (0,0,0) and (1,0,0))
        public List<Vector3Int> occupiedPositions { get; protected set; }
        public Identity identity { get; protected set; }

        public TangibleEntity(Vector3Int startPosition, ObjectIdentity identity) {
            this.startPosition = startPosition;
            this.identity = identity;
            occupiedPositions = identity.occupiedPositions;
        }

        public TangibleEntity(Vector3Int startPosition, TravellerIdentity identity) {
            this.startPosition = startPosition;
            this.identity = identity;

            EntitySize entitySize = identity.stats.size;

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
