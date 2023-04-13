using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class ExclaimOrder : Order
    {
        public Entity exclaimingEntity { get; private set; }

        public ExclaimOrder(Entity exclaimingEntity) {
            this.exclaimingEntity = exclaimingEntity;
        }
    }
}
