using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace GameMechanics {
    public class EntityDisplay
    {
        public GameObject prefab { get; private set; }
        public SpriteLibraryAsset spriteLibraryAsset { get; private set; }
        public Vector3 offset { get; private set; }
        public Vector3 scale { get; private set; }

        public EntityDisplay(GameObject prefab, SpriteLibraryAsset spriteLibraryAsset,
                Vector3 offset, Vector3 scale) {
            this.prefab = prefab;
            this.spriteLibraryAsset = spriteLibraryAsset;
            this.offset = offset;
            this.scale = scale;
        }

        public EntityDisplay(GameObject prefab) {
            this.prefab = prefab;
        }
    }
}
