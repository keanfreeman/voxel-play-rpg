using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class SpawnOrder : Order {
        public Vector3Int destination;
        public TangibleEntity entity;

        public SpawnOrder(Vector3Int destination, TangibleEntity entity) {
            this.destination = destination;
            this.entity = entity;
        }
    }
}
