using GameMechanics;
using EntityDefinition;
using Orders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EntityDefinition {
    [Serializable]
    public class StoryEventCube : IntangibleEntity {
        public OrderGroup orderGroup;
        public int cubeRadius;

        public StoryEventCube(Vector3Int startPosition, int cubeRadius, string prefabName,
                OrderGroup orderGroup) : base(startPosition, prefabName) {
            this.orderGroup = orderGroup;
            this.cubeRadius = cubeRadius;
        }
    }
}
