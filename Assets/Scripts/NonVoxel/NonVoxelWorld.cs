using Instantiated;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld : MonoBehaviour {
        private HashSet<TangibleEntity> entities = new HashSet<TangibleEntity>();
        private Dictionary<Vector3Int, TangibleEntity> positionToEntity
            = new Dictionary<Vector3Int, TangibleEntity>();

        public HashSet<NPC> npcs = new HashSet<NPC>();
        public Dictionary<EntityDefinition.Spawnable, InstantiatedEntity> instantiationMap 
            = new Dictionary<EntityDefinition.Spawnable, InstantiatedEntity>();

        public InstantiatedEntity GetEntityFromDefinition(EntityDefinition.Spawnable definition) {
            return instantiationMap.GetValueOrDefault(definition, null);
        }

        public void AddEntity(TangibleEntity entity) {
            entities.Add(entity);
            foreach (Vector3Int position in entity.occupiedPositions) {
                if (positionToEntity.ContainsKey(position)) {
                    Debug.LogError($"Adding an entity {entity.name} to a position that's " +
                        $"already occupied by {positionToEntity[position].name}.");
                }
                positionToEntity[position] = entity;
            }
        }

        public void SetPositions(TangibleEntity entity) {
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = entity;
            }
        }

        public void RemovePositions(TangibleEntity entity) {
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = null;
            }
        }

        public void DestroyEntities() {
            foreach (TangibleEntity npc in npcs) {
                npc.enabled = false;
                Destroy(npc.gameObject);
            }
            npcs.Clear();

            foreach (TangibleEntity behavior in entities) {
                if (behavior.GetType() != typeof(PlayerCharacter)) {
                    Destroy(behavior);
                }
            }
            entities.Clear();
            positionToEntity.Clear();
            instantiationMap.Clear();
        }

        public TangibleEntity GetEntityFromPosition(Vector3Int position) {
            return positionToEntity.GetValueOrDefault(position, null);
        }

        public void OnDeleteEntity(TangibleEntity entity) {
            if (entity.GetType() == typeof(NPC)) {
                npcs.Remove((NPC)entity);
            }
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = null;
            }
            entities.Remove(entity);

            if (instantiationMap.ContainsKey(entity.GetEntity())) {
                instantiationMap[entity.GetEntity()] = null;
            }
        }

        public bool IsPositionOccupied(Vector3Int position) {
            TangibleEntity behavior = positionToEntity.GetValueOrDefault(position, null);
            if (behavior == null || behavior.GetType() == typeof(SceneExitCube)) {
                return false;
            }
            return true;
        }

        public bool IsPositionOccupied(Vector3Int position, TangibleEntity ignoredCreature) {
            return IsPositionOccupied(position, new List<TangibleEntity> { ignoredCreature });
        }

        public bool IsPositionOccupied(Vector3Int position, ICollection<TangibleEntity> ignoredCreatures) {
            TangibleEntity behavior = positionToEntity.GetValueOrDefault(position, null);
            if (behavior == null || ignoredCreatures.Contains(behavior) 
                    || behavior.GetType() == typeof(SceneExitCube)) {
                return false;
            }
            return true;
        }

        public bool IsInteractable(Vector3Int position) {
            if (IsPositionOccupied(position)) {
                TangibleEntity entity = positionToEntity[position];
                if (entity.GetType() == typeof(NPC) && entity.IsInteractable()) {
                    return true;
                }
            }
            return false;
        }
    }
}
