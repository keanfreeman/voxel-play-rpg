using Instantiated;
using MovementDirection;
using NonVoxelEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using VoxelPlay;

public static class Coordinates
{
    // target must not be in caster, and cone length must be above 0
    public static List<Vector3Int> GetPointsInCone(Traveller caster, Vector3Int target, int coneLength) {
        List<Vector3Int> EMPTY_LIST = new();
        if (caster.occupiedPositions.Contains(target) || coneLength < 1) {
            return EMPTY_LIST;
        }

        // get point closest to target
        float closestDist = float.MaxValue;
        Vector3Int coneOrigin = caster.occupiedPositions.First();
        foreach (Vector3Int position in caster.occupiedPositions) {
            float lineLength = GetDirectLineLength(position, target);
            if (lineLength < closestDist) {
                closestDist = lineLength;
                coneOrigin = position;
            }
        }

        // cone spreads in the direction closest to both the origin and the target. the cone cannot 
        // spread from a diagonal corner, though.
        HashSet<Vector3Int> surrounding = GetPositionsSurroundingTraveller(caster);
        Vector3Int coneSpreadStart = surrounding.First();
        closestDist = float.MaxValue;
        float diagCornerDistance = Mathf.Sqrt(3);
        foreach (Vector3Int position in surrounding) {
            float distToOrigin = GetDirectLineLength(position, coneOrigin);
            if (distToOrigin >= diagCornerDistance) {
                // do not support for diagonal corner
                continue;
            }
            float distToTarget = GetDirectLineLength(position, target);
            float sum = distToOrigin + distToTarget;
            if (sum < closestDist) {
                closestDist = sum;
                coneSpreadStart = position;
            }
        }
        Vector3Int coneDirection = coneSpreadStart - coneOrigin;

        List<Vector3Int> conePoints = new();
        int currConeRadius = 0;
        while (currConeRadius < coneLength) {
            Vector3Int currConeCenter = coneSpreadStart + (coneDirection * currConeRadius);
            conePoints.AddRange(GetPointsInPlaneTangent(currConeCenter, coneDirection, currConeRadius));
            currConeRadius++;
        }

        return conePoints;
    }

