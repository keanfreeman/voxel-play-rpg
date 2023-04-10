using InstantiatedEntity;
using NonVoxelEntity;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld : MonoBehaviour {
        private HashSet<InstantiatedNVE> entities = new HashSet<InstantiatedNVE>();
        private Dictionary<Vector3Int, InstantiatedNVE> positionToEntity
            = new Dictionary<Vector3Int, InstantiatedNVE>();

        public HashSet<NPCBehavior> npcs = new HashSet<NPCBehavior>();

        public void AddEntity(InstantiatedNVE entity) {
            entities.Add(entity);
            foreach (Vector3Int position in entity.occupiedPositions) {
                if (positionToEntity.ContainsKey(position)) {
                    Debug.LogError($"Adding an entity {entity.name} to a position that's " +
                        $"already occupied by {positionToEntity[position].name}.");
                }
                positionToEntity[position] = entity;
            }
        }

        public void SetPositions(InstantiatedNVE entity) {
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = entity;
            }
        }

        public void RemovePositions(InstantiatedNVE entity) {
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = null;
            }
        }

        public void DestroyEntities() {
            foreach (InstantiatedNVE npc in npcs) {
                npc.enabled = false;
                Destroy(npc.gameObject);
            }
            npcs.Clear();

            foreach (InstantiatedNVE behavior in entities) {
                if (behavior.GetType() != typeof(PlayerMovement)) {
                    Destroy(behavior);
                }
            }
            entities.Clear();
            positionToEntity.Clear();
        }

        public InstantiatedNVE GetNVEFromPosition(Vector3Int position) {
            return positionToEntity.GetValueOrDefault(position, null);
        }

        public void OnDeleteEntity(InstantiatedNVE entity) {
            if (entity.GetType() == typeof(NPCBehavior)) {
                npcs.Remove((NPCBehavior)entity);
            }
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = null;
            }
            entities.Remove(entity);
        }

        public bool IsPositionOccupied(Vector3Int position) {
            InstantiatedNVE behavior = positionToEntity.GetValueOrDefault(position, null);
            if (behavior == null || behavior.GetType() == typeof(SceneExit)) {
                return false;
            }
            return true;
        }

        public bool IsPositionOccupied(Vector3Int position, InstantiatedNVE ignoredCreature) {
            return IsPositionOccupied(position, new List<InstantiatedNVE> { ignoredCreature });
        }

        public bool IsPositionOccupied(Vector3Int position, ICollection<InstantiatedNVE> ignoredCreatures) {
            InstantiatedNVE behavior = positionToEntity.GetValueOrDefault(position, null);
            if (behavior == null || ignoredCreatures.Contains(behavior) 
                    || behavior.GetType() == typeof(SceneExit)) {
                return false;
            }
            return true;
        }

        public List<Vector3Int> GetInteractableAdjacentObjects(Vector3Int currPosition, 
                PlayerMovement playerMovement) {
            List<Vector3Int> occupiedVoxels = new List<Vector3Int>();
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {
                        Vector3Int checkPosition = currPosition + new Vector3Int(x, y, z);
                        if (checkPosition != currPosition 
                                && IsPositionOccupied(checkPosition, playerMovement)) {
                            InstantiatedNVE entity = positionToEntity[checkPosition];
                            if (entity.GetType() == typeof(NPCBehavior) && entity.IsInteractable()) {
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
