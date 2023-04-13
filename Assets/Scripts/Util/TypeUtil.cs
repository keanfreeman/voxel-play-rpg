using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TypeUtil
{
    public static bool IsSameTypeOrSubclass(object object1, object object2) {
        Type type1 = object1.GetType();
        Type type2 = object2.GetType();
        return IsSameTypeOrSubclass(type1, type2);
    }

    public static bool IsSameTypeOrSubclass(object object1, Type type2) {
        Type type1 = object1.GetType();
        return IsSameTypeOrSubclass(type1, type2);
    }

    public static bool IsSameTypeOrSubclass(Type type1, Type type2) {
        return type1 == type2 || type1.IsSubclassOf(type2);
    }
}
