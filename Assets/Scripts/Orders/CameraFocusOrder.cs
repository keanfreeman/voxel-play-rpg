using EntityDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class CameraFocusOrder : Order
    {
        public NPC focusTarget { get; private set; }

        public CameraFocusOrder(NPC focusTarget) {
            this.focusTarget = focusTarget;
        }
    }
}
