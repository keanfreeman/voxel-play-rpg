using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    public class SceneExitCube : IntangibleEntity {
        public EnvChangeDestination destination { get; private set; }

        public SceneExitCube(Vector3Int startPosition, EnvChangeDestination destination, 
                GameObject prefab) : base(startPosition, prefab) {
            this.destination = destination;
        }
    }

    public class EnvChangeDestination {
        public int destinationEnv { get; private set; }
        public Vector3Int destinationTile { get; private set; }

        public EnvChangeDestination(int destinationEnvironment, Vector3Int destinationTile) {
            this.destinationEnv = destinationEnvironment;
            this.destinationTile = destinationTile;
        }
    }
}
