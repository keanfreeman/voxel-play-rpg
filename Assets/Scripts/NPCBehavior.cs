using MovementDirection;
using NonVoxel;
using NonVoxelEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

// moves around randomly
public class NPCBehavior : Traveller
{
    [SerializeField]
    const float TIME_TO_ROTATE = 0.5f;
    private const float NPC_MIN_IDLE_TIME = 1;
    private const float NPC_MAX_IDLE_TIME = 5;

    private System.Random rng;
    private VoxelPlayEnvironment voxelPlayEnvironment;
    private SpriteMovement spriteMovement;
    private Transform childTransform;

    private float lastMoveTime = 0;

    private bool isRotating = false;
    float rotateStartTimestamp;
    Quaternion startRotation;
    Quaternion endRotation;

    public bool encounteredPlayer = false;
    public KeyCode rotationDirection = KeyCode.None;
    public NPC npcInfo;
    public HashSet<NPCBehavior> teammates;

    void Awake() {
        childTransform = transform.GetChild(0);
    }

    //void Update() {
    //    HandleCameraRotation();
    //}
    
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            encounteredPlayer = true;
        }
    }

    public void Init(NonVoxelWorld nonVoxelWorld, SpriteMovement spriteMovement,
        VoxelPlayEnvironment voxelPlayEnvironment, System.Random rng, NPC npcInfo) {
        this.nonVoxelWorld = nonVoxelWorld;
        this.spriteMovement = spriteMovement;
        this.voxelPlayEnvironment = voxelPlayEnvironment;
        this.rng = rng;
        this.npcInfo = npcInfo;

        currVoxel = nonVoxelWorld.GetPosition(gameObject);
    }

    public Vector3Int GetRandomOneTileMovement() {
        bool isX = rng.Next(0, 2) == 0 ? true : false;
        if (isX) {
            return rng.Next(0, 2) == 0 ? Vector3Int.left : Vector3Int.right;
        }
        return rng.Next(0, 2) == 0 ? Vector3Int.forward : Vector3Int.back;
    }

    public void HandleRandomMovement() {
        if (isRotating || Time.time - lastMoveTime < NPC_MIN_IDLE_TIME) {
            return;
        }
        lastMoveTime = Time.time;

        Vector3Int newPosition = currVoxel + GetRandomOneTileMovement();
        Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
            newPosition, currVoxel);
        if (!actualCoordinate.HasValue) {
            return;
        }
        Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();

        if (!nonVoxelWorld.IsPositionOccupied(destinationCoordinate)
                && spriteMovement.IsReachablePosition(destinationCoordinate)) {
            MoveToPoint(destinationCoordinate);
        }
    }

    private void HandleCameraRotation() {
        if (isRotating && !IsRotationDone()) {
            return;
        }
        isRotating = false;

        startRotation = childTransform.localRotation;
        if (rotationDirection == KeyCode.LeftArrow) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles
                + (Vector3.up * 90f));
        }
        else if (rotationDirection == KeyCode.RightArrow) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles
                + (Vector3.up * -90f));
        }
        else {
            return;
        }

        rotationDirection = KeyCode.None;
        isRotating = true;
        rotateStartTimestamp = Time.time;
    }

    private bool IsRotationDone() {
        float timeSinceMoveBegan = Time.time - rotateStartTimestamp;
        float fractionRotationComplete = Mathf.Min(timeSinceMoveBegan / TIME_TO_ROTATE, 1);
        childTransform.localRotation = Quaternion.Lerp(startRotation, endRotation,
            fractionRotationComplete);

        return fractionRotationComplete == 1;
    }

    public bool IsInteractable() {
        return true;
    }
}
