
using UnityEngine;

namespace NonVoxelEntity {
    public class PlayerCharacter : Entity {
        public PlayerStats stats;
        public PlayerCharacter(GameObject prefab, Vector3Int startPosition,
            PlayerStats playerStats)
            : base(prefab, startPosition) {
            this.stats = playerStats;
        }
    }

    public struct PlayerStats {
        public string name { get; private set; }
        public int level { get; private set; }

        // Speed in feet per 6 seconds
        public int baseSpeed { get; private set; }
        public int hitPoints { get; private set; }

        public int strength { get; private set; }
        public int dexterity { get; private set; }
        public int constitution { get; private set; }
        public int intelligence { get; private set; }
        public int wisdom { get; private set; }
        public int charisma { get; private set; }

        public PlayerStats(string name, int level, int baseSpeed, int hitPoints,
                int strength, int dexterity, int constitution, int intelligence,
                int wisdom, int charisma) {
            this.name = name;
            this.level = level;
            this.baseSpeed = baseSpeed;
            this.hitPoints = hitPoints;
            this.strength = strength;
            this.dexterity = dexterity;
            this.constitution = constitution;
            this.intelligence = intelligence;
            this.wisdom = wisdom;
            this.charisma = charisma;
        }
    }
}
