using Instantiated;
using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelPlay;

public class EnvironmentSceneManager : MonoBehaviour
{
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] NonVoxelManager nonVoxelManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] ConstructionUI constructionUI;

    private EnvChangeDestination destination;

    void Awake() {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        destination = new EnvChangeDestination(3, Vector3Int.zero);
    }

    private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode) {
        VoxelPlayEnvironment environment = FindObjectOfType<VoxelPlayEnvironment>();
        if (environment == null) {
            // load scene after init scene
            SceneManager.LoadScene(3);
            return;
        }

        voxelWorldManager.SetVoxelPlayEnvironment(environment);
        voxelWorldManager.AssignEvent(constructionUI.OnEnvInitialized);

        nonVoxelManager.SetUpEntities(destination.destinationEnv);
        
        environment.cameraMain = cameraManager.GetMainCamera();
        cameraManager.AttachCameraToPlayer(partyManager.mainCharacter);

        partyManager.SetCurrControlledCharacter(partyManager.mainCharacter);
        
        StartCoroutine(gameStateManager.SetControlState(ControlState.SPRITE_NEUTRAL));
    }

    public void LoadNextScene(EnvChangeDestination destination) {
        gameStateManager.SetControlState(ControlState.LOADING);
        partyManager.currControlledCharacter.HaltMovement();
        this.destination = destination;
        voxelWorldManager.SetVoxelPlayEnvironment(null);
        nonVoxelManager.DestroyEntities();
        SceneManager.LoadScene(destination.destinationEnv);
    }
}
