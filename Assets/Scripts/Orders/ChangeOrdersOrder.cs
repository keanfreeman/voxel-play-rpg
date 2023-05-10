using EntityDefinition;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class ChangeOrdersOrder : Order
    {
        // todo - make possible with other things that can hold orders
        public Guid orderHolderGuid;
        public OrderGroup newOrders;

        [JsonConstructor]
        public ChangeOrdersOrder(Guid orderHolderGuid, OrderGroup newOrders) {
            this.orderHolderGuid = orderHolderGuid;
            this.newOrders = newOrders;
        }

        public ChangeOrdersOrder(TangibleEntity targetOrderHolder, OrderGroup newOrders) {
            this.orderHolderGuid = targetOrderHolder.guid;
            this.newOrders = newOrders;
        }
    }
}
