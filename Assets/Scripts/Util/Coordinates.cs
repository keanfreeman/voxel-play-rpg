using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public static class Coordinates
{
    public static List<Vector3Int> GetAdjacentCoordinates(Vector3Int position) {
        List<Vector3Int> adjacentCoordinates = new List<Vector3Int>();
        adjacentCoordinates.Capacity = 26;
        for (int x = -1; x < 2; x++) {
            for (int y = -1; y < 2; y++) {
                for (int z = -1; z < 2; z++) {
                    Vector3Int newCoordinate = position + new Vector3Int(x, y, z);
                    if (newCoordinate == position) {
                        continue;
                    }
                    adjacentCoordinates.Add(newCoordinate);
                }
            }
        }
        return adjacentCoordinates;
    }
}
