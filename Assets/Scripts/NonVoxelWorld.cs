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

        public void RotateNonPlayerCreatures(KeyCode rotationDirection) {
            foreach (GameObject gameObject in creatureToPosition.Keys) {
                NPCBehavior npcBehavior = gameObject.GetComponent<NPCBehavior>();
                if (npcBehavior != null) {
                    npcBehavior.rotationDirection = rotationDirection;
                }
            }
        }

        public List<Vector3Int> GetInteractableAdjacentObjects(Vector3Int currPosition) {
            List<Vector3Int> occupiedVoxels = new List<Vector3Int>();
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {
                        Vector3Int checkPosition = currPosition + new Vector3Int(x, y, z);
                        if (checkPosition != currPosition && IsPositionOccupied(checkPosition)) {
                            NPCBehavior npcBehavior = positionToCreature[checkPosition].GetComponent<NPCBehavior>();
                            if (npcBehavior != null && npcBehavior.IsInteractable()) {
                                occupiedVoxels.Add(checkPosition);
                            }
                        }
                    }
                }
            }

            return occupiedVoxels;
        }
    }
}
