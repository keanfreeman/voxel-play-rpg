using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Instantiated {
    public class SceneExitCube : IntangibleEntity
    {
        private EnvironmentSceneManager worldManager;
        private EntityDefinition.EnvChangeDestination destination;

        public void Init(EnvironmentSceneManager worldManager, 
                EntityDefinition.EnvChangeDestination destination, 
                EntityDefinition.SceneExitCube sceneExitCubeInfo) {
            this.worldManager = worldManager;
            this.destination = destination;
            this.entity = sceneExitCubeInfo;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player") {
                Debug.Log($"Entered cube going to {destination.destinationTile}, " +
                    $"{destination.destinationEnv}");
                worldManager.LoadNextScene(destination);
            }
        }
    }
}
