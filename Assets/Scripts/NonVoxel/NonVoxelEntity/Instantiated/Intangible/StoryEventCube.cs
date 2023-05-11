using EntityDefinition;
using NonVoxel;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instantiated {
    public class StoryEventCube : IntangibleEntity
    {
        private OrderManager orderManager;
        private NonVoxelWorld nonVoxelWorld;
        private PartyManager partyManager;
        private InputManager inputManager;

        public EntityDefinition.StoryEventCube cubeInfo { get; private set; }

        public void Init(EntityDefinition.StoryEventCube entityDefinition, OrderManager orderManager,
                NonVoxelWorld nonVoxelWorld, PartyManager partyManager, InputManager inputManager) {
            this.entity = entityDefinition;
            this.cubeInfo = entityDefinition;
            this.orderManager = orderManager;
            this.nonVoxelWorld = nonVoxelWorld;
            this.partyManager = partyManager;
            this.inputManager = inputManager;
            this.transform.localScale *= entityDefinition.cubeRadius;
        }

        private void OnTriggerEnter(Collider other) {
            PlayerCharacter pc = other.gameObject.GetComponent<PlayerCharacter>();
            if (pc != null && partyManager.currControlledCharacter == pc) {
                inputManager.LockPlayerControls();
                pc.HaltMovement();

                orderManager.ExecuteOrders(cubeInfo.orderGroup);
                if (cubeInfo.orderGroup.destroyOnComplete) {
                    nonVoxelWorld.DestroyEntity(this);
                }
            }
        }
    }
}
