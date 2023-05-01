using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;
using MovementDirection;

namespace EntityDefinition {
    [Serializable]
    public class NPC : Traveller {
        public BattleGroup battleGroup;
        public Faction faction;
        public IdleBehavior idleBehavior;

        public NPC(Vector3Int startPosition, Faction faction, 
                IdleBehavior idleBehavior, string travellerIdentity)
                : base(startPosition, travellerIdentity) {
            this.faction = faction;
            this.idleBehavior = idleBehavior;
        }
    }

    [Serializable]
    public class BattleGroup {
        public Guid groupID;
        public List<NPC> combatants;

        public BattleGroup(List<NPC> combatants) {
            this.combatants = combatants;
            this.groupID = Guid.NewGuid();
            foreach (NPC npc in this.combatants) {
                npc.battleGroup = this;
            }
        }
    }

    public enum IdleBehavior {
        STAND,
        WANDER
    }

    public enum Faction {
        PLAYER,
        ENEMY,
        NEUTRAL
    }
}
