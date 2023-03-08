using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InstantiatedEntity {
    public class SceneExit : MonoBehaviour
    {
        private EnvironmentSceneManager worldManager;
        private Destination destination;

        public void Init(EnvironmentSceneManager worldManager, Destination destination) {
            this.worldManager = worldManager;
            this.destination = destination;
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
