using System.Collections.Generic;
using UnityEngine;


namespace VoxelPlay.GPULighting {

    public class VoxelPlayLightManager : MonoBehaviour {


        static List<VoxelPlayLight> lights = new List<VoxelPlayLight>();
        static bool shouldSortLights;

        const int MAX_LIGHTS = 32; // also given by shader buffer length
        int lastX, lastY, lastZ;
        Vector3 camPos;
        Vector4[] lightPosBuffer;
        Vector4[] lightColorBuffer;
        VoxelPlayEnvironment env;

        static class ShaderParams {
            public static int GlobalLightPositionsArray = Shader.PropertyToID("_VPPointLightPosition");
            public static int GlobalLightColorsArray = Shader.PropertyToID("_VPPointLightColor");
            public static int GlobalLightCount = Shader.PropertyToID("_VPPointLightCount");
            public static int GlobalLightMaxDistSqr = Shader.PropertyToID("_VPPointMaxDistanceSqr");
        }

        public static void RegisterLight(VoxelPlayLight light) {
            if (light != null && light.pointLight.type == LightType.Point && !lights.Contains(light)) {
                lights.Add(light);
                shouldSortLights = true;
            }
        }

        public static void UnregisterLight(VoxelPlayLight light) {
            if (light != null && lights.Contains(light)) {
                lights.Remove(light);
                shouldSortLights = true;
            }
        }

        void OnEnable() {
            if (lightPosBuffer == null || lightPosBuffer.Length < MAX_LIGHTS) {
                lightPosBuffer = new Vector4[MAX_LIGHTS];
            }
            if (lightColorBuffer == null || lightColorBuffer.Length < MAX_LIGHTS) {
                lightColorBuffer = new Vector4[MAX_LIGHTS];
            }
            shouldSortLights = true;
        }

        private void Start() {
            env = VoxelPlayEnvironment.GetSceneInstance(gameObject.scene.buildIndex);
            if (!VoxelPlayEnvironment.supportsBrightPointLights || VoxelPlayEnvironment.supportsURPNativeLights) {
                DestroyImmediate(this);
                return;
            }
        }

        void OnPreRender() {
            if (env == null) return;
            camPos = env.currentAnchorPosWS;
            FastMath.FloorToInt(camPos.x, camPos.y, camPos.z, out int x, out int y, out int z);
            x >>= 3;
            y >>= 3;
            z >>= 3;
            if (lastX == x && lastY == y && lastZ == z)
                return;
            lastX = x;
            lastY = y;
            lastZ = z;
            shouldSortLights = true;
        }

        void LateUpdate() {
            if (shouldSortLights) {
                shouldSortLights = false;
                lights.Sort(distanceComparer);
            }
            UpdateLights();
        }

        void UpdateLights() {
            float worldLightIntensity = Mathf.Max(env.world.lightIntensityMultiplier, 0);
            float worldLightScattering = Mathf.Max(env.world.lightScattering, 0);
            int i = 0;
            int lightCount = lights.Count;
            Camera cam = env.currentCamera;
            Vector3 camPos = Vector3.zero, camForward = Vector3.one;
            bool excludeLightsBehind = false;
            if (cam != null) {
                camPos = cam.transform.position;
                camForward = cam.transform.forward;
                excludeLightsBehind = true;
            }

            for (int k = 0; k < lightCount; k++) {

                VoxelPlayLight vpLight = lights[k];
                if (vpLight == null) {
                    lights.RemoveAt(k);
                    k--;
                    continue;
                }

                Light light = lights[k].pointLight;
                if (light == null || !vpLight.enabled) continue;

                // ignore light if it's behind camera + range
                Vector3 lightPos = light.transform.position;
                float range = 0.0001f + light.range * worldLightScattering;
                if (excludeLightsBehind) {
                    Vector3 toLight = lightPos - camPos;
                    float dot = Vector3.Dot(camForward, lightPos - camPos);
                    if (dot < 0 && toLight.sqrMagnitude > range * range) {
                        continue;
                    }
                }

                // ignore if intensity is zero
                float intensity = light.intensity * worldLightIntensity;
                if (intensity <= 0) continue;

                lightPosBuffer[i].x = lightPos.x;
                lightPosBuffer[i].y = lightPos.y;
                lightPosBuffer[i].z = lightPos.z;
                lightPosBuffer[i].w = range;
                Color color = light.color;
                lightColorBuffer[i].x = color.r * intensity;
                lightColorBuffer[i].y = color.g * intensity;
                lightColorBuffer[i].z = color.b * intensity;
                lightColorBuffer[i].w = color.a;
                i++;
                if (i >= MAX_LIGHTS) break;
            }

            while (i < MAX_LIGHTS) {
                lightPosBuffer[i].x = float.MaxValue;
                lightPosBuffer[i].y = float.MaxValue;
                lightPosBuffer[i].z = float.MaxValue;
                lightPosBuffer[i].w = 1.0f;
                lightColorBuffer[i].x = 0;
                lightColorBuffer[i].y = 0;
                lightColorBuffer[i].z = 0;
                lightColorBuffer[i].w = 0;
                i++;
            }
            Shader.SetGlobalVectorArray(ShaderParams.GlobalLightPositionsArray, lightPosBuffer);
            Shader.SetGlobalVectorArray(ShaderParams.GlobalLightColorsArray, lightColorBuffer);
            Shader.SetGlobalInt(ShaderParams.GlobalLightCount, i);
            float maxLightDistance = env.brightPointsMaxDistance * env.brightPointsMaxDistance;
            Shader.SetGlobalFloat(ShaderParams.GlobalLightMaxDistSqr, maxLightDistance);
        }


        int distanceComparer(VoxelPlayLight a, VoxelPlayLight b) {
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            float distA = FastVector.SqrDistance(ref camPos, ref posA);
            float distB = FastVector.SqrDistance(ref camPos, ref posB);
            if (distA < distB)
                return -1;
            if (distA > distB)
                return 1;
            return 0;
        }

    }

}
