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

        public bool IsInteractable(Vector3Int position) {
            if (IsPositionOccupied(position)) {
                InstantiatedNVE entity = positionToEntity[position];
                if (entity.GetType() == typeof(NPCBehavior) && entity.IsInteractable()) {
                    return true;
                }
            }
            return false;
        }
    }
}
