using InstantiatedEntity;
using NonVoxelEntity;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld : MonoBehaviour {
        private Dictionary<InstantiatedNVE, Vector3Int> entityToPosition
            = new Dictionary<InstantiatedNVE, Vector3Int>();
        private Dictionary<Vector3Int, InstantiatedNVE> positionToEntity
            = new Dictionary<Vector3Int, InstantiatedNVE>();

        public HashSet<NPCBehavior> npcs = new HashSet<NPCBehavior>();

        public void DestroyEntities() {
            foreach (InstantiatedNVE npc in npcs) {
                npc.enabled = false;
                Destroy(npc.gameObject);
            }
            npcs.Clear();

            foreach (InstantiatedNVE behavior in entityToPosition.Keys) {
                if (behavior.GetType() != typeof(PlayerMovement)) {
                    Destroy(behavior);
                }
            }
            entityToPosition.Clear();
            positionToEntity.Clear();
        }

        public bool IsInWorld(InstantiatedNVE behavior) {
            return entityToPosition.ContainsKey(behavior);
        }

        public Vector3Int GetPosition(InstantiatedNVE behavior) {
            return entityToPosition[behavior];
        }

        public void SetPosition(InstantiatedNVE behavior, Vector3Int position) {
            if (entityToPosition.ContainsKey(behavior)) {
                Vector3Int oldPosition = entityToPosition[behavior];
                if (positionToEntity.ContainsKey(oldPosition)) {
                    positionToEntity.Remove(oldPosition);
                }
            }

            entityToPosition[behavior] = position;
            positionToEntity[position] = behavior;
        }

        public InstantiatedNVE GetNVEFromPosition(Vector3Int position) {
            return positionToEntity.GetValueOrDefault(position, null);
        }

        public void ResetPosition(Vector3Int position) {
            InstantiatedNVE entity = positionToEntity[position];
            if (entity.GetType() == typeof(NPCBehavior)) {
                npcs.Remove((NPCBehavior)entity);
            }
            entityToPosition.Remove(entity);
            positionToEntity.Remove(position);
        }

        public bool IsPositionOccupied(Vector3Int position) {
            InstantiatedNVE behavior = positionToEntity.GetValueOrDefault(position, null);
            if (behavior == null) {
                return false;
            }
            if (behavior.GetType() == typeof(SceneExit)) {
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
                                positionToEntity[checkPosition].GetComponent<NPCBehavior>();
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
