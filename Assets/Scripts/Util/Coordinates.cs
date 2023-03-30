using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public static class Coordinates
{
    // If diagonals are allowed
    public static int NumPointsBetween(Vector3Int a, Vector3Int b) {
        int distance = 0;
        Vector3Int c = a - b;
        // three dimensions are same
        if (c.x > 0 && c.y > 0 && c.z > 0) {
            int min = Mathf.Min(c.x, c.y, c.z);
            distance += min;
            c -= new Vector3Int(min, min, min);
        }
        else if (c.x < 0 && c.y < 0 && c.z < 0) {
            int max = Mathf.Max(c.x, c.y, c.z);
            distance -= max;
            c += new Vector3Int(max, max, max);
        }

        // two dimensions are same
        if (c.x > 0 && c.y > 0) {
            int min = Mathf.Min(c.x, c.y);
            distance += min;
            c -= new Vector3Int(min, min, 0);
        }
        else if (c.y > 0 && c.z > 0) {
            int min = Mathf.Min(c.y, c.z);
            distance += min;
            c -= new Vector3Int(0, min, min);
        }
        else if(c.x > 0 && c.z > 0) {
            int min = Mathf.Min(c.x, c.z);
            distance += min;
            c -= new Vector3Int(min, 0, min);
        }
        else if (c.x < 0 && c.y < 0) {
            int max = Mathf.Max(c.x, c.y);
            distance -= max;
            c += new Vector3Int(max, max, 0);
        }
        else if (c.y < 0 && c.z < 0) {
            int max = Mathf.Max(c.y, c.z);
            distance -= max;
            c += new Vector3Int(0, max, max);
        }
        else if (c.x < 0 && c.z < 0) {
            int max = Mathf.Max(c.x, c.z);
            distance -= max;
            c += new Vector3Int(max, 0, max);
        }

        distance += Mathf.Abs(c.x) + Mathf.Abs(c.y) + Mathf.Abs(c.z);

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
