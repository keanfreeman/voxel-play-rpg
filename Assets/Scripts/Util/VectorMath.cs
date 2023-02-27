using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorMath
{
    public static Vector2 rotate(Vector2 v, float radians) {
        return new Vector2(
            v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
            v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
        );
    }
}
