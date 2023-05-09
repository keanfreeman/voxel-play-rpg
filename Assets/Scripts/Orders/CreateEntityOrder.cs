using EntityDefinition;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class CreateEntityOrder : Order {
        public Entity entity;

        public CreateEntityOrder(Entity entity) {
            this.entity = entity;
        }
    }
}
