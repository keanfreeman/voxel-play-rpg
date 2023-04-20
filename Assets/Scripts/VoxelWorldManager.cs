using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;

public class VoxelWorldManager : MonoBehaviour
{
    [SerializeField] private EnvironmentSceneManager envSceneManager;
    [SerializeField] private InteractableVoxels interactableVoxels;

    private VoxelPlayEnvironment environment;

    public VoxelPlayEnvironment GetEnvironment() {
        return environment;
    }

    public void AssignEvent(VoxelPlayEvent action) {
        environment.OnInitialized += action;
    }

    public void SetVoxelPlayEnvironment(VoxelPlayEnvironment environment) {
        this.environment = environment;
    }

    public Voxel GetVoxelFromPosition(Vector3d position) {
        return environment.GetVoxel(position);
    }

    public List<Vector3d> GetInteractableAdjacentVoxels(Vector3d currPosition) {
        List<Vector3d> interactableAdjacentVoxels = new List<Vector3d>();
        for (int x = -1; x < 2; x++) {
            for (int y = -1; y < 2; y++) {
                for (int z = -1; z < 2; z++) {
                    Vector3d checkPosition = currPosition + new Vector3d(x, y, z);
                    if (checkPosition == currPosition) {
                        continue;
                    }

                    Voxel voxel = environment.GetVoxel(checkPosition);
                    if (voxel == Voxel.Empty) {
                        continue;
                    }

                    VoxelDefinition type = voxel.type;
                    if (interactableVoxels.interactableVoxels.Contains(type)) {
                        interactableAdjacentVoxels.Add(checkPosition);
                    }
                }
            }
        }

        return interactableAdjacentVoxels;
    }
}
