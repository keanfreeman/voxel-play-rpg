using NonVoxel;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instantiated {
    public class TangibleObject : TangibleEntity {
        public EntityDefinition.TangibleObject objectInfo { get; private set; }

        public void Init(NonVoxelWorld nonVoxelWorld, EntityDefinition.TangibleObject nonVoxelObject) {
            this.nonVoxelWorld = nonVoxelWorld;
            objectInfo = nonVoxelObject;
            SetCurrPositions(nonVoxelObject, nonVoxelObject.startRotation);
        }

        public override bool IsInteractable() {
            return true;
        }
    }
}
