using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class OrderGroup {
        public bool destroyOnComplete { get; protected set; } = true;
        public List<Order> orders { get; private set; } = new List<Order>();

        public OrderGroup(bool destroyOnComplete, List<Order> orders) {
            this.destroyOnComplete = destroyOnComplete;
            this.orders = orders;
        }
    }
}
