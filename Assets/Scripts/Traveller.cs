using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Traveller : MonoBehaviour
{
    [SerializeField] protected NonVoxelWorld nonVoxelWorld;
    [SerializeField] protected Animator animator;

    protected Vector3Int moveStartPoint;
    protected Vector3Int moveEndPoint;
    protected float moveStartTimestamp;

    public bool isMoving { get; protected set; }
    public Vector3Int currVoxel { get; protected set; }

    protected const float TIME_TO_MOVE_A_TILE = 0.2f;

    private void Update() {
        AnimateMove();
        AnimateRotation();
    }

    private void AnimateMove() {
        if (isMoving) {
            float timeSinceMoveBegan = Time.time - moveStartTimestamp;
            float fractionOfMovementDone = Mathf.Min(timeSinceMoveBegan / (TIME_TO_MOVE_A_TILE), 1f);
            transform.position = Vector3.Lerp(moveStartPoint, moveEndPoint, fractionOfMovementDone);

            if (fractionOfMovementDone >= 1f) {
                SetMoveAnimation(false);
                isMoving = false;
            }
        }
    }

    private void AnimateRotation() {
        // TODO
    }

    public void SetCurrVoxel(Vector3Int currVoxel) {
        this.currVoxel = currVoxel;
    }

    public void MoveToPoint(Vector3Int point) {
        nonVoxelWorld.SetPosition(gameObject, point);
        moveStartPoint = currVoxel;
        moveEndPoint = point;
        currVoxel = point;
        moveStartTimestamp = Time.time;
        isMoving = true;
        SetMoveAnimation(isMoving);
    }

    public void SetMoveAnimation(bool state) {
        if (animator != null) {
            animator.SetBool("isMoving", state);
        }
    }

    public abstract void RotateSprite(float degrees);

    public abstract void SetSpriteRotation(Vector3 rotation);
}
