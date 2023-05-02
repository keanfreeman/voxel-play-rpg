using NonVoxel;
using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instantiated {
    public class TangibleObject : TangibleEntity {
        [SerializeField] public Transform leafTransform;

        public EntityDefinition.TangibleObject objectInfo { get; private set; }

        private ObjectIdentitySO objectIdentity;

        public void Init(NonVoxelWorld nonVoxelWorld, EntityDefinition.TangibleObject nonVoxelObject,
                ObjectIdentitySO objectIdentity) {
            this.nonVoxelWorld = nonVoxelWorld;
            this.entity = nonVoxelObject;
            objectInfo = nonVoxelObject;
            this.objectIdentity = objectIdentity;
            SetCurrPositions(nonVoxelObject.spawnPosition, objectIdentity, nonVoxelObject.startRotation);
        }

        public override bool IsInteractable() {
            return true;
        }
    }
}
