using MovementDirection;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstantiatedEntity {
    public abstract class Traveller : InstantiatedNVE {
        [SerializeField] protected NonVoxelWorld nonVoxelWorld;
        [SerializeField] protected Animator animator;
        [SerializeField] protected CameraManager cameraManager;

        protected Vector3Int moveStartPoint;
        protected Vector3Int moveEndPoint;
        protected float moveStartTimestamp;
        private float moveFinishedTimestamp;
        public SpriteMoveDirection permanentMoveDirection { get; protected set; } = SpriteMoveDirection.NONE;

        public bool isMoving { get; protected set; }
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
                    MoveToPoint(direction.Value);
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

        public void MoveToPoint(Vector3Int point) {
            nonVoxelWorld.SetPosition(this, point);
            moveStartPoint = currVoxel;
            moveEndPoint = point;
            currVoxel = point;
            moveStartTimestamp = Time.time;
            isMoving = true;
            SetMoveAnimation(isMoving);
        }

        public void SetMoveAnimation(bool state) {
            animator.SetBool("isMoving", state);
        }

        public abstract void RotateSprite(float degrees);

        public abstract void SetSpriteRotation(Vector3 rotation);

        protected abstract Vector3Int? GetDestinationFromDirection(SpriteMoveDirection spriteMoveDirection);
    }
}