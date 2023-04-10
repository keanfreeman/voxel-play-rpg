using NonVoxel;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InstantiatedEntity {
    public abstract class InstantiatedNVE : MonoBehaviour {
        [SerializeField] protected NonVoxelWorld nonVoxelWorld;

        // is the bottom-left point
        public Vector3Int origin { get; protected set; }
        public List<Vector3Int> occupiedPositions { get; protected set; }

        public void SetCurrPositions(Entity entityInfo) {
            this.origin = entityInfo.startPosition;

            occupiedPositions = new List<Vector3Int>();
            foreach (Vector3Int position in entityInfo.occupiedPositions) {
                Vector3Int pointInWorld = position + entityInfo.startPosition;
                occupiedPositions.Add(pointInWorld);
            }
        }

        public void MoveAllPoints(Vector3Int point) {
            nonVoxelWorld.RemovePositions(this);

            Vector3Int difference = point - origin;
            origin = point;

            List<Vector3Int> temp = new List<Vector3Int>();
            foreach (Vector3Int position in occupiedPositions) {
                temp.Add(position + difference);
            }
            occupiedPositions = temp;

            nonVoxelWorld.SetPositions(this);
        }

        public HashSet<Vector3Int> GetPositionsIfOriginAtPosition(Vector3Int newOrigin) {
            HashSet<Vector3Int> newPositions = new HashSet<Vector3Int>(occupiedPositions.Count);
            Vector3Int difference = newOrigin - origin;
            foreach (Vector3Int position in occupiedPositions) {
                newPositions.Add(position + difference);
            }

            return newPositions;
        }

        public abstract bool IsInteractable();
    }
}
