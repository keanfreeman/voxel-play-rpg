using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;

public class VoxelWorld
{
    private VoxelPlayEnvironment environment;
    private InteractableVoxels interactableVoxels;

    public VoxelWorld(VoxelPlayEnvironment environment, InteractableVoxels interactableVoxels) {
        this.environment = environment;
        this.interactableVoxels = interactableVoxels;
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
