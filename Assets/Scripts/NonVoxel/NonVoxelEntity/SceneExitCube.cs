using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public class SceneExitCube : Entity {
        public Destination destination { get; private set; }

        public SceneExitCube(GameObject prefab, Vector3Int startPosition,
                Destination destination) : base(prefab, startPosition) {
            this.destination = destination;
        }
    }

    public class Destination {
        public SceneIndex destinationScene { get; private set; }
        public Vector3Int destinationTile { get; private set; }

        public Destination(SceneIndex destinationScene, Vector3Int destinationTile) {
            this.destinationScene = destinationScene;
            this.destinationTile = destinationTile;
        }
    }
}
