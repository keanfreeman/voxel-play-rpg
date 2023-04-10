using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public enum EntitySize {
        TINY = 0,
        SMALL = 1,
        MEDIUM = 2,
        LARGE = 3,
        HUGE = 4,
        GARGANTUAN = 5
    }

    public static class EntitySizeCalcs {
        public static int GetRadius(EntitySize entitySize) {
            return entitySize < EntitySize.LARGE ? 1
                : entitySize == EntitySize.LARGE ? 2
                : entitySize == EntitySize.HUGE ? 3
                : 4;
        }
    }
}
