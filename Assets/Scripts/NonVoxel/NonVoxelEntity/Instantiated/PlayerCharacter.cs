using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementDirection;
using UnityEngine.InputSystem;
using NonVoxel;
using UnityEngine.U2D.Animation;
using Orders;

namespace Instantiated {
    public class PlayerCharacter : Traveller {
        [SerializeField] public GameObject playerObject;
        [SerializeField] public SpriteLibrary spriteLibrary;
        [SerializeField] SpriteMovement spriteMovement;

        private Direction playerCameraDirection = Direction.NORTH;

        private void Awake() {
            DontDestroyOnLoad(this);
        }

        private void OnDestroy() {
            StopAllCoroutines();
            enabled = false;
        }

        public void Init(SpriteMovement spriteMovement, EntityDefinition.PlayerCharacter playerInfo,
                TravellerIdentitySO identity, NonVoxelWorld nonVoxelWorld, 
                CameraManager cameraManager, PartyManager partyManager, FeatureManager featureManager,
                RandomManager randomManager) {
            this.spriteMovement = spriteMovement;
            this.entity = playerInfo;
            this.travellerIdentity = identity;
            this.nonVoxelWorld = nonVoxelWorld;
            this.cameraManager = cameraManager;
            this.partyManager = partyManager;
            this.spriteLibrary.spriteLibraryAsset = identity.spriteLibraryAsset;
            this.featureManager = featureManager;
            this.featureManager.SetUpFeatures(this);
            this.randomManager = randomManager;
        }

        public void HaltMovement() {
            permanentMoveDirection = SpriteMoveDirection.NONE;
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
            List<TangibleEntity> ignoredCreatures = new List<TangibleEntity> { this };
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

        public override bool IsInteractable() {
            return false;
        }

        public override void SetInteractionOrders(OrderGroup newOrders) {
            throw new System.NotImplementedException("Player interaction orders should not be set.");
        }

        public new EntityDefinition.PlayerCharacter GetEntity() {
            return (EntityDefinition.PlayerCharacter)entity;
        }

        public override EntityDefinition.Faction GetFaction() {
            return EntityDefinition.Faction.PLAYER;
        }
    }
}
