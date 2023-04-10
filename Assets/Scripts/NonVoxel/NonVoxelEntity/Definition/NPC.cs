using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;
using MovementDirection;

namespace NonVoxelEntity {
    public class NPC : Entity {
        public BattleGroup battleGroup { get; set; }
        public NPCStats stats { get; private set; }
        public Faction faction { get; private set; }

        public NPC(Vector3Int startPosition, NPCStats stats,
                EntityDisplay entityDisplay)
                : base(startPosition, stats.size, entityDisplay) {
            this.startPosition = startPosition;
            this.stats = stats;
            this.faction = Faction.ENEMY;
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

    public enum Faction {
        PLAYER,
        ENEMY,
        NEUTRAL
    }
}
