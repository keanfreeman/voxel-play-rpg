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
using Orders;
using System;

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

        public abstract void SetInteractionOrders(OrderGroup newOrders);

        public new EntityDefinition.TangibleEntity GetEntity() {
            return (EntityDefinition.TangibleEntity)base.GetEntity();
        }

        public Vector3Int GetPointInEntityClosestTo(Vector3Int target) {
            List<Tuple<Vector3Int, float>> pointsSorted = occupiedPositions
                .Select((Vector3Int position) => new Tuple<Vector3Int, float>(position,
                    Coordinates.GetDirectLineLength(position, target)))
                .ToList();
            pointsSorted.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            return pointsSorted[0].Item1;
        }

        // todo - use a more efficient algorithm if necessary
        public Tuple<Vector3Int, Vector3Int> GetNearestPoints(TangibleEntity tangibleEntity) {
            Vector3Int? thisClosestPoint = null;
            Vector3Int? targetClosestPoint = null;
            float closestDistance = float.MaxValue;
            foreach (Vector3Int targetCurrPosition in tangibleEntity.occupiedPositions) {
                Vector3Int thisCurrPoint = GetPointInEntityClosestTo(targetCurrPosition);
                float currDistance = Coordinates.GetDirectLineLength(targetCurrPosition, thisCurrPoint);
                if (currDistance < closestDistance) {
                    thisClosestPoint = thisCurrPoint;
                    targetClosestPoint = targetCurrPosition;
                    closestDistance = currDistance;
                }
            }

            return new Tuple<Vector3Int, Vector3Int>(thisClosestPoint.Value, targetClosestPoint.Value);
        }
    }
}