    private static List<Vector3Int> GetPointsInPlaneTangent(Vector3Int planeCenter, Vector3Int direction,
            int planeRadius) {
        if (planeRadius < 1) {
            return new() { planeCenter };
        }

        List<Vector3Int> points = new();
        // there are 3 face planes (x, y, z)
        if (direction.x == 0 && direction.y == 0) {
            for (int x = planeCenter.x - planeRadius; x <= planeCenter.x + planeRadius; x++) {
                for (int y = planeCenter.y - planeRadius; y <= planeCenter.y + planeRadius; y++) {
                    points.Add(new(x, y, planeCenter.z));
                }
            }
            return points;
        }
        else if (direction.y == 0 && direction.z == 0) {
            for (int y = planeCenter.y - planeRadius; y <= planeCenter.y + planeRadius; y++) {
                for (int z = planeCenter.z - planeRadius; z <= planeCenter.z + planeRadius; z++) {
                    points.Add(new(planeCenter.x, y, z));
                }
            }
            return points;
        }
        else if (direction.x == 0 && direction.z == 0) {
            for (int x = planeCenter.x - planeRadius; x <= planeCenter.x + planeRadius; x++) {
                for (int z = planeCenter.z - planeRadius; z <= planeCenter.z + planeRadius; z++) {
                    points.Add(new(x, planeCenter.y, z));
                }
            }
            return points;
        }

        // there are 6 edge planes (xy, (-x)y, yz, (-y)z, xz, x(-z))
        // - 6 more are mirrored
        else if (direction.z == 0 && ((direction.x > 0 && direction.y > 0) 
                || (direction.x < 0 && direction.y < 0))) { // up right or down left
            Vector3Int iterator = new(1, -1, 0);
            Vector3Int start = planeCenter + new Vector3Int(-planeRadius, planeRadius, 0);
            Vector3Int pastEnd = planeCenter + new Vector3Int(planeRadius + 1, (-planeRadius) - 1, 0);

            Vector3Int curr = start;
            while (curr != pastEnd) {
                for (int z = planeCenter.z - planeRadius; z <= planeCenter.z + planeRadius; z++) {
                    points.Add(new(curr.x, curr.y, z));
                }
                curr += iterator;
            }
            return points;
        }
        else if (direction.z == 0 && ((direction.x > 0 && direction.y < 0)
                || (direction.x < 0 && direction.y > 0))) { // down right or up left
            Vector3Int iterator = new(1, 1, 0);
            Vector3Int start = planeCenter + new Vector3Int(-planeRadius, -planeRadius, 0);
            Vector3Int pastEnd = planeCenter + new Vector3Int(planeRadius + 1, planeRadius + 1, 0);

            Vector3Int curr = start;
            while (curr != pastEnd) {
                for (int z = planeCenter.z - planeRadius; z <= planeCenter.z + planeRadius; z++) {
                    points.Add(new(curr.x, curr.y, z));
                }
                curr += iterator;
            }
            return points;
        }

        else if (direction.x == 0 && ((direction.y > 0 && direction.z > 0) 
                || (direction.y < 0 && direction.z < 0))) { // up forward or down back
            Vector3Int iterator = new(0, 1, -1);
            Vector3Int start = planeCenter + new Vector3Int(0, -planeRadius, planeRadius);
            Vector3Int pastEnd = planeCenter + new Vector3Int(0, planeRadius + 1, (-planeRadius) - 1);

            Vector3Int curr = start;
            while (curr != pastEnd) {
                for (int x = planeCenter.x - planeRadius; x <= planeCenter.x + planeRadius; x++) {
                    points.Add(new(x, curr.y, curr.z));
                }
                curr += iterator;
            }
            return points;
        }
        else if (direction.x == 0 && ((direction.y < 0 && direction.z > 0)
                || (direction.y > 0 && direction.z < 0))) { // down forward or up back
            Vector3Int iterator = new(0, 1, 1);
            Vector3Int start = planeCenter + new Vector3Int(0, -planeRadius, -planeRadius);
            Vector3Int pastEnd = planeCenter + new Vector3Int(0, planeRadius + 1, planeRadius + 1);

            Vector3Int curr = start;
            while (curr != pastEnd) {
                for (int x = planeCenter.x - planeRadius; x <= planeCenter.x + planeRadius; x++) {
                    points.Add(new(x, curr.y, curr.z));
                }
                curr += iterator;
            }
            return points;
        }

        else if (direction.y == 0 && ((direction.z > 0 && direction.x > 0) 
                || (direction.z < 0 && direction.x < 0))) { // forward and right or back and left
            Vector3Int iterator = new(1, 0, -1); // todo check if needs to be reversed
            Vector3Int start = planeCenter + new Vector3Int(-planeRadius, 0, planeRadius);
            Vector3Int pastEnd = planeCenter + new Vector3Int(planeRadius + 1, 0, (-planeRadius) - 1);

            Vector3Int curr = start;
            while (curr != pastEnd) {
                for (int y = planeCenter.y - planeRadius; y <= planeCenter.y + planeRadius; y++) {
                    points.Add(new(curr.x, y, curr.z));
                }
                curr += iterator;
            }
            return points;
        }
        else if (direction.y == 0 && ((direction.z > 0 && direction.x < 0)
                || (direction.z < 0 && direction.x > 0))) { // forward and left or back and right
            Vector3Int iterator = new(1, 0, 1);
            Vector3Int start = planeCenter + new Vector3Int(-planeRadius, 0, -planeRadius);
            Vector3Int pastEnd = planeCenter + new Vector3Int(planeRadius + 1, 0, planeRadius + 1);

            Vector3Int curr = start;
            while (curr != pastEnd) {
                for (int y = planeCenter.y - planeRadius; y <= planeCenter.y + planeRadius; y++) {
                    points.Add(new(curr.x, y, curr.z));
                }
                curr += iterator;
            }
            return points;
        }

        // diagonal corner, not supported
        else {
            // there are 4 bottom corner planes (xyz, xy(-z), (-x)y(-z), (-x)yz)
            // - each has a mirror on the top
            throw new NotSupportedException("Cannot draw a plane from a corner.");
        }
    }

