using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonVoxelEntity {
    public enum EntitySize {
        Tiny = 0,
        Small = 1,
        Medium = 2,
        Large = 3,
        Huge = 4,
        Gargantuan = 5
    }

    public static class EntitySizeCalcs {
        public static int GetRadius(EntitySize entitySize) {
            return entitySize < EntitySize.Large ? 1
                : entitySize == EntitySize.Large ? 2
                : entitySize == EntitySize.Huge ? 3
                : 4;
        }

        public static List<Vector3Int> GetPositionsFromSizeCategory(Vector3Int origin, EntitySize size,
            bool onlyReturnFloorPositions = false) {
            if (size < EntitySize.Large) {
                return new List<Vector3Int> { origin };
            }

            List<Vector3Int> result = new List<Vector3Int>();
            int numDimensions = size == EntitySize.Large ? 2
                : size == EntitySize.Huge ? 3
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
