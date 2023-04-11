using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementDirection;
using UnityEngine.InputSystem;
using GameMechanics;
using NonVoxel;
using UnityEngine.U2D.Animation;
using System.Linq;
using VoxelPlay;
using UnityEditorInternal.VersionControl;
using Ink.Runtime;

namespace InstantiatedEntity {
    public class PlayerMovement : Traveller {
        [SerializeField] public GameObject playerObject;
        [SerializeField] public GameObject seeThroughTarget;
        [SerializeField] public SpriteLibrary spriteLibrary;
        [SerializeField] SpriteMovement spriteMovement;

        public NonVoxelEntity.PlayerCharacter playerInfo { get; private set; }

        private Direction playerCameraDirection = Direction.NORTH;

        public void Init(SpriteMovement spriteMovement, NonVoxelEntity.PlayerCharacter playerInfo,
                NonVoxelWorld nonVoxelWorld, CameraManager cameraManager, PartyManager partyManager) {
            this.spriteMovement = spriteMovement;
            this.playerInfo = playerInfo;
            this.nonVoxelWorld = nonVoxelWorld;
            this.cameraManager = cameraManager;
            this.partyManager = partyManager;
        }

        public void HaltMovement() {
            StopAllCoroutines();
            isMoving = false;
            SetMoveAnimation(isMoving);
        }

        public void HandleControllerMove(InputAction.CallbackContext obj) {
            Vector2 stickValue = obj.ReadValue<Vector2>();

            SpriteMoveDirection requestedDirection;
            float angle = Vector2.SignedAngle(stickValue, Vector2.down);
            if (angle > 45 && angle <= 135) {
                requestedDirection = SpriteMoveDirection.LEFT;
            }
            else if (angle > -45 && angle <= 45) {
                requestedDirection = SpriteMoveDirection.BACK;
            }
            else if (angle > -135 && angle <= -45) {
                requestedDirection = SpriteMoveDirection.RIGHT;
            }
            else {
                requestedDirection = SpriteMoveDirection.FORWARD;
            }
            permanentMoveDirection = requestedDirection;
        }

        public void HandleControllerMoveCancel(InputAction.CallbackContext obj) {
            permanentMoveDirection = SpriteMoveDirection.NONE;
        }

        protected override Vector3Int? GetDestinationFromDirection(SpriteMoveDirection direction) {
            SpriteMoveDirection cameraAdjustedPlayerMove = CameraAdjustedPlayerMove(
                direction, playerCameraDirection);
            Vector3Int desiredCoordinate = spriteMovement.GetSpriteDesiredCoordinate(
                this, cameraAdjustedPlayerMove);
            List<InstantiatedNVE> ignoredCreatures = new List<InstantiatedNVE> { this };
            Vector3Int ? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
                desiredCoordinate, this, ignoredCreatures);
            if (!actualCoordinate.HasValue) {
                Debug.Log("Not a valid destination terrain-wise.");
                return null;
            }
            Vector3Int destinationCoordinate = actualCoordinate.Value;
            if (nonVoxelWorld.IsPositionOccupied(destinationCoordinate, this)) {
                Debug.Log("Tried to move into a non-voxel-occupied space.");
                return null;
            }

            return destinationCoordinate;
        }

        private SpriteMoveDirection CameraAdjustedPlayerMove(SpriteMoveDirection moveDirection,
                Direction playerCameraDirection) {
            if (DirectionCalcs.isMovingNorth(moveDirection, playerCameraDirection)) {
                return SpriteMoveDirection.FORWARD;
            }
            else if (DirectionCalcs.isMovingEast(moveDirection, playerCameraDirection)) {
                return SpriteMoveDirection.RIGHT;
            }
            else if (DirectionCalcs.isMovingSouth(moveDirection, playerCameraDirection)) {
                return SpriteMoveDirection.BACK;
            }
            else if (DirectionCalcs.isMovingWest(moveDirection, playerCameraDirection)) {
                return SpriteMoveDirection.LEFT;
            }
            throw new System.ArgumentException("Impossible direction provided.");
        }

        public void RotateCameraDirection(float direction) {
            playerCameraDirection = DirectionCalcs.RotateCameraDirection(direction,
                playerCameraDirection);
        }

        public void SetPlayerCameraDirection(Direction direction) {
            playerCameraDirection = direction;
        }

        public override void RotateSprite(float degrees) {
            rotationTransform.Rotate(Vector3.up, degrees);
        }

        public override Stats GetStats() {
            return playerInfo.stats;
        }

        public override bool IsInteractable() {
            return false;
        }
    }
}
