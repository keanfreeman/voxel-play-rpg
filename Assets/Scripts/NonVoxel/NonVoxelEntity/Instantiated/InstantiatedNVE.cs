using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstantiatedEntity {
    // Instantiated NonVoxelEntity
    public abstract class InstantiatedNVE : MonoBehaviour {
        public Vector3Int currVoxel { get; protected set; }
    }
}
