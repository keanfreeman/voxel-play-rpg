using Instantiated;
using EntityDefinition;
using Orders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Orders {
    [Serializable]
    public class MoveImmediateOrder : Order
    {
        public Vector3Int destination;
        public EntityDefinition.TangibleEntity entity;

        public MoveImmediateOrder(Vector3Int destination, EntityDefinition.TangibleEntity entity) {
            this.destination = destination;
            this.entity = entity;
        }
    }
}
