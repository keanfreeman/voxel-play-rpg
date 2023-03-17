using InstantiatedEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] public PlayerCharacter playerCharacter;
    public List<PlayerCharacter> partyMembers;

    public MonoBehaviour currSelected { get; set; }

    void Awake() {
        partyMembers = new List<PlayerCharacter>();
        currSelected = playerCharacter;
    }

    public void MoveSelected() {
        //if (inputManager.WasSelectTriggered()) {
        //    Vector3Int start = playerMovement.currVoxel;
        //    Node startNode = new Node(start);
        //    Node endNode = new Node(currVoxel);
        //    List<Vector3Int> path = pathfinder.FindPath(startNode, endNode);
        //    Debug.Log(path.Count);
        //    pathVisualizer.DrawPath(path);
        //
        //    movementManager.MoveAlongPath(playerMovement, path);
        //}
    }
}
