using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachedCameraBottom : MonoBehaviour
{
    private Vector3 moveStartPoint;
    private Vector3 moveEndPoint;
    private float moveStartTime;

    private const float TRANSITION_TIME = 0.2f;

    public void MoveTo(Vector3 position) {
        moveStartTime = Time.time;
        moveStartPoint = transform.position;
        moveEndPoint = position;
        StartCoroutine(AnimateMove());
    }

    private IEnumerator AnimateMove() {
        while (Time.time - moveStartTime < TRANSITION_TIME) {
            float fractionOfMovementDone = (Time.time - moveStartTime) / TRANSITION_TIME;
            transform.position = Vector3.Lerp(moveStartPoint, moveEndPoint,
                fractionOfMovementDone);
            yield return null;
        }
        transform.position = moveEndPoint;
    }
}
