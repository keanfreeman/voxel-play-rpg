using InstantiatedEntity;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] GameObject hitParticleEffect;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] GameObject exclaimEffect;

    Vector3 CENTER_OFFSET = new Vector3(0.5f, 0.5f, 0.5f);

    public IEnumerator GenerateHitEffect(Vector3 location) {
        Vector3 particleLocation = location + CENTER_OFFSET;
        GameObject spawned = Instantiate(hitParticleEffect, particleLocation,
            cameraManager.GetMainCamera().transform.rotation);

        while (spawned != null) {
            yield return null;
        }
    }

    public IEnumerator GenerateHitEffect(Traveller traveller) {
        int creatureRadius = EntitySizeCalcs.GetRadius(traveller.GetStats().size);
        Vector3 particleLocation = traveller.origin + CENTER_OFFSET * creatureRadius;
        GameObject spawned = Instantiate(hitParticleEffect, particleLocation,
            cameraManager.GetMainCamera().transform.rotation);
        spawned.transform.localScale *= creatureRadius;

        while (spawned != null) {
            yield return null;
        }
    }

    public IEnumerator GenerateExclaimEffect(Traveller traveller) {
        int creatureRadius = EntitySizeCalcs.GetRadius(traveller.GetStats().size);
        Vector3 effectLocation = traveller.origin + CENTER_OFFSET * creatureRadius + Vector3.up;
        GameObject spawned = Instantiate(exclaimEffect, effectLocation,
            cameraManager.GetMainCamera().transform.rotation);
        spawned.transform.localScale *= creatureRadius;

        while (spawned != null) {
            yield return null;
        }
    }
}
