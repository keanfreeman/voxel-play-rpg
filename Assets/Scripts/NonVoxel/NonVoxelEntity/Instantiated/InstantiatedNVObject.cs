using NonVoxel;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstantiatedEntity {
    public class InstantiatedNVObject : InstantiatedNVE {
        public NonVoxelObject objectInfo { get; private set; }

        public void Init(NonVoxelWorld nonVoxelWorld, NonVoxelObject nonVoxelObject) {
            this.nonVoxelWorld = nonVoxelWorld;
            objectInfo = nonVoxelObject;
            SetCurrPositions(nonVoxelObject, nonVoxelObject.startRotation);
        }

        public override bool IsInteractable() {
            return true;
        }
    }
}
