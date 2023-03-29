
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;

namespace NonVoxelEntity {
    public class PlayerCharacter : Entity {
        public Stats stats;
        public SpriteLibraryAsset spriteLibraryAsset { get; set; }
        public Vector3 spriteScale { get; private set; }

        public PlayerCharacter(GameObject prefab, Vector3Int startPosition,
                Stats stats, SpriteLibraryAsset spriteLibraryAsset,
                Vector3 spriteScale)
                : base(prefab, startPosition) {
            this.stats = stats;
            this.spriteLibraryAsset = spriteLibraryAsset;
            this.spriteScale = spriteScale;
        }
    }
}
