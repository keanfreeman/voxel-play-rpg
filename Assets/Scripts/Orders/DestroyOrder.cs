using EntityDefinition;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class DestroyOrder : Order
    {
        public Guid entityGuid;

        [JsonConstructor]
        public DestroyOrder(Guid entityGuid) {
            this.entityGuid = entityGuid;
        }

        public DestroyOrder(Entity entityToDestroy) {
            this.entityGuid = entityToDestroy.guid;
        }
    }
}
