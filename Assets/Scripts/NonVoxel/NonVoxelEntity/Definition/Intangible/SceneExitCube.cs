using GameMechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public class SceneExitCube : IntangibleEntity {
        public EnvChangeDestination destination;

        public SceneExitCube(Vector3Int startPosition, EnvChangeDestination destination, 
                string prefabName) : base(startPosition, prefabName) {
            this.destination = destination;
        }
    }

    [Serializable]
    public class EnvChangeDestination {
        public int destinationEnv;
        public Vector3Int destinationTile;

        public EnvChangeDestination(int destinationEnvironment, Vector3Int destinationTile) {
            this.destinationEnv = destinationEnvironment;
            this.destinationTile = destinationTile;
        }
    }
}
