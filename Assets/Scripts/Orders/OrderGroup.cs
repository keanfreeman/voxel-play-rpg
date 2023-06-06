using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class OrderGroup {
        public bool destroyOnComplete;
        public List<Order> orders;

        [JsonConstructor]
        public OrderGroup(List<Order> orders, bool destroyOnComplete) {
            this.orders = orders;
            this.destroyOnComplete = destroyOnComplete;
        }

        public OrderGroup(List<Order> orders) {
            this.orders = orders;
            this.destroyOnComplete = true;
        }

        public OrderGroup(Order order) {
            this.orders = new List<Order> { order };
            this.destroyOnComplete = false;
        }
    }
}
