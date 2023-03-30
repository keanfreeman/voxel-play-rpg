using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] GameObject hitParticleEffect;
    [SerializeField] CameraManager cameraManager;

    Vector3 CENTER_OFFSET = new Vector3(0.5f, 0.5f, 0.5f);

    public IEnumerator GenerateHitEffect(Vector3 location) {
        Vector3 particleLocation = location + CENTER_OFFSET;
        GameObject spawned = Instantiate(hitParticleEffect, particleLocation,
            cameraManager.GetMainCamera().transform.rotation);

        while (spawned != null) {
            yield return null;
        }
    }
}
