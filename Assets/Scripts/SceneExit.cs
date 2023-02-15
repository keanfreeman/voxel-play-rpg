using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour
{
    private SceneChanger sceneChanger;
    private Destination destination;

    public void Init(SceneChanger sceneChanger, Destination destination) {
        this.sceneChanger = sceneChanger;
        this.destination = destination;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            SceneIndex currentScene = (SceneIndex)SceneManager.GetActiveScene().buildIndex;
            //todo - if in same scene, tell the player manager to moveimmediate to coordinates
            Debug.Log($"Entered cube from {currentScene} and going to {destination.destinationTile}, {destination.destinationScene}");
            sceneChanger.LoadNextScene(currentScene, destination);
        }
    }
}
