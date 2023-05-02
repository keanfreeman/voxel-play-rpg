using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Instantiated {
    public class SceneExitCube : IntangibleEntity
    {
        private EnvironmentSceneManager environmentSceneManager;
        private InputManager inputManager;
        private PartyManager partyManager;
        private EntityDefinition.EnvChangeDestination destination;

        public void Init(EnvironmentSceneManager worldManager, InputManager inputManager,
                PartyManager partyManager, EntityDefinition.EnvChangeDestination destination, 
                EntityDefinition.SceneExitCube sceneExitCubeInfo) {
            this.environmentSceneManager = worldManager;
            this.inputManager = inputManager;
            this.partyManager = partyManager;
            this.destination = destination;
            this.entity = sceneExitCubeInfo;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player") {
                Debug.Log($"Entered cube going to {destination.destinationTile}, " +
                    $"{destination.sceneIndex}");
                StartCoroutine(StopPlayerAndLoadScene(other));
            }
        }

        private IEnumerator StopPlayerAndLoadScene(Collider other) {
            inputManager.LockPlayerControls();
            PlayerCharacter pc = other.gameObject.GetComponent<PlayerCharacter>();
            pc.HaltMovement();
            while (pc.isMoving || pc.IsAnimatingMove()) {
                yield return null;
            }

            yield return environmentSceneManager.LoadNextScene(destination);
        }
    }
}
