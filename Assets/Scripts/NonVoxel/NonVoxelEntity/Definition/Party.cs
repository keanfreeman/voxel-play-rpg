using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public class Party : Spawnable {
        public PlayerCharacter mainCharacter { get; private set; }
        public List<PlayerCharacter> members { get; private set; }

        public Party(PlayerCharacter mainCharacter, List<PlayerCharacter> members) {
            this.mainCharacter = mainCharacter;
            this.members = members;
        }
    }
}