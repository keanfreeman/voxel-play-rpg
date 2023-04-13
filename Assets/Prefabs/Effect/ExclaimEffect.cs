using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExclaimEffect : MonoBehaviour
{
    private const float TIME_TO_BOUNCE = 0.5f;
    private const float TIME_TO_HANG_AROUND = 0.5f;
    private const float BOUNCE_HEIGHT = 1f;

    private void Awake() {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate() {
        float startTime = Time.time;
        Vector3 startPosition = transform.position;

        while (Time.time - startTime < TIME_TO_BOUNCE) {
            float fractionDone = (Time.time - startTime) / TIME_TO_BOUNCE;
            float midpointFactor = 4 * BOUNCE_HEIGHT;
            float currHeight = -midpointFactor * (fractionDone * fractionDone) 
                + midpointFactor * fractionDone;
            transform.position = startPosition + new Vector3(0, currHeight, 0);
            yield return null;
        }

        yield return new WaitForSeconds(TIME_TO_HANG_AROUND);

        Destroy(gameObject);
    }
}
