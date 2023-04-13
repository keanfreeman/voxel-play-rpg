using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class ExclaimOrder : Order
    {
        public TangibleEntity exclaimingEntity { get; private set; }

        public ExclaimOrder(TangibleEntity exclaimingEntity) {
            this.exclaimingEntity = exclaimingEntity;
        }
    }
}
