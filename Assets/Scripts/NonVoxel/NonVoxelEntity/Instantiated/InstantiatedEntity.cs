using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instantiated {
    public abstract class InstantiatedEntity : MonoBehaviour {
        protected Entity entity;

        public Entity GetEntity() {
            return entity;
        }
    }
}
