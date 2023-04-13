using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class MoveOrder : Order
    {
        public Vector3Int destination { get; private set; }
        public PlayerCharacter player { get; private set; }

        public MoveOrder(Vector3Int destination, PlayerCharacter player) {
            this.destination = destination;
            this.player = player;
        }
    }
}
