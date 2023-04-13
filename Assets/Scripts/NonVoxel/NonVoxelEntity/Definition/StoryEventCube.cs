using GameMechanics;
using NonVoxelEntity;
using Orders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public class StoryEventCube : Entity {
        public OrderGroup orderGroup { get; private set; }
        public int cubeRadius { get; private set; }

        public StoryEventCube(Vector3Int startPosition, int cubeRadius, EntityDisplay entityDisplay,
                OrderGroup orderGroup) : base(startPosition, entityDisplay) {
            this.orderGroup = orderGroup;
            this.cubeRadius = cubeRadius;
        }
    }
}
