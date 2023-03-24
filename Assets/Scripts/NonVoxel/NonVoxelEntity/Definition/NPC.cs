using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public class NPC : Entity {
        public BattleGroup battleGroup { get; set; }
        public NPCStats stats { get; private set; }

        public NPC(GameObject prefab, Vector3Int startPosition, NPCStats stats)
                : base(prefab, startPosition) {
            this.prefab = prefab;
            this.startPosition = startPosition;
            this.stats = stats;
        }
    }

    public struct NPCStats {
        public string name { get; private set; }
        public string challengeRating { get; private set; }

        public int armorClass { get; private set; }
        public int speed { get; private set; }
        public int hpMax { get; private set; }

        public int strength { get; private set; }
        public int dexterity { get; private set; }
        public int constitution { get; private set; }
        public int intelligence { get; private set; }
        public int wisdom { get; private set; }
        public int charisma { get; private set; }

        public List<GameMechanics.Action> actions { get; private set; }

        public NPCStats(string name, string challengeRating, int armorClass, int speed,
                int hpMax, int strength, int dexterity, int constitution, int intelligence,
                int wisdom, int charisma, List<GameMechanics.Action> actions) {
            this.name = name;
            this.challengeRating = challengeRating;
            this.armorClass = armorClass;
            this.speed = speed;
            this.hpMax = hpMax;
            this.strength = strength;
            this.dexterity = dexterity;
            this.constitution = constitution;
            this.intelligence = intelligence;
            this.wisdom = wisdom;
            this.charisma = charisma;
            this.actions = actions;
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
}
