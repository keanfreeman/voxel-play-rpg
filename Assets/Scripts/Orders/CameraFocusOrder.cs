using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class CameraFocusOrder : Order
    {
        public NPC focusTarget;

        public CameraFocusOrder(NPC focusTarget) {
            this.focusTarget = focusTarget;
        }
    }
}
