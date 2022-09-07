using System.Collections.Generic;
using UnityEngine;

namespace NonVoxel {
    public class NonVoxelWorld {
        public Dictionary<GameObject, Vector3Int> creatures
            = new Dictionary<GameObject, Vector3Int>();

        private GameObject player;

        public NonVoxelWorld(GameObject player, Vector3Int playerPosition) {
            this.player = player;
            creatures[player] = playerPosition;
        }

        public Vector3Int GetPlayerPosition() {
            return creatures[player];
        }

        public void SetPlayerPosition(Vector3Int position) {
            creatures[player] = position;
        }

        public Vector3Int GetPosition(GameObject gameObject) {
            return creatures[gameObject];
        }

        public void SetPosition(GameObject gameObject, Vector3Int position) {
            creatures[gameObject] = position;
        }
    }
}
