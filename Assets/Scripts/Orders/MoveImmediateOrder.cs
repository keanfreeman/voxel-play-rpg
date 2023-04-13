using Instantiated;
using EntityDefinition;
using Orders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class MoveImmediateOrder : Order
    {
        public Vector3Int destination { get; private set; }
        public EntityDefinition.TangibleEntity entity { get; private set; }

        public MoveImmediateOrder(Vector3Int destination, EntityDefinition.TangibleEntity entity) {
            this.destination = destination;
            this.entity = entity;
        }
    }
}
