using GameMechanics;
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

        private float lastMoveTime = 0;

        public bool encounteredPlayer = false;
        public NPC npcInfo;
        public HashSet<NPCBehavior> teammates;

        private const float NPC_MIN_IDLE_TIME = 1;
        private const float NPC_MAX_IDLE_TIME = 5;

        private void OnTriggerEnter(Collider other) {
            if (npcInfo.faction == Faction.ENEMY && other.gameObject.tag == "Player") {
                encounteredPlayer = true;
            }
        }

        public void Init(NonVoxelWorld nonVoxelWorld, SpriteMovement spriteMovement,
                System.Random rng, NPC npcInfo, CameraManager cameraManager, 
                PartyManager partyManager) {
            this.nonVoxelWorld = nonVoxelWorld;
            this.spriteMovement = spriteMovement;
            this.rng = rng;
            this.npcInfo = npcInfo;
            currHP = npcInfo.stats.hitPoints;
            this.cameraManager = cameraManager;
            this.spriteLibrary.spriteLibraryAsset = npcInfo.entityDisplay.spriteLibraryAsset;
            spriteObjectTransform.localScale = npcInfo.entityDisplay.spriteScale;
            rotationTransform.localPosition = npcInfo.entityDisplay.offset;
            this.partyManager = partyManager;
            SetCurrPositions(npcInfo);
        }

        protected override Vector3Int? GetDestinationFromDirection(SpriteMoveDirection spriteMoveDirection) {
            // only implemented for override purposes
            return spriteMovement.GetTerrainAdjustedCoordinate(origin + GetRandomOneTileMovement(),
                this, new List<InstantiatedNVE>{this});
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

            Vector3Int newPosition = origin + GetRandomOneTileMovement();
            List<InstantiatedNVE> ignoredCreatures = new List<InstantiatedNVE> { this };
            Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
                newPosition, this, ignoredCreatures);
            if (!actualCoordinate.HasValue) {
                return;
            }
            Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();

            if (spriteMovement.IsReachablePosition(destinationCoordinate, this, ignoredCreatures)) {
                MoveOriginToPoint(destinationCoordinate);
            }
        }

        public override bool IsInteractable() {
            return true;
        }

        public override void RotateSprite(float degrees) {
            rotationTransform.Rotate(Vector3.up, degrees);
        }

        public override void SetSpriteRotation(Vector3 rotation) {
            rotationTransform.rotation = Quaternion.Euler(rotation);
        }

        public override Stats GetStats() {
            return npcInfo.stats;
        }
    }
}
