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

        public EntityDefinition.StoryEventCube cubeInfo { get; private set; }

        public void Init(EntityDefinition.StoryEventCube entityDefinition, OrderManager orderManager,
                NonVoxelWorld nonVoxelWorld) {
            this.entity = entityDefinition;
            this.cubeInfo = entityDefinition;
            this.orderManager = orderManager;
            this.nonVoxelWorld = nonVoxelWorld;
            this.transform.localScale *= entityDefinition.cubeRadius;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player") {
                orderManager.ExecuteOrders(cubeInfo.orderGroup);
                if (cubeInfo.orderGroup.destroyOnComplete) {
                    nonVoxelWorld.DestroyEntity(this);
                }
            }
        }
    }
}
