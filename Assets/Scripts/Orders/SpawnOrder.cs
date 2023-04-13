using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class SpawnOrder : Order {
        public Vector3Int destination { get; private set; }
        public TangibleEntity entity { get; private set; }

        public SpawnOrder(Vector3Int destination, TangibleEntity entity) {
            this.destination = destination;
            this.entity = entity;
        }
    }
}
