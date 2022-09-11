using System.Collections.Generic;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld {
        private Dictionary<GameObject, Vector3Int> creatureToPosition
            = new Dictionary<GameObject, Vector3Int>();
        private Dictionary<Vector3Int, GameObject> positionToCreature
            = new Dictionary<Vector3Int, GameObject>();

        public NonVoxelWorld() {
        }

        public Vector3Int GetPosition(GameObject gameObject) {
            return creatureToPosition[gameObject];
        }

        public void SetPosition(GameObject gameObject, Vector3Int position) {
            if (creatureToPosition.ContainsKey(gameObject)) {
                Vector3Int oldPosition = creatureToPosition[gameObject];
                if (positionToCreature.ContainsKey(oldPosition)) {
                    positionToCreature.Remove(oldPosition);
                }
            }

            creatureToPosition[gameObject] = position;
            positionToCreature[position] = gameObject;
        }

        public bool IsPositionOccupied(Vector3Int position) {
            return positionToCreature.ContainsKey(position);
        }
    }
}
