using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;
using MovementDirection;

namespace EntityDefinition {
    public class NPC : Traveller {
        public BattleGroup battleGroup { get; set; }
        public NPCStats stats { get; private set; }
        public Faction faction { get; private set; }
        public IdleBehavior idleBehavior { get; private set; }

        public NPC(Vector3Int startPosition, NPCStats stats,
                EntityDisplay entityDisplay, Faction faction, 
                IdleBehavior idleBehavior = IdleBehavior.STAND)
                : base(startPosition, stats.size, entityDisplay) {
            this.startPosition = startPosition;
            this.stats = stats;
            this.faction = faction;
            this.idleBehavior = idleBehavior;
        }
    }

    public class BattleGroup {
        public Guid groupID;
        public List<NPC> combatants { get; set; }

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
