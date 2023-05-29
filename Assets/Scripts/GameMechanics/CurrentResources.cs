using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    [Serializable]
    public class CurrentResources
    {
        public Dictionary<ResourceID, ResourceStatus> resourceStatuses { get; private set; }

        [JsonConstructor]
        public CurrentResources(Dictionary<ResourceID, ResourceStatus> resourceStatuses) {
            this.resourceStatuses = resourceStatuses;
        }

        public CurrentResources(List<Resource> resourceDefinitions) {
            resourceStatuses = new();
            foreach (Resource resource in resourceDefinitions) {
                resourceStatuses[resource.id] = new ResourceStatus(resource);
            }
        }

        public bool HasRemainingResource(ResourceID resourceID) {
            return resourceStatuses.ContainsKey(resourceID) && resourceStatuses[resourceID].remainingUses > 0;
        }

        public void DeductUses(ResourceID resourceID) {
            if (resourceStatuses.ContainsKey(resourceID)) resourceStatuses[resourceID].DecrementUses();
        }
    }

    [Serializable]
    public class ResourceStatus {
        public int remainingUses { get; private set; }
        public Resource resourceDefinition { get; private set; }

        [JsonConstructor]
        public ResourceStatus(int remainingUses, Resource resourceDefinition) {
            this.remainingUses = remainingUses;
            this.resourceDefinition = resourceDefinition;
        }

        public ResourceStatus(Resource resourceDefinition) {
            this.resourceDefinition = resourceDefinition;
            this.remainingUses = resourceDefinition.maxUses;
        }

        public void DecrementUses() {
            remainingUses -= 1;
        }

        public void ResetUses() {
            remainingUses = resourceDefinition.maxUses;
        }
    }
}