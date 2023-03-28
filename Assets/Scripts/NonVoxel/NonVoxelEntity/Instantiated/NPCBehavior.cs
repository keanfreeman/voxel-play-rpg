using MovementDirection;
using NonVoxel;
using NonVoxelEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using VoxelPlay;

namespace InstantiatedEntity {
    // moves around randomly
    public class NPCBehavior : Traveller {
        [SerializeField] SpriteLibrary spriteLibrary;
        [SerializeField] Transform spriteObjectTransform;

        private System.Random rng;
        private SpriteMovement spriteMovement;
        private Transform rotationTransform;

        private float lastMoveTime = 0;

        public bool encounteredPlayer = false;
        public NPC npcInfo;
        public HashSet<NPCBehavior> teammates;

        private const float NPC_MIN_IDLE_TIME = 1;
        private const float NPC_MAX_IDLE_TIME = 5;

        void Awake() {
            rotationTransform = transform.GetChild(0);
        }

        private void OnTriggerEnter(Collider other) {
            if (npcInfo.faction == Faction.ENEMY && other.gameObject.tag == "Player") {
                encounteredPlayer = true;
            }
        }

        public void Init(NonVoxelWorld nonVoxelWorld, SpriteMovement spriteMovement,
                System.Random rng, NPC npcInfo, CameraManager cameraManager) {
            this.nonVoxelWorld = nonVoxelWorld;
            this.spriteMovement = spriteMovement;
            this.rng = rng;
            this.npcInfo = npcInfo;
            currHP = npcInfo.stats.hpMax;
            this.cameraManager = cameraManager;
            this.spriteLibrary.spriteLibraryAsset = npcInfo.spriteLibraryAsset;
            spriteObjectTransform.localScale = npcInfo.spriteScale;
            currVoxel = nonVoxelWorld.GetPosition(this);
        }

        protected override Vector3Int? GetDestinationFromDirection(SpriteMoveDirection spriteMoveDirection) {
            // only implemented for override purposes
            return spriteMovement.GetTerrainAdjustedCoordinate(currVoxel, currVoxel + GetRandomOneTileMovement());
        }

        public Vector3Int GetRandomOneTileMovement() {
            bool isX = rng.Next(0, 2) == 0 ? true : false;
            if (isX) {
                return rng.Next(0, 2) == 0 ? Vector3Int.left : Vector3Int.right;
            }
            return rng.Next(0, 2) == 0 ? Vector3Int.forward : Vector3Int.back;
        }

        public void HandleRandomMovement() {
            if (cameraManager.isRotating || Time.time - lastMoveTime < NPC_MIN_IDLE_TIME) {
                return;
            }
            lastMoveTime = Time.time;

            Vector3Int newPosition = currVoxel + GetRandomOneTileMovement();
            Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
                newPosition, currVoxel);
            if (!actualCoordinate.HasValue) {
                return;
            }
            Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();

            if (!nonVoxelWorld.IsPositionOccupied(destinationCoordinate)
                    && spriteMovement.IsReachablePosition(destinationCoordinate, true)) {
                MoveToPoint(destinationCoordinate);
            }
        }

        public bool IsInteractable() {
            return true;
        }

        public override void RotateSprite(float degrees) {
            rotationTransform.Rotate(Vector3.up, degrees);
        }

        public override void SetSpriteRotation(Vector3 rotation) {
            rotationTransform.rotation = Quaternion.Euler(rotation);
        }
    }
}
