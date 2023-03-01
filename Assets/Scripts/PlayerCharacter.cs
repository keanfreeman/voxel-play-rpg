using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstantiatedEntity {
    public class PlayerCharacter : MonoBehaviour
    {
        public PlayerStats stats;

        public void Init(NonVoxelEntity.PlayerCharacter playerCharacter) {
            this.stats = playerCharacter.stats;
        }
    }
}
