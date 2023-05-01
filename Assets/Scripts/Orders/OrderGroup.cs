using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class OrderGroup {
        public bool destroyOnComplete = true;
        public List<Order> orders = new List<Order>();

        public OrderGroup(bool destroyOnComplete, List<Order> orders) {
            this.destroyOnComplete = destroyOnComplete;
            this.orders = orders;
        }
    }
}
