using GameMechanics;
using EntityDefinition;
using Orders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public class StoryEventCube : IntangibleEntity {
        public OrderGroup orderGroup { get; private set; }
        public int cubeRadius { get; private set; }

        public StoryEventCube(Vector3Int startPosition, int cubeRadius, GameObject prefab,
                OrderGroup orderGroup) : base(startPosition, prefab) {
            this.orderGroup = orderGroup;
            this.cubeRadius = cubeRadius;
        }
    }
}
