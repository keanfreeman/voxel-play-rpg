using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public static class Coordinates
{
    // Returns the number of coordinates from A to B in a direct path.
    public static int NumPointsBetween(Vector3Int a, Vector3Int b) {
        int distance = 0;
        Vector3Int c = a - b;
        Vector3Int absolute = new Vector3Int(Mathf.Abs(c.x), Mathf.Abs(c.y), Mathf.Abs(c.z));

        if (absolute.x != 0 && absolute.y != 0 && absolute.z != 0) {
            int min = Mathf.Min(absolute.x, absolute.y, absolute.z);
            distance += min;
            absolute.x -= min;
            absolute.y -= min;
            absolute.z -= min;
        }

        if (absolute.x != 0 && absolute.y != 0) {
            int min = Mathf.Min(absolute.x, absolute.y);
            distance += min;
            absolute.x -= min;
            absolute.y -= min;
        }
        else if (absolute.y != 0 && absolute.z != 0) {
            int min = Mathf.Min(absolute.y, absolute.z);
            distance += min;
            absolute.y -= min;
            absolute.z -= min;
        }
        else if (absolute.x != 0 && absolute.z != 0) {
            int min = Mathf.Min(absolute.x, absolute.z);
            distance += min;
            absolute.x -= min;
            absolute.z -= min;
        }

        distance += absolute.x + absolute.y + absolute.z;
        return distance;
    }

    public static bool IsNextTo(Vector3Int a, Vector3Int b) {
        Vector3Int c = a - b;
        return Mathf.Abs(c.x) <= 1 && Mathf.Abs(c.y) <= 1 && Mathf.Abs(c.z) <= 1;
    }

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
