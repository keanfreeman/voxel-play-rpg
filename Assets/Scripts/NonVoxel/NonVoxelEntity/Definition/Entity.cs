using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public abstract class Entity
    {
        public Vector3Int spawnPosition;
        public Guid guid;

        [JsonConstructor]
        protected Entity(Vector3Int spawnPosition, Guid guid) {
            this.spawnPosition = spawnPosition;
            this.guid = guid;
        }

        protected Entity(Vector3Int spawnPosition) {
            this.spawnPosition = spawnPosition;
            this.guid = Guid.NewGuid();
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            Entity other = (Entity)obj;
            return guid == other.guid;
        }

        public override int GetHashCode() {
            return guid.GetHashCode();
        }
    }
}
