using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class DoMultipleOrder : Order
    {
        public OrderGroup orderGroup;

        public DoMultipleOrder(OrderGroup orders) {
            this.orderGroup = orders;
        }
    }
}
