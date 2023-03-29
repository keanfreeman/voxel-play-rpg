using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;

namespace NonVoxelEntity {
    public class NPC : Entity {
        public BattleGroup battleGroup { get; set; }
        public NPCStats stats { get; private set; }
        public SpriteLibraryAsset spriteLibraryAsset { get; set; }
        public Vector3 spriteScale { get; private set; }
        public Faction faction { get; private set; }

        public NPC(GameObject prefab, Vector3Int startPosition, NPCStats stats,
                SpriteLibraryAsset spriteLibraryAsset, Vector3 spriteScale,
                Faction faction)
                : base(prefab, startPosition) {
            this.prefab = prefab;
            this.startPosition = startPosition;
            this.stats = stats;
            this.spriteLibraryAsset = spriteLibraryAsset;
            this.spriteScale = spriteScale;
            this.faction = faction;
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
