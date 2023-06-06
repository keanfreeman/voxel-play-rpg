using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DieNamespace {
    [Serializable]
    public struct Die
    {
        public int numDice;
        public int diceSize;
        public int modifier;

        [JsonConstructor]
        public Die(int numDice, int diceSize, int modifier) {
            this.numDice = numDice;
            this.diceSize = diceSize;
            this.modifier = modifier;
        }

        public Die(int numDice, int diceSize) {
            this.numDice = numDice;
            this.diceSize = diceSize;
            this.modifier = 0;
        }

        public override string ToString() {
            return $"{numDice}d{diceSize}+{modifier}";
        }
    }
}