    // diagonals are considered to be more than 1 point away
    public static List<Vector3Int> GetPointsInSphereCenteredOn(Vector3Int center, int radius) {
        if (radius < 0) throw new ArgumentException("Must provide nonnegative radius.");

        List<Vector3Int> points = new();

        Vector3Int bottomLeft = center - (Vector3Int.one * radius);
        Vector3Int topRight = center + (Vector3Int.one * radius);
        for (int x = bottomLeft.x; x <= topRight.x; x++) {
            for (int y = bottomLeft.y; y <= topRight.y; y++) {
                for (int z = bottomLeft.z; z <= topRight.z; z++) {
                    Vector3Int currPoint = new(x, y, z);
                    float distance = GetDirectLineLength(currPoint, center);
                    if (distance <= radius) points.Add(currPoint);
                }
            }
        }

        return points;
    }

    public static Vector3Int GetBottomLeftOfCuboid(Vector3Int start, Vector3Int end) {
        return new(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Min(start.z, end.z));
    }

    public static int GetNumPointsInCuboid(Vector3Int start, Vector3Int end) {
        Vector3Int diff = end - start;
        int numPoints = (Mathf.Abs(diff.x) + 1) * (Mathf.Abs(diff.y) + 1) * (Mathf.Abs(diff.z) + 1);
        return numPoints;
    }

    public static Dictionary<Vector3Int, VoxelDefinition> GetPointsInCuboid(Vector3Int start,
            Vector3Int end, VoxelDefinition voxelDefinition) {
        if (start == end) return new() { { start, voxelDefinition } };

        int numPoints = GetNumPointsInCuboid(start, end);
        Dictionary<Vector3Int, VoxelDefinition> points = new(numPoints);

        int xIterator;
        int xTarget;
        if (start.x <= end.x) {
            xIterator = 1;
            xTarget = end.x + 1;
        }
        else {
            xIterator = -1;
            xTarget = end.x - 1;
        }

        int yIterator;
        int yTarget;
        if (start.y <= end.y) {
            yIterator = 1;
            yTarget = end.y + 1;
        }
        else {
            yIterator = -1;
            yTarget = end.y - 1;
        }

        int zIterator;
        int zTarget;
        if (start.z <= end.z) {
            zIterator = 1;
            zTarget = end.z + 1;
        }
        else {
            zIterator = -1;
            zTarget = end.z - 1;
        }

        for (int x = start.x; x != xTarget; x += xIterator) {
            for (int y = start.y; y != yTarget; y += yIterator) {
                for (int z = start.z; z != zTarget; z += zIterator) {
                    points.Add(new Vector3Int(x, y, z), voxelDefinition);
                }
            }
        }

        return points;
    }

    public static Quaternion GetRotationFromAngle(float yAngle) {
        Vector3 rotation = new Vector3(0, yAngle, 0);
        return Quaternion.Euler(rotation);
    }

    public static bool IsNextTo(Vector3Int a, Vector3Int b) {
        Vector3Int c = a - b;
        return Mathf.Abs(c.x) <= 1 && Mathf.Abs(c.y) <= 1 && Mathf.Abs(c.z) <= 1;
    }

    public static bool IsNextTo(Traveller traveller1, TangibleEntity traveller2) {
        HashSet<Vector3Int> adjacentPositions = GetPositionsSurroundingTraveller(traveller1, 1);
        foreach (Vector3Int position in traveller2.occupiedPositions) {
            if (adjacentPositions.Contains(position)) {
                return true;
            }
        }

        return false;
    }

