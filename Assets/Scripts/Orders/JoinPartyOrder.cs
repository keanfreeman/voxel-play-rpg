using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class JoinPartyOrder : Order {
        public Guid newPartyMemberID;

        public JoinPartyOrder(Guid newPartyMemberID) {
            this.newPartyMemberID = newPartyMemberID;
        }
    }
}
