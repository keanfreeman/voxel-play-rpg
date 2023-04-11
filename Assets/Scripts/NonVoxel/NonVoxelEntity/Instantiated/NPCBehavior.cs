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
        private GameStateManager gameStateManager;

        public NPC npcInfo;
        public HashSet<NPCBehavior> teammates;
        public bool inCombat { get; set; } = false;

        private const int NPC_MIN_IDLE_TIME = 1;
        private const int NPC_MAX_IDLE_TIME = 3;

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player" && npcInfo.faction == Faction.ENEMY) {
                gameStateManager.EnterCombat(this);
            }
        }

        public void Init(NonVoxelWorld nonVoxelWorld, SpriteMovement spriteMovement,
                System.Random rng, NPC npcInfo, CameraManager cameraManager, 
                PartyManager partyManager, GameStateManager gameStateManager) {
            this.nonVoxelWorld = nonVoxelWorld;
            this.spriteMovement = spriteMovement;
            this.rng = rng;
            this.npcInfo = npcInfo;
            currHP = npcInfo.stats.hitPoints;
            this.cameraManager = cameraManager;
            this.spriteLibrary.spriteLibraryAsset = npcInfo.entityDisplay.spriteLibraryAsset;
            spriteObjectTransform.localScale = npcInfo.entityDisplay.scale;
            rotationTransform.localPosition = npcInfo.entityDisplay.offset;
            this.partyManager = partyManager;
            this.gameStateManager = gameStateManager;
            SetCurrPositions(npcInfo);

            StartCoroutine(MoveCoroutine());
        }

        private IEnumerator MoveCoroutine() {
            while (gameStateManager.controlState == ControlState.LOADING) {
                yield return new WaitForSeconds(1);
            }

            while (true) {
                if (inCombat) {
                    yield break;
                }

                Vector3Int newPosition = origin + GetRandomOneTileMovement();
                List<InstantiatedNVE> ignoredCreatures = new List<InstantiatedNVE> { this };
                Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
                    newPosition, this, ignoredCreatures);
                if (actualCoordinate.HasValue) {
                    Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();
                    if (spriteMovement.IsReachablePosition(destinationCoordinate, this, ignoredCreatures)) {
                        MoveOriginToPoint(destinationCoordinate);
                    }
                }

                double waitTime = (rng.NextDouble() * NPC_MAX_IDLE_TIME) + NPC_MIN_IDLE_TIME;
                yield return new WaitForSeconds((float)waitTime);
            }
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

        public override bool IsInteractable() {
            return true;
        }

        public override void RotateSprite(float degrees) {
            rotationTransform.Rotate(Vector3.up, degrees);
        }

        public override Stats GetStats() {
            return npcInfo.stats;
        }
    }
}
