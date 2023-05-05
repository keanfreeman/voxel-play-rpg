using EntityDefinition;
using GameMechanics;
using MovementDirection;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instantiated {
    public abstract class Traveller : TangibleEntity {
        [SerializeField] public SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;
        [SerializeField] protected CameraManager cameraManager;
        [SerializeField] protected PartyManager partyManager;

        protected TravellerIdentitySO travellerIdentity;
        protected Vector3Int moveStartPoint;
        protected Vector3Int moveEndPoint;
        protected float moveStartTimestamp;
        private float moveFinishedTimestamp;
        public SpriteMoveDirection permanentMoveDirection { get; protected set; } = SpriteMoveDirection.NONE;

        public bool isMoving { get; protected set; } = false;
        public int currHP { get; protected set; }

        protected const float TIME_TO_MOVE_A_TILE = 0.2f;
        private const float ANIMATION_COOLDOWN_TIME = 0.1f;

        private void Update() {
            AnimateMove();
            if (!isMoving && Time.time - moveFinishedTimestamp > ANIMATION_COOLDOWN_TIME) {
                SetMoveAnimation(false);
                moveFinishedTimestamp = float.MaxValue;
            }
        }

        public void SetHP(int newValue) {
            currHP = newValue;
        }

        private void AnimateMove() {
            if (!isMoving && !cameraManager.isRotating && permanentMoveDirection != SpriteMoveDirection.NONE) {
                Vector3Int? direction = GetDestinationFromDirection(permanentMoveDirection);
                if (direction.HasValue) {
                    MoveOriginToPoint(direction.Value);
                }
            }
            if (isMoving) {
                float timeSinceMoveBegan = Time.time - moveStartTimestamp;
                float fractionOfMovementDone = Mathf.Min(timeSinceMoveBegan / (TIME_TO_MOVE_A_TILE), 1f);
                transform.position = Vector3.Lerp(moveStartPoint, moveEndPoint, fractionOfMovementDone);

                if (fractionOfMovementDone >= 1f) {
                    moveFinishedTimestamp = Time.time;
                    isMoving = false;
                }
            }
        }

        public void MoveOriginToPoint(Vector3Int point) {
            moveStartPoint = origin;

            nonVoxelWorld.RemovePositions(this);
            MoveOccupiedPositionsTo(point);
            nonVoxelWorld.SetPositions(this);

            moveEndPoint = point;

            moveStartTimestamp = Time.time;
            isMoving = true;
            SetMoveAnimation(isMoving);
            SetMoveDirectionRelativeToCamera();
        }

        public void SetMoveAnimation(bool state) {
            animator.SetBool("isMoving", state);
        }

        public bool IsAnimatingMove() {
            return animator.GetBool("isMoving");
        }

        private void SetMoveDirectionRelativeToCamera() {
            Direction cameraDirection = cameraManager.GetCameraApproximateDirection();
            Vector3Int diff = moveEndPoint - moveStartPoint;
            Direction absoluteCreatureMove = diff.z > 0 ? Direction.NORTH :
                diff.z < 0 ? Direction.SOUTH :
                diff.x > 0 ? Direction.EAST :
                Direction.WEST;
            if ((cameraDirection == Direction.NORTH && absoluteCreatureMove == Direction.EAST)
                    || (cameraDirection == Direction.EAST && absoluteCreatureMove == Direction.SOUTH)
                    || (cameraDirection == Direction.SOUTH && absoluteCreatureMove == Direction.WEST)
                    || (cameraDirection == Direction.WEST && absoluteCreatureMove == Direction.NORTH)) {
                spriteRenderer.flipX = true;
            }
            else if ((cameraDirection == Direction.NORTH && absoluteCreatureMove == Direction.WEST)
                    || (cameraDirection == Direction.EAST && absoluteCreatureMove == Direction.NORTH)
                    || (cameraDirection == Direction.SOUTH && absoluteCreatureMove == Direction.EAST)
                    || (cameraDirection == Direction.WEST && absoluteCreatureMove == Direction.SOUTH)) {
                spriteRenderer.flipX = false;
            }
        }

        public abstract void RotateSprite(float degrees);

        public StatsSO GetStats() {
            return travellerIdentity.stats;
        }

        protected abstract Vector3Int? GetDestinationFromDirection(SpriteMoveDirection spriteMoveDirection);
    }
}
