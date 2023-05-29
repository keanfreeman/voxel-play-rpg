using GameMechanics;
using Newtonsoft.Json;
using NonVoxelEntity;
using Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityDefinition {
    [Serializable]
    public abstract class Traveller : TangibleEntity {
        public Vector3Int? currSpawnPosition;
        public CurrentResources resources;
        public int currHP;
        public CurrentStatus statusEffects;

        [JsonConstructor]
        public Traveller(Vector3Int startPosition, string travellerIdentity, OrderGroup interactOrders,
                CurrentResources resources, int currHP, CurrentStatus statusEffects) 
                : base(startPosition, travellerIdentity, interactOrders) {
            this.resources = resources;
            this.currHP = currHP;
            this.statusEffects = statusEffects;
        }

        public Traveller(Vector3Int startPosition, string travellerIdentity)
                : base(startPosition, travellerIdentity) {
            this.currHP = -1;
            this.statusEffects = new();
        }
    }
}
