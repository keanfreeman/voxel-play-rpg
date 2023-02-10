using System;
using UnityEngine;
using VoxelPlay.GPULighting;

namespace VoxelPlay {

    [HelpURL("https://kronnect.freshdesk.com/support/solutions/articles/42000084968-how-point-lights-work-in-voxel-play-")]
    [ExecuteInEditMode]
    public class VoxelPlayLight : MonoBehaviour {

        [NonSerialized] public Light pointLight;

        [NonSerialized] private VoxelPlayLightManager voxelPlayLightManager;

        [SerializeField] public VoxelPlayEnvironment voxelPlayEnvironment;

        public void OnEnable() {
            if (voxelPlayLightManager == null) {
                voxelPlayLightManager = voxelPlayEnvironment.voxelPlayLightManager;
            }
            pointLight = GetComponent<Light>();
            voxelPlayLightManager.RegisterLight(this);
        }

        public void OnDisable() {
            voxelPlayLightManager.UnregisterLight(this);
        }
    }
}
