using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public class InteractableVoxels : MonoBehaviour
{
    [SerializeField]
    private List<VoxelDefinition> interactableVoxelsList = new List<VoxelDefinition>();

    public HashSet<VoxelDefinition> interactableVoxels { get; private set; }

    public void Start() {
        this.interactableVoxels = new HashSet<VoxelDefinition>(interactableVoxelsList);
    }

    public void GetFunctionForVoxel(VoxelDefinition voxelDefinition) {
        // TODO return the thing it does
    }
}
