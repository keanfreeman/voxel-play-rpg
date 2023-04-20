using MovementDirection;
using NonVoxel;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Instantiated {
    public abstract class TangibleEntity : InstantiatedEntity {
        [SerializeField] protected NonVoxelWorld nonVoxelWorld;
        [SerializeField] public Transform rotationTransform;

        // is the bottom-left point
        public Vector3Int origin { get; protected set; }
        public List<Vector3Int> occupiedPositions { get; protected set; }

        public void SetCurrPositions(EntityDefinition.TangibleEntity entityInfo) {
            SetCurrPositions(entityInfo, Direction.NORTH);
        }

        public void SetCurrPositions(EntityDefinition.TangibleEntity entityInfo, Direction rotation) {
            this.origin = entityInfo.startPosition;

            int numRotations = rotation == Direction.NORTH ? 0
                : rotation == Direction.WEST ? 1
                : rotation == Direction.SOUTH ? 2
                : 3;
            occupiedPositions = new List<Vector3Int>(entityInfo.occupiedPositions.Count);
            foreach (Vector3Int position in entityInfo.occupiedPositions) {
                Vector3Int rotatedPosition = Coordinates.RotatePointCounterClockwiseAroundCenter(
                    position, Vector3Int.zero, numRotations);
                Vector3Int pointInWorld = entityInfo.startPosition + rotatedPosition;
                occupiedPositions.Add(pointInWorld);
            }

            SetDisplayRotation(Coordinates.GetRotationFromAngle(
                DirectionCalcs.GetDegreesFromDirection(rotation)));
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

        public void SetDisplayRotation(Quaternion rotation) {
            rotationTransform.rotation = rotation;
        }

        public abstract bool IsInteractable();
    }
}
