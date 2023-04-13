using NonVoxelEntity;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstantiatedEntity {
    public class StoryEventCube : IntangibleEntity
    {
        private OrderManager orderManager;

        public NonVoxelEntity.StoryEventCube entityDefinition { get; private set; }

        public void Init(NonVoxelEntity.StoryEventCube entityDefinition, OrderManager orderManager) {
            this.entityDefinition = entityDefinition;
            this.orderManager = orderManager;
            this.transform.localScale *= entityDefinition.cubeRadius;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player") {
                orderManager.ExecuteOrders(entityDefinition.orderGroup);
                if (entityDefinition.orderGroup.destroyOnComplete) {
                    Destroy(this);
                }
            }
        }
    }
}
