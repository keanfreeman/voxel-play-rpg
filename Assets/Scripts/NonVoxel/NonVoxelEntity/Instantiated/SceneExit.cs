using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InstantiatedEntity {
    public class SceneExit : InstantiatedNVE
    {
        private EnvironmentSceneManager worldManager;
        private Destination destination;

        public void Init(EnvironmentSceneManager worldManager, Destination destination, 
                SceneExitCube sceneExitCubeInfo) {
            this.worldManager = worldManager;
            this.destination = destination;
            SetCurrPositions(sceneExitCubeInfo);
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player") {
                Debug.Log($"Entered cube going to {destination.destinationTile}, " +
                    $"{destination.destinationEnv}");
                worldManager.LoadNextScene(destination);
            }
        }

        public override bool IsInteractable() {
            return false;
        }
    }
}
