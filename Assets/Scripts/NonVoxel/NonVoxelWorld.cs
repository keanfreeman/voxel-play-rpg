using System.Collections.Generic;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld {
        private Dictionary<GameObject, Vector3Int> objectToPosition
            = new Dictionary<GameObject, Vector3Int>();
        private Dictionary<Vector3Int, GameObject> positionToObject
            = new Dictionary<Vector3Int, GameObject>();

        public HashSet<MonoBehaviour> npcs = new HashSet<MonoBehaviour>();

        public bool IsInWorld(GameObject gameObject) {
            return objectToPosition.ContainsKey(gameObject);
        }

        public Vector3Int GetPosition(GameObject gameObject) {
            return objectToPosition[gameObject];
        }

        public void SetPosition(GameObject gameObject, Vector3Int position) {
            if (objectToPosition.ContainsKey(gameObject)) {
                Vector3Int oldPosition = objectToPosition[gameObject];
                if (positionToObject.ContainsKey(oldPosition)) {
                    positionToObject.Remove(oldPosition);
                }
            }

            objectToPosition[gameObject] = position;
            positionToObject[position] = gameObject;
        }

        public GameObject GetObjectFromPosition(Vector3Int position) {
            return positionToObject[position];
        }

        public bool IsPositionOccupied(Vector3Int position) {
            GameObject gameObject = positionToObject.GetValueOrDefault(position, null);
            if (gameObject == null) {
                return false;
            }
            if (gameObject.GetComponent<SceneExit>() != null) {
                return false;
            }
            return true;
        }

        public void RotateNonPlayerCreatures(KeyCode rotationDirection) {
            foreach (NPCBehavior npcBehavior in npcs) {
                npcBehavior.rotationDirection = rotationDirection;
            }
        }

        public List<Vector3Int> GetInteractableAdjacentObjects(Vector3Int currPosition) {
            List<Vector3Int> occupiedVoxels = new List<Vector3Int>();
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {
                        Vector3Int checkPosition = currPosition + new Vector3Int(x, y, z);
                        if (checkPosition != currPosition && IsPositionOccupied(checkPosition)) {
                            NPCBehavior npcBehavior = positionToObject[checkPosition].GetComponent<NPCBehavior>();
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
