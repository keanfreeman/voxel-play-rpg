using MovementDirection;
using NonVoxel;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GameMechanics;
using NonVoxelEntity;

namespace Instantiated {
    public abstract class TangibleEntity : InstantiatedEntity {
        [SerializeField] protected NonVoxelWorld nonVoxelWorld;
        [SerializeField] public Transform rotationTransform;

        // is the bottom-left point
        public Vector3Int origin { get; protected set; }
        public List<Vector3Int> occupiedPositions { get; protected set; }

        public void SetCurrPositions(Vector3Int newOrigin, TravellerIdentitySO travellerID) {
            this.origin = newOrigin;
            transform.position = origin;

            this.occupiedPositions =  EntitySizeCalcs.GetPositionsFromSizeCategory(origin, 
                travellerID.stats.size);
        }

        public void SetCurrPositions(Vector3Int newOrigin, ObjectIdentitySO objectID, 
                Direction rotation) {
            this.origin = newOrigin;
            transform.position = origin;
            this.occupiedPositions = new();

            int numRotations = rotation == Direction.NORTH ? 0
                : rotation == Direction.WEST ? 1
                : rotation == Direction.SOUTH ? 2
                : 3;
            foreach (Vector3Int position in objectID.occupiedPositions) {
                Vector3Int rotatedPosition = Coordinates.RotatePointCounterClockwiseAroundCenter(
                    position, Vector3Int.zero, numRotations);
                Vector3Int pointInWorld = origin + rotatedPosition;
                this.occupiedPositions.Add(pointInWorld);
            }

            SetDisplayRotation(Coordinates.GetRotationFromAngle(
                DirectionCalcs.GetDegreesFromDirection(rotation)));
        }

        public void MoveOccupiedPositionsTo(Vector3Int point) {
            Vector3Int difference = point - origin;
            origin = point;

            List<Vector3Int> temp = new List<Vector3Int>();
            foreach (Vector3Int position in occupiedPositions) {
                temp.Add(position + difference);
            }
            occupiedPositions = temp;
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

        public new EntityDefinition.TangibleEntity GetEntity() {
            return (EntityDefinition.TangibleEntity)base.GetEntity();
        }
    }
}
