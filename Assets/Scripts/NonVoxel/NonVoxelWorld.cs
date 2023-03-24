using InstantiatedEntity;
using NonVoxelEntity;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld : MonoBehaviour {
        [SerializeField] GameObject playerInstance;

        private Dictionary<InstantiatedNVE, Vector3Int> behaviorToPosition
            = new Dictionary<InstantiatedNVE, Vector3Int>();
        private Dictionary<Vector3Int, InstantiatedNVE> positionToBehavior
            = new Dictionary<Vector3Int, InstantiatedNVE>();

        public HashSet<NPCBehavior> npcs = new HashSet<NPCBehavior>();

        public void DestroyEntities() {
            foreach (MonoBehaviour npc in npcs) {
                npc.enabled = false;
                Destroy(npc.gameObject);
            }
            npcs.Clear();

            foreach (MonoBehaviour behavior in behaviorToPosition.Keys) {
                if (behavior.GetType() != typeof(PlayerMovement)) {
                    Destroy(behavior);
                }
            }
            behaviorToPosition.Clear();
            positionToBehavior.Clear();
        }

        public bool IsInWorld(InstantiatedNVE behavior) {
            return behaviorToPosition.ContainsKey(behavior);
        }

        public Vector3Int GetPosition(InstantiatedNVE behavior) {
            return behaviorToPosition[behavior];
        }

        public void SetPosition(InstantiatedNVE behavior, Vector3Int position) {
            if (behaviorToPosition.ContainsKey(behavior)) {
                Vector3Int oldPosition = behaviorToPosition[behavior];
                if (positionToBehavior.ContainsKey(oldPosition)) {
                    positionToBehavior.Remove(oldPosition);
                }
            }

            behaviorToPosition[behavior] = position;
            positionToBehavior[position] = behavior;
        }

        public MonoBehaviour GetNVEFromPosition(Vector3Int position) {
            return positionToBehavior[position];
        }

        public bool IsPositionOccupied(Vector3Int position) {
            MonoBehaviour behavior = positionToBehavior.GetValueOrDefault(position, null);
            if (behavior == null) {
                return false;
            }
            if (behavior.GetComponent<SceneExit>() != null) {
                return false;
            }
            return true;
        }

        public List<Vector3Int> GetInteractableAdjacentObjects(Vector3Int currPosition) {
            List<Vector3Int> occupiedVoxels = new List<Vector3Int>();
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {
                        Vector3Int checkPosition = currPosition + new Vector3Int(x, y, z);
                        if (checkPosition != currPosition && IsPositionOccupied(checkPosition)) {
                            NPCBehavior npcBehavior = 
                                positionToBehavior[checkPosition].GetComponent<NPCBehavior>();
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
