using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class ExclaimOrder : Order
    {
        public TangibleEntity exclaimingEntity;

        public ExclaimOrder(TangibleEntity exclaimingEntity) {
            this.exclaimingEntity = exclaimingEntity;
        }
    }
}
