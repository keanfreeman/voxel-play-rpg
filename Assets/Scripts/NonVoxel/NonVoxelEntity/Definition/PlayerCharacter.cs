
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using GameMechanics;
using MovementDirection;

namespace NonVoxelEntity {
    public class PlayerCharacter : Entity {
        public Stats stats;

        public PlayerCharacter(Vector3Int startPosition,
                Stats stats, EntityDisplay spriteDisplay)
                : base(startPosition, stats.size, spriteDisplay) {
            this.stats = stats;
        }
    }
}
