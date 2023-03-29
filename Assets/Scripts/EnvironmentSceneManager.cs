using InstantiatedEntity;
using NonVoxelEntity;
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

    private Destination destination;

    void Awake() {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        destination = new Destination(3, Vector3Int.zero);
    }

    private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode) {
        VoxelPlayEnvironment environment = FindObjectOfType<VoxelPlayEnvironment>();
        if (environment == null) {
            // load scene after init scene
            SceneManager.LoadScene(3);
            return;
        }

        voxelWorldManager.SetVoxelPlayEnvironment(environment);

        nonVoxelManager.SetUpEntities(destination.destinationEnv);
        
        environment.cameraMain = cameraManager.GetMainCamera();
        cameraManager.AttachCameraToPlayer(partyManager.mainCharacter);

        partyManager.SetCurrControlledCharacter(partyManager.mainCharacter);
        
        gameStateManager.controlState = ControlState.SPRITE_NEUTRAL;
    }

    public void LoadNextScene(Destination destination) {
        gameStateManager.controlState = ControlState.LOADING;
        partyManager.currControlledCharacter.HaltMovement();
        this.destination = destination;
        voxelWorldManager.SetVoxelPlayEnvironment(null);
        nonVoxelManager.DestroyEntities();
        SceneManager.LoadScene(destination.destinationEnv);
    }
}
