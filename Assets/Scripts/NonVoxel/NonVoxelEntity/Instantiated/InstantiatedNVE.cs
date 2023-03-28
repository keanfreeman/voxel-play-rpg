using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstantiatedEntity {
    public abstract class InstantiatedNVE : MonoBehaviour {
        public Vector3Int currVoxel { get; protected set; }
    }
}
