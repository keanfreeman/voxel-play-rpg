using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class MoveOrder : Order
    {
        public Vector3Int destination;
        public PlayerCharacter player;

        public MoveOrder(Vector3Int destination, PlayerCharacter player) {
            this.destination = destination;
            this.player = player;
        }
    }
}
