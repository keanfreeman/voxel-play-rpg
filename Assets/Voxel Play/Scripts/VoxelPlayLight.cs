using System;
using UnityEngine;
using VoxelPlay.GPULighting;

namespace VoxelPlay {

    [HelpURL("https://kronnect.freshdesk.com/support/solutions/articles/42000084968-how-point-lights-work-in-voxel-play-")]
    [ExecuteInEditMode]
    public class VoxelPlayLight : MonoBehaviour {

        [NonSerialized] public Light pointLight;

        public void OnEnable() {
            pointLight = GetComponent<Light>();
            VoxelPlayLightManager.RegisterLight(this);
        }

        public void OnDisable() {
            VoxelPlayLightManager.UnregisterLight(this);
        }




    }
}
