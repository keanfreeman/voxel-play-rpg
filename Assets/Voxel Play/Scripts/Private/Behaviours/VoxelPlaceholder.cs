using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelPlay
{

    public class VoxelPlaceholder : MonoBehaviour
    {

        [NonSerialized]
        public int resistancePointsLeft;

        [NonSerialized]
        public Renderer damageIndicator;

        [NonSerialized]
        public VoxelChunk chunk;

        [NonSerialized]
        public int voxelIndex;

        [NonSerialized]
        public GameObject modelTemplate;

        [NonSerialized]
        public GameObject modelInstance;

        /// <summary>
        /// Default bounds when the placeholder is created
        /// </summary>
        [NonSerialized]
        public Bounds bounds;

        /// <summary>
        /// Stores the rotation when the placeholder was created. Used to detect if the voxel has been rotated and update the placeholder accordingly
        /// </summary>
        [NonSerialized]
        public float currentRotationDegrees;


        /// <summary>
        /// Current bounds in world space, taking into account that the renderer might have changed position or scale by some script
        /// </summary>
        public Bounds GetWorldSpaceBounds ()
        {
            Vector3 center, size;
            if (modelMeshRenderers != null && modelMeshRenderers.Length > 0 && modelMeshRenderers [0] != null) {
                center = modelMeshRenderers [0].bounds.center;
                size = modelMeshRenderers [0].bounds.size;
            } else {
                center = transform.position + bounds.center;
                size = bounds.size;
            }
            return new Bounds (center, size);
        }

        [NonSerialized]
        public MeshFilter modelMeshFilter;

        // Keep reference to original mesh of the custom voxel prefab in case it's instanced to adjust its colors due to smooth lighting
        [NonSerialized]
        public Mesh originalMesh;

        public MeshRenderer modelMeshRenderer {
            get {
                if (modelMeshRenderers == null) return null;
                return modelMeshRenderers [0];
            }

        }

        [NonSerialized]
        public MeshRenderer [] modelMeshRenderers;

        [NonSerialized]
        public Rigidbody rb;

        [NonSerialized]
        public Color32 lastMivTintColor = new Color32 (255, 255, 255, 15); // last computed tint color when rendered a miv in this position

        public Material damageIndicatorMaterial {
            get {
                if (_damageIndicatorMaterial == null && damageIndicator != null) {
                    _damageIndicatorMaterial = Instantiate<Material> (damageIndicator.sharedMaterial);
                    damageIndicator.sharedMaterial = _damageIndicatorMaterial;
                }
                return _damageIndicatorMaterial;
            }
        }


        float recoveryTime;
        Material _damageIndicatorMaterial;
        static bool[] prevEnableState;

        private void OnDestroy ()
        {
            CancelInvoke (nameof (Recover));
            StopAllCoroutines ();
        }

        public void StartHealthRecovery (VoxelChunk chunk, int voxelIndex, float damageDuration)
        {
            this.chunk = chunk;
            this.voxelIndex = voxelIndex;
            recoveryTime = Time.time + damageDuration;
            CancelInvoke (nameof (Recover));
            Invoke (nameof (Recover), damageDuration + 0.1f);
        }

        void Recover ()
        {
            float time = Time.time;
            if (time >= recoveryTime) {
                if (chunk != null && chunk.voxels [voxelIndex].typeIndex != 0) {
                    VoxelPlayEnvironment env = VoxelPlayEnvironment.GetSceneInstance(gameObject.scene.buildIndex);
                    resistancePointsLeft = env.voxelDefinitions[chunk.voxels [voxelIndex].type()].resistancePoints;
                }
                if (damageIndicator != null) {
                    damageIndicator.enabled = false;
                }
            }
        }


        public void SetAutoCancelDynamic (float delay)
        {
            Invoke (nameof (CancelDynamic), delay + UnityEngine.Random.value);
        }

        public void CancelDynamic ()
        {
            if (this != null && isActiveAndEnabled) {
                StartCoroutine (Consolidate ());
            }
        }

        public void CancelDynamicNow() {
            VoxelPlayEnvironment env = VoxelPlayEnvironment.GetSceneInstance(gameObject.scene.buildIndex);
            if (env == null || gameObject.Equals(null))
                return;
            env.VoxelCancelDynamic(this);
        }

        IEnumerator Consolidate ()
        {
            WaitForSeconds w = new WaitForSeconds (1f);
            VoxelPlayEnvironment env = VoxelPlayEnvironment.GetSceneInstance(gameObject.scene.buildIndex);
            if (env == null || gameObject.Equals (null))
                yield break;
            if (env.GetChunk (transform.position, out VoxelChunk targetChunk, false)) {
                const float maxDist = 100 * 100;
                if (env == null || gameObject.Equals (null) || env.cameraMain == null)
                    yield break;
                while (FastVector.SqrDistanceByValue ((Vector3)targetChunk.position, env.cameraMain.transform.position) < maxDist && env.ChunkIsInFrustum (targetChunk)) {
                    yield return w;
                }
                env.VoxelCancelDynamic (this);
            }
        }

        /// <summary>
        /// Toggles renderers visibility preserving visibility set by other systems
        /// </summary>
        /// <param name="enabled"></param>
        [MethodImpl (256)] // equals to MethodImplOptions.AggressiveInlining
        public void ToggleRenderers (bool enabled)
        {
            if (modelMeshRenderers == null) return;

            int renderersCount = modelMeshRenderers.Length;
            if (prevEnableState == null || prevEnableState.Length < renderersCount) {
                prevEnableState = new bool[renderersCount];
            }
            for (int j = 0; j < renderersCount; j++) {
                if (modelMeshRenderers [j] != null) {
                    if (enabled) {
                        modelMeshRenderers[j].enabled = prevEnableState[j];
                    } else {
                        prevEnableState[j] = modelMeshRenderers[j].enabled;
                        modelMeshRenderers[j].enabled = false;
                    }
                }
            }
        }


    }
}