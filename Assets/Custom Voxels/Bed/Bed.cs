using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public enum CustomVoxel {
    BED
}

public class Bed : VoxelPlayInteractiveObject {
    public const CustomVoxel CUSTOM_VOXEL = CustomVoxel.BED;

    public new void OnDestroy() {
        Debug.Log("tried my own OnDestroy");

        base.OnDestroy();
    }
}
