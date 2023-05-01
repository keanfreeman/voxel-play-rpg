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

        public static List<Vector3Int> GetPositionsFromSizeCategory(Vector3Int origin, EntitySize size,
            bool onlyReturnFloorPositions = false) {
            if (size < EntitySize.LARGE) {
                return new List<Vector3Int> { origin };
            }

            List<Vector3Int> result = new List<Vector3Int>();
            int numDimensions = size == EntitySize.LARGE ? 2
                : size == EntitySize.HUGE ? 3
                : 4;
            int yMax = onlyReturnFloorPositions ? 1 : numDimensions;
            for (int x = 0; x < numDimensions; x++) {
                for (int y = 0; y < yMax; y++) {
                    for (int z = 0; z < numDimensions; z++) {
                        result.Add(origin + new Vector3Int(x, y, z));
                    }
                }
            }

            return result;
        }
    }
}
