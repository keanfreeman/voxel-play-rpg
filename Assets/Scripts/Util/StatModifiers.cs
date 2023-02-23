using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StatModifiers
{
    public static int GetModifierForStat(int statValue) {
        if (statValue < 0) {
            return -5;
        }
        if (statValue > 30) {
            return 10;
        }
        return (statValue / 2) - 5;
    }
}