    // sorted from closest positions to furthest
    public static List<Vector3Int> GetOriginPositionsWhereXIsNextToY(Traveller traveller, 
            Traveller target) {
        EntitySize firstSize = traveller.GetStats().size;
        HashSet<Vector3Int> immediatelyAdjacentPositions = GetPositionsSurroundingTraveller(
                target, 1);
        HashSet<Vector3Int> possibleOriginPositions = GetPositionsSurroundingTraveller(
                target, EntitySizeCalcs.GetRadius(firstSize));
        HashSet<Vector3Int> targetPoints = new HashSet<Vector3Int>(target.occupiedPositions);

        List<Vector3Int> validPositions = new List<Vector3Int>();
        foreach (Vector3Int position in possibleOriginPositions) {
            HashSet<Vector3Int> newPositionPoints = traveller.GetPositionsIfOriginAtPosition(position);
            if (
                    // checks if overlaps with target
                    newPositionPoints.Intersect(targetPoints).Count() == 0
                    // checks if adjacent with target
                    && newPositionPoints.Intersect(immediatelyAdjacentPositions).Count() > 0) {
                validPositions.Add(position);
            }
        }

        return validPositions;
    }

    public static List<Vector3Int> GetPointsOfSquare(Vector3Int origin, EntitySize size) {
        int radius = EntitySizeCalcs.GetRadius(size);
        List<Vector3Int> squarePoints = new List<Vector3Int>(4);
        squarePoints.Add(origin);
        squarePoints.Add(new Vector3Int(origin.x + radius, origin.y, origin.z));
        squarePoints.Add(new Vector3Int(origin.x, origin.y + radius, origin.z));
        squarePoints.Add(new Vector3Int(origin.x, origin.y, origin.z + radius));
        return squarePoints;
    }

    // TODO - allow for static objects too, whose occupied coordinates may not be square
    public static HashSet<Vector3Int> GetPositionsSurroundingTraveller(Traveller traveller,
            int radius = 1) {
        if (radius < 1) {
            return new HashSet<Vector3Int>();
        }

        HashSet<Vector3Int> innerPoints = new HashSet<Vector3Int>(traveller.occupiedPositions);
        HashSet<Vector3Int> outerPoints = new HashSet<Vector3Int>();
        int squareWidth = EntitySizeCalcs.GetRadius(traveller.GetStats().size);
        for (int x = -radius; x < squareWidth + radius; x++) {
            for (int y = -radius; y < squareWidth + radius; y++) {
                for (int z = -radius; z < squareWidth + radius; z++) {
                    Vector3Int position = new Vector3Int(x, y, z) + traveller.origin;
                    if (!innerPoints.Contains(position)) {
                        outerPoints.Add(position);
                    }
                }
            }
        }

        return outerPoints;
    }

    public static List<Vector3Int> GetFloorPositions(ICollection<Vector3Int> positions) {
        List<Vector3Int> floorPositions = new List<Vector3Int>();
        int lowest = int.MaxValue;
        foreach (Vector3Int position in positions) {
            if (position.y < lowest) {
                floorPositions.Clear();
                floorPositions.Add(position);
                lowest = position.y;
            }
            else if (position.y == lowest) {
                floorPositions.Add(position);
            }
        }

        return floorPositions;
    }

    public static Vector3Int RotatePointCounterClockwiseAroundCenter(Vector3Int a, Vector3Int center, 
            int numRotations) {
        int remainder = numRotations % 4;
        if (remainder == 0) {
            return a;
        }

        // 1,1,1 becomes -1, 1, 1 becomes -1, 1, -1 becomes 1, 1, -1
        // 1, 0, 2 becomes -2, 0, 1 becomes -1, 0, -2 becomes 2, 0, -1
        // y is unchanged
        // flipping 180 degrees flips the sign of x and z
        // flipping 90 degrees swaps x and z. z's sign is swapped if numRotations is even,
        // otherwise swap x's sign
        Vector3Int difference = a - center;

        bool swapXZ = remainder == 1 || remainder == 3;
        if (swapXZ) {
            (difference.z, difference.x) = (difference.x, difference.z);
        }

        bool flipXSign = remainder == 1 || remainder == 2;
        bool flipZSign = remainder == 2 || remainder == 3;
        if (flipXSign) {
            difference.x *= -1;
        }
        if (flipZSign) {
            difference.z *= -1;
        }

        return difference + center;
    }

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

    public static float GetDirectLineLength(Vector3Int start, Vector3Int end) {
        return Mathf.Abs((end - start).magnitude);
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
