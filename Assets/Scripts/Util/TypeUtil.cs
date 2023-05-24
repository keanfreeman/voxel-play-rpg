using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TypeUtils
{
    public static bool IsSameTypeOrIsSubclass(object object1, Type type2) {
        Type type1 = object1.GetType();
        return AreTypesSameOrSubclass(type1, type2);
    }

    private static bool AreTypesSameOrSubclass(Type type1, Type type2) {
        return type1 == type2 || type1.IsSubclassOf(type2);
    }
}
