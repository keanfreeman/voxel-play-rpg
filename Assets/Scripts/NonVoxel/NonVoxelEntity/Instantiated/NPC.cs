using GameMechanics;
using MovementDirection;
using NonVoxel;
using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using VoxelPlay;
using System.Linq;
using UnityEngine.UIElements;
using Orders;

namespace Instantiated {
    public class NPC : Traveller {
        [SerializeField] SpriteLibrary spriteLibrary;
        [SerializeField] Transform spriteObjectTransform;

        private System.Random rng;
        private SpriteMovement spriteMovement;
        private GameStateManager gameStateManager;

        public HashSet<NPC> teammates;
        public bool inCombat { get; set; } = false;

        private const int NPC_MIN_IDLE_TIME = 1;
        private const int NPC_MAX_IDLE_TIME = 3;
        private const int NPC_MAX_WANDER_DISTANCE = 2;

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player" && GetEntity().faction == Faction.ENEMY) {
                gameStateManager.EnterCombat(this);
            }
        }

        private void OnDestroy() {
            StopAllCoroutines();
            enabled = false;
        }

        public void Init(NonVoxelWorld nonVoxelWorld, SpriteMovement spriteMovement,
                System.Random rng, EntityDefinition.NPC npcInfo, TravellerIdentitySO identity, 
                CameraManager cameraManager, PartyManager partyManager, 
                GameStateManager gameStateManager, FeatureManager featureManager, 
                RandomManager randomManager, VisualRollManager visualRollManager) {
            this.nonVoxelWorld = nonVoxelWorld;
            this.spriteMovement = spriteMovement;
            this.rng = rng;
            this.entity = npcInfo;
            this.travellerIdentity = identity;
            currHP = GetStats().hitPoints;
            this.cameraManager = cameraManager;
            this.spriteLibrary.spriteLibraryAsset = identity.spriteLibraryAsset;
            spriteObjectTransform.localScale = identity.scale;
            rotationTransform.localPosition = identity.offset;
            this.partyManager = partyManager;
            this.gameStateManager = gameStateManager;
            this.featureManager = featureManager;
            this.featureManager.SetUpFeatures(this);
            this.randomManager = randomManager;
            this.visualRollManager = visualRollManager;
            Vector3Int spawn = npcInfo.currSpawnPosition.HasValue ? npcInfo.currSpawnPosition.Value
                : npcInfo.spawnPosition;
            SetCurrPositions(spawn, identity);

            if (GetEntity().idleBehavior == IdleBehavior.WANDER) {
                StartCoroutine(MoveCoroutine());
            }
        }

        private IEnumerator MoveCoroutine() {
            while (gameStateManager.controlState == ControlState.LOADING) {
                yield return new WaitForSeconds(1);
            }

            while (!inCombat && enabled) {
                // find reachable positions
                List<Vector3Int> adjacentPositions = Coordinates.GetAdjacentCoordinates(origin);
                List<TangibleEntity> ignoredCreatures = new List<TangibleEntity> { this };
                List<Vector3Int> reachablePositions = adjacentPositions
                    .Where((Vector3Int target) => {
                        int numPointsBetween = Coordinates.NumPointsBetween(target, GetEntity().spawnPosition);
                        return numPointsBetween <= NPC_MAX_WANDER_DISTANCE;
                    })
                    .Where((Vector3Int target) => 
                        spriteMovement.IsReachablePosition(target, this, ignoredCreatures))
                    .ToList();

                // select one at random
                if (reachablePositions.Count > 0) {
                    Vector3Int chosen = reachablePositions[rng.Next(0, reachablePositions.Count)];
                    MoveOriginToPoint(chosen);
                }

                double waitTime = (rng.NextDouble() * NPC_MAX_IDLE_TIME) + NPC_MIN_IDLE_TIME;
                yield return new WaitForSeconds((float)waitTime);
            }
        }

        // only implemented for override purposes
        protected override Vector3Int? GetDestinationFromDirection(SpriteMoveDirection spriteMoveDirection) {
            throw new NotImplementedException();
        }

        public override bool IsInteractable() {
            OrderGroup orderGroup = GetEntity().interactOrders;
            return orderGroup != null && orderGroup.orders.Count > 0;
        }

        public override void SetInteractionOrders(OrderGroup newOrders) {
            GetEntity().interactOrders = newOrders;
        }

        public override void RotateSprite(float degrees) {
            rotationTransform.Rotate(Vector3.up, degrees);
        }

        public new EntityDefinition.NPC GetEntity() {
            return (EntityDefinition.NPC)entity;
        }

        public override Faction GetFaction() {
            return GetEntity().faction;
        }
    }
}
