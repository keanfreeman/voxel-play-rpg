using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace EntityDefinition {
    public class TravellerIdentity : Identity {
        public SpriteLibraryAsset spriteLibraryAsset;
        public Vector3 offset;
        public Vector3 scale;
        public Stats stats;

        public TravellerIdentity(GameObject prefab, SpriteLibraryAsset spriteLibraryAsset, Vector3 offset,
                Vector3 scale, Stats stats) : base(prefab) {
            this.spriteLibraryAsset = spriteLibraryAsset;
            this.offset = offset;
            this.scale = scale;
            this.stats = stats;
        }
    }
}
