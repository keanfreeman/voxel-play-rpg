using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instantiated {
    public abstract class InstantiatedEntity : MonoBehaviour {
        protected Spawnable entity;

        public Spawnable GetEntity() {
            return entity;
        } 
    }
}
