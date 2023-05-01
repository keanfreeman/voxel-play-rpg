using Instantiated;
using Saving;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld : MonoBehaviour {
        [SerializeField] EnvironmentSceneManager environmentSceneManager;

        public Dictionary<EntityDefinition.Entity, InstantiatedEntity> instantiationMap { get; private set; }
            = new Dictionary<EntityDefinition.Entity, InstantiatedEntity>();
        private Dictionary<Vector3Int, InstantiatedEntity> positionToEntity
            = new Dictionary<Vector3Int, InstantiatedEntity>();

        public HashSet<NPC> npcs { get; private set; } = new HashSet<NPC>();

        public InstantiatedEntity GetEntityFromDefinition(EntityDefinition.Entity definition) {
            return instantiationMap.GetValueOrDefault(definition, null);
        }

        public void AddTangibleEntity(EntityDefinition.TangibleEntity entityDefinition, 
                TangibleEntity entityInstantiation) {
            instantiationMap[entityDefinition] = entityInstantiation;
            if (entityDefinition.GetType() == typeof(EntityDefinition.NPC)) {
                npcs.Add((NPC)entityInstantiation);
            }

            foreach (Vector3Int position in entityInstantiation.occupiedPositions) {
                if (positionToEntity.ContainsKey(position)) {
                    Debug.LogError($"Adding an entity {entityInstantiation.name} to a position that's " +
                        $"already occupied by {positionToEntity[position].name}.");
                }
                positionToEntity[position] = entityInstantiation;
            }
        }

        public void AddIntangibleEntity(EntityDefinition.IntangibleEntity intangibleEntityDef,
                IntangibleEntity intangibleEntityInstantiation) {
            instantiationMap.Add(intangibleEntityDef, intangibleEntityInstantiation);
        }

        public void SetPositions(TangibleEntity entity) {
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = entity;
            }
        }

        public void RemovePositions(TangibleEntity entity) {
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity.Remove(position);
            }
        }

        public IEnumerator DestroyAllEntities(bool includePlayer) {
            foreach (InstantiatedEntity entity in instantiationMap.Values) {
                if (includePlayer || entity.GetType() != typeof(PlayerCharacter)) {
                    Destroy(entity.gameObject);
                }
            }

            yield return new WaitForEndOfFrame();

            npcs.Clear();
            positionToEntity.Clear();
            instantiationMap.Clear();
        }

        public InstantiatedEntity GetEntityFromPosition(Vector3Int position) {
            return positionToEntity.GetValueOrDefault(position, null);
        }

        public void OnDeleteEntity(TangibleEntity entity) {
            if (entity.GetType() == typeof(NPC)) {
                npcs.Remove((NPC)entity);
            }
            foreach (Vector3Int position in entity.occupiedPositions) {
                positionToEntity[position] = null;
            }

            if (instantiationMap.ContainsKey(entity.GetEntity())) {
                instantiationMap[entity.GetEntity()] = null;
            }
        }

        public bool IsPositionOccupied(Vector3Int position) {
            InstantiatedEntity behavior = positionToEntity.GetValueOrDefault(position, null);
            return behavior != null && TypeUtils.IsSameTypeOrIsSubclass(behavior, typeof(TangibleEntity));
        }

        public bool IsPositionOccupied(Vector3Int position, TangibleEntity ignoredCreature) {
            return IsPositionOccupied(position, new List<TangibleEntity> { ignoredCreature });
        }

        public bool IsPositionOccupied(Vector3Int position, ICollection<TangibleEntity> ignoredCreatures) {
            InstantiatedEntity behavior = positionToEntity.GetValueOrDefault(position, null);
            return behavior != null && !ignoredCreatures.Contains(behavior)
                && TypeUtils.IsSameTypeOrIsSubclass(behavior, typeof(TangibleEntity));
        }

        public TangibleEntity GetInteractableEntity(Vector3Int position) {
            if (IsPositionOccupied(position)) {
                InstantiatedEntity entity = positionToEntity[position];
                if (entity.GetType() == typeof(NPC)) {
                    return (TangibleEntity)entity;
                }
            }
            return null;
        }
    }
}
